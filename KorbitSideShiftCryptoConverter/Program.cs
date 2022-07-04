// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;

using Newtonsoft.Json;

// How many milliseconds wait till next conversion
const int ConversionFrequencyMilliseconds = 10000;

// Global HTTP client used by every other method
HttpClient httpClient = new();
httpClient.Timeout = TimeSpan.FromMilliseconds(5000);


// Symbols mapping from Korbit exchange
// https://exchange.korbit.co.kr/faq/articles/?id=5SrSC3yggkWhcSL0O1KSz4
Dictionary<string, string> korbitSymbols = new(StringComparer.OrdinalIgnoreCase)
{
    { "BTC", "btc_krw" },
    { "BCH", "bch_krw" },
    { "XLM", "xlm_krw" },
    { "XRP", "xrp_krw" },
    { "ADA", "ada_krw" },
    { "SOL", "sol_krw" },
    { "FTM", "ftm_krw" },
    { "AXS", "axs_krw" },
    { "YFI", "yfi_krw" },
    { "DOGE", "doge_krw" },
    { "SHIB", "shib_krw" },
    { "COMP", "comp_krw" },
    { "AVAX", "avax_krw" },
    { "MATIC", "matic_krw" },

    { "DAI", "dai_krw" },
    { "USDC", "usdc_krw" },
};

// Korbit fees when you withdraw crypto from exchange
// https://exchange.korbit.co.kr/faq/articles/?id=5SrSC3yggkWhcSL0O1KSz4
Dictionary<string, decimal> korbitWithdrawalFees = new(StringComparer.OrdinalIgnoreCase)
{
    { "BTC", 0.001m },
    { "BCH", 0.001m },
    { "XLM", 0.01m },
    { "XRP", 1m },
    { "ADA", 0.5m },
    { "SOL", 0.01m },
    { "FTM", 10m },
    { "AXS", 0.7m },
    { "YFI", 0.001m },
    { "DOGE", 20m },
    { "SHIB", 200_000m },
    { "COMP", 0.07m },
    { "AVAX", 0.01m },
    { "MATIC", 15m },

    { "DAI", 20m },
    { "USDC", 28m },
};

// Symbols mapping from sideshift.ai exchange
// https://sideshift.ai/
Dictionary<string, string> sideShiftSymbols = new(StringComparer.OrdinalIgnoreCase)
{
    { "BTC", "btc-btc" },
    { "BCH", "bch-bch" },
    { "XLM", "xlm-xlm" },
    { "XRP", "xrp-xrp" },
    { "ADA", "ada-ada" },
    { "SOL", "sol-sol" },
    { "FTM", "ftm-ftm" },
    { "AXS", "axs-ethereum" },
    { "YFI", "yfi-ethereum" },
    { "DOGE", "doge-doge" },
    { "SHIB", "shib-ethereum" },
    { "COMP", "comp-ethereum" },
    { "AVAX", "avax-avax" },
    { "MATIC", "matic-ethereum" },

    { "DAI", "dai-ethereum" },
    { "USDC", "usdc-ethereum" },
};

// Check that both exchanges have same symbols
Trace.Assert(
    korbitSymbols.Keys.ToHashSet().SequenceEqual(sideShiftSymbols.Keys.ToHashSet())
);

// Check that for each symbol there is a corresponding withdrawal fee for Korbit exchange
Trace.Assert(
    korbitSymbols.Keys.ToHashSet().SequenceEqual(korbitWithdrawalFees.Keys.ToHashSet())
);

async Task<decimal> getKorbitPrice(string coinSymbol)
{
    // Korbit API
    // https://apidocs.korbit.co.kr/

    var korbitSymbol = korbitSymbols[coinSymbol];

    var korbitUrl = $"https://api.korbit.co.kr/v1/ticker/detailed?currency_pair={korbitSymbol}";
    var korbitResponse = await httpClient.GetAsync(korbitUrl);

    var korbitJson = await korbitResponse.Content.ReadAsStringAsync();
    KorbitTickerDetailed korbitTickerDetailed = JsonConvert.DeserializeObject<KorbitTickerDetailed>(korbitJson)!;
    Trace.Assert(korbitTickerDetailed is not null);

    return korbitTickerDetailed.Last;
}

async Task<IDictionary<string, decimal>> getKorbitPrices()
{
    var prices = new Dictionary<string, decimal>();

    var korbitUrl = "https://api.korbit.co.kr/v1/ticker/detailed/all";
    var korbitResponse = await httpClient.GetAsync(korbitUrl);

    var korbitJson = await korbitResponse.Content.ReadAsStringAsync();
    IDictionary<string, KorbitTickerDetailed> korbitTickerDetailedAll = JsonConvert.DeserializeObject<Dictionary<string, KorbitTickerDetailed>>(korbitJson)!;
    Trace.Assert(korbitTickerDetailedAll is not null);

    foreach ((var korbitSymbol, var korbitPair) in korbitSymbols)
    {
        if (!korbitTickerDetailedAll.TryGetValue(korbitPair, out var korbitTickerDetailed))
            throw new Exception($"Korbit API returned no data for {korbitSymbol}");

        prices[korbitSymbol] = korbitTickerDetailed.Last;
    }

    foreach (var coinSymbol in korbitSymbols.Keys)
    {
        var price = await getKorbitPrice(coinSymbol);
        prices[coinSymbol] = price;
    }

    return prices;
}

