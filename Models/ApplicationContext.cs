using Microsoft.EntityFrameworkCore;

namespace PetCare.Models.Veterinarian
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Veterinarian> Veterinarians { get; set; }
    }
}