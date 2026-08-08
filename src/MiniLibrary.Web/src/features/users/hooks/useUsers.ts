import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchUsers, assignRole } from '../api/usersApi';
import type { UserRole } from '@/types/models';

export function useUsers(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['users', page, pageSize],
    queryFn: () => fetchUsers(page, pageSize),
  });
}

export function useAssignRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: UserRole }) => assignRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });
}
