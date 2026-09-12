import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { ApiError, cartApi } from '../api/client';

const CartContext = createContext(null);
const emptyCart = () => ({ items: [], totalQuantity: 0, subtotal: 0 });
const readToken = () => localStorage.getItem('accessToken');

export function CartProvider({ children }) {
  const [token, setToken] = useState(readToken);
  const [cart, setCart] = useState(emptyCart);
  const [loading, setLoading] = useState(Boolean(token));
  const [error, setError] = useState('');
  const [pending, setPending] = useState(false);
  const requestId = useRef(0);
  const activeRead = useRef(null);
  const mutation = useRef(null);
  const mounted = useRef(false);

  const refresh = useCallback(async () => {
    if (mutation.current) {
      // Coalesce signals on this mutation, not across sessions.
      mutation.current.refreshRequested = true;
      return;
    }
    const currentToken = readToken();
    const id = ++requestId.current;
    activeRead.current?.abort();
    const controller = new AbortController();
    activeRead.current = controller;
    if (!currentToken) {
      setCart(emptyCart()); setLoading(false); setError('');
      return;
    }
    setLoading(true); setError('');
    try {
      const next = await cartApi.get(controller.signal);
      if (mounted.current && id === requestId.current && currentToken === readToken()) setCart(next);
    } catch (failure) {
      if (failure.name !== 'AbortError' && mounted.current && id === requestId.current
          && currentToken === readToken()) setError(failure.message);
    } finally {
      if (mounted.current && id === requestId.current) setLoading(false);
    }
  }, []);

  useEffect(() => {
    mounted.current = true;
    const sessionChanged = () => {
      ++requestId.current;
      activeRead.current?.abort();
      mutation.current = null;
      setPending(false); setCart(emptyCart()); setError('');
      const nextToken = readToken();
      setToken(nextToken);
      setLoading(Boolean(nextToken));
      // Also refresh when saveSession reuses the current token.
      void refresh();
    };
    const storageChanged = (event) => {
      if (event.key === 'accessToken' || event.key === null) sessionChanged();
      else if (event.key === 'cart-updated') void refresh();
    };
    const focus = () => void refresh();
    window.addEventListener('session-changed', sessionChanged);
    window.addEventListener('storage', storageChanged);
    window.addEventListener('focus', focus);
    void refresh();
    return () => {
      mounted.current = false;
      ++requestId.current;
      activeRead.current?.abort();
      window.removeEventListener('session-changed', sessionChanged);
      window.removeEventListener('storage', storageChanged);
      window.removeEventListener('focus', focus);
    };
  }, [refresh]);

  const mutate = useCallback(async (operation) => {
    const currentToken = readToken();
    if (!currentToken) throw new ApiError('Please sign in to use your cart.', 401);
    if (mutation.current) throw new ApiError('Your previous cart change is still saving.', 409);
    const marker = { refreshRequested: false };
    mutation.current = marker;
    ++requestId.current;
    activeRead.current?.abort();
    setPending(true); setLoading(false); setError('');
    let failed = false;
    try {
      const next = await operation();
      if (!mounted.current || currentToken !== readToken() || mutation.current !== marker)
        throw new ApiError('Your session changed. Please check the current cart.', 401);
      setCart(next);
      // Signal other tabs without storing cart data in the browser.
      try { localStorage.setItem('cart-updated', `${Date.now()}-${Math.random()}`); } catch { /* Cart is already saved. */ }
      return next;
    } catch (failure) {
      failed = true;
      throw failure;
    } finally {
      if (mounted.current && mutation.current === marker) {
        mutation.current = null;
        setPending(false);
        // Reconcile lost responses and refresh signals received while this write was pending.
        if (failed || marker.refreshRequested) void refresh();
      }
    }
  }, [refresh]);

  return (
    <CartContext.Provider value={{
      cart, loading, error, pending, isAuthenticated: Boolean(token), refresh,
      addItem: (productId, variantId, quantity) => mutate(() => cartApi.add(productId, variantId, quantity)),
      updateItem: (itemId, quantity) => mutate(() => cartApi.update(itemId, quantity)),
      removeItem: (itemId) => mutate(() => cartApi.remove(itemId)),
    }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) throw new Error('useCart requires CartProvider.');
  return context;
}
