import test, { beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import { apiRequest, cartApi, clearSession, saveSession } from '../src/api/client.js';

let storage;
let sessionEvents;
beforeEach(() => {
  storage = new Map(); sessionEvents = 0;
  globalThis.localStorage = {
    getItem: (key) => storage.get(key) ?? null,
    setItem: (key, value) => storage.set(key, value),
    removeItem: (key) => storage.delete(key),
  };
  globalThis.window = new EventTarget();
  window.addEventListener('session-changed', () => sessionEvents++);
});
const json = (body, status = 200, type = 'application/json') => new Response(JSON.stringify(body), {
  status, headers: { 'Content-Type': type },
});

test('session save and logout notify the cart provider', () => {
  saveSession({ accessToken: 'token', userId: 1, email: 'user@example.test', role: 'Customer' });
  assert.equal(localStorage.getItem('accessToken'), 'token');
  clearSession();
  assert.equal(localStorage.getItem('accessToken'), null);
  assert.equal(sessionEvents, 2);
});
test('cart writes send bearer authentication and only product/option/quantity', async () => {
  storage.set('accessToken', 'token');
  globalThis.fetch = async (url, options) => {
    assert.equal(url, '/api/v1/cart/items');
    assert.equal(options.method, 'POST');
    assert.equal(options.headers.get('Authorization'), 'Bearer token');
    assert.deepEqual(JSON.parse(options.body), { productId: 1, productVariantId: 2, quantity: 3 });
    return json({ items: [], totalQuantity: 0, subtotal: 0 });
  };
  await cartApi.add(1, 2, 3);
});
test('cart requests without a session do not reach the network', async () => {
  globalThis.fetch = () => assert.fail('must not call fetch');
  await assert.rejects(cartApi.get(), { status: 401 });
});
test('validation problem JSON returns the actual field error', async () => {
  globalThis.fetch = async () => json({ title: 'Validation failed', errors: { Quantity: ['Quantity must be between 1 and 99.'] } }, 400, 'application/problem+json');
  await assert.rejects(apiRequest('cart'), { message: 'Quantity must be between 1 and 99.', status: 400 });
});
test('current-token 401 clears the session', async () => {
  storage.set('accessToken', 'old');
  globalThis.fetch = async () => json({ message: 'Expired' }, 401);
  await assert.rejects(cartApi.get(), { status: 401 });
  assert.equal(localStorage.getItem('accessToken'), null);
});
test('an old request 401 cannot clear a newer login', async () => {
  storage.set('accessToken', 'old');
  globalThis.fetch = async () => { storage.set('accessToken', 'new'); return json({}, 401); };
  await assert.rejects(cartApi.get(), { status: 401 });
  assert.equal(localStorage.getItem('accessToken'), 'new');
});
test('network failures stay failures rather than becoming an empty cart', async () => {
  globalThis.fetch = async () => { throw new TypeError('offline'); };
  await assert.rejects(apiRequest('products'), { status: 0 });
});
test('canceled reads preserve AbortError', async () => {
  globalThis.fetch = async () => { throw new DOMException('Aborted', 'AbortError'); };
  await assert.rejects(apiRequest('products'), { name: 'AbortError' });
});
test('a 204 response is not parsed as JSON', async () => {
  globalThis.fetch = async () => new Response(null, { status: 204, headers: { 'Content-Type': 'application/json' } });
  assert.equal(await apiRequest('anything'), null);
});
test('malformed JSON is reported as an API error', async () => {
  globalThis.fetch = async () => new Response('{oops', { headers: { 'Content-Type': 'application/json' } });
  await assert.rejects(apiRequest('products'), { name: 'ApiError', message: 'The server returned an unreadable response. Please try again.' });
});
test('quantity updates and removals use the correct endpoint and verbs', async () => {
  storage.set('accessToken', 'token');
  const calls = [];
  globalThis.fetch = async (url, options) => { calls.push([url, options.method, options.body]); return json({ items: [] }); };
  await cartApi.update(42, 2); await cartApi.remove(42);
  assert.deepEqual(calls, [
    ['/api/v1/cart/items/42', 'PUT', '{"quantity":2}'],
    ['/api/v1/cart/items/42', 'DELETE', undefined],
  ]);
});
