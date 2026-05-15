using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.AuthService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers.AuthControllers
{
    [Route("api/auth")]
    public class LoginController : AuthController<tbSoftwareUser>
    {
        public LoginController(IAuthService<tbSoftwareUser> repository) : base(repository)
        {
        }
    }

}