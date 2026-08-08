import { apiClient } from '@/services/apiClient';
import type { PagedResponse } from '@/types/models';

export interface CreateRatingRequest {
  score: number;
  reviewText?: string;
}

export interface MyRating {
  id: string;
  bookId: string;
  bookTitle: string;
  bookAuthor: string;
  score: number;
  reviewText: string;
  usefulVotes: number;
  createdAt: string;
  updatedAt: string;
}

export interface CommunityRating {
  id: string;
  bookId: string;
  bookTitle: string;
  bookAuthor: string;
  userId: string;
  userName: string;
  score: number;
  reviewText: string;
  usefulVotes: number;
  createdAt: string;
}

export interface BookRating {
  id: string;
  bookId: string;
  userId: string;
  userName: string;
  score: number;
  reviewText: string;
  usefulVotes: number;
  createdAt: string;
  updatedAt: string;
}

export async function fetchBookRatings(
  bookId: string,
  page: number,
  pageSize: number,
): Promise<PagedResponse<BookRating>> {
  const { data } = await apiClient.get<PagedResponse<BookRating>>(`/books/${bookId}/ratings`, {
    params: { page, pageSize },
  });
  return data;
}

export async function fetchMyRatings(
  page: number,
  pageSize: number,
): Promise<PagedResponse<MyRating>> {
  const { data } = await apiClient.get<PagedResponse<MyRating>>('/ratings/my', {
    params: { page, pageSize },
  });
  return data;
}

export async function fetchRecentRatings(
  page: number,
  pageSize: number,
): Promise<PagedResponse<CommunityRating>> {
  const { data } = await apiClient.get<PagedResponse<CommunityRating>>('/ratings/recent', {
    params: { page, pageSize },
  });
  return data;
}

export async function createOrUpdateRating(
  bookId: string,
  request: CreateRatingRequest,
): Promise<void> {
  await apiClient.post(`/books/${bookId}/ratings`, request);
}

export async function deleteRating(bookId: string): Promise<void> {
  await apiClient.delete(`/books/${bookId}/ratings`);
}

export async function voteUseful(ratingId: string): Promise<void> {
  await apiClient.post(`/ratings/${ratingId}/useful`);
}
