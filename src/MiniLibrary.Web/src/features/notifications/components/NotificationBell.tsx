import { Badge, IconButton, keyframes } from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';

const bellShake = keyframes`
  0% { transform: rotate(0deg); }
  10% { transform: rotate(14deg); }
  20% { transform: rotate(-12deg); }
  30% { transform: rotate(10deg); }
  40% { transform: rotate(-8deg); }
  50% { transform: rotate(6deg); }
  60% { transform: rotate(-4deg); }
  70% { transform: rotate(2deg); }
  80% { transform: rotate(-1deg); }
  90% { transform: rotate(1deg); }
  100% { transform: rotate(0deg); }
`;

interface NotificationBellProps {
  onClick: () => void;
  unreadCount: number;
  animate?: boolean;
}

export function NotificationBell({ onClick, unreadCount, animate = false }: NotificationBellProps) {
  const Icon = animate ? NotificationsActiveIcon : NotificationsIcon;

  return (
    <IconButton
      aria-label="notifications"
      onClick={onClick}
      sx={
        animate
          ? {
              animation: `${bellShake} 0.6s ease-in-out`,
              animationIterationCount: 3,
            }
          : undefined
      }
    >
      <Badge badgeContent={unreadCount} color="error" max={99}>
        <Icon
          sx={
            animate
              ? { color: 'warning.main' }
              : undefined
          }
        />
      </Badge>
    </IconButton>
  );
}
