using IDC.Shared.Models;
using IDC.Shared.Models.SysMan;
using SysMan.Models;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

namespace SysMan.Services.LDapServices
{
    public class LdapUserService
    {
        private readonly string _domain = "C500.edu.vn";
        private readonly string _ouDistinguishedName = "OU=c500,DC=c500,DC=edu,DC=vn"; // Thay đổi theo tổ chức của bạn
        
        // Tên người dùng và tài khoản đăng nhập vào LDAP
        private readonly string _adminUsername = "nghiand";
        private readonly string _adminPassword = "@Abc123";

        /// <summary>
        /// Hàm thảo một tài khoản người dùng trên LDAP
        /// </summary>
        /// <param name="username">Tên người dùng</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="displayName">Tên hiển thị</param>
        /// <param name="email">Địa chỉ thư điện tử</param>
        /// <exception cref="Exception"></exception>
        public bool CreateUser(string username, string password, 
            string displayName, string email)// ví dụ: "OU=CNTT,DC=c500,DC=edu,DC=vn"
        {
            using (var context = new PrincipalContext(ContextType.Domain, _domain, _ouDistinguishedName, _adminUsername, _adminPassword))
            {
                if (CheckExistUser(username))
                {
                    return false;
                }

                using (var user = new UserPrincipal(context))
                {
                    user.SamAccountName = username;
                    user.UserPrincipalName = $"{username}@{_domain}";
                    user.DisplayName = displayName;
                    user.EmailAddress = email;
                    //user.ExpirePasswordNow(); // Bắt buộc đổi mật khẩu khi đăng nhập nếu cần

                    try
                    {
                        user.Save();
                        // 2) ĐẶT MẬT KHẨU (yêu cầu đã Save ở bước 1)
                        user.SetPassword(password);
                        // 3) BẬT TÀI KHOẢN
                        user.Enabled = true;
                        // 5) Lưu lần cuối
                        user.Save();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                        throw new Exception("Không thể tạo tài khoản LDAP: " + ex.Message, ex);
                    }
                }
            }
        }

        public List<vNguoiDungHeThong> getAllLDAPUsers()
        {
            List<vNguoiDungHeThong> lstUser = new List<vNguoiDungHeThong>();
            vNguoiDungHeThong usr;
            using (var Context = new PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Domain, "C500.edu.vn", "nghiand", "@Abc123"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(Context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        if (de.Properties["sAMAccountName"].Value != null)
                        {
                            usr = new vNguoiDungHeThong();
                            usr.UserName = de.Properties["sAMAccountName"].Value.ToString();

                            // if (de.Properties["userPrincipalName"].Value!=null)
                            //     usr.FullName = de.Properties["userPrincipalName"].Value.ToString();//email

                            usr.FullName = de.Properties["cn"].Value.ToString();
                            lstUser.Add(usr);
                        }
                    }
                }
            };
            return lstUser;
        }

        public bool CheckExistUser(string username)
        {
            // Tạo context kết nối đến domain
            using (var context = new PrincipalContext(ContextType.Domain, "C500.edu.vn", "nghiand", "@Abc123"))
            {
                // Tìm user theo sAMAccountName (hoặc UPN, hoặc email)
                string usernameToCheck = username;

                using (var user = UserPrincipal.FindByIdentity(context, usernameToCheck))
                {
                    if (user != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Phương thức xóa một người dùng trên LDAP
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>true: xóa được; flase: không khóa được</returns>
        public bool DeleteUser(string username)
        {
            // Tạo context kết nối đến domain
            using (var context = new PrincipalContext(ContextType.Domain, "C500.edu.vn", "C500.edu.vn", "nghiand", "@Abc123"))
            {
                // Tìm user theo sAMAccountName (hoặc UPN, hoặc email)
                string usernameToCheck = username;

                using (var user = UserPrincipal.FindByIdentity(context, usernameToCheck))
                {
                    if (user != null)
                    {
                        user.Delete();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }



        public bool ResetUserPassword(string username)
        {
            // Tạo context kết nối đến domain
            using (var context = new PrincipalContext(ContextType.Domain, "C500.edu.vn", "C500.edu.vn", "nghiand", "@Abc123"))
            {
                // Tìm user theo sAMAccountName (hoặc UPN, hoặc email)
                string usernameToCheck = username;
                string newPassword = "@Abc123"; // Mật khẩu mới
                using (var user = UserPrincipal.FindByIdentity(context, usernameToCheck))
                {
                    if (user != null)
                    {
                        user.SetPassword(newPassword);   // Reset mật khẩu
                        user.Save();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public bool ChangeUserPassword(string username, string password)
        {
            // Tạo context kết nối đến domain
            using (var context = new PrincipalContext(ContextType.Domain, "C500.edu.vn", "C500.edu.vn", "nghiand", "@Abc123"))
            {
                // Tìm user theo sAMAccountName (hoặc UPN, hoặc email)
                string usernameToCheck = username;
                string newPassword = "@Abc123"; // Mật khẩu mới
                using (var user = UserPrincipal.FindByIdentity(context, usernameToCheck))
                {
                    if (user != null)
                    {
                        user.ChangePassword(password, newPassword);
                        user.Save();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
