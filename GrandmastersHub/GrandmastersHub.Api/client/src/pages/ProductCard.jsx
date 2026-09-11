import { Link } from 'react-router-dom';

const ProductCard = ({ product }) => {
  return (
    <div className="product-card">
      <div 
        className="product-image" 
        style={product.imageUrl ? { backgroundImage: `url(${product.imageUrl})` } : undefined}
        role="img"
        aria-label={product.name}
      ></div>
      <div className="product-info">
        <span className="product-category">{product.category}</span>
        <h3 className="product-name">{product.name}</h3>
        {product.description && <p className="product-description">{product.description}</p>}
      </div>
      <div className="card-footer">
        <span className="product-price">R {Number(product.price).toLocaleString('en-ZA', { minimumFractionDigits: 2 })}</span>
        <Link to={`/product/${product.id}`} className="view-details">
          View Details <span>→</span>
        </Link>
      </div>
    </div>
  );
};

export default ProductCard;
