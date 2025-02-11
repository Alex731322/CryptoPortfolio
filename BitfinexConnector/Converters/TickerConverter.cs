using BitfinexConnector.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BitfinexConnector.Converters
{
    public class TickerConverter : JsonConverter<Ticker>
    {
        public override Ticker ReadJson(JsonReader reader, Type objectType, Ticker existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray arr = JArray.Load(reader);
            return new Ticker
            {
                Bid = arr[0].Value<decimal>(),
                BidSize = arr[1].Value<decimal>(),
                Ask = arr[2].Value<decimal>(),
                AskSize = arr[3].Value<decimal>(),
                DailyChange = arr[4].Value<decimal>(),
                DailyChangePercent = arr[5].Value<decimal>(),
                LastPrice = arr[6].Value<decimal>(),
                Volume = arr[7].Value<decimal>(),
                High = arr[8].Value<decimal>(),
                Low = arr[9].Value<decimal>()
            };
        }

        public override void WriteJson(JsonWriter writer, Ticker? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    
}
