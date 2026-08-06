#!/bin/bash
set -e

# Load environment variables from .env if present
if [ -f .env ]; then
    export $(grep -v '^#' .env | xargs)
fi

SA_PASSWORD="${SA_PASSWORD:-YourStrong!Passw0rd}"
DB_CONTAINER="${DB_CONTAINER:-minilibrary-db}"
DB_NAME="${DB_NAME:-MiniLibraryDb}"

echo "Seeding MiniLibrary database with sample data..."
echo "Container: $DB_CONTAINER | Database: $DB_NAME"

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
for i in $(seq 1 30); do
    if docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "SQL Server is ready."
        break
    fi
    echo "  Attempt $i/30 - waiting..."
    sleep 2
done

# Create database if it doesn't exist
docker exec -i "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C -Q \
    "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DB_NAME') CREATE DATABASE [$DB_NAME];"

echo "Inserting seed data..."

docker exec -i "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C -d "$DB_NAME" << 'EOF'
-- =============================================================================
-- MiniLibrary Seed Data
-- Idempotent: safe to run multiple times
-- =============================================================================

SET NOCOUNT ON;

-- ---------------------------------------------------------------------------
-- Users (Admin, Librarian, Members)
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@minilibrary.com')
BEGIN
    INSERT INTO Users (Id, Email, FullName, ExternalId, Provider, Role, IsDeleted, EmailAlertsExpiration, EmailAlertsAvailability, CreatedAt, UpdatedAt)
    VALUES
        ('10000000-0000-0000-0000-000000000001', 'admin@minilibrary.com', 'System Administrator', 'admin-external-001', 'Google', 'Admin', 0, 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('10000000-0000-0000-0000-000000000002', 'librarian@minilibrary.com', 'Maria Garcia', 'librarian-external-001', 'Google', 'Librarian', 0, 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('10000000-0000-0000-0000-000000000003', 'carlos@example.com', 'Carlos Rodriguez', 'member-external-001', 'Microsoft', 'Member', 0, 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('10000000-0000-0000-0000-000000000004', 'ana@example.com', 'Ana Martinez', 'member-external-002', 'Google', 'Member', 0, 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('10000000-0000-0000-0000-000000000005', 'luis@example.com', 'Luis Fernandez', 'member-external-003', 'Microsoft', 'Member', 0, 1, 0, GETUTCDATE(), GETUTCDATE());

    PRINT 'Users seeded successfully.';
END
ELSE
    PRINT 'Users already exist, skipping.';

-- ---------------------------------------------------------------------------
-- Books (varied categories)
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780132350884')
BEGIN
    INSERT INTO Books (Id, Title, Author, ISBN, PublishedYear, Description, Category, Status, AverageRating, TotalRatings, IsDeleted, CreatedAt, UpdatedAt)
    VALUES
        -- Technology
        ('20000000-0000-0000-0000-000000000001', 'Clean Code', 'Robert C. Martin', '9780132350884', 2008,
         'A Handbook of Agile Software Craftsmanship. Even bad code can function. But if code isnt clean, it can bring a development organization to its knees.',
         'Technology', 'Available', 4.5, 12, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000002', 'The Pragmatic Programmer', 'David Thomas, Andrew Hunt', '9780135957059', 2019,
         'Your journey to mastery. Straight from the trenches, The Pragmatic Programmer cuts through the increasing specialization and technicalities of modern software.',
         'Technology', 'Available', 4.7, 8, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000003', 'Design Patterns', 'Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides', '9780201633610', 1994,
         'Elements of Reusable Object-Oriented Software. Capturing a wealth of experience about the design of object-oriented software.',
         'Technology', 'Available', 4.3, 15, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000004', 'Refactoring', 'Martin Fowler', '9780134757599', 2018,
         'Improving the Design of Existing Code. For more than twenty years, experienced programmers worldwide have relied on this book.',
         'Technology', 'CheckedOut', 4.6, 6, 0, GETUTCDATE(), GETUTCDATE()),

        -- Fiction
        ('20000000-0000-0000-0000-000000000005', 'One Hundred Years of Solitude', 'Gabriel Garcia Marquez', '9780060883287', 1967,
         'The brilliant, bestselling, landmark novel that tells the story of the Buendia family and mirrors the history of Latin America.',
         'Fiction', 'Available', 4.8, 20, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000006', 'Don Quixote', 'Miguel de Cervantes', '9780060934347', 1605,
         'The classic tale of the knight-errant who sets out to right wrongs and bring justice to the world.',
         'Fiction', 'Available', 4.4, 10, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000007', 'The House of the Spirits', 'Isabel Allende', '9780553383805', 1982,
         'The story of the Trueba family spanning four generations, weaving reality with the supernatural.',
         'Fiction', 'CheckedOut', 4.2, 7, 0, GETUTCDATE(), GETUTCDATE()),

        -- Science
        ('20000000-0000-0000-0000-000000000008', 'A Brief History of Time', 'Stephen Hawking', '9780553380163', 1988,
         'From the Big Bang to Black Holes. A landmark volume in science writing for the layperson.',
         'Science', 'Available', 4.6, 18, 0, GETUTCDATE(), GETUTCDATE()),

        ('20000000-0000-0000-0000-000000000009', 'Cosmos', 'Carl Sagan', '9780345539434', 1980,
         'Cosmos retraces the fourteen billion years of cosmic evolution that have transformed matter into consciousness.',
         'Science', 'Available', 4.9, 22, 0, GETUTCDATE(), GETUTCDATE()),

        -- History
        ('20000000-0000-0000-0000-000000000010', 'Sapiens: A Brief History of Humankind', 'Yuval Noah Harari', '9780062316097', 2011,
         'A groundbreaking narrative of humanitys creation and evolution that explores how we came to believe in gods, nations, and human rights.',
         'History', 'Available', 4.5, 25, 0, GETUTCDATE(), GETUTCDATE());

    PRINT 'Books seeded successfully.';
END
ELSE
    PRINT 'Books already exist, skipping.';

-- ---------------------------------------------------------------------------
-- Book Loans (sample active and returned loans)
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM BookLoans WHERE BookId = '20000000-0000-0000-0000-000000000004')
BEGIN
    INSERT INTO BookLoans (Id, BookId, UserId, BorrowedAt, DueDate, ReturnedAt)
    VALUES
        -- Active loans (matching CheckedOut book statuses)
        ('30000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000003',
         DATEADD(DAY, -7, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), NULL),

        ('30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000004',
         DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, 4, GETUTCDATE()), NULL),

        -- Returned loans (history)
        ('30000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003',
         DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -16, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE())),

        ('30000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000004',
         DATEADD(DAY, -45, GETUTCDATE()), DATEADD(DAY, -31, GETUTCDATE()), DATEADD(DAY, -33, GETUTCDATE())),

        ('30000000-0000-0000-0000-000000000005', '20000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000005',
         DATEADD(DAY, -60, GETUTCDATE()), DATEADD(DAY, -46, GETUTCDATE()), DATEADD(DAY, -50, GETUTCDATE())),

        ('30000000-0000-0000-0000-000000000006', '20000000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000003',
         DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -6, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE()));

    PRINT 'Book loans seeded successfully.';
