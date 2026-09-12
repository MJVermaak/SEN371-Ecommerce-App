# Product details and persistent cart

The product route is `/product/:id`. It loads the product from the API and displays its images, description, selectable variants, variant price and available stock. Invalid or removed products have a not-found page. Failed requests have a retry action.

Sign in to add products. The login and registration links retain the product or cart destination. Guest carts are not stored or merged. Each account has its own database-backed cart.

## Cart endpoints

All cart endpoints require a bearer token. They derive the account ID from its validated `sub` claim. Request bodies cannot select an account or set prices.

| Method | Path | Body |
| --- | --- | --- |
| GET | `/api/v1/cart` | None |
| POST | `/api/v1/cart/items` | `{ "productId": 1, "productVariantId": 1, "quantity": 2 }` |
| PUT | `/api/v1/cart/items/{cartItemId}` | `{ "quantity": 1 }` |
| DELETE | `/api/v1/cart/items/{cartItemId}` | None |

Successful requests return the complete cart, including quantities, current variant prices, line totals, total quantity and subtotal. Use IDs returned by the product and cart endpoints. The example IDs are not fixed demo identifiers.

Adding the same variant increments its existing item. Quantities must be integers between 1 and 99 and cannot exceed current stock. A variant must belong to the specified product. Updating or deleting another account's item returns 404. Missing or expired authentication returns 401. Invalid input returns 400. Insufficient stock or a concurrent-write conflict returns 409.

SQL Server cart mutations run in a serializable transaction and take an update lock on the account row before loading its cart. This also covers simultaneous first additions when no cart exists. Only changed cart rows are saved. Product and inventory records are not updated.

The browser uses the API response instead of inventing a successful cart update. It refreshes after failed mutations to reconcile stock changes or a lost response. The header count shares this state. Logout clears it. Responses belonging to an earlier session cannot replace the current cart. Other tabs receive a refresh signal without storing cart contents in localStorage.

## Limits

A cart does not reserve stock. Its prices are current catalog prices, not a guaranteed checkout quote. Checkout, delivery calculation, payment and order creation remain unimplemented. The cart explicitly says this instead of presenting a nonfunctional checkout button or a made-up delivery charge.

These changes reuse the existing Carts, CartItems, Products, ProductVariants, ProductImages and Inventory tables. They do not add a schema migration or modify InitialCreate or SeedDemoData. Products require a variant with inventory before they can be added to a cart. The existing demo seed supplies these rows.

## Run and verify

Start the API and Vite using the working local configuration. No database reset or migration regeneration is needed.

From the repository root:

```powershell
dotnet test .\GrandmastersHub\GrandmastersHub.Tests --filter FullyQualifiedName~ShoppingFlowTests
```

These HTTP integration tests use a separate temporary SQLite database per test. They exercise JWT authentication, product detail responses, add/update/remove persistence, account isolation, stock limits, client-price rejection, missing products and deleted accounts. They do not validate SQL Server locking hints or parallel writes against SQL Server. The older repository tests still depend on the developer SQL Server configuration.

From `GrandmastersHub/GrandmastersHub.Api/client`:

```powershell
npm ci
npm test
npm run build
```

The JavaScript tests cover request payloads, authentication, session-change events, JSON error responses, expired-token races, quantity limits and safe login return paths. They are not browser end-to-end tests.

For a manual acceptance check, open a catalog card, choose an in-stock variant, sign in and add it. Open the cart, change its quantity, refresh the browser and confirm it persists. Remove it and confirm the header count changes. Sign into another account and confirm the first account's cart is absent. Check product and cart layouts on a narrow viewport. Attempt to add more than the displayed stock and confirm the request fails without changing saved quantities.
