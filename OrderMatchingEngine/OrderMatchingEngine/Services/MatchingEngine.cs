using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderMatchingEngine.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OrderMatchingTests")]

namespace OrderMatchingEngine.Services
{
    public class MatchingEngine
    {   
        internal OrderBook orderBook;   

        public decimal TotalPnL { get; set; } = 0;

        internal void MatchBuy(Order buy)
        {
            while (buy.Quantity > 0 && orderBook.SellOrders.Any())
            {
                var bestSell = orderBook.SellOrders.First();

                if (bestSell.Key > buy.Price)
                    break;

                var sellQueue = bestSell.Value;
                var sellOrder = sellQueue.Peek();

                int tradeQty = Math.Min(buy.Quantity, sellOrder.Quantity);

                TotalPnL += (buy.Price - sellOrder.Price) * tradeQty;

                buy.Quantity -= tradeQty;
                sellOrder.Quantity -= tradeQty;

                if (sellOrder.Quantity == 0)
                {
                    sellQueue.Dequeue();
                    if (sellQueue.Count == 0)
                        orderBook.SellOrders.Remove(bestSell.Key);
                }
            }
        }

        internal void MatchSell(Order sell)
        {
            while (sell.Quantity > 0 && orderBook.BuyOrders.Any())
            {
                var bestBuy = orderBook.BuyOrders.First();

                if (bestBuy.Key < sell.Price)
                    break;

                var buyQueue = bestBuy.Value;
                var buyOrder = buyQueue.Peek();

                int tradeQty = Math.Min(sell.Quantity, buyOrder.Quantity);

                TotalPnL += (buyOrder.Price - sell.Price) * tradeQty;
                sell.Quantity -= tradeQty;
                buyOrder.Quantity -= tradeQty;

                if (buyOrder.Quantity == 0)
                {
                    buyQueue.Dequeue();
                    if (buyQueue.Count == 0)
                        orderBook.BuyOrders.Remove(bestBuy.Key);
                }
            }
        }

        internal void AddToBook(Order order)
        {
            var book = order.Side == "BUY" ? orderBook.BuyOrders : orderBook.SellOrders;

            if (!book.ContainsKey(order.Price))
                book[order.Price] = new Queue<Order>();

            book[order.Price].Enqueue(order);
        }

        public void AddOrder(Order order)
        {
            if (order.Side == "BUY")
                MatchBuy(order);
            else
                MatchSell(order);

            if (order.Quantity > 0)
                AddToBook(order);

            orderBook.OrderMap[order.Id] = order;
        }

        public void Cancel(string orderId)
        {
            if (!orderBook.OrderMap.ContainsKey(orderId))
                return;

            var order = orderBook.OrderMap[orderId];
            var book = order.Side == "BUY" ? orderBook.BuyOrders : orderBook.SellOrders;

            if (book.ContainsKey(order.Price))
            {
                var queue = book[order.Price];
                var newQueue = new Queue<Order>(queue.Where(o => o.Id != orderId));

                if (newQueue.Count > 0)
                    book[order.Price] = newQueue;
                else
                    book.Remove(order.Price);
            }

            orderBook.OrderMap.Remove(orderId);
        }

        public void Modify(string id, string side, decimal price, int qty)
        {
            Cancel(id);

            var newOrder = new Order
            {
                Id = id,
                Side = side,
                Price = price,
                Quantity = qty,
                Timestamp = DateTime.Now
            };

            AddOrder(newOrder);
        }

        public void Print()
        {
            Console.WriteLine("SELL:");
            foreach (var kvp in orderBook.SellOrders)
            {
                Console.WriteLine($"{kvp.Key} -> {kvp.Value.Sum(o => o.Quantity)}");
            }

            Console.WriteLine("\nBUY:");
            foreach (var kvp in orderBook.BuyOrders)
            {
                Console.WriteLine($"{kvp.Key} -> {kvp.Value.Sum(o => o.Quantity)}");
            }

            Console.WriteLine($"\nTotal PnL: {TotalPnL}");
        }

        public MatchingEngine()
        {
            orderBook = new OrderBook();  
        }
    }
}
