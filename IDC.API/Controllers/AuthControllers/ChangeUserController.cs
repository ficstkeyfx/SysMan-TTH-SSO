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
                if (string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest(new { message = "Tên đăng nhập là bắt buộc" });

                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Email là bắt buộc" });

                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                if (adminToken == null)
                    return StatusCode(500, new { message = "Không thể lấy quyền quản trị" });

                var userId = await _keycloakRepo.GetUserId(request.Username, adminToken);
                if (string.IsNullOrEmpty(userId))
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{request.Username}'" });

                // ✅ Check email trùng với user KHÁC (không phải chính user đang edit)
                var users = await _keycloakRepo.GetUsers();
                var emailExists = users?.Any(u =>
                    u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
                    !u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)
                ) ?? false;

                if (emailExists)
                    return Conflict(new { field = "email", message = $"Email '{request.Email}' đã được sử dụng bởi tài khoản khác" });

                var success = await _keycloakRepo.UpdateUser(
                    userId, request.Email, request.FirstName, request.LastName, adminToken);

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