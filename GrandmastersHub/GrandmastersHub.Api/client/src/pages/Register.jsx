import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authApi } from '../api/client';
import { safeReturnTo } from '../lib/shopping';

function Register() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const loginPath = `/login?returnTo=${encodeURIComponent(safeReturnTo(searchParams.get('returnTo')))}`;
  const redirectTimer = useRef(null);
  useEffect(() => () => clearTimeout(redirectTimer.current), []);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (loading || message) return;
    setError('');
    if (!email.trim() || !password) {
      setError('Please enter both an email address and password.');
      return;
    }
    try {
      setLoading(true);
      await authApi.register({ email: email.trim(), password });
      setMessage('Account created successfully. Redirecting you to sign in...');
      setEmail('');
      setPassword('');
      redirectTimer.current = setTimeout(() => navigate(loginPath, { replace: true }), 1500);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to create your account.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-page">
      <div className="auth-card">
        <div className="eyebrow"><div className="eyebrow-line"></div><span>JOIN THE HUB</span></div>
        <h1 className="auth-title">Create Account</h1>
        <p className="auth-description">
          Create your Grandmaster's Hub account and begin exploring our exclusive collection.
        </p>
        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="register-email">EMAIL ADDRESS</label>
            <input id="register-email" type="email" placeholder="you@example.com" value={email}
              onChange={(event) => setEmail(event.target.value)} disabled={loading || Boolean(message)} autoComplete="email" required />
          </div>
          <div className="form-group">
            <label htmlFor="register-password">PASSWORD</label>
            <input id="register-password" type="password" placeholder="Create a secure password" value={password}
              onChange={(event) => setPassword(event.target.value)} disabled={loading || Boolean(message)} autoComplete="new-password" required />
          </div>
          {error && <div className="auth-error" role="alert">{error}</div>}
          {message && <div className="auth-success" role="status">{message}</div>}
          <button type="submit" className="auth-submit" disabled={loading || Boolean(message)}>
            {loading ? 'CREATING ACCOUNT...' : message ? 'ACCOUNT CREATED' : 'CREATE ACCOUNT'}
          </button>
        </form>
        <p className="auth-switch">Already have an account?{' '}<Link to={loginPath}>Sign In</Link></p>
      </div>
    </section>
  );
}

export default Register;
