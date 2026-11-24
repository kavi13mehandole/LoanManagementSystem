namespace LoanManagement.Application.DTOs.Auth;

public class AuthResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    // JWT token
    public string? Token { get; set; }

    // Token expiration time
    public DateTime? ExpiresAt { get; set; }
}