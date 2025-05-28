using System;
using System.Collections.Generic;

namespace PourOverCafeSystem_Admin.Database;

public partial class CafeTable
{
    public int TableId { get; set; }

    public string? TableName { get; set; }

    public int? Capacity { get; set; }

    public bool? HasPowerOutlet { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
