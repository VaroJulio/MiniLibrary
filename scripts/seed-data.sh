#!/bin/bash
set -e

export $(grep -v '^#' .env | xargs)

echo "🌱 Seeding database with sample data..."

docker exec -i minilibrary-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d MiniLibraryDb << 'EOF'
-- Sample Categories
INSERT INTO Categories (Id, Name, Description) VALUES
  (NEWID(), 'Fiction', 'Novels, short stories, and literary fiction'),
  (NEWID(), 'Science', 'Scientific literature and research'),
  (NEWID(), 'Technology', 'Computing, programming, and engineering'),
  (NEWID(), 'History', 'Historical accounts and analyses');

-- Sample Books
INSERT INTO Books (Id, Title, Author, ISBN, PublishedYear, Description, Status, CreatedAt) VALUES
  (NEWID(), 'Clean Code', 'Robert C. Martin', '978-0132350884', 2008, 'A handbook of agile software craftsmanship', 0, GETDATE()),
  (NEWID(), 'The Pragmatic Programmer', 'David Thomas, Andrew Hunt', '978-0135957059', 2019, 'Your journey to mastery', 0, GETDATE()),
  (NEWID(), 'Design Patterns', 'Gang of Four', '978-0201633610', 1994, 'Elements of reusable object-oriented software', 0, GETDATE()),
  (NEWID(), 'Refactoring', 'Martin Fowler', '978-0134757599', 2018, 'Improving the design of existing code', 0, GETDATE());

PRINT 'Seed data inserted successfully!'
EOF

echo "✅ Database seeded!"
