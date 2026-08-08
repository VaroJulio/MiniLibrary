import { apiClient } from '@/services/apiClient';
import type { Recommendation } from '@/types/models';

export async function fetchRecommendations(): Promise<Recommendation[]> {
  const { data } = await apiClient.get<{ data: Recommendation[] }>('/recommendations');
  return data.data;
}
