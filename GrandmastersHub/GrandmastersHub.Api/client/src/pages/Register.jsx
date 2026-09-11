import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '../api/client';

function Register() {
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();

    setError('');
    setMessage('');

    if (!email || !password) {
      setError('Please enter both an email address and password.');
      return;
    }

    try {
      setLoading(true);

      await authApi.register({
        email: email.trim(),
        password,
      });

      setMessage(
        'Account created successfully. Redirecting you to sign in...'
      );

      setEmail('');
      setPassword('');

      setTimeout(() => {
        navigate('/login');
      }, 1500);

    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to create your account.'
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

          <span>
            JOIN THE HUB
          </span>
        </div>

        <h1 className="auth-title">
          Create Account
        </h1>

        <p className="auth-description">
          Create your Grandmaster's Hub account and begin exploring
          our exclusive collection.
        </p>

        <form
          className="auth-form"
          onSubmit={handleSubmit}
        >

          <div className="form-group">

            <label>
              EMAIL ADDRESS
            </label>

            <input
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              disabled={loading}
            />

          </div>


          <div className="form-group">

            <label>
              PASSWORD
            </label>

            <input
              type="password"
              placeholder="Create a secure password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              disabled={loading}
            />

          </div>


          {error && (
            <div className="auth-error">
              {error}
            </div>
          )}


          {message && (
            <div className="auth-success">
              {message}
            </div>
          )}


          <button
            type="submit"
            className="auth-submit"
            disabled={loading}
          >
            {loading
              ? 'CREATING ACCOUNT...'
              : 'CREATE ACCOUNT'
            }
          </button>

        </form>


        <p className="auth-switch">

          Already have an account?

          {' '}

          <Link to="/login">
            Sign In
          </Link>

        </p>

      </div>

    </section>
  );
}

export default Register;
