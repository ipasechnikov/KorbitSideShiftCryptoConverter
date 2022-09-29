using System.Diagnostics;

using Newtonsoft.Json;

namespace KorbitSideShiftCryptoConverter.Core
{
    public class SideShiftAPI
    {
        // sideshift.ai API
        // https://documenter.getpostman.com/view/6895769/TWDZGvjd#f7af3db9-0882-4a05-801e-ed4b85641f7f

        private readonly HttpClient _httpClient;
        private readonly IDictionary<Symbol, string> _coins = new Dictionary<Symbol, string>()
        {
            { Symbol.ADA, "ada-ada" },
            { Symbol.BCH, "bch-bch" },
            { Symbol.BTC, "btc-btc" },
            { Symbol.DAI, "dai-ethereum" },
            { Symbol.ETH, "eth-ethereum" },
            { Symbol.SOL, "sol-sol" },
            { Symbol.USDC, "usdc-ethereum" },
            { Symbol.XLM, "xlm-xlm" },
            { Symbol.XRP, "xrp-xrp" },
        };

        public SideShiftAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IEnumerable<Symbol> Symbols => _coins.Keys;

        public async Task<decimal> GetRate(Symbol depositCoin, Symbol settleCoin)
        {
            var apiDepositCoin = _coins[depositCoin];
            var apiSettleCoin = _coins[settleCoin];

            var sideShiftUrl = $"https://sideshift.ai/api/v2/pair/{apiDepositCoin}/{apiSettleCoin}";
            var response = await _httpClient.GetAsync(sideShiftUrl);
            var responseJson = await response.Content.ReadAsStringAsync();

            SideShiftPair pair = JsonConvert.DeserializeObject<SideShiftPair>(responseJson)!;

            // Exchange rate cannot be zero. Having a zero exchange rate means that there is a bug in our program
            Trace.Assert(pair.Rate != 0);

            return pair.Rate;
        }

        public async Task<IDictionary<Symbol, decimal>> GetRates(Symbol settleCoin)
        {
            return await Task.WhenAll(
                _coins
                    .Where(kv => kv.Key != settleCoin)
                    .Select(async kv => (
                        depositCoin: kv.Key,
                        rate: await GetRate(kv.Key, settleCoin)
                    ))
            ).ContinueWith(t => t.Result.ToDictionary(
                t => t.depositCoin,
                t => t.rate)
            );
        }
    }
}
