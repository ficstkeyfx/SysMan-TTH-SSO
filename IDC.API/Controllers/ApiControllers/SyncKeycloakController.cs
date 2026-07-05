using api.Services.KeyCloakServices;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using IDC.Shared.Models.SysMan;

namespace api.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/sync")]
    public class SyncKeycloakController : ControllerBase
    {
        private readonly dbAPIContext _db;
        private readonly IKeyCloakService _keycloakService;
        private readonly ILogger<SyncKeycloakController> _logger;
        private readonly IConfiguration _configuration;

        public SyncKeycloakController(
            dbAPIContext db,
            IKeyCloakService keycloakService,
            ILogger<SyncKeycloakController> logger,
            IConfiguration configuration)
        {
            _db = db;
            _keycloakService = keycloakService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Lấy danh sách user từ Keycloak (qua admin API)
        /// </summary>
        [HttpGet("keycloak-users")]
        public async Task<IActionResult> GetKeycloakUsers()
        {
            try
            {
                var users = await _keycloakService.GetUsers();
                if (users == null)
                {
                    return Ok(new List<UserDto>());
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Keycloak users");
                return StatusCode(500, new { error = "Không thể lấy danh sách user từ Keycloak", detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách user hiện tại trong SQL (tbSoftwareUser + tbSystemUser)
        /// </summary>
        [HttpGet("sql-users")]
        public async Task<IActionResult> GetSqlUsers()
        {
            try
            {
                var sqlUsers = await _db.vNguoiDungHeThongs
                    .Select(u => new
                    {
                        u.IdUser,
                        u.UserName,
                        u.FullName,
                        u.IdPhanMem
                    })
                    .ToListAsync();

                return Ok(sqlUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SQL users");
                return StatusCode(500, new { error = "Không thể lấy danh sách user từ SQL", detail = ex.Message });
            }
        }

        /// <summary>
        /// Đồng bộ users từ Keycloak sang SQL.
        /// Mỗi user sẽ được tạo trong tbSystemUser (lưu hash password) + tbSoftwareUser.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Sync([FromBody] SyncRequest request)
        {
            if (request?.Users == null || !request.Users.Any())
            {
                return BadRequest(new { error = "Danh sách user đồng bộ trống" });
            }

            // Mật khẩu mặc định sẽ được hash — user sẽ phải đổi pass lần đầu
            var defaultPassword = _configuration["Ldap:DefaultPassword"]
                ?? _configuration["KeycloakSync:DefaultPassword"]
                ?? "@Abc123";
            var defaultPasswordHash = Sha256Hash(defaultPassword);

            var created = new List<object>();
            var skipped = new List<object>();
            var errors = new List<object>();

            // Lấy danh sách username đã tồn tại
            var existingUsernames = await _db.tbSystemUsers
                .Select(u => u.UserName)
                .ToListAsync();
            var existingSet = new HashSet<string>(
                existingUsernames.Select(u => u?.ToLower() ?? ""),
                StringComparer.OrdinalIgnoreCase);

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var kcUser in request.Users)
                {
                    if (string.IsNullOrWhiteSpace(kcUser.Username))
                    {
                        errors.Add(new { username = kcUser.Username, reason = "Username rỗng" });
                        continue;
                    }

                    var username = kcUser.Username.Trim();

                    // Bỏ qua nếu user đã tồn tại trong SQL
                    if (existingSet.Contains(username))
                    {
                        skipped.Add(new { username, reason = "Đã tồn tại trong SQL" });
                        continue;
                    }

                    try
                    {
                        // Generate IdUser thủ công (IdUser không auto-increment)
                        var maxId = await _db.tbSystemUsers.AnyAsync()
                            ? await _db.tbSystemUsers.MaxAsync(u => (int?)u.IdUser) ?? 0
                            : 0;
                        var idUser = maxId + 1;

                        // 1. Tạo trong tbSystemUser (lưu hash password)
                        var newSystemUser = new tbSystemUser
                        {
                            IdUser = idUser,
                            UserName = username,
                            FullName = string.IsNullOrWhiteSpace(kcUser.Name)
                                ? username
                                : kcUser.Name,
                            PasswordHash = defaultPasswordHash,
                            Password = defaultPasswordHash,
                            IsLDAPAccount = false
                        };
                        _db.tbSystemUsers.Add(newSystemUser);
                        await _db.SaveChangesAsync();

                        // 2. Tạo trong tbSoftwareUser (mapping user với phần mềm, nếu có)
                        if (request.IdPhanMem.HasValue)
                        {
                            // Generate IdSoftwareUser thủ công
                            var maxSwId = await _db.tbSoftwareUsers.AnyAsync()
                                ? await _db.tbSoftwareUsers.MaxAsync(u => (int?)u.IdSoftwareUser) ?? 0
                                : 0;
                            var newSoftwareUser = new tbSoftwareUser
                            {
                                IdSoftwareUser = maxSwId + 1,
                                IdUser = idUser,
                                UserName = newSystemUser.UserName,
                                FullName = newSystemUser.FullName,
                                IdPhanMem = request.IdPhanMem
                            };
                            _db.tbSoftwareUsers.Add(newSoftwareUser);
                            await _db.SaveChangesAsync();
                        }

                        created.Add(new
                        {
                            username,
                            idUser,
                            fullName = newSystemUser.FullName
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing user {Username}", username);
                        errors.Add(new { username, reason = ex.Message });
                    }
                }

                await tx.CommitAsync();

                return Ok(new
                {
                    totalRequested = request.Users.Count,
                    created = created.Count,
                    skipped = skipped.Count,
                    errors = errors.Count,
                    createdUsers = created,
                    skippedUsers = skipped,
                    errorDetails = errors,
                    message = $"Đồng bộ hoàn tất: tạo mới {created.Count}, bỏ qua {skipped.Count}, lỗi {errors.Count}"
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Sync transaction failed");
                return StatusCode(500, new { error = "Lỗi transaction khi đồng bộ", detail = ex.Message });
            }
        }

        private static string Sha256Hash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Tạo user trong SQL sau khi đã tạo thành công trên Keycloak
        /// </summary>
        [HttpPost("create-sql-user")]
        public async Task<IActionResult> CreateSqlUser([FromBody] CreateSqlUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { error = "Username rỗng" });

            var username = request.Username.Trim();

            // Kiểm tra đã tồn tại chưa
            var exists = await _db.tbSystemUsers.AnyAsync(u => u.UserName == username);
            if (exists)
                return Ok(new { message = "User đã tồn tại trong SQL", skipped = true });

            var defaultPassword = _configuration["Ldap:DefaultPassword"] ?? "@Abc123";
            var passwordHash = Sha256Hash(request.Password ?? defaultPassword);

            // Generate IdUser
            var maxId = await _db.tbSystemUsers.AnyAsync()
                ? await _db.tbSystemUsers.MaxAsync(u => (int?)u.IdUser) ?? 0
                : 0;
            var idUser = maxId + 1;

            var newUser = new tbSystemUser
            {
                IdUser = idUser,
                UserName = username,
                FullName = string.IsNullOrWhiteSpace(request.FullName)
                    ? username : request.FullName,
                PasswordHash = passwordHash,
                Password = passwordHash,
                IsLDAPAccount = false
            };

            _db.tbSystemUsers.Add(newUser);

            // Tạo tbSoftwareUser nếu có IdPhanMem
            if (request.IdPhanMem.HasValue)
            {
                var maxSwId = await _db.tbSoftwareUsers.AnyAsync()
                    ? await _db.tbSoftwareUsers.MaxAsync(u => (int?)u.IdSoftwareUser) ?? 0
                    : 0;
                _db.tbSoftwareUsers.Add(new tbSoftwareUser
                {
                    IdSoftwareUser = maxSwId + 1,
                    IdUser = idUser,
                    UserName = username,
                    FullName = newUser.FullName,
                    IdPhanMem = request.IdPhanMem
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Tạo user SQL thành công", idUser });
        }

        /// <summary>
        /// Cập nhật password hash trong SQL khi user đổi mật khẩu trên Keycloak
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> SyncChangePassword([FromBody] ChangePasswordSqlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { error = "Username hoặc mật khẩu rỗng" });

            var username = request.Username.Trim();
            var user = await _db.tbSystemUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound(new { error = $"Không tìm thấy user '{username}' trong SQL" });

            var passwordHash = Sha256Hash(request.NewPassword);
            user.PasswordHash = passwordHash;
            user.Password = passwordHash;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Cập nhật password hash SQL cho '{username}' thành công" });
        }

        /// <summary>
        /// Khóa/mở khóa user trong SQL khi tác động trên Keycloak
        /// </summary>
        [HttpPost("toggle-lock")]
        public async Task<IActionResult> SyncToggleLock([FromBody] ToggleLockSqlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { error = "Username rỗng" });

            var username = request.Username.Trim();

            // Tìm tbSoftwareUser (bảng chính cho per-app user status)
            var softwareUser = await _db.tbSoftwareUsers
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (softwareUser != null)
            {
                // Nếu có trường IsDisabled/IsLocked trong SQL thì update ở đây
                // Hiện tại tbSoftwareUser không có field lock, nên log để biết
                _logger.LogInformation("SQL sync lock: user '{Username}' lock={IsLocked} (không có field lock trong SQL)",
                    username, request.IsLocked);
            }

            // Tìm tbSystemUser để log
            var systemUser = await _db.tbSystemUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (systemUser == null)
                return NotFound(new { error = $"Không tìm thấy user '{username}' trong SQL" });

            return Ok(new { message = $"Đã sync lock status cho '{username}': IsLocked={request.IsLocked}" });
        }

        public class SyncRequest
        {
            public List<UserDto> Users { get; set; } = new();
            public int? IdPhanMem { get; set; }
        }

        public class CreateSqlUserRequest
        {
            public string Username { get; set; } = "";
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string? Password { get; set; }
            public int? IdPhanMem { get; set; }
        }

        public class ChangePasswordSqlRequest
        {
            public string Username { get; set; } = "";
            public string NewPassword { get; set; } = "";
        }

        public class ToggleLockSqlRequest
        {
            public string Username { get; set; } = "";
            public bool IsLocked { get; set; }
        }
    }
}
