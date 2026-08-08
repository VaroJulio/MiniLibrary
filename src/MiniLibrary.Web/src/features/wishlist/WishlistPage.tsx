import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  Chip,
  IconButton,
  Pagination,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import FavoriteIcon from '@mui/icons-material/Favorite';
import { useWishlist, useRemoveFromWishlist } from './hooks/useWishlist';
import { EmptyState } from '@/components/EmptyState';

const PAGE_SIZE = 20;

export default function WishlistPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const { data, isLoading } = useWishlist(page, PAGE_SIZE);
  const removeMutation = useRemoveFromWishlist();

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <FavoriteIcon color="error" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          My Wishlist
        </Typography>
      </Stack>

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={72} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !data || data.data.length === 0 ? (
        <EmptyState
          title="Your wishlist is empty"
          message="Add books you'd like to read from the catalog. You'll be notified when they become available."
        />
      ) : (
        <>
          <Stack spacing={1}>
            {data.data.map((entry) => (
              <Card key={entry.bookId} sx={{ '&:hover': { boxShadow: 3 } }}>
                <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Box
                      sx={{ cursor: 'pointer', flex: 1 }}
                      onClick={() => navigate(`/books/${entry.bookId}`)}
                    >
                      <Typography variant="subtitle2" fontWeight={600}>
                        {entry.title}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {entry.author}
                      </Typography>
                    </Box>
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Chip
                        label={entry.bookStatus === 'Available' ? 'Available' : 'Checked Out'}
                        color={entry.bookStatus === 'Available' ? 'success' : 'default'}
                        size="small"
                      />
                      <Typography variant="caption" color="text.disabled">
                        {new Date(entry.addedAt).toLocaleDateString()}
                      </Typography>
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => removeMutation.mutate(entry.bookId)}
                        disabled={removeMutation.isPending}
                        aria-label="remove from wishlist"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Stack>
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
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
    </Box>
  );
}
