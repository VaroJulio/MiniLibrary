import { Button } from '@mui/material';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/services/apiClient';
import type { BookStatus, WishlistEntry, PagedResponse } from '@/types/models';

interface WishlistButtonProps {
  bookId: string;
  bookStatus: BookStatus;
}

export function WishlistButton({ bookId, bookStatus }: WishlistButtonProps) {
  const queryClient = useQueryClient();

  const { data: wishlist } = useQuery({
    queryKey: ['wishlist'],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResponse<WishlistEntry>>('/wishlist', {
        params: { page: 1, pageSize: 20 },
      });
      return data;
    },
  });

  const isInWishlist = wishlist?.data.some((entry) => entry.bookId === bookId) ?? false;

  const addMutation = useMutation({
    mutationFn: async () => {
      await apiClient.post('/wishlist', { bookId });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wishlist'] });
    },
  });

  const removeMutation = useMutation({
    mutationFn: async () => {
      await apiClient.delete(`/wishlist/${bookId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wishlist'] });
    },
  });

  const isPending = addMutation.isPending || removeMutation.isPending;

  const handleClick = () => {
    if (isInWishlist) {
      removeMutation.mutate();
    } else {
      addMutation.mutate();
    }
  };

  // Only show wishlist button if book is checked out (want to be notified when available)
  if (bookStatus === 'Available' && !isInWishlist) {
    return null;
  }

  return (
    <Button
      variant={isInWishlist ? 'contained' : 'outlined'}
      color={isInWishlist ? 'error' : 'primary'}
      startIcon={isInWishlist ? <FavoriteIcon /> : <FavoriteBorderIcon />}
      onClick={handleClick}
      disabled={isPending}
      fullWidth
    >
      {isInWishlist ? 'In Wishlist' : 'Add to Wishlist'}
    </Button>
  );
}
