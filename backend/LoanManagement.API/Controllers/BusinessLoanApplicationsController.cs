
using System.Security.Claims;
using LoanManagement.Application.DTOs.LoanApplications;
using LoanManagement.Infrastructure.Entities;
using LoanManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/business/loan-applications")]
[Authorize(Roles = "Business")]
public class BusinessLoanApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusinessLoanApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications(
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var applications = await _context.LoanApplications
            .AsNoTracking()
            .Where(a => a.LoanProduct.Business.UserId == userId)
            .Select(a => new
            {
                a.ApplicationId,
                a.CustomerId,
                CustomerName = a.Customer.FirstName + " " + a.Customer.LastName,
                a.LoanProductId,
                a.LoanProduct.ProductName,
                a.RequestedAmount,
                a.RequestedTenureMonths,
                a.ApplicationStatus,
                a.AppliedAt,
                a.Remarks
            })
            .ToListAsync(cancellationToken);

        return Ok(applications);
    }

    [HttpPut("{id:int}/review")]
    public async Task<IActionResult> ReviewApplication(
        int id,
        ReviewLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var application = await _context.LoanApplications
            .Include(a => a.LoanProduct)
            .FirstOrDefaultAsync(
                a => a.ApplicationId == id
                     && a.LoanProduct.Business.UserId == userId,
                cancellationToken);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        if (application.ApplicationStatus != "Pending")
            return BadRequest(new { message = "Application has already been reviewed." });

        application.ApplicationStatus = request.Status;
        application.Remarks = request.Remarks;
        application.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Application reviewed successfully.",
            applicationId = application.ApplicationId,
            status = application.ApplicationStatus
        });
    }


    [HttpPost("{id:int}/create-loan")]
    public async Task<IActionResult> CreateLoan(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Find approved application belonging to this business
        var application = await _context.LoanApplications
            .Include(a => a.LoanProduct)
            .ThenInclude(p => p.Business)
            .FirstOrDefaultAsync(
                a => a.ApplicationId == id &&
                     a.LoanProduct.Business.UserId == userId,
                cancellationToken);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        if (application.ApplicationStatus != "Approved")
            return BadRequest(new { message = "Only approved applications can create loans." });

        // Prevent duplicate loan creation
        var existingLoan = await _context.Loans
            .FirstOrDefaultAsync(
                l => l.ApplicationId == id,
                cancellationToken);

        if (existingLoan != null)
            return BadRequest(new { message = "Loan already exists for this application." });

        var principal = application.RequestedAmount;
        var annualRate = application.LoanProduct.AnnualInterestRate;
        var tenure = application.RequestedTenureMonths;

        var monthlyRate = annualRate / 1200m;

        decimal emi;

        if (monthlyRate == 0)
        {
            emi = principal / tenure;
        }
        else
        {
            var power = (decimal)Math.Pow(
                (double)(1 + monthlyRate), tenure);

            emi = principal * monthlyRate * power / (power - 1);
        }

        emi = Math.Round(emi, 2);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddMonths(tenure);

        var loan = new Loan
        {
            ApplicationId = application.ApplicationId,
            LoanNumber = "LN-" +
                DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) +
                "-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            ApprovedAmount = principal,
            AnnualInterestRate = annualRate,
            TenureMonths = tenure,
            StartDate = startDate,
            EndDate = endDate,
            LoanStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync(cancellationToken);

            var schedules = new List<RepaymentSchedule>();

            var outstandingBalance = principal;

            for (int month = 1; month <= tenure; month++)
            {
                var interest = Math.Round(
                    outstandingBalance * monthlyRate, 2);

                var installmentPrincipal = Math.Round(
                    emi - interest, 2);

                // Adjust final installment to clear remaining balance
                if (month == tenure)
                {
                    installmentPrincipal = outstandingBalance;
                }

                var installmentEmi = Math.Round(
                    installmentPrincipal + interest, 2);

                schedules.Add(new RepaymentSchedule
                {
                    LoanId = loan.LoanId,
                    InstallmentNumber = month,
                    DueDate = startDate.AddMonths(month),
                    PrincipalAmount = installmentPrincipal,
                    InterestAmount = interest,
                    Emiamount = installmentEmi,
                    PaidAmount = 0,
                    PaymentStatus = "Pending"
                });

                outstandingBalance = Math.Round(
                    outstandingBalance - installmentPrincipal, 2);
            }

            _context.RepaymentSchedules.AddRange(schedules);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Loan and EMI schedule created successfully.",
                loanId = loan.LoanId,
                loanNumber = loan.LoanNumber,
                approvedAmount = loan.ApprovedAmount,
                annualInterestRate = loan.AnnualInterestRate,
                tenureMonths = loan.TenureMonths,
                monthlyEmi = emi,
                totalInstallments = schedules.Count,
                loanStatus = loan.LoanStatus
            });
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

}
