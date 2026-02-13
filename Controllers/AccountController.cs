using ExhibitionEntrySystem.Data;
using ExhibitionEntrySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? CheckAuth()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            var passes = await _context.Passes
                .Include(p => p.Pavilion)
                .Include(p => p.Vehicle)
                .Include(p => p.Visitor)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                User = user,
                Passes = passes
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserFullName", user.FullName);

            TempData["Success"] = "✅ Профиль успешно обновлен";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "❌ Пароли не совпадают");
                return View();
            }

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = await _context.Users.FindAsync(userId);

            if (user.PasswordHash != PasswordHelper.Hash(currentPassword))
            {
                ModelState.AddModelError("", "❌ Неверный текущий пароль");
                return View();
            }

            user.PasswordHash = PasswordHelper.Hash(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Пароль успешно изменен";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> PassDetails(int id)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var pass = await _context.Passes
                .Include(p => p.Pavilion)
                .Include(p => p.Vehicle)
                .Include(p => p.Visitor)
                .Include(p => p.PassEvents)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (pass == null)
                return NotFound();

            return View(pass);
        }
    }
}