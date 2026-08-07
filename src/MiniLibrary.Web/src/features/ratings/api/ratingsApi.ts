import { apiClient } from '@/services/apiClient';
import type { Rating, PagedResponse } from '@/types/models';

export interface CreateRatingRequest {
  score: number;
  reviewText?: string;
}

export async function fetchBookRatings(
  bookId: string,
  page: number,
  pageSize: number,
): Promise<PagedResponse<Rating>> {
  const { data } = await apiClient.get<PagedResponse<Rating>>(`/books/${bookId}/ratings`, {
    params: { page, pageSize },
  });
  return data;
}

export async function createOrUpdateRating(
  bookId: string,
  request: CreateRatingRequest,
): Promise<void> {
  await apiClient.post(`/books/${bookId}/ratings`, request);
}

export async function deleteRating(bookId: string): Promise<void> {
  await apiClient.delete(`/books/${bookId}/ratings`);
}

export async function voteUseful(ratingId: string): Promise<void> {
  await apiClient.post(`/ratings/${ratingId}/useful`);
}
