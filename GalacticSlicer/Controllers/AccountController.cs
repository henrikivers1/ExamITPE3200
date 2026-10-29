using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.Services;

namespace GalacticSlicer.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly PlayerProgressService _progressService;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger, PlayerProgressService progressService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _progressService = progressService;
    }

    // GET: /Account/SignUp
    [HttpGet]
    public IActionResult SignUp() => View(new SignUpViewModel());

    // POST: /Account/SignUp
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser { UserName = model.Username };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _progressService.GetOrCreateProgressAsync(user.Id);

            _logger.LogInformation(
                "[AccountController] New user registered: {Username}",
                model.Username);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // Identity explains what was wrong (username taken, password too weak, ...)
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        _logger.LogWarning("[AccountController] Sign up failed for {Username}", model.Username);
        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("[AccountController] User logged in: {Username}", model.Username);

            // Only redirect to local URLs, never to another website
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("[AccountController] Failed login attempt for {Username}", model.Username);
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}