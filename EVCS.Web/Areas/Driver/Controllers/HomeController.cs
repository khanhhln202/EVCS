using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Driver.Controllers
{
    [Area("Driver")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
