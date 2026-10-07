using CounsellingServices.Api.DTOs;
using CounsellingServices.Api.Domain.Entities;
using CounsellingServices.Api.Infrastructure;
using CounsellingServices.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CounsellingServices.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AppDbContext dbContext, ITokenService tokenService) : ControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("Full name, email, and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            return Conflict("Email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Role = request.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        if (request.Role == UserRole.Counsellor)
        {
            dbContext.CounsellorProfiles.Add(new CounsellorProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RegistrationNumber = $"REG-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                Specialty = "General Counselling"
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var token = tokenService.Generate(user);
        return Ok(new AuthResponse(token.token, token.expiresAt, user.Role.ToString(), user.Id));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = tokenService.Generate(user);
        return Ok(new AuthResponse(token.token, token.expiresAt, user.Role.ToString(), user.Id));
    }
}
