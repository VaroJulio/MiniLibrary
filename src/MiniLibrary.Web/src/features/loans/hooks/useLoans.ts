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
    mutationFn: (loanId: string) => checkInBook(loanId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [LOAN_HISTORY_KEY] });
      queryClient.invalidateQueries({ queryKey: ['books'] });
      queryClient.invalidateQueries({ queryKey: ['book-detail'] });
    },
  });
}
