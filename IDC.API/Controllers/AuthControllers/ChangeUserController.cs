using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChangeUserController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public ChangeUserController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUser([FromBody] ChangeUserRequest request)
        {
            try
            {
                // 1. Validate email bắt buộc
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email là bắt buộc" });

                // 2. Xác thực username + password
                var userToken = await _keycloakRepo.GetUserAccessToken(request.Username, request.Password);
                if (userToken == null)
                    return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });

                // 3. Lấy Admin Token
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                if (adminToken == null)
                    return StatusCode(500, new { message = "Không thể lấy quyền quản trị" });

                // 4. Lấy userId
                var userId = await _keycloakRepo.GetUserId(request.Username, adminToken);
                if (string.IsNullOrEmpty(userId))
                    return NotFound(new { message = "Không tìm thấy tài khoản" });

                // 5. Cập nhật thông tin
                var success = await _keycloakRepo.UpdateUser(userId, request.Email, request.FirstName, request.LastName, adminToken);
                if (success)
                    return Ok(new { message = "Cập nhật thông tin thành công" });

                return StatusCode(500, new { message = "Cập nhật thông tin không thành công, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}