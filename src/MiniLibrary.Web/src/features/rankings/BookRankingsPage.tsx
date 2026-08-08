import { useState } from 'react';
import {
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
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';
import { useBookRankings } from './hooks/useRankings';
import { EmptyState } from '@/components/EmptyState';
import { StarDisplay } from '@/features/ratings/components/StarDisplay';

const SORT_OPTIONS = [
  { value: 'averageRating', label: 'Average Rating' },
  { value: 'totalRatings', label: 'Number of Ratings' },
  { value: 'totalLoans', label: 'Total Loans' },
];

export default function BookRankingsPage() {
  const [sortBy, setSortBy] = useState('averageRating');
  const [category, setCategory] = useState('');
  const { data: rankings, isLoading } = useBookRankings({
    sortBy,
    category: category || undefined,
  });

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <EmojiEventsIcon color="secondary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          Book Rankings
        </Typography>
      </Stack>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        <TextField
          select
          label="Sort by"
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          {SORT_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
          ))}
        </TextField>
        <TextField
          label="Category"
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          placeholder="Filter by category"
          sx={{ minWidth: 160 }}
        />
      </Stack>

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 10 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={52} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !rankings || rankings.length === 0 ? (
        <EmptyState title="No rankings yet" message="Books need at least 3 ratings to appear in rankings." />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell width={60}>#</TableCell>
                <TableCell>Book</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Rating</TableCell>
                <TableCell align="right">Loans</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rankings.map((book) => (
                <TableRow key={book.position} hover>
                  <TableCell>
                    <Typography fontWeight={700} color={book.position <= 3 ? 'secondary.main' : 'text.primary'}>
                      {book.position}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>{book.title}</Typography>
                    <Typography variant="caption" color="text.secondary">{book.author}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={book.category} size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" alignItems="center" spacing={0.5}>
                      <StarDisplay score={book.averageRating} size="small" />
                      <Typography variant="caption" color="text.secondary">
                        ({book.totalRatings})
                      </Typography>
                    </Stack>
                  </TableCell>
                  <TableCell align="right">{book.totalLoans}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
