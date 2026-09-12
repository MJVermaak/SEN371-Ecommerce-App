import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../cart/CartProvider';
import ShopImage from '../components/ShopImage';
import { money } from '../lib/shopping';

export default function Cart() {
  const { cart, loading, error, pending, isAuthenticated, refresh, updateItem, removeItem } = useCart();
  const [feedback, setFeedback] = useState('');
  const [actionError, setActionError] = useState('');
  useEffect(() => { void refresh(); }, [refresh]);

  const change = async (operation, message) => {
    setActionError(''); setFeedback('');
    try { await operation(); setFeedback(message); }
    catch (failure) { setActionError(failure.message); }
  };

  if (!isAuthenticated) return <section className="shop-page shop-state">
    <h1>Your cart, saved for you</h1><p>Sign in to add products and access your saved cart.</p>
    <Link to="/login?returnTo=/cart" className="btn-primary">Sign in</Link>
    <Link to="/boards" className="shop-back">Continue shopping</Link>
  </section>;
  if (loading) return <section className="shop-page shop-state" role="status">Loading your cart...</section>;
  if (error) return <section className="shop-page shop-state" role="alert">
    <h1>Unable to load your cart</h1><p>{error}</p>
    <button type="button" className="btn-secondary" onClick={refresh}>Try again</button>
  </section>;
  if (!cart.items.length) return <section className="shop-page shop-state">
    <p className="eyebrow">Your collection starts here</p><h1>Your cart is empty</h1>
    <p>Explore boards, clocks, books and bespoke sets.</p>
    <Link to="/boards" className="btn-primary">Explore the collection</Link>
  </section>;

  return <section className="shop-page saved-cart" aria-busy={pending}>
    <header className="saved-cart-header"><div><p className="eyebrow">Your selection</p><h1>Your cart</h1></div>
      <p>{cart.totalQuantity} {cart.totalQuantity === 1 ? 'item' : 'items'}</p></header>
    {actionError && <p className="shop-alert" role="alert">{actionError}</p>}
    <p className="shop-success" role="status">{feedback}</p>
    <div className="saved-cart-layout">
      <div className="saved-cart-items">
        {cart.items.map((item) => <article className="saved-cart-item" key={item.cartItemId}>
          <Link to={`/product/${item.productId}`} className="saved-cart-image">
            <ShopImage src={item.imageUrl} category={item.categoryName} alt={item.name} />
          </Link>
          <div className="saved-cart-info">
            <p className="eyebrow">{item.categoryName}</p>
            <h2><Link to={`/product/${item.productId}`}>{item.name}</Link></h2>
            <p>{item.variantName}</p><p>{money(item.unitPrice)} each</p>
            <div className="saved-cart-controls">
              <div className="quantity-stepper" role="group" aria-label={`Quantity for ${item.name}`}>
                <button type="button" aria-label={`Decrease quantity of ${item.name}`}
                  disabled={pending || item.quantity <= 1} onClick={() => change(
                    () => updateItem(item.cartItemId, item.quantity - 1), 'Quantity updated.')}>&minus;</button>
                <span aria-label={`Quantity: ${item.quantity}`}>{item.quantity}</span>
                <button type="button" aria-label={`Increase quantity of ${item.name}`}
                  disabled={pending || item.quantity >= Math.min(99, item.stockQuantity)} onClick={() => change(
                    () => updateItem(item.cartItemId, item.quantity + 1), 'Quantity updated.')}>+</button>
              </div>
              <button type="button" className="shop-text-button" disabled={pending}
                onClick={() => change(() => removeItem(item.cartItemId), `${item.name} removed.`)}>Remove</button>
            </div>
            {!item.isAvailable && <div className="shop-alert">
              <p>{item.stockQuantity > 0 ? `Only ${item.stockQuantity} are now available. Reduce the quantity.` : 'This option is now out of stock. Please remove it.'}</p>
              {item.stockQuantity > 0 && <button type="button" className="shop-text-button" disabled={pending}
                onClick={() => change(() => updateItem(item.cartItemId, Math.min(99, item.stockQuantity)), 'Quantity updated.')}>
                Set quantity to {Math.min(99, item.stockQuantity)}
              </button>}
            </div>}
          </div>
          <strong className="saved-cart-line-total">{money(item.lineTotal)}</strong>
        </article>)}
        <Link to="/boards" className="shop-back">Continue shopping</Link>
      </div>
      <aside className="saved-cart-summary">
        <h2>Cart summary</h2>
        <div className="saved-cart-summary-row"><span>Items</span><span>{cart.totalQuantity}</span></div>
        <div className="saved-cart-subtotal"><span>Subtotal</span><strong>{money(cart.subtotal)}</strong></div>
        <p className="shop-note">Your products are saved to your account. Prices and availability come from the current catalog.</p>
        <Link to="/checkout" className="btn-primary detail-add">Proceed to checkout</Link>
        <div className="checkout-notice"><h3>Payment on delivery</h3>
          <p>Delivery and the final total are confirmed on the next step. No online payment is taken.</p></div>
      </aside>
    </div>
  </section>;
}
