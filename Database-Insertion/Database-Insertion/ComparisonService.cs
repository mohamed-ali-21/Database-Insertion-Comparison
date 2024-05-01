using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Database_Insertion
{
    public class ComparisonService
    {
        private readonly ApplicationDbContext _dbContext;

        public ComparisonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void InsertBySQLBulkCopy()
        {
            var sw = Stopwatch.StartNew();
            sw.Stop();
            Console.WriteLine($"SQL Bulk Copy: {sw.ElapsedMilliseconds.ToString("N0")} ms");
        }

        public void InsertByAdd(List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();
            foreach (var item in customers)
                _dbContext.Customers.Add(item);

            _dbContext.SaveChanges();

            sw.Stop();
            Console.WriteLine($"EF Add Method: {sw.ElapsedMilliseconds.ToString("N0")} ms");
            _dbContext.ChangeTracker.Clear();
        }

        public void InsertByAddRange(List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            _dbContext.Customers.AddRange(customers);
            _dbContext.SaveChanges();

            sw.Stop();
            Console.WriteLine($"EF AddRange Method: {sw.ElapsedMilliseconds.ToString("N0")} ms");
            _dbContext.ChangeTracker.Clear();
        }

        public void InsertByBulkInsert(List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            if (_dbContext == null)
                Console.WriteLine("hhh");
            _dbContext.BulkInsert(customers);

            sw.Stop();
            Console.WriteLine($"EF BulkInsert Method: {sw.ElapsedMilliseconds.ToString("N0")} ms");
            _dbContext.ChangeTracker.Clear();
        }

        public void InsertByDapper(List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            using var connection = new MySqlConnection(Constants.ConnectionString);
            connection.Open();

            const string sql =
                @"
                    INSERT INTO customers (Name, PhoneNumber, Address, NumberOfOrders)
                    VALUES (@Name, @PhoneNumber, @Address, @NumberOfOrders);
                    ";

            connection.Execute(sql, customers);

            sw.Stop();
            Console.WriteLine($"Dapper: {sw.ElapsedMilliseconds.ToString("N0")} ms");
        }

        public void TruncateTable()
        {
            _dbContext.TruncateTable();
        }

        public List<Customer> GetCustomers()
        {
            List<Customer> customers = new List<Customer>();
            for (int i = 0; i < Constants.ItemsQuantity; i++)
            {
                customers.Add(new Customer
                {
                    Name = "Name - " + i,
                    PhoneNumber = "PhoneNumber - " + i,
                    Address = "Address - " + i,
                    NumberOfOrders = i
                });
            }

            return customers;
        }
    }
}