END
ELSE
    PRINT 'Book loans already exist, skipping.';

-- ---------------------------------------------------------------------------
-- Ratings (sample reviews)
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Ratings WHERE UserId = '10000000-0000-0000-0000-000000000003' AND BookId = '20000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO Ratings (Id, BookId, UserId, Score, ReviewText, UsefulVotes, CreatedAt, UpdatedAt)
    VALUES
        ('40000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003',
         5, 'Essential reading for any developer. Changed how I write code.', 3, GETUTCDATE(), GETUTCDATE()),

        ('40000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000004',
         5, 'A masterpiece of magical realism. Unforgettable characters and storytelling.', 5, GETUTCDATE(), GETUTCDATE()),

        ('40000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000005',
         4, 'Complex topics made accessible. A great introduction to cosmology.', 2, GETUTCDATE(), GETUTCDATE()),

        ('40000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000003',
         5, 'Sagan makes the universe feel like home. Poetic and scientific in equal measure.', 4, GETUTCDATE(), GETUTCDATE());

    PRINT 'Ratings seeded successfully.';
END
ELSE
    PRINT 'Ratings already exist, skipping.';

-- ---------------------------------------------------------------------------
-- Badges (sample achievements)
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Badges WHERE UserId = '10000000-0000-0000-0000-000000000003')
BEGIN
    INSERT INTO Badges (Id, UserId, BadgeType, EarnedAt)
    VALUES
        ('50000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003', 'PrimerPrestamo', DATEADD(DAY, -30, GETUTCDATE())),
        ('50000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000004', 'PrimerPrestamo', DATEADD(DAY, -45, GETUTCDATE())),
        ('50000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000005', 'PrimerPrestamo', DATEADD(DAY, -60, GETUTCDATE())),
        ('50000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000003', 'CriticoLiterario', DATEADD(DAY, -10, GETUTCDATE()));

    PRINT 'Badges seeded successfully.';
END
ELSE
    PRINT 'Badges already exist, skipping.';

PRINT '';
PRINT '=== MiniLibrary seed data complete! ===';
PRINT '';
GO
EOF

echo ""
echo "Database seeded successfully!"
echo ""
echo "Sample credentials (SSO-based, no passwords):"
echo "  Admin:     admin@minilibrary.com"
echo "  Librarian: librarian@minilibrary.com"
echo "  Members:   carlos@example.com, ana@example.com, luis@example.com"
