using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public ResetPasswordController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // 1. Kiểm tra tài khoản có tồn tại không
                var userExists = await _keycloakRepo.CheckUserExists(request.Username);
                if (!userExists)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{request.Username}'" });

                // 2. Lấy thông tin user để xác thực email
                var user = await _keycloakRepo.GetUserByUsername(request.Username);
                if (user == null)
                    return NotFound(new { message = "Không thể lấy thông tin tài khoản" });

                if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Email không khớp với tài khoản" });

                // 3. Reset mật khẩu
                var success = await _keycloakRepo.ResetPassword(request.Username, request.NewPassword);
                if (success)
                    return Ok(new { message = "Đặt lại mật khẩu thành công" });

                return StatusCode(500, new { message = "Đặt lại mật khẩu không thành công, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}