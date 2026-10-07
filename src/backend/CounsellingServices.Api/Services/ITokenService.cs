using CounsellingServices.Api.Domain.Entities;

namespace CounsellingServices.Api.Services;

public interface ITokenService
{
    (string token, DateTimeOffset expiresAt) Generate(User user);
}
