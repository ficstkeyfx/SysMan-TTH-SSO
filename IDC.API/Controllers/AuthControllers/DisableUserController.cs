using api.Services.KeyCloakServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LockAccountController : ControllerBase
    {
        private readonly IKeyCloakService _keycloakRepo;

        public LockAccountController(IKeyCloakService keycloakRepo)
        {
            _keycloakRepo = keycloakRepo;
        }

        // GET api/LockAccount/{username}
        // Kiểm tra trạng thái tài khoản: đang khóa hay không
        [HttpGet("{username}")]
        public async Task<IActionResult> GetStatus(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return BadRequest(new { message = "Tên đăng nhập không được để trống" });

                var user = await _keycloakRepo.GetUserByUsername(username);
                if (user == null)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{username}'" });

                return Ok(new
                {
                    username   = user.Username,
                    isLocked   = !user.Enabled,
                    status     = user.Enabled ? "Chưa khóa" : "Đã khóa",
                    canEnable  = !user.Enabled,   // đang khóa  → có thể mở khóa
                    canDisable = user.Enabled      // đang mở    → có thể khóa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST api/LockAccount/lock/{username}
        // Khóa tài khoản (Disable)
        [HttpPost("lock/{username}")]
        public async Task<IActionResult> Lock(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return BadRequest(new { message = "Tên đăng nhập không được để trống" });

                // Kiểm tra user tồn tại và trạng thái hiện tại
                var user = await _keycloakRepo.GetUserByUsername(username);
                if (user == null)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{username}'" });

                if (!user.Enabled)
                    return BadRequest(new { message = $"Tài khoản '{username}' đã bị khóa trước đó" });

                // Lấy userId rồi Disable
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                var userId     = await _keycloakRepo.GetUserId(username, adminToken);

                var success = await _keycloakRepo.DisableUser(userId);
                if (!success)
                    return StatusCode(500, new { message = "Khóa tài khoản không thành công, vui lòng thử lại" });

                return Ok(new
                {
                    message  = $"Khóa tài khoản '{username}' thành công",
                    username = username,
                    isLocked = true,
                    status   = "Đã khóa"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST api/LockAccount/unlock/{username}
        // Mở khóa tài khoản (Enable)
        [HttpPost("unlock/{username}")]
        public async Task<IActionResult> Unlock(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return BadRequest(new { message = "Tên đăng nhập không được để trống" });

                // Kiểm tra user tồn tại và trạng thái hiện tại
                var user = await _keycloakRepo.GetUserByUsername(username);
                if (user == null)
                    return NotFound(new { message = $"Không tìm thấy tài khoản '{username}'" });

                if (user.Enabled)
                    return BadRequest(new { message = $"Tài khoản '{username}' đang hoạt động bình thường" });

                // Lấy userId rồi Enable
                var adminToken = await _keycloakRepo.GetAdminAccessToken();
                var userId     = await _keycloakRepo.GetUserId(username, adminToken);

                var success = await _keycloakRepo.EnableUser(userId);
                if (!success)
                    return StatusCode(500, new { message = "Mở khóa tài khoản không thành công, vui lòng thử lại" });

                return Ok(new
                {
                    message  = $"Mở khóa tài khoản '{username}' thành công",
                    username = username,
                    isLocked = false,
                    status   = "Chưa khóa"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
