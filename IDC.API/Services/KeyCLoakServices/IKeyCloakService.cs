using api.SsoKeyCloak;

namespace api.Services.KeyCloakServices
{
    public interface IKeyCloakService
    {

        // Phương thức lấy tên người dùng
        string? GetUsernameFromToken(string token);

        // Phương thức lấy thông tin token của một người dùng
        Task<TokenResponse?> GetUserTokenInfo(string username, string password);

        // Phương thức xóa cache realm để cập nhật thông tin người dùng mới nhất
        Task ClearRealmCache(string adminToken);

        // Phương thức lấy Access Token của người dùng
        Task<string> GetUserAccessToken(string username, string password);

        // Phương thức lấy Access token của quản trị hệ thống
        Task<string> GetAdminAccessToken();

        // Phương thức lấy định danh người dùng
        Task<string> GetUserId(string username, string adminToken);

        // Lấy thông tin user theo username
        Task<KeycloakUserResponse?> GetUserByUsername(string username);


        // Lấy thông tin user theo username
        Task<List<UserDto>> GetUsers();

        // Phương thức kiểm tra xem một người dùng có trong CSDL không
        Task<bool> UsernameExistsInDatabase(string username);

        // Phương thức kiểm tra sự tồn tại của người dùng
        Task<bool> CheckUserExists(string username);

        // Phương thức tạo người dùng mới                               
        Task<CreateUserResponse> CreateUser(CreateUserRequest newUser);

        // Phương thức đổi mật khẩu của người dùng
        Task<bool> ChangeUserPassword(string userId, string newPassword, string adminToken);


        // Phương thức xoá người dùng
        Task<bool> DeleteUser(string userId);
        
        // Hàm disable user
        Task<bool> DisableUser(string userId);
        // Hàm enable user
        Task<bool> EnableUser(string userId);
        Task<bool> ValidateUserInfo(ResetPasswordRequest request);
        Task<bool> ResetPassword(string username, string newPassword);
        // Phương thức cập nhật thông tin người dùng
        Task<bool> UpdateUser(string userId, string email, string firstName, string lastName, string adminToken);
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenResponse
    {
        public string Access_token { get; set; }
        public string Refresh_token { get; set; }
        public int Expires_in { get; set; }
        public string Token_type { get; set; }
    }

    public class KeycloakUserInfo
    {
        public string Id        { get; set; }
        public string Username  { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Email     { get; set; }
        public bool   Enabled   { get; set; }  // false = đang bị khóa
    }

    public class ChangePasswordWithUserModel
    {
        public string Username { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class KeycloakUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }

    public class UsernameModel
    {
        public string Username { get; set; }
    }
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }

    // Keycloak API Models
    public class KeycloakUserModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public bool EmailVerified { get; set; } = false;
        public List<KeycloakCredential> Credentials { get; set; } = new();
    }
    public class ChangeUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class KeycloakCredential
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Temporary { get; set; } = false;
    }

    public class KeycloakUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool EmailVerified { get; set; }
    }

    public class DeleteUserRequest
    {
        public string Username { get; set; } = string.Empty;
        // public string Email { get; set; } = string.Empty;
        // public string FirstName { get; set; } = string.Empty;
        // public string LastName { get; set; } = string.Empty;
    }

    public class DisableUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    public class EnableUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }   // thêm email
        public bool   Enabled  { get; set; }


    }
    public class ResetPasswordRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }  // 🔹 Nhận mật khẩu từ client
    }

    public class KeycloakUserReset
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
}
