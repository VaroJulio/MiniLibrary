import { useState } from 'react';
import { Alert, Box, Button, Rating, Stack, TextField, Typography } from '@mui/material';
import { useCreateRating } from '../hooks/useRatings';

interface RatingFormProps {
  bookId: string;
  onSuccess?: () => void;
}

export function RatingForm({ bookId, onSuccess }: RatingFormProps) {
  const [score, setScore] = useState<number | null>(null);
  const [reviewText, setReviewText] = useState('');
  const mutation = useCreateRating(bookId);

  const handleSubmit = () => {
    if (!score) return;
    mutation.mutate(
      { score, reviewText: reviewText.trim() || undefined },
      { onSuccess: () => { onSuccess?.(); setScore(null); setReviewText(''); } },
    );
  };

  return (
    <Box>
      {mutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(mutation.error as Error)?.message ?? 'Failed to submit rating. You must have read this book to rate it.'}
        </Alert>
      )}
      <Stack spacing={2}>
        <Box>
          <Typography variant="body2" sx={{ mb: 0.5 }}>Your rating</Typography>
          <Rating
            value={score}
            onChange={(_, newValue) => setScore(newValue)}
            size="large"
          />
        </Box>
        <TextField
          label="Review (optional)"
          value={reviewText}
          onChange={(e) => setReviewText(e.target.value)}
          multiline
          rows={3}
          fullWidth
          inputProps={{ maxLength: 1000 }}
          helperText={`${reviewText.length}/1000`}
        />
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={!score || mutation.isPending}
        >
          {mutation.isPending ? 'Submitting...' : 'Submit Rating'}
        </Button>
      </Stack>
    </Box>
  );
}
