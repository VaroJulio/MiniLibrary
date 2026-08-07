import { useQuery } from '@tanstack/react-query';
import { fetchMyBadges, fetchLeaderboard } from '../api/gamificationApi';

export function useMyBadges() {
  return useQuery({
    queryKey: ['my-badges'],
    queryFn: fetchMyBadges,
  });
}

export function useLeaderboard() {
  return useQuery({
    queryKey: ['gamification-leaderboard'],
    queryFn: fetchLeaderboard,
    staleTime: 60 * 60 * 1000, // 1 hour
  });
}
