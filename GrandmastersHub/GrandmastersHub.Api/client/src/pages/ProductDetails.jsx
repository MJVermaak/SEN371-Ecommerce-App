import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { catalogApi } from '../api/client';
import { useCart } from '../cart/CartProvider';
import ShopImage from '../components/ShopImage';
import { collectionPath, money, remainingQuantity, validQuantity } from '../lib/shopping';

export default function ProductDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { cart, isAuthenticated, addItem, pending, loading: cartLoading, error: cartError, refresh } = useCart();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [notFound, setNotFound] = useState(false);
  const [reload, setReload] = useState(0);
  const [variantId, setVariantId] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [imageIndex, setImageIndex] = useState(0);
  const [feedback, setFeedback] = useState('');
  const [actionError, setActionError] = useState('');

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true); setError(''); setProduct(null); setNotFound(false);
    setFeedback(''); setActionError(''); setImageIndex(0); setQuantity(1);
    if (!/^[1-9]\d*$/.test(id) || Number(id) > 2147483647) {
      setNotFound(true); setLoading(false);
      return () => controller.abort();
    }
    catalogApi.getProduct(id, controller.signal).then((data) => {
      if (controller.signal.aborted) return;
      if (!data) { setNotFound(true); return; }
      setProduct(data);
      const option = data.variants?.find((variant) => variant.stockQuantity > 0) || data.variants?.[0];
      setVariantId(option?.productVariantId ?? null);
    }).catch((failure) => {
      if (controller.signal.aborted) return;
      if (failure.status === 404) setNotFound(true);
      else setError(failure.message);
    }).finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [id, reload]);

  if (loading) return <section className="shop-page shop-state" role="status">Loading product...</section>;
  if (notFound) return <section className="shop-page shop-state">
    <h1>Product not found</h1><p>This product may have been removed.</p>
    <Link to="/boards" className="btn-primary">Browse the collection</Link>
  </section>;
  if (error || !product) return <section className="shop-page shop-state" role="alert">
    <h1>Unable to load this product</h1><p>{error}</p>
    <button className="btn-secondary" onClick={() => setReload((value) => value + 1)}>Try again</button>
  </section>;

  const variant = product.variants?.find((option) => option.productVariantId === variantId);
  const inCart = cart.items.filter((item) => item.productVariantId === variantId)
    .reduce((sum, item) => sum + item.quantity, 0);
  const available = remainingQuantity(variant?.stockQuantity, inCart);
  const images = product.imageUrls?.length ? product.imageUrls : [product.imageUrl];
  const allowed = variant && validQuantity(quantity, available) && !pending && !cartLoading && !cartError;

  const add = async (event) => {
    event.preventDefault(); setActionError(''); setFeedback('');
    if (!isAuthenticated) { navigate(`/login?returnTo=/product/${product.productId}`); return; }
    if (!allowed) return;
    try {
      await addItem(product.productId, variantId, quantity);
      setFeedback(`${quantity} ${quantity === 1 ? 'item' : 'items'} added to your cart.`);
      setQuantity(1);
    } catch (failure) {
      if (failure.status === 401) navigate(`/login?returnTo=/product/${product.productId}`);
      else setActionError(failure.message);
    }
  };

  return (
    <section className="shop-page product-detail">
      <nav className="shop-breadcrumb" aria-label="Breadcrumb">
        <Link to="/">Home</Link><span aria-hidden="true">/</span>
        <Link to={collectionPath(product.categoryName)}>{product.categoryName || 'Collection'}</Link>
        <span aria-hidden="true">/</span><span aria-current="page">{product.name}</span>
      </nav>
      <div className="detail-layout">
        <div className="detail-gallery">
          <div className="detail-main-image">
            <ShopImage src={images[imageIndex]} category={product.categoryName} alt={product.name} />
          </div>
          {images.length > 1 && <div className="detail-thumbnails" aria-label="Product images">
            {images.map((image, index) => <button key={`${image}-${index}`} type="button"
              aria-label={`Show image ${index + 1}`} aria-pressed={index === imageIndex}
              onClick={() => setImageIndex(index)}>
              <ShopImage src={image} category={product.categoryName} alt="" />
            </button>)}
          </div>}
        </div>
        <div className="detail-info">
          <p className="eyebrow">{product.categoryName}</p>
          <h1>{product.name}</h1>
          <p className="detail-price">{money(variant?.price ?? product.price)}</p>
          <p className="detail-description">{product.description || 'Explore this piece from our collection.'}</p>
          <form onSubmit={add} className="detail-purchase">
            {!!product.variants?.length && <div className="shop-field">
              <label htmlFor="product-variant">Option</label>
              <select id="product-variant" value={variantId ?? ''} disabled={pending}
                onChange={(event) => { setVariantId(Number(event.target.value)); setQuantity(1); setFeedback(''); setActionError(''); }}>
                {product.variants.map((option) => <option key={option.productVariantId} value={option.productVariantId}>
                  {option.name} {option.stockQuantity < 1 ? '(out of stock)' : ''}
                </option>)}
              </select>
            </div>}
            <p className={`stock-status ${variant?.stockQuantity > 0 ? '' : 'stock-unavailable'}`}>
              {!variant ? 'No purchasable options are available yet.'
                : variant.stockQuantity > 0 ? `${variant.stockQuantity} in stock` : 'Currently out of stock'}
              {inCart > 0 && `. ${inCart} already in your cart.`}
            </p>
            {available > 0 && <div className="shop-field quantity-field">
              <label htmlFor="product-quantity">Quantity</label>
              <input id="product-quantity" type="number" min="1" max={available} step="1" required
                disabled={pending} value={quantity}
                onChange={(event) => { setQuantity(event.target.value === '' ? '' : Number(event.target.value)); setFeedback(''); }} />
            </div>}
            {isAuthenticated ? <button type="submit" className="btn-primary detail-add" disabled={!allowed}>
              {pending ? 'Adding...' : cartLoading ? 'Loading cart...' : available > 0 ? 'Add to cart' : 'Unavailable'}
            </button> : <Link className="btn-primary detail-add" to={`/login?returnTo=/product/${product.productId}`}>
              Sign in to add to cart
            </Link>}
            {cartError && <div className="shop-alert" role="alert"><p>{cartError}</p>
              <button type="button" className="shop-text-button" onClick={refresh}>Reload cart</button></div>}
            {actionError && <p className="shop-alert" role="alert">{actionError}</p>}
            <div aria-live="polite">{feedback && <p className="shop-success">{feedback} <Link to="/cart">View cart</Link></p>}</div>
            <p className="shop-note">Your cart is saved to your account. Adding an item does not reserve stock.</p>
          </form>
          <dl className="detail-facts">
            <div><dt>Collection</dt><dd>{product.categoryName}</dd></div>
            <div><dt>Product reference</dt><dd>GH-{product.productId}</dd></div>
          </dl>
          <Link className="shop-back" to={collectionPath(product.categoryName)}>Back to the collection</Link>
        </div>
      </div>
    </section>
  );
}
