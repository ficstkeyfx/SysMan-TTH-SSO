using api.SsoKeyCloak;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using api.Services.KeyCloakServices;
namespace api.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ChangePasswordController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _keycloakUrl;
        private readonly string _realm;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _adminUsername;
        private readonly string _adminPassword;


        private readonly IKeyCloakService _keycloakRepo;
        //public ChangePasswordController(HttpClient httpClient)
        //{
        //    _httpClient = httpClient;
        //    _keycloakUrl = "http://10.0.8.32:8080";
        //    _realm = "TestSSO";
        //    _clientId = "TestSSO";
        //    _clientSecret = "XQ70pctA5bqZAwZWbMRV52uVsjv50rHf";
        //    _adminUsername = "admin";
        //    _adminPassword = "admin";
        //}

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordWithUserModel model)
        {
            try
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu mới không khớp với mật khẩu xác nhận");
                }

                // Verify user credentials first
                var userToken = await _keycloakRepo.GetUserAccessToken(model.Username, model.CurrentPassword);
                if (userToken == null)
                    return Unauthorized("Invalid current credentials");

                // Get admin token
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                if (adminToken == null)
                    return StatusCode(500, "Failed to get admin access");

                // Get user ID using admin token
                var userId = await _keycloakRepo.GetUserId(model.Username, adminToken);
                if (string.IsNullOrEmpty(userId))
                    return BadRequest("Không tìm thấy tài khoản");

                // Change password
                var success = await _keycloakRepo.ChangeUserPassword(userId, model.NewPassword, adminToken);
                if (success)
                    return Ok(new { message = "Đổi mật khẩu thành công" });

                return StatusCode(500, new { message = "Kiểm tra lại mật khẩu, đổi mật khẩu không thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        //private async Task<string> GetUserToken(string username, string password)
        //{
        //    var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
        //    var parameters = new Dictionary<string, string>
        //    {
        //        { "client_id", _clientId },
        //        { "client_secret", _clientSecret },
        //        { "grant_type", "password" },
        //        { "username", username },
        //        { "password", password }
        //    };

        //    var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
        //    if (!response.IsSuccessStatusCode) return null;

        //    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        //    return result?.Access_token;
        //}

        //private async Task<string> GetAdminToken()
        //{
        //    var tokenEndpoint = $"{_keycloakUrl}/realms/master/protocol/openid-connect/token";
        //    var parameters = new Dictionary<string, string>
        //    {
        //        { "client_id", "admin-cli" },
        //        { "grant_type", "password" },
        //        { "username", _adminUsername },
        //        { "password", _adminPassword }
        //    };

        //    var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));
        //    if (!response.IsSuccessStatusCode) return null;

        //    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        //    return result?.Access_token;
        //}

        //private async Task<string> GetUserId(string username, string adminToken)
        //{
        //    var userEndpoint = $"{_keycloakUrl}/admin/realms/{_realm}/users?username={username}";
        //    _httpClient.DefaultRequestHeaders.Authorization = 
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        //    var response = await _httpClient.GetAsync(userEndpoint);
        //    if (!response.IsSuccessStatusCode) return null;

        //    var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>();
        //    return users?.FirstOrDefault()?.Id;
        //}

        //private async Task<bool> ChangeUserPassword(string userId, string newPassword, string adminToken)
        //{
        //    var passwordEndpoint = $"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/reset-password";
        //    _httpClient.DefaultRequestHeaders.Authorization = 
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        //    var content = JsonContent.Create(new
        //    {
        //        type = "password",
        //        value = newPassword,
        //        temporary = false
        //    });

        //    var response = await _httpClient.PutAsync(passwordEndpoint, content);
        //    return response.IsSuccessStatusCode;
        //}
    }

    //public class ChangePasswordWithUserModel
    //{
    //    public string Username { get; set; }
    //    public string CurrentPassword { get; set; }
    //    public string NewPassword { get; set; }
    //    public string ConfirmPassword { get; set; }
    //}

    //public class KeycloakUser
    //{
    //    public string Id { get; set; }
    //    public string Username { get; set; }
    //}
}