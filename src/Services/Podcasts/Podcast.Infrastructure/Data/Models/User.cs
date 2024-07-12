using Microsoft.AspNetCore.Identity;
using Podcast.Common.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Podcast.Infrastructure.Data.Models;

public class User : IdentityUser<Guid>
{
    [Required]
    public string Name { get; set; }

	public string? RefreshToken { get; set; }

	public string? CustomerId { get; set; }

	public DateTime RefreshTokenExpiryTime { get; set; }

    public User() { }

    public User(SignUpDto dto)
    {
        Name = dto.FullName;
        UserName = dto.UserName;
        Email = dto.Email;
    }
}
