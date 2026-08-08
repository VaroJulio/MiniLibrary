import { Box, Card, CardContent, Stack, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import LibraryBooksIcon from '@mui/icons-material/LibraryBooks';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import SwapHorizIcon from '@mui/icons-material/SwapHoriz';
import PeopleIcon from '@mui/icons-material/People';
import type { DashboardStats } from '../api/dashboardApi';

interface StatsCardsProps {
  stats: DashboardStats;
}

export function StatsCards({ stats }: StatsCardsProps) {
  const cards = [
    { label: 'Total Books', value: stats.totalBooks, icon: <LibraryBooksIcon />, color: '#1E3A5F' },
    { label: 'Available', value: stats.availableBooks, icon: <CheckCircleIcon />, color: '#10B981' },
    { label: 'Checked Out', value: stats.checkedOutBooks, icon: <SwapHorizIcon />, color: '#F59E0B' },
    { label: 'Active Loans', value: stats.activeLoans, icon: <PeopleIcon />, color: '#3B82F6' },
  ];

  return (
    <Grid container spacing={2}>
      {cards.map((card) => (
        <Grid size={{ xs: 6, md: 3 }} key={card.label}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={1.5}>
                <Stack
                  alignItems="center"
                  justifyContent="center"
                  sx={{
                    width: 40,
                    height: 40,
                    borderRadius: 2,
                    bgcolor: `${card.color}14`,
                    color: card.color,
                  }}
                >
                  {card.icon}
                </Stack>
                <Box>
                  <Typography variant="h5" fontWeight={700}>
                    {card.value.toLocaleString()}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {card.label}
                  </Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}
