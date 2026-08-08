import { Box, Button, Container, Paper, Typography, Stack } from '@mui/material';
import GoogleIcon from '@mui/icons-material/Google';
import MicrosoftIcon from '@mui/icons-material/Window';

export default function LoginPage() {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

  // Determine the auth base URL:
  // - If VITE_API_BASE_URL is a full URL (Azure), extract the origin
  // - If relative (/api), use the current API server directly
  const getAuthUrl = () => {
    if (apiBaseUrl.startsWith('http')) {
      // Azure: VITE_API_BASE_URL = "https://minilibrary-api.xxx.azurecontainerapps.io/api"
      // Extract origin: "https://minilibrary-api.xxx.azurecontainerapps.io"
      try {
        const url = new URL(apiBaseUrl);
        return url.origin;
      } catch {
        return 'http://localhost:5000';
      }
    }
    // Local: VITE_API_BASE_URL = "/api" → use localhost:5000 directly
    return 'http://localhost:5000';
  };

  const authUrl = getAuthUrl();

  const handleGoogleLogin = () => {
    window.location.href = `${authUrl}/api/auth/login/google`;
  };

  const handleMicrosoftLogin = () => {
    window.location.href = `${authUrl}/api/auth/login/microsoft`;
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
        </Paper>
      </Box>
    </Container>
  );
}
