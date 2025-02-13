using BitfinexConnector.Models;
using BitfinexConnector.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BitfinexApp.Client.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly BitfinexDataService _bitfinexDataService;
        private readonly PortfolioService _portfolioService;

        public ObservableCollection<Trade> Trades { get; } = new();
        public ObservableCollection<Candle> Candles { get; } = new();
        public ObservableCollection<PortfolioBalance> PortfolioBalances { get; } = new();

        private string _tickerInfo;
        public string TickerInfo
        {
            get => _tickerInfo;
            set => SetField(ref _tickerInfo, value);
        }

        public MainViewModel(BitfinexDataService bitfinexDataService, PortfolioService portfolioService)
        {
            _bitfinexDataService = bitfinexDataService;
            _portfolioService = portfolioService;

            var messageProcessor = _bitfinexDataService.MessageProcessor;
            messageProcessor.NewSellTrade += OnNewSellTrade;
            messageProcessor.NewBuyTrade += OnNewBuyTrade;
            messageProcessor.CandleSeriesProcessing += OnCandleSeriesProcessing;

            InitializeDataAsync().ConfigureAwait(false);
        }

        private async Task InitializeDataAsync()
        {
            await LoadInitialDataAsync();
            await ConnectWebSocketAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var ticker = await _bitfinexDataService.GetTickerAsync("BTCUSD");
                TickerInfo = FormatTickerInfo(ticker);

                var trades = await _bitfinexDataService.GetTradesAsync("BTCUSD", 50);
                foreach (var trade in trades)
                {
                    Trades.Add(trade);
                }

                var candles = await _bitfinexDataService.GetCandlesAsync("BTCUSD", 60);
                foreach (var candle in candles)
                {
                    Candles.Add(candle);
                }

                var balances = await _portfolioService.CalculateBalancesAsync();
                foreach (var balance in balances)
                {
                    PortfolioBalances.Add(balance);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Arguemt has some troubles {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown error: {ex.Message}");
            }
        }

        private async Task ConnectWebSocketAsync()
        {
            await _bitfinexDataService.ConectWebSocketAsync();
            _bitfinexDataService.SubscribeTrades("BTCUSD");
            _bitfinexDataService.SubscribeCandles("BTCUSD", 60);
        }
        private string FormatTickerInfo(Ticker ticker)
        {
            return $@"
                      Bid: {ticker.Bid} Size: {ticker.BidSize}
                      Ask: {ticker.Ask} Size: {ticker.AskSize}
                      Last Price: {ticker.LastPrice}
                      Daily Change: {ticker.DailyChange} - {ticker.DailyChangePercent}
                      Volume: {ticker.Volume}
                      High: {ticker.High}
                      Low: {ticker.Low}";
        }

        private async Task LoadPortfolioDataAsync()
        {
            var balances = await _portfolioService.CalculateBalancesAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                PortfolioBalances.Clear();
                foreach (var balance in balances)
                {
                    PortfolioBalances.Add(balance);
                }
            });
        }

        public void OnNewBuyTrade(Trade trade)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Trades.Add(trade);
            });
        }

        public void OnNewSellTrade(Trade trade)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Trades.Add(trade);
            });
        }

        public void OnCandleSeriesProcessing(Candle candle)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Candles.Add(candle);
            });
        }

        #region NotifyPropert
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
