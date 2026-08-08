import React, { Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline } from '@mui/material';
import { ThemeContextProvider } from '@/theme/ThemeContext';
import { AuthProvider } from '@/features/auth/AuthContext';
import { AppLayout } from '@/components/AppLayout';
import { LoadingSpinner } from '@/components/LoadingSpinner';

// Lazy-loaded route components for code splitting
const LoginPage = React.lazy(() => import('@/features/auth/LoginPage'));
const OAuthCallback = React.lazy(() => import('@/features/auth/OAuthCallback'));
const DashboardPage = React.lazy(() => import('@/features/dashboard/DashboardPage'));
const BookListPage = React.lazy(() => import('@/features/books/BookListPage'));
const BookDetailPage = React.lazy(() => import('@/features/books/BookDetailPage'));
const SearchPage = React.lazy(() => import('@/features/search/SearchPage'));
const LoanHistoryPage = React.lazy(() => import('@/features/loans/LoanHistoryPage'));
const RecommendationsPage = React.lazy(() => import('@/features/recommendations/RecommendationsPage'));
const RatingsPage = React.lazy(() => import('@/features/ratings/RatingsPage'));
const BookRankingsPage = React.lazy(() => import('@/features/rankings/BookRankingsPage'));
const ReaderRankingsPage = React.lazy(() => import('@/features/rankings/ReaderRankingsPage'));
const WishlistPage = React.lazy(() => import('@/features/wishlist/WishlistPage'));
const GamificationPage = React.lazy(() => import('@/features/gamification/GamificationPage'));
const NotificationsPage = React.lazy(() => import('@/features/notifications/NotificationsPage'));
const UserManagementPage = React.lazy(() => import('@/features/users/UserManagementPage'));

export function App() {
  return (
    <ThemeContextProvider>
      <CssBaseline />
      <AuthProvider>
        <Suspense fallback={<LoadingSpinner fullPage />}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/callback" element={<OAuthCallback />} />
            <Route element={<AppLayout />}>
              <Route path="/" element={<Navigate to="/books" replace />} />
              <Route path="/books" element={<BookListPage />} />
              <Route path="/books/:id" element={<BookDetailPage />} />
              <Route path="/search" element={<SearchPage />} />
              <Route path="/loans" element={<LoanHistoryPage />} />
              <Route path="/recommendations" element={<RecommendationsPage />} />
              <Route path="/ratings" element={<RatingsPage />} />
              <Route path="/rankings/books" element={<BookRankingsPage />} />
              <Route path="/rankings/readers" element={<ReaderRankingsPage />} />
              <Route path="/wishlist" element={<WishlistPage />} />
              <Route path="/gamification" element={<GamificationPage />} />
              <Route path="/notifications" element={<NotificationsPage />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/users" element={<UserManagementPage />} />
            </Route>
          </Routes>
        </Suspense>
      </AuthProvider>
    </ThemeContextProvider>
  );
}
