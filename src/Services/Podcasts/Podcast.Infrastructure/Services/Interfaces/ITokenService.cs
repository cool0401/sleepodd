using Podcast.Infrastructure.Data.Models;
using System.Security.Claims;

namespace Podcast.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
		string GenerateAccessToken(List<Claim> claims);
		string GenerateRefreshToken();
		Task<List<Claim>> GetClaims(User user);
		ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
	}
}
