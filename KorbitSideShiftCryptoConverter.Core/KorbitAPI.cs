using System.Diagnostics;

using Newtonsoft.Json;

namespace KorbitSideShiftCryptoConverter.Core
{
    public class KorbitAPI
    {
        // Korbit API
        // https://apidocs.korbit.co.kr/

        private readonly HttpClient _httpClient;
        private readonly IDictionary<Symbol, string> _currencyPairs = new Dictionary<Symbol, string>()
        {
            { Symbol.ADA, "ada_krw" },
            { Symbol.BCH, "bch_krw" },
            { Symbol.BTC, "btc_krw" },
            { Symbol.DAI, "dai_krw" },
            { Symbol.ETH, "eth_krw" },
            { Symbol.SOL, "sol_krw" },
            { Symbol.USDC, "usdc_krw" },
            { Symbol.XLM, "xlm_krw" },
            { Symbol.XRP, "xrp_krw" },
        };

        public KorbitAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Check that for each symbol there is a corresponding withdrawal fee
            Trace.Assert(
                _currencyPairs.Keys.ToHashSet().SequenceEqual(WithdrawalFees.Keys.ToHashSet())
            );
        }

        public IEnumerable<Symbol> Symbols => _currencyPairs.Keys;

        public IDictionary<Symbol, decimal> WithdrawalFees { get; } = new Dictionary<Symbol, decimal>()
        {
            { Symbol.ADA, 0.5m },
            { Symbol.BCH, 0.001m },
            { Symbol.BTC, 0.001m },
            { Symbol.DAI, 20m },
            { Symbol.ETH, 0.009m },
            { Symbol.SOL, 0.01m },
            { Symbol.USDC, 28m },
            { Symbol.XLM, 0.01m },
            { Symbol.XRP, 1m },
        };

        public async Task<IDictionary<Symbol, decimal>> GetPrices()
        {
            var response = await _httpClient.GetAsync("https://api.korbit.co.kr/v1/ticker/detailed/all");
            var responseJson = await response.Content.ReadAsStringAsync();
            IDictionary<string, KorbitTickerDetailed> tickerDetailedAll = JsonConvert.DeserializeObject<Dictionary<string, KorbitTickerDetailed>>(responseJson)!;

            var prices = new Dictionary<Symbol, decimal>();

            foreach ((var coinSymbol, var currencyPair) in _currencyPairs)
            {
                if (!tickerDetailedAll.TryGetValue(currencyPair, out var tickerDetailed))
                    throw new Exception($"Korbit API returned no data for {coinSymbol}");

                prices[coinSymbol] = tickerDetailed.Last;
            }

            return prices;
        }
    }
}
