
using System.Security.Claims;
using LoanManagement.Application.DTOs.Payments;
using LoanManagement.Infrastructure.Entities;
using LoanManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize(Roles = "Customer")]
public class CustomerLoansController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomerLoansController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Get Customer Loans
    [HttpGet("loans")]
    public async Task<IActionResult> GetMyLoans(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var loans = await _context.Loans
            .AsNoTracking()
            .Where(l => l.Application.Customer.UserId == userId)
            .Select(l => new
            {
                l.LoanId,
                l.LoanNumber,
                l.ApprovedAmount,
                l.AnnualInterestRate,
                l.TenureMonths,
                l.StartDate,
                l.EndDate,
                l.LoanStatus,
                ProductName = l.Application.LoanProduct.ProductName
            })
            .ToListAsync(cancellationToken);

        return Ok(loans);
    }

    // Get EMI Schedules
    [HttpGet("loans/{loanId:int}/schedules")]
    public async Task<IActionResult> GetSchedules(
        int loanId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var schedules = await _context.RepaymentSchedules
            .AsNoTracking()
            .Where(s => s.LoanId == loanId &&
                        s.Loan.Application.Customer.UserId == userId)
            .OrderBy(s => s.InstallmentNumber)
            .Select(s => new
            {
                s.ScheduleId,
                s.InstallmentNumber,
                s.DueDate,
                s.PrincipalAmount,
                s.InterestAmount,
                EmiAmount = s.Emiamount,
                s.PaidAmount,
                s.PaymentStatus
            })
            .ToListAsync(cancellationToken);

        return Ok(schedules);
    }

    // Make EMI Payment (Demo)
    [HttpPost("payments")]
    public async Task<IActionResult> MakePayment(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var schedule = await _context.RepaymentSchedules
            .Include(s => s.Loan)
                .ThenInclude(l => l.Application)
            .FirstOrDefaultAsync(
                s => s.ScheduleId == request.ScheduleId &&
                     s.Loan.Application.Customer.UserId == userId,
                cancellationToken);

        if (schedule == null)
            return NotFound(new { message = "EMI schedule not found." });

        if (schedule.PaymentStatus == "Paid")
            return BadRequest(new { message = "This EMI is already paid." });

        var amount = schedule.Emiamount - schedule.PaidAmount;

        if (amount <= 0)
            return BadRequest(new { message = "No outstanding amount." });

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var payment = new Payment
            {
                ScheduleId = schedule.ScheduleId,
                TransactionReference = "PAY-" +
                    Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                Amount = amount,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Successful",
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            schedule.PaidAmount += amount;
            schedule.PaymentStatus = "Paid";

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "EMI payment successful.",
                paymentId = payment.PaymentId,
                transactionReference = payment.TransactionReference,
                amountPaid = payment.Amount,
                paymentStatus = payment.PaymentStatus
            });
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var payments = await _context.Payments
            .Where(p => p.Schedule.Loan.Application.Customer.UserId == userId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new
            {
                p.PaymentId,
                p.ScheduleId,
                p.TransactionReference,
                p.Amount,
                p.PaymentMethod,
                p.PaymentStatus,
                p.PaymentDate,
                p.Schedule.InstallmentNumber,
                p.Schedule.DueDate,
                p.Schedule.Loan.LoanNumber
            })
            .ToListAsync();

        return Ok(payments);
    }
}
