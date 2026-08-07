import { Stack } from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import StarBorderIcon from '@mui/icons-material/StarBorder';
import StarHalfIcon from '@mui/icons-material/StarHalf';

interface StarDisplayProps {
  score: number;
  size?: 'small' | 'medium';
}

export function StarDisplay({ score, size = 'medium' }: StarDisplayProps) {
  const fontSize = size === 'small' ? 16 : 20;
  const stars = [];

  for (let i = 1; i <= 5; i++) {
    if (score >= i) {
      stars.push(<StarIcon key={i} sx={{ fontSize, color: 'secondary.main' }} />);
    } else if (score >= i - 0.5) {
      stars.push(<StarHalfIcon key={i} sx={{ fontSize, color: 'secondary.main' }} />);
    } else {
      stars.push(<StarBorderIcon key={i} sx={{ fontSize, color: 'action.disabled' }} />);
    }
  }

  return <Stack direction="row">{stars}</Stack>;
}
