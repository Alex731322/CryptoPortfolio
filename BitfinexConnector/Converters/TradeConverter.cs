using BitfinexConnector.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitfinexConnector.Converters
{
    internal class TradeConverter : JsonConverter<Trade>
    {
        private readonly string _pair;
        public TradeConverter(string pair)
        {
            _pair = pair;
        }

        public override Trade ReadJson(JsonReader reader, Type objectType, Trade existingValue, bool hasExistingValue, JsonSerializer serializer)
        { 
            JArray array = JArray.Load(reader);

            return new Trade
            {
                Id = array[0].ToString(),
                Amount = array[2].Value<decimal>(),
                Time = DateTimeOffset.FromUnixTimeMilliseconds(array[1].ToObject<long>()),
                Price = array[3].ToObject<decimal>(),
                Side = array[2].ToObject<decimal>() >= 0 ? "buy" : "sell",
                Pair = _pair
            };
        }

        public override void WriteJson(JsonWriter writer, Trade? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
