using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PourOverCafeSystem_Admin.Database;

public class TimerController : Controller
{
    private readonly PourOverCoffeeDbContext _context;

    public TimerController(PourOverCoffeeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> WaitList()
    {
        var activeTimers = await _context.Timers
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Table)
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Payments)
            .Where(t => t.StartTime != null && t.Arrived == false &&
                        t.Reservation.Payments.Any(p => p.PaymentStatus == "Approved"))
            .ToListAsync();

        return View(activeTimers);
    }
}
