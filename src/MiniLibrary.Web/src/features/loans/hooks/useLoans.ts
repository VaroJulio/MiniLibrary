import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchLoanHistory, checkOutBook, checkInBook } from '../api/loansApi';

const LOAN_HISTORY_KEY = 'loan-history';

export function useLoanHistory(page: number, pageSize: number) {
  return useQuery({
    queryKey: [LOAN_HISTORY_KEY, page, pageSize],
    queryFn: () => fetchLoanHistory(page, pageSize),
  });
}

export function useCheckOut() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (bookId: string) => checkOutBook(bookId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [LOAN_HISTORY_KEY] });
      queryClient.invalidateQueries({ queryKey: ['books'] });
      queryClient.invalidateQueries({ queryKey: ['book-detail'] });
    },
  });
}

export function useCheckIn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (bookId: string) => checkInBook(bookId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [LOAN_HISTORY_KEY] });
      queryClient.invalidateQueries({ queryKey: ['books'] });
      queryClient.invalidateQueries({ queryKey: ['book-detail'] });
    },
  });
}

export function useHasReadBook(bookId: string | undefined) {
  const { data } = useQuery({
    queryKey: [LOAN_HISTORY_KEY, 'all'],
    queryFn: () => fetchLoanHistory(1, 100),
    staleTime: 5 * 60 * 1000,
  });

  if (!bookId || !data) return false;
  return data.data.some((loan) => loan.bookId === bookId && loan.returnedAt !== null);
}
