import { Box, Pagination, Skeleton, Stack, Typography } from '@mui/material';
import type { PaginationMetadata } from '@/types/models';
import { EmptyState } from './EmptyState';

interface PagedListProps<T> {
  items: T[] | undefined;
  pagination: PaginationMetadata | undefined;
  isLoading: boolean;
  onPageChange: (page: number) => void;
  renderItem: (item: T, index: number) => React.ReactNode;
  skeletonCount?: number;
  skeletonHeight?: number;
  emptyTitle?: string;
  emptyMessage?: string;
}

export function PagedList<T>({
  items,
  pagination,
  isLoading,
  onPageChange,
  renderItem,
  skeletonCount = 5,
  skeletonHeight = 72,
  emptyTitle,
  emptyMessage,
}: PagedListProps<T>) {
  if (isLoading) {
    return (
      <Stack spacing={1}>
        {Array.from({ length: skeletonCount }, (_, i) => (
          <Skeleton
            key={i}
            variant="rounded"
            height={skeletonHeight}
            sx={{ borderRadius: 2 }}
          />
        ))}
      </Stack>
    );
  }

  if (!items || items.length === 0) {
    return <EmptyState title={emptyTitle} message={emptyMessage} />;
  }

  return (
    <Box>
      <Stack spacing={1}>
        {items.map((item, index) => renderItem(item, index))}
      </Stack>
      {pagination && pagination.totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', mt: 3, gap: 2 }}>
          <Pagination
            count={pagination.totalPages}
            page={pagination.currentPage}
            onChange={(_, page) => onPageChange(page)}
            color="primary"
            shape="rounded"
          />
          <Typography variant="caption" color="text.secondary">
            {pagination.totalCount} total
          </Typography>
        </Box>
      )}
    </Box>
  );
}
