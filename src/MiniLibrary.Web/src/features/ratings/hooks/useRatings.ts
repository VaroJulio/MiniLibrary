import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchBookRatings,
  createOrUpdateRating,
  deleteRating,
  voteUseful,
  type CreateRatingRequest,
} from '../api/ratingsApi';

export function useBookRatings(bookId: string, page: number, pageSize: number) {
  return useQuery({
    queryKey: ['book-ratings', bookId, page, pageSize],
    queryFn: () => fetchBookRatings(bookId, page, pageSize),
    enabled: !!bookId,
  });
}

export function useCreateRating(bookId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateRatingRequest) => createOrUpdateRating(bookId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['book-ratings', bookId] });
      queryClient.invalidateQueries({ queryKey: ['book-detail', bookId] });
    },
  });
}

export function useDeleteRating(bookId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => deleteRating(bookId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['book-ratings', bookId] });
      queryClient.invalidateQueries({ queryKey: ['book-detail', bookId] });
    },
  });
}

export function useVoteUseful() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (ratingId: string) => voteUseful(ratingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['book-ratings'] });
    },
  });
}
