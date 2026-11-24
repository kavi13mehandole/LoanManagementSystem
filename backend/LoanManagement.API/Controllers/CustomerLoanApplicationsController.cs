
using System.Security.Claims;
using LoanManagement.Application.DTOs.LoanApplications;
using LoanManagement.Infrastructure.Persistence;
using LoanManagement.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/customer/loan-applications")]
[Authorize(Roles = "Customer")]
public class CustomerLoanApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomerLoanApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var customer = await _context.CustomerProfiles
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);

        if (customer == null)
            return NotFound(new { message = "Customer profile not found." });

        var product = await _context.LoanProducts
            .FirstOrDefaultAsync(
                p => p.LoanProductId == request.LoanProductId
                     && p.IsActive,
                cancellationToken);

        if (product == null)
            return NotFound(new { message = "Active loan product not found." });

        if (request.RequestedAmount < product.MinLoanAmount ||
            request.RequestedAmount > product.MaxLoanAmount)
            return BadRequest(new { message = "Requested amount is outside the allowed range." });

        if (request.RequestedTenureMonths < product.MinTenureMonths ||
            request.RequestedTenureMonths > product.MaxTenureMonths)
            return BadRequest(new { message = "Requested tenure is outside the allowed range." });

        var application = new LoanApplication
        {
            CustomerId = customer.CustomerId,
            LoanProductId = product.LoanProductId,
            RequestedAmount = request.RequestedAmount,
            RequestedTenureMonths = request.RequestedTenureMonths,
            ApplicationStatus = "Pending",
            AppliedAt = DateTime.UtcNow
        };

        _context.LoanApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Loan application submitted successfully.",
            applicationId = application.ApplicationId,
            status = application.ApplicationStatus
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyApplications(
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var applications = await _context.LoanApplications
            .AsNoTracking()
            .Where(a => a.Customer.UserId == userId)
            .Select(a => new LoanApplicationResponse
            {
                ApplicationId = a.ApplicationId,
                LoanProductId = a.LoanProductId,
                ProductName = a.LoanProduct.ProductName,
                RequestedAmount = a.RequestedAmount,
                RequestedTenureMonths = a.RequestedTenureMonths,
                Status = a.ApplicationStatus,
                AppliedAt = a.AppliedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(applications);
    }
}
