import { apiClient } from '@/services/apiClient';

export interface BookRanking {
  position: number;
  title: string;
  author: string;
  category: string;
  averageRating: number;
  totalRatings: number;
  totalLoans: number;
  status: string;
}

export interface ReaderRanking {
  position: number;
  name: string;
  booksRead: number;
  favoriteCategory: string;
  averageRatingGiven: number;
}

export interface ReaderRankingsResponse {
  rankings: ReaderRanking[];
  myPosition: number | null;
}

export async function fetchBookRankings(params?: {
  category?: string;
  sortBy?: string;
}): Promise<BookRanking[]> {
  const { data } = await apiClient.get<BookRanking[]>('/rankings/books', { params });
  return data;
}

export async function fetchReaderRankings(period?: string): Promise<ReaderRankingsResponse> {
  const { data } = await apiClient.get<ReaderRankingsResponse>('/rankings/readers', {
    params: period ? { period } : undefined,
  });
  return data;
}
