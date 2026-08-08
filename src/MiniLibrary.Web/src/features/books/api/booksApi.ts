import { apiClient } from '@/services/apiClient';
import type { Book, PagedResponse } from '@/types/models';
import type { BookFilters, CreateBookRequest, UpdateBookRequest, BookDetailResponse } from '../types';

export async function fetchBooks(
  page: number,
  pageSize: number,
  filters?: BookFilters,
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

export async function fetchBookById(id: string): Promise<BookDetailResponse> {
  const { data } = await apiClient.get<BookDetailResponse>(`/books/${id}`);
  return data;
}

export async function createBook(request: CreateBookRequest): Promise<Book> {
  const { data } = await apiClient.post<Book>('/books', request);
  return data;
}

export async function updateBook(request: UpdateBookRequest): Promise<Book> {
  const { id, ...body } = request;
  const { data } = await apiClient.put<Book>(`/books/${id}`, body);
  return data;
}

export async function deleteBook(id: string): Promise<void> {
  await apiClient.delete(`/books/${id}`);
}
