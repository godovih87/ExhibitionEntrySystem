using ExhibitionEntrySystem.Data;
using ExhibitionEntrySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ===== Авторизация =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "❌ Пароль обязателен";
                return View();
            }

            var admin = _context.SecurityLogins.FirstOrDefault(x => x.LoginType == "Admin");

            if (admin == null || admin.PasswordHash != PasswordHelper.Hash(password))
            {
                ViewBag.Error = "❌ Неверный пароль администратора";
                return View();
            }

            // Сессия для администратора
            HttpContext.Session.SetString("ADMIN", "true");
            return RedirectToAction("Dashboard");
        }

        // ===== Панель администратора =====
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("ADMIN") != "true")
                return RedirectToAction("Login");

            var passes = _context.Passes
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .Include(p => p.Pavilion)
                .OrderByDescending(p => p.StartTime)
                .ToList();

            return View(passes);
        }

        // ===== Редактирование пропуска =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPass(Pass model)
        {
            if (model == null) return BadRequest();

            var pass = _context.Passes
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .FirstOrDefault(p => p.Id == model.Id);

            if (pass == null) return NotFound();

            // ===== Обновляем данные посетителя =====
            if (pass.Visitor == null)
                pass.Visitor = new Visitor();

            pass.Visitor.OrganizationName = model.Visitor?.OrganizationName ?? pass.Visitor.OrganizationName;
            pass.Visitor.ContactPerson = model.Visitor?.ContactPerson ?? pass.Visitor.ContactPerson;
            pass.Visitor.PhoneNumber = model.Visitor?.PhoneNumber ?? pass.Visitor.PhoneNumber;

            // ===== Обновляем данные транспортного средства =====
            if (pass.Vehicle == null)
                pass.Vehicle = new Vehicle();

            pass.Vehicle.VehicleType = model.Vehicle?.VehicleType ?? pass.Vehicle.VehicleType;
            pass.Vehicle.LicensePlate = model.Vehicle?.LicensePlate ?? pass.Vehicle.LicensePlate;

            // ===== Обновляем статус и время =====
            pass.Status = model.Status ?? pass.Status;
            pass.StartTime = model.StartTime != default ? model.StartTime : pass.StartTime;
            pass.EndTime = model.EndTime != default ? model.EndTime : pass.EndTime;

            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        // ===== Удаление всего пропуска =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePass(int id)
        {
            var pass = _context.Passes
                .Include(p => p.PassEvents)
                .Include(p => p.Vehicle)
                .Include(p => p.Visitor)
                .FirstOrDefault(p => p.Id == id);

            if (pass == null) return NotFound();

            // Удаляем все связанные данные
            if (pass.PassEvents != null && pass.PassEvents.Any())
                _context.PassEvents.RemoveRange(pass.PassEvents);

            if (pass.Vehicle != null)
                _context.Vehicles.Remove(pass.Vehicle);

            if (pass.Visitor != null)
                _context.Visitors.Remove(pass.Visitor);

            _context.Passes.Remove(pass);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
    }
}
