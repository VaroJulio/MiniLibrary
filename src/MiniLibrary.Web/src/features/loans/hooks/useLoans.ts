import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchLoanHistory, checkOutBook, checkInBook } from '../api/loansApi';

const LOAN_HISTORY_KEY = 'loan-history';

export function useLoanHistory(page: number, pageSize: number) {
  return useQuery({
    queryKey: [LOAN_HISTORY_KEY, page, pageSize],
    queryFn: () => fetchLoanHistory(page, pageSize),
    staleTime: 30_000, // 30s — loan status changes on checkout/checkin (invalidated by mutations)
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
    queryKey: [LOAN_HISTORY_KEY, 1, 100],
    queryFn: () => fetchLoanHistory(1, 100),
    staleTime: 30_000,
  });

  if (!bookId || !data) return false;

  const hasReturnedLoan = data.data.some((loan) => loan.bookId === bookId && loan.returnedAt !== null);
  const hasActiveLoan = data.data.some((loan) => loan.bookId === bookId && loan.returnedAt === null);

  // Show rating form only if user has completed a loan AND doesn't currently have the book checked out
  return hasReturnedLoan && !hasActiveLoan;
}
