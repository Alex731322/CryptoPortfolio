namespace BitfinexConnector.Constants
{
    public static class BitfinexProcessConstants
    {
        public const string ChannelTypeCandles = "candles";
        public const string ChannelTypeTrades = "trades";

        public const string EventSubscribed = "subscribed";
        public const string EventUnsubscribed = "unsubscribed";

        public const string HeartbeatMessageType = "hb";
        public const string TradeUpdateType = "te";
        public const string CandleUpdateType = "hu";
    }
}
