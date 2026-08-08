import { apiClient } from '@/services/apiClient';

export interface EarnedBadge {
  badgeType: string;
  earnedAt: string;
}

export interface PendingBadge {
  badgeType: string;
  currentCount: number;
  requiredCount: number;
  progressPercent: number;
}

export interface UserBadgesResponse {
  earnedBadges: EarnedBadge[];
  pendingBadges: PendingBadge[];
}

export interface LeaderboardEntry {
  position: number;
  userId: string;
  name: string;
  badgeCount: number;
}

export async function fetchMyBadges(): Promise<UserBadgesResponse> {
  const { data } = await apiClient.get<{ data: UserBadgesResponse }>('/gamification/badges');
  return data.data;
}

export async function fetchLeaderboard(): Promise<LeaderboardEntry[]> {
  const { data } = await apiClient.get<{ data: LeaderboardEntry[] }>('/gamification/leaderboard');
  return data.data;
}
