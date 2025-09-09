using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models;

namespace SistemaVetIng.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
     
    }
}
