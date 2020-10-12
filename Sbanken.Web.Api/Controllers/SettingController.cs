using Hub.Storage.Factories;
using Hub.Storage.Providers;
using Hub.Web.ApiControllers;
using Microsoft.AspNetCore.Mvc;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : SettingControllerBase
    {

        public SettingController(ISettingProvider settingProvider, 
            ISettingFactory settingFactory) : base(settingProvider, settingFactory)
        {
        }
    }
}