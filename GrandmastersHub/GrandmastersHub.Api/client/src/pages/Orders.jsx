import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ordersApi } from '../api/client';
import { money } from '../lib/shopping';

export default function Orders() {
  const [page, setPage] = useState(1); const [data, setData] = useState(null); const [error, setError] = useState('');
  const load = useCallback(async () => { setError(''); try { setData(await ordersApi.list(page, 10)); } catch (failure) { setError(failure.message); } }, [page]);
  useEffect(() => { void load(); }, [load]);
  if (error) return <section className="shop-page shop-state" role="alert"><h1>Unable to load orders</h1><p>{error}</p><button className="btn-secondary" onClick={load}>Try again</button></section>;
  if (!data) return <section className="shop-page shop-state" role="status">Loading your orders...</section>;
  return <section className="shop-page orders-page"><p className="eyebrow">Your account</p><h1>Order history</h1>
    {!data.items.length ? <div className="shop-state"><p>You have not placed an order yet.</p><Link to="/boards" className="btn-primary">Browse the collection</Link></div> : <div className="orders-list">{data.items.map((order) => <Link to={`/orders/${order.orderId}`} className="order-card" key={order.orderId}><div><strong>{order.orderNumber}</strong><p>{new Date(order.createdAt).toLocaleDateString('en-ZA', { dateStyle: 'medium' })} · {order.totalQuantity} item{order.totalQuantity === 1 ? '' : 's'}</p></div><div><span>{order.status} · {order.paymentStatus}</span><strong>{money(order.totalAmount)}</strong></div></Link>)}</div>}
    {data.totalCount > data.pageSize && <div className="order-pagination"><button className="btn-secondary" disabled={page === 1} onClick={() => setPage((value) => value - 1)}>Previous</button><span>Page {page}</span><button className="btn-secondary" disabled={page * data.pageSize >= data.totalCount} onClick={() => setPage((value) => value + 1)}>Next</button></div>}
  </section>;
}
