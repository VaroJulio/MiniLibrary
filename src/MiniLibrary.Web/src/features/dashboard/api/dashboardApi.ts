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
  const { data } = await apiClient.get<DashboardStats>('/dashboard/stats');
  return data;
}

export async function fetchLoanMetrics(): Promise<LoanMetrics> {
  const { data } = await apiClient.get<LoanMetrics>('/dashboard/loan-metrics');
  return data;
}
