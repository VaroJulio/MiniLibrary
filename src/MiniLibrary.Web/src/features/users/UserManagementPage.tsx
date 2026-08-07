import { useState } from 'react';
import {
  Alert,
  Box,
  Chip,
  MenuItem,
  Pagination,
  Paper,
  Select,
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
import PeopleIcon from '@mui/icons-material/People';
import { useUsers, useAssignRole } from './hooks/useUsers';
import { EmptyState } from '@/components/EmptyState';
import type { User, UserRole } from '@/types/models';

const PAGE_SIZE = 20;
const ROLES: UserRole[] = ['Admin', 'Librarian', 'Member'];

const ROLE_COLORS: Record<UserRole, 'error' | 'warning' | 'default'> = {
  Admin: 'error',
  Librarian: 'warning',
  Member: 'default',
};

export default function UserManagementPage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useUsers(page, PAGE_SIZE);
  const assignRoleMutation = useAssignRole();

  const handleRoleChange = (user: User, newRole: UserRole) => {
    if (newRole === user.role) return;
    assignRoleMutation.mutate({ userId: user.id, role: newRole });
  };

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <PeopleIcon color="primary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          User Management
        </Typography>
      </Stack>

      {assignRoleMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(assignRoleMutation.error as Error)?.message ?? 'Failed to update role. Cannot remove the last Admin.'}
        </Alert>
      )}

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 10 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={52} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !data || data.data.length === 0 ? (
        <EmptyState title="No users" message="No users found in the system." />
      ) : (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Current Role</TableCell>
                  <TableCell>Change Role</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.data.map((user) => (
                  <TableRow key={user.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontWeight={500}>{user.name}</Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">{user.email}</Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.role}
                        color={ROLE_COLORS[user.role]}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Select
                        value={user.role}
                        onChange={(e) => handleRoleChange(user, e.target.value as UserRole)}
                        size="small"
                        sx={{ minWidth: 120 }}
                        disabled={assignRoleMutation.isPending}
                      >
                        {ROLES.map((role) => (
                          <MenuItem key={role} value={role}>{role}</MenuItem>
                        ))}
                      </Select>
                    </TableCell>
                  </TableRow>
                ))}
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
