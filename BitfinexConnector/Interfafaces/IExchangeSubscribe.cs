namespace BitfinexConnector.Interfafaces
{
    public interface IExchangeSubscribe
    {
        Task SubscribeTrades(string pair, int maxCount = 100);
        Task UnsubscribeTrades(string pair);
        Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        Task UnsubscribeCandles(string pair);
    }
}
