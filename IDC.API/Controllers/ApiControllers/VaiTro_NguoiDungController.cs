using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/vaitro_nguoidung")]
    public class VaiTro_NguoiDungController : ApiController<tbVaiTro_NguoiDung>
    {
        public VaiTro_NguoiDungController(IApiService<tbVaiTro_NguoiDung> repository) : base(repository)
        {
        }
    }

}