import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError, authApi, clearSession } from '../api/client';

function Profile() {
  const navigate = useNavigate();

  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadProfile = useCallback(async () => {
    const token = localStorage.getItem('accessToken');

    // No authentication token
    if (!token) {
      clearSession();

      navigate('/login', {
        replace: true,
        state: {
          message: 'Please sign in to view your profile.',
        },
      });

      return;
    }

    try {
      setLoading(true);
      setError('');

      const data = await authApi.getProfile();

      setProfile(data);

    } catch (err) {
      setProfile(null);

      if (err instanceof ApiError && err.status === 401) {
        navigate('/login', {
          replace: true,
          state: {
            message: 'Your session has expired or is invalid. Please sign in again.',
          },
        });
        return;
      }

      setError(
        err instanceof Error
          ? err.message
          : 'An unexpected error occurred while loading your profile.'
      );
    } finally {
      setLoading(false);
    }
  }, [navigate]);


  useEffect(() => {
    loadProfile();
  }, [loadProfile]);


  const handleLogout = () => {
    clearSession();

    setProfile(null);

    navigate('/login', {
      replace: true,
    });
  };

  if (loading) {
    return (
      <section className="profile-page">

        <div className="profile-state-card">

          <div className="profile-spinner"></div>

          <h2>
            Loading Your Profile
          </h2>

          <p>
            Please wait while we securely retrieve your account information.
          </p>

        </div>

      </section>
    );
  }

  if (error) {
    return (
      <section className="profile-page">

        <div className="profile-state-card profile-error-card">

          <span className="profile-state-icon">
            !
          </span>

          <h2>
            Unable to Load Profile
          </h2>

          <p>
            {error}
          </p>

          <div className="profile-actions">

            <button
              className="btn-primary"
              onClick={loadProfile}
            >
              Try Again
            </button>

            <Link
              to="/"
              className="btn-secondary"
            >
              Return Home
            </Link>

          </div>

        </div>

      </section>
    );
  }


  if (!profile) {
    return null;
  }

  return (
    <section className="profile-page">

      <div className="profile-header">

        <div>

          <div className="eyebrow">

            <div className="eyebrow-line"></div>

            <span>
              Secure Account Area
            </span>

          </div>


          <h1 className="profile-title">
            My Account
          </h1>


          <p className="profile-subtitle">
            View your Grandmaster&apos;s Hub account information.
          </p>

        </div>


        <div className="profile-badge">
          {profile.role}
        </div>

      </div>


      <div className="profile-grid">

        {/* ACCOUNT INFORMATION */}
        <article className="profile-card profile-main-card">

          <div className="profile-card-header">

            <span className="profile-card-label">
              ACCOUNT INFORMATION
            </span>

          </div>


          <div className="profile-details">

            <div className="profile-detail">

              <span className="profile-detail-label">
                USER ID
              </span>

              <strong>
                #{profile.userId}
              </strong>

            </div>


            <div className="profile-detail">

              <span className="profile-detail-label">
                EMAIL ADDRESS
              </span>

              <strong>
                {profile.email}
              </strong>

            </div>


            <div className="profile-detail">

              <span className="profile-detail-label">
                ACCOUNT ROLE
              </span>

              <strong>
                {profile.role}
              </strong>

            </div>

          </div>

        </article>


        {/* ACCOUNT STATUS */}
        <article className="profile-card">

          <div className="profile-card-header">

            <span className="profile-card-label">
              ACCOUNT STATUS
            </span>

          </div>


          <div className="profile-status-list">

            <div className="profile-status-item">

              <span className="status-check">
                ✓
              </span>

              <span>
                Authenticated account
              </span>

            </div>


            <div className="profile-status-item">

              <span className="status-check">
                ✓
              </span>

              <span>
                Secure JWT authentication
              </span>

            </div>


            <div className="profile-status-item">

              <span className="status-check">
                ✓
              </span>

              <span>
                Profile data retrieved securely
              </span>

            </div>

          </div>

        </article>

      </div>


      {/* ACTIONS */}
      <div className="profile-actions profile-bottom-actions">

        <button
          className="btn-secondary"
          onClick={loadProfile}
        >
          Refresh Profile
        </button>


        <button
          className="profile-logout-button"
          onClick={handleLogout}
        >
          Log Out
        </button>

      </div>

    </section>
  );
}

export default Profile;
