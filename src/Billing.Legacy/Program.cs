using System.Text.Json;

var inputPath = args.FirstOrDefault() ?? Path.Combine("samples", "input-sample.json");
var json = await File.ReadAllTextAsync(inputPath);
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var payload = JsonSerializer.Deserialize<Root>(json,options) ?? new Root();


Console.WriteLine($"BILLING PERIOD: {payload.BillingPeriod}\n");

foreach (var c in payload.Customers ?? new())
{
    // Wszystko w jednym miejscu: zasady cen, podatki, rabaty, raportowanie
    decimal price = 0;

    // Plan
    if (c.Plan == "Basic")
    {
        price = 29m; // abonament
        price += c.Usage * 0.10m; // nadwyżka
        if (c.Usage > 500) price *= 0.95m; // "magiczny" rabat
    }
    else if (c.Plan == "Pro")
    {
        price = 59m;
        var extra = c.Usage - 200;
        if (extra > 0) price += extra * 0.07m;
    }
    else if (c.Plan == "Enterprise")
    {
        price = 199m;
        price += Math.Max(0, c.Usage - 5000) * 0.03m;
        if (c.Usage == 0) price *= 0.9m; // dziwna zniżka
    }
    else
    {
        Console.WriteLine($"WARN: Unknown plan '{c.Plan}', defaulting to Basic");
        price = 29m + c.Usage * 0.10m;
    }

    // Podatek (twardo zakodowany)
    price = price * 1.23m; // 23% VAT

    // Formatowanie raportu (też tutaj)
    Console.WriteLine($"{c.Id};{c.Plan};{c.Usage};{price:F2}");
}
