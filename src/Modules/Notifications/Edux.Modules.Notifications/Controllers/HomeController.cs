using Microsoft.AspNetCore.Mvc;

namespace Edux.Modules.Notifications.Controllers
{
    [Route(NotificationsModule.BasePath)]
    internal sealed class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Get()
            => Ok("Edux Notifications Module");
    }
}
