import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError, ordersApi } from '../api/client';
import { useCart } from '../cart/CartProvider';
import ShopImage from '../components/ShopImage';
import { checkoutKey, money } from '../lib/shopping';

const blankAddress = {
  recipientName: '', streetAddress: '', city: '', province: '', postalCode: '', country: 'South Africa',
};

export default function Checkout() {
  const navigate = useNavigate();
  const { isAuthenticated, refresh: refreshCart } = useCart();
  const [preview, setPreview] = useState(null);
  const [address, setAddress] = useState(blankAddress);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setPreview(await ordersApi.preview()); }
    catch (failure) { setError(failure.message); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { if (isAuthenticated) void load(); }, [isAuthenticated, load]);

  if (!isAuthenticated) return <section className="shop-page shop-state">
    <h1>Sign in to checkout</h1><p>Your cart and order will be kept securely with your account.</p>
    <Link to="/login?returnTo=/checkout" className="btn-primary">Sign in</Link>
  </section>;
  if (loading) return <section className="shop-page shop-state" role="status">Preparing your checkout...</section>;
  if (error && !preview) return <section className="shop-page shop-state" role="alert">
    <h1>Unable to prepare checkout</h1><p>{error}</p><button className="btn-secondary" onClick={load}>Try again</button>
  </section>;
  if (!preview?.items?.length) return <section className="shop-page shop-state">
    <h1>Your cart is empty</h1><p>Add a piece to your collection before checking out.</p>
    <Link to="/boards" className="btn-primary">Browse the collection</Link>
  </section>;

  const unavailable = preview.items.some((item) => !item.isAvailable);
  const update = (event) => setAddress((current) => ({ ...current, [event.target.name]: event.target.value }));
  const submit = async (event) => {
    event.preventDefault(); setError(''); setSubmitting(true);
    const storageKey = `checkout-key:${preview.cartFingerprint}`;
    let key = sessionStorage.getItem(storageKey);
    if (!key) { key = checkoutKey(); sessionStorage.setItem(storageKey, key); }
    try {
      const order = await ordersApi.place(key, preview.cartFingerprint, address);
      sessionStorage.removeItem(storageKey);
      await refreshCart();
      navigate(`/orders/${order.orderId}`, { replace: true, state: { placed: true } });
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 0) {
        try {
          const recovered = await ordersApi.getByCheckout(key);
          sessionStorage.removeItem(storageKey);
          await refreshCart();
          navigate(`/orders/${recovered.orderId}`, { replace: true, state: { placed: true } });
          return;
        } catch (recoveryFailure) {
          if (!(recoveryFailure instanceof ApiError) || recoveryFailure.status !== 404) {
            setError('We could not confirm whether your order was received. Try again to safely resume this checkout.');
            return;
          }
        }
      }
      if (failure instanceof ApiError && failure.status === 409) await load();
      setError(failure.message);
    } finally { setSubmitting(false); }
  };

  return <section className="shop-page checkout-page" aria-busy={submitting}>
    <div className="shop-breadcrumb"><Link to="/cart">Cart</Link><span>/</span><span>Checkout</span></div>
    <header className="saved-cart-header"><div><p className="eyebrow">Secure checkout</p><h1>Delivery details</h1></div></header>
    {error && <p className="shop-alert" role="alert">{error}</p>}
    <div className="checkout-layout">
      <form className="checkout-form" onSubmit={submit}>
        <h2>Shipping address</h2>
        <div className="checkout-fields">
          <div className="shop-field checkout-wide"><label htmlFor="recipientName">Recipient name</label><input id="recipientName" name="recipientName" value={address.recipientName} onChange={update} maxLength="100" autoComplete="name" required /></div>
          <div className="shop-field checkout-wide"><label htmlFor="streetAddress">Street address</label><input id="streetAddress" name="streetAddress" value={address.streetAddress} onChange={update} maxLength="100" autoComplete="street-address" required /></div>
          <div className="shop-field"><label htmlFor="city">City</label><input id="city" name="city" value={address.city} onChange={update} maxLength="100" autoComplete="address-level2" required /></div>
          <div className="shop-field"><label htmlFor="province">Province</label><input id="province" name="province" value={address.province} onChange={update} maxLength="100" autoComplete="address-level1" required /></div>
          <div className="shop-field"><label htmlFor="postalCode">Postal code</label><input id="postalCode" name="postalCode" value={address.postalCode} onChange={update} pattern="[0-9]{4}" inputMode="numeric" autoComplete="postal-code" required /></div>
          <div className="shop-field"><label htmlFor="country">Country</label><input id="country" name="country" value={address.country} readOnly /></div>
        </div>
        <div className="checkout-payment"><h2>Payment</h2><p><strong>Payment on delivery</strong></p><p className="shop-note">This demonstration store records the order as unpaid. No card details are requested or charged.</p></div>
        <button type="submit" className="btn-primary detail-add" disabled={submitting || unavailable}>{submitting ? 'Placing order...' : `Place order · ${money(preview.totalAmount)}`}</button>
      </form>
      <aside className="saved-cart-summary checkout-summary"><h2>Order summary</h2>
        {preview.items.map((item) => <div className="checkout-line" key={item.cartItemId}>
          <ShopImage src={item.imageUrl} alt={item.productName} /><div><strong>{item.productName}</strong><p>{item.variantName} · Qty {item.quantity}</p>{!item.isAvailable && <p className="shop-alert">Stock changed — return to your cart.</p>}</div><span>{money(item.lineTotal)}</span>
        </div>)}
        <div className="saved-cart-summary-row"><span>Subtotal</span><span>{money(preview.subtotal)}</span></div>
        <div className="saved-cart-summary-row"><span>Delivery</span><span>{money(preview.deliveryFee)}</span></div>
        <div className="saved-cart-subtotal"><span>Total</span><strong>{money(preview.totalAmount)}</strong></div>
        {unavailable && <Link to="/cart" className="btn-secondary detail-add">Review cart</Link>}
      </aside>
    </div>
  </section>;
}
