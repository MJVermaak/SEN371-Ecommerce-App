import ProductCard from '../pages/ProductCard';

const Catalog = ({ title, categoryName }) => {
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

  // Filter the list to only include products that match the current page's category
  const displayProducts = allMockProducts.filter(product => product.category === categoryName);

  return (
    <div className="catalog-container">
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