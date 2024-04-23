using SpotifyRecommendationApp.Models;

namespace SpotifyRecommendationApp.Data;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Track> Tracks { get; set; }
}