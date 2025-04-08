using HotelReservastionsManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelReservastionsManager.Data
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ReservationClients> ReservationClients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReservationClients>()
                .HasKey(rc => new { rc.ReservationId, rc.ClientId });

            modelBuilder.Entity<ReservationClients>()

                .HasOne(rc => rc.Reservation)
                .WithMany(r => r.ReservationClients)
                .HasForeignKey(rc => rc.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservationClients>()
                .HasOne(rc => rc.Client)
                .WithMany(c => c.ReservationClients)
                .HasForeignKey(rc => rc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(re => re.Room)
                .WithMany(ro => ro.Reservations)
                .HasForeignKey(re => re.RoomNumber)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
