import { Typography, Box } from '@mui/material';

export default function RecommendationsPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Recommendations
      </Typography>
      <Typography color="text.secondary">
        Personalized book recommendations will appear here.
      </Typography>
    </Box>
  );
}
