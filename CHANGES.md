# Changes added to the project

First run the database script (new install: `MyStore.sql`; existing DB: `MyStore_AddProductStock.sql`).

| Feature | What was added | Where |
|---|---|---|
| **Stock (SL tồn)** | New column `Product.ProductStock` (>= 0). Checkout deducts stock, cancel gives it back, admin status change keeps it in sync. Atomic SQL `UPDATE ... WHERE ProductStock >= qty` inside a transaction, so the last item cannot be sold twice. | `Services/StockService.cs`, `Services/OrderService.cs`, `OrderController`, `PaypalController`, `Admin/CustomersController`, EDMX, `MyStore.sql` |
| Stock in the UI | "Hết hàng" / "Chỉ còn N" labels, add-to-cart blocked above stock, cart quantity capped, admin stock column + low-stock highlight | storefront + admin views |
| **Data constraints (CRUD)** | Unique category name (DB + code), unique product name per category, category must exist, length limits, image max 2 MB + unique file names, stock edit applied as a delta so it never overwrites sales | `Admin/CategoriesController`, `Admin/ProductsController`, `ModelValidation.cs`, `MyStore.sql` |
| **Format validation** | VN phone `^(0\|\+84)(3\|5\|7\|8\|9)[0-9]{8}$`, username 3-50 chars `[A-Za-z0-9_.-]`, name/price/stock ranges. Shared in `ValidationPatterns` | `Models/ModelValidation.cs`, `RegisterVM`, `ProfileVM`, `Admin/AccountsController` |
| **Search / filter** | Storefront: price range + "only in stock". Admin products: category + stock filter + stock sort. Admin categories: search | `HomeController.ProductList`, `Admin/ProductsController`, `Admin/CategoriesController` + views |
| **Session login/logout** | Login stores Username/Role/CustomerName in Session (guest cart is kept). Logout clears identity + cart + pending PayPal, expires the cookie, abandons the session. Session is rebuilt from the cookie if it timed out. Logged-in pages are no-store (Back button after logout). Admin logout no longer wipes a customer's session. Forms timeout fixed (was 3600 **minutes**). | `Filters/SessionSyncAttribute.cs`, `AccountController`, `Admin/AuthController`, `Web.config` |

## Quick test list
1. Product with stock 3: add 4 to cart -> refused. Add 3, checkout -> stock becomes 0, shows "Hết hàng".
2. My orders -> cancel that order -> stock back to 3. Admin order page: change status to "Đã hủy" / back -> stock follows.
3. Register with phone `12345` or username `a b` -> rejected. Create a category with an existing name -> rejected.
4. Login, add to cart, logout -> cart is empty, Back button does not show the profile page.
