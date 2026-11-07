using System.Globalization;
using System.Text;

namespace ContosoPizza.Services;

public static class SalesSummaryService
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("en-US");

    public static void GenerateSalesSummary(string salesDirectory, string reportFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(salesDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportFilePath);

        if (!Directory.Exists(salesDirectory))
            throw new DirectoryNotFoundException($"Sales directory not found: {salesDirectory}");

        var reportPath = Path.GetFullPath(reportFilePath);
        var reportDirectory = Path.GetDirectoryName(reportPath);
        if (reportDirectory is not null)
            Directory.CreateDirectory(reportDirectory);

        var salesFiles = Directory.GetFiles(salesDirectory, "*.txt")
            .Where(file => !string.Equals(Path.GetFullPath(file), reportPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var salesByFile = salesFiles
            .Select(file => (FileName: Path.GetFileName(file), Total: ReadSalesTotal(file)))
            .ToArray();
        var totalSales = salesByFile.Sum(sales => sales.Total);

        var report = new StringBuilder()
            .AppendLine("Sales Summary")
            .AppendLine("----------------------------")
            .AppendLine($" Total Sales: {totalSales.ToString("C", CurrencyCulture)}")
            .AppendLine()
            .AppendLine(" Details:");

        foreach (var sales in salesByFile)
            report.AppendLine($"  {sales.FileName}: {sales.Total.ToString("C", CurrencyCulture)}");

        File.WriteAllText(reportPath, report.ToString());
    }

    private static decimal ReadSalesTotal(string salesFile)
    {
        var total = 0m;
        var lineNumber = 0;

        foreach (var line in File.ReadLines(salesFile))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!decimal.TryParse(line, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                throw new FormatException($"Invalid sales amount in '{salesFile}' on line {lineNumber}: '{line}'.");

            total += amount;
        }

        return total;
    }
}
