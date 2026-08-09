import { useQuery } from '@tanstack/react-query';
import { searchBooks, semanticSearch, type SearchFilters } from '../api/searchApi';

export function useTextSearch(page: number, pageSize: number, filters?: SearchFilters) {
  return useQuery({
    queryKey: ['text-search', page, pageSize, filters],
    queryFn: () => searchBooks(page, pageSize, filters),
    enabled: !!filters?.query || !!filters?.category || !!filters?.status,
    staleTime: 30_000, // 30s — search results are stable for a given query
  });
}

export function useSemanticSearch(query: string) {
  return useQuery({
    queryKey: ['semantic-search', query],
    queryFn: () => semanticSearch(query),
    enabled: query.trim().length > 0,
    staleTime: 60_000, // 60s — semantic results don't change for same query
  });
}
