using BitfinexConnector.Clients;
using BitfinexConnector.Models;
using BitfinexConnector.Processors;
using Serilog;

namespace BitfinexConnector.Services
{
    public class BitfinexDataService
    {
        private readonly BitfinexRestClient _restClient;
        private readonly BitfinexWebSocketClient _webScocket;
        private readonly BitfinexSubscriptionService _subscriptionService;
        private readonly BitfinexMessageProcessor _messageProcessor;

        public BitfinexDataService(
            BitfinexRestClient restClient,
            BitfinexWebSocketClient webSocket,
            BitfinexSubscriptionService subscriptionService,
            BitfinexMessageProcessor messageProcessor)
        {
            _restClient = restClient;
            _webScocket = webSocket;
            _subscriptionService = subscriptionService;
            _messageProcessor = messageProcessor;
        }

        public BitfinexMessageProcessor MessageProcessor => _messageProcessor;

        public async Task<Ticker> GetTickerAsync(string pair)
        {
            try
            {
                return await _restClient.GetTickerAsync(pair);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured in getting ticket");
                throw;
            }
        }

        public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int period)
        {
            try
            {
                return await _restClient.GetNewTradesAsync(pair, period);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured in getting Trades");
                throw;
            }
        }


        public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int period)
        {
            try
            {
                return await _restClient.GetCandleSeriesAsync(
                                pair,
                                period,
                                DateTimeOffset.UtcNow.AddHours(-1),
                                DateTimeOffset.UtcNow,
                                100);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured in getting Candles");
                throw;
            }
        }

        public async Task ConectWebSocketAsync()
            => await _webScocket.ConnectAsync();

        public void SubscribeTrades(string pair)
            => _subscriptionService.SubscribeTrades(pair);

        public void SubscribeCandles(string pair, int period)
            => _subscriptionService.SubscribeCandles(pair, period);

    }
}
