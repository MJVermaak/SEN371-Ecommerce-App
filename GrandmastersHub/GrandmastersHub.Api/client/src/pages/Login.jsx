import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi, saveSession } from '../api/client';

function Login() {
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();

    setError('');

    if (!email.trim() || !password.trim()) {
      setError('Please enter both your email address and password.');
      return;
    }

    try {
      setLoading(true);

      const data = await authApi.login({
        email: email.trim(),
        password,
      });

      saveSession(data);

      navigate('/profile');

    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected error occurred. Please try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-page">

      <div className="auth-card">

        <div className="eyebrow">
          <div className="eyebrow-line"></div>

          <span>Member Access</span>
        </div>

        <h1 className="auth-title">
          Welcome Back
        </h1>

        <p className="auth-subtitle">
          Sign in to access your Grandmaster&apos;s Hub account.
        </p>


        {error && (
          <div className="auth-error">
            <span>!</span>

            <p>{error}</p>
          </div>
        )}


        <form
          className="auth-form"
          onSubmit={handleSubmit}
        >

          <div className="form-group">

            <label htmlFor="email">
              EMAIL ADDRESS
            </label>

            <input
              id="email"
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              disabled={loading}
              autoComplete="email"
            />

          </div>


          <div className="form-group">

            <label htmlFor="password">
              PASSWORD
            </label>

            <input
              id="password"
              type="password"
              placeholder="Enter your password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              disabled={loading}
              autoComplete="current-password"
            />

          </div>


          <button
            type="submit"
            className="btn-primary auth-submit"
            disabled={loading}
          >
            {loading
              ? 'Signing In...'
              : 'Sign In'}
          </button>

        </form>


        <div className="auth-footer">

          <span>
            Don't have an account?
          </span>

          <Link to="/register">
            Create one
          </Link>

        </div>

      </div>

    </section>
  );
}

export default Login;
