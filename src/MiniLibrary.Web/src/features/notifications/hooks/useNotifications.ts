import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchNotifications, markNotificationRead } from '../api/notificationsApi';

export function useNotifications(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['notifications', page, pageSize],
    queryFn: () => fetchNotifications(page, pageSize),
    staleTime: 10_000, // 10s — notifications are dynamic but don't need instant refresh
    refetchInterval: 60 * 1000, // Poll every 60s for new notifications
  });
}

export function useUnreadCount() {
  const { data } = useNotifications(1, 50);
  return data?.data.filter((n) => !n.isRead).length ?? 0;
}

export function useMarkRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => markNotificationRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}
