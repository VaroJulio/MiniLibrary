import { Typography, Box } from '@mui/material';

export default function RatingsPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" fontWeight={700} gutterBottom>
        Ratings &amp; Reviews
      </Typography>
      <Typography color="text.secondary">
        Rate and review books from their detail pages. Your reviews help other readers discover great books.
      </Typography>
    </Box>
  );
}
