# Contoso Pizza assignment submission

Repository: https://github.com/caarloosdab/ContossoPizza

## 1. Pizza API evidence

The following responses were captured from the running API at `http://localhost:5249`.

Initial pizza list:

```http
GET /pizza
```

Status: `200 OK`

```json
[
  {"id":1,"name":"Classic Italian","isGlutenFree":false},
  {"id":2,"name":"Veggie","isGlutenFree":true}
]
```

After adding the additional pizza:

```http
GET /pizza
```

Status: `200 OK`

```json
[
  {"id":1,"name":"Classic Italian","isGlutenFree":false},
  {"id":2,"name":"Veggie","isGlutenFree":true},
  {"id":3,"name":"Hawaii","isGlutenFree":false}
]
```

## 2. Pizza API operations

### GET

Request:

```http
GET /pizza
```

Response: the existing pizzas shown above.  
Status code: `200`

### POST

Request:

```http
POST /pizza
Content-Type: application/json

{"name":"Hawaii","isGlutenFree":false}
```

Response:

```json
{"id":3,"name":"Hawaii","isGlutenFree":false}
```

Status code: `201`

### PUT

Request:

```http
PUT /pizza/3
Content-Type: application/json

{"id":3,"name":"Hawaiian","isGlutenFree":false}
```

Response: no response body.  
Status code: `204`

### DELETE

Request:

```http
DELETE /pizza/3
```

Response: no response body.  
Status code: `204`

## 3. Sales summary function

The function reads top-level `.txt` sales files from the supplied directory. Each non-empty line in a sales file must be a number in invariant-culture format. It sums each file, sums the overall sales, and writes the currency-formatted report to the requested path. Invalid sales data and missing input directories are reported as exceptions.

```csharp
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
```
