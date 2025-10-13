using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
