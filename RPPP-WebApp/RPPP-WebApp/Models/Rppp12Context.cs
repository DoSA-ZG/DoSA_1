using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Models;

public partial class Rppp12Context : DbContext
{
    public Rppp12Context()
    {
    }

    public Rppp12Context(DbContextOptions<Rppp12Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Infrastructure> Infrastructures { get; set; }

    public virtual DbSet<Leasing> Leasings { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    public virtual DbSet<OperationType> OperationTypes { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<Plot> Plots { get; set; }

    public virtual DbSet<Purpose> Purposes { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SoilType> SoilTypes { get; set; }

    public virtual DbSet<Species> Species { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("RPPP12");

            optionsBuilder.UseSqlServer(connectionString);

            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("address_PK");

            entity.ToTable("address");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.PostalCode).HasColumnName("postal_code");
            entity.Property(e => e.Street)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("street");
        });

        modelBuilder.Entity<Infrastructure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Infrastructure_PK");

            entity.ToTable("infrastructure");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Condition)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("condition");
            entity.Property(e => e.CoordX).HasColumnName("coord_x");
            entity.Property(e => e.CoordY).HasColumnName("coord_y");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PlotId).HasColumnName("plot_ID");

            entity.HasOne(d => d.Plot).WithMany(p => p.Infrastructures)
                .HasForeignKey(d => d.PlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("infrastructure_FK");
        });

        modelBuilder.Entity<Leasing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("leasing_PK");

            entity.ToTable("leasing");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.EndDate)
                .HasColumnType("date")
                .HasColumnName("end_date");
            entity.Property(e => e.OwnerId).HasColumnName("owner_ID");
            entity.Property(e => e.PlotId).HasColumnName("plot_ID");
            entity.Property(e => e.RentierId).HasColumnName("rentier_ID");
            entity.Property(e => e.StartDate)
                .HasColumnType("date")
                .HasColumnName("start_date");

            entity.HasOne(d => d.Owner).WithMany(p => p.LeasingOwners)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leasing_FK_1");

            entity.HasOne(d => d.Plot).WithMany(p => p.Leasings)
                .HasForeignKey(d => d.PlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leasing_FK");

            entity.HasOne(d => d.Rentier).WithMany(p => p.LeasingRentiers)
                .HasForeignKey(d => d.RentierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leasing_FK_2");
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("operation_PK");

            entity.ToTable("operation");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.OperationTypeId).HasColumnName("operation_type_ID");
            entity.Property(e => e.PlantId).HasColumnName("plant_ID");
            entity.Property(e => e.RequestId).HasColumnName("request_ID");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.OperationType).WithMany(p => p.Operations)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("operation_FK");

            entity.HasOne(d => d.Plant).WithMany(p => p.Operations)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("operation_FK_2");

            entity.HasOne(d => d.Request).WithMany(p => p.Operations)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("operation_FK_1");
        });

        modelBuilder.Entity<OperationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("operation_type_PK");

            entity.ToTable("operation_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_PK");

            entity.ToTable("order");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CustomerId).HasColumnName("customer_ID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("order_FK");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("person_PK");

            entity.ToTable("person");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.AddressId).HasColumnName("address_ID");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_ID");

            entity.HasOne(d => d.Address).WithMany(p => p.People)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_FK_1");

            entity.HasOne(d => d.Role).WithMany(p => p.People)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_FK");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plant_PK");

            entity.ToTable("plant");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.PlotId).HasColumnName("plot_ID");
            entity.Property(e => e.PurposeId).HasColumnName("purpose_ID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SpeciesId).HasColumnName("species_ID");

            entity.HasOne(d => d.Plot).WithMany(p => p.Plants)
                .HasForeignKey(d => d.PlotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("plant_FK");

            entity.HasOne(d => d.Purpose).WithMany(p => p.Plants)
                .HasForeignKey(d => d.PurposeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plant_FK_2");

            entity.HasOne(d => d.Species).WithMany(p => p.Plants)
                .HasForeignKey(d => d.SpeciesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plant_FK_1");
        });

        modelBuilder.Entity<Plot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Plot_PK");

            entity.ToTable("plot");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CoordX).HasColumnName("coord_x");
            entity.Property(e => e.CoordY).HasColumnName("coord_y");
            entity.Property(e => e.LightIntensity).HasColumnName("light_intensity");
            entity.Property(e => e.OwnerId).HasColumnName("owner_ID");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.SoilId).HasColumnName("soil_ID");

            entity.HasOne(d => d.Owner).WithMany(p => p.Plots)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plot_FK_1");

            entity.HasOne(d => d.Soil).WithMany(p => p.Plots)
                .HasForeignKey(d => d.SoilId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Plot_FK");
        });

        modelBuilder.Entity<Purpose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purpose_PK");

            entity.ToTable("purpose");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("request_PK");

            entity.ToTable("request");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.NeededSpeciesId).HasColumnName("needed_species_ID");
            entity.Property(e => e.OrderId).HasColumnName("order_ID");

            entity.HasOne(d => d.NeededSpecies).WithMany(p => p.Requests)
                .HasForeignKey(d => d.NeededSpeciesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("request_FK");

            entity.HasOne(d => d.Order).WithMany(p => p.Requests)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("request_FK_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Role_PK");

            entity.ToTable("role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SoilType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("soil_type_PK");

            entity.ToTable("soil_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Species>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("species_PK");

            entity.ToTable("species");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.NutritionalValues)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nutritional_values");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
