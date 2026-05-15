using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers
{
    [Route("api/chucnang")]
    public class ChucNangController : ApiController<tbChucNang>
    {
        public ChucNangController(IApiService<tbChucNang> repository) : base(repository)
        {
        }
    }

}