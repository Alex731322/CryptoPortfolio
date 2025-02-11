using BitfinexConnector.Models;

namespace BitfinexConnector.Interfafaces
{
    public interface IRestClient
    {
        Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
        Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
        Task<Ticker> GetTickerAsync(string symbol);
    }
}
