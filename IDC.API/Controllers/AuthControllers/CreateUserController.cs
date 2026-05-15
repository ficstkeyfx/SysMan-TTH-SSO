using System.Text;
using System.Text.Json;
using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.SsoKeyCloak
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateUserController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        // ✅ Thêm constructor injection (giống AuthKeyCloakController)
        public CreateUserController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            var userExists = await _keycloakRepo.CheckUserExists(request.Username);
            if (userExists)
            {
                return Conflict(new { Message = "User with this username already exists" });
            }

            var result = await _keycloakRepo.CreateUser(request);

            if (result.Success)
            {
                return CreatedAtAction(nameof(CreateUser), new { id = result.UserId }, result);
            }

            return BadRequest(result);
        }

        [HttpGet("check/{username}")]
        public async Task<IActionResult> CheckUserExists(string username)
        {
            var exists = await _keycloakRepo.CheckUserExists(username);
            return Ok(new { Username = username, Exists = exists });
        }
    }
}