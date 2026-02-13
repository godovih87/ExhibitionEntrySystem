using ExhibitionEntrySystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Controllers
{
    public class PavilionController : Controller
    {
        private readonly AppDbContext _context;

        public PavilionController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pavilions = _context.Pavilions.ToList();

            ViewBag.IsAuthenticated = HttpContext.Session.GetInt32("UserId") != null;

            return View(pavilions);
        }

        public IActionResult TimeSlots(int pavilionId, DateTime? date = null)
        {
            var pavilion = _context.Pavilions.Find(pavilionId);
            if (pavilion == null) return NotFound("Павильон не найден");

            var currentDate = date ?? DateTime.Today;
            var slotsPerHour = pavilion.MaxSlotsPerHour;

            var passes = _context.Passes
                .Where(p => p.PavilionId == pavilionId && p.StartTime.Date == currentDate.Date)
                .ToList();

            var hours = Enumerable.Range(9, 10)
                .Select(h =>
                {
                    var start = currentDate.Date.AddHours(h);
                    var end = start.AddHours(1);
                    var booked = passes.Count(p => p.StartTime < end && p.EndTime > start);
                    return new
                    {
                        Hour = h,
                        Available = slotsPerHour - booked,
                        StartTime = start,
                        IsAvailable = (slotsPerHour - booked) > 0
                    };
                }).ToList();

            ViewBag.PavilionId = pavilionId;
            ViewBag.PavilionName = pavilion.Name;
            ViewBag.Hours = hours;
            ViewBag.CurrentDate = currentDate.ToString("yyyy-MM-dd");
            ViewBag.IsAuthenticated = HttpContext.Session.GetInt32("UserId") != null;

            return View();
        }
    }
}