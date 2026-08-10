import { useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Box, CircularProgress, Typography, Alert } from '@mui/material';
import { useAuth } from './AuthContext';

/**
 * OAuth callback page.
 * After successful OAuth login, the backend sets HttpOnly auth cookies and
 * redirects here. This component simply triggers a user refresh (GET /auth/me)
 * to update the auth state, then navigates to the home page.
 *
 * If there's an error query param (from failed OAuth), shows the error briefly.
 */
export default function OAuthCallback() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { refreshUser } = useAuth();
  const processed = useRef(false);

  useEffect(() => {
    if (processed.current) return;
    processed.current = true;

    const error = searchParams.get('error');

    if (error) {
      // Brief delay so user sees the error before redirect
      setTimeout(() => navigate('/login', { replace: true }), 2000);
      return;
    }

    // Cookies are already set by the backend redirect — just refresh user state
    refreshUser().then(() => {
      navigate('/', { replace: true });
    }).catch(() => {
      navigate('/login', { replace: true });
    });
  }, [searchParams, navigate, refreshUser]);

  const error = searchParams.get('error');

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        gap: 2,
        p: 3,
      }}
    >
      {error ? (
        <Alert severity="error" sx={{ maxWidth: 400 }}>
          Authentication failed: {error}. Redirecting to login...
        </Alert>
      ) : (
        <>
          <CircularProgress />
          <Typography variant="body1" color="text.secondary">
            Completing sign in...
          </Typography>
        </>
      )}
    </Box>
  );
}
