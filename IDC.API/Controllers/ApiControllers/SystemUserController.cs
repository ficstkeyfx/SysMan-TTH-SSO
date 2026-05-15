using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/systemuser")]
    public class SystemUserController : ApiController<tbSystemUser>
    {
        public SystemUserController(IApiService<tbSystemUser> repository) : base(repository)
        {
        }
    }

}