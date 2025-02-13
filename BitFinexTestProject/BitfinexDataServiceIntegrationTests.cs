using BitfinexConnector.Clients;
using BitfinexConnector.Processors;
using BitfinexConnector.Services;

namespace BitFinexTestProject
{
    public class Tests
    {
        private BitfinexRestClient _restClient;
        private BitfinexMessageProcessor _messageProcessor;
        private BitfinexWebSocketClient _websocketClient;
        private BitfinexSubscriptionService _subscriptionService;
        private BitfinexDataService _dataService;

        [SetUp]
        public void Setup()
        {
            _restClient = new BitfinexRestClient();
            _messageProcessor = new BitfinexMessageProcessor();
            _websocketClient = new BitfinexWebSocketClient(_messageProcessor);
            _subscriptionService = new BitfinexSubscriptionService(_websocketClient, _messageProcessor);
            _dataService = new BitfinexDataService(_restClient, _websocketClient, _subscriptionService, _messageProcessor);
        }

        [Test]
        public async Task BitfinexDataService_SubscribeToTrades_Should_Take_SellTrade_In_One_Minute()
        {
            //Arrange
            var received = false;
            _messageProcessor.NewSellTrade += _ => received = true;

            // Act
            _dataService.SubscribeTrades("BTCUSD");
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Assert
            Assert.IsTrue(received);
        }

        [Test]
        public async Task BitfinexDataService_SubscribeToTrades_Should_Take_BuyTrade_In_One_Minute()
        {
            // Arrange
            var received = false;
            _messageProcessor.NewBuyTrade += _ => received = true;

            // Act
            _dataService.SubscribeTrades("BTCUSD");
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Assert
            Assert.IsTrue(received);
        }

        [Test]
        public async Task BitfinexDataService_SubscribeToCandles_Should_Take_Candle_In_One_Minute()
        {
            // Arrange
            var received = false;
            _messageProcessor.CandleSeriesProcessing += _ => received = true;

            // Act
            _dataService.SubscribeCandles("BTCUSD", 60);
            await Task.Delay(TimeSpan.FromMinutes(1));

            // Assert
            Assert.IsTrue(received);
        }

        [TearDown]
        public void TearDown()
        {
            _restClient.Dispose();
            _websocketClient.Dispose();
        }
    }
}