using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Models;

namespace ReservaCabanasSite.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cabana> Cabanas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<CabanaImagen> CabanaImagenes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Temporada> Temporadas { get; set; }
    }
}