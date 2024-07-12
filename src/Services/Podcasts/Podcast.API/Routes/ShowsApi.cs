using Podcast.Common.DTOs;
using Microsoft.AspNetCore.Identity;
using Podcast.Infrastructure.Http;

namespace Podcast.API.Routes;

public static class ShowsApi
{
    public static RouteGroupBuilder MapShowsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllShows).WithName("GetShows");
        group.MapGet("/{id}", GetShowById).WithName("GetShowsById");
        group.MapPost("/", CreateShow).RequireAuthorization().WithName("CreateShow");
        return group;
    }

    public static async ValueTask<Ok<List<ShowDto>>> GetAllShows(int limit, string? term, Guid? categoryId, CancellationToken cancellationToken, PodcastDbContext podcastDbContext, ShowClient showClient)
    {
        var showsQuery = podcastDbContext.Shows.Include(show => show.ShowCategories)
        .ThenInclude(x => x.Category)
        .AsQueryable();

        if (!string.IsNullOrEmpty(term))
            showsQuery = showsQuery.Where(show => show.Title.Contains(term));

        if (categoryId is not null)
            showsQuery = showsQuery.Where(show =>
                show.ShowCategories.Any(y => y.Category!.Id == categoryId));

        var shows = await showsQuery.OrderByDescending(x => x.Title)
            .Take(limit)
            .Select(x => new ShowDto(x))
            .ToListAsync(cancellationToken);

        //List<ShowDto> showsWithValidLinks = Task.WhenAll(shows.Select(async show => await showClient.CheckLink(show.Link) ? show : null)).Result.Where(show => show is not null).ToList()!;
        return TypedResults.Ok(shows);
    }


    public static async ValueTask<Results<NotFound, Ok<ShowDto>>> GetShowById(PodcastDbContext podcastDbContext, Guid id, CancellationToken cancellationToken)
    {
        var show = await podcastDbContext.Shows
            .Include(show => show.Episodes.OrderByDescending(episode => episode.Published))
            .Include(show => show.ShowCategories)
            .ThenInclude(x => x.Category)
            .Where(x => x.Id == id)
            .Select(show => new ShowDto(show))
            .FirstOrDefaultAsync(cancellationToken);
        return show == null ? TypedResults.NotFound() : TypedResults.Ok(show);
    }

    public static async ValueTask<Results<NotFound, BadRequest<string>, Ok<string>>> CreateShow(
        HttpContext context,
        PodcastDbContext podcastDbContext, 
        CreateShowDto dto,
        UserManager<User> userManager,
        CancellationToken cancellationToken)
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");
        if (string.IsNullOrEmpty(dto.Title) || string.IsNullOrEmpty(dto.Description) || string.IsNullOrEmpty(dto.Image) || string.IsNullOrEmpty(dto.Category))
        {
            return TypedResults.BadRequest("Incorrect input");
        }

        var user = await userManager.FindByNameAsync(context.User.Identity.Name);
        
        if (user is null)
        	return TypedResults.BadRequest("User with this email address not found.");
                
        var newShow = new Show(user.Name,  dto.Description, user.Email, "en-us", dto.Title, "", dto.Image, DateTime.Now);
        
        var showCategory = new ShowCategory(newShow.Id, Guid.Parse(dto.Category));
        
        newShow.ShowCategories.Add(showCategory);
        podcastDbContext.Shows.Add(newShow);
        await podcastDbContext.SaveChangesAsync(cancellationToken);
        
        return TypedResults.Ok("Creating show is successful");
    }
}
