using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PourOverCafeSystem_Admin.Database;
using Microsoft.AspNetCore.SignalR;
using PourOverCafeSystem_Admin.Hubs;


namespace PourOverCafeSystem_Admin.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly PourOverCoffeeDbContext _context;
        private readonly IHubContext<ReservationHub> _hubContext;

        public ApprovalController(PourOverCoffeeDbContext context, IHubContext<ReservationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pendingPayments = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Table) // <- Include the table data
                .Where(p => p.PaymentStatus == "Pending")
                .ToListAsync();

            return View(pendingPayments);
        }


        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            var manilaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));

            if (payment != null && payment.Reservation != null)
            {
                payment.PaymentStatus = "Approved";
                payment.Reservation.ReservationStatus = "Approved";

                var timer = new PourOverCafeSystem_Admin.Database.Timer
                {
                    ReservationId = payment.ReservationId,
                    StartTime = manilaTime,
                    EndTime = manilaTime.AddSeconds(10),
                    Arrived = null
                };

                _context.Timers.Add(timer);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "RefreshDashboard");

                return RedirectToAction("Index", "Dashboard");
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Disapprove(int id, string remarks)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .ThenInclude(r => r.Table)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.PaymentStatus = "Cancelled";
                payment.Remarks = remarks;

                if (payment.Reservation != null)
                {
                    payment.Reservation.ReservationStatus = "Cancelled";

                    if (payment.Reservation.Table != null)
                    {
                        payment.Reservation.Table.Status = "Available";
                    }
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "RefreshDashboard");
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
