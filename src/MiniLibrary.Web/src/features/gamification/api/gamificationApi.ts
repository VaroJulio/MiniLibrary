import { apiClient } from '@/services/apiClient';
import type { Badge } from '@/types/models';

export interface LeaderboardEntry {
  userId: string;
  name: string;
  badgeCount: number;
}

export async function fetchMyBadges(): Promise<Badge[]> {
  const { data } = await apiClient.get<Badge[]>('/gamification/badges');
  return data;
}

export async function fetchUserBadges(userId: string): Promise<Badge[]> {
  const { data } = await apiClient.get<Badge[]>(`/gamification/badges/${userId}`);
  return data;
}

export async function fetchLeaderboard(): Promise<LeaderboardEntry[]> {
  const { data } = await apiClient.get<LeaderboardEntry[]>('/gamification/leaderboard');
  return data;
}
