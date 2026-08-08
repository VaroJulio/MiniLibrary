import { Box, Typography } from '@mui/material';
import type { SvgIconComponent } from '@mui/icons-material';
import InboxIcon from '@mui/icons-material/Inbox';

interface EmptyStateProps {
  title?: string;
  message?: string;
  icon?: SvgIconComponent;
  action?: React.ReactNode;
}

export function EmptyState({
  title = 'Nothing here yet',
  message = 'There are no items to display.',
  icon: Icon = InboxIcon,
  action,
}: EmptyStateProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 8,
        px: 2,
        textAlign: 'center',
      }}
    >
      <Icon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
      <Typography variant="h6" color="text.secondary" gutterBottom>
        {title}
      </Typography>
      <Typography variant="body2" color="text.disabled" sx={{ maxWidth: 360, mb: action ? 3 : 0 }}>
        {message}
      </Typography>
      {action}
    </Box>
  );
}
