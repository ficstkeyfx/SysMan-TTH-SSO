using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using System.Net.Http.Headers;
using SysMan.Services.ITVFServices;
using SysMan.Models;
using IDC.Shared.Models.SysMan;
namespace SysMan
{
    public class TVFServices : ITVFServices
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                  .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json")
                  .Build();
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        readonly NotificationService _notificationService;
        public TVFServices(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, NotificationService notificationService)
        {
            httpClient = new HttpClient { BaseAddress = new Uri(configuration.GetConnectionString("API") ?? "http://localhost:5240") };
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _notificationService = notificationService;
        }

        // Helper method to retrieve the token
        private async Task<string> GetTokenAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            string token = authState.User.Identity?.Name ?? "";
            return token;
        }
        public async Task<List<tbChucNang>> getChucNang(string apiPath, int IdUser)
        {
            string token = await GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response;
            apiPath = apiPath + "?IdUser=" + IdUser;
            response = await _httpClient.GetAsync(apiPath);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceResponse<List<tbChucNang>>>();
                if (data == null || data.Data == null)
                {
                    throw new NullReferenceException("The API response or Data is null.");
                }
                return data.Data;
            }
            else
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }
        }
        public async Task<List<tbSystemUser>> getNguoiDungThuocNhom(string apiPath, int IdNhom)
        {
            string token = await GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response;
            apiPath = apiPath + "?IdNhom=" + IdNhom;
            response = await _httpClient.GetAsync(apiPath);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceResponse<List<tbSystemUser>>>();
                if (data == null || data.Data == null)
                {
                    throw new NullReferenceException("The API response or Data is null.");
                }
                return data.Data;
            }
            else
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }
        }
    }
}