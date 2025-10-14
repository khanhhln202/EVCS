using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
