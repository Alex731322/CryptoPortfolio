namespace BitfinexConnector.Converters
{
    public static class TimeframeConverter
    {
        public static string ConvertToTimeframe(int periodInSec)
        {
            return periodInSec switch
            {
                60 => "1m",
                300 => "5m",
                900 => "15m",
                3600 => "1h",
                86400 => "1D",
                604800 => "1W",
                1209600 => "14D",

                _ => throw new ArgumentException($"Unsupported timeframe: {periodInSec} seconds")
            };
        }
    }
}
