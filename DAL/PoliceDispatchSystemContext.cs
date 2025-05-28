using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBEntities.DBEntities.Models;

public partial class PoliceDispatchSystemContext : DbContext
{
    public PoliceDispatchSystemContext()
    {
    }

    public PoliceDispatchSystemContext(DbContextOptions<PoliceDispatchSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Call> Calls { get; set; }

    public virtual DbSet<CallAssignment> CallAssignments { get; set; }

    public virtual DbSet<Dispatcher> Dispatchers { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventZone> EventZones { get; set; }

    public virtual DbSet<OfficerAssignment> OfficerAssignments { get; set; }

    public virtual DbSet<PoliceOfficer> PoliceOfficers { get; set; }

    public virtual DbSet<StrategicZone> StrategicZones { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=RIKI-FISHER\\SQLEXPRESS01;Initial Catalog=PoliceDispatchSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Call>(entity =>
        {
            entity.HasKey(e => e.CallId).HasName("PK__Calls__5180CF8AEB5F44F5");

            entity.Property(e => e.CallId).HasColumnName("CallID");
            entity.Property(e => e.CallTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Event).WithMany(p => p.Calls)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Calls__EventID__5FB337D6");
        });

        modelBuilder.Entity<CallAssignment>(entity =>
        {
            entity.HasKey(e => new { e.PoliceOfficerId, e.CallId }).HasName("PK__CallAssi__8916B0C5D15141CE");

            entity.Property(e => e.PoliceOfficerId).HasColumnName("PoliceOfficerID");
            entity.Property(e => e.CallId).HasColumnName("CallID");
            entity.Property(e => e.AssignmentTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Call).WithMany(p => p.CallAssignments)
                .HasForeignKey(d => d.CallId)
                .HasConstraintName("FK__CallAssig__CallI__6A30C649");

            entity.HasOne(d => d.PoliceOfficer).WithMany(p => p.CallAssignments)
                .HasForeignKey(d => d.PoliceOfficerId)
                .HasConstraintName("FK__CallAssig__Polic__693CA210");
        });

        modelBuilder.Entity<Dispatcher>(entity =>
        {
            entity.HasKey(e => e.DispatcherId).HasName("PK__Dispatch__EB9ED16475FB0585");

            entity.Property(e => e.DispatcherId)
                .ValueGeneratedNever()
                .HasColumnName("DispatcherID");

            entity.HasOne(d => d.DispatcherNavigation).WithOne(p => p.Dispatcher)
                .HasForeignKey<Dispatcher>(d => d.DispatcherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Dispatche__Dispa__4E88ABD4");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C870BCB8A487");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.Priority).HasMaxLength(20);
        });

        modelBuilder.Entity<EventZone>(entity =>
        {
            entity.HasKey(e => e.ZoneId).HasName("PK__EventZon__60166795B97B0357");

            entity.Property(e => e.ZoneId).HasColumnName("ZoneID");
            entity.Property(e => e.EventId).HasColumnName("EventID");

            entity.HasOne(d => d.Event).WithMany(p => p.EventZones)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__EventZone__Event__59063A47");
        });

        modelBuilder.Entity<OfficerAssignment>(entity =>
        {
            entity.HasKey(e => new { e.PoliceOfficerId, e.EventId }).HasName("PK__OfficerA__2B9AF0BA658CC852");

            entity.Property(e => e.PoliceOfficerId).HasColumnName("PoliceOfficerID");
            entity.Property(e => e.EventId).HasColumnName("EventID");

            entity.HasOne(d => d.Event).WithMany(p => p.OfficerAssignments)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__OfficerAs__Event__66603565");

            entity.HasOne(d => d.PoliceOfficer).WithMany(p => p.OfficerAssignments)
                .HasForeignKey(d => d.PoliceOfficerId)
                .HasConstraintName("FK__OfficerAs__Polic__656C112C");
        });

        modelBuilder.Entity<PoliceOfficer>(entity =>
        {
            entity.HasKey(e => e.PoliceOfficerId).HasName("PK__PoliceOf__3C0EBC3D491360C8");

            entity.Property(e => e.PoliceOfficerId)
                .ValueGeneratedNever()
                .HasColumnName("PoliceOfficerID");
            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

            entity.HasOne(d => d.PoliceOfficerNavigation).WithOne(p => p.PoliceOfficer)
                .HasForeignKey<PoliceOfficer>(d => d.PoliceOfficerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PoliceOff__Polic__534D60F1");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.PoliceOfficers)
                .HasForeignKey(d => d.VehicleTypeId)
                .HasConstraintName("FK__PoliceOff__Vehic__5441852A");
        });

        modelBuilder.Entity<StrategicZone>(entity =>
        {
            entity.HasKey(e => e.StrategicZoneId).HasName("PK__Strategi__A5D901D2A0241AE6");

            entity.Property(e => e.StrategicZoneId).HasColumnName("StrategicZoneID");
            entity.Property(e => e.EventId).HasColumnName("EventID");

            entity.HasOne(d => d.Event).WithMany(p => p.StrategicZones)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Strategic__Event__5BE2A6F2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACF32EA063");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E41C50CB37").IsUnique();

            entity.HasIndex(e => e.Idnumber, "UQ__Users__564DB08ABC6D1F27").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105344E0DCCFB").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Idnumber)
                .HasMaxLength(20)
                .HasColumnName("IDNumber");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.VehicleTypeId).HasName("PK__VehicleT__9F44962333F16957");

            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            entity.Property(e => e.VehicleName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
