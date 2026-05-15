﻿// using SysMan.Services.IApiServices;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SysMan.Services.IApiKCServices
{
    public class ApiKCServices : IApiKCServices
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                  .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json")
                  .Build();
        
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly NotificationService _notificationService;
        
        // ✅ KEYCLOAK TOKEN MANAGEMENT (for WSO2 API calls)
        private string? _keycloakToken;
        private DateTime _tokenExpiryTime;
        private readonly object _tokenLock = new object();
        
        // Keycloak configuration
        private readonly string _keycloakUrl;
        private readonly string _realm;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _username;
        private readonly string _password;

        public ApiKCServices(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, NotificationService notificationService)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            // _httpClient = httpClient;
            var _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://10.0.27.122:8243/apiFinal/v1/")
            };
            // if(!(_httpClient.BaseAddress != null))
            // _httpClient.BaseAddress = new Uri(configuration.GetConnectionString("API") ?? "https://10.0.27.122:8243/apiFinal/v1/");
            
            _authenticationStateProvider = authenticationStateProvider;
            _notificationService = notificationService;

            // Load Keycloak configuration
            _keycloakUrl = configuration["Keycloak:Url"] ?? "http://10.0.8.32:8080";
            _realm = configuration["Keycloak:Realm"] ?? "master";
            _clientId = configuration["Keycloak:ClientId"] ?? "apim-client";
            _clientSecret = configuration["Keycloak:ClientSecret"] ?? "HgRX1EYCnHCxZaPlfRMnoxPoXssfZdWw";
            _username = configuration["Keycloak:Username"] ?? "admin";
            _password = configuration["Keycloak:Password"] ?? "admin";

            Console.WriteLine($"✅ ApiServices initialized for WSO2 API: {_httpClient.BaseAddress}");
            Console.WriteLine($"✅ Keycloak configured: {_keycloakUrl}/realms/{_realm}");
        }

        // ✅ GET KEYCLOAK TOKEN FOR WSO2 API CALLS
        private async Task<string> GetKeycloakTokenAsync()
        {
            lock (_tokenLock)
            {
                // Check if current token is still valid
                if (!string.IsNullOrEmpty(_keycloakToken) && !IsKeycloakTokenExpired())
                {
                    Console.WriteLine("✅ Using cached Keycloak token");
                    return _keycloakToken;
                }
            }

            Console.WriteLine("🔄 Fetching new Keycloak token...");

            // Token is expired or doesn't exist, get a new one
            var success = await RefreshKeycloakTokenAsync();
            if (!success || string.IsNullOrEmpty(_keycloakToken))
            {
                throw new UnauthorizedAccessException("Failed to obtain Keycloak token for WSO2 API");
            }

            return _keycloakToken;
        }

        // ✅ WSO2 API CALLS WITH KEYCLOAK TOKEN
        private async Task<HttpResponseMessage> ExecuteWSO2ApiCallAsync(Func<Task<HttpResponseMessage>> httpOperation)
        {
            // Get Keycloak token for WSO2 API
            var keycloakToken = await GetKeycloakTokenAsync();
            
            // Set Keycloak Bearer token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", keycloakToken);
            
            Console.WriteLine($"🌐 WSO2 API call with Keycloak token: {keycloakToken.Substring(0, 50)}...");
            
            var response = await httpOperation();
            
            Console.WriteLine($"📡 WSO2 API Response: {response.StatusCode}");

            // If unauthorized, try to refresh Keycloak token once and retry
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("🔒 Got 401 from WSO2 API, refreshing Keycloak token...");
                
                // Clear token and get fresh one
                ClearToken();
                keycloakToken = await GetKeycloakTokenAsync();
                
                // Retry with fresh token
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", keycloakToken);
                response = await httpOperation();
                
                Console.WriteLine($"📡 WSO2 API Retry Response: {response.StatusCode}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _notificationService?.Notify(NotificationSeverity.Error,
                        "Lỗi xác thực",
                        "Không thể xác thực với WSO2 API. Vui lòng kiểm tra cấu hình Keycloak.",
                        duration: 5000);
                }
            }

            return response;
        }

        public async Task<List<T>> GetAll<T>(string apiPath, string lamda = null)
        {
            if (lamda == null)
            {
                apiPath = apiPath + "?lamda=\" \"";
            }
            else
            {
                apiPath = apiPath + "?lamda=" + lamda;
            }

            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.GetAsync(apiPath));
            Console.WriteLine($"===?==={response.IsSuccessStatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceResponse<List<T>>>();
                if (data == null || data.Data == null)
                {
                    throw new NullReferenceException("The API response or Data is null.");
                }
                return data.Data;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ WSO2 API Error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"WSO2 API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
                }
            }
        }

        public async Task<List<object>> GetByColumns(string apiPath, string columns = "", string lamda = null)
        {
            if (lamda == null)
            {
                apiPath = apiPath + "?columns=" + columns + "&lamda=\" \"";
            }
            else
            {
                apiPath = apiPath + "?columns=" + columns + "&lamda=" + lamda;
            }

            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.GetAsync(apiPath));

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceResponse<List<object>>>();
                if (data == null || data.Data == null)
                {
                    throw new NullReferenceException("The API response or Data is null.");
                }
                return data.Data;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"WSO2 API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }
        }

        public async Task<T> GetId<T>(string apiPath, string seqName)
        {
            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.GetAsync(apiPath + $"?seqName={seqName}"));

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ServiceResponse<T>>();
                if (data == null || data.Data == null)
                {
                    throw new NullReferenceException("The API response or Data is null.");
                }
                return data.Data;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"WSO2 API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
            }
        }

        public async Task Create<T>(string apiPath, object data)
        {
            var jsonContent = JsonContent.Create(data);
            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.PostAsync(apiPath, jsonContent));

            if (response.IsSuccessStatusCode)
            {
                _notificationService.Notify(NotificationSeverity.Success, $"Thông báo:", "Đã thêm mới dữ liệu", duration: 1500);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Create Error: {response.StatusCode} - {errorContent}");
                _notificationService.Notify(NotificationSeverity.Error, $"Có lỗi đã xảy ra: {response.StatusCode}", duration: -1);
            }
        }

        public async Task Update<T>(string apiPath, object data)
        {
            var jsonContent = JsonContent.Create(data);
            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.PutAsync(apiPath, jsonContent));

            if (response.IsSuccessStatusCode)
            {
                _notificationService.Notify(NotificationSeverity.Success, $"Thông báo:", "Đã cập nhật dữ liệu", duration: 1500);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Update Error: {response.StatusCode} - {errorContent}");
                _notificationService.Notify(NotificationSeverity.Error, $"Có lỗi đã xảy ra: {response.StatusCode}", duration: -1);
            }
        }

        public async Task Delete<T>(string apiPath, string lamda = "", bool showMessage = true)
        {
            apiPath = apiPath + "?lamda=" + lamda;
            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.DeleteAsync(apiPath));

            if (response.IsSuccessStatusCode)
            {
                if (showMessage)
                    _notificationService.Notify(NotificationSeverity.Info, $"Xoá dữ liệu thành công", duration: 2000);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Delete Error: {response.StatusCode} - {errorContent}");
                if (showMessage)
                    _notificationService.Notify(NotificationSeverity.Error, $"Lỗi khi xoá dữ liệu: {response.StatusCode}", duration: -1);
            }
        }

        public async Task Delete<T>(string apiPath, List<T> data)
        {
            var jsonContent = JsonContent.Create(data);
            var response = await ExecuteWSO2ApiCallAsync(() => _httpClient.PutAsync(apiPath, jsonContent));

            if (response.IsSuccessStatusCode)
            {
                _notificationService.Notify(NotificationSeverity.Info, $"Xoá dữ liệu thành công", duration: 2000);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ WSO2 API Bulk Delete Error: {response.StatusCode} - {errorContent}");
                _notificationService.Notify(NotificationSeverity.Error, $"Lỗi khi xoá dữ liệu: {response.StatusCode}", duration: -1);
            }
        }

        // ✅ KEYCLOAK TOKEN MANAGEMENT METHODS
        public async Task<bool> RefreshKeycloakTokenAsync()
        {
            try
            {
                var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
                Console.WriteLine($"🔑 Getting Keycloak token from: {tokenEndpoint}");

                // Prepare the request (same as your curl command)
                var requestContent = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "password"),
                    new("username", _username),
                    new("password", _password)
                };

                var formContent = new FormUrlEncodedContent(requestContent);

                // Create authorization header (Basic Auth - same as your curl)
                // var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
                // Console.WriteLine(credentials);
                var credentials = "ZGZhYmExMzEtYzk5Mi00OTcwLTg1OGEtYzIyN2FlNTc1MmM0OlR5Snk4SGJCaVhZeEtiMmxJdEtxbU9IYmM2cmk2VHlo";
                using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
                {
                    Content = formContent
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                // Use a separate HttpClient for token requests
                using var tokenHttpClient = new HttpClient();
                var response = await tokenHttpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"🌐 Keycloak Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(responseContent);
                    Console.WriteLine($"🌐 Keycloak Response: {tokenResponse.AccessToken}");
                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        lock (_tokenLock)
                        {
                            _keycloakToken = tokenResponse.AccessToken;
                            // Set expiry time with 30 seconds buffer
                            // _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                            // int k = 0;
                        }
                        
                        Console.WriteLine($"✅ Keycloak token obtained, expires at: {_tokenExpiryTime}");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Keycloak token request failed: {responseContent}");
                    _notificationService?.Notify(NotificationSeverity.Warning,
                        "Keycloak Warning",
                        $"Could not get Keycloak token: {response.StatusCode}",
                        duration: 3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during Keycloak token request: {ex.Message}");
                _notificationService?.Notify(NotificationSeverity.Error,
                    "Keycloak Error",
                    $"Exception getting Keycloak token: {ex.Message}",
                    duration: 5000);
            }

            return false;
        }

        public void ClearToken()
        {
            lock (_tokenLock)
            {
                _keycloakToken = null;
                _tokenExpiryTime = DateTime.MinValue;
            }
            Console.WriteLine("🧹 Cleared Keycloak token cache");
        }

        public bool IsTokenValid()
        {
            lock (_tokenLock)
            {
                return !string.IsNullOrEmpty(_keycloakToken) && !IsKeycloakTokenExpired();
            }
        }

        private bool IsKeycloakTokenExpired()
        {
            return DateTime.UtcNow >= _tokenExpiryTime;
        }

        // ✅ DEBUG AND UTILITY METHODS
        public async Task<string> GetCurrentKeycloakToken()
        {
            return await GetKeycloakTokenAsync();
        }

        public void LogKeycloakTokenInfo()
        {
            lock (_tokenLock)
            {
                Console.WriteLine("🔍 KEYCLOAK TOKEN INFO:");
                Console.WriteLine($"  Has Token: {!string.IsNullOrEmpty(_keycloakToken)}");
                Console.WriteLine($"  Expires At: {_tokenExpiryTime}");
                Console.WriteLine($"  Is Expired: {IsKeycloakTokenExpired()}");
                Console.WriteLine($"  Keycloak URL: {_keycloakUrl}");
                Console.WriteLine($"  Client ID: {_clientId}");
            }
        }

        public async Task<bool> TestKeycloakConnection()
        {
            try
            {
                Console.WriteLine("🧪 Testing Keycloak connection...");
                var success = await RefreshKeycloakTokenAsync();
                Console.WriteLine($"🧪 Keycloak test result: {success}");
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🧪 Keycloak test failed: {ex.Message}");
                return false;
            }
        }

        // Keycloak token response model
        private class KeycloakTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_expires_in")]
            public int RefreshExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("session_state")]
            public string? SessionState { get; set; }

            [JsonPropertyName("scope")]
            public string? Scope { get; set; }
        }
    }
}