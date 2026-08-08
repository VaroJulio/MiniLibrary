import { useQuery } from '@tanstack/react-query';
import { fetchBookRankings, fetchReaderRankings } from '../api/rankingsApi';

export function useBookRankings(params?: { category?: string; sortBy?: string }) {
  return useQuery({
    queryKey: ['book-rankings', params],
    queryFn: () => fetchBookRankings(params),
    staleTime: 15 * 60 * 1000, // 15 min cache
  });
}

export function useReaderRankings(period?: string) {
  return useQuery({
    queryKey: ['reader-rankings', period],
    queryFn: () => fetchReaderRankings(period),
    staleTime: 60 * 60 * 1000, // 1 hour cache
  });
}
