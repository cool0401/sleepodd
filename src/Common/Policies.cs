using Microsoft.AspNetCore.Authorization;

namespace Podcast.Common
{
	public static class Policies
	{
		public const string IsAdmin = "Admin";
		public const string IsUser = "User";
		public const string IsReadOnly = "IsReadOnly";

		public static AuthorizationPolicy IsAdminPolicy()
		{
			return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
												   .RequireRole("Admin")
												   .Build();
		}

		public static AuthorizationPolicy IsUserPolicy()
		{
			return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
												   .RequireRole("User")
												   .Build();
		}

		public static AuthorizationPolicy IsReadOnlyPolicy()
		{
			return new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.RequireClaim("ReadOnly", "true")
				.Build();
		}
	}
}
