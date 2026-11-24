using LoanManagement.Application.DTOs.LoanProducts;
using LoanManagement.Application.Interfaces;
using LoanManagement.Infrastructure.Entities;
using LoanManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.Infrastructure.Services;

public class LoanProductService : ILoanProductService
{
    private readonly ApplicationDbContext _dbContext;

    public LoanProductService(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    // CREATE LOAN PRODUCT
    public async Task<LoanProductResponse> CreateAsync(
        int userId,
        CreateLoanProductRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Validate loan amount
        if (request.MinLoanAmount > request.MaxLoanAmount)
        {
            throw new ArgumentException(
                "Minimum loan amount cannot be greater than maximum loan amount.");
        }

        // Step 2: Validate tenure
        if (request.MinTenureMonths > request.MaxTenureMonths)
        {
            throw new ArgumentException(
                "Minimum tenure cannot be greater than maximum tenure.");
        }

        // Step 3: Find business profile using logged-in UserId
        var business = await _dbContext.BusinessProfiles
            .FirstOrDefaultAsync(
                b => b.UserId == userId,
                cancellationToken);

        if (business == null)
        {
            throw new InvalidOperationException(
                "Business profile not found for this user.");
        }

        // Step 4: Create loan product
        var loanProduct = new LoanProduct
        {
            BusinessId = business.BusinessId,
            ProductName = request.ProductName.Trim(),
            MinLoanAmount = request.MinLoanAmount,
            MaxLoanAmount = request.MaxLoanAmount,
            AnnualInterestRate = request.AnnualInterestRate,
            MinTenureMonths = request.MinTenureMonths,
            MaxTenureMonths = request.MaxTenureMonths,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Step 5: Save into database
        _dbContext.LoanProducts.Add(loanProduct);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Step 6: Return response DTO
        return MapToResponse(loanProduct);
    }

    // GET ALL LOAN PRODUCTS OF LOGGED-IN BUSINESS
    public async Task<List<LoanProductResponse>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanProducts
            .AsNoTracking()
            .Where(p => p.Business.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new LoanProductResponse
            {
                LoanProductId = p.LoanProductId,
                ProductName = p.ProductName,
                MinLoanAmount = p.MinLoanAmount,
                MaxLoanAmount = p.MaxLoanAmount,
                AnnualInterestRate = p.AnnualInterestRate,
                MinTenureMonths = p.MinTenureMonths,
                MaxTenureMonths = p.MaxTenureMonths,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    // GET SINGLE LOAN PRODUCT
    public async Task<LoanProductResponse?> GetByIdAsync(
        int userId,
        int loanProductId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanProducts
            .AsNoTracking()
            .Where(p =>
                p.LoanProductId == loanProductId &&
                p.Business.UserId == userId)
            .Select(p => new LoanProductResponse
            {
                LoanProductId = p.LoanProductId,
                ProductName = p.ProductName,
                MinLoanAmount = p.MinLoanAmount,
                MaxLoanAmount = p.MaxLoanAmount,
                AnnualInterestRate = p.AnnualInterestRate,
                MinTenureMonths = p.MinTenureMonths,
                MaxTenureMonths = p.MaxTenureMonths,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<LoanProductResponse?> UpdateAsync(
        int userId,
        int loanProductId,
        UpdateLoanProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.MinLoanAmount > request.MaxLoanAmount)
            throw new ArgumentException(
                "Minimum loan amount cannot exceed maximum loan amount.");

        if (request.MinTenureMonths > request.MaxTenureMonths)
            throw new ArgumentException(
                "Minimum tenure cannot exceed maximum tenure.");

        var product = await _dbContext.LoanProducts
            .Include(p => p.Business)
            .FirstOrDefaultAsync(
                p => p.LoanProductId == loanProductId
                     && p.Business.UserId == userId,
                cancellationToken);

        if (product == null)
            return null;

        if (!product.IsActive)
            throw new InvalidOperationException(
                "Inactive loan products cannot be updated.");

        product.ProductName = request.ProductName.Trim();
        product.MinLoanAmount = request.MinLoanAmount;
        product.MaxLoanAmount = request.MaxLoanAmount;
        product.AnnualInterestRate = request.AnnualInterestRate;
        product.MinTenureMonths = request.MinTenureMonths;
        product.MaxTenureMonths = request.MaxTenureMonths;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }

    public async Task<bool> DeactivateAsync(
        int userId,
        int loanProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.LoanProducts
            .Include(p => p.Business)
            .FirstOrDefaultAsync(
                p => p.LoanProductId == loanProductId
                     && p.Business.UserId == userId,
                cancellationToken);

        if (product == null)
            return false;

        if (!product.IsActive)
            return true;

        product.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }


    // COMMON MAPPING METHOD
    private static LoanProductResponse MapToResponse(
        LoanProduct product)
    {
        return new LoanProductResponse
        {
            LoanProductId = product.LoanProductId,
            ProductName = product.ProductName,
            MinLoanAmount = product.MinLoanAmount,
            MaxLoanAmount = product.MaxLoanAmount,
            AnnualInterestRate = product.AnnualInterestRate,
            MinTenureMonths = product.MinTenureMonths,
            MaxTenureMonths = product.MaxTenureMonths,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}