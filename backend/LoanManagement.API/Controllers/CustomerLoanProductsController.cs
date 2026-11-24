
using LoanManagement.Application.DTOs.LoanProducts;
using LoanManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/customer/loan-products")]
[Authorize(Roles = "Customer")]
public class CustomerLoanProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomerLoanProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _context.LoanProducts
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new CustomerLoanProductResponse
            {
                LoanProductId = p.LoanProductId,
                ProductName = p.ProductName,
                BusinessName = p.Business.BusinessName,
                MinLoanAmount = p.MinLoanAmount,
                MaxLoanAmount = p.MaxLoanAmount,
                AnnualInterestRate = p.AnnualInterestRate,
                MinTenureMonths = p.MinTenureMonths,
                MaxTenureMonths = p.MaxTenureMonths
            })
            .ToListAsync(cancellationToken);

        return Ok(products);
    }
}
