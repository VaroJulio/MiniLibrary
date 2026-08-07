import { apiClient } from '@/services/apiClient';
import type { User, UserRole, PagedResponse } from '@/types/models';

export async function fetchUsers(page: number, pageSize: number): Promise<PagedResponse<User>> {
  const { data } = await apiClient.get<PagedResponse<User>>('/users', {
    params: { page, pageSize },
  });
  return data;
}

export async function assignRole(userId: string, role: UserRole): Promise<void> {
  await apiClient.put(`/users/${userId}/role`, { role });
}
