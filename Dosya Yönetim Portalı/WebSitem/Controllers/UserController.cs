using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebSitem.Models;

namespace WebSitem.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly INotyfService _notyfService;

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, INotyfService notyfService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _notyfService = notyfService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    var userRole = await _userManager.GetRolesAsync(user);
                    if (userRole.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }



                    if (result.Succeeded)
                    {
                        _notyfService.Success("Başarıyla giriş yaptınız!");
                        return RedirectToAction("Index", "Home");
                    }
                }
                _notyfService.Error("Email veya şifre hatalı!");
                ModelState.AddModelError("", "Email veya şifre hatalı!");
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StorageLimit = 10L * 1024 * 1024 * 1024, 
                    UsedStorage = 0
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Rol kontrolü ve atama
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new AppRole { Name = "User" });
                    }
                    
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    _notyfService.Information("Başarıyla kayıt oldunuz! Size atanan depolama alanı: 10GB");
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    _notyfService.Error(error.Description);
                }
            
            return View(model);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notyfService.Success("Başarıyla çıkış yaptınız!");
            return RedirectToAction("Login");
        }
    }
}
