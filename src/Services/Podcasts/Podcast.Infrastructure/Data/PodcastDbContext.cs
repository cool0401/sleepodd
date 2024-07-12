using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Podcast.Infrastructure.Data.Models;

namespace Podcast.Infrastructure.Data;

public class PodcastDbContext : IdentityDbContext<User, Role, Guid>
{
    protected PodcastDbContext()
    {
    }

    public PodcastDbContext(DbContextOptions<PodcastDbContext> options) : base(options)
    {
    }

    public DbSet<Show> Shows => Set<Show>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<ShowCategory> ShowCategories => Set<ShowCategory>();
    /*public DbSet<Feed> Feeds => Set<Feed>();*/
    /*public DbSet<UserSubmittedFeed> UserSubmittedFeeds => Set<UserSubmittedFeed>();*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    modelBuilder.Entity<ShowCategory>().HasKey(sc => new { sc.ShowId, sc.CategoryId });
	    modelBuilder.Entity<ShowCategory>()
                    .HasOne(sc => sc.Show)
                    .WithMany(s => s.ShowCategories)
                    .HasForeignKey(sc => sc.ShowId);
        
        modelBuilder.Entity<ShowCategory>()
            .HasOne(sc => sc.Category)
            .WithMany(c => c.ShowCategories)
            .HasForeignKey(sc => sc.CategoryId);
                
        /*modelBuilder.Entity<Feed>().HasData(Seed.Feeds);*/
        modelBuilder.Entity<Category>().HasData(Seed.Categories);
        /*modelBuilder.Entity<FeedCategory>().HasData(Seed.FeedCategories);*/
        /*modelBuilder.Entity<FeedCategory>().HasKey(prop => new { prop.FeedId, prop.CategoryId });*/
        
		base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
				 .HasData(new Role { Name = "User", NormalizedName = "USER", Id = Guid.NewGuid(), ConcurrencyStamp = Guid.NewGuid().ToString() });
		modelBuilder.Entity<Role>()
			   .HasData(new Role { Name = "Admin", NormalizedName = "ADMIN", Id = Guid.NewGuid(), ConcurrencyStamp = Guid.NewGuid().ToString() });
    }
}