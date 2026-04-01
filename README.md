# 📘 Tradesocio Assignment – Order Matching & PnL Engine (.NET 6)

## 🧩 Overview
This project implements an **in-memory Order Matching Engine and Position & PnL Engine** using **.NET 6**.

It simulates a simplified trading system where BUY and SELL orders are matched based on **price-time priority**, with support for **partial fills**.

The focus of this implementation is on:
- Efficient data structures
- Correct matching logic
- Clean and maintainable code
- Proper unit testing

---

## 🎯 Evaluation Focus
This solution is built keeping the assignment evaluation criteria in mind:

- ✅ Data structure efficiency  
- ✅ Correctness of matching logic  
- ✅ Clean code design  
- ✅ Edge case handling  
- ✅ Unit testing coverage  
- ⚡ Extensibility for concurrency (optional)

---

## 🛠️ Tech Stack
- .NET 6
- C#
- xUnit (Unit Testing)
- In-memory collections (no database)

---

# 🧱 Task 1 – Order Matching Engine

## 🚀 Features
- Add new orders (**NEW**)
- Cancel orders (**CANCEL**)
- Modify orders (**MODIFY**)
- Print order book (**PRINT**)
- Automatic matching of BUY and SELL orders
- Supports **partial fills**
- Maintains **price-time priority**

---

## 🧠 Approach & Design

### 1. Data Structures

BuyOrders  : SortedDictionary<decimal, Queue<Order>> (Descending)
SellOrders : SortedDictionary<decimal, Queue<Order>> (Ascending)
OrderMap   : Dictionary<string, Order>

## 🧠 Design Decisions & Matching Logic

### 📌 Why this design?

To ensure **efficiency, correctness, and scalability**, the following data structures were chosen:

#### 🔹 SortedDictionary (Price Levels)
- Used for both BUY and SELL sides of the order book
- Provides **O(log n)** insertion and retrieval
- Ensures quick access to:
  - **Lowest SELL price**
  - **Highest BUY price**
- Enables efficient price-based matching

#### 🔹 Queue (FIFO per Price Level)
- Each price level maintains a `Queue<Order>`
- Guarantees **First-In-First-Out (FIFO)** execution
- Ensures **time priority** among orders at the same price

#### 🔹 OrderMap (HashMap)
- Stores all active orders using `OrderId` as key
- Provides **O(1)** lookup
- Enables fast:
  - Order cancellation
  - Order modification

---

## ⚙️ Matching Logic

The engine follows strict rules for matching BUY and SELL orders:

### 🟢 BUY Order Matching
- A BUY order is matched against the **lowest available SELL price**


### 🔴 SELL Order Matching
- A SELL order is matched against the **highest available BUY price**


---

## 🔄 Matching Flow

When a new order is received:

1. Retrieve the **best opposing order**:
 - BUY → lowest SELL
 - SELL → highest BUY

2. Check if prices satisfy matching condition

3. Execute trade:


4. Update quantities:
- Reduce both BUY and SELL quantities
- Supports **partial fills**

5. Remove fully executed orders from the book

6. Repeat until:
- Order is fully matched, OR
- No valid match is available

7. If quantity remains:
- Add order to the order book

---

## ⚖️ Price-Time Priority

The engine strictly follows **price-time priority**, which is the industry standard:

### 💰 Price Priority
- Orders with better price are matched first:
- Higher price for BUY
- Lower price for SELL

### ⏱️ Time Priority
- Among orders with the same price:
- Earlier orders are executed first
- Achieved using FIFO queues

---

## 🔁 Order Lifecycle (What the Engine Handles)

### ✅ NEW Order
- Attempts matching immediately
- Remaining quantity (if any) is added to order book

### ❌ CANCEL Order
- Removes order from:
- OrderMap
- Corresponding price queue
- Deletes price level if empty

### 🔄 MODIFY Order
- Implemented as:

- Ensures updated order follows fresh price-time priority

---

## ⚠️ Edge Cases Handled

- Cancel non-existent order (safe no-op)
- Modify order across sides (BUY → SELL)
- Empty order book scenarios
- Partial vs full order fills
- Removal of empty price levels
- Multiple orders at same price level (FIFO maintained)

---

## 📊 Position & PnL Engine (Task 2)

### 📌 Objective
Track trading performance for each instrument using:

- Net Quantity (**netQty**)
- Average Price (**avgPrice**)
- Realized PnL
- Unrealized PnL

---

### ⚙️ Core Logic

#### 🔹 FILL Event
- Updates:
- netQty
- avgPrice
- realizedPnL

#### 🔹 PRICE Update
- Updates:
- unrealizedPnL based on current market price

---

### 💡 PnL Calculation

#### Realized PnL
- Generated when positions are closed (buy vs sell difference)

#### Unrealized PnL
- Based on current market price vs average price

---

## 🧪 Testing Strategy

Unit tests are implemented using **xUnit**.

### ✔ Covered Scenarios:
- Order addition (BUY/SELL)
- Order cancellation
- Order modification
- Matching logic correctness
- Edge cases (invalid cancel, empty book)
- Internal state validation using `InternalsVisibleTo`

---

## 🏁 Summary

This implementation ensures:

- Efficient order processing using optimal data structures
- Accurate and deterministic matching logic
- Strict adherence to price-time priority
- Robust handling of edge cases
- Clean, testable, and extensible code design
