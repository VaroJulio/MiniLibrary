import { useState } from 'react';
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  IconButton,
  Pagination,
  Skeleton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import VolumeUpIcon from '@mui/icons-material/VolumeUp';
import VolumeOffIcon from '@mui/icons-material/VolumeOff';
import { useNotifications, useMarkRead } from './hooks/useNotifications';
import { isSoundMuted, setSoundMuted } from './hooks/useNotificationSound';
import { EmptyState } from '@/components/EmptyState';

const PAGE_SIZE = 20;

export default function NotificationsPage() {
  const [page, setPage] = useState(1);
  const [muted, setMuted] = useState(isSoundMuted);
  const { data, isLoading } = useNotifications(page, PAGE_SIZE);
  const markReadMutation = useMarkRead();

  const handleClick = (id: string, isRead: boolean) => {
    if (!isRead) {
      markReadMutation.mutate(id);
    }
  };

  const handleToggleMute = () => {
    const newValue = !muted;
    setSoundMuted(newValue);
    setMuted(newValue);
  };

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <NotificationsIcon color="primary" />
        <Typography variant="h4" component="h1" fontWeight={700} sx={{ flexGrow: 1 }}>
          Notifications
        </Typography>
        <Tooltip title={muted ? 'Unmute notification sounds' : 'Mute notification sounds'}>
          <IconButton onClick={handleToggleMute} aria-label="toggle notification sound">
            {muted ? <VolumeOffIcon color="disabled" /> : <VolumeUpIcon color="primary" />}
          </IconButton>
        </Tooltip>
      </Stack>

      {isLoading ? (
        <Stack spacing={1}>
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={72} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      ) : !data || data.data.length === 0 ? (
        <EmptyState
          title="No notifications"
          message="You're all caught up! Notifications will appear here when books become available or you earn badges."
        />
      ) : (
        <>
          <Stack spacing={1}>
            {data.data.map((notification) => (
              <Card
                key={notification.id}
                sx={{
                  opacity: notification.isRead ? 0.7 : 1,
                  borderLeft: notification.isRead ? 'none' : '3px solid',
                  borderColor: 'primary.main',
                }}
              >
                <CardActionArea onClick={() => handleClick(notification.id, notification.isRead)}>
                  <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                    <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="subtitle2" fontWeight={notification.isRead ? 400 : 600}>
                          {notification.title}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {notification.message}
                        </Typography>
                      </Box>
                      <Stack alignItems="flex-end" spacing={0.5}>
                        <Typography variant="caption" color="text.disabled">
                          {new Date(notification.createdAt).toLocaleDateString()}
                        </Typography>
                        {!notification.isRead && (
                          <Chip label="New" size="small" color="primary" />
                        )}
                      </Stack>
                    </Stack>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Stack>
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
