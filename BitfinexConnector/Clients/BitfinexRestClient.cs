using BitfinexConnector.Converters;
using BitfinexConnector.Interfafaces;
using BitfinexConnector.Models;
using Newtonsoft.Json;
using System.Text;

namespace BitfinexConnector.Clients
{
    public class BitfinexRestClient : IDisposable, IRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _uri = new("https://api.bitfinex.com/v2/");

        public BitfinexRestClient()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = _uri
            };
        }

        public async Task<Ticker> GetTickerAsync(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Symbol is required");
            
            var endpoint = $"ticker/t{symbol}";
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
             
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Ticker>(jsonData, new TickerConverter());
        }

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            if (string.IsNullOrEmpty(pair))
                throw new ArgumentException($"Pair must be provided");

            if (maxCount > 100)
                throw new ArgumentException($"{nameof(maxCount)} cannot to be more than 100");

            var endpoint = $"trades/t{pair}/hist?limit={maxCount}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var jsonData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<Trade>>(jsonData, new TradeConverter(pair));
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            if (string.IsNullOrEmpty(pair))
                throw new ArgumentException($"Pair must be provided");

            var timeframe = TimeframeConverter.ConvertToTimeframe(periodInSec);
            var endpoint = new StringBuilder($"candles/trade:{timeframe}:t{pair}/hist?limit={count}");

            if (from.HasValue)
                endpoint.Append($"&start={from.Value.ToUnixTimeMilliseconds()}");

            if (to.HasValue)
                endpoint.Append($"&end={to.Value.ToUnixTimeMilliseconds()}");

            var response = await _httpClient.GetAsync(endpoint.ToString());
            response.EnsureSuccessStatusCode();

            var jsonData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<Candle>>(jsonData, new CandleConverter(pair));
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
