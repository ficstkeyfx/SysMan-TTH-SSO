using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeleteUserController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public DeleteUserController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            try
            {
                // 1. Kiểm tra user có tồn tại không
                var userExists = await _keycloakRepo.CheckUserExists(request.Username);
                if (!userExists)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{request.Username}'" });

                // 2. Lấy Admin Token
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                if (adminToken == null)
                    return StatusCode(500, new { message = "Không thể lấy quyền quản trị" });

                // 3. Lấy userId từ username
                var userId = await _keycloakRepo.GetUserId(request.Username, adminToken);
                if (string.IsNullOrEmpty(userId))
                    return NotFound(new { message = "Không tìm thấy định danh tài khoản" });

                // 4. Xoá user
                var success = await _keycloakRepo.DeleteUser(userId);
                if (success)
                    return Ok(new { message = $"Xoá tài khoản '{request.Username}' thành công" });

                return StatusCode(500, new { message = "Xoá tài khoản không thành công, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}