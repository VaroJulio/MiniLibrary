import { useState } from 'react';
import { Button } from '@mui/material';
import AssignmentReturnIcon from '@mui/icons-material/AssignmentReturn';
import { useCheckIn } from '../hooks/useLoans';
import { CheckInRatingDialog } from './CheckInRatingDialog';

interface CheckInButtonProps {
  bookId: string;
  bookTitle?: string;
  disabled?: boolean;
}

export function CheckInButton({ bookId, bookTitle, disabled }: CheckInButtonProps) {
  const mutation = useCheckIn();
  const [showRatingDialog, setShowRatingDialog] = useState(false);

  const handleCheckIn = () => {
    mutation.mutate(bookId, {
      onSuccess: () => {
        setShowRatingDialog(true);
      },
    });
  };

  return (
    <>
      <Button
        variant="outlined"
        size="small"
        startIcon={<AssignmentReturnIcon />}
        onClick={handleCheckIn}
        disabled={disabled || mutation.isPending}
      >
        {mutation.isPending ? 'Returning...' : 'Return'}
      </Button>

      {showRatingDialog && (
        <CheckInRatingDialog
          open={showRatingDialog}
          bookId={bookId}
          bookTitle={bookTitle ?? 'this book'}
          onClose={() => setShowRatingDialog(false)}
        />
      )}
    </>
  );
}
