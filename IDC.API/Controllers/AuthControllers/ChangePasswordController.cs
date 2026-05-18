using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public ChangePasswordController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordWithUserModel model)
        {
            try
            {
                // 1. Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (model.NewPassword != model.ConfirmPassword)
                    return BadRequest(new { message = "Mật khẩu mới không khớp với mật khẩu xác nhận" });

                // 2. Xác thực thông tin đăng nhập hiện tại của người dùng
                var userToken = await _keycloakRepo.GetUserAccessToken(model.Username, model.CurrentPassword);
                if (userToken == null)
                    return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu hiện tại không đúng" });

                // 3. Lấy Admin Token
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                if (adminToken == null)
                    return StatusCode(500, new { message = "Không thể lấy quyền quản trị" });

                // 4. Lấy userId từ username
                var userId = await _keycloakRepo.GetUserId(model.Username, adminToken);
                if (string.IsNullOrEmpty(userId))
                    return NotFound(new { message = "Không tìm thấy tài khoản" });

                // 5. Đổi mật khẩu
                var success = await _keycloakRepo.ChangeUserPassword(userId, model.NewPassword, adminToken);
                if (success)
                    return Ok(new { message = "Đổi mật khẩu thành công" });

                return StatusCode(500, new { message = "Đổi mật khẩu không thành công, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}