using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/chucnang_nhom")]
    public class ChucNang_NhomController : ApiController<tbChucNang_Nhom>
    {
        public ChucNang_NhomController(IApiService<tbChucNang_Nhom> repository) : base(repository)
        {
        }
    }

}