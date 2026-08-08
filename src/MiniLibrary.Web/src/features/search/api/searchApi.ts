import { apiClient } from '@/services/apiClient';
import type { Book, PagedResponse } from '@/types/models';

export interface SearchFilters {
  query?: string;
  category?: string;
  status?: string;
  yearFrom?: number;
  yearTo?: number;
}

export interface SemanticSearchResult {
  id: string;
  title: string;
  author: string;
  isbn: string;
  category: string;
  description: string;
  publicationYear: number;
  status: string;
  relevanceScore: number;
}

export interface SemanticSearchResponse {
  results: SemanticSearchResult[];
  usedFallback: boolean;
}

export async function searchBooks(
  page: number,
  pageSize: number,
  filters?: SearchFilters,
): Promise<PagedResponse<Book>> {
  const params: Record<string, string | number> = { page, pageSize };
  if (filters?.query) params.q = filters.query;
  if (filters?.category) params.category = filters.category;
  if (filters?.status) params.status = filters.status;
  if (filters?.yearFrom) params.yearFrom = filters.yearFrom;
  if (filters?.yearTo) params.yearTo = filters.yearTo;

  const { data } = await apiClient.get<PagedResponse<Book>>('/search/books', { params });
  return data;
}

export async function semanticSearch(query: string): Promise<SemanticSearchResponse> {
  const { data } = await apiClient.get<{ data: SemanticSearchResult[]; usedFallback: boolean; totalResults: number }>('/search/semantic', {
    params: { q: query },
  });
  return { results: data.data, usedFallback: data.usedFallback };
}
