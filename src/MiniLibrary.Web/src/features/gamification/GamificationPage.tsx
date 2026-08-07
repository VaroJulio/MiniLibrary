import {
  Box,
  Card,
  CardContent,
  Chip,
  LinearProgress,
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
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';
import { useMyBadges, useLeaderboard } from './hooks/useGamification';
import { EmptyState } from '@/components/EmptyState';
import type { Badge } from '@/types/models';

export default function GamificationPage() {
  const { data: badges, isLoading: badgesLoading } = useMyBadges();
  const { data: leaderboard, isLoading: leaderboardLoading } = useLeaderboard();

  const earnedBadges = badges?.filter((b) => b.earnedAt) ?? [];
  const pendingBadges = badges?.filter((b) => !b.earnedAt) ?? [];

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <EmojiEventsIcon color="secondary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          Badges &amp; Achievements
        </Typography>
      </Stack>

      {/* Earned Badges */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
        Earned ({earnedBadges.length})
      </Typography>

      {badgesLoading ? (
        <Stack direction="row" spacing={2} sx={{ mb: 4, flexWrap: 'wrap' }}>
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} variant="rounded" width={160} height={100} sx={{ borderRadius: 3 }} />
          ))}
        </Stack>
      ) : earnedBadges.length === 0 ? (
        <EmptyState title="No badges yet" message="Keep reading and engaging to earn your first badge!" />
      ) : (
        <Stack direction="row" spacing={2} sx={{ mb: 4, flexWrap: 'wrap', gap: 2 }}>
          {earnedBadges.map((badge) => (
            <BadgeCard key={badge.id} badge={badge} earned />
          ))}
        </Stack>
      )}

      {/* Pending Badges */}
      {pendingBadges.length > 0 && (
        <>
          <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
            In Progress ({pendingBadges.length})
          </Typography>
          <Stack spacing={1.5} sx={{ mb: 4 }}>
            {pendingBadges.map((badge) => (
              <Card key={badge.id} variant="outlined">
                <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
                    <Box>
                      <Typography variant="subtitle2" fontWeight={600}>{badge.name}</Typography>
                      <Typography variant="caption" color="text.secondary">{badge.description}</Typography>
                    </Box>
                    <Chip label={`${Math.round(badge.progress)}%`} size="small" variant="outlined" />
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={badge.progress}
                    sx={{ height: 6, borderRadius: 3 }}
                  />
                </CardContent>
              </Card>
            ))}
          </Stack>
        </>
      )}

      {/* Leaderboard */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
        Top 10 Badge Holders
      </Typography>

      {leaderboardLoading ? (
        <Skeleton variant="rounded" height={300} sx={{ borderRadius: 3 }} />
      ) : !leaderboard || leaderboard.length === 0 ? (
        <EmptyState title="No leaderboard data" message="Leaderboard will appear once members earn badges." />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell width={60}>#</TableCell>
                <TableCell>Member</TableCell>
                <TableCell align="right">Badges</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {leaderboard.map((entry, index) => (
                <TableRow key={entry.userId} hover>
                  <TableCell>
                    <Typography fontWeight={700} color={index < 3 ? 'secondary.main' : 'text.primary'}>
                      {index + 1}
                    </Typography>
                  </TableCell>
                  <TableCell>{entry.name}</TableCell>
                  <TableCell align="right">
                    <Chip label={entry.badgeCount} color="primary" size="small" />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

function BadgeCard({ badge, earned }: { badge: Badge; earned: boolean }) {
  return (
    <Card sx={{ width: 160, textAlign: 'center', opacity: earned ? 1 : 0.5 }}>
      <CardContent sx={{ py: 2 }}>
        <EmojiEventsIcon sx={{ fontSize: 32, color: earned ? 'secondary.main' : 'action.disabled' }} />
        <Typography variant="subtitle2" fontWeight={600} sx={{ mt: 1 }}>
          {badge.name}
        </Typography>
        {earned && badge.earnedAt && (
          <Typography variant="caption" color="text.disabled">
            {new Date(badge.earnedAt).toLocaleDateString()}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}
