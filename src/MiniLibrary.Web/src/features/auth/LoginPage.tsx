import { Box, Button, Container, Paper, Typography, Stack, Divider, Select, MenuItem, FormControl, InputLabel } from '@mui/material';
import GoogleIcon from '@mui/icons-material/Google';
import MicrosoftIcon from '@mui/icons-material/Window';
import DeveloperModeIcon from '@mui/icons-material/DeveloperMode';
import { useState } from 'react';
import { useAuth } from './AuthContext';
import { apiClient } from '@/services/apiClient';

export default function LoginPage() {
  const { login, refreshUser } = useAuth();
  const [devRole, setDevRole] = useState<string>('Member');
  const [devLoading, setDevLoading] = useState(false);

  const handleDevLogin = async () => {
    setDevLoading(true);
    try {
      // POST /auth/dev-token sets HttpOnly cookies; response contains user info
      await apiClient.post('/auth/dev-token', { role: devRole });
      // Refresh auth state from the newly-set cookies
      await refreshUser();
      window.location.href = '/';
    } catch {
      alert('Dev tokens are disabled on this server or API is not running.');
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
              onClick={() => login('google')}
              fullWidth
              sx={{
                bgcolor: (theme) =>
                  theme.palette.mode === 'dark' ? '#4285F4' : 'primary.main',
                color: '#FFFFFF',
                '&:hover': {
                  bgcolor: (theme) =>
                    theme.palette.mode === 'dark' ? '#3367D6' : 'primary.dark',
                },
              }}
            >
              Sign in with Google
            </Button>
            <Button
              variant="outlined"
              size="large"
              startIcon={<MicrosoftIcon />}
              onClick={() => login('microsoft')}
              fullWidth
              sx={{
                borderColor: (theme) =>
                  theme.palette.mode === 'dark' ? '#90CAF9' : 'primary.main',
                color: (theme) =>
                  theme.palette.mode === 'dark' ? '#90CAF9' : 'primary.main',
                '&:hover': {
                  borderColor: (theme) =>
                    theme.palette.mode === 'dark' ? '#BBDEFB' : 'primary.dark',
                  bgcolor: (theme) =>
                    theme.palette.mode === 'dark' ? 'rgba(144, 202, 249, 0.08)' : 'rgba(30, 58, 95, 0.04)',
                },
              }}
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
