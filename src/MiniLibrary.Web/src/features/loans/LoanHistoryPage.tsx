import { useState } from 'react';
import {
  Box,
  Chip,
  Pagination,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { useLoanHistory } from './hooks/useLoans';
import { CheckInButton } from './components/CheckInButton';
import { EmptyState } from '@/components/EmptyState';
import type { BookLoan } from '@/types/models';

const PAGE_SIZE = 20;

function getLoanStatus(loan: BookLoan): { label: string; color: 'success' | 'warning' | 'error' } {
  if (loan.returnedAt) {
    return { label: 'Returned', color: 'success' };
  }
  const now = new Date();
  const dueDate = new Date(loan.dueDate);
  if (now > dueDate) {
    return { label: 'Overdue', color: 'error' };
  }
  return { label: 'Active', color: 'warning' };
}

export default function LoanHistoryPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useLoanHistory(page, PAGE_SIZE);

  return (
    <Box>
      <Typography variant="h4" component="h1" fontWeight={700} sx={{ mb: 3 }}>
        My Loans
      </Typography>

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={52} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !data || data.data.length === 0 ? (
        <EmptyState
          title="No loan history"
          message="You haven't borrowed any books yet. Visit the catalog to check out a book."
        />
      ) : (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Book</TableCell>
                  <TableCell>Borrowed</TableCell>
                  <TableCell>Due Date</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.data.map((loan) => {
                  const status = getLoanStatus(loan);
                  return (
                    <TableRow key={loan.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {loan.bookTitle}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {new Date(loan.borrowedAt).toLocaleDateString()}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {new Date(loan.dueDate).toLocaleDateString()}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip label={status.label} color={status.color} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        {!loan.returnedAt && (
                          <CheckInButton bookId={loan.bookId} bookTitle={loan.bookTitle} />
                        )}
                        {loan.returnedAt && (
                          <Typography variant="caption" color="text.disabled">
                            {new Date(loan.returnedAt).toLocaleDateString()}
                          </Typography>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>

          {data.pagination.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination
                count={data.pagination.totalPages}
                page={page}
                onChange={(_, p) => setPage(p)}
                color="primary"
                shape="rounded"
              />
            </Box>
          )}
        </>
      )}
    </Box>
  );
}
