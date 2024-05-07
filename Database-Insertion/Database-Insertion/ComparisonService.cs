using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data;
using System.Data.SqlClient;
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

        public async void InsertBySQLBulkCopy(List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            using var bulkCopy = new SqlBulkCopy(Constants.SqlServerConnectionString);

            bulkCopy.DestinationTableName = "dbo.Customers";

            bulkCopy.ColumnMappings.Add(nameof(Customer.Name), "Name");
            bulkCopy.ColumnMappings.Add(nameof(Customer.Address), "Address");
            bulkCopy.ColumnMappings.Add(nameof(Customer.PhoneNumber), "PhoneNumber");
            bulkCopy.ColumnMappings.Add(nameof(Customer.NumberOfOrders), "NumberOfOrders");

            bulkCopy.WriteToServer(GetUsersDataTable(customers));

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

            using var connection = new MySqlConnection(Constants.MySQLConnectionString);
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
        public async Task PostgresBinaryCopyInsertAsync()
        {
            var globalSW = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();
            string connectionString = "Host=localhost;Port=5432;Username=user-name;Password=strong-password;Database=postgres";

            int factory = 2;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                Console.WriteLine($"The open connection take {sw.ElapsedMilliseconds} ms");

                //sw = Stopwatch.StartNew();
                await Copy(conn, 26_000_000, 25_000_000);
                Console.WriteLine($"Execute take {sw.ElapsedMilliseconds} ms");
            }

            Console.WriteLine($"Total: {globalSW.ElapsedMilliseconds.ToString("N0")} ms");
            globalSW.Stop();
        }

        private async Task Copy(NpgsqlConnection npgsqlConnection, int numRecords, int statrt)
        {
            using var writer = npgsqlConnection.BeginBinaryImport("COPY public.\"Customer\" (Id, Name, PhoneNumber, NumberOfOrders, Address) FROM STDIN (FORMAT BINARY)");
            var sw = Stopwatch.StartNew();

            for (int i = statrt; i < numRecords; i++)
            {
                // Write values for each record
                await writer.StartRowAsync();
                await writer.WriteAsync(i + 100);
                await writer.WriteAsync("Name - " + i);
                await writer.WriteAsync("PhoneNumber - " + i);
                await writer.WriteAsync(i);
                await writer.WriteAsync("Address - " + i);
            }
            Console.WriteLine($"Loop : {sw.ElapsedMilliseconds}");
            sw.Stop();
            await writer.CompleteAsync();
        }


        DataTable GetUsersDataTable(List<Customer> customers)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(nameof(Customer.Name), typeof(string));
            dataTable.Columns.Add(nameof(Customer.Address), typeof(string));
            dataTable.Columns.Add(nameof(Customer.PhoneNumber), typeof(string));
            dataTable.Columns.Add(nameof(Customer.NumberOfOrders), typeof(int));

            foreach (var customer in customers)
            {
                dataTable.Rows.Add(
                    customer.Name, customer.Address, customer.PhoneNumber, customer.NumberOfOrders);
            }

            return dataTable;
        }
    }
}
