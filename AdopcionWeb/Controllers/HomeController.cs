using AdopcionWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdopcionWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Animales()
        {
            return View();
        }

        public IActionResult Proceso()
        {
            return View();
        }

        public IActionResult Donaciones()
        {
            return View();
        }

        public IActionResult Preguntas()
        {
            return View();
        }

        public IActionResult Sesion()
        {
            return View();
        }

        // Página de administración: solo para el rol Admin
        public IActionResult Admin()
        {
            if (HttpContext.Session.GetString("Rol") != "Admin")
            {
                return RedirectToAction("Sesion", new { requerido = 1 });
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
