using BitfinexConnector.Clients;

namespace BitfinexConnector.Wallet
{
    public class PortfolioService
    {
        private readonly BitfinexRestClient _restClient;

        public PortfolioService(BitfinexRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<List<PortfolioBalance>> CalculateBalancesAsync()
        {
            var holdings = new Dictionary<string, decimal>
            {
                { "BTC", 1m },
                { "XRP", 15000m },
                { "XMR", 50m },
            };

            var exchangeRates = new Dictionary<string, decimal>();

            exchangeRates["BTC"] = (await _restClient.GetTickerAsync("BTCUST")).LastPrice;
            exchangeRates["XRP"] = (await _restClient.GetTickerAsync("XRPUST")).LastPrice;
            exchangeRates["XMR"] = (await _restClient.GetTickerAsync("XMRUST")).LastPrice;

            decimal totalUsdt = holdings.Sum(s =>
                s.Value * exchangeRates[s.Key]);

            var balances = new List<PortfolioBalance>
            {
                new PortfolioBalance { Currency = "USDT", Balance = totalUsdt }
            };

            foreach (var rate in exchangeRates)
            {
                if (rate.Value == 0) continue; // Пропускаем нулевые курсы

                balances.Add(new PortfolioBalance
                {
                    Currency = rate.Key,
                    Balance = totalUsdt / rate.Value
                });
            }

            return balances;
        }
    }
}
