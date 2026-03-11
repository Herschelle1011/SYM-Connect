using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SYM_CONNECT.Models;
using System.Diagnostics;

namespace SYM_CONNECT.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult UserManagement()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel());
        }
    }
}
