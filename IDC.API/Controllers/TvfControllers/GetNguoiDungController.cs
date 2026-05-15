using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.ApiService;
using IDC.Backend.Services.TvfService;
using IDC.Shared.Models.SysMan;
namespace api.Controllers.TvfControllers
{
    [Route("api/tvf")]
    public class GetNguoiDungController : TvfNguoiDungController<tbSoftwareUser>
    {
        public GetNguoiDungController(ITvfService<tbSoftwareUser> repository) : base(repository)
        {
        }
    }

}