# Demo video script (about 8 minutes)

Goal: show that three apps (ABP + Angular web, WinForms POS, VB.NET warehouse) share one live backend.

## Before recording

1. Start SQL Server LocalDB, then run `dotnet run` in `aspnet-core/src/InventoryPos.DbMigrator` (creates the DB and demo users).
2. Start the API: `dotnet run` in `aspnet-core/src/InventoryPos.HttpApi.Host` (https://localhost:44395).
3. Start Angular: `npx ng serve` in `angular` (http://localhost:4200).
4. Open the POS app (`pos-winforms/InventoryPos.Pos`) and the warehouse app (`warehouse-vb/InventoryPos.Warehouse`) with `dotnet run`, or launch both from Visual Studio.
5. Arrange windows side by side: browser on the left, the two desktop apps stacked on the right. Close unrelated tabs and notifications.

Demo accounts (password `1q2w3E*` for all): `admin`, `cashier`, `warehouse`.

## Script

### 1. Introduction (30 s)
Say: "This system has three clients on one ASP.NET Core backend built on ABP Framework: an Angular admin portal, a C# WinForms point of sale, and a VB.NET warehouse app. The desktop apps never touch the database; they call the REST API."
Show: the folder layout in the README (`aspnet-core`, `angular`, `pos-winforms`, `warehouse-vb`) or Swagger at `/swagger`.

### 2. Admin portal: set up the catalog (1.5 min)
1. Log in to Angular as `admin`. Point out the dashboard (today's sales, low stock, inventory value, 7-day chart).
2. Catalog > Categories: create "Beverages".
3. Catalog > Products: create "Cola 330ml", SKU `COLA-330`, price 1.50, reorder threshold 5. Optionally attach a photo.
4. Point out that quantity on hand starts at 0.
5. Click **Label** to show the Code 128 barcode sheet.

### 3. Warehouse app (VB.NET): receive stock (1.5 min)
1. Log in as `warehouse`.
2. Select "Cola 330ml", choose **Stock In**, quantity 20, Submit.
3. Show the low-stock tint on rows under the threshold, and the **Low stock only** filter.
4. (Optional) Back in Angular as `admin`, create a supplier and a purchase order. Then in the warehouse app use **Receive Purchase Order...** to receive part of it, showing that a partial delivery keeps the order open.

### 4. POS app (WinForms): make a sale (2 min)
1. Log in as `cashier`.
2. Type `COLA-330` in the SKU box and press Enter (or press F2 first). Set quantity 3.
3. Apply a 10% discount and pick **Cash**, tendering 10.00. Show subtotal, discount, tax, total and change updating live.
4. Press F9 to check out. Show the print-preview receipt.
5. Try to sell more than is on hand to show the server rejecting it ("Not enough stock").

### 5. Back to the admin portal: prove it is one backend (1.5 min)
1. Refresh Products: on hand is 20 - 3 = 17.
2. Stock Transactions: shows the Stock In from the VB app and the sale deduction.
3. Sale Orders: shows the POS sale with discount, tax and payment details.
4. Click **Refund** on that sale for one unit. Show stock restored, and the dashboard netting the refund.
5. Click **Export CSV** on one list.

### 6. Roles and security (45 s)
1. Log in to Angular as `cashier`: the menu hides pages the role can't use.
2. Mention that the API enforces the same permissions server-side.

### 7. Wrap-up (30 s)
Say: "ABP Framework is used as the free stand-in for ASP.NET Zero, which is a paid template from the same team. Source code for all four projects is attached."

## Tips
- Do one full dry run first and note any step that fails.
- If the browser shows a certificate warning, run `dotnet dev-certs https --trust` beforehand.
- Keep each step short; speed up or cut waiting time when editing.
