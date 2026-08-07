import { useQuery } from '@tanstack/react-query';
import { searchBooks, semanticSearch, type SearchFilters } from '../api/searchApi';

export function useTextSearch(page: number, pageSize: number, filters?: SearchFilters) {
  return useQuery({
    queryKey: ['text-search', page, pageSize, filters],
    queryFn: () => searchBooks(page, pageSize, filters),
    enabled: !!filters?.query || !!filters?.category || !!filters?.status,
  });
}

export function useSemanticSearch(query: string) {
  return useQuery({
    queryKey: ['semantic-search', query],
    queryFn: () => semanticSearch(query),
    enabled: query.trim().length > 0,
  });
}
