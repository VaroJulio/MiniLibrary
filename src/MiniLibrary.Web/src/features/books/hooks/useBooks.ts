import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchBooks, fetchBookById, createBook, updateBook, deleteBook } from '../api/booksApi';
import type { BookFilters, CreateBookRequest, UpdateBookRequest } from '../types';

const BOOKS_KEY = 'books';
const BOOK_DETAIL_KEY = 'book-detail';

export function useBooks(page: number, pageSize: number, filters?: BookFilters) {
  return useQuery({
    queryKey: [BOOKS_KEY, page, pageSize, filters],
    queryFn: () => fetchBooks(page, pageSize, filters),
    staleTime: 30_000, // 30s — book list changes infrequently
  });
}

export function useBookDetail(id: string | undefined) {
  return useQuery({
    queryKey: [BOOK_DETAIL_KEY, id],
    queryFn: () => fetchBookById(id!),
    enabled: !!id,
    staleTime: 60_000, // 60s — individual book details are fairly stable
  });
}

export function useCreateBook() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateBookRequest) => createBook(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [BOOKS_KEY] });
    },
  });
}

export function useUpdateBook() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateBookRequest) => updateBook(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [BOOKS_KEY] });
      queryClient.invalidateQueries({ queryKey: [BOOK_DETAIL_KEY, variables.id] });
    },
  });
}

export function useDeleteBook() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteBook(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [BOOKS_KEY] });
    },
  });
}
