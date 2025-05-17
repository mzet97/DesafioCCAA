using SistemaLivro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SistemaLivro.Web.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
