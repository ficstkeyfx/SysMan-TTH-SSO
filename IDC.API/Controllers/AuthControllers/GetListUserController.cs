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

                // Map sang DTO có thêm isLocked
                var data = users.Select(u => new
                {
                    u.Username,
                    u.Name,
                    u.Email,
                    u.Enabled,
                    IsLocked = !u.Enabled   // enabled=false → isLocked=true
                });

                return Ok(new { message = "Lấy danh sách thành công", total = users.Count, data });
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

                return Ok(new
                {
                    message = "Lấy thông tin thành công",
                    data = new
                    {
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.Enabled,
                        IsLocked = !user.Enabled
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
