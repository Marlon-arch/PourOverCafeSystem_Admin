using Microsoft.AspNetCore.Mvc;
using PourOverCafeSystem_Admin.Database;
using System.Linq;

namespace PourOverCafeSystem_Admin.Controllers
{
    public class LoginController : Controller
    {
        private readonly PourOverCoffeeDbContext _context;

        public LoginController(PourOverCoffeeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid username or password.";
            return View("Index");
        }


        [HttpGet]
        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Index");
        }
    }
}
