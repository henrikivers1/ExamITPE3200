using Microsoft.AspNetCore.Mvc;

namespace ExamITPE3200.Controllers;

// Controller only render the view as per 23.09, logic for database will be added later 

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }
}