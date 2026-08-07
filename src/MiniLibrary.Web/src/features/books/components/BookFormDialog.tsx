import { useState } from 'react';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from '@mui/material';
import { useCreateBook, useUpdateBook } from '../hooks/useBooks';
import type { CreateBookRequest } from '../types';
import type { Book } from '@/types/models';

interface BookFormDialogProps {
  open: boolean;
  onClose: () => void;
  book?: Book;
}

interface FormErrors {
  title?: string;
  author?: string;
  isbn?: string;
  category?: string;
  description?: string;
  publicationYear?: string;
}

export function BookFormDialog({ open, onClose, book }: BookFormDialogProps) {
  const isEditing = !!book;
  const createMutation = useCreateBook();
  const updateMutation = useUpdateBook();
  const mutation = isEditing ? updateMutation : createMutation;

  const [form, setForm] = useState<CreateBookRequest>({
    title: book?.title ?? '',
    author: book?.author ?? '',
    isbn: book?.isbn ?? '',
    category: book?.category ?? '',
    description: book?.description ?? '',
    publicationYear: book?.publicationYear ?? new Date().getFullYear(),
  });
  const [errors, setErrors] = useState<FormErrors>({});

  const validate = (): boolean => {
    const newErrors: FormErrors = {};
    if (!form.title.trim() || form.title.length > 255) {
      newErrors.title = 'Title is required (1–255 characters)';
    }
    if (!form.author.trim() || form.author.length > 200) {
      newErrors.author = 'Author is required (1–200 characters)';
    }
    if (!/^\d{13}$/.test(form.isbn)) {
      newErrors.isbn = 'ISBN must be exactly 13 digits';
    }
    if (!form.category.trim() || form.category.length > 100) {
      newErrors.category = 'Category is required (1–100 characters)';
    }
    if (form.description.length > 2000) {
      newErrors.description = 'Description must be at most 2000 characters';
    }
    const currentYear = new Date().getFullYear();
    if (form.publicationYear < 1450 || form.publicationYear > currentYear) {
      newErrors.publicationYear = `Year must be between 1450 and ${currentYear}`;
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = () => {
    if (!validate()) return;

    if (isEditing && book) {
      updateMutation.mutate(
        { ...form, id: book.id },
        { onSuccess: () => onClose() },
      );
    } else {
      createMutation.mutate(form, { onSuccess: () => onClose() });
    }
  };

  const handleChange = (field: keyof CreateBookRequest, value: string | number) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEditing ? 'Edit Book' : 'Add New Book'}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {mutation.isError && (
            <Alert severity="error">
              {(mutation.error as Error)?.message ?? 'An error occurred. Please try again.'}
            </Alert>
          )}
          <TextField
            label="Title"
            value={form.title}
            onChange={(e) => handleChange('title', e.target.value)}
            error={!!errors.title}
            helperText={errors.title}
            required
            fullWidth
          />
          <TextField
            label="Author"
            value={form.author}
            onChange={(e) => handleChange('author', e.target.value)}
            error={!!errors.author}
            helperText={errors.author}
            required
            fullWidth
          />
          <TextField
            label="ISBN-13"
            value={form.isbn}
            onChange={(e) => handleChange('isbn', e.target.value)}
            error={!!errors.isbn}
            helperText={errors.isbn}
            required
            fullWidth
            inputProps={{ maxLength: 13 }}
          />
          <TextField
            label="Category"
            value={form.category}
            onChange={(e) => handleChange('category', e.target.value)}
            error={!!errors.category}
            helperText={errors.category}
            required
            fullWidth
          />
          <TextField
            label="Publication Year"
            type="number"
            value={form.publicationYear}
            onChange={(e) => handleChange('publicationYear', parseInt(e.target.value) || 0)}
            error={!!errors.publicationYear}
            helperText={errors.publicationYear}
            required
            fullWidth
          />
          <TextField
            label="Description"
            value={form.description}
            onChange={(e) => handleChange('description', e.target.value)}
            error={!!errors.description}
            helperText={errors.description ?? `${form.description.length}/2000`}
            multiline
            rows={4}
            fullWidth
          />
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} disabled={mutation.isPending}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={mutation.isPending}
        >
          {mutation.isPending ? 'Saving...' : isEditing ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
