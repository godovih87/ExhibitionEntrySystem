using ExhibitionEntrySystem.Data;
using ExhibitionEntrySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Controllers
{
    public class CheckpointController : Controller
    {
        private readonly AppDbContext _context;

        public CheckpointController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Auth(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Key = key;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Auth(string key, string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "❌ Email и пароль обязательны";
                ViewBag.Key = key;
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email &&
                                         (u.Role == "KPP" || u.Role == "Admin") &&
                                         u.PasswordHash == PasswordHelper.Hash(password));

            if (user == null)
            {
                ViewBag.Error = "❌ Неверный email, пароль или у вас нет прав доступа к КПП";
                ViewBag.Key = key;
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserFullName", user.FullName);

            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Scan", new { key });
        }

        [HttpGet]
        public async Task<IActionResult> Scan(string key)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Auth", new { key });
            }

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "KPP" && role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var pass = await _context.Passes
                .Include(p => p.Pavilion)
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .Include(p => p.PassEvents)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.SecretKey == key);

            if (pass == null)
            {
                return NotFound("Пропуск не найден");
            }

            return View("Result", pass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entry(int passId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "KPP" && role != "Admin")
                return RedirectToAction("AccessDenied", "Auth");

            var pass = await _context.Passes.FirstOrDefaultAsync(p => p.Id == passId);
            if (pass == null) return NotFound();

            var now = DateTime.Now;

            if (now < pass.StartTime || now > pass.EndTime)
            {
                TempData["Error"] = "⛔ Пропуск не активен или просрочен";
                return RedirectToAction("Scan", new { key = pass.SecretKey });
            }

            if (pass.Status != "Сформирован")
            {
                TempData["Error"] = "⛔ Повторный въезд запрещён";
                return RedirectToAction("Scan", new { key = pass.SecretKey });
            }

            pass.Status = "Въехал";
            pass.EntryTime = now;

            _context.PassEvents.Add(new PassEvent
            {
                PassId = pass.Id,
                EventTime = now,
                EventType = "Entry"
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Въезд зарегистрирован";
            return RedirectToAction("Scan", new { key = pass.SecretKey });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Exit(int passId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "KPP" && role != "Admin")
                return RedirectToAction("AccessDenied", "Auth");

            var pass = await _context.Passes.FirstOrDefaultAsync(p => p.Id == passId);
            if (pass == null) return NotFound();

            if (pass.Status != "Въехал")
            {
                TempData["Error"] = "⛔ Выезд невозможен";
                return RedirectToAction("Scan", new { key = pass.SecretKey });
            }

            pass.Status = "Выехал";
            pass.ExitTime = DateTime.Now;

            _context.PassEvents.Add(new PassEvent
            {
                PassId = pass.Id,
                EventTime = DateTime.Now,
                EventType = "Exit"
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Выезд зарегистрирован";
            return RedirectToAction("Scan", new { key = pass.SecretKey });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Pavilion");
        }
    }
}