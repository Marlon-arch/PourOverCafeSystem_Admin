using System;
using System.Collections.Generic;

namespace PourOverCafeSystem_Admin.Database;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? ReservationId { get; set; }

    public string? GcashNumber { get; set; }

    public string? ReceiptNumber { get; set; }

    public string? ScreenshotPath { get; set; }

    public string? PaymentStatus { get; set; }

    public string? Remarks { get; set; }

    public virtual Reservation? Reservation { get; set; }
}
