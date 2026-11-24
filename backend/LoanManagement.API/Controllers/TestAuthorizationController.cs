using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestAuthorizationController : ControllerBase
{
    // Any authenticated user can access
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Message = "You are authenticated.",
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    // Only Business users can access
    [Authorize(Roles = "Business")]
    [HttpGet("business")]
    public IActionResult BusinessOnly()
    {
        return Ok(new
        {
            Message = "Welcome Business user!"
        });
    }

    // Only Customer users can access
    [Authorize(Roles = "Customer")]
    [HttpGet("customer")]
    public IActionResult CustomerOnly()
    {
        return Ok(new
        {
            Message = "Welcome Customer!"
        });
    }

    // Only Admin users can access
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            Message = "Welcome Admin!"
        });
    }
}