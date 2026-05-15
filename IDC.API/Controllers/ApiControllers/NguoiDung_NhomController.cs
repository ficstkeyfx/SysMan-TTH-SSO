using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/nguoidung_nhom")]
    public class NguoiDung_NhomController : ApiController<tbNguoiDung_Nhom>
    {
        public NguoiDung_NhomController(IApiService<tbNguoiDung_Nhom> repository) : base(repository)
        {
        }
    }

}