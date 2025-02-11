using BitfinexConnector.Interfafaces;
using BitfinexConnector.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BitfinexConnector.Services
{
    public class BitfinexMessageService : IMessageProcessor
    {
        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        private readonly Dictionary<int, ChannelInfo> _channelMap;

        public BitfinexMessageService()
        {
            _channelMap = new Dictionary<int, ChannelInfo>();
        }

        public void ProcessMessage(string message)
        {
            try
            {
                var json = JToken.Parse(message);

                if (json is JObject && json["event"]?.ToString() == "subscribed")
                {
                    var channelId = json["chanId"].Value<int>();

                    var pair = json["symbol"]?
                                .ToString()
                                .TrimStart('t')
                                ?? json["key"]
                                .ToString()
                                .Split(':')
                                .Last()
                                .TrimStart('t');

                    _channelMap[channelId] = new ChannelInfo
                    {
                        Pair = pair,
                        ChannelType = json["channel"].ToString()
                    };
                }
                else if (json is JArray array)
                {
                    var channelId = array[0].Value<int>();

                    if (_channelMap.TryGetValue(channelId, out var channelInfo))
                    {
                        if (channelInfo.ChannelType == "candles")
                            HandleCandleMessage(message);
                        else if (channelInfo.ChannelType == "trades")
                            HandleTradeMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void HandleTradeMessage(string message)
        {
            var data = JsonConvert.DeserializeObject<JArray>(message);

            if (data[1].ToString() == "te")
            {
                var tradePayload = data[2] as JArray;

                var trade = new Trade
                {
                    Id = tradePayload[0].Value<string>(),
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(tradePayload[1].Value<long>()),
                    Amount = tradePayload[2].Value<decimal>(),
                    Price = tradePayload[3].Value<decimal>(),
                    Pair = "BTCUSD",
                    Side = tradePayload[2].Value<decimal>() > 0 ? "buy" : "sell"
                };

                if (trade.Amount > 0)
                    NewBuyTrade?.Invoke(trade);
                else
                    NewSellTrade?.Invoke(trade);
            }
        }

        private void HandleCandleMessage(string message)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<JArray>(message);
                var channelId = data[0].Value<int>();

                if (!_channelMap.TryGetValue(channelId, out var channelInfo))
                    return;

                if (data[1] is JArray candlesData)
                {
                    foreach (var candleItem in candlesData)
                    {
                        ProcessSingleCandle(candleItem as JArray, channelInfo.Pair);
                    }
                }
                else if (data[1].Type == JTokenType.String)
                {
                    var updateType = data[1].Value<string>();
                    if (updateType == "hu" && data[2] is JArray candleData)
                    {
                        ProcessSingleCandle(candleData, channelInfo.Pair);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ProcessSingleCandle(JArray candleData, string pair)
        {
            if (candleData == null || candleData.Count < 6)
                return;

            var candle = new Candle
            {
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(candleData[0].Value<long>()),
                OpenPrice = candleData[1].Value<decimal>(),
                ClosePrice = candleData[2].Value<decimal>(),
                HighPrice = candleData[3].Value<decimal>(),
                LowPrice = candleData[4].Value<decimal>(),
                TotalVolume = candleData[5].Value<decimal>(),
                Pair = pair
            };

            CandleSeriesProcessing?.Invoke(candle);
        }

    }
}
