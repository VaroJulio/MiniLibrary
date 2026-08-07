import { useQuery } from '@tanstack/react-query';
import { fetchMyBadges, fetchLeaderboard, type UserBadgesResponse, type LeaderboardEntry } from '../api/gamificationApi';

export function useMyBadges() {
  return useQuery<UserBadgesResponse>({
    queryKey: ['my-badges'],
    queryFn: fetchMyBadges,
  });
}

export function useLeaderboard() {
  return useQuery<LeaderboardEntry[]>({
    queryKey: ['gamification-leaderboard'],
    queryFn: fetchLeaderboard,
    staleTime: 60 * 60 * 1000,
  });
}
