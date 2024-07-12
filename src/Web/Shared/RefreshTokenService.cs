using Microsoft.AspNetCore.Components.Authorization;
using Podcast.Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcast.Shared
{
	public class RefreshTokenService
	{
		private readonly AuthenticationStateProvider _authProvider;
		private readonly IPodcastService _podcastService;
		public RefreshTokenService(AuthenticationStateProvider authProvider, IPodcastService podcastService)
		{
			_authProvider = authProvider;
			_podcastService = podcastService;
		}
		public async Task<string> TryRefreshToken()
		{
			var authState = await _authProvider.GetAuthenticationStateAsync();
			var user = authState.User;
			if (user.Identity.IsAuthenticated)
			{

				var exp = user.FindFirst(c => c.Type.Equals("exp"))?.Value;
				var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
				var timeUTC = DateTime.UtcNow;
				var diff = expTime - timeUTC;
				if (diff.TotalMinutes <= 1)
					return await _podcastService.RefreshToken();
			}
			return string.Empty;
		}
	}
}
