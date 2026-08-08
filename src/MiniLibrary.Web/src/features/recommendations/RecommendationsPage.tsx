import {
  Box,
  Card,
  CardContent,
  Chip,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import { useRecommendations } from './hooks/useRecommendations';
import { EmptyState } from '@/components/EmptyState';
import type { Recommendation } from '@/types/models';

export default function RecommendationsPage() {
  const { data: recommendations, isLoading } = useRecommendations();

  return (
    <Box>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 3 }}>
        <AutoAwesomeIcon color="secondary" />
        <Typography variant="h4" component="h1" fontWeight={700}>
          Recommended for You
        </Typography>
      </Stack>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Personalized suggestions based on your reading history and preferences.
      </Typography>

      {isLoading ? (
        <Stack spacing={2}>
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} variant="rounded" height={120} sx={{ borderRadius: 3 }} />
          ))}
        </Stack>
      ) : !recommendations || recommendations.length === 0 ? (
        <EmptyState
          title="No recommendations yet"
          message="Read more books to get personalized recommendations. We need at least 3 completed loans to generate suggestions."
        />
      ) : (
        <Stack spacing={2}>
          {recommendations.map((rec, index) => (
            <RecommendationCard key={index} recommendation={rec} />
          ))}
        </Stack>
      )}
    </Box>
  );
}

function RecommendationCard({ recommendation }: { recommendation: Recommendation }) {
  return (
    <Card sx={{ '&:hover': { boxShadow: 3 } }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" fontWeight={600}>
              {recommendation.title}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              by {recommendation.author}
            </Typography>
            <Chip
              label={recommendation.category}
              size="small"
              variant="outlined"
              sx={{ mt: 1 }}
            />
          </Box>
          <AutoAwesomeIcon sx={{ color: 'secondary.main', ml: 2, mt: 0.5 }} fontSize="small" />
        </Stack>
        <Typography variant="body2" sx={{ mt: 1.5, fontStyle: 'italic' }} color="text.secondary">
          &ldquo;{recommendation.justification}&rdquo;
        </Typography>
      </CardContent>
    </Card>
  );
}