async Task<decimal> getSideShiftRate(string coinSymbolSrc, string coinSymbolDst)
{
    // sideshift.ai API
    // https://documenter.getpostman.com/view/6895769/TWDZGvjd#f7af3db9-0882-4a05-801e-ed4b85641f7f

    var sideShiftSymbolSrc = sideShiftSymbols[coinSymbolSrc];
    var sideShiftSymbolDst = sideShiftSymbols[coinSymbolDst];

    var sideShiftUrl = $"https://sideshift.ai/api/v2/pair/{sideShiftSymbolSrc}/{sideShiftSymbolDst}";
    var sideShiftResponse = await httpClient.GetAsync(sideShiftUrl);

    var sideShiftJson = await sideShiftResponse.Content.ReadAsStringAsync();
    SideShiftPair sideShiftPair = JsonConvert.DeserializeObject<SideShiftPair>(sideShiftJson)!;
    Trace.Assert(sideShiftPair is not null);

    return sideShiftPair.Rate;
}

async Task<decimal> getTargetCoinAmount(decimal money, string intermediateCoin, string targetCoin, decimal korbitPrice = 0)
{
    if (korbitPrice == 0)
        korbitPrice = await getKorbitPrice(intermediateCoin);

    var korbitFee = korbitWithdrawalFees[intermediateCoin];
    var sideShiftRate = await getSideShiftRate(intermediateCoin, targetCoin);

    var finalAmount = (money / korbitPrice - korbitFee) * sideShiftRate;

    return finalAmount;
}


async Task<IEnumerable<(string symbol, decimal amount)>> getAllTargetCoinAmounts(decimal money, string targetCoin)
{
    IDictionary<string, decimal> korbitPrices = await getKorbitPrices();

    return await Task.WhenAll(
        korbitSymbols.Keys
            .Where(symbol => symbol != targetCoin)
            .Select(async intermediateCoin => (
                symbol: intermediateCoin,
                amount: await getTargetCoinAmount(money, intermediateCoin, targetCoin, korbitPrices[intermediateCoin])
            ))
    ).ContinueWith(
        t => t.Result.OrderByDescending(
            sa => sa.amount
    ).AsEnumerable());
}

void printHorizontalLine()
{
    // -1 is to account for the trailing newline and prevent terminal from wrapping a line
    Console.WriteLine(new string('-', Console.WindowWidth - 1));
}

void printAvailableCoins()
{
    var availableCoinsStr = string.Join(", ", korbitSymbols.Keys.OrderBy(s => s));
    Console.WriteLine($"\nAvailable coins: {availableCoinsStr}");
}

// Input amount of Korean Won you want to convert to crypto
Console.Write("Please enter amount of money you are spending in KRW (default: 200,000 KRW): ");
var moneyStr = Console.ReadLine();
if (string.IsNullOrEmpty(moneyStr))
    moneyStr = "200,000";

var money = decimal.Parse(moneyStr, NumberStyles.Any);

printAvailableCoins();

// Coin you want to acquire at the end of all conversions
Console.Write("Please enter coin name (default: BTC): ");
var targetCoin = Console.ReadLine().ToUpper();
if (string.IsNullOrEmpty(targetCoin))
    targetCoin = "BTC";

Trace.Assert(sideShiftSymbols.ContainsKey(targetCoin));
Trace.Assert(korbitSymbols.ContainsKey(targetCoin));

Console.WriteLine("\nStarting conversion loop...\n");

// Infinite loop until Ctrl+C is pressed
for (var i = 1; ; i++)
{
    var delayTask = Task.Delay(ConversionFrequencyMilliseconds);
    var finalAmounts = await getAllTargetCoinAmounts(money, targetCoin);

    var korbitPrice = await getKorbitPrice(targetCoin);
    var korbitFee = korbitWithdrawalFees[targetCoin];

    // Print header
    printHorizontalLine();
    Console.WriteLine($"Conversion #{i} ({DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
    printHorizontalLine();

    Console.WriteLine($"Converting {money:N0} KRW into {targetCoin}\n");

    // Print price if you buy your coin directly from Korbit
    var korbitExchangeKeep = money / korbitPrice;
    var korbitExchangeWithdrawl = korbitExchangeKeep - korbitFee;
    Console.WriteLine($"Directly from Korbit (send to wallet) KRW -> {targetCoin} : {korbitExchangeWithdrawl} {targetCoin}");
    Console.WriteLine($"Directly from Korbit (keep on Korbit) KRW -> {targetCoin} : {korbitExchangeKeep} {targetCoin}\n");

    // Print all prices
    Console.WriteLine("sideshift.io");
    foreach ((var symbol, var amount) in finalAmounts)
    {
        var coinDiff = amount - korbitExchangeKeep;
        var krwDiff = coinDiff * korbitPrice;

        Console.WriteLine($"KRW -> {symbol,5} -> {targetCoin} : {amount} {targetCoin} (diff with Korbit: {krwDiff,7:N0} KRW)");
    }

    // Print best coin and how much of target coin you'll get if you convert through this coin
    printHorizontalLine();
    var best = finalAmounts.First();
    Console.WriteLine($"Best value: {best.symbol} -> {best.amount} {targetCoin}");

    // Print footer
    printHorizontalLine();

    // Wait for next iteration
    Console.WriteLine($"Waiting {ConversionFrequencyMilliseconds / 1000.0:N1} sec for next conversion...\n");
    await delayTask;
}