# API Documentation

The API is documented via Swagger/OpenAPI and available at:
- **Development**: http://localhost:5000/swagger
- **JSON spec**: http://localhost:5000/swagger/v1/swagger.json

## Endpoints Overview

### Books
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/books | List all books (paginated) |
| GET | /api/books/{id} | Get book by ID |
| POST | /api/books | Create a new book |
| PUT | /api/books/{id} | Update a book |
| DELETE | /api/books/{id} | Delete a book |
| GET | /api/books/search?q={query} | Search books |
| POST | /api/books/{id}/checkout | Check out a book |
| POST | /api/books/{id}/checkin | Return a book |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/register | Register new user |
| POST | /api/auth/login | Login |
| POST | /api/auth/refresh | Refresh token |
| GET | /api/users/me | Get current user profile |

### AI Features
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/ai/recommendations | Get book recommendations |
| POST | /api/ai/search | AI-powered semantic search |

### Admin
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/admin/users | List all users |
| PUT | /api/admin/users/{id}/role | Update user role |
| GET | /api/admin/stats | Dashboard statistics |

## Authentication
All endpoints except `/api/auth/*` require a Bearer token.
Roles: `Admin`, `Librarian`, `Member`
