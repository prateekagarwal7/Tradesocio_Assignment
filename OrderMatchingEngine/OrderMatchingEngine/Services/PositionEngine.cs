using System;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PositionPnLEngine
{
    public class PositionEngine
    {
        private Dictionary<string, Position> _positions = new();
        public IReadOnlyDictionary<string, Position> Positions => _positions;

        // Command: FILL
        public void Fill(string side, string symbol, int qty, decimal price)
        {
            if (!_positions.ContainsKey(symbol))
                _positions[symbol] = new Position { Symbol = symbol };

            var pos = _positions[symbol];

            if (side.ToUpper() == "BUY")
            {
                decimal totalCost = pos.AvgPrice * pos.NetQty + price * qty;
                pos.NetQty += qty;
                pos.AvgPrice = totalCost / pos.NetQty;
            }
            else if (side.ToUpper() == "SELL")
            {
                if (pos.NetQty >= qty)
                {
                    pos.RealizedPnL += qty * (price - pos.AvgPrice);
                    pos.NetQty -= qty;
                    if (pos.NetQty == 0) pos.AvgPrice = 0;
                }
                else
                {
                    // Selling more than held: partial logic
                    pos.RealizedPnL += pos.NetQty * (price - pos.AvgPrice);
                    pos.NetQty = qty - pos.NetQty;
                    pos.AvgPrice = price;
                }
            }
            else
            {
                Console.WriteLine("Invalid side: " + side);
            }
        }

        // Command: PRICE
        public void UpdatePrice(string symbol, decimal marketPrice)
        {
            if (!_positions.ContainsKey(symbol))
                _positions[symbol] = new Position { Symbol = symbol };

            _positions[symbol].MarketPrice = marketPrice;
        }

        // Command: PRINT
        public void PrintPositions()
        {
            Console.WriteLine("Symbol | NetQty | AvgPrice | RealizedPnL | MarketPrice | UnrealizedPnL");
            foreach (var pos in _positions.Values)
            {
                Console.WriteLine($"{pos.Symbol} | {pos.NetQty} | {pos.AvgPrice} | {pos.RealizedPnL} | {pos.MarketPrice} | {pos.UnrealizedPnL}");
            }
        }
    }
}
