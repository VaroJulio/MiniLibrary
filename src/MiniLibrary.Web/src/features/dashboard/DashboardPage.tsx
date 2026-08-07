import { Typography, Box } from '@mui/material';

export default function DashboardPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Dashboard
      </Typography>
      <Typography color="text.secondary">
        Dashboard statistics will be displayed here.
      </Typography>
    </Box>
  );
}
