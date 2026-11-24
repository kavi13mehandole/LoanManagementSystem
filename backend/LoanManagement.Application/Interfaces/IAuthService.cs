
using LoanManagement.Application.DTOs.Auth;

namespace LoanManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterBusinessAsync(
        RegisterBusinessRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RegisterCustomerAsync(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

}
