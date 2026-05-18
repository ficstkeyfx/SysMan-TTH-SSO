using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetListUserController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public GetListUserController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetListUser()
        {
            try
            {
                var users = await _keycloakRepo.GetUsers();
                if (users == null)
                    return StatusCode(500, new { message = "Không thể lấy danh sách người dùng" });

                if (!users.Any())
                    return Ok(new { message = "Không có người dùng nào", data = users });

                return Ok(new { message = "Lấy danh sách thành công", total = users.Count, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _keycloakRepo.GetUserByUsername(username);
                if (user == null)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{username}'" });

                return Ok(new { message = "Lấy thông tin thành công", data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}