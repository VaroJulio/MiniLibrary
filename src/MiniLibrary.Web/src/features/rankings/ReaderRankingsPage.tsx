import { useState } from 'react';
import {
  Alert,
  Box,
  Chip,
  MenuItem,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import LeaderboardIcon from '@mui/icons-material/Leaderboard';
import { useReaderRankings } from './hooks/useRankings';
import { EmptyState } from '@/components/EmptyState';

const PERIOD_OPTIONS = [
  { value: '30', label: 'Last 30 days' },
  { value: '90', label: 'Last 90 days' },
  { value: '365', label: 'Last 12 months' },
  { value: '', label: 'All time' },
];

export default function ReaderRankingsPage() {
  const [period, setPeriod] = useState('90');
  const { data, isLoading } = useReaderRankings(period || undefined);

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <LeaderboardIcon color="secondary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          Reader Rankings
        </Typography>
      </Stack>

      <TextField
        select
        label="Period"
        value={period}
        onChange={(e) => setPeriod(e.target.value)}
        sx={{ minWidth: 180, mb: 3 }}
      >
        {PERIOD_OPTIONS.map((opt) => (
          <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
        ))}
      </TextField>

      {data?.myPosition && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Your position: <strong>#{data.myPosition}</strong>
        </Alert>
      )}

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 10 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={52} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !data || data.rankings.length === 0 ? (
        <EmptyState title="No reader rankings yet" message="Start reading to appear in the rankings!" />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell width={60}>#</TableCell>
                <TableCell>Reader</TableCell>
                <TableCell>Books Read</TableCell>
                <TableCell>Favorite Category</TableCell>
                <TableCell align="right">Avg Rating Given</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.rankings.map((reader) => (
                <TableRow key={reader.position} hover>
                  <TableCell>
                    <Typography fontWeight={700} color={reader.position <= 3 ? 'secondary.main' : 'text.primary'}>
                      {reader.position}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>{reader.name}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={`${reader.booksRead} books`} size="small" color="primary" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">{reader.favoriteCategory}</Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="body2">★ {reader.averageRatingGiven.toFixed(1)}</Typography>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
