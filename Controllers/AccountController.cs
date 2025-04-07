using CafeMenuWebApp.Data;
using CafeMenuWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace CafeMenuWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.USERNAME == model.Username);

            if (user == null || !await VerifyPassword(model.Password, user.HASHPASSWORD, user.SALTPASSWORD))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.USERNAME),
                new Claim(ClaimTypes.NameIdentifier, user.USERID.ToString()),
                new Claim("TenantId", user.TenantId.ToString()),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private async Task<bool> VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            var isValidParam = new SqlParameter("@IsValid", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC VerifyPassword @Password, @Salt, @StoredHash, @IsValid OUTPUT",
                new SqlParameter("@Password", password),
                new SqlParameter("@Salt", storedSalt),
                new SqlParameter("@StoredHash", storedHash),
                isValidParam);

            return (bool)isValidParam.Value;
        }

        private async Task<(byte[] hash, byte[] salt)> GenerateHashAndSalt(string password)
        {
            var saltParam = new SqlParameter("@Salt", System.Data.SqlDbType.VarBinary, 16) { Direction = System.Data.ParameterDirection.Output };
            var hashParam = new SqlParameter("@Hash", System.Data.SqlDbType.VarBinary, 64) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC HashPassword @Password, @Salt OUTPUT, @Hash OUTPUT",
                new SqlParameter("@Password", password),
                saltParam,
                hashParam);

            return ((byte[])hashParam.Value, (byte[])saltParam.Value);
        }
    }
}