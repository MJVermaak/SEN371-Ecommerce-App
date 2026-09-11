const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api/v1';
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
  if (data?.title) return data.title;

  const validationMessage = data?.errors
    ? Object.values(data.errors).flat().find(Boolean)
    : null;

  return validationMessage || fallback;
};

export const clearSession = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('user');
};

export const saveSession = (auth) => {
  localStorage.setItem('accessToken', auth.accessToken);
  localStorage.setItem('user', JSON.stringify({
    userId: auth.userId,
    email: auth.email,
    role: auth.role,
  }));
};

export async function apiRequest(path, { body, auth = false, headers, ...options } = {}) {
  const requestHeaders = new Headers(headers);
  requestHeaders.set('Accept', 'application/json');

  if (body !== undefined) requestHeaders.set('Content-Type', 'application/json');

  if (auth) {
    const token = localStorage.getItem('accessToken');
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

  const isJson = response.headers.get('content-type')?.includes('application/json');
  const data = isJson ? await response.json() : null;

  if (!response.ok) {
    if (auth && response.status === 401) clearSession();
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
