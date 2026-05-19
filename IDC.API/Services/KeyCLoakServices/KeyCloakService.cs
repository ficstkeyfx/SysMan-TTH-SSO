// using api.UserSync;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using api.Controllers.AuthControllers;
namespace api.Services.KeyCloakServices
{
    public class KeycloakService : IKeyCloakService
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                  .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json")
                  .Build();

        private static SqlConnection _db;
        private readonly ILogger<KeycloakService> _logger;
        private string _keycloakUrl = $"http://192.168.93.198:8080";
        private string _realm = $"TestSSO";
        private string _clientId = $"TestSSO";
        private string _clientSecret = $"XsSps46oZSEvYbYJR3oQuIg57vffKYZH";
        private string _adminUsername = $"admin";
        private string _adminPassword = $"admin";

        public KeycloakService(SqlConnection db, ILogger<KeycloakService> logger)
        {
            _db = db;
            _logger = logger;

        }

        /// <summary>
        /// Phương thức lấy tên đăng nhập từ token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Tên đăng nhập</returns>
        public string? GetUsernameFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    return jsonToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Phương thức lấy thông tin token của người dùng từ ứng dụng KeyCloak
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu đăng nhập</param>
        /// <returns>Thông tin về token</returns>
        public async Task<TokenResponse?> GetUserTokenInfo(string username, string password)
        {
            //var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";

            // urlBaseApi của KeyCloak
            string apiUrl = _keycloakUrl;

            // Đường dẫn lấy token
            string apiRoute = $"realms/{_realm}/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret},
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            };

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            //var response = await _httpClient.PostAsync(tokenEndpoint,
            //    new FormUrlEncodedContent(parameters));
            var response = await _httpClient.PostAsync(apiRoute,
                new FormUrlEncodedContent(parameters));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenResponse>();
            }

            return null;
        }


        /// <summary>
        /// Phương thức lấy Access_token của người dùng
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Access Token</returns>
        public async Task<string> GetUserAccessToken(string username, string password)
        {
            var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            };

            string apiUrl = _keycloakUrl;
            string apiRoute = $"realms/{_realm}/protocol/openid-connect/token";
            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
            // var content = await response.Content.ReadAsStringAsync();

            // Console.WriteLine(content);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.Access_token;
        }


        /// <summary>
        /// Phương thức lấy Access Token của quản trị hệ thống
        /// </summary>
        /// <returns>Access Token</returns>
        public async Task<string?> GetAdminAccessToken()
        {
            //var tokenEndpoint = $"{_keycloakUrl}/realms/master/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                { "client_id", "admin-cli" },
                { "grant_type", "password" },
                { "username", _adminUsername },
                { "password", _adminPassword }
            };

            string apiUrl = _keycloakUrl;
            string apiRoute = $"realms/master/protocol/openid-connect/token";
            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            var response = await _httpClient.PostAsync(apiRoute,
                new FormUrlEncodedContent(parameters));


            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return tokenResponse?.Access_token;
            }

            return null;
        }
        /// <summary>
        /// Phương thức lấy số định danh người dùng
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="adminToken">Token của người quản trị</param>
        /// <returns>Định danh của người dùng</returns>
        public async Task<string> GetUserId(string username, string adminToken)
        {
            //var userEndpoint = $"{_keycloakUrl}/admin/realms/{_realm}/users?username={username}";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users?username={username}";
            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };


            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _httpClient.GetAsync(apiRoute);
            if (!response.IsSuccessStatusCode) return null;

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>();
            return users?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Phương thức lấy thông tin người dùng KeyCloak
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns></returns>
        public async Task<KeycloakUserResponse?> GetUserByUsername(string username)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return null;
            }

            //var searchEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users?username={username}&exact=true";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users?username={username}&exact=true";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            var response = await _httpClient.GetAsync(apiRoute);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>();
            return users?.FirstOrDefault();
        }

        public async Task<List<UserDto>> GetUsers()
        {
            var token = await GetAdminAccessToken();
            if (token == null) return null;

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users?briefRepresentation=false&max=1000&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };
            _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");

            var response = await _httpClient.GetAsync(apiRoute);
            if (!response.IsSuccessStatusCode) return null;

            var usersJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            var users = new List<UserDto>();

            foreach (var u in usersJson.EnumerateArray())
            {
                var username = u.TryGetProperty("username", out var usr) ? usr.GetString() : "";
                var firstName = u.TryGetProperty("firstName", out var fn) ? fn.GetString() : "";
                var lastName = u.TryGetProperty("lastName", out var ln) ? ln.GetString() : "";
                var email = u.TryGetProperty("email", out var em) ? em.GetString() : "";
                var enabled = !u.TryGetProperty("enabled", out var en) || en.GetBoolean();

                users.Add(new UserDto
                {
                    Username = username ?? "",
                    Name = $"{firstName} {lastName}".Trim(),
                    Email = email ?? "",
                    Enabled = enabled
                });
            }

            return users;
        }

        public async Task ClearRealmCache(string adminToken)
        {
            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/clear-caches";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            // Keycloak API để clear user cache
            await _httpClient.PostAsync($"/admin/realms/{_realm}/clear-user-cache", null);
        }

        /// <summary>
        /// Phương thức kiểm tra một người dùng có trong CSDL hay không
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>true: nếu tồn tại; flase: không tồn tại</returns>
        public async Task<bool> UsernameExistsInDatabase(string username)
        {

            string _connectionString = "Server=172.17.10.77,1433;Database=keycloak_db;User Id=sa;Password=@Abc12345;TrustServerCertificate=True;";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = "SELECT COUNT(*) FROM users WHERE username = @Username";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database error when checking username: {ex.Message}");
                throw;
            }
            return false;
        }


        /// <summary>
        /// Phương thức kiểm tra một người dùng có tồn tại hay không
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<bool> CheckUserExists(string username)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return false;
            }

            //var searchEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users?username={username}&exact=true";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users?username={username}&exact=true";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            var response = await _httpClient.GetAsync(apiRoute);

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>();
                return users?.Any() == true;
            }

            return false;
        }


        /// <summary>
        /// Phương thức tạo một người dùng KeyCloak
        /// </summary>
        /// <param name="newUser">Người dùng mới</param>
        /// <returns></returns>

        public async Task<CreateUserResponse> CreateUser(CreateUserRequest newUser)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return new CreateUserResponse
                {
                    Success = false,
                    Message = "Failed to obtain admin token"
                };
            }

            //var createUserEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users";

            var keycloakUser = new KeycloakUserModel
            {
                Username = newUser.Username,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Enabled = true,
                EmailVerified = false,
                Credentials = new List<KeycloakCredential>
                {
                    new KeycloakCredential
                    {
                        Type = "password",
                        Value = newUser.Password,
                        Temporary = false
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(keycloakUser, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users";
            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiRoute, content);

            if (response.IsSuccessStatusCode)
            {
                // Get the user ID from the Location header
                var locationHeader = response.Headers.Location?.ToString();
                var userId = locationHeader?.Split('/').LastOrDefault();

                return new CreateUserResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    UserId = userId
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new CreateUserResponse
                {
                    Success = false,
                    Message = $"Failed to create user: {response.StatusCode} - {errorContent}"
                };
            }
        }

        /// <summary>
        /// Phương thức đổi mật khẩu của người dùng
        /// </summary>
        /// <param name="userId">Số định danh của người dùng</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <param name="adminToken">Token của người quản trị</param>
        /// <returns></returns>

        public async Task<bool> ChangeUserPassword(string userId, string newPassword, string adminToken)
        {
            //var passwordEndpoint = $"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/reset-password";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users/{userId}/reset-password";
            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };


            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var content = JsonContent.Create(new
            {
                type = "password",
                value = newPassword,
                temporary = false
            });

            var response = await _httpClient.PutAsync(apiRoute, content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Phương thức xoá một người dùng
        /// </summary>
        /// <param name="userId">Số định danh người dùng</param>
        /// <returns>true: nếu xoá được; flase: không xoá được</returns>
        public async Task<bool> DeleteUser(string userId)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return false;
            }

            //var deleteUserEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users/{userId}";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            var response = await _httpClient.DeleteAsync(apiRoute);
            return response.IsSuccessStatusCode;
        }



        /// <summary>
        /// Phương thức Disable một người dùng
        /// </summary>
        /// <param name="userId">Số định danh người dùng</param>
        /// <returns></returns>
        public async Task<bool> DisableUser(string userId)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return false;
            }

            //var updateUserEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users/{userId}";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            // Chỉ gửi trường Enabled = false
            var payload = new { enabled = false };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiRoute, content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Phương thức Enable một người dùng
        /// </summary>
        /// <param name="userId">Số định danh người dùng</param>
        /// <returns></returns>
        public async Task<bool> EnableUser(string userId)
        {
            var adminToken = await GetAdminAccessToken();
            if (string.IsNullOrEmpty(adminToken))
            {
                return false;
            }

            //var updateUserEndpoint = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";

            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users/{userId}";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            // Chỉ gửi trường Enabled = true
            var payload = new { enabled = true };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiRoute, content);
            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Phương thức xác thực các thông tin người dùng
        /// </summary>
        /// <param name="request">Các thông tin xác thực</param>
        /// <returns></returns>
        public async Task<bool> ValidateUserInfo(ResetPasswordRequest request)
        {
            string _connectionString = "Server=172.17.10.77,1433;Database=keycloak_db;User Id=sa;Password=@Abc12345;TrustServerCertificate=True;";
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"
                    SELECT COUNT(*) FROM users 
                    WHERE username = @Username 
                    AND email = @Email 
                    AND SDT = @SDT 
                    AND CCCD = @CCCD;";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", request.Username);
                command.Parameters.AddWithValue("@Email", request.Email);
                // command.Parameters.AddWithValue("@SDT", request.SDT);
                // command.Parameters.AddWithValue("@CCCD", request.CCCD);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database error: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdateUser(string userId, string email, string firstName, string lastName, string adminToken)
        {
            string apiUrl = _keycloakUrl;
            string apiRoute = $"/admin/realms/{_realm}/users/{userId}";

            using var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            var payload = new
            {
                email = email,
                firstName = firstName,
                lastName = lastName
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiRoute, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPassword(string username, string newPassword)
        {
            try
            {
                var adminToken = await GetAdminAccessToken();
                if (string.IsNullOrEmpty(adminToken))
                {
                    _logger.LogError("Failed to obtain Keycloak admin token.");
                    return false;
                }

                // 🔹 Lấy userId từ Keycloak dựa vào username
                string keycloakApiUrl = $"http://192.168.93.198:8080/admin/realms/TestSSO/users?username={username}";
                var request = new HttpRequestMessage(HttpMethod.Get, keycloakApiUrl);
                request.Headers.Add("Authorization", $"Bearer {adminToken}");


                string apiUrl = _keycloakUrl;
                string apiRoute = $"/admin/realms/TestSSO/users?username={username}";

                using var _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(configuration.GetConnectionString(apiUrl) ?? apiUrl)
                };


                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch user ID from Keycloak. Status: {response.StatusCode}");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<KeycloakUserReset>>(content);
                if (users == null || users.Count == 0)
                {
                    _logger.LogError("User not found in Keycloak.");
                    return false;
                }

                string userId = users[0].Id;
                _logger.LogInformation($"Updating password for Keycloak user ID: {userId}");

                // 🔹 Cập nhật mật khẩu mới
                string passwordUpdateUrl = $"http://192.168.93.198:8080/admin/realms/TestSSO/users/{userId}/reset-password";
                var passwordUpdatePayload = new
                {
                    type = "password",
                    value = newPassword,
                    temporary = false
                };

                var passwordUpdateRequest = new HttpRequestMessage(HttpMethod.Put, passwordUpdateUrl)
                {
                    Headers = { { "Authorization", $"Bearer {adminToken}" } },
                    Content = new StringContent(JsonConvert.SerializeObject(passwordUpdatePayload), Encoding.UTF8, "application/json")
                };

                var passwordUpdateResponse = await _httpClient.SendAsync(passwordUpdateRequest);
                if (!passwordUpdateResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to update password in Keycloak. Status: {passwordUpdateResponse.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating password in Keycloak: {ex.Message}");
                return false;
            }
        }
    }
}
