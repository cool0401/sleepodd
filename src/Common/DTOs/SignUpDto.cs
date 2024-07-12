namespace Podcast.Common.DTOs
{
	public record SignUpDto
	{
		public string Email { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

	}
}
