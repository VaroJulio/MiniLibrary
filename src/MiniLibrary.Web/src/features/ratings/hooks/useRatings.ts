import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  fetchBookRatings,
  fetchMyRatings,
  fetchRecentRatings,
  fetchCanRate,
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

export function useMyRatings(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['my-ratings', page, pageSize],
    queryFn: () => fetchMyRatings(page, pageSize),
  });
}

export function useRecentRatings(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['recent-ratings', page, pageSize],
    queryFn: () => fetchRecentRatings(page, pageSize),
  });
}

export function useCanRate(bookId: string | undefined) {
  return useQuery({
    queryKey: ['can-rate', bookId],
    queryFn: () => fetchCanRate(bookId!),
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
      queryClient.invalidateQueries({ queryKey: ['my-ratings'] });
      queryClient.invalidateQueries({ queryKey: ['recent-ratings'] });
      queryClient.invalidateQueries({ queryKey: ['can-rate', bookId] });
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
      queryClient.invalidateQueries({ queryKey: ['my-ratings'] });
      queryClient.invalidateQueries({ queryKey: ['recent-ratings'] });
      queryClient.invalidateQueries({ queryKey: ['can-rate', bookId] });
    },
  });
}

export function useVoteUseful() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (ratingId: string) => voteUseful(ratingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['book-ratings'] });
      queryClient.invalidateQueries({ queryKey: ['recent-ratings'] });
    },
  });
}
