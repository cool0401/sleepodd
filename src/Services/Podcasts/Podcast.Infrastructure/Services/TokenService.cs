using Microsoft.IdentityModel.Tokens;
using Podcast.Infrastructure.Services.Interfaces;
using Podcast.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Podcast.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Podcast.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
		private readonly JwtSettings _jwtSettings;
		private readonly UserManager<User> _userManager;

		public TokenService(JwtSettings jwtSettings, UserManager<User> userManager)
		{
			_jwtSettings = jwtSettings;
			_userManager = userManager;
		}

		public string GenerateAccessToken(List<Claim> claims)
		{

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims,
				expires: DateTime.Now.AddMinutes(15),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return tokenString;
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}

		public async Task<List<Claim>> GetClaims(User user)
		{
			var roles = await _userManager.GetRolesAsync(user);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, roles.FirstOrDefault())
			};

			return claims;
		}

		public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = true, //you might want to validate the audience and issuer depending on your use case
				ValidateIssuer = true,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
				ValidateLifetime = false, //here we are saying that we don't care about the token's expiration date<]
				ValidIssuer = _jwtSettings.Issuer,
				ValidAudience = _jwtSettings.Audience
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("Invalid token");

			return principal;
		}
	}
}
