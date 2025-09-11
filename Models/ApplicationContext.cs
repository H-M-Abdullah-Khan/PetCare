using Microsoft.EntityFrameworkCore;

namespace PetCare.Models.Veterinarian
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PetRecord> PetRecords { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}