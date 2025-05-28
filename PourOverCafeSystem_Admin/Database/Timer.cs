using System;
using System.Collections.Generic;

namespace PourOverCafeSystem_Admin.Database;

public partial class Timer
{
    public int TimerId { get; set; }

    public int? ReservationId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool? Arrived { get; set; }

    public virtual Reservation? Reservation { get; set; }
}
