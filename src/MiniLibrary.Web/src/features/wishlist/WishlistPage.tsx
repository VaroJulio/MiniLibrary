import { Typography, Box } from '@mui/material';

export default function WishlistPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        My Wishlist
      </Typography>
      <Typography color="text.secondary">
        Your wishlist will be displayed here.
      </Typography>
    </Box>
  );
}
