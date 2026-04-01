# Tradesocio_Assignment
This Repo presents a console application for the Order Management
# Order Matching Engine (.NET 6)

## Overview
This project is an **in-memory Order Matching Engine** built using **.NET 6**.  
It simulates a simple trading system where **BUY** and **SELL** orders are matched based on **price-time priority**, supporting **partial fills**.

The engine supports **CRUD operations** on orders, maintaining an efficient order book.

---

## Features

- Add new orders (`NEW`)
- Cancel existing orders (`CANCEL`)
- Modify orders (`MODIFY`)
- Print current order book (`PRINT`)
- Matches orders automatically:
  - **BUY** matches the lowest **SELL**
  - **SELL** matches the highest **BUY**
  - Supports **partial fills**
- In-memory data structures for fast operations
- `.gitignore` configured to ignore build artifacts

---
## Approach & Logic

### 1. Data Structures

- **OrderBook**:
  - `BuyOrders` → `SortedDictionary<decimal, Queue<Order>>` (Descending)
  - `SellOrders` → `SortedDictionary<decimal, Queue<Order>>` (Ascending)
  - `OrderMap` → `Dictionary<string, Order>` (to quickly access any order)

- **Order**:
  - `Id`, `Side` (BUY/SELL), `Price`, `Quantity`, `Timestamp`

---

### 2. Matching Logic

- When a **new order** is added:
  - **BUY order** → matched with lowest SELL orders
  - **SELL order** → matched with highest BUY orders
  - **Price-time priority** is followed
  - **Partial fills** handled by reducing quantities

- After matching:
  - Remaining quantity (if any) is added to the order book
  - OrderBook queues are updated accordingly
  - CRUD operations allow modification or cancellation of existing orders

---

### 3. CRUD Operations

- **Add Order** → `MatchingEngine.AddOrder(Order order)`
- **Cancel Order** → remove from `OrderMap` & queue
- **Modify Order** → adjust quantity/price, then reprocess for matching

---
