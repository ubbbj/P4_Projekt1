using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SpotifyRecommendationApp.Services;
using SpotifyRecommendationApp.Models;
using SpotifyRecommendationApp.Data;

namespace SpotifyRecommendationApp;

public partial class MainWindow : Window
{
    private readonly SpotifyService _spotifyService;
    private readonly DatabaseService _databaseService;

    public MainWindow()
    {
        InitializeComponent();

        var configurationService = new ConfigurationService("config.cfg");

        string clientId = configurationService.GetClientId();
        string clientSecret = configurationService.GetClientSecret();
        string sqlConnectionString = configurationService.GetSqlConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(sqlConnectionString);

        _spotifyService = new SpotifyService(clientId, clientSecret);
        _databaseService = new DatabaseService(new AppDbContext(optionsBuilder.Options));

        LoadTracksFromDatabase();
    }

    private async void LoadTracksFromDatabase()
    {
        var tracks = await _databaseService.GetTracksFromDatabase();
        TracksListBox.ItemsSource = tracks;
    }


    private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl)
        {
            if (DatabaseTab.IsSelected)
            {
                LoadTracksFromDatabase();
            }
        }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string artists =
            await _spotifyService.GetSpotifyId(ArtistTextBox.Text);
        string tracks =
            await _spotifyService.GetSpotifyId(TrackTextBox.Text);
        string genres = GenreTextBox.Text;
        string amount = AmountTrack.Text;

        if (string.IsNullOrEmpty(artists) && string.IsNullOrEmpty(tracks) && string.IsNullOrEmpty(genres))
        {
            MessageBox.Show("Nie wprowadzono danych.");
            return;
        }

        List<string> recommendations =
            await _spotifyService.GetSpotifyRecommendations(artists, genres, tracks,
                amount);

        RecommendationsListBox.ItemsSource = recommendations;
    }


    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (RecommendationsListBox.SelectedItems.Count == RecommendationsListBox.Items.Count)
            RecommendationsListBox.UnselectAll();
        else
            RecommendationsListBox.SelectAll();
    }

    private async void SaveSelectedTracksButton_Click(object sender, RoutedEventArgs e)
    {
        bool anySelected = false;
        int totalRowsAffected = 0;

        foreach (var selectedItem in RecommendationsListBox.SelectedItems)
        {
            string trackInfo = selectedItem.ToString();
            string[] trackParts = trackInfo.Split(" - ");
            string artist = trackParts[0];
            string title = trackParts[1];

            bool isSaved = await _databaseService.SaveTrackToDatabase(title, artist);
            if (isSaved)
                totalRowsAffected++;

            anySelected = true;
        }

        if (anySelected)
            MessageBox.Show($"Dodano {totalRowsAffected} utworów do bazy danych.");
        else
            MessageBox.Show("Nie zaznaczono żadnych utworów do dodania.");
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadTracksFromDatabase();
    }

    private async void DeleteSelectedTracksButton_Click(object sender, RoutedEventArgs e)
    {
        List<Track> tracksToDelete = new List<Track>();

        foreach (var item in TracksListBox.SelectedItems)
        {
            if (item is Track selectedTrack)
            {
                tracksToDelete.Add(selectedTrack);
            }
        }

        foreach (Track track in tracksToDelete)
        {
            await _databaseService.DeleteTrackFromDatabase(track.Title, track.Artist);
        }

        LoadTracksFromDatabase();
    }

    private void SelectAllTracksButton_Click(object sender, RoutedEventArgs e)
    {
        if (TracksListBox.SelectedItems.Count == TracksListBox.Items.Count)
            TracksListBox.UnselectAll();
        else
            TracksListBox.SelectAll();
    }

    private async void SearchTrackTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTrackTextBox.Text;
        await FilterTracks(searchText);
    }

    private async Task FilterTracks(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            TracksListBox.ItemsSource = await _databaseService.GetTracksFromDatabase();
        }
        else
        {
            var allTracks = await _databaseService.GetTracksFromDatabase();
            var filteredTracks = allTracks
                .Where(track => track.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                track.Artist.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            TracksListBox.ItemsSource = filteredTracks;
        }
    }
}