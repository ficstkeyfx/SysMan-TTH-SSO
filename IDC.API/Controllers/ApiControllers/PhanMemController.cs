using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/phanmem")]
    public class PhanMemController : ApiController<tbPhanMem>
    {
        public PhanMemController(IApiService<tbPhanMem> repository) : base(repository)
        {
        }
    }

}