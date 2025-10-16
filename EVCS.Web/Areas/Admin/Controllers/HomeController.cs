using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class HomeController : Controller
    {
        // GET: /Admin
        public IActionResult Index()
        {
            // Redirect Admin root to the Stations list (change if needed)
            return RedirectToAction("Index", "Stations", new { area = "Admin" });
        }
    }
}