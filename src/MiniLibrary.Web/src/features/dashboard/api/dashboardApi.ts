import { apiClient } from '@/services/apiClient';

export interface DashboardStats {
  totalBooks: number;
  availableBooks: number;
  checkedOutBooks: number;
  activeLoans: number;
  usersByRole: { role: string; count: number }[];
}

export interface LoanMetrics {
  loansByPeriod: { period: string; count: number }[];
  popularCategories: { category: string; count: number }[];
  topBooks: { title: string; author: string; loanCount: number }[];
}

export async function fetchDashboardStats(): Promise<DashboardStats> {
  const { data } = await apiClient.get<{ data: { totalBooks: number; availableBooks: number; checkedOutBooks: number; activeLoans: number; usersByRole: Record<string, number> } }>('/dashboard/stats');
  const raw = data.data;
  return {
    totalBooks: raw.totalBooks,
    availableBooks: raw.availableBooks,
    checkedOutBooks: raw.checkedOutBooks,
    activeLoans: raw.activeLoans,
    usersByRole: Object.entries(raw.usersByRole).map(([role, count]) => ({ role, count })),
  };
}

export async function fetchLoanMetrics(): Promise<LoanMetrics> {
  const { data } = await apiClient.get<{ data: LoanMetrics }>('/dashboard/loan-metrics');
  return data.data;
}
