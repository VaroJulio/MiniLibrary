import { Box, Button, Container, Paper, Typography, Stack } from '@mui/material';
import GoogleIcon from '@mui/icons-material/Google';
import MicrosoftIcon from '@mui/icons-material/Window';

export default function LoginPage() {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

  // For OAuth, redirect directly to the API (not through nginx proxy)
  // because Google needs to redirect back to the API's exact host:port
  const authUrl = apiBaseUrl.startsWith('http') ? apiBaseUrl : 'http://localhost:5000';

  const handleGoogleLogin = () => {
    window.location.href = `${authUrl}/auth/login/google`;
  };

  const handleMicrosoftLogin = () => {
    window.location.href = `${authUrl}/auth/login/microsoft`;
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
