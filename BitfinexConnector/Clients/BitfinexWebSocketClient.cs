using System.Net.WebSockets;

public class BitfinexWebSocketClient : IDisposable
{
    private ClientWebSocket _clientWebSocket;

    public void Connect()
    {
    }

    public void Dispose()
    {
       _clientWebSocket?.Dispose();
    }
}