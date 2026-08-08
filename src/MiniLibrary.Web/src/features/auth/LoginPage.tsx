import { Box, Button, Container, Paper, Typography, Stack, Divider, Select, MenuItem, FormControl, InputLabel } from '@mui/material';
import GoogleIcon from '@mui/icons-material/Google';
import MicrosoftIcon from '@mui/icons-material/Window';
import DeveloperModeIcon from '@mui/icons-material/DeveloperMode';
import { useState } from 'react';
import { useAuth } from './AuthContext';

export default function LoginPage() {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';
  const { handleCallback } = useAuth();
  const [devRole, setDevRole] = useState<string>('Member');
  const [devLoading, setDevLoading] = useState(false);

  // Determine the auth base URL:
  // - If VITE_API_BASE_URL is a full URL (Azure), extract the origin
  // - If relative (/api), use the current API server directly
  const getAuthUrl = () => {
    if (apiBaseUrl.startsWith('http')) {
      try {
        const url = new URL(apiBaseUrl);
        return url.origin;
      } catch {
        return 'http://localhost:5000';
      }
    }
    return 'http://localhost:5000';
  };

  const authUrl = getAuthUrl();

  const handleGoogleLogin = () => {
    window.location.href = `${authUrl}/api/auth/login/google`;
  };

  const handleMicrosoftLogin = () => {
    window.location.href = `${authUrl}/api/auth/login/microsoft`;
  };

  const handleDevLogin = async () => {
    setDevLoading(true);
    try {
      const baseUrl = apiBaseUrl || '/api';
      const response = await fetch(`${baseUrl}/auth/dev-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ role: devRole }),
      });
      if (response.ok) {
        const data = await response.json();
        handleCallback(data.accessToken, data.refreshToken);
        window.location.href = '/';
      } else {
        alert('Dev tokens are disabled on this server.');
      }
    } catch (err) {
      alert('Failed to connect to API. Is it running?');
    } finally {
      setDevLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
        }}
      >
        <Paper elevation={3} sx={{ p: 4, width: '100%', textAlign: 'center' }}>
          <Typography variant="h4" component="h1" gutterBottom>
            MiniLibrary
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
            Sign in to access the library system
          </Typography>
          <Stack spacing={2}>
            <Button
              variant="contained"
              size="large"
              startIcon={<GoogleIcon />}
              onClick={handleGoogleLogin}
              fullWidth
            >
              Sign in with Google
            </Button>
            <Button
              variant="outlined"
              size="large"
              startIcon={<MicrosoftIcon />}
              onClick={handleMicrosoftLogin}
              fullWidth
            >
              Sign in with Microsoft
            </Button>
          </Stack>

          <Divider sx={{ my: 3 }}>Development</Divider>

          <Stack spacing={2}>
            <FormControl fullWidth size="small">
              <InputLabel>Role</InputLabel>
              <Select
                value={devRole}
                label="Role"
                onChange={(e) => setDevRole(e.target.value)}
              >
                <MenuItem value="Admin">Admin</MenuItem>
                <MenuItem value="Librarian">Librarian</MenuItem>
                <MenuItem value="Member">Member</MenuItem>
              </Select>
            </FormControl>
            <Button
              variant="contained"
              color="secondary"
              size="large"
              startIcon={<DeveloperModeIcon />}
              onClick={handleDevLogin}
              disabled={devLoading}
              fullWidth
            >
              {devLoading ? 'Signing in...' : `Dev Login as ${devRole}`}
            </Button>
          </Stack>
        </Paper>
      </Box>
    </Container>
  );
}
