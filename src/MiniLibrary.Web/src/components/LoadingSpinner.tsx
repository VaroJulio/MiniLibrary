import { Box, CircularProgress } from '@mui/material';

interface LoadingSpinnerProps {
  fullPage?: boolean;
}

export function LoadingSpinner({ fullPage = false }: LoadingSpinnerProps) {
  if (fullPage) {
    return (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
      }}
    >
      <CircularProgress />
    </Box>
  );
}
