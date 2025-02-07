namespace BitfinexConnector.Clients
{
    public class BitfinexRestClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public BitfinexRestClient()
        {
            _httpClient = new HttpClient();
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
