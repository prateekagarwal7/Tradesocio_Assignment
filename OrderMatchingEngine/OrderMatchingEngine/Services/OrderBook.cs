using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderMatchingEngine.Models;

namespace OrderMatchingEngine.Services
{
    public class OrderBook
    {
        public SortedDictionary<decimal, Queue<Order>> BuyOrders;
        public SortedDictionary<decimal, Queue<Order>> SellOrders;

        public Dictionary<string, Order> OrderMap;

        public OrderBook()
        {
            BuyOrders = new SortedDictionary<decimal, Queue<Order>>(
                Comparer<decimal>.Create((a, b) => b.CompareTo(a)) // DESC
            );

            SellOrders = new SortedDictionary<decimal, Queue<Order>>(); // ASC

            OrderMap = new Dictionary<string, Order>();
        }
    }
}
