using BitfinexConnector.Converters;
using BitfinexConnector.Interfafaces;

namespace BitfinexConnector.Services
{
    public class BitfinexSubscriptionService : IExchangeSubscribe
    {
        private readonly BitfinexWebSocketClient _bitfinexWebSocketClient;
        private readonly IMessageProcessor _messageProcessor;

        public BitfinexSubscriptionService(BitfinexWebSocketClient bitfinexWebSocketClient, IMessageProcessor messageProcessor)
        {
            _bitfinexWebSocketClient = bitfinexWebSocketClient;
            _messageProcessor = messageProcessor;
        }

        public async Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            var timeframe = TimeframeConverter.ConvertToTimeframe(periodInSec);
            var message = $@"
            {{
                ""event"":""subscribe"",
                ""channel"":""candles"",
                ""key"":""trade:{timeframe}:t{pair}""
            }}";
            await _bitfinexWebSocketClient.SendMessageAsync(message);
        }

        public async Task SubscribeTrades(string pair, int maxCount = 100)
        {
            var message = $@"
            {{
                ""event"":""subscribe"",
                ""channel"":""trades"",
                ""symbol"":""t{pair}""
            }}";
            await _bitfinexWebSocketClient.SendMessageAsync(message);
        }

        public async Task UnsubscribeCandles(string pair)
        {
            throw new NotImplementedException();
        }

        public async Task UnsubscribeTrades(string pair)
        {
            throw new NotImplementedException();
        }
    }
}
