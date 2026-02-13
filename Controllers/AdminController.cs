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

        private IActionResult? CheckAdminAuth()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string password)
        {
            return RedirectToAction("Login", "Auth");
        }

        public async Task<IActionResult> Dashboard()
        {
            var authCheck = CheckAdminAuth();
            if (authCheck != null) return authCheck;

            var passes = await _context.Passes
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .Include(p => p.Pavilion)
                .Include(p => p.User)
                .OrderByDescending(p => p.StartTime)
                .ToListAsync();

            return View(passes);
        }

        public async Task<IActionResult> Users()
        {
            var authCheck = CheckAdminAuth();
            if (authCheck != null) return authCheck;

            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(int userId, string newRole)
        {
            var authCheck = CheckAdminAuth();
            if (authCheck != null) return authCheck;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Role == "Admin" && newRole != "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    TempData["Error"] = "❌ Нельзя удалить последнего администратора";
                    return RedirectToAction("Users");
                }
            }

            user.Role = newRole;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"✅ Роль пользователя изменена на {newRole}";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPass(Pass model)
        {
            var authCheck = CheckAdminAuth();
            if (authCheck != null) return authCheck;

            if (model == null) return BadRequest();

            var pass = await _context.Passes
                .Include(p => p.Visitor)
                .Include(p => p.Vehicle)
                .Include(p => p.Pavilion)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (pass == null) return NotFound();

            var now = DateTime.Now;

            bool timeChanged = false;
            var newStartTime = pass.StartTime;
            var newEndTime = pass.EndTime;

            if (model.StartTime != default && model.StartTime != pass.StartTime)
            {
                newStartTime = model.StartTime;
                timeChanged = true;
            }

            if (model.EndTime != default && model.EndTime != pass.EndTime)
            {
                newEndTime = model.EndTime;
                timeChanged = true;
            }

            if (timeChanged)
            {
                if (newStartTime.Date < DateTime.Today)
                {
                    TempData["Error"] = "❌ Нельзя установить дату в прошлом";
                    return RedirectToAction("Dashboard");
                }

                if (newStartTime.Date == DateTime.Today && newStartTime < now)
                {
                    TempData["Error"] = "❌ Нельзя установить время, которое уже прошло (сейчас " + now.ToString("HH:mm") + ")";
                    return RedirectToAction("Dashboard");
                }

                if (newEndTime <= newStartTime)
                {
                    TempData["Error"] = "❌ Время окончания должно быть позже времени начала";
                    return RedirectToAction("Dashboard");
                }

                if ((newEndTime - newStartTime).TotalMinutes < 30)
                {
                    TempData["Error"] = "❌ Минимальная длительность пропуска - 30 минут";
                    return RedirectToAction("Dashboard");
                }

                if ((newEndTime - newStartTime).TotalHours > 8)
                {
                    TempData["Error"] = "❌ Максимальная длительность пропуска - 8 часов";
                    return RedirectToAction("Dashboard");
                }

                var booked = await _context.Passes
                    .CountAsync(p => p.Id != pass.Id &&
                                    p.PavilionId == pass.PavilionId &&
                                    p.StartTime < newEndTime &&
                                    p.EndTime > newStartTime);

                if (booked >= pass.Pavilion.MaxSlotsPerHour)
                {
                    TempData["Error"] = "❌ На это время уже нет свободных слотов";
                    return RedirectToAction("Dashboard");
                }

                pass.StartTime = newStartTime;
                pass.EndTime = newEndTime;
            }

            if (pass.Visitor == null)
                pass.Visitor = new Visitor();

            if (model.Visitor != null)
            {
                if (!string.IsNullOrEmpty(model.Visitor.OrganizationName))
                    pass.Visitor.OrganizationName = model.Visitor.OrganizationName;

                if (!string.IsNullOrEmpty(model.Visitor.ContactPerson))
                    pass.Visitor.ContactPerson = model.Visitor.ContactPerson;

                if (!string.IsNullOrEmpty(model.Visitor.PhoneNumber))
                    pass.Visitor.PhoneNumber = model.Visitor.PhoneNumber;
            }

            if (pass.Vehicle == null)
                pass.Vehicle = new Vehicle();

            if (model.Vehicle != null)
            {
                if (!string.IsNullOrEmpty(model.Vehicle.VehicleType))
                    pass.Vehicle.VehicleType = model.Vehicle.VehicleType;

                if (!string.IsNullOrEmpty(model.Vehicle.LicensePlate))
                    pass.Vehicle.LicensePlate = model.Vehicle.LicensePlate;
            }

            if (!string.IsNullOrEmpty(model.Status) && model.Status != pass.Status)
                pass.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Пропуск успешно обновлен";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePass(int id)
        {
            var authCheck = CheckAdminAuth();
            if (authCheck != null) return authCheck;

            var pass = await _context.Passes
                .Include(p => p.PassEvents)
                .Include(p => p.Vehicle)
                .Include(p => p.Visitor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pass == null) return NotFound();

            if (pass.PassEvents != null && pass.PassEvents.Any())
                _context.PassEvents.RemoveRange(pass.PassEvents);

            if (pass.Vehicle != null)
                _context.Vehicles.Remove(pass.Vehicle);

            if (pass.Visitor != null)
                _context.Visitors.Remove(pass.Visitor);

            _context.Passes.Remove(pass);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Пропуск успешно удален";
            return RedirectToAction("Dashboard");
        }
    }
}