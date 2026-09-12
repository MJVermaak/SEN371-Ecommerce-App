import test from 'node:test';
import assert from 'node:assert/strict';
import { checkoutKey, collectionPath, fallbackImage, money, remainingQuantity, safeReturnTo, validQuantity } from '../src/lib/shopping.js';

for (const name of ['Boards', 'Clocks', 'Books', 'Bespoke']) {
  test(`${name} uses its collection route and a local fallback image`, () => {
    assert.equal(collectionPath(name), `/${name.toLowerCase()}`);
    assert.match(fallbackImage(name), /^\/images\//);
  });
}
test('prices use rand with two decimal places', () => {
  assert.match(money(125.5), /125[.,]50/);
  assert.match(money(125.5), /R/);
});
test('quantity validation rejects fractions, blanks, negative values and over-stock requests', () => {
  for (const value of ['', 0, -1, 1.5, NaN, Infinity, 6]) assert.equal(validQuantity(value, 5), false);
  assert.equal(validQuantity(5, 5), true);
});
test('remaining capacity accounts for existing cart units and the 99-unit limit', () => {
  assert.equal(remainingQuantity(5, 2), 3);
  assert.equal(remainingQuantity(200, 98), 1);
  assert.equal(remainingQuantity(2, 5), 0);
  assert.equal(remainingQuantity(undefined), 0);
});
test('login return path allows only known local routes', () => {
  assert.equal(safeReturnTo('/product/24'), '/product/24');
  assert.equal(safeReturnTo('/cart'), '/cart');
  for (const value of [null, '//evil.example', 'https://evil.example', '/\\evil.example', '/product/1?next=bad', '/product/0'])
    assert.equal(safeReturnTo(value), '/profile');
});
test('checkout and owned order pages are valid login return paths', () => {
  assert.equal(safeReturnTo('/checkout'), '/checkout');
  assert.equal(safeReturnTo('/orders'), '/orders');
  assert.equal(safeReturnTo('/orders/42'), '/orders/42');
});
test('checkout references are valid version 4 UUIDs', () => {
  assert.match(checkoutKey(), /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i);
});
