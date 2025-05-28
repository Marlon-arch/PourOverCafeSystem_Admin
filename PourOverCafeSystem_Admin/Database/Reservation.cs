using System;
using System.Collections.Generic;

namespace PourOverCafeSystem_Admin.Database;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int? TableId { get; set; }

    public string? GuestName { get; set; }

    public string? ContactNumber { get; set; }

    public int? NumberOfGuests { get; set; }

    public DateTime? ReservationDateTime { get; set; }

    public string? OrderText { get; set; }

    public string? ReservationStatus { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual CafeTable? Table { get; set; }

    public virtual ICollection<Timer> Timers { get; set; } = new List<Timer>();
}
