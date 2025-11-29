using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PourOverCafeSystem_Admin.Database;

public partial class PourOverCoffeeDbContext : DbContext
{
    public PourOverCoffeeDbContext()
    {
    }

    public PourOverCoffeeDbContext(DbContextOptions<PourOverCoffeeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CafeTable> CafeTables { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Timer> Timers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CafeTable>(entity =>
        {
            entity.HasKey(e => e.TableId).HasName("PK__CafeTabl__7D5F018E258B7355");

            entity.Property(e => e.TableId).HasColumnName("TableID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.TableName).HasMaxLength(10);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A589EB4AA0F");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.GcashNumber)
                .HasMaxLength(20)
                .HasColumnName("GCashNumber");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ReceiptNumber).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(255);
            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.ScreenshotPath).HasMaxLength(200);

            entity.HasOne(d => d.Reservation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK__Payments__Reserv__412EB0B6");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F0458F98BC5");

            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.ContactNumber).HasMaxLength(20);
            entity.Property(e => e.GuestName).HasMaxLength(100);
            entity.Property(e => e.ReservationDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReservationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TableId).HasColumnName("TableID");

            entity.HasOne(d => d.Table).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.TableId)
                .HasConstraintName("FK__Reservati__Table__3C69FB99");
        });

        modelBuilder.Entity<Timer>(entity =>
        {
            entity.HasKey(e => e.TimerId).HasName("PK__Timers__431D6D501484B5A3");

            entity.Property(e => e.TimerId).HasColumnName("TimerID");
            entity.Property(e => e.Arrived).HasDefaultValue(false);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Timers)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK__Timers__Reservat__44FF419A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC96AAE6FF");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(10);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
