import type { Book } from '@/types/models';

export interface BookFilters {
  query?: string;
  category?: string;
  status?: string;
  yearFrom?: number;
  yearTo?: number;
}

export interface CreateBookRequest {
  title: string;
  author: string;
  isbn: string;
  category: string;
  description: string;
  publicationYear: number;
}

export interface UpdateBookRequest extends CreateBookRequest {
  id: string;
}

export interface BookDetailResponse extends Book {
  recentRatings?: {
    id: string;
    userName: string;
    score: number;
    reviewText: string | null;
    createdAt: string;
  }[];
}
