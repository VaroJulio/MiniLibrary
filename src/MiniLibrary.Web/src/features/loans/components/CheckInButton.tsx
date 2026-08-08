import { Button } from '@mui/material';
import AssignmentReturnIcon from '@mui/icons-material/AssignmentReturn';
import { useCheckIn } from '../hooks/useLoans';

interface CheckInButtonProps {
  bookId: string;
  disabled?: boolean;
}

export function CheckInButton({ bookId, disabled }: CheckInButtonProps) {
  const mutation = useCheckIn();

  return (
    <Button
      variant="outlined"
      size="small"
      startIcon={<AssignmentReturnIcon />}
      onClick={() => mutation.mutate(bookId)}
      disabled={disabled || mutation.isPending}
    >
      {mutation.isPending ? 'Returning...' : 'Return'}
    </Button>
  );
}
