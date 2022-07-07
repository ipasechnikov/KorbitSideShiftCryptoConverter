namespace KorbitSideShiftCryptoConverter.Common
{
    public class Converter
    {
        private readonly KorbitAPI _korbitApi;
        private readonly SideShiftAPI _sideShiftApi;

        public Converter(KorbitAPI korbitApi, SideShiftAPI sideShiftApi)
        {
            _korbitApi = korbitApi;
            _sideShiftApi = sideShiftApi;
        }

        public IEnumerable<(Symbol depositCoin, decimal coinAmount)> Convert(
            decimal money, IEnumerable<Symbol> depositCoins, Symbol settleCoin, IDictionary<Symbol, decimal> korbitPrices, IDictionary<Symbol, decimal> sideShiftRates
        )
        {
            return depositCoins
                .Where(symbol => symbol != settleCoin)
                .Select(depositCoin => (
                    depositCoin: depositCoin,
                    coinAmount: Convert(money, korbitPrices[depositCoin], _korbitApi.WithdrawalFees[depositCoin], sideShiftRates[depositCoin])
                ))
                .AsEnumerable();
        }

        public async Task<IEnumerable<(Symbol depositCoin, decimal coinAmount)>> Convert(decimal money, IEnumerable<Symbol> depositCoins, Symbol settleCoin)
        {
            var korbitPricesTask = _korbitApi.GetPrices();
            var sideShiftRatesTask = _sideShiftApi.GetRates(settleCoin);
            await Task.WhenAll(korbitPricesTask, sideShiftRatesTask);

            IDictionary<Symbol, decimal> korbitPrices = korbitPricesTask.Result;
            IDictionary<Symbol, decimal> sideShiftRates = sideShiftRatesTask.Result;

            return Convert(money, depositCoins, settleCoin, korbitPrices, sideShiftRates);
        }

        private decimal Convert(decimal money, decimal korbitPrice, decimal korbitWithdrawalFee, decimal sideShiftRate)
        {
            return (money / korbitPrice - korbitWithdrawalFee) * sideShiftRate;
        }
    }
}
