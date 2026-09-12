const configuredBaseUrl = import.meta.env?.VITE_API_BASE_URL || '/api/v1';
const API_BASE_URL = configuredBaseUrl.replace(/\/$/, '');

export class ApiError extends Error {
  constructor(message, status, details) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

const getErrorMessage = (data, fallback) => {
  if (data?.message) return data.message;
  if (data?.detail) return data.detail;

  const validationMessage = data?.errors
    ? Object.values(data.errors).flat().find(Boolean)
    : null;

  return validationMessage || data?.title || fallback;
};

export const clearSession = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('user');
  window.dispatchEvent(new Event('session-changed'));
};

export const saveSession = (auth) => {
  localStorage.setItem('accessToken', auth.accessToken);
  localStorage.setItem('user', JSON.stringify({
    userId: auth.userId,
    email: auth.email,
    role: auth.role,
  }));
  window.dispatchEvent(new Event('session-changed'));
};

export async function apiRequest(path, { body, auth = false, headers, ...options } = {}) {
  const requestHeaders = new Headers(headers);
  requestHeaders.set('Accept', 'application/json');

  if (body !== undefined) requestHeaders.set('Content-Type', 'application/json');

  const token = auth ? localStorage.getItem('accessToken') : null;
  if (auth) {
    if (!token) throw new ApiError('Please sign in to continue.', 401);
    requestHeaders.set('Authorization', `Bearer ${token}`);
  }

  let response;
  try {
    response = await fetch(`${API_BASE_URL}/${path.replace(/^\//, '')}`, {
      ...options,
      headers: requestHeaders,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch (error) {
    if (error?.name === 'AbortError') throw error;
    throw new ApiError('Unable to connect to the server. Please try again.', 0);
  }

  const contentType = response.headers.get('content-type') || '';
  const isJson = contentType.includes('application/json') || contentType.includes('+json');
  let data = null;
  if (isJson && response.status !== 204) {
    try { data = await response.json(); }
    catch {
      throw new ApiError('The server returned an unreadable response. Please try again.', response.status);
    }
  }

  if (!response.ok) {
    // An old request must not log out a newly signed-in account.
    if (auth && response.status === 401 && localStorage.getItem('accessToken') === token) clearSession();
    throw new ApiError(
      getErrorMessage(data, `The request failed with status ${response.status}.`),
      response.status,
      data,
    );
  }

  return data;
}

export const authApi = {
  login: (credentials) => apiRequest('auth/login', { method: 'POST', body: credentials }),
  register: (details) => apiRequest('auth/register', { method: 'POST', body: details }),
  getProfile: () => apiRequest('auth/me', { auth: true }),
};

export const catalogApi = {
  getProducts: (signal) => apiRequest('products', { signal }),
  getProduct: (id, signal) => apiRequest(`products/${id}`, { signal }),
  getCategories: (signal) => apiRequest('categories', { signal }),
};

export const cartApi = {
  get: (signal) => apiRequest('cart', { auth: true, signal }),
  add: (productId, productVariantId, quantity) => apiRequest('cart/items', {
    method: 'POST', auth: true, body: { productId, productVariantId, quantity },
  }),
  update: (itemId, quantity) => apiRequest(`cart/items/${itemId}`, {
    method: 'PUT', auth: true, body: { quantity },
  }),
  remove: (itemId) => apiRequest(`cart/items/${itemId}`, { method: 'DELETE', auth: true }),
};

export const ordersApi = {
  preview: (signal) => apiRequest('orders/checkout', { auth: true, signal }),
  place: (checkoutKey, cartFingerprint, shippingAddress) => apiRequest('orders', {
    method: 'POST', auth: true, body: { checkoutKey, cartFingerprint, shippingAddress },
  }),
  list: (page = 1, pageSize = 10, signal) => apiRequest(
    `orders?page=${encodeURIComponent(page)}&pageSize=${encodeURIComponent(pageSize)}`,
    { auth: true, signal },
  ),
  get: (id, signal) => apiRequest(`orders/${id}`, { auth: true, signal }),
  getByCheckout: (checkoutKey, signal) => apiRequest(
    `orders/by-checkout/${encodeURIComponent(checkoutKey)}`, { auth: true, signal },
  ),
};
