using LoanManagement.Application.DTOs.LoanProducts;
using LoanManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/business/loan-products")]
[Authorize(Roles = "Business")]
public class BusinessLoanProductsController : ControllerBase
{
    private readonly ILoanProductService _loanProductService;

    public BusinessLoanProductsController(
        ILoanProductService loanProductService)
    {
        _loanProductService = loanProductService;
    }

    // CREATE LOAN PRODUCT
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLoanProductRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _loanProductService.CreateAsync(
                userId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.LoanProductId },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET ALL PRODUCTS
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _loanProductService.GetAllAsync(
            userId.Value,
            cancellationToken);

        return Ok(result);
    }

    // GET PRODUCT BY ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _loanProductService.GetByIdAsync(
            userId.Value,
            id,
            cancellationToken);

        if (result == null)
            return NotFound(new { message = "Loan product not found." });

        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateLoanProductRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _loanProductService.UpdateAsync(
                userId.Value,
                id,
                request,
                cancellationToken);

            if (result == null)
                return NotFound(new { message = "Loan product not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _loanProductService.DeactivateAsync(
            userId.Value,
            id,
            cancellationToken);

        if (!result)
            return NotFound(new { message = "Loan product not found." });

        return Ok(new { message = "Loan product deactivated successfully." });
    }

}