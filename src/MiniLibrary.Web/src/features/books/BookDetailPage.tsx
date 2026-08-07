import { Typography, Box } from '@mui/material';

export default function BookDetailPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Book Details
      </Typography>
      <Typography color="text.secondary">
        Book details will be displayed here.
      </Typography>
    </Box>
  );
}
