import { apiClient } from '@/services/apiClient';
import type { WishlistEntry, PagedResponse } from '@/types/models';

export async function fetchWishlist(page: number, pageSize: number): Promise<PagedResponse<WishlistEntry>> {
  const { data } = await apiClient.get<PagedResponse<WishlistEntry>>('/wishlist', {
    params: { page, pageSize },
  });
  return data;
}

export async function removeFromWishlist(bookId: string): Promise<void> {
  await apiClient.delete(`/wishlist/${bookId}`);
}
