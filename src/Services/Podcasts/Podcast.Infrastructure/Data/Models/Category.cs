using Podcast.Common.DTOs;

namespace Podcast.Infrastructure.Data.Models;

public class Category
{
    public Category(Guid id, string genre)
    {
        Id = id;
        Genre = genre;
    }

    public Category(CreateCategoryDto dto)
    {
        Id = Guid.NewGuid();
        Genre = dto.Genre;
    }

    public Guid Id { get; private set; }
    public string Genre { get; private set; }
    
    public ICollection<ShowCategory> ShowCategories { get; private set; } = new List<ShowCategory>();
}