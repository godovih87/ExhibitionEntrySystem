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

        // ===== Форма ввода пароля КПП =====
        [HttpGet]
        public IActionResult Auth(string key)
        {
            ViewBag.Key = key;
            return View(); // ищет Auth.cshtml
        }

        [HttpPost]
        public IActionResult Auth(string key, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "❌ Пароль обязателен";
                ViewBag.Key = key;
                return View();
            }

            var kpp = _context.SecurityLogins.FirstOrDefault(x => x.LoginType == "KPP");

            if (kpp == null || kpp.PasswordHash != PasswordHelper.Hash(password))
            {
                ViewBag.Error = "❌ Неверный пароль КПП";
                ViewBag.Key = key;
                return View();
            }


            // Пароль верный  сохраняем сессию
            HttpContext.Session.SetString("KPP", "true");

            return RedirectToAction("Scan", new { key });
        }

        // ===== Просмотр пропуска через QR =====
        [HttpGet]
        public IActionResult Scan(string key)
        {
            if (HttpContext.Session.GetString("KPP") != "true")
                return RedirectToAction("Auth", new { key });

            var pass = _context.Passes
                .Include(p => p.Pavilion)
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .Include(p => p.PassEvents)
                .FirstOrDefault(p => p.SecretKey == key);

            if (pass == null)
                return NotFound("Пропуск не найден");

            return View("Result", pass);
        }

        // ===== Въезд =====
        [HttpPost]
        public IActionResult Entry(int passId)
        {
            var pass = _context.Passes.FirstOrDefault(p => p.Id == passId);
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

            _context.SaveChanges();
            return RedirectToAction("Scan", new { key = pass.SecretKey });
        }

        // ===== Выезд =====
        [HttpPost]
        public IActionResult Exit(int passId)
        {
            var pass = _context.Passes.FirstOrDefault(p => p.Id == passId);
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

            _context.SaveChanges();
            return RedirectToAction("Scan", new { key = pass.SecretKey });
        }
    }
}
