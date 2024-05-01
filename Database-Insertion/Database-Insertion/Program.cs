using Database_Insertion;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine($"The added quantity is: {Constants.ItemsQuantity.ToString("N0")}");
        ComparisonService comparisonService = new ComparisonService(new ApplicationDbContext());

        List<Customer> customers = comparisonService.GetCustomers();
        //comparisonService.InsertByAdd(customers);
        //comparisonService.TruncateTable();

        //comparisonService.InsertByAddRange(customers);
        //comparisonService.TruncateTable();

        //comparisonService.InsertByDapper(customers);
        //comparisonService.TruncateTable();

        comparisonService.InsertByBulkInsert(customers);
        comparisonService.TruncateTable();

        //comparisonService.InsertBySQLBulkCopy();
        //comparisonService.TruncateTable();

        Console.WriteLine("Done");
        Console.ReadKey();
    }
}