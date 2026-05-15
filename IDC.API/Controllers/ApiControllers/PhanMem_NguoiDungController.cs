using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/phanmem_nguoidung")]
    public class PhanMem_NguoiDungController : ApiController<tbPhanMem_NguoiDung>
    {
        public PhanMem_NguoiDungController(IApiService<tbPhanMem_NguoiDung> repository) : base(repository)
        {
        }
    }

}