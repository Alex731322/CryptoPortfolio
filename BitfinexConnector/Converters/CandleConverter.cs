using BitfinexConnector.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitfinexConnector.Converters
{
    public class CandleConverter : JsonConverter<Candle>
    {
        private readonly string _pair;

        public CandleConverter(string pair) => _pair = pair;
        public override Candle ReadJson(JsonReader reader, Type objectType, Candle existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray arr = JArray.Load(reader);
            return new Candle
            {
                Pair = _pair,
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(arr[0].Value<long>()),
                OpenPrice = arr[1].Value<decimal>(),
                ClosePrice = arr[2].Value<decimal>(),
                HighPrice = arr[3].Value<decimal>(),
                LowPrice = arr[4].Value<decimal>(),
                TotalVolume = arr[5].Value<decimal>()
            };
        }

        public override void WriteJson(JsonWriter writer, Candle? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
