using System;

    public class Position
    {
        public string Symbol { get; set; }
        public int NetQty { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal RealizedPnL { get; set; }
        public decimal MarketPrice { get; set; }

        public decimal UnrealizedPnL => NetQty * (MarketPrice - AvgPrice);
    }
