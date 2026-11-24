
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagement.Infrastructure.Persistence;

namespace LoanManagement.API.Controllers;

[Route("api/business/dashboard")]
[ApiController]
[Authorize(Roles = "Business")]
public class BusinessDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusinessDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var business = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (business == null)
            return NotFound(new { message = "Business profile not found." });

        var businessId = business.BusinessId;

        var applications = _context.LoanApplications
            .Where(a => a.LoanProduct.BusinessId == businessId);

        var loans = _context.Loans
            .Where(l => l.Application.LoanProduct.BusinessId == businessId);

        var totalCustomers = await applications
            .Select(a => a.CustomerId)
            .Distinct()
            .CountAsync();

        var totalApplications = await applications.CountAsync();

        var totalLoans = await loans.CountAsync();

        var activeLoans = await loans
            .CountAsync(l => l.LoanStatus == "Active");

        var totalDisbursedAmount = await loans
            .SumAsync(l => (decimal?)l.ApprovedAmount) ?? 0;

        return Ok(new
        {
            business.BusinessId,
            business.BusinessName,
            TotalCustomers = totalCustomers,
            TotalApplications = totalApplications,
            TotalLoans = totalLoans,
            ActiveLoans = activeLoans,
            TotalDisbursedAmount = totalDisbursedAmount
        });
    }


    [HttpGet("loans")]
    public async Task<IActionResult> GetBusinessLoans()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var loans = await _context.Loans
            .Where(l => l.Application.LoanProduct.Business.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
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
                CustomerName = l.Application.Customer.FirstName
                    + " " + l.Application.Customer.LastName,
                CustomerEmail = l.Application.Customer.User.Email,
                ProductName = l.Application.LoanProduct.ProductName
            })
            .ToListAsync();

        return Ok(loans);
    }


    [HttpGet("customers")]
    public async Task<IActionResult> GetBusinessCustomers()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var customers = await _context.LoanApplications
            .Where(a => a.LoanProduct.Business.UserId == userId)
            .Select(a => new
            {
                a.Customer.CustomerId,
                CustomerName = a.Customer.FirstName + " " + a.Customer.LastName,
                a.Customer.PhoneNumber,
                CustomerEmail = a.Customer.User.Email,
                a.Customer.Address
            })
            .Distinct()
            .OrderBy(c => c.CustomerName)
            .ToListAsync();

        return Ok(customers);
    }

}
