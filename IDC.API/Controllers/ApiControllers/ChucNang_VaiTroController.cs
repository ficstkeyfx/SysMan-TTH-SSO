using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/chucnang_vaitro")]
    public class ChucNang_VaiTroController : ApiController<tbChucNang_VaiTro>
    {
        public ChucNang_VaiTroController(IApiService<tbChucNang_VaiTro> repository) : base(repository)
        {
        }
    }

}