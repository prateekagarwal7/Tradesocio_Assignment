using Xunit;
using OrderMatchingEngine; // Namespace of your main project
using PositionPnLEngine;

namespace OrderMatchingTests
{
    public class PositionEngineTests
    {
        [Fact]
        public void Fill_BuyOrder_UpdatesNetQtyAndAvgPrice()
        {
            var engine = new PositionEngine();

            engine.Fill("BUY", "AAPL", 10, 150); // Buy 10 @ 150
            engine.Fill("BUY", "AAPL", 20, 160); // Buy 20 @ 160

            var pos = engine.Positions["AAPL"];
            Assert.Equal(30, pos.NetQty);
            Assert.Equal(156.66666666666666666666666667m, pos.AvgPrice); // (10*150 + 20*160)/30
        }

        [Fact]
        public void Fill_SellOrder_UpdatesRealizedPnL()
        {
            var engine = new PositionEngine();

            engine.Fill("BUY", "AAPL", 20, 150);
            engine.Fill("SELL", "AAPL", 10, 160);

            var pos = engine.Positions["AAPL"];
            Assert.Equal(10, pos.NetQty);
            Assert.Equal(150, pos.AvgPrice);
            Assert.Equal(100, pos.RealizedPnL); // 10*(160-150)
        }

        [Fact]
        public void UpdatePrice_CalculatesUnrealizedPnL()
        {
            var engine = new PositionEngine();

            engine.Fill("BUY", "AAPL", 10, 150);
            engine.UpdatePrice("AAPL", 160);

            var pos = engine.Positions["AAPL"];
            Assert.Equal(100, pos.UnrealizedPnL); // 10*(160-150)
        }
    }
}