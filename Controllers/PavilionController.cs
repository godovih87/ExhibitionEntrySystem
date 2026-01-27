using ExhibitionEntrySystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PavilionController : Controller
{
    private readonly AppDbContext _context;

    public PavilionController(AppDbContext context)
    {
        _context = context;
    }

    // Список павильонов
    public IActionResult Index()
    {
        var pavilions = _context.Pavilions.ToList();
        return View(pavilions);
    }

    // Список слотов по павильону
    public IActionResult TimeSlots(int pavilionId, DateTime? date = null)
    {
        var pavilion = _context.Pavilions.Find(pavilionId);
        if (pavilion == null) return NotFound("Павильон не найден");

        var currentDate = date ?? DateTime.Today;
        var slotsPerHour = pavilion.MaxSlotsPerHour;

        // Получаем все пропуски на этот день
        var passes = _context.Passes
            .Where(p => p.PavilionId == pavilionId && p.StartTime.Date == currentDate.Date)
            .ToList();

        // Генерируем часы и доступные слоты
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
                    StartTime = start
                };
            }).ToList();

        ViewBag.PavilionId = pavilionId;       // <-- теперь передаём ID павильона
        ViewBag.PavilionName = pavilion.Name;
        ViewBag.Hours = hours;
        ViewBag.CurrentDate = currentDate.ToString("yyyy-MM-dd");

        return View();
    }
}
