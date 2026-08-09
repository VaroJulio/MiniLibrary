import { useQuery } from '@tanstack/react-query';
import { fetchDashboardStats, fetchLoanMetrics } from '../api/dashboardApi';

export function useDashboardStats() {
  return useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: fetchDashboardStats,
    staleTime: 60_000, // 60s — dashboard aggregates don't change rapidly
  });
}

export function useLoanMetrics() {
  return useQuery({
    queryKey: ['loan-metrics'],
    queryFn: fetchLoanMetrics,
    staleTime: 60_000, // 60s — metrics are aggregate data
  });
}
