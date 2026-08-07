import { useQuery } from '@tanstack/react-query';
import { fetchDashboardStats, fetchLoanMetrics } from '../api/dashboardApi';

export function useDashboardStats() {
  return useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: fetchDashboardStats,
  });
}

export function useLoanMetrics() {
  return useQuery({
    queryKey: ['loan-metrics'],
    queryFn: fetchLoanMetrics,
  });
}
