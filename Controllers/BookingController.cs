using ExhibitionEntrySystem.Data;
using ExhibitionEntrySystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExhibitionEntrySystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult BookForm(int pavilionId, DateTime startTime)
        {
            var pavilion = _context.Pavilions.Find(pavilionId);
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
        public IActionResult BookForm(int pavilionId, Booking model)
        {
            var pavilionDb = _context.Pavilions.Find(pavilionId);
            if (pavilionDb == null)
            {
                ModelState.AddModelError("", "❌ Павильон не найден");
                return View(model);
            }

            // ===== Проверка ModelState =====
            if (!ModelState.IsValid)
            {
                ViewBag.PavilionId = pavilionId;
                ViewBag.PavilionName = pavilionDb.Name;
                return View(model);
            }

            // ===== Проверка свободных слотов =====
            var booked = _context.Passes.Count(p =>
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

            // ===== Создаём Visitor =====
            var visitor = new Visitor
            {
                OrganizationName = model.OrganizationName,
                ContactPerson = model.ContactPerson,
                PhoneNumber = model.PhoneNumber
            };
            _context.Visitors.Add(visitor);
            _context.SaveChanges();

            // ===== Создаём Vehicle =====
            var vehicle = new Vehicle
            {
                VehicleType = model.VehicleType,
                LicensePlate = model.LicensePlate
            };
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();

            // ===== Создаём Pass =====
            var pass = new Pass
            {
                VisitorId = visitor.Id,
                VehicleId = vehicle.Id,
                PavilionId = pavilionId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Status = "Сформирован",
                SecretKey = Guid.NewGuid().ToString()
            };
            _context.Passes.Add(pass);
            _context.SaveChanges();

            // ===== Генерация QR =====
            var scanUrl = Url.Action("Auth", "Checkpoint", new { key = pass.SecretKey }, Request.Scheme);
            ViewBag.QRCode = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={scanUrl}";
            ViewBag.PassId = pass.Id;
            ViewBag.PavilionName = pavilionDb.Name;

            return View("Success");
        }
    }
}
