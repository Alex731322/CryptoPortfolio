using BitfinexApp.Client.ViewModels;
using BitfinexConnector.Clients;
using BitfinexConnector.Processors;
using BitfinexConnector.Services;
using System.Windows;

namespace BitfinexApp.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var restClient = new BitfinexRestClient();
            var messageProcessor = new BitfinexMessageProcessor();
            var websocketClient = new BitfinexWebSocketClient(messageProcessor);
            var subscriptionService = new BitfinexSubscriptionService(websocketClient, messageProcessor);
            
            var dataService = new BitfinexDataService(restClient, websocketClient, subscriptionService, messageProcessor);
            var portfolioService = new PortfolioService(restClient);

            DataContext = new MainViewModel(dataService, portfolioService);
        }
    }
}