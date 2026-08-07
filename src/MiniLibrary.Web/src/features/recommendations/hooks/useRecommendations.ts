import { useQuery } from '@tanstack/react-query';
import { fetchRecommendations } from '../api/recommendationsApi';

export function useRecommendations() {
  return useQuery({
    queryKey: ['recommendations'],
    queryFn: fetchRecommendations,
    staleTime: 60 * 60 * 1000, // 1 hour (matches backend cache)
  });
}
