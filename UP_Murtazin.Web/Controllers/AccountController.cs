using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Extensions;
using UP_Murtazin.Web.Models;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class AccountController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private const string CaptchaAnswerKey = "CaptchaAnswer";
    private readonly SqlDataService _dataService;

    public AccountController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = BuildLoginModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        string expected = HttpContext.Session.GetString(CaptchaAnswerKey) ?? string.Empty;
        if (!ModelState.IsValid || !string.Equals(model.CaptchaAnswer.Trim(), expected, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials or captcha answer.");
            model.CaptchaExpression = HttpContext.Session.GetString("CaptchaExpression") ?? string.Empty;
            return View(model);
        }

        var user = await _dataService.AuthenticateAsync(model.Email, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            model = BuildLoginModel();
            return View(model);
        }

        HttpContext.Session.SetJson(UserSessionKey, user);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(UserSessionKey);
        return RedirectToAction(nameof(Login));
    }

    private LoginViewModel BuildLoginModel()
    {
        var random = new Random();
        int a = random.Next(1, 20);
        int b = random.Next(1, 20);
        string expression = $"{a} + {b}";
        string answer = (a + b).ToString();
        HttpContext.Session.SetString("CaptchaExpression", expression);
        HttpContext.Session.SetString(CaptchaAnswerKey, answer);
        return new LoginViewModel { CaptchaExpression = expression };
    }
}
