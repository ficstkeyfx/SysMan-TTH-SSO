using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/vaitro")]
    public class VaiTroController : ApiController<tbVaiTro>
    {
        public VaiTroController(IApiService<tbVaiTro> repository) : base(repository)
        {
        }
    }

}