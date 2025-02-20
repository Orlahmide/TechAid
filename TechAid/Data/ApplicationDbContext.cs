using Microsoft.EntityFrameworkCore;
using TechAid.Models.Entity;



namespace TechAid.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Ticket> Tickets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Employee)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Department)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Category)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
               .Property(t => t.Status)
               .HasConversion<string>();

            modelBuilder.Entity<Employee>()
              .Property(t => t.Role)
              .HasConversion<string>();


        }

    }

    
}
