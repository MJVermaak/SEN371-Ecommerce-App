import Catalog from './pages/Catalog';
import Login from './pages/Login';
import Profile from './pages/Profile';
import Register from './pages/Register';

import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import './index.css';
import { useState, useEffect } from 'react';
import LoadingScreen from './pages/LoadingScreen';
import Cart from './pages/Cart';

function AnimatedRoutes() {
    const location = useLocation();

    return (
        <main className="main-content fade-page" key={location.pathname}>
            <Routes location={location} key={location.pathname}>
                <Route
                    path="/"
                    element={
                        <section className="hero-section fade-page">
                            <div className="hero-content">
                                <div>
                                    <div className="eyebrow">
                                        <div className="eyebrow-line"></div>
                                        <span>The Ultimate Standard of Play</span>
                                    </div>

                                    <h1 className="hero-title">Master Your Strategy</h1>

                                    <p className="hero-desc">
                                        Hand-carved premium equipment crafted from rare hardwoods,
                                        volcanic obsidian, and fine Italian marble.
                                    </p>

                                    <div className="hero-actions">
                                        <Link to="/boards" className="btn-primary">
                                            Shop Now
                                        </Link>

                                        <Link to="/bespoke" className="btn-secondary">
                                            The Heritage
                                        </Link>
                                    </div>
                                </div>
                            </div>

                            <div className="hero-image-placeholder">
                                <img src="/images/Main-Page-Lander.png" alt="Welcome" />
                            </div>
                        </section>
                    }
                />

                <Route
                    path="/boards"
                    element={
                        <div className="fade-page">
                            <Catalog title="The Master's Collection" categoryName="boards" />
                        </div>
                    }
                />
                <Route
                    path="/clocks"
                    element={
                        <div className="fade-page">
                            <Catalog title="Precision Clocks" categoryName="clocks" />
                        </div>
                    }
                />
                <Route
                    path="/books"
                    element={
                        <div className="fade-page">
                            <Catalog title="Chess Literature" categoryName="books" />
                        </div>
                    }
                />

                <Route
                    path="/bespoke"
                    element={
                        <div className="fade-page">
                            <Catalog title="Bespoke Custom Sets" categoryName="bespoke" />
                        </div>
                    }
                />
                <Route
                    path="/cart"
                    element={
                        <div className="fade-page">
                            <Cart />
                        </div>
                    }
                />

            </Routes>
        </main>
    );
}
function App() {

  

    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const timer = setTimeout(() => setLoading(false), 1800);
        return () => clearTimeout(timer);
    }, []);

    if (loading) {
        return <LoadingScreen />;
    }
  return (
    <Router>
      <div className="app-container">
        
        {/* Global Header matching Figma properties */}
        <header className="main-header">
          <Link to="/" className="logo-group">
            <div className="logo-icon"></div>
            <span className="logo-text">The Grandmaster's Hub</span>
          </Link>
          
          <nav className="nav-links">
            <Link to="/boards">Boards</Link>
            <Link to="/clocks">Clocks</Link>
            <Link to="/books">Books</Link>
            <Link to="/bespoke">Bespoke Sets</Link>
            <div className="nav-divider"></div>
            <Link to="/cart" className="cart-button">
              <div className="cart-icon"></div>
              <span>Cart (0)</span>
            </Link>
            <Link to="/profile">Account</Link>
                      <Link to="/cart" className="cart-button">
                          <svg
                              className="cart-icon"
                              viewBox="0 0 24 24"
                              fill="none"
                              xmlns="http://www.w3.org/2000/svg"
                          >
                              <path
                                  d="M3 4H5L7 14H17L20 7H8"
                                  stroke="currentColor"
                                  strokeWidth="1.8"
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                              />
                              <circle cx="9" cy="19" r="1.5" fill="currentColor" />
                              <circle cx="17" cy="19" r="1.5" fill="currentColor" />
                          </svg>
                          <span>Cart (0)</span>
                      </Link>
          </nav>
        </header>

        {/* Dynamic Route Content */}
        <main className="main-content">
          <Routes>
  <Route path="/" element={
    <section className="hero-section">
      <div className="hero-content">
        <div>
          <div className="eyebrow">
            <div className="eyebrow-line"></div>
            <span>The Ultimate Standard of Play</span>
          </div>
          <h1 className="hero-title">Master Your Strategy</h1>
          <p className="hero-desc">
            Hand-carved premium equipment crafted from rare hardwoods, volcanic obsidian, and fine Italian marble. For grandmasters, collectors, and those who settle for nothing less than absolute perfection.
          </p>
          <div className="hero-actions">
            <Link to="/boards" className="btn-primary">Shop Now</Link>
            <Link to="/bespoke" className="btn-secondary">The Heritage</Link>
          </div>
        </div>
      </div>
      <div className="hero-image-placeholder">
        <img src="/images/Main-Page-Lander.png" alt="Welcome" />
      </div>
    </section>
  } />
  
            {/* Registering all category routes to use the Catalog grid layout with filtered data */}
            <Route path="/boards" element={<Catalog title="The Master's Collection" categoryName="boards" />} />
            <Route path="/clocks" element={<Catalog title="Precision Clocks" categoryName="clocks" />} />
            <Route path="/books" element={<Catalog title="Chess Literature" categoryName="books" />} />
            <Route path="/bespoke" element={<Catalog title="Bespoke Custom Sets" categoryName="bespoke" />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/profile" element={<Profile />} />

  <Route path="/cart" element={<h2 style={{padding: '80px'}}>Shopping Cart (Coming Soon)</h2>} />
</Routes>
        </main>
       
              <AnimatedRoutes />

        {/* Global Footer */}
        <footer className="main-footer">
          <div className="footer-top">
            <div className="footer-newsletter">
              <Link to="/" className="logo-group">
                <div className="logo-icon"></div>
                <span className="logo-text">The Grandmaster's Hub</span>
              </Link>
              <p>Subscribe to receive exclusive access to bespoke collection drops, design history, and masterclass tactical guides.</p>
            </div>
            {/* The rest of the footer column links can be scaffolded here */}
          </div>
          <div className="footer-bottom">
            <span>© 2026 The Grandmaster's Hub. All Rights Reserved.</span>
            <div className="footer-links">
              <span>Privacy Policy</span>
              <span>Terms of Service</span>
              <span>White Glove Courier</span>
            </div>
          </div>
        </footer>

      </div>
    </Router>
  );
}

export default App;
