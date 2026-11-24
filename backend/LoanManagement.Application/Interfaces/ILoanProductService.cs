
using LoanManagement.Application.DTOs.LoanProducts;

namespace LoanManagement.Application.Interfaces;

public interface ILoanProductService
{
    Task<LoanProductResponse> CreateAsync(
        int userId,
        CreateLoanProductRequest request,
        CancellationToken cancellationToken = default);

    Task<List<LoanProductResponse>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<LoanProductResponse?> GetByIdAsync(
        int userId,
        int loanProductId,
        CancellationToken cancellationToken = default);

    Task<LoanProductResponse?> UpdateAsync(
        int userId,
        int loanProductId,
        UpdateLoanProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        int userId,
        int loanProductId,
        CancellationToken cancellationToken = default);
}
