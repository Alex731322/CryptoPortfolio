using BitfinexConnector.Services;
using System.Net.WebSockets;
using System.Text;

public class BitfinexWebSocketClient : IDisposable
{
    private readonly ClientWebSocket _clientWebSocket = new();
    private readonly Uri _uri;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly BitfinexMessageService _bitfinexMessageService;
    public BitfinexWebSocketClient(BitfinexMessageService bitfinexMessageService)
    {
        _uri = new("wss://api-pub.bitfinex.com/ws/2");
        _bitfinexMessageService = bitfinexMessageService;
    }

    public async Task ConnectAsync()
    {
        if (_clientWebSocket.State == WebSocketState.Open)
            return;

        await _clientWebSocket.ConnectAsync(
                _uri,
                _cancellationTokenSource.Token);

        _ = Task.Run(() => ReceiveAsync());
    }

    public async Task ReceiveAsync()
    {
        var buffer = new byte[8192];

        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                _cancellationTokenSource.Token);

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            _bitfinexMessageService.ProcessMessage(message);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_clientWebSocket.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

        var bytes = Encoding.UTF8.GetBytes(message);

        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            _cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _clientWebSocket?.Dispose();
        _cancellationTokenSource?.Cancel();
    }
}



/*using BitfinexConnector.Converters;
using BitfinexConnector.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

public class BitfinexWebSocketClient : IDisposable
{
    private ClientWebSocket _clientWebSocket;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, Action<string>> _channelHandlers = new();
    private readonly Uri _uri;

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    public event Action<Candle> CandleSeriesProcessing;


    private readonly Dictionary<int, ChannelInfo> _channelMap = new();

    public BitfinexWebSocketClient()
    {
        _uri = new("wss://api-pub.bitfinex.com/ws/2");
        _clientWebSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ConnectAsync()
    {
        if (_clientWebSocket.State == WebSocketState.Open)
            return;

        await _clientWebSocket.ConnectAsync(
            _uri,
            _cancellationTokenSource.Token);

        _ = Task.Run(() => ReceiveAsync());
    }

    public async Task SendMessageAsync(string message)
    {
        if (_clientWebSocket.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

        var bytes = Encoding.UTF8.GetBytes(message);
        
        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            _cancellationTokenSource.Token);
    }

    public async Task ReceiveAsync()
    {
        var buffer = new byte[8192];

        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                _cancellationTokenSource.Token);

            if(result.MessageType == WebSocketMessageType.Close)
                break; 

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            ProcessMessage(message);
        }
    }

    public async Task SubscribeTrades(string pair, int maxCount = 100)
    {
        var message = $@"
        {{
            ""event"": ""subscribe"",
            ""channel"": ""trades"",
            ""symbol"": ""t{pair}""
        }}";

        await SendMessageAsync(message);
    }

    public async Task SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
    {
        var timeframe = TimeframeConverter.ConvertToTimeframe(periodInSec);
        var message = $@"
        {{
            ""event"": ""subscribe"",
            ""channel"": ""candles"",
            ""key"": ""trade:{timeframe}:t{pair}""
        }}";

        await SendMessageAsync(message);
    }

    public async Task UnsubscribeTrades(string pair)
    {
        var message = $@"
        {{
            ""event"":""unsubscribe"",
            ""symbol"":""t{pair}""
        }}";

        await SendMessageAsync(message);
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
            Console.WriteLine($"Candle processing error: {ex.Message}");
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

    private void ProcessMessage(string message)
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

    public void Dispose()
    {
        _clientWebSocket?.Dispose();
        _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default);
    }
} */