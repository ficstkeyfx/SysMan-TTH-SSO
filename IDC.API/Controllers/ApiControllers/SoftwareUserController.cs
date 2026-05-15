using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/softwareuser")]
    public class SoftwareUserController : ApiController<tbSoftwareUser>
    {
        public SoftwareUserController(IApiService<tbSoftwareUser> repository) : base(repository)
        {
        }
    }

}