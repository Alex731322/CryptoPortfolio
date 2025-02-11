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