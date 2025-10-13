using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
