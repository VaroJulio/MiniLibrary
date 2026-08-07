import { Typography, Box } from '@mui/material';

export default function LoanHistoryPage() {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Loan History
      </Typography>
      <Typography color="text.secondary">
        Your loan history will be displayed here.
      </Typography>
    </Box>
  );
}
