import { apiClient } from '@/services/apiClient';
import type { Notification, PagedResponse } from '@/types/models';

export async function fetchNotifications(
  page: number,
  pageSize: number,
): Promise<PagedResponse<Notification>> {
  const { data } = await apiClient.get<PagedResponse<Notification>>('/notifications', {
    params: { page, pageSize },
  });
  return data;
}

export async function markNotificationRead(id: string): Promise<void> {
  await apiClient.put(`/notifications/${id}/read`);
}
