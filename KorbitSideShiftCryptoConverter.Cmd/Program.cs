// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;

using KorbitSideShiftCryptoConverter.Core;

// How many milliseconds wait till next conversion
const int ConversionFrequencyMilliseconds = 10_000;

// How much of KRW to convert
const decimal DefaultConversionAmount = 200_000;

// Global HTTP client used by APIs
HttpClient httpClient = new();

// Create APIs
KorbitAPI korbitApi = new(httpClient);
SideShiftAPI sideShiftApi = new(httpClient);

// Create converter
Converter converter = new(korbitApi, sideShiftApi);

// Check that both exchanges have same symbols available
Trace.Assert(
    korbitApi.Symbols.ToHashSet().SequenceEqual(sideShiftApi.Symbols.ToHashSet()),
    "Korbit and SideShift have different symbols available"
);

void printHorizontalLine()
{
    // -1 is to account for the trailing newline and prevent terminal from wrapping a line
    Console.WriteLine(new string('-', Console.WindowWidth - 1));
}

// Print program name and version
printHorizontalLine();
var assemblyName = System.Reflection.Assembly.GetEntryAssembly()!.GetName();
Console.WriteLine($"{assemblyName.Name} v{assemblyName.Version}");
printHorizontalLine();

// Input amount of Korean Won you want to convert to crypto
Console.Write($"\nPlease enter amount of money you are spending in KRW (default: {DefaultConversionAmount:N0} KRW): ");
var moneyStr = Console.ReadLine();
var money = !string.IsNullOrEmpty(moneyStr) ? decimal.Parse(moneyStr, NumberStyles.Any) : DefaultConversionAmount;

// Remove decimal part. You cannot send fraction of a won to the exchange
money = decimal.Floor(money);

Trace.Assert(money > 0, "Amount of money must be greater than 0");

// Print available coins
var availableCoinsStr = string.Join(", ", korbitApi.Symbols.OrderBy(s => s));
Console.WriteLine($"\nAvailable coins: {availableCoinsStr}");

// Coin you want to acquire at the end of all conversions
Console.Write("Please enter coin name (default: BTC): ");
var settleCoinStr = Console.ReadLine();

if (settleCoinStr is not null)
    settleCoinStr = settleCoinStr.Trim();

var settleCoin = default(Symbol);

if (string.IsNullOrEmpty(settleCoinStr))
    settleCoin = Symbol.BTC;
else
    settleCoin = Enum.Parse<Symbol>(settleCoinStr.ToUpper());

// Ensure that both exchanges have settle coin available
Trace.Assert(korbitApi.Symbols.Contains(settleCoin), $"Settle coin {settleCoin} is not available at Korbit");
Trace.Assert(sideShiftApi.Symbols.Contains(settleCoin), $"Settle coin {settleCoin} is not available at SideShift");

// Intermediate coins that you buy only to exchange for settle coin
var depositCoins = korbitApi.Symbols.Except(new[] { settleCoin });

Console.WriteLine("\nStarting conversion loop...\n");

// Infinite loop until Ctrl+C is pressed
for (var i = 1; ; i++)
{
    // Get prices and rates in parallel
    var korbitPricesTask = korbitApi.GetPrices();
    var sideShiftRatesTask = sideShiftApi.GetRates(settleCoin);
    Task.WaitAll(korbitPricesTask, sideShiftRatesTask);

    var korbitPrices = korbitPricesTask.Result;
    var sideShiftRates = sideShiftRatesTask.Result;

    // Print header
    printHorizontalLine();
    Console.WriteLine($"Conversion #{i} ({DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
    printHorizontalLine();

    Console.WriteLine($"Converting {money:N0} KRW into {settleCoin}");
    Console.WriteLine();

    // Print price if you buy your coin directly from Korbit
    var korbitPrice = korbitPrices[settleCoin];
    var korbitFee = korbitApi.WithdrawalFees[settleCoin];
    var korbitExchangeKeep = money / korbitPrice;
    var korbitExchangeSend = korbitExchangeKeep - korbitFee;

    Console.WriteLine("korbit.co.kr");
    Console.WriteLine($"Keep on Korbit: KRW -> {settleCoin} : {korbitExchangeKeep:N10} {settleCoin}");
    Console.WriteLine($"Send to wallet: KRW -> {settleCoin} : {korbitExchangeSend:N10} {settleCoin}");
    Console.WriteLine();

    // Convert each deposit coin to settle coin and order by amount of settle coin you get
    var finalAmounts = converter
        .Convert(money, depositCoins, settleCoin, korbitPrices, sideShiftRates)
        .OrderByDescending(kvp => kvp.coinAmount);

    // Print each conversion
    Console.WriteLine("sideshift.io");

    foreach ((var symbol, var amount) in finalAmounts)
    {
        var coinDiff = amount - korbitExchangeKeep;
        var krwDiff = coinDiff * korbitPrice;

        Console.WriteLine($"KRW -> {symbol,5} -> {settleCoin} : {amount:N10} {settleCoin} (diff with keeping on Korbit: {krwDiff,7:N0} KRW)");
    }

    // Print best coin and how much of target coin you'll get if you convert through this coin
    printHorizontalLine();
    var best = finalAmounts.First();
    Console.WriteLine($"Best value: {best.depositCoin} -> {best.coinAmount:N10} {settleCoin}");

    // Print footer
    printHorizontalLine();

    // Wait for next iteration to reduce load on public APIs
    Console.WriteLine($"Waiting {ConversionFrequencyMilliseconds / 1000.0:N1} sec for next conversion...");
    Console.WriteLine();
    Task.Delay(ConversionFrequencyMilliseconds).Wait();
}