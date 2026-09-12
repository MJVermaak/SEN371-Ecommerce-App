import { useCallback, useEffect, useState } from 'react';
import { Link, useLocation, useParams } from 'react-router-dom';
import { ordersApi } from '../api/client';
import ShopImage from '../components/ShopImage';
import { money } from '../lib/shopping';

export default function OrderDetails() {
  const { id } = useParams(); const location = useLocation();
  const [order, setOrder] = useState(null); const [error, setError] = useState('');
  const load = useCallback(async () => { setError(''); try { setOrder(await ordersApi.get(id)); } catch (failure) { setError(failure.message); } }, [id]);
  useEffect(() => { void load(); }, [load]);
  if (error) return <section className="shop-page shop-state" role="alert"><h1>Unable to load order</h1><p>{error}</p><button className="btn-secondary" onClick={load}>Try again</button></section>;
  if (!order) return <section className="shop-page shop-state" role="status">Loading your order...</section>;
  return <section className="shop-page order-page">
    <p className="eyebrow">{location.state?.placed ? 'Order placed' : 'Order details'}</p><h1>{location.state?.placed ? 'Thank you for your order' : order.orderNumber}</h1>
    <p className="order-lead">Order <strong>{order.orderNumber}</strong> was received on {new Date(order.createdAt).toLocaleDateString('en-ZA', { dateStyle: 'long' })}.</p>
    <div className="order-status"><span>Order: <strong>{order.status}</strong></span><span>Payment: <strong>{order.paymentStatus}</strong></span><span>Method: <strong>{order.paymentMethod}</strong></span></div>
    <div className="checkout-layout"><div className="order-lines"><h2>Items</h2>{order.items.map((item) => <article className="checkout-line" key={item.productVariantId}>
      <ShopImage src={item.imageUrl} alt={item.productName} /><div><strong>{item.productName}</strong><p>{item.variantName} · Qty {item.quantity}</p></div><span>{money(item.lineTotal)}</span>
    </article>)}</div><aside className="saved-cart-summary"><h2>Receipt</h2><div className="saved-cart-summary-row"><span>Subtotal</span><span>{money(order.subtotal)}</span></div><div className="saved-cart-summary-row"><span>Delivery</span><span>{money(order.deliveryFee)}</span></div><div className="saved-cart-subtotal"><span>Total</span><strong>{money(order.totalAmount)}</strong></div>
      {order.shippingAddress && <div className="order-address"><h3>Deliver to</h3><p>{order.shippingAddress.recipientName}<br />{order.shippingAddress.streetAddress}<br />{order.shippingAddress.city}, {order.shippingAddress.province} {order.shippingAddress.postalCode}<br />{order.shippingAddress.country}</p></div>}
    </aside></div><div className="order-actions"><Link to="/orders" className="btn-secondary">View all orders</Link><Link to="/boards" className="btn-primary">Continue shopping</Link></div>
  </section>;
}
