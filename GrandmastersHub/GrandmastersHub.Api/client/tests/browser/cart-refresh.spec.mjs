import { expect, test as base } from '@playwright/test';

function cart(quantity, name = 'Test board') {
  return {
    items: [{
      cartItemId: 1, productId: 1, productVariantId: 1, name,
      categoryName: 'Boards', variantName: 'Walnut',
      imageUrl: '/images/Shopping-plain-chess-board.jpg',
      unitPrice: 100, quantity, lineTotal: quantity * 100,
      stockQuantity: 99, isAvailable: true,
    }],
    totalQuantity: quantity,
    subtotal: quantity * 100,
  };
}

const test = base.extend({
  shop: async ({ page, context }, use) => {
    const state = {
      alice: cart(1), bob: cart(7, 'Bob board'),
      reads: [], writes: [], failNextRead: false,
    };
    await context.addInitScript(() => {
      localStorage.setItem('accessToken', 'alice-test-token');
      localStorage.setItem('user', JSON.stringify({ userId: 1, email: 'alice@example.test', role: 'Customer' }));
    });
    await page.route('**/api/v1/**', async (route) => {
      const request = route.request();
      const path = new URL(request.url()).pathname;
      if (path === '/api/v1/cart' && request.method() === 'GET') {
        const owner = request.headers().authorization;
        state.reads.push(owner);
        if (state.failNextRead) {
          state.failNextRead = false;
          await route.fulfill({ status: 503, json: { message: 'Cart refresh temporarily unavailable.' } });
        } else {
          await route.fulfill({ json: owner === 'Bearer bob-test-token' ? state.bob : state.alice });
        }
      } else if (path.startsWith('/api/v1/cart/items/') && request.method() === 'PUT') {
        // Hold the HTTP response even though the test may advance the server's cart.
        // This reproduces commits and responses arriving in different orders.
        state.writes.push(route);
      } else {
        await route.fulfill({ status: 404, json: { message: `Unexpected test request: ${request.method()} ${path}` } });
      }
    });
    await page.goto('/cart');
    await expect(page.locator('[aria-label="Quantity: 1"]')).toHaveText('1');
    await page.evaluate(() => {
      window.cartStorageEvents = 0;
      window.addEventListener('storage', (event) => {
        if (event.key === 'cart-updated') window.cartStorageEvents += 1;
      });
    });

    // Same browser context and origin, but no app in the second tab to add extra reads.
    const otherTab = await context.newPage();
    await otherTab.route('**/cart-event-source', (route) => route.fulfill({
      contentType: 'text/html', body: '<!doctype html><title>Cart event source</title>',
    }));
    await otherTab.goto('/cart-event-source');
    let signals = 0;
    const signal = async () => {
      signals += 1;
      await otherTab.evaluate((value) => localStorage.setItem('cart-updated', String(value)), signals);
      // Wait for delivery, not a timeout. The provider's listener has run before this counter updates.
      await expect.poll(() => page.evaluate(() => window.cartStorageEvents)).toBe(signals);
    };
    const startWrite = async (name = 'Test board') => {
      const count = state.writes.length;
      await page.getByRole('button', { name: `Increase quantity of ${name}`, exact: true }).click();
      await expect.poll(() => state.writes.length).toBe(count + 1);
      await expect(page.locator('.saved-cart')).toHaveAttribute('aria-busy', 'true');
      return state.writes[count];
    };
    const expectQuantity = async (quantity) => {
      await expect(page.locator(`[aria-label="Quantity: ${quantity}"]`)).toHaveText(String(quantity));
      await expect(page.locator('.saved-cart-header > p')).toHaveText(`${quantity} items`);
      await expect(page.locator('.saved-cart')).toHaveAttribute('aria-busy', 'false');
    };
    await use({ state, otherTab, signal, startWrite, expectQuantity });
  },
});

