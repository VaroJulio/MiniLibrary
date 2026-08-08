import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Button,
  Divider,
  Pagination,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import RateReviewIcon from '@mui/icons-material/RateReview';
import { useMyRatings, useRecentRatings, useDeleteRating, useVoteUseful } from './hooks/useRatings';
import { ReviewCard } from './components/ReviewCard';
import { EmptyState } from '@/components/EmptyState';

const MY_PAGE_SIZE = 10;
const RECENT_PAGE_SIZE = 20;

export default function RatingsPage() {
  const navigate = useNavigate();
  const [myPage, setMyPage] = useState(1);
  const [recentPage, setRecentPage] = useState(1);
  const [deleteTarget, setDeleteTarget] = useState<{ bookId: string; title: string } | null>(null);

  const { data: myData, isLoading: myLoading, error: myError } = useMyRatings(myPage, MY_PAGE_SIZE);
  const { data: recentData, isLoading: recentLoading } = useRecentRatings(recentPage, RECENT_PAGE_SIZE);
  const voteMutation = useVoteUseful();

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <RateReviewIcon color="primary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          Ratings & Reviews
        </Typography>
      </Stack>

      {/* My Reviews Section */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
        My Reviews
      </Typography>

      {myLoading ? (
        <Stack spacing={1} sx={{ mb: 4 }}>
          {Array.from({ length: 3 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={100} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : myError ? (
        <Alert severity="error" sx={{ mb: 4 }}>
          Failed to load your reviews. Please try again later.
        </Alert>
      ) : !myData || myData.data.length === 0 ? (
        <Box sx={{ mb: 4 }}>
          <EmptyState
            title="You haven't rated any books yet"
            message="Rate books from their detail pages after returning them. Your reviews help other readers discover great books."
          />
        </Box>
      ) : (
        <Box sx={{ mb: 4 }}>
          <Stack spacing={1}>
            {myData.data.map((rating) => (
              <MyRatingCard
                key={rating.id}
                rating={rating}
                onBookClick={() => navigate(`/books/${rating.bookId}`)}
                onDelete={() => setDeleteTarget({ bookId: rating.bookId, title: rating.bookTitle })}
              />
            ))}
          </Stack>
          {myData.pagination.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
              <Pagination
                count={myData.pagination.totalPages}
                page={myPage}
                onChange={(_, p) => setMyPage(p)}
                color="primary"
                shape="rounded"
              />
            </Box>
          )}
        </Box>
      )}

      <Divider sx={{ my: 3 }} />

      {/* Recent Community Reviews Section */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
        Recent Community Reviews
      </Typography>

      {recentLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={100} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !recentData || recentData.data.length === 0 ? (
        <EmptyState
          title="No community reviews yet"
          message="Be the first to review a book and help other readers."
        />
      ) : (
        <>
          <Stack spacing={1}>
            {recentData.data.map((rating) => (
              <ReviewCard
                key={rating.id}
                bookTitle={rating.bookTitle}
                bookAuthor={rating.bookAuthor}
                userName={rating.userName}
                score={rating.score}
                reviewText={rating.reviewText}
                usefulVotes={rating.usefulVotes}
                createdAt={rating.createdAt}
                onBookClick={() => navigate(`/books/${rating.bookId}`)}
                onVoteUseful={() => voteMutation.mutate(rating.id)}
                isVotePending={voteMutation.isPending}
                showUserName
              />
            ))}
          </Stack>
          {recentData.pagination.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination
                count={recentData.pagination.totalPages}
                page={recentPage}
                onChange={(_, p) => setRecentPage(p)}
                color="primary"
                shape="rounded"
              />
            </Box>
          )}
        </>
      )}

      {/* Delete Confirmation Dialog */}
      {deleteTarget && (
        <DeleteConfirmDialog
          bookTitle={deleteTarget.title}
          bookId={deleteTarget.bookId}
          onClose={() => setDeleteTarget(null)}
        />
      )}
    </Box>
  );
}

function MyRatingCard({
  rating,
  onBookClick,
  onDelete,
}: {
  rating: { id: string; bookId: string; bookTitle: string; bookAuthor: string; score: number; reviewText: string; usefulVotes: number; createdAt: string };
  onBookClick: () => void;
  onDelete: () => void;
}) {
  return (
    <ReviewCard
      bookTitle={rating.bookTitle}
      bookAuthor={rating.bookAuthor}
      score={rating.score}
      reviewText={rating.reviewText}
      usefulVotes={rating.usefulVotes}
      createdAt={rating.createdAt}
      onBookClick={onBookClick}
      onDelete={onDelete}
    />
  );
}

function DeleteConfirmDialog({
  bookTitle,
  bookId,
  onClose,
}: {
  bookTitle: string;
  bookId: string;
  onClose: () => void;
}) {
  const deleteMutation = useDeleteRating(bookId);

  const handleDelete = () => {
    deleteMutation.mutate(undefined, {
      onSuccess: () => onClose(),
    });
  };

  return (
    <Dialog open onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Delete Review</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Are you sure you want to delete your review for <strong>{bookTitle}</strong>? This action cannot be undone.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={deleteMutation.isPending}>
          Cancel
        </Button>
        <Button
          onClick={handleDelete}
          color="error"
          variant="contained"
          disabled={deleteMutation.isPending}
        >
          {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
