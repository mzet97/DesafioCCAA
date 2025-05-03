using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Web.Controllers;

[Authorize]
public class BooksController(
    ILogger<BooksController> logger,
    IMediator mediator) : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
