using BitfinexConnector.Clients;
using BitfinexConnector.Models;

namespace BitfinexConnector.Services
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
            var cryptoWallet = new Dictionary<string, decimal>
            {
                { "BTC", 1 },
                { "XRP", 15000 },
                { "XMR", 50 },
                { "DSH", 30 }
            };

            var cryptoCurrencyRates = new Dictionary<string, decimal>();

            cryptoCurrencyRates["BTC"] = (await _restClient.GetTickerAsync("BTCUSD")).LastPrice;
            cryptoCurrencyRates["XRP"] = (await _restClient.GetTickerAsync("XRPUSD")).LastPrice;
            cryptoCurrencyRates["XMR"] = (await _restClient.GetTickerAsync("XMRUSD")).LastPrice;
            cryptoCurrencyRates["DSH"] = (await _restClient.GetTickerAsync("DSHUSD")).LastPrice;

            decimal totalUsdt = cryptoWallet.Sum(s => s.Value * cryptoCurrencyRates[s.Key]);

            var balances = new List<PortfolioBalance>
            {
                new PortfolioBalance { Currency = "USDT", Balance = totalUsdt }
            };

            foreach (var coin in cryptoCurrencyRates)
            {
                balances.Add(new PortfolioBalance
                {
                    Currency = coin.Key,
                    Balance = totalUsdt / coin.Value
                });
            }

            return balances;
        }
    }
}
