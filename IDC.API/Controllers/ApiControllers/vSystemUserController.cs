using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/vsystemuser")]
    public class vSystemUserController : ApiController<vSystemUser>
    {
        public vSystemUserController(IApiService<vSystemUser> repository) : base(repository)
        {
        }
    }

}