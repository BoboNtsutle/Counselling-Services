using CounsellingServices.Api.Domain.Entities;

namespace CounsellingServices.Api.DTOs;

public sealed record RegisterRequest(string FullName, string Email, string Password, UserRole Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, string Role, Guid UserId);
