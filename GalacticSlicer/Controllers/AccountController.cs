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
    private readonly IPlayerProgressService _progressService;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger, IPlayerProgressService progressService)
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

        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Sign up crashed for {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Could not create your account right now. Please try again.");
        }

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

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("[AccountController] User logged in: {Username}", model.Username);

                // Only redirect to local URLs, never to another website
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Too many wrong passwords in a row (rules in Program.cs)
            if (result.IsLockedOut)
            {
                _logger.LogWarning("[AccountController] Locked out after failed logins: {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Too many failed attempts. Please wait 5 minutes and try again.");
                return View(model);
            }

            _logger.LogWarning("[AccountController] Failed login attempt for {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Login crashed for {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Could not log you in right now. Please try again.");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name;

        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("[AccountController] User logged out: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AccountController] Logout failed for {Username}", username);
        }

        return RedirectToAction("Index", "Home");
    }
}