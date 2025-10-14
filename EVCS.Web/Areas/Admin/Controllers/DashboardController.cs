using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();
    }
}
