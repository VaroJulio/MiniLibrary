import { apiClient } from '@/services/apiClient';
import type { BookLoan, PagedResponse } from '@/types/models';

export async function fetchLoanHistory(
  page: number,
  pageSize: number,
): Promise<PagedResponse<BookLoan>> {
  const { data } = await apiClient.get<PagedResponse<BookLoan>>('/loans/history', {
    params: { page, pageSize },
  });
  return data;
}

export async function checkOutBook(bookId: string): Promise<void> {
  await apiClient.post('/loans/checkout', { bookId });
}

export async function checkInBook(loanId: string): Promise<void> {
  await apiClient.post('/loans/checkin', { loanId });
}
