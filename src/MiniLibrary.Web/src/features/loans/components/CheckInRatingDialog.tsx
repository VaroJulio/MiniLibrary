import { useState } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from '@mui/material';
import RateReviewIcon from '@mui/icons-material/RateReview';
import { RatingForm } from '@/features/ratings/components/RatingForm';

interface CheckInRatingDialogProps {
  open: boolean;
  bookId: string;
  bookTitle: string;
  onClose: () => void;
}

/**
 * Dialog shown after successful book return prompting the user to rate the book.
 * Options: "Rate Now" (shows inline RatingForm) or "Maybe Later" (dismisses).
 */
export function CheckInRatingDialog({ open, bookId, bookTitle, onClose }: CheckInRatingDialogProps) {
  const [showForm, setShowForm] = useState(false);

  const handleRateNow = () => {
    setShowForm(true);
  };

  const handleRatingSuccess = () => {
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Stack direction="row" alignItems="center" spacing={1}>
          <RateReviewIcon color="primary" />
          <span>Rate this Book</span>
        </Stack>
      </DialogTitle>
      <DialogContent>
        {!showForm ? (
          <Typography>
            You just returned <strong>{bookTitle}</strong>. Would you like to rate it now?
          </Typography>
        ) : (
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Rate <strong>{bookTitle}</strong>
            </Typography>
            <RatingForm bookId={bookId} onSuccess={handleRatingSuccess} />
          </Stack>
        )}
      </DialogContent>
      {!showForm && (
        <DialogActions>
          <Button onClick={onClose}>Maybe Later</Button>
          <Button variant="contained" onClick={handleRateNow}>
            Rate Now
          </Button>
        </DialogActions>
      )}
    </Dialog>
  );
}
