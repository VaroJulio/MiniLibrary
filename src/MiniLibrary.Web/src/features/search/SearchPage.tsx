import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Card,
  CardContent,
  Chip,
  LinearProgress,
  MenuItem,
  Pagination,
  Paper,
  Skeleton,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import { SearchBar } from './components/SearchBar';
import { useTextSearch, useSemanticSearch } from './hooks/useSearch';
import { EmptyState } from '@/components/EmptyState';
import type { SearchFilters } from './api/searchApi';
import type { Book } from '@/types/models';

const PAGE_SIZE = 20;

const STATUS_OPTIONS = [
  { value: '', label: 'All Status' },
  { value: 'Available', label: 'Available' },
  { value: 'CheckedOut', label: 'Checked Out' },
];

export default function SearchPage() {
  const navigate = useNavigate();
  const [tab, setTab] = useState(0);

  // Text search state
  const [searchQuery, setSearchQuery] = useState('');
  const [activeQuery, setActiveQuery] = useState('');
  const [filters, setFilters] = useState<SearchFilters>({});
  const [page, setPage] = useState(1);

  // Semantic search state
  const [semanticQuery, setSemanticQuery] = useState('');
  const [activeSemanticQuery, setActiveSemanticQuery] = useState('');

  const textSearch = useTextSearch(page, PAGE_SIZE, { ...filters, query: activeQuery || undefined });
  const semanticSearch = useSemanticSearch(activeSemanticQuery);

  const handleTextSearch = (value: string) => {
    setActiveQuery(value);
    setPage(1);
  };

  const handleSemanticSearch = (value: string) => {
    setActiveSemanticQuery(value);
  };

  const handleFilterChange = (key: keyof SearchFilters, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value || undefined }));
    setPage(1);
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" fontWeight={700} sx={{ mb: 3 }}>
        Search Books
      </Typography>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="Text Search" />
        <Tab label="AI Search" icon={<AutoAwesomeIcon />} iconPosition="start" />
      </Tabs>

      {/* Text Search Tab */}
      {tab === 0 && (
        <Box>
          <Stack spacing={2} sx={{ mb: 3 }}>
            <SearchBar
              value={searchQuery}
              onChange={setSearchQuery}
              onSearch={handleTextSearch}
              placeholder="Search by title, author, ISBN, or category..."
            />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                select
                label="Status"
                value={filters.status ?? ''}
                onChange={(e) => handleFilterChange('status', e.target.value)}
                sx={{ minWidth: 140 }}
              >
                {STATUS_OPTIONS.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                label="Category"
                value={filters.category ?? ''}
                onChange={(e) => handleFilterChange('category', e.target.value)}
                placeholder="e.g. Fiction"
                sx={{ minWidth: 140 }}
              />
              <TextField
                label="Year From"
                type="number"
                value={filters.yearFrom ?? ''}
                onChange={(e) => handleFilterChange('yearFrom', e.target.value)}
                sx={{ minWidth: 120 }}
              />
              <TextField
                label="Year To"
                type="number"
                value={filters.yearTo ?? ''}
                onChange={(e) => handleFilterChange('yearTo', e.target.value)}
                sx={{ minWidth: 120 }}
              />
            </Stack>
          </Stack>

          {textSearch.isLoading && (
            <Stack spacing={1}>
              {Array.from({ length: 5 }, (_, i) => (
                <Skeleton key={i} variant="rounded" height={72} sx={{ borderRadius: 2 }} />
              ))}
            </Stack>
          )}

          {textSearch.data && textSearch.data.data.length === 0 && (
            <EmptyState title="No results" message="Try a different search or adjust your filters." />
          )}

          {textSearch.data && textSearch.data.data.length > 0 && (
            <>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {textSearch.data.pagination.totalCount} result{textSearch.data.pagination.totalCount !== 1 ? 's' : ''} found
              </Typography>
              <Stack spacing={1}>
                {textSearch.data.data.map((book) => (
                  <SearchResultCard
                    key={book.id}
                    book={book}
                    onClick={() => navigate(`/books/${book.id}`)}
                  />
                ))}
              </Stack>
              {textSearch.data.pagination.totalPages > 1 && (
                <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
                  <Pagination
                    count={textSearch.data.pagination.totalPages}
                    page={page}
                    onChange={(_, p) => setPage(p)}
                    color="primary"
                    shape="rounded"
                  />
                </Box>
              )}
            </>
          )}

          {!activeQuery && !filters.category && !filters.status && (
            <EmptyState title="Start searching" message="Enter a query or apply filters to find books." />
          )}
        </Box>
      )}

      {/* Semantic Search Tab */}
      {tab === 1 && (
        <Box>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Describe what you're looking for in natural language. Our AI will find the most relevant books.
            </Typography>
            <SearchBar
              value={semanticQuery}
              onChange={setSemanticQuery}
              onSearch={handleSemanticSearch}
              placeholder="e.g. A mystery novel set in Victorian London with a female detective..."
              debounceMs={800}
            />
          </Paper>

          {semanticSearch.isLoading && <LinearProgress sx={{ mb: 2 }} />}

          {semanticSearch.data?.usedFallback && (
            <Alert severity="info" sx={{ mb: 2 }}>
              AI search is temporarily unavailable. Showing text search results instead.
            </Alert>
          )}

          {semanticSearch.data && semanticSearch.data.results.length === 0 && (
            <EmptyState title="No matches" message="Try describing what you're looking for differently." />
          )}

          {semanticSearch.data && semanticSearch.data.results.length > 0 && (
            <Stack spacing={1}>
              {semanticSearch.data.results.map((result) => (
                <Card
                  key={result.id}
                  sx={{ cursor: 'pointer', '&:hover': { boxShadow: 3 } }}
                  onClick={() => navigate(`/books/${result.id}`)}
                >
                  <CardContent>
                    <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="subtitle1" fontWeight={600}>
                          {result.title}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {result.author} · {result.category} · {result.publicationYear}
                        </Typography>
                        {result.description && (
                          <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ mt: 0.5, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 500 }}
                          >
                            {result.description}
                          </Typography>
                        )}
                      </Box>
                      <Chip
                        label={`${Math.round(result.relevanceScore * 100)}% match`}
                        color="primary"
                        size="small"
                        variant="outlined"
                        sx={{ ml: 2 }}
                      />
                    </Stack>
                  </CardContent>
                </Card>
              ))}
            </Stack>
          )}

          {!activeSemanticQuery && (
            <EmptyState
              title="AI-powered search"
              message="Describe what kind of book you're looking for and we'll find the best matches."
            />
          )}
        </Box>
      )}
    </Box>
  );
}

function SearchResultCard({ book, onClick }: { book: Book; onClick: () => void }) {
  return (
    <Card
      sx={{ cursor: 'pointer', '&:hover': { boxShadow: 3 } }}
      onClick={onClick}
    >
      <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography variant="subtitle2" fontWeight={600}>
              {book.title}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {book.author} · {book.category} · {book.publicationYear}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <Chip
              label={book.status === 'Available' ? 'Available' : 'Checked Out'}
              color={book.status === 'Available' ? 'success' : 'default'}
              size="small"
            />
            {book.totalRatings > 0 && (
              <Chip
                label={`★ ${book.averageRating.toFixed(1)}`}
                size="small"
                variant="outlined"
              />
            )}
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  );
}
