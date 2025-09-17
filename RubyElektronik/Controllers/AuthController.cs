using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RubyElektronik.Models;
using RubyElektronik.Services;
using System.ComponentModel.DataAnnotations;

namespace RubyElektronik.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        // Check if user has admin role
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin"))
        {
            await _signInManager.SignOutAsync();
            ModelState.AddModelError("", "Bu sayfaya erişim yetkiniz bulunmamaktadır.");
            return View(model);
        }

        return RedirectToAction("Dashboard", "Admin");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Add user to Admin role
            await _userManager.AddToRoleAsync(user, "Admin");
            
            TempData["SuccessMessage"] = "Admin kullanıcısı başarıyla oluşturuldu.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = "";
    
    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";
    
    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = "";
    
    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";
    
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50, ErrorMessage = "Ad 50 karakterden uzun olamaz")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = "";
    
    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50, ErrorMessage = "Soyad 50 karakterden uzun olamaz")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = "";
    
    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";
    
    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = "";
}
