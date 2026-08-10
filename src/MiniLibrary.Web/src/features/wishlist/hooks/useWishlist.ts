import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchWishlist, removeFromWishlist } from '../api/wishlistApi';

export function useWishlist(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['wishlist', page, pageSize],
    queryFn: () => fetchWishlist(page, pageSize),
    staleTime: 30_000, // 30s — wishlist changes on add/remove (invalidated by mutations)
  });
}

export function useRemoveFromWishlist() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (bookId: string) => removeFromWishlist(bookId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['wishlist'] });
    },
  });
}
