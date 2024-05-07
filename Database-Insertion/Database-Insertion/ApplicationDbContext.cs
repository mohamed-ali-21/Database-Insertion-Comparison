using Microsoft.EntityFrameworkCore;

namespace Database_Insertion
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(Constants.MySQLConnectionString);
        }

        public void TruncateTable()
        {
            Database.ExecuteSqlRaw($"TRUNCATE TABLE customers;");
        }
    }
}
