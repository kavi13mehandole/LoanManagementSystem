
using LoanManagement.Application.DTOs.Auth;
using LoanManagement.Application.Interfaces;
using LoanManagement.Infrastructure.Entities;
using LoanManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoanManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
     ApplicationDbContext context,
     IPasswordHasher<User> passwordHasher,
     IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterBusinessAsync(
        RegisterBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Check whether email already exists
        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == email, cancellationToken);

        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email is already registered."
            };
        }

        var user = new User
        {
            Email = email,
            Role = "Business",
            IsActive = true
        };

        // Hash password before saving
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        // Create associated business profile
        user.BusinessProfile = new BusinessProfile
        {
            BusinessName = request.BusinessName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim()
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.Users.Add(user);

            // Save user and profile in the same transaction
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new AuthResponse
            {
                Success = true,
                Message = "Business registered successfully.",
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed. Please verify the details or check whether the email is already registered."
            };
        }
    }

    public async Task<AuthResponse> RegisterCustomerAsync(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Check whether email already exists
        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == email, cancellationToken);

        if (emailExists)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email is already registered."
            };
        }

        var user = new User
        {
            Email = email,
            Role = "Customer",
            IsActive = true
        };

        // Hash password before saving
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        // Create associated customer profile
        user.CustomerProfile = new CustomerProfile
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim()
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.Users.Add(user);

            // Save user and profile in the same transaction
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new AuthResponse
            {
                Success = true,
                Message = "Customer registered successfully.",
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed. Please verify the details or check whether the email is already registered."
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == email && u.IsActive,
                cancellationToken);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // JWT settings
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        var expiryMinutes = int.Parse(
            _configuration["Jwt:ExpiryMinutes"] ?? "60");

        // Create claims
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

        // Create signing key
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        // Generate token
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserId,
            Email = user.Email,
            Role = user.Role,
            Token = tokenString,
            ExpiresAt = expiresAt
        };
    }
}
