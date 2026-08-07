import { Button } from '@mui/material';
import AssignmentReturnIcon from '@mui/icons-material/AssignmentReturn';
import { useCheckIn } from '../hooks/useLoans';

interface CheckInButtonProps {
  loanId: string;
  disabled?: boolean;
}

export function CheckInButton({ loanId, disabled }: CheckInButtonProps) {
  const mutation = useCheckIn();

  return (
    <Button
      variant="outlined"
      size="small"
      startIcon={<AssignmentReturnIcon />}
      onClick={() => mutation.mutate(loanId)}
      disabled={disabled || mutation.isPending}
    >
      {mutation.isPending ? 'Returning...' : 'Return'}
    </Button>
  );
}
