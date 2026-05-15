using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/sessionlogin")]
    public class SessionLoginController : ApiController<tbSessionLogin>
    {
        public SessionLoginController(IApiService<tbSessionLogin> repository) : base(repository)
        {
        }
    }

}