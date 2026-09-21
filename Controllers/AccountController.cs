using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;
using UTHMLibrary.Models;
using UTHMLibrary.Services;
using UTHMLibrary.ViewModels;

namespace UTHMLibrary.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ApplicationDbContext db,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    // ============================================
    // LOGIN
    // ============================================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Role == "Student");
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        await SignInUser(user, model.RememberMe);
        return Redirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    // ============================================
    // ADMIN LOGIN
    // ============================================

    [HttpGet]
    public IActionResult AdminLogin() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Role == "Admin");
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid admin credentials.");
            return View(model);
        }

        await SignInUser(user, false);
        return RedirectToAction("Index", "Admin");
    }

    // ============================================
    // REGISTER (STEP 1)
    // ============================================

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Check if email already exists
        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email already registered.");
            return View(model);
        }

        // Check if matric number already exists
        if (await _db.Users.AnyAsync(u => u.MatricNumber == model.MatricNumber))
        {
            ModelState.AddModelError("MatricNumber", "Matric number already registered.");
            return View(model);
        }

        // Generate verification code
        var code = GenerateVerificationCode();

        // Store registration data temporarily
        var registerDataJson = JsonSerializer.Serialize(model);
        HttpContext.Session.SetString("RegisterData", registerDataJson);
        HttpContext.Session.SetString("VerificationCode", code);
        HttpContext.Session.SetString("VerificationEmail", model.Email);

        // Send verification email
        var sent = await _emailService.SendVerificationCodeAsync(model.Email, code, model.FullName);

        if (!sent)
        {
            ModelState.AddModelError("", "Failed to send verification email. Please try again.");
            return View(model);
        }

        TempData["Success"] = "📧 Verification code sent to your email. Please check your inbox.";
        return RedirectToAction("VerifyEmail");
    }

    // ============================================
    // VERIFY EMAIL (STEP 2)
    // ============================================

    [HttpGet]
    public IActionResult VerifyEmail()
    {
        var email = HttpContext.Session.GetString("VerificationEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Register");

        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Email = model.Email;
            return View(model);
        }

        var storedEmail = HttpContext.Session.GetString("VerificationEmail");
        var storedCode = HttpContext.Session.GetString("VerificationCode");
        var registerDataJson = HttpContext.Session.GetString("RegisterData");

        if (string.IsNullOrEmpty(storedEmail) || string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(registerDataJson))
        {
            TempData["Error"] = "Verification session expired. Please register again.";
            return RedirectToAction("Register");
        }

        if (model.Email != storedEmail)
        {
            ModelState.AddModelError("", "Email mismatch.");
            ViewBag.Email = model.Email;
            return View(model);
        }

        if (model.Code != storedCode)
        {
            ModelState.AddModelError("Code", "Invalid verification code. Please try again.");
            ViewBag.Email = model.Email;
            return View(model);
        }

        // Get registration data from session
        var registerData = JsonSerializer.Deserialize<RegisterViewModel>(registerDataJson);

        // Create user
        var user = new User
        {
            FullName = registerData.FullName,
            Email = registerData.Email,
            MatricNumber = registerData.MatricNumber.ToUpper(),
            Faculty = registerData.Faculty,
            Phone = registerData.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Clear session data
        HttpContext.Session.Remove("RegisterData");
        HttpContext.Session.Remove("VerificationCode");
        HttpContext.Session.Remove("VerificationEmail");

        // Sign in user
        await SignInUser(user, false);

        TempData["Success"] = "✅ Registration successful! Welcome to UTHM Library.";
        return RedirectToAction("Index", "Home");
    }

    // ============================================
    // RESEND VERIFICATION CODE
    // ============================================

    [HttpPost]
    public async Task<JsonResult> ResendVerification([FromBody] ResendVerificationViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Email))
        {
            return Json(new { success = false, message = "Invalid email." });
        }

        var registerDataJson = HttpContext.Session.GetString("RegisterData");
        if (string.IsNullOrEmpty(registerDataJson))
        {
            return Json(new { success = false, message = "Registration session expired. Please register again." });
        }

        var registerData = JsonSerializer.Deserialize<RegisterViewModel>(registerDataJson);

        if (registerData.Email != model.Email)
        {
            return Json(new { success = false, message = "Email mismatch." });
        }

        var code = GenerateVerificationCode();
        HttpContext.Session.SetString("VerificationCode", code);

        var sent = await _emailService.SendVerificationCodeAsync(model.Email, code, registerData.FullName);

        if (sent)
        {
            return Json(new { success = true, message = "New verification code sent!" });
        }

        return Json(new { success = false, message = "Failed to send code. Please try again." });
    }

    // ============================================
    // LOGOUT
    // ============================================

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ============================================
    // ACCESS DENIED
    // ============================================

    public IActionResult AccessDenied() => View();

    // ============================================
    // HELPER METHODS
    // ============================================

    private async Task SignInUser(User user, bool remember)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("MatricNumber", user.MatricNumber)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties { IsPersistent = remember };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}