test('replays an other-tab refresh after installing an older mutation response', async ({ shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  // Local write committed with quantity 2, then the other tab committed quantity 3.
  shop.state.alice = cart(3);
  await shop.signal();
  expect(shop.state.reads).toHaveLength(reads);
  await write.fulfill({ json: cart(2) });
  await shop.expectQuantity(3);
  expect(shop.state.reads).toHaveLength(reads + 1);

  // A subsequent successful mutation must not inherit the previous refresh signal.
  const nextWrite = await shop.startWrite();
  shop.state.alice = cart(4);
  await nextWrite.fulfill({ json: cart(4) });
  await shop.expectQuantity(4);
  expect(shop.state.reads).toHaveLength(reads + 1);
});

test('coalesces repeated storage and focus signals into one deferred read', async ({ page, shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  shop.state.alice = cart(3);
  await shop.signal();
  await shop.signal();
  await page.evaluate(() => {
    window.dispatchEvent(new Event('focus'));
    window.dispatchEvent(new Event('focus'));
  });
  expect(shop.state.reads).toHaveLength(reads);
  await write.fulfill({ json: cart(2) });
  await shop.expectQuantity(3);
  expect(shop.state.reads).toHaveLength(reads + 1);
});

test('a rejected write and a queued refresh share one reconciliation read', async ({ page, shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  shop.state.alice = cart(3);
  await shop.signal();
  await write.fulfill({ status: 409, json: { message: 'The cart changed in another request.' } });
  await shop.expectQuantity(3);
  await expect(page.getByRole('alert')).toContainText('The cart changed in another request.');
  expect(shop.state.reads).toHaveLength(reads + 1);
});

test('a lost mutation response still reconciles without a refresh signal', async ({ page, shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  shop.state.alice = cart(2);
  await write.abort('failed');
  await shop.expectQuantity(2);
  await expect(page.getByRole('alert')).toContainText('Unable to connect to the server.');
  expect(shop.state.reads).toHaveLength(reads + 1);
});

test('a successful mutation without refresh signals does not add a read', async ({ shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  shop.state.alice = cart(2);
  await write.fulfill({ json: cart(2) });
  await shop.expectQuantity(2);
  expect(shop.state.reads).toHaveLength(reads);
});

test('an old session cannot drain a newer mutation refresh queue', async ({ page, shop }) => {
  const aliceWrite = await shop.startWrite();
  await shop.signal();
  await shop.otherTab.evaluate(() => {
    localStorage.setItem('user', JSON.stringify({ userId: 2, email: 'bob@example.test', role: 'Customer' }));
    localStorage.setItem('accessToken', 'bob-test-token');
  });
  await shop.expectQuantity(7);
  await expect(page.getByRole('heading', { name: 'Bob board', exact: true })).toBeVisible();
  expect(shop.state.reads.at(-1)).toBe('Bearer bob-test-token');

  const bobWrite = await shop.startWrite('Bob board');
  const reads = shop.state.reads.length;
  await shop.signal();
  await aliceWrite.fulfill({ json: cart(2) });
  await expect(page.getByRole('alert')).toContainText('Your session changed.');
  await expect(page.locator('.saved-cart')).toHaveAttribute('aria-busy', 'true');
  expect(shop.state.reads).toHaveLength(reads);

  shop.state.bob = cart(9, 'Bob board');
  await bobWrite.fulfill({ json: cart(8, 'Bob board') });
  await shop.expectQuantity(9);
  expect(shop.state.reads).toHaveLength(reads + 1);
  expect(shop.state.reads.at(-1)).toBe('Bearer bob-test-token');
});

test('a failed deferred read exposes retry and does not leave the queue stuck', async ({ page, shop }) => {
  const write = await shop.startWrite();
  const reads = shop.state.reads.length;
  shop.state.alice = cart(3);
  await shop.signal();
  shop.state.failNextRead = true;
  await write.fulfill({ json: cart(2) });
  await expect(page.getByRole('heading', { name: 'Unable to load your cart' })).toBeVisible();
  expect(shop.state.reads).toHaveLength(reads + 1);
  await page.getByRole('button', { name: 'Try again', exact: true }).click();
  await shop.expectQuantity(3);
  expect(shop.state.reads).toHaveLength(reads + 2);
});
