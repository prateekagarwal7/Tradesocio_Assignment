using Xunit;
using OrderMatchingEngine; // Namespace of your main project
using PositionPnLEngine;
using OrderMatchingEngine.Models;
using OrderMatchingEngine.Services;
using System;
using System.Linq;

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

    public class MatchingEngineTests
    {
        private MatchingEngine engine;

        public MatchingEngineTests()
        {
            engine = new MatchingEngine();
        }

        [Fact]
        public void AddToBook_ShouldAddBuyOrder()
        {
            var order = new Order
            {
                Id = "1",
                Side = "BUY",
                Price = 100m,
                Quantity = 10,
                Timestamp = DateTime.Now
            };

            engine.AddToBook(order); // internal method

            var buyOrders = engine.orderBook.BuyOrders; // access internal field

            Assert.True(buyOrders.ContainsKey(100m));
            Assert.Single(buyOrders[100m]);
            Assert.Equal(10, buyOrders[100m].First().Quantity);
        }
        [Fact]
        public void MatchBuy_ShouldUpdatePnL()
        {
            // Arrange
            var sellOrder = new Order { Id = "s1", Side = "SELL", Price = 100m, Quantity = 5, Timestamp = DateTime.Now };
            engine.AddToBook(sellOrder);

            var buyOrder = new Order { Id = "b1", Side = "BUY", Price = 120m, Quantity = 5, Timestamp = DateTime.Now };

            // Act
            engine.MatchBuy(buyOrder);

            // Assert
            Assert.False(engine.orderBook.SellOrders.ContainsKey(100m));
            Assert.True(engine.TotalPnL > 0);
        }

        [Fact]
        public void MatchSell_ShouldFullyMatchAndRemoveBuyOrder_AndUpdatePnL()
        {
            // Arrange: Add BUY order to book
            var buyOrder = new Order
            {
                Id = "b1",
                Side = "BUY",
                Price = 120m,
                Quantity = 5,
                Timestamp = DateTime.Now
            };

            engine.AddToBook(buyOrder);

            // Incoming SELL order (should match)
            var sellOrder = new Order
            {
                Id = "s1",
                Side = "SELL",
                Price = 100m,
                Quantity = 5,
                Timestamp = DateTime.Now
            };

            engine.MatchSell(sellOrder);

            Assert.False(engine.orderBook.BuyOrders.ContainsKey(120m));

            Assert.Equal(0, sellOrder.Quantity);

            Assert.True(engine.TotalPnL > 0);
        }

        [Fact]
        public void MatchSell_ShouldPartiallyFillBuyOrder()
        {
            // Arrange
            var buyOrder = new Order
            {
                Id = "b1",
                Side = "BUY",
                Price = 120m,
                Quantity = 10,
                Timestamp = DateTime.Now
            };

            engine.AddToBook(buyOrder);

            var sellOrder = new Order
            {
                Id = "s1",
                Side = "SELL",
                Price = 100m,
                Quantity = 5,
                Timestamp = DateTime.Now
            };

            engine.MatchSell(sellOrder);

            Assert.True(engine.orderBook.BuyOrders.ContainsKey(120m));

            var remaining = engine.orderBook.BuyOrders[120m].Peek();
            Assert.Equal(5, remaining.Quantity);

            Assert.Equal(0, sellOrder.Quantity);

            Assert.True(engine.TotalPnL > 0);
        }

        [Fact]
        public void AddOrder_Should_Add_Buy_Order_To_Book()
        {
            var order = new Order
            {
                Id = "1",
                Side = "BUY",
                Price = 100,
                Quantity = 10
            };

            engine.AddOrder(order);

            Assert.True(engine.orderBook.BuyOrders.ContainsKey(100));
            Assert.Equal(10, engine.orderBook.BuyOrders[100].Peek().Quantity);
            Assert.True(engine.orderBook.OrderMap.ContainsKey("1"));
        }

        [Fact]
        public void AddOrder_Should_Add_Sell_Order_To_Book()
        {
            var order = new Order
            {
                Id = "2",
                Side = "SELL",
                Price = 120,
                Quantity = 5
            };

            engine.AddOrder(order);

            Assert.True(engine.orderBook.SellOrders.ContainsKey(120));
            Assert.Equal(5, engine.orderBook.SellOrders[120].Peek().Quantity);
        }

        [Fact]
        public void Cancel_Should_Remove_Order_From_Book_And_Map()
        {
            var order = new Order
            {
                Id = "3",
                Side = "BUY",
                Price = 100,
                Quantity = 10
            };

            engine.AddOrder(order);

            engine.Cancel("3");

            Assert.False(engine.orderBook.OrderMap.ContainsKey("3"));
            Assert.False(engine.orderBook.BuyOrders.ContainsKey(100));
        }

        [Fact]
        public void Cancel_Should_Do_Nothing_If_Order_Not_Exists()
        {
            engine.Cancel("invalid");

            Assert.Empty(engine.orderBook.OrderMap);
        }

        [Fact]
        public void Modify_Should_Update_Order()
        {
            var order = new Order
            {
                Id = "4",
                Side = "BUY",
                Price = 100,
                Quantity = 10
            };

            engine.AddOrder(order);

            engine.Modify("4", "SELL", 150, 20);

            Assert.True(engine.orderBook.OrderMap.ContainsKey("4"));

            var modifiedOrder = engine.orderBook.OrderMap["4"];

            Assert.Equal("SELL", modifiedOrder.Side);
            Assert.Equal(150, modifiedOrder.Price);
            Assert.Equal(20, modifiedOrder.Quantity);

            Assert.True(engine.orderBook.SellOrders.ContainsKey(150));
        }

        [Fact]
        public void Modify_Should_Remove_Old_Order_From_Previous_Book()
        {
            var order = new Order
            {
                Id = "5",
                Side = "BUY",
                Price = 100,
                Quantity = 10
            };

            engine.AddOrder(order);

            engine.Modify("5", "SELL", 200, 5);

            Assert.False(engine.orderBook.BuyOrders.ContainsKey(100));
            Assert.True(engine.orderBook.SellOrders.ContainsKey(200));
        }

    }
}