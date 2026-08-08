import { Badge, IconButton } from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import { useUnreadCount } from '../hooks/useNotifications';

interface NotificationBellProps {
  onClick: () => void;
}

export function NotificationBell({ onClick }: NotificationBellProps) {
  const unreadCount = useUnreadCount();

  return (
    <IconButton aria-label="notifications" onClick={onClick}>
      <Badge badgeContent={unreadCount} color="error" max={99}>
        <NotificationsIcon />
      </Badge>
    </IconButton>
  );
}
