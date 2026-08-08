import {
  Box,
  Card,
  CardContent,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbUpOutlinedIcon from '@mui/icons-material/ThumbUpOutlined';
import DeleteIcon from '@mui/icons-material/Delete';
import { StarDisplay } from './StarDisplay';

interface ReviewCardProps {
  bookTitle: string;
  bookAuthor?: string;
  userName?: string;
  score: number;
  reviewText: string;
  usefulVotes: number;
  createdAt: string;
  onBookClick?: () => void;
  onDelete?: () => void;
  onVoteUseful?: () => void;
  isDeletePending?: boolean;
  isVotePending?: boolean;
  showUserName?: boolean;
}

export function ReviewCard({
  bookTitle,
  bookAuthor,
  userName,
  score,
  reviewText,
  usefulVotes,
  createdAt,
  onBookClick,
  onDelete,
  onVoteUseful,
  isDeletePending,
  isVotePending,
  showUserName = false,
}: ReviewCardProps) {
  return (
    <Card sx={{ '&:hover': { boxShadow: 3 } }}>
      <CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
        <Stack spacing={1}>
          <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
            <Box sx={{ flex: 1 }}>
              <Typography
                variant="subtitle2"
                fontWeight={600}
                sx={{
                  cursor: onBookClick ? 'pointer' : 'default',
                  '&:hover': onBookClick ? { color: 'primary.main' } : undefined,
                }}
                onClick={onBookClick}
              >
                {bookTitle}
              </Typography>
              {bookAuthor && (
                <Typography variant="body2" color="text.secondary">
                  {bookAuthor}
                </Typography>
              )}
              {showUserName && userName && (
                <Typography variant="caption" color="text.secondary">
                  by {userName}
                </Typography>
              )}
            </Box>
            <Stack direction="row" spacing={0.5} alignItems="center">
              <StarDisplay score={score} size="small" />
              <Typography variant="caption" color="text.disabled" sx={{ ml: 1 }}>
                {new Date(createdAt).toLocaleDateString()}
              </Typography>
            </Stack>
          </Stack>

          {reviewText && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {reviewText.length > 200 ? `${reviewText.slice(0, 200)}...` : reviewText}
            </Typography>
          )}

          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Stack direction="row" spacing={1} alignItems="center">
              {onVoteUseful && (
                <Tooltip title="Mark as useful">
                  <IconButton
                    size="small"
                    onClick={onVoteUseful}
                    disabled={isVotePending}
                    aria-label="mark review as useful"
                  >
                    {usefulVotes > 0 ? (
                      <ThumbUpIcon fontSize="small" color="primary" />
                    ) : (
                      <ThumbUpOutlinedIcon fontSize="small" />
                    )}
                  </IconButton>
                </Tooltip>
              )}
              {usefulVotes > 0 && (
                <Typography variant="caption" color="text.secondary">
                  {usefulVotes} {usefulVotes === 1 ? 'person' : 'people'} found this useful
                </Typography>
              )}
            </Stack>
            {onDelete && (
              <Tooltip title="Delete review">
                <IconButton
                  size="small"
                  color="error"
                  onClick={onDelete}
                  disabled={isDeletePending}
                  aria-label="delete review"
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  );
}
