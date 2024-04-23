using System.IO;

namespace SpotifyRecommendationApp.Services;

public class ConfigurationService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _sqlConnectionString;

    public ConfigurationService(string configFilePath)
    {
        var lines = File.ReadAllLines(configFilePath);
        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts[0] == "ClientId")
            {
                _clientId = parts[1];
            }
            else if (parts[0] == "ClientSecret")
            {
                _clientSecret = parts[1];
            }
            else if (parts[0] == "SqlConnectionString")
            {
                _sqlConnectionString = parts[1];
            }
        }
    }

    public string GetClientId()
    {
        return _clientId;
    }

    public string GetClientSecret()
    {
        return _clientSecret;
    }

    public string GetSqlConnectionString()
    {
        return _sqlConnectionString;
    }
}