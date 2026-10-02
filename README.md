# InventoryPos — Inventory & POS System

A small inventory and point-of-sale system built across the three required stacks for the assignment, sharing one backend database through a REST API:

| App | Tech | Role |
|---|---|---|
| `angular` (+ `aspnet-core`) | **ABP Framework** (the free, open-source engine ASP.NET Zero is built on) + Angular | Admin web portal: manage Categories/Products, view Stock history and Sales |
| `pos-winforms/InventoryPos.Pos` | **WinForms** (C#) | Point-of-sale checkout app for store staff |
| `warehouse-vb/InventoryPos.Warehouse` | **VB.NET** (WinForms) | Warehouse stock in/out app |

All three talk to the same backend (`InventoryPos.HttpApi.Host`) over its REST API — the desktop apps have no direct database access, matching how a real system like this would be built.

## Why ABP Framework instead of ASP.NET Zero

ASP.NET Zero is a commercial product that requires a paid license (after a trial) and an aspnetzero.com account. ABP Framework is the free, open-source engine that ASP.NET Zero itself is built on top of, and provides the same core features used here — multi-tenancy-ready identity, role/permission management, an admin UI theme, and OpenIddict-based auth — with no license required.

## Domain model

- **Category** — simple name.
- **Product** — SKU, name, category, price, quantity on hand, reorder threshold.
- **StockTransaction** — an In or Out movement against a product (warehouse app writes these).
- **SaleOrder / SaleOrderItem** — a completed sale with line items (POS app writes these).

Stock math lives entirely on the server (`StockTransactionAppService`, `SaleOrderAppService`): a Stock In increases `QuantityOnHand`, a Stock Out or a Sale decreases it, with a server-side check that blocks selling/removing more than is on hand. Both desktop apps simply call the API — neither touches the database directly.

## Running everything

You'll need: .NET 9 SDK, Node.js, SQL Server LocalDB, and Visual Studio (or just the SDK + an editor). All of this was verified working on Windows with .NET 9 SDK + VS2022 Community + the built-in `MSSQLLocalDB` instance.

### 1. Backend API

```bash
cd aspnet-core/src/InventoryPos.DbMigrator
dotnet run
```

This creates the LocalDB database (`InventoryPos`) and seeds the default admin account (`admin` / `1q2w3E*`). Run it again any time after pulling new migrations.

Then start the API host:

```bash
cd aspnet-core/src/InventoryPos.HttpApi.Host
dotnet run
```

It listens on `https://localhost:44395` (Swagger at `/swagger`). The first time, trust the local dev HTTPS certificate if prompted:

```bash
dotnet dev-certs https --trust
```

### 2. Angular admin app

```bash
cd angular
npm install
npx ng serve
```

Open `http://localhost:4200`, log in with `admin` / `1q2w3E*`. The home page is a **Dashboard** (today's sales, product count, low-stock count, inventory value, 7-day sales chart, top sellers, low-stock list), served by one backend call (`GET /api/app/dashboard`). The **Catalog** menu has Categories, Products, Stock Transactions, and Sale Orders; Products, Stock Transactions and Sale Orders each have an **Export CSV** button.

If the backend's client-side libs (`wwwroot/libs`) are ever missing (500 errors on every page), regenerate them from `aspnet-core/src/InventoryPos.HttpApi.Host`:

```bash
npx yarn install --ignore-engines
abp install-libs
```

### 3. WinForms POS app

```bash
cd pos-winforms/InventoryPos.Pos
dotnet run
```

Log in (`cashier` / `1q2w3E*` or `admin`), then either pick a product and set a quantity, or type/scan a SKU into the quick-entry box and press Enter. **F2** focuses the SKU box, **F3** the search box, **F9** checks out. Checkout opens a print-preview receipt you can print.

### 4. VB.NET Warehouse app

```bash
cd warehouse-vb/InventoryPos.Warehouse
dotnet run
```

Log in (`warehouse` / `1q2w3E*` or `admin`), select a product, choose **Stock In** or **Stock Out**, enter a quantity, and **Submit**. Low-stock rows are tinted red (out-of-stock rows are solid red), a banner counts them, and **Low stock only** filters the list.

## Roles

`DbMigrator` seeds two demo roles and users (password `1q2w3E*` for all demo accounts):

| User | Role | Can do |
|---|---|---|
| `admin` | admin | Everything |
| `cashier` | Cashier | View products/categories, create and view sales |
| `warehouse` | Warehouse | View products/categories, record and view stock transactions |

The Angular menu hides pages a role can't use, and the API enforces the same permissions server-side. Manage roles under **Administration > Identity Management**. After pulling this change, re-run `DbMigrator` once to create the roles and users.

Both desktop apps authenticate against the backend's existing `InventoryPos_App` OpenIddict client using the OAuth2 **password** grant (already enabled on that client by default in the ABP template) — no extra backend configuration was needed for this.

## Demo script (for the video)

1. In the **Angular** admin, create a Category ("Beverages") and a Product ("Cola 330ml", SKU `COLA-330`) — note it starts at 0 on hand.
2. In the **VB.NET warehouse app**, log in, select that product, Stock In a quantity (e.g. 20).
3. In the **WinForms POS app**, log in, add the product to a cart, and Checkout.
4. Back in the **Angular** admin, refresh Products — the on-hand quantity reflects both the stock-in and the sale, and the Sale Orders / Stock Transactions pages show the new records — proving all three apps share one live backend.

## Project layout

```
InventoryPos-System/
  aspnet-core/     ABP backend solution (InventoryPos.sln)
  angular/         Angular admin SPA
  pos-winforms/
    InventoryPos.Pos/        WinForms POS app (C#)
  warehouse-vb/
    InventoryPos.Warehouse/  WinForms warehouse app (VB.NET)
```

## Out of scope (by design)

No real barcode scanner/printer hardware, no multi-tenant UI, no discounts/tax, no payment gateway — kept out to stay focused on the three required tech stacks and a learnable scope.
