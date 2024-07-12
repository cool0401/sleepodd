using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcast.Common.DTOs
{
	public record SignInResultDto
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
	}
}
