
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Identity;
using OnlineShop.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography; 
using System.Text; 
using OnlineShop.Models.ViewModels; 
using Microsoft.AspNetCore.Http; 
using System.Collections.Generic; 

namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart"; 

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Register()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError(string.Empty, "Користувач з таким ім'ям вже існує.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password), 
                    Role = "User" 
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                
                await Authenticate(user.Username, user.Role);

                return RedirectToAction("Index", "Home"); 
            }
            return View(model);
        }

        
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

                if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Неправильне ім'я користувача або пароль.");
                    return View(model);
                }

                await Authenticate(user.Username, user.Role);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home"); 
                }
            }
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            
            HttpContext.Session.Remove(CartSessionKey);
            

            return RedirectToAction("Index", "Home");
        }

        
        public IActionResult AccessDenied()
        {
            return View();
        }


        

        private async Task Authenticate(string userName, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}