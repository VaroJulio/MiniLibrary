import { Button, Tooltip } from '@mui/material';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/services/apiClient';
import type { Book } from '@/types/models';

interface CheckOutButtonProps {
  book: Book;
}

export function CheckOutButton({ book }: CheckOutButtonProps) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async () => {
      await apiClient.post('/loans/checkout', { bookId: book.id });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['books'] });
      queryClient.invalidateQueries({ queryKey: ['book-detail', book.id] });
      queryClient.invalidateQueries({ queryKey: ['loan-history'] });
    },
  });

  if (book.status !== 'Available') {
    return (
      <Tooltip title="This book is currently checked out">
        <span>
          <Button variant="contained" disabled fullWidth startIcon={<ShoppingCartIcon />}>
            Unavailable
          </Button>
        </span>
      </Tooltip>
    );
  }

  return (
    <Button
      variant="contained"
      color="primary"
      fullWidth
      startIcon={<ShoppingCartIcon />}
      onClick={() => mutation.mutate()}
      disabled={mutation.isPending}
    >
      {mutation.isPending ? 'Checking out...' : 'Check Out'}
    </Button>
  );
}
