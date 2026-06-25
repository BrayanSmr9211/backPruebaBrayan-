using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Venue>().HasData(
            new Venue { Id = 1, Nombre = "Auditorio Central", Capacidad = 200, Ciudad = "Bogota" },
            new Venue { Id = 2, Nombre = "Sala Norte", Capacidad = 50, Ciudad = "Bogota" },
            new Venue { Id = 3, Nombre = "Arena Sur", Capacidad = 500, Ciudad = "Medellin" }
        );

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId);

            entity.Property(e => e.PrecioEntrada)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasOne(r => r.Evento)
                .WithMany(e => e.Reservas)
                .HasForeignKey(r => r.EventoId);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
        });
    }
}

