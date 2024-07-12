namespace Podcast.Infrastructure.Data.Models;

public class ShowCategory
{
    public ShowCategory(Guid showId, Guid categoryId)
    {
        ShowId = showId;
        CategoryId = categoryId;
    }

    public Guid ShowId { get; private set; }
    public Guid CategoryId { get; private set; }
    
    public Show? Show { get; private set; }
    public Category? Category { get; private set; }
}