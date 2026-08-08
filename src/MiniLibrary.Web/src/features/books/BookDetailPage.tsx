import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  IconButton,
  Link,
  Paper,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import StarIcon from '@mui/icons-material/Star';
import { useBookDetail, useDeleteBook } from './hooks/useBooks';
import { useAuth } from '@/features/auth/AuthContext';
import { BookFormDialog } from './components/BookFormDialog';
import { CheckOutButton } from '@/features/loans/components/CheckOutButton';
import { WishlistButton } from './components/WishlistButton';
import { RatingForm } from '@/features/ratings/components/RatingForm';
import { useHasReadBook } from '@/features/loans/hooks/useLoans';

export default function BookDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { data: book, isLoading, error } = useBookDetail(id);
  const deleteMutation = useDeleteBook();
  const [showEditForm, setShowEditForm] = useState(false);
  const hasReadBook = useHasReadBook(id);

  const canManageBooks = user?.role === 'Admin' || user?.role === 'Librarian';

  const handleDelete = () => {
    if (!id) return;
    if (!window.confirm('Are you sure you want to delete this book?')) return;
    deleteMutation.mutate(id, {
      onSuccess: () => navigate('/books'),
    });
  };

  if (isLoading) {
    return (
      <Box>
        <Skeleton variant="text" width={200} height={40} />
        <Skeleton variant="rounded" height={300} sx={{ mt: 2, borderRadius: 3 }} />
      </Box>
    );
  }

  if (error || !book) {
    return (
      <Box>
        <Alert severity="error">Book not found or an error occurred.</Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/books')} sx={{ mt: 2 }}>
          Back to catalog
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      {/* Breadcrumbs */}
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link
          component="button"
          underline="hover"
          color="inherit"
          onClick={() => navigate('/books')}
        >
          Catalog
        </Link>
        <Typography color="text.primary">{book.title}</Typography>
      </Breadcrumbs>

      {/* Header */}
      <Stack direction="row" justifyContent="space-between" alignItems="flex-start" sx={{ mb: 3 }}>
        <Box>
          <Typography variant="h4" fontWeight={700}>
            {book.title}
          </Typography>
          <Typography variant="h6" color="text.secondary">
            by {book.author}
          </Typography>
        </Box>
        {canManageBooks && (
          <Stack direction="row" spacing={1}>
            <IconButton onClick={() => setShowEditForm(true)} aria-label="edit book">
              <EditIcon />
            </IconButton>
            <IconButton
              onClick={handleDelete}
              color="error"
              disabled={deleteMutation.isPending}
              aria-label="delete book"
            >
              <DeleteIcon />
            </IconButton>
          </Stack>
        )}
      </Stack>

      {deleteMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(deleteMutation.error as Error)?.message ?? 'Cannot delete this book. It may have active loans.'}
        </Alert>
      )}

      <Grid container spacing={3}>
        {/* Main Info */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3 }}>
            <Stack spacing={2}>
              <Stack direction="row" spacing={1} flexWrap="wrap">
                <Chip
                  label={book.status === 'Available' ? 'Available' : 'Checked Out'}
                  color={book.status === 'Available' ? 'success' : 'warning'}
                />
                <Chip label={book.category} variant="outlined" />
                <Chip label={`${book.publicationYear}`} variant="outlined" size="small" />
              </Stack>

              <Typography variant="body2" color="text.secondary">
                ISBN: {book.isbn}
              </Typography>

              {book.description && (
                <>
                  <Divider />
                  <Typography variant="body1">{book.description}</Typography>
                </>
              )}

              {/* Rating Summary */}
              {book.totalRatings > 0 && (
                <>
                  <Divider />
                  <Stack direction="row" alignItems="center" spacing={1}>
                    <StarIcon sx={{ color: 'secondary.main' }} />
                    <Typography variant="h6" fontWeight={600}>
                      {book.averageRating.toFixed(1)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      ({book.totalRatings} {book.totalRatings === 1 ? 'rating' : 'ratings'})
                    </Typography>
                  </Stack>
                </>
              )}
            </Stack>
          </Paper>

          {/* Recent Reviews */}
          {book.recentRatings && book.recentRatings.length > 0 && (
            <Paper sx={{ p: 3, mt: 2 }}>
              <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                Recent Reviews
              </Typography>
              <Stack spacing={2} divider={<Divider />}>
                {book.recentRatings.map((rating) => (
                  <Box key={rating.id}>
                    <Stack direction="row" justifyContent="space-between" alignItems="center">
                      <Typography variant="subtitle2">{rating.userName}</Typography>
                      <Stack direction="row" alignItems="center" spacing={0.5}>
                        <StarIcon sx={{ fontSize: 16, color: 'secondary.main' }} />
                        <Typography variant="body2">{rating.score}</Typography>
                      </Stack>
                    </Stack>
                    {rating.reviewText && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        {rating.reviewText}
                      </Typography>
                    )}
                    <Typography variant="caption" color="text.disabled">
                      {new Date(rating.createdAt).toLocaleDateString()}
                    </Typography>
                  </Box>
                ))}
              </Stack>
            </Paper>
          )}

          {/* Rate this Book */}
          {user && hasReadBook && (
            <Paper sx={{ p: 3, mt: 2 }}>
              <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                Rate this Book
              </Typography>
              <RatingForm bookId={book.id} />
            </Paper>
          )}
        </Grid>

        {/* Sidebar Actions */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Stack spacing={2}>
                <CheckOutButton book={book} />
                <WishlistButton bookId={book.id} bookStatus={book.status} />
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Edit Form Dialog */}
      {showEditForm && (
        <BookFormDialog open={showEditForm} onClose={() => setShowEditForm(false)} book={book} />
      )}
    </Box>
  );
}
