using Podcast.Common.DTOs;

namespace Podcast.API.Routes;

public static class CategoriesApi
{
    public static RouteGroupBuilder MapCategoriesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCategories).WithName("GetCategories");
        group.MapPost("/create", CreateCategory).WithName("CreateCategory");
        return group;
    }

    public static async ValueTask<Ok<List<CategoryDto>>> GetAllCategories(PodcastDbContext podcastDbContext, CancellationToken cancellationToken)
    {
        var categories = await podcastDbContext.Categories.Select(x => new CategoryDto(x.Id, x.Genre))
            .ToListAsync(cancellationToken);
        return TypedResults.Ok(categories);
    }

    public static async ValueTask<Results<BadRequest<string>, Ok<string>>> CreateCategory(PodcastDbContext podcastDbContext, CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");
        if (string.IsNullOrEmpty(dto.Genre))
        {
            return TypedResults.BadRequest("Incorrect input");
        }

        var newCategory = new Category(dto);
        
        podcastDbContext.Categories.Add(newCategory);
        await podcastDbContext.SaveChangesAsync(cancellationToken);
        
        return TypedResults.Ok("Creating category is successful");
    }
}
