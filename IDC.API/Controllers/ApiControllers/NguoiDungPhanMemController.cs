using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/nguoidungphanmem")]
    public class NguoiDungPhanMemController : ApiController<vNguoiDungPhanMem>
    {
        public NguoiDungPhanMemController(IApiService<vNguoiDungPhanMem> repository) : base(repository)
        {
        }
    }

}