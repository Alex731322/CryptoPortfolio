using BitfinexConnector.Constants;
using BitfinexConnector.Converters;
using BitfinexConnector.Interfafaces;
using BitfinexConnector.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BitfinexConnector.Processors
{
    public class BitfinexMessageProcessor : IMessageProcessor
    {
        private readonly Dictionary<int, ChannelInfo> _channelMap = new();

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public void ProcessMessage(string message)
        {
            try
            {
                var json = JToken.Parse(message);

                if (json is JObject obj && obj["event"]?.ToString() == BitfinexProcessConstants.EventSubscribed)
                    HandleSubscription(obj);

                if (json is JArray array)
                    HandleDataArray(array);
            }
            catch (Exception ex)
            {
                Log.Error("Error with handling message");
            }
        }

        private void HandleSubscription(JObject message)
        {
            var channelId = message["chanId"].Value<int>();
            var pair = message["symbol"]?
                       .ToString() ??
                       message["key"]?
                       .ToString()
                       .Split(':')[2];

            pair = pair.TrimStart('t', 'f');
            _channelMap[channelId] = new ChannelInfo
            {
                Pair = pair,
                ChannelType = message["channel"].ToString()
            };
        }

        private void HandleDataArray(JArray data)
        {
            var channelId = data[0].Value<int>();

            if (!_channelMap.TryGetValue(channelId, out var channel)) return;

            if (data[1].ToString() == BitfinexProcessConstants.HeartbeatMessageType) return;

            switch (channel.ChannelType)
            {
                case BitfinexProcessConstants.ChannelTypeTrades:
                    HandleTrade(data, channel.Pair);
                    break;
                case BitfinexProcessConstants.ChannelTypeCandles:
                    HandleCandle(data, channel.Pair);
                    break;
            }
        }

        private void HandleTrade(JArray data, string pair)
        {
            if (data[1].ToString() != BitfinexProcessConstants.TradeUpdateType) 
                return;

            var tradeData = data[2] as JArray;

            try
            {
                var converter = new TradeConverter(pair);
                using var reader = tradeData.CreateReader();
                var trade = converter.ReadJson(reader, typeof(Trade), null, JsonSerializer.CreateDefault()) as Trade;

                    if (trade.Amount > 0)
                        NewBuyTrade?.Invoke(trade);
                    else
                        NewSellTrade?.Invoke(trade);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error with handling trade");
            }
        }

        private void HandleCandle(JArray data, string pair)
        {
            try
            {
                var candleData = data[1] is JArray arr ? arr : data[2] as JArray;
                var converter = new CandleConverter(pair);
                using var reader = candleData.CreateReader();
                var candle = converter.ReadJson(reader, typeof(Candle), null, JsonSerializer.CreateDefault()) as Candle;

                CandleSeriesProcessing?.Invoke(candle);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error with handling candle");
            }
        }
    }
}