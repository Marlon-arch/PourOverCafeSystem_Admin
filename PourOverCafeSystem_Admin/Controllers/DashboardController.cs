using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PourOverCafeSystem_Admin.Database;
using PourOverCafeSystem_Admin.ViewModels;
using Microsoft.AspNetCore.SignalR;
using PourOverCafeSystem_Admin.Hubs;


namespace PourOverCafeSystem_Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly PourOverCoffeeDbContext _context;
        private readonly IHubContext<ReservationHub> _hubContext;

        public DashboardController(PourOverCoffeeDbContext context, IHubContext<ReservationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public class TimerRequest
        {
            public int timerId { get; set; }
        }

        private DateTime GetManilaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = GetManilaTime();

            // Automatically expire overdue reservations
            var expiredTimers = await _context.Timers
                .Include(t => t.Reservation)
                .ThenInclude(r => r.Table)
                .Where(t => t.EndTime < now && t.Reservation.ReservationStatus == "Approved" && t.Arrived != true)
                .ToListAsync();

            foreach (var timer in expiredTimers)
            {
                if (timer.Reservation != null)
                {
                    timer.Reservation.ReservationStatus = "Expired";

                    if (timer.Reservation.Table != null)
                        timer.Reservation.Table.Status = "Available";

                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.ReservationId == timer.ReservationId);
                    if (payment != null)
                        payment.PaymentStatus = "Cancelled";
                }
            }

            await _context.SaveChangesAsync();

            if (expiredTimers.Any())
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "RefreshDashboard");
                await _hubContext.Clients.All.SendAsync("WaitlistUpdated");
                await _hubContext.Clients.All.SendAsync("TableStatusUpdated");
            }

            var model = new DashboardViewModel
            {
                Approvals = await _context.Payments
                    .Include(p => p.Reservation)
                    .ThenInclude(r => r.Table)
                    .Where(p => p.PaymentStatus == "Pending")
                    .ToListAsync(),

                WaitlistActive = await _context.Timers
                    .Include(t => t.Reservation)
                    .ThenInclude(r => r.Table)
                    .Where(t =>
                        t.Reservation.ReservationStatus == "Approved" &&
                        (t.Arrived == null || t.Arrived == false) &&
                        t.EndTime > now)
                    .ToListAsync(),

                WaitlistInactive = await _context.Timers
                    .Include(t => t.Reservation)
                    .ThenInclude(r => r.Table)
                    .Where(t =>
                        t.Reservation.ReservationStatus == "Expired"
                        || t.Reservation.ReservationStatus == "Cancelled"
                        || t.Reservation.ReservationStatus == "Completed")
                    .ToListAsync(),

                Arrivals = await _context.Timers
                    .Include(t => t.Reservation)
                    .ThenInclude(r => r.Table)
                    .Where(t => t.Arrived == true)
                    .ToListAsync(),

                Tables = await _context.CafeTables.ToListAsync(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkArrived(int timerId)
        {
            var timer = await _context.Timers
                .Include(t => t.Reservation)
                .ThenInclude(r => r.Table)
                .FirstOrDefaultAsync(t => t.TimerId == timerId);

            if (timer != null)
            {
                timer.Arrived = true;
                timer.Reservation.ReservationStatus = "Completed";
                timer.Reservation.Table.Status = "Unavailable";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation(int timerId)
        {
            var timer = await _context.Timers
                .Include(t => t.Reservation)
                .ThenInclude(r => r.Table)
                .FirstOrDefaultAsync(t => t.TimerId == timerId);

            if (timer != null)
            {
                timer.Reservation.ReservationStatus = "Cancelled";
                timer.Reservation.Table.Status = "Available";
                timer.Arrived = false;

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == timer.ReservationId);
                if (payment != null)
                    payment.PaymentStatus = "Cancelled";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTimer([FromBody] TimerRequest data)
        {
            var timer = await _context.Timers.FindAsync(data.timerId);
            if (timer != null)
            {
                _context.Timers.Remove(timer);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> ExpireTimer(int timerId)
        {
            var timer = await _context.Timers
                .Include(t => t.Reservation)
                .ThenInclude(r => r.Table)
                .FirstOrDefaultAsync(t => t.TimerId == timerId);

            if (timer != null && timer.Reservation != null)
            {
                timer.Reservation.ReservationStatus = "Expired";

                if (timer.Reservation.Table != null)
                    timer.Reservation.Table.Status = "Available";

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.ReservationId == timer.ReservationId);

                if (payment != null)
                    payment.PaymentStatus = "Cancelled";

                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("WaitlistUpdated");

                await _hubContext.Clients.All.SendAsync("TableStatusUpdated");

                return Ok();
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAllInactive()
        {
            var now = GetManilaTime();

            var timers = await _context.Timers
                .Include(t => t.Reservation)
                .Where(t =>
                    t.Reservation.ReservationStatus == "Completed"
                    || t.Reservation.ReservationStatus == "Expired"
                    || t.Reservation.ReservationStatus == "Cancelled")
                .ToListAsync();

            _context.Timers.RemoveRange(timers);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> UpdateTableStatus(int tableId, string status)
        {
            var table = await _context.CafeTables.FirstOrDefaultAsync(t => t.TableId == tableId);
            if (table != null)
            {
                table.Status = status;
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("TableStatusUpdated");

                return Ok();
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Summary(DateTime? from, DateTime? to, string? search)
        {
            var start = from ?? DateTime.Today;
            var end = to?.AddDays(1).AddTicks(-1) ?? GetManilaTime();

            var query = _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.ReservationDateTime >= start && r.ReservationDateTime <= end);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r => r.GuestName != null && r.GuestName.ToLower().Contains(search));
            }

            var reservations = await query
                .OrderByDescending(r => r.ReservationDateTime)
                .ToListAsync();

            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.SearchQuery = search;

            return View("Summary", reservations);
        }

        [HttpPost]
        [Route("api/broadcast-refresh")]
        public async Task<IActionResult> BroadcastRefresh()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "RefreshDashboard");
            return Ok("SignalR message sent from Admin");
        }

    }
}
