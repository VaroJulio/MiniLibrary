import { Typography, Box } from '@mui/material';

export default function BookListPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Book Catalog
      </Typography>
      <Typography color="text.secondary">
        Book catalog will be displayed here.
      </Typography>
    </Box>
  );
}
