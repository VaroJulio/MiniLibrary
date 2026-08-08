export type UserRole = 'Admin' | 'Librarian' | 'Member';

export interface User {
  id: string;
  email: string;
  name: string;
  role: UserRole;
}

export type BookStatus = 'Available' | 'CheckedOut';

export interface Book {
  id: string;
  title: string;
  author: string;
  isbn: string;
  category: string;
  description: string;
  publicationYear: number;
  status: BookStatus;
  averageRating: number;
  totalRatings: number;
}

export interface BookLoan {
  id: string;
  bookId: string;
  bookTitle: string;
  borrowedAt: string;
  dueDate: string;
  returnedAt: string | null;
}

export interface Rating {
  id: string;
  bookId: string;
  userId: string;
  userName: string;
  score: number;
  reviewText: string | null;
  createdAt: string;
  usefulVotes: number;
}

export interface Recommendation {
  title: string;
  author: string;
  category: string;
  justification: string;
}

export interface WishlistEntry {
  bookId: string;
  title: string;
  author: string;
  bookStatus: BookStatus;
  addedAt: string;
}

export interface Badge {
  id: string;
  type: string;
  name: string;
  description: string;
  earnedAt: string | null;
  progress: number;
}

export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface PagedResponse<T> {
  data: T[];
  pagination: PaginationMetadata;
}
