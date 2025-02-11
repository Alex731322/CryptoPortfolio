using BitfinexConnector.Clients;
using BitfinexConnector.Models;
using BitfinexConnector.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BitfinexApp.Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private readonly BitfinexRestClient _restClient;
        private readonly BitfinexWebSocketClient _websocketClient;
        private readonly BitfinexMessageService _bitfinexMessageService;
        private readonly BitfinexSubscriptionService _subscriptionService;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Trade> _trades;
        public ObservableCollection<Trade> Trades
        {
            get => _trades;
            set => SetField(ref _trades, value);
        }

        private ObservableCollection<Candle> _candles;
        public ObservableCollection<Candle> Candles
        {
            get => _candles;
            set => SetField(ref _candles, value);
        }

        public MainWindow()
        {
            this.DataContext = this;

            _restClient = new BitfinexRestClient();
            _bitfinexMessageService = new BitfinexMessageService();

            _websocketClient = new BitfinexWebSocketClient(_bitfinexMessageService);
            _subscriptionService = new BitfinexSubscriptionService(_websocketClient, _bitfinexMessageService);

            _bitfinexMessageService.NewSellTrade += OnNewSellTrade;
            _bitfinexMessageService.NewBuyTrade += OnNewBuyTrade;
            _bitfinexMessageService.CandleSeriesProcessing += OnCandleSeriesProcessing;

            this.Loaded += MainWindow_Loaded;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ConnectWebSocketAsync();
        }
        private async void GetTickerButton_Click(object sender, RoutedEventArgs e)
        {
            var ticker = await _restClient.GetTickerAsync("BTCUSD");
            TickerOutput.Text =
                $"Bid: {ticker.Bid} Size: {ticker.BidSize}\n" +
                $"Ask: {ticker.Ask} Size: {ticker.AskSize}\n" +
                $"Last Price: {ticker.LastPrice}\n" +
                $"Daily Change: {ticker.DailyChange} ({ticker.DailyChangePercent:P})\n" +
                $"Volume: {ticker.Volume}\n" +
                $"High: {ticker.High}\n" +
                $"Low: {ticker.Low}\n";
        }

        private async void GetTradesButton_Click(object sender, RoutedEventArgs e)
        {
            var trades = await _restClient.GetNewTradesAsync("BTCUSD", 50);
            Trades = new ObservableCollection<Trade>(trades);
        }

        private async void GetCandlesButton_Click(object sender, RoutedEventArgs e)
        {
            var candles = await _restClient.GetCandleSeriesAsync(
                "BTCUSD",
                60,
                DateTimeOffset.UtcNow.AddHours(-1),
                DateTimeOffset.UtcNow,
                100
            );
            Candles = new ObservableCollection<Candle>(candles);
        }

        private async Task ConnectWebSocketAsync()
        {
            await _websocketClient.ConnectAsync();
            _subscriptionService.SubscribeTrades("BTCUSD");
            _subscriptionService.SubscribeCandles("BTCUSD", 60);
        }

        private void OnNewBuyTrade(Trade trade)
        {
            Dispatcher.Invoke(() =>
            {
                 Trades.Add(trade);
            });
        }

        private void OnNewSellTrade(Trade trade)
        {
            Dispatcher.Invoke(() =>
            {
                Trades.Add(trade);
            }); 
        }

        private void OnCandleSeriesProcessing(Candle candle)
        {
            Dispatcher.Invoke(() =>
            {
                Candles.Add(candle);
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _restClient.Dispose();
        }
    }
}
