import { useCallback, useEffect, useState } from 'react';
import { catalogApi } from '../api/client';
import ProductCard from './ProductCard';

const CATEGORY_IMAGES = {
  boards: '/images/Shopping-plain-chess-board.jpg',
  clocks: '/images/Shopping-clock-1.png',
  books: '/images/book-1.png',
  bespoke: '/images/Volcanic.png',
};

const normalize = (value = '') => value.trim().toLowerCase();

const Catalog = ({ title, categoryName }) => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  const loadProducts = useCallback(() => setReloadKey((value) => value + 1), []);

  useEffect(() => {
    const controller = new AbortController();

    const fetchProducts = async () => {
      try {
        setLoading(true);
        setError('');
import { useEffect, useState } from 'react';
import ProductCard from '../pages/ProductCard';

const Catalog = ({ title, categoryName }) => {

    const [visible, setVisible] = useState(false);

    useEffect(() => {
        setVisible(false);
        const id = requestAnimationFrame(() => setVisible(true));
        return () => cancelAnimationFrame(id);
    }, [categoryName]);
  // A larger master list of mock data covering all categories
  const allMockProducts = [
    {
      id: 1,
      category: 'boards',
      name: 'Sovereign Burled Walnut & Brass Board',
      price: 1450.00,
      imageUrl: '/images/Shopping-plain-chess-board.jpg'
    },
    {
      id: 2,
      category: 'boards',
      name: 'Black and White Classic Chess Board',
      price: 650.00,
      imageUrl: '/images/shopping-black-white-board.png'
    },
    {
      id: 3,
      category: 'clocks',
      name: 'The Grandmaster Mechanical Swiss Clock',
      price: 890.00,
      imageUrl: '/images/Shopping-clock-1.png'
    },
    {
      id: 4,
      category: 'clocks',
      name: 'Digital Tournament Clock Pro',
      price: 120.00,
      imageUrl: '/images/Digital-clock.png'
    },
    {
      id: 5,
      category: 'books',
      name: 'Modern Chess Openings - 15th Edition',
      price: 45.00,
      imageUrl: '/images/book-1.png'
    },
    {
      id: 6,
      category: 'bespoke',
      name: 'Hand-carved Obsidian & Volcanic Glass Set',
      price: 2100.00,
      imageUrl: '/images/Volcanic.png'
    }
  ];

        const data = await catalogApi.getProducts(controller.signal);
        setProducts(Array.isArray(data) ? data : []);
      } catch (requestError) {
        if (requestError?.name !== 'AbortError') {
          setProducts([]);
          setError(requestError instanceof Error
            ? requestError.message
            : 'Unable to load the catalog.');
        }
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    };

    fetchProducts();
    return () => controller.abort();
  }, [reloadKey]);

  const requestedCategory = normalize(categoryName);
  const displayProducts = products
    .filter((product) => {
      if (!requestedCategory) return true;
      return normalize(product.categoryName).includes(requestedCategory);
    })
    .map((product) => ({
      ...product,
      id: product.productId,
      category: product.categoryName || categoryName,
      imageUrl: product.imageUrl || CATEGORY_IMAGES[requestedCategory],
    }));

  return (
    <section className="catalog-container" aria-busy={loading}>
      <h1 className="section-title">{title}</h1>

      {loading && (
        <div className="catalog-state" role="status">
          Loading the collection...
        </div>
      )}

      {!loading && error && (
        <div className="catalog-state catalog-error" role="alert">
          <p>{error}</p>
          <button type="button" className="btn-secondary" onClick={loadProducts}>
            Try Again
          </button>
        </div>
      )}

      {!loading && !error && displayProducts.length === 0 && (
        <div className="catalog-state">
          No products are available in this collection yet.
        </div>
      )}

      {!loading && !error && displayProducts.length > 0 && (
        <div className="product-grid">
          {displayProducts.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </section>
      <div className={`catalog-container ${visible ? 'fade-page' : ''}`}>
      <h2 className="section-title">{title}</h2>
      <div className="product-grid">
        {displayProducts.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
};

export default Catalog;
