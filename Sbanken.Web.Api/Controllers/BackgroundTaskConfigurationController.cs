using Hub.Storage.Factories;
using Hub.Storage.Providers;
using Hub.Web.ApiControllers;
using Microsoft.AspNetCore.Mvc;

namespace Sbanken.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackgroundTaskConfigurationController : BackgroundTaskConfigurationControllerBase
    {
        public BackgroundTaskConfigurationController(IBackgroundTaskConfigurationProvider backgroundTaskConfigurationProvider, 
            IBackgroundTaskConfigurationFactory backgroundTaskConfigurationFactory) : base(backgroundTaskConfigurationProvider, backgroundTaskConfigurationFactory)
        {
        }
    }
}