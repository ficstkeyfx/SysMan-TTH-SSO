using Microsoft.AspNetCore.Mvc;
using IDC.Backend.Servives.Controllers;
using IDC.Backend.Servives.NumService;
namespace api.Controllers.KeyControllers
{
    [Route("api/num")]
    public class KeyController : NumController
    {
        public KeyController(INumService repository) : base(repository)
        {
        }
    }

}