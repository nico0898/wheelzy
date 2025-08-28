using api.DTOs;
using api.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Make> Makes => Set<Make>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<SubModel> SubModels => Set<SubModel>();
    public DbSet<CarSpec> CarSpecs => Set<CarSpec>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<LocationBuyer> LocationBuyers => Set<LocationBuyer>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<CarStatus> CarStatuses => Set<CarStatus>();

    public DbSet<CarSummaryRow> CarSummaryRows => Set<CarSummaryRow>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // ===== Make
        model.Entity<Make>(e =>
        {
            e.ToTable("Make");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique(); // evitar TOYOTA/Toyota duplicado a nivel nombre exacto
        });

        // ===== Model
        model.Entity<Model>(e =>
        {
            e.ToTable("Model");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);

            e.HasOne(x => x.Make)
            .WithMany(x => x.Models)
            .HasForeignKey(x => x.MakeId)
            .OnDelete(DeleteBehavior.Restrict);

            // Único por MakeId + Name
            e.HasIndex(x => new { x.MakeId, x.Name }).IsUnique();
        });

        // ===== SubModel
        model.Entity<SubModel>(e =>
        {
            e.ToTable("SubModel");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);

            e.HasOne(x => x.Model)
            .WithMany(x => x.SubModels)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

            // Único por ModelId + Name
            e.HasIndex(x => new { x.ModelId, x.Name }).IsUnique();
        });

        // ===== CarSpec
        model.Entity<CarSpec>(e =>
        {
            e.ToTable("CarSpec");
            e.HasKey(x => x.Id);

            // Si no querés usar columna reservada [Year], renombra:
            e.Property(x => x.Year).HasColumnName("Year"); // o "CarYear" si prefieres

            e.HasOne(x => x.Make)
            .WithMany()
            .HasForeignKey(x => x.MakeId)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Model)
            .WithMany()
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SubModel)
            .WithMany()
            .HasForeignKey(x => x.SubModelId)
            .OnDelete(DeleteBehavior.Restrict);

            // Unicidad de la combinación Year + Make + Model + SubModel
            e.HasIndex(x => new { x.Year, x.MakeId, x.ModelId, x.SubModelId }).IsUnique();
        });

        // ===== Location
        model.Entity<Location>(e =>
        {
            e.ToTable("Location");
            e.HasKey(x => x.Id);
            e.Property(x => x.ZipCode).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.ZipCode).IsUnique();
        });

        // ===== Car
        model.Entity<Car>(e =>
        {
            e.ToTable("Car");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.CarSpec)
            .WithMany(x => x.Cars)
            .HasForeignKey(x => x.CarSpecId)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Location)
            .WithMany(x => x.Cars)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Buyer
        model.Entity<Buyer>(e =>
        {
            e.ToTable("Buyer");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // ===== LocationBuyer (many-to-many)
        model.Entity<LocationBuyer>(e =>
        {
            e.ToTable("LocationBuyer");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Location)
            .WithMany(x => x.LocationBuyers)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Buyer)
            .WithMany(x => x.LocationBuyers)
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);

            // Evitar duplicar la misma relación
            e.HasIndex(x => new { x.LocationId, x.BuyerId }).IsUnique();
        });

        // ===== Offer
        model.Entity<Offer>(e =>
        {
            e.ToTable("Offer");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);

            e.HasOne(x => x.Car)
            .WithMany(x => x.Offers)
            .HasForeignKey(x => x.CarId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Buyer)
            .WithMany(x => x.Offers)
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

            // "Solo una oferta actual por auto"
            // Índice filtrado (SQL Server) => requiere UseSqlServer
            e.HasIndex(x => x.CarId)
            .HasDatabaseName("IX_Offer_Car_Current")
            .HasFilter("[Current] = 1")
            .IsUnique();
        });

        // ===== Status
        model.Entity<Status>(e =>
        {
            e.ToTable("Status");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // ===== CarStatus (historial)
        model.Entity<CarStatus>(e =>
        {
            e.ToTable("CarStatus");
            e.HasKey(x => x.Id);

            e.Property(x => x.ChangedAt).IsRequired();
            e.Property(x => x.ChangedBy).HasMaxLength(100);

            e.HasOne(x => x.Car)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.CarId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Status)
            .WithMany(x => x.CarStatuses)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

            // "Solo un estado actual por auto"
            e.HasIndex(x => x.CarId)
            .HasDatabaseName("IX_CarStatus_Car_IsCurrent")
            .HasFilter("[IsCurrent] = 1")
            .IsUnique();
        });

        model.Entity<CarSummaryRow>(e =>
        {
            e.HasNoKey();          // keyless
            e.ToView(null);        // no está mapeado a una vista real
        });
    }
}
