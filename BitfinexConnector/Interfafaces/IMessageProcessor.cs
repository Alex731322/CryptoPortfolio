using BitfinexConnector.Models;

namespace BitfinexConnector.Interfafaces
{
    public interface IMessageProcessor
    {
        void ProcessMessage(string message);
        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        event Action<Candle> CandleSeriesProcessing;
    }
}
