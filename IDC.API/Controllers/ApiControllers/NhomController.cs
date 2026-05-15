using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/nhom")]
    public class NhomController : ApiController<tbNhom>
    {
        public NhomController(IApiService<tbNhom> repository) : base(repository)
        {
        }
    }

}