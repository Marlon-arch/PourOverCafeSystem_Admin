using PourOverCafeSystem_Admin.Database;
using System.Collections.Generic;

namespace PourOverCafeSystem_Admin.ViewModels
{
    public class DashboardViewModel
    {
        public List<Payment> Approvals { get; set; }
        public List<CafeTable> Tables { get; set; }
        public bool CafeIsClosed { get; set; }
        public List<PourOverCafeSystem_Admin.Database.Timer> Arrivals { get; set; }
        public List<PourOverCafeSystem_Admin.Database.Timer> WaitlistActive { get; set; }
        public List<PourOverCafeSystem_Admin.Database.Timer> WaitlistInactive { get; set; }
        public List<Reservation> AllReservations { get; set; }

    }
}
