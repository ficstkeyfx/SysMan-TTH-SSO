﻿using System.IdentityModel.Tokens.Jwt;
using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.SsoKeyCloak
{
    //public class KeycloakAuthService
    //{
    //    private readonly HttpClient _httpClient;
        
    //    public KeycloakAuthService()
    //    {
    //        _httpClient = new HttpClient();
    //    }

    //    public string? GetUserFromToken(string token)
    //    {
    //        try
    //        {
    //            var handler = new JwtSecurityTokenHandler();
    //            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

    //            if (jsonToken != null)
    //            {
    //                return jsonToken.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value;
    //            }
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //        return null;
    //    }

    //    public async Task<TokenResponse?> GetTokenAsync(string username, string password)
    //    {
    //        var tokenEndpoint = "http://172.17.10.77:8080/realms/TestSSO/protocol/openid-connect/token";

    //        var parameters = new Dictionary<string, string>
    //        {
    //            { "client_id", "TestSSO" },  
    //            { "client_secret", "XQ70pctA5bqZAwZWbMRV52uVsjv50rHf"}, 
    //            { "grant_type", "password" }, 
    //            { "username", username }, 
    //            { "password", password }  
    //        };

    //        var response = await _httpClient.PostAsync(tokenEndpoint,
    //            new FormUrlEncodedContent(parameters));

    //        if (response.IsSuccessStatusCode)
    //        {
    //            return await response.Content.ReadFromJsonAsync<TokenResponse>();
    //        }

    //        return null;
    //    }
    //}

    [ApiController]
    [Route("api/[controller]")]
    public class AuthKeyCloakController : ControllerBase
    {
        //private readonly KeycloakAuthService _authService;
        private readonly IKeyCloakService _keycloakRepo;
        //    public AuthKeyCloakController()
        //{
        //    _authService = new KeycloakAuthService();
        //}
        public AuthKeyCloakController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _keycloakRepo.GetUserAccessToken(model.Username, model.Password);
            Console.WriteLine("Result:",result);

            if (result != null)
            {
                return Ok(result);
            }

            return Unauthorized();
        }

        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("No token provided");
            }

            var username = _keycloakRepo.GetUsernameFromToken(token);
            
            if (username != null)
            {
                return Ok(new { Username = username });
            }

            return Unauthorized("Invalid token");
        }
    }

    //public class LoginModel
    //{
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //}

    //public class TokenResponse
    //{
    //    public string Access_token { get; set; }
    //    public string Refresh_token { get; set; }
    //    public int Expires_in { get; set; }
    //    public string Token_type { get; set; }
    //}
}
