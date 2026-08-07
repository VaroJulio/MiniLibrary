import { Typography, Box } from '@mui/material';

export default function NotificationsPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Notifications
      </Typography>
      <Typography color="text.secondary">
        Your notifications will be displayed here.
      </Typography>
    </Box>
  );
}
