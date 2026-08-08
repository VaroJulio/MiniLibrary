import {
  Box,
  Card,
  CardContent,
  LinearProgress,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useDashboardStats, useLoanMetrics } from './hooks/useDashboard';
import { StatsCards } from './components/StatsCards';

export default function DashboardPage() {
  const { data: stats, isLoading: statsLoading } = useDashboardStats();
  const { data: metrics, isLoading: metricsLoading } = useLoanMetrics();

  return (
    <Box>
      <Typography variant="h4" component="h1" fontWeight={700} sx={{ mb: 3 }}>
        Dashboard
      </Typography>

      {/* Stats Cards */}
      {statsLoading ? (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          {Array.from({ length: 4 }, (_, i) => (
            <Grid size={{ xs: 6, md: 3 }} key={i}>
              <Skeleton variant="rounded" height={88} sx={{ borderRadius: 3 }} />
            </Grid>
          ))}
        </Grid>
      ) : stats ? (
        <Box sx={{ mb: 3 }}>
          <StatsCards stats={stats} />
        </Box>
      ) : null}

      {/* Users by Role */}
      {stats && stats.usersByRole.length > 0 && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
            Users by Role
          </Typography>
          <Stack spacing={1.5}>
            {stats.usersByRole.map((item) => (
              <Box key={item.role}>
                <Stack direction="row" justifyContent="space-between" sx={{ mb: 0.5 }}>
                  <Typography variant="body2">{item.role}</Typography>
                  <Typography variant="body2" fontWeight={600}>{item.count}</Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={Math.min((item.count / Math.max(...stats.usersByRole.map((u) => u.count))) * 100, 100)}
                  sx={{ height: 6, borderRadius: 3 }}
                />
              </Box>
            ))}
          </Stack>
        </Paper>
      )}

      {/* Loan Metrics */}
      {metricsLoading ? (
        <Skeleton variant="rounded" height={300} sx={{ borderRadius: 3 }} />
      ) : metrics ? (
        <Grid container spacing={3}>
          {/* Popular Categories */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                Popular Categories
              </Typography>
              <Stack spacing={1.5}>
                {metrics.popularCategories.map((cat) => (
                  <Box key={cat.category}>
                    <Stack direction="row" justifyContent="space-between" sx={{ mb: 0.5 }}>
                      <Typography variant="body2">{cat.category}</Typography>
                      <Typography variant="body2" fontWeight={600}>{cat.count} loans</Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(
                        (cat.count / Math.max(...metrics.popularCategories.map((c) => c.count))) * 100,
                        100,
                      )}
                      color="secondary"
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                ))}
              </Stack>
            </Paper>
          </Grid>

          {/* Top Books */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                Top 10 Most Borrowed
              </Typography>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>#</TableCell>
                      <TableCell>Title</TableCell>
                      <TableCell align="right">Loans</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {metrics.topBooks.map((book, index) => (
                      <TableRow key={book.title} hover>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>
                          <Typography variant="body2" fontWeight={500}>
                            {book.title}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {book.author}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">{book.loanCount}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>
          </Grid>

          {/* Loans by Period */}
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                Loan Activity
              </Typography>
              <Stack spacing={1}>
                {metrics.loansByPeriod.map((item) => (
                  <Card key={item.period} variant="outlined">
                    <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
                      <Stack direction="row" justifyContent="space-between" alignItems="center">
                        <Typography variant="body2">{item.period}</Typography>
                        <Typography variant="h6" fontWeight={700} color="primary">
                          {item.count}
                        </Typography>
                      </Stack>
                    </CardContent>
                  </Card>
                ))}
              </Stack>
            </Paper>
          </Grid>
        </Grid>
      ) : null}
    </Box>
  );
}
