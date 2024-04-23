using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyRecommendationApp.Models;
using SpotifyRecommendationApp.Data;

namespace SpotifyRecommendationApp.Services;

public class DatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Track>> GetTracksFromDatabase()
    {
        return await _context.Tracks.ToListAsync();
    }

    public async Task<bool> SaveTrackToDatabase(string title, string artist)
    {
        var existingTrack = await _context.Tracks
            .FirstOrDefaultAsync(t => t.Title == title && t.Artist == artist);

        if (existingTrack != null)
        {
            return false;
        }

        var track = new Track { Title = title, Artist = artist };
        _context.Tracks.Add(track);
        int rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task DeleteTrackFromDatabase(string title, string artist)
    {
        var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Title == title && t.Artist == artist);
        if (track != null)
        {
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
        }
    }
}