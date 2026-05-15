using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/nguoidunghethong")]
    public class NguoiDungHeThongController : ApiController<vNguoiDungHeThong>
    {
        public NguoiDungHeThongController(IApiService<vNguoiDungHeThong> repository) : base(repository)
        {
        }
    }

}