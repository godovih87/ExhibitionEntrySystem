using ExhibitionEntrySystem.Data;
using ExhibitionEntrySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? CheckAuth()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> BookForm(int pavilionId, DateTime startTime)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var pavilion = await _context.Pavilions.FindAsync(pavilionId);
            if (pavilion == null) return NotFound("Павильон не найден");

            ViewBag.PavilionId = pavilionId;
            ViewBag.PavilionName = pavilion.Name;

            return View(new Booking
            {
                StartTime = startTime,
                EndTime = startTime.AddHours(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookForm(int pavilionId, Booking model)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var pavilionDb = await _context.Pavilions.FindAsync(pavilionId);
            if (pavilionDb == null)
            {
                ModelState.AddModelError("", "❌ Павильон не найден");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PavilionId = pavilionId;
                ViewBag.PavilionName = pavilionDb.Name;
                return View(model);
            }

            var booked = await _context.Passes.CountAsync(p =>
                p.PavilionId == pavilionId &&
                p.StartTime < model.EndTime &&
                p.EndTime > model.StartTime);

            if (booked >= pavilionDb.MaxSlotsPerHour)
            {
                ModelState.AddModelError("", "❌ Все слоты на это время заняты");
                ViewBag.PavilionId = pavilionId;
                ViewBag.PavilionName = pavilionDb.Name;
                return View(model);
            }

            var visitor = new Visitor
            {
                OrganizationName = model.OrganizationName,
                ContactPerson = model.ContactPerson,
                PhoneNumber = model.PhoneNumber
            };
            _context.Visitors.Add(visitor);
            await _context.SaveChangesAsync();

            var vehicle = new Vehicle
            {
                VehicleType = model.VehicleType,
                LicensePlate = model.LicensePlate
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var secretKey = Guid.NewGuid().ToString();

            var pass = new Pass
            {
                UserId = userId.Value,
                VisitorId = visitor.Id,
                VehicleId = vehicle.Id,
                PavilionId = pavilionId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Status = "Сформирован",
                SecretKey = secretKey,
                CreatedAt = DateTime.Now
            };
            _context.Passes.Add(pass);
            await _context.SaveChangesAsync();

            var scanUrl = Url.Action("Auth", "Checkpoint", new { key = pass.SecretKey }, Request.Scheme);
            ViewBag.QRCode = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={Uri.EscapeDataString(scanUrl)}";
            ViewBag.PassId = pass.Id;
            ViewBag.SecretKey = pass.SecretKey;
            ViewBag.PavilionName = pavilionDb.Name;

            return View("Success", model);
        }
    }
}