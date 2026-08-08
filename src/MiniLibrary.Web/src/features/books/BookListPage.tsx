import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  IconButton,
  InputAdornment,
  MenuItem,
  Skeleton,
  Stack,
  TextField,
  Typography,
  Pagination,
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import SearchIcon from '@mui/icons-material/Search';
import AddIcon from '@mui/icons-material/Add';
import { useBooks } from './hooks/useBooks';
import { useAuth } from '@/features/auth/AuthContext';
import { EmptyState } from '@/components/EmptyState';
import { BookFormDialog } from './components/BookFormDialog';
import type { BookFilters } from './types';
import type { Book } from '@/types/models';

const PAGE_SIZE = 12;

const STATUS_OPTIONS = [
  { value: '', label: 'All Status' },
  { value: 'Available', label: 'Available' },
  { value: 'CheckedOut', label: 'Checked Out' },
];

export default function BookListPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<BookFilters>({});
  const [searchInput, setSearchInput] = useState('');
  const [showForm, setShowForm] = useState(false);

  const { data, isLoading } = useBooks(page, PAGE_SIZE, filters);
  const canManageBooks = user?.role === 'Admin' || user?.role === 'Librarian';

  const handleSearch = useCallback(() => {
    setFilters((prev) => ({ ...prev, query: searchInput || undefined }));
    setPage(1);
  }, [searchInput]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === 'Enter') handleSearch();
    },
    [handleSearch],
  );

  const handleFilterChange = (key: keyof BookFilters, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value || undefined }));
    setPage(1);
  };

  const handleBookClick = (book: Book) => {
    navigate(`/books/${book.id}`);
  };

  return (
    <Box>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight={700}>
          Book Catalog
        </Typography>
        {canManageBooks && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setShowForm(true)}>
            Add Book
          </Button>
        )}
      </Stack>

      {/* Search and Filters */}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        <TextField
          placeholder="Search books..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={handleKeyDown}
          sx={{ flexGrow: 1 }}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconButton onClick={handleSearch} edge="end" aria-label="search">
                  <SearchIcon />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />
        <TextField
          select
          label="Status"
          value={filters.status ?? ''}
          onChange={(e) => handleFilterChange('status', e.target.value)}
          sx={{ minWidth: 140 }}
        >
          {STATUS_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          label="Category"
          value={filters.category ?? ''}
          onChange={(e) => handleFilterChange('category', e.target.value)}
          placeholder="e.g. Fiction"
          sx={{ minWidth: 140 }}
        />
      </Stack>

      {/* Book Grid */}
      {isLoading ? (
        <Grid container spacing={2}>
          {Array.from({ length: PAGE_SIZE }, (_, i) => (
            <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={i}>
              <Skeleton variant="rounded" height={200} sx={{ borderRadius: 3 }} />
            </Grid>
          ))}
        </Grid>
      ) : !data || data.data.length === 0 ? (
        <EmptyState
          title="No books found"
          message="Try adjusting your search or filters."
        />
      ) : (
        <>
          <Grid container spacing={2}>
            {data.data.map((book) => (
              <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={book.id}>
                <BookCard book={book} onClick={() => handleBookClick(book)} />
              </Grid>
            ))}
          </Grid>
          {data.pagination.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination
                count={data.pagination.totalPages}
                page={page}
                onChange={(_, p) => setPage(p)}
                color="primary"
                shape="rounded"
              />
            </Box>
          )}
        </>
      )}

      {/* Book Form Dialog */}
      {showForm && (
        <BookFormDialog open={showForm} onClose={() => setShowForm(false)} />
      )}
    </Box>
  );
}

function BookCard({ book, onClick }: { book: Book; onClick: () => void }) {
  return (
    <Card
      sx={{
        cursor: 'pointer',
        height: '100%',
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': {
          boxShadow: 4,
          transform: 'translateY(-2px)',
        },
      }}
      onClick={onClick}
    >
      <CardContent>
        <Typography variant="subtitle1" fontWeight={600} noWrap>
          {book.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" noWrap sx={{ mb: 1 }}>
          {book.author}
        </Typography>
        <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 1 }}>
          <Chip
            label={book.status === 'Available' ? 'Available' : 'Checked Out'}
            color={book.status === 'Available' ? 'success' : 'warning'}
            size="small"
          />
          <Chip label={book.category} size="small" variant="outlined" />
        </Stack>
        <Typography variant="caption" color="text.secondary">
          {book.publicationYear} · ISBN: {book.isbn}
        </Typography>
        {book.totalRatings > 0 && (
          <Typography variant="caption" display="block" color="text.secondary">
            ★ {book.averageRating.toFixed(1)} ({book.totalRatings} ratings)
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}
