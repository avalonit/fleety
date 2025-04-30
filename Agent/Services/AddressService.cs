using System.Text.Json;
using AITrackerAgent.Interfaces;
using AITrackerAgent.Classes;
using Microsoft.Extensions.Configuration;

namespace AITrackerAgent.Services
{
    public class AddressServices : IAddressService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration configuration;

        public AddressServices(IConfiguration configuration)
        {
            this._httpClient = new HttpClient();
            this.configuration = configuration;
        }

        public async Task<string> GetFormattedAddressFromCordinates(double lat, double lng)
        {
            var settings = configuration.Get<AppSettings>();

            var url = $"https://atlas.microsoft.com/reverseGeocode?api-version=2025-01-01&coordinates={lng.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&subscription-key={settings.AtlasMapKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(jsonResponse);

                return featureCollection?.Features?[0]?.Properties?.Address?.FormattedAddress ?? "Address not found";
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network issues, invalid JSON)
                return $"Error: {ex.Message}";
            }
        }
        public async Task<Address?> GetAddressFromCordinates(double lat, double lng)
        {
            var settings = configuration.Get<AppSettings>();
            var url = $"https://atlas.microsoft.com/reverseGeocode?api-version=2025-01-01&coordinates={lng.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&subscription-key={settings.AtlasMapKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(jsonResponse);

                return featureCollection?.Features?[0]?.Properties?.Address ?? null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

  
}
