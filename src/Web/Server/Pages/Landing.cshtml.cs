using Microsoft.AspNetCore.Mvc.RazorPages;
using Podcast.Shared;
using Podcast.Shared.Interface;

namespace Podcast.Server.Pages
{
    public class Landing : PageModel
    {
        private readonly IPodcastService _podcastService;

        public Show[]? FeaturedShows { get; set; }

        public Landing(IPodcastService podcastService)
        {
            _podcastService = podcastService;
        }

        public async Task OnGet()
        {
            var shows = await _podcastService.GetShows(50, null);
            FeaturedShows = shows?.Where(s => s.IsFeatured).ToArray();
        }
    }
}
