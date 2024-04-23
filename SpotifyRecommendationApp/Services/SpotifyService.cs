using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace SpotifyRecommendationApp.Services;

public class SpotifyService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _spotifyToken;

    public SpotifyService(string clientId, string clientSecret)
    {
        this._clientId = clientId;
        this._clientSecret = clientSecret;
        this._spotifyToken = Task.Run(GetSpotifyToken).Result;
    }

    private async Task<string> GetSpotifyToken()
    {
        var client = new HttpClient();

        var requestContent = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }
        };

        var content = new FormUrlEncodedContent(requestContent);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<dynamic>(responseJson);
            return responseData.access_token;
        }
        else
        {
            throw new Exception("Nie można uzyskać tokenu API Spotify.");
        }
    }

    public async Task<string> GetSpotifyId(string searchQuery)
    {
        string spotifyApiUrl =
            $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(searchQuery)}&type=artist,track&limit=1";

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _spotifyToken);
            HttpResponseMessage response = await httpClient.GetAsync(spotifyApiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic searchData = JsonConvert.DeserializeObject(responseBody);

                if (searchData.artists.items.Count > 0)
                    return searchData.artists.items[0].id;
                else if (searchData.tracks.items.Count > 0)
                    return searchData.tracks.items[0].id;
            }
        }

        return null;
    }

    public async Task<List<string>> GetSpotifyRecommendations(string seedArtists, string seedGenres, string seedTracks,
        string limit)
    {
        List<string> recommendations = new List<string>();

        using (HttpClient client = new HttpClient())
        {
            string baseUrl =
                $"https://api.spotify.com/v1/recommendations?seed_artists={seedArtists}&seed_genres={seedGenres}&seed_tracks={seedTracks}&";

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _spotifyToken);
            HttpResponseMessage response = await client.GetAsync(baseUrl + $"&limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(responseBody);

                foreach (dynamic track in data.tracks)
                {
                    recommendations.Add($"{track.artists[0].name} - {track.name}");
                }
            }
            else
            {
                string errorMessage = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => "Nieprawidłowe żądanie. Sprawdź swoje kryteria wyszukiwania.",
                    HttpStatusCode.Unauthorized => "Nieautoryzowany. Sprawdź swój klucz API Spotify.",
                    _ => "Nie udało się pobrać rekomendacji z API Spotify."
                };

                MessageBox.Show(errorMessage);
            }
        }

        return recommendations;
    }
}