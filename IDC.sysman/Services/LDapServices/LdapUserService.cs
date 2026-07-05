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
        private readonly string _domain;
        private readonly string _ouDistinguishedName;
        private readonly string _adminUsername;
        private readonly string _adminPassword;
        private readonly string _defaultPassword;

        public LdapUserService(IConfiguration configuration)
        {
            _domain = configuration["Ldap:Domain"] ?? string.Empty;
            _ouDistinguishedName = configuration["Ldap:OuDistinguishedName"] ?? string.Empty;
            _adminUsername = configuration["Ldap:AdminUsername"] ?? string.Empty;
            _adminPassword = configuration["Ldap:AdminPassword"] ?? string.Empty;
            _defaultPassword = configuration["Ldap:DefaultPassword"] ?? string.Empty;
        }

        /// <summary>
        /// Hàm thảo một tài khoản người dùng trên LDAP
        /// </summary>
        /// <param name="username">Tên người dùng</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="displayName">Tên hiển thị</param>
        /// <param name="email">Địa chỉ thư điện tử</param>
        /// <exception cref="Exception"></exception>
        public bool CreateUser(string username, string password,
            string displayName, string email)
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

                    try
                    {
                        user.Save();
                        user.SetPassword(password);
                        user.Enabled = true;
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
            using (var Context = new PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Domain, _domain, _adminUsername, _adminPassword))
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
            using (var context = new PrincipalContext(ContextType.Domain, _domain, _adminUsername, _adminPassword))
            {
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
            using (var context = new PrincipalContext(ContextType.Domain, _domain, _domain, _adminUsername, _adminPassword))
            {
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
            using (var context = new PrincipalContext(ContextType.Domain, _domain, _domain, _adminUsername, _adminPassword))
            {
                string usernameToCheck = username;
                string newPassword = _defaultPassword;
                using (var user = UserPrincipal.FindByIdentity(context, usernameToCheck))
                {
                    if (user != null)
                    {
                        user.SetPassword(newPassword);
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
            using (var context = new PrincipalContext(ContextType.Domain, _domain, _domain, _adminUsername, _adminPassword))
            {
                string usernameToCheck = username;
                string newPassword = _defaultPassword;
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