import { Link } from 'react-router-dom';

const Cart = () => {
    const cartItems = [
        {
            id: 1,
            name: "Sovereign Burled Walnut Board",
            variant: "Walnut / Premium",
            price: 1450,
            quantity: 1,
            image: "/images/Shopping-plain-chess-board.jpg"
        },
        {
            id: 2,
            name: "Digital Tournament Clock",
            variant: "Pro Edition",
            price: 890,
            quantity: 2,
            image: "/images/Shopping-clock-1.png"
        },
        {
            id: 3,
            name: "Modern Chess Openings",
            variant: "15th Edition",
            price: 45,
            quantity: 1,
            image: "/images/book-1.png"
        }
    ];

    return (
        <div className="cart-page">
            <div className="cart-page-header">
                <h1>Your Cart</h1>
                <span>{cartItems.length} items</span>
            </div>

            <div className="cart-layout">

                <div className="cart-products">
                    {cartItems.map(item => (
                        <div key={item.id} className="cart-item">
                            <img src={item.image} alt={item.name} />

                            <div className="cart-item-info">
                                <h3>{item.name}</h3>
                                <p>{item.variant}</p>

                                <div className="qty-box">
                                    <button>-</button>
                                    <span>{item.quantity}</span>
                                    <button>+</button>
                                </div>
                            </div>

                            <div className="cart-price">
                                <strong>
                                    R. {item.price.toLocaleString('en-ZA', {
                                        minimumFractionDigits: 2
                                    })}
                                </strong>

                                <button className="remove-btn">Remove</button>
                            </div>
                        </div>
                    ))}
                </div>

                <div className="cart-summary">
                    <h2>Order Summary</h2>

                    <div className="summary-row">
                        <span>Subtotal</span>
                        <span>R. 3 275.00</span>
                    </div>

                    <div className="summary-row">
                        <span>Delivery</span>
                        <span>Free</span>
                    </div>

                    <div className="summary-total">
                        <span>Total</span>
                        <strong>R. 3 275.00</strong>
                    </div>

                    <button className="btn-primary cart-checkout">
                        Proceed to Checkout
                    </button>

                    <Link to="/boards" className="continue-shopping">
                        ← Continue Shopping
                    </Link>
                </div>

            </div>
        </div>
    );
};

export default Cart;