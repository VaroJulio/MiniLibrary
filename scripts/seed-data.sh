#!/bin/bash
set -e

# =============================================================================
# MiniLibrary Seed Data Script
# Populates the database with sample data via API calls.
# Works against both local (docker-compose) and Azure deployments.
#
# Usage:
#   LOCAL:  ./scripts/seed-data.sh
#   AZURE:  API_BASE_URL=https://minilibrary-api.icymoss-654fea4b.centralus.azurecontainerapps.io ./scripts/seed-data.sh
# =============================================================================

API_BASE_URL="${API_BASE_URL:-http://localhost:5000}"

echo "╔══════════════════════════════════════════════╗"
echo "║  MiniLibrary - Seed Data                    ║"
echo "╠══════════════════════════════════════════════╣"
echo "║  API: $API_BASE_URL"
echo "╚══════════════════════════════════════════════╝"
echo ""

# Check API is reachable
echo "🔍 Checking API health..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/health" 2>/dev/null || echo "000")
if [ "$HTTP_CODE" != "200" ]; then
    echo "❌ API not reachable at $API_BASE_URL/health (HTTP $HTTP_CODE)"
    echo "   Make sure the API is running."
    exit 1
fi
echo "✅ API is healthy"
echo ""

# =============================================================================
# Helper functions
# =============================================================================

get_token() {
    local role=$1
    local name=$2
    local email=$3
    curl -s -X POST "$API_BASE_URL/api/auth/dev-token" \
        -H "Content-Type: application/json" \
        -d "{\"role\":\"$role\",\"name\":\"$name\",\"email\":\"$email\"}"
}

create_book() {
    local token=$1
    local title=$2
    local author=$3
    local isbn=$4
    local year=$5
    local desc=$6
    local category=$7
    
    curl -s -X POST "$API_BASE_URL/api/books" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "{\"title\":\"$title\",\"author\":\"$author\",\"isbn\":\"$isbn\",\"publishedYear\":$year,\"description\":\"$desc\",\"category\":\"$category\"}"
}

checkout_book() {
    local token=$1
    local book_id=$2
    
    curl -s -X POST "$API_BASE_URL/api/loans/checkout" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "{\"bookId\":\"$book_id\"}"
}

checkin_book() {
    local token=$1
    local book_id=$2
    
    curl -s -X POST "$API_BASE_URL/api/loans/checkin" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "{\"bookId\":\"$book_id\"}"
}

rate_book() {
    local token=$1
    local book_id=$2
    local score=$3
    local review=$4
    
    curl -s -X POST "$API_BASE_URL/api/books/$book_id/ratings" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $token" \
        -d "{\"score\":$score,\"reviewText\":\"$review\"}"
}

# =============================================================================
# 1. Create Users (via dev-token — auto-creates users)
# =============================================================================

echo "👤 Creating users..."

ADMIN_RESPONSE=$(get_token "Admin" "Ana Martinez" "ana.martinez@minilibrary.demo")
ADMIN_TOKEN=$(echo "$ADMIN_RESPONSE" | jq -r '.accessToken')

LIBRARIAN_RESPONSE=$(get_token "Librarian" "Carlos Lopez" "carlos.lopez@minilibrary.demo")
LIBRARIAN_TOKEN=$(echo "$LIBRARIAN_RESPONSE" | jq -r '.accessToken')

MEMBER1_RESPONSE=$(get_token "Member" "Maria Garcia" "maria.garcia@minilibrary.demo")
MEMBER1_TOKEN=$(echo "$MEMBER1_RESPONSE" | jq -r '.accessToken')
MEMBER1_ID=$(echo "$MEMBER1_RESPONSE" | jq -r '.user.id')

MEMBER2_RESPONSE=$(get_token "Member" "Pedro Sanchez" "pedro.sanchez@minilibrary.demo")
MEMBER2_TOKEN=$(echo "$MEMBER2_RESPONSE" | jq -r '.accessToken')
MEMBER2_ID=$(echo "$MEMBER2_RESPONSE" | jq -r '.user.id')

MEMBER3_RESPONSE=$(get_token "Member" "Laura Fernandez" "laura.fernandez@minilibrary.demo")
MEMBER3_TOKEN=$(echo "$MEMBER3_RESPONSE" | jq -r '.accessToken')
MEMBER3_ID=$(echo "$MEMBER3_RESPONSE" | jq -r '.user.id')

MEMBER4_RESPONSE=$(get_token "Member" "Diego Torres" "diego.torres@minilibrary.demo")
MEMBER4_TOKEN=$(echo "$MEMBER4_RESPONSE" | jq -r '.accessToken')
MEMBER4_ID=$(echo "$MEMBER4_RESPONSE" | jq -r '.user.id')

if [ "$ADMIN_TOKEN" == "null" ] || [ -z "$ADMIN_TOKEN" ]; then
    echo "❌ Failed to create users. Dev tokens might be disabled."
    echo "   Response: $ADMIN_RESPONSE"
    exit 1
fi
echo "✅ 6 users created (1 Admin, 1 Librarian, 4 Members)"

# =============================================================================
# 2. Create Books (25 books across categories)
# =============================================================================

echo "📚 Creating books..."

# Fiction
B1=$(create_book "$LIBRARIAN_TOKEN" "Cien Anos de Soledad" "Gabriel Garcia Marquez" "9780307474728" 1967 "The epic tale of the Buendia family in the mythical town of Macondo. A masterpiece of magical realism." "Fiction")
B1_ID=$(echo "$B1" | jq -r '.id')

B2=$(create_book "$LIBRARIAN_TOKEN" "Don Quijote de la Mancha" "Miguel de Cervantes" "9788420412146" 1605 "The adventures of a nobleman who loses his sanity and decides to become a knight-errant." "Fiction")
B2_ID=$(echo "$B2" | jq -r '.id')

B3=$(create_book "$LIBRARIAN_TOKEN" "El Amor en los Tiempos del Colera" "Gabriel Garcia Marquez" "9780307389732" 1985 "A love story that spans over fifty years, exploring the nature of love and aging." "Fiction")
B3_ID=$(echo "$B3" | jq -r '.id')

B4=$(create_book "$LIBRARIAN_TOKEN" "La Sombra del Viento" "Carlos Ruiz Zafon" "9780143034902" 2001 "A young boy discovers a mysterious book that leads him into a labyrinth of secrets in post-war Barcelona." "Fiction")
B4_ID=$(echo "$B4" | jq -r '.id')

B5=$(create_book "$LIBRARIAN_TOKEN" "Rayuela" "Julio Cortazar" "9788437604572" 1963 "An experimental novel that can be read in multiple orders, following Horacio Oliveira in Paris and Buenos Aires." "Fiction")
B5_ID=$(echo "$B5" | jq -r '.id')

# Science Fiction
B6=$(create_book "$LIBRARIAN_TOKEN" "Dune" "Frank Herbert" "9780441013593" 1965 "A science fiction epic set on the desert planet Arrakis. Politics, religion, and ecology intertwine." "Science Fiction")
B6_ID=$(echo "$B6" | jq -r '.id')

B7=$(create_book "$LIBRARIAN_TOKEN" "Foundation" "Isaac Asimov" "9780553293357" 1951 "The story of a group of scientists who seek to preserve knowledge as civilization crumbles." "Science Fiction")
B7_ID=$(echo "$B7" | jq -r '.id')

B8=$(create_book "$LIBRARIAN_TOKEN" "Neuromancer" "William Gibson" "9780441569595" 1984 "A washed-up computer hacker is hired for one last job in a dystopian future of cyberspace." "Science Fiction")
B8_ID=$(echo "$B8" | jq -r '.id')

B9=$(create_book "$LIBRARIAN_TOKEN" "The Left Hand of Darkness" "Ursula K. Le Guin" "9780441478125" 1969 "An envoy from Earth visits a planet where inhabitants can change their gender." "Science Fiction")
B9_ID=$(echo "$B9" | jq -r '.id')

# Technology
B10=$(create_book "$LIBRARIAN_TOKEN" "Clean Code" "Robert C. Martin" "9780132350884" 2008 "A handbook of agile software craftsmanship with principles for writing clean, maintainable code." "Technology")
B10_ID=$(echo "$B10" | jq -r '.id')

B11=$(create_book "$LIBRARIAN_TOKEN" "Design Patterns" "Gang of Four" "9780201633610" 1994 "Elements of reusable object-oriented software. The definitive guide to design patterns." "Technology")
B11_ID=$(echo "$B11" | jq -r '.id')

B12=$(create_book "$LIBRARIAN_TOKEN" "The Pragmatic Programmer" "David Thomas and Andrew Hunt" "9780135957059" 2019 "A guide to becoming a better programmer, from journeyman to master craftsman." "Technology")
B12_ID=$(echo "$B12" | jq -r '.id')

B13=$(create_book "$LIBRARIAN_TOKEN" "Domain-Driven Design" "Eric Evans" "9780321125217" 2003 "Tackling complexity in the heart of software through domain modeling." "Technology")
B13_ID=$(echo "$B13" | jq -r '.id')

B14=$(create_book "$LIBRARIAN_TOKEN" "Building Microservices" "Sam Newman" "9781492034025" 2021 "Designing fine-grained systems for the modern distributed architecture." "Technology")
B14_ID=$(echo "$B14" | jq -r '.id')

# History
B15=$(create_book "$LIBRARIAN_TOKEN" "Sapiens" "Yuval Noah Harari" "9780062316097" 2014 "A brief history of humankind from the Stone Age to the present." "History")
B15_ID=$(echo "$B15" | jq -r '.id')

B16=$(create_book "$LIBRARIAN_TOKEN" "Guns, Germs, and Steel" "Jared Diamond" "9780393354324" 1997 "Why some societies dominate others: the fates of human societies explained." "History")
B16_ID=$(echo "$B16" | jq -r '.id')

B17=$(create_book "$LIBRARIAN_TOKEN" "A Short History of Nearly Everything" "Bill Bryson" "9780767908184" 2003 "An accessible exploration of science and the history of discovery." "History")
B17_ID=$(echo "$B17" | jq -r '.id')

# Philosophy
B18=$(create_book "$LIBRARIAN_TOKEN" "Meditations" "Marcus Aurelius" "9780140449334" 180 "The personal writings of the Roman Emperor on Stoic philosophy." "Philosophy")
B18_ID=$(echo "$B18" | jq -r '.id')

B19=$(create_book "$LIBRARIAN_TOKEN" "The Republic" "Plato" "9780140455113" 1450 "A Socratic dialogue concerning justice, the ideal state, and the philosopher king." "Philosophy")
B19_ID=$(echo "$B19" | jq -r '.id')

# Business
B20=$(create_book "$LIBRARIAN_TOKEN" "The Lean Startup" "Eric Ries" "9780307887894" 2011 "How constant innovation creates radically successful businesses." "Business")
B20_ID=$(echo "$B20" | jq -r '.id')

B21=$(create_book "$LIBRARIAN_TOKEN" "Zero to One" "Peter Thiel" "9780804139298" 2014 "Notes on startups, or how to build the future through creating something new." "Business")
B21_ID=$(echo "$B21" | jq -r '.id')

# Science
B22=$(create_book "$LIBRARIAN_TOKEN" "A Brief History of Time" "Stephen Hawking" "9780553380163" 1988 "From the Big Bang to black holes: an exploration of the universe for non-scientists." "Science")
B22_ID=$(echo "$B22" | jq -r '.id')

B23=$(create_book "$LIBRARIAN_TOKEN" "The Selfish Gene" "Richard Dawkins" "9780198788607" 1976 "A gene-centered view of evolution that revolutionized biology." "Science")
B23_ID=$(echo "$B23" | jq -r '.id')

# Poetry
B24=$(create_book "$LIBRARIAN_TOKEN" "Veinte Poemas de Amor" "Pablo Neruda" "9788497593601" 1924 "Twenty love poems and a song of despair. A classic of Latin American poetry." "Poetry")
B24_ID=$(echo "$B24" | jq -r '.id')

B25=$(create_book "$LIBRARIAN_TOKEN" "Leaves of Grass" "Walt Whitman" "9780486456768" 1855 "A poetry collection celebrating the human body, nature, and democracy." "Poetry")
B25_ID=$(echo "$B25" | jq -r '.id')

# Count successes
BOOK_COUNT=$(echo "$B1 $B2 $B3 $B4 $B5 $B6 $B7 $B8 $B9 $B10 $B11 $B12 $B13 $B14 $B15 $B16 $B17 $B18 $B19 $B20 $B21 $B22 $B23 $B24 $B25" | grep -o '"id"' | wc -l)
echo "✅ $BOOK_COUNT books created across 8 categories"

# =============================================================================
# 3. Create Loans (check-out and return books to build history)
# =============================================================================

echo "📖 Creating loan history..."

# Member 1 (Maria) - borrows and returns several books
checkout_book "$MEMBER1_TOKEN" "$B1_ID" > /dev/null 2>&1
checkin_book "$MEMBER1_TOKEN" "$B1_ID" > /dev/null 2>&1

checkout_book "$MEMBER1_TOKEN" "$B3_ID" > /dev/null 2>&1
checkin_book "$MEMBER1_TOKEN" "$B3_ID" > /dev/null 2>&1

checkout_book "$MEMBER1_TOKEN" "$B10_ID" > /dev/null 2>&1
checkin_book "$MEMBER1_TOKEN" "$B10_ID" > /dev/null 2>&1

checkout_book "$MEMBER1_TOKEN" "$B15_ID" > /dev/null 2>&1
checkin_book "$MEMBER1_TOKEN" "$B15_ID" > /dev/null 2>&1

checkout_book "$MEMBER1_TOKEN" "$B6_ID" > /dev/null 2>&1
checkin_book "$MEMBER1_TOKEN" "$B6_ID" > /dev/null 2>&1

# Member 2 (Pedro) - borrows and returns
checkout_book "$MEMBER2_TOKEN" "$B6_ID" > /dev/null 2>&1
checkin_book "$MEMBER2_TOKEN" "$B6_ID" > /dev/null 2>&1

checkout_book "$MEMBER2_TOKEN" "$B7_ID" > /dev/null 2>&1
checkin_book "$MEMBER2_TOKEN" "$B7_ID" > /dev/null 2>&1

checkout_book "$MEMBER2_TOKEN" "$B8_ID" > /dev/null 2>&1
checkin_book "$MEMBER2_TOKEN" "$B8_ID" > /dev/null 2>&1

checkout_book "$MEMBER2_TOKEN" "$B10_ID" > /dev/null 2>&1
checkin_book "$MEMBER2_TOKEN" "$B10_ID" > /dev/null 2>&1

# Member 3 (Laura) - borrows and returns
checkout_book "$MEMBER3_TOKEN" "$B1_ID" > /dev/null 2>&1
checkin_book "$MEMBER3_TOKEN" "$B1_ID" > /dev/null 2>&1

checkout_book "$MEMBER3_TOKEN" "$B4_ID" > /dev/null 2>&1
checkin_book "$MEMBER3_TOKEN" "$B4_ID" > /dev/null 2>&1

checkout_book "$MEMBER3_TOKEN" "$B5_ID" > /dev/null 2>&1
checkin_book "$MEMBER3_TOKEN" "$B5_ID" > /dev/null 2>&1

checkout_book "$MEMBER3_TOKEN" "$B15_ID" > /dev/null 2>&1
checkin_book "$MEMBER3_TOKEN" "$B15_ID" > /dev/null 2>&1

# Member 4 (Diego) - some active loans (not returned)
checkout_book "$MEMBER4_TOKEN" "$B12_ID" > /dev/null 2>&1
checkin_book "$MEMBER4_TOKEN" "$B12_ID" > /dev/null 2>&1

checkout_book "$MEMBER4_TOKEN" "$B22_ID" > /dev/null 2>&1
checkin_book "$MEMBER4_TOKEN" "$B22_ID" > /dev/null 2>&1

checkout_book "$MEMBER4_TOKEN" "$B20_ID" > /dev/null 2>&1
checkin_book "$MEMBER4_TOKEN" "$B20_ID" > /dev/null 2>&1

# Active loans (currently checked out)
checkout_book "$MEMBER1_TOKEN" "$B22_ID" > /dev/null 2>&1
checkout_book "$MEMBER2_TOKEN" "$B13_ID" > /dev/null 2>&1
checkout_book "$MEMBER4_TOKEN" "$B6_ID" > /dev/null 2>&1

echo "✅ Loan history created (16 returned + 3 active loans)"

# =============================================================================
# 4. Create Ratings and Reviews
# =============================================================================

echo "⭐ Creating ratings and reviews..."

# Maria's ratings
rate_book "$MEMBER1_TOKEN" "$B1_ID" 5 "An absolute masterpiece. Garcia Marquez weaves a world that feels both fantastical and deeply real." > /dev/null 2>&1
rate_book "$MEMBER1_TOKEN" "$B3_ID" 4 "Beautiful prose and a touching love story that spans decades." > /dev/null 2>&1
rate_book "$MEMBER1_TOKEN" "$B10_ID" 5 "Essential reading for any developer. Changed how I think about code quality." > /dev/null 2>&1
rate_book "$MEMBER1_TOKEN" "$B15_ID" 4 "Fascinating perspective on human history, though sometimes oversimplifies." > /dev/null 2>&1
rate_book "$MEMBER1_TOKEN" "$B6_ID" 5 "Epic worldbuilding. Herbert created an entire universe that feels alive." > /dev/null 2>&1

# Pedro's ratings
rate_book "$MEMBER2_TOKEN" "$B6_ID" 5 "One of the greatest science fiction novels ever written. The politics are fascinating." > /dev/null 2>&1
rate_book "$MEMBER2_TOKEN" "$B7_ID" 4 "Asimov at his best. The concept of psychohistory is brilliant." > /dev/null 2>&1
rate_book "$MEMBER2_TOKEN" "$B8_ID" 4 "Groundbreaking cyberpunk. Gibson predicted so much of our digital world." > /dev/null 2>&1
rate_book "$MEMBER2_TOKEN" "$B10_ID" 5 "Should be required reading in every CS program. Practical and insightful." > /dev/null 2>&1

# Laura's ratings
rate_book "$MEMBER3_TOKEN" "$B1_ID" 5 "I have read this three times and discover something new each time. Magical." > /dev/null 2>&1
rate_book "$MEMBER3_TOKEN" "$B4_ID" 5 "Could not put it down. The atmosphere of Barcelona is captivating." > /dev/null 2>&1
rate_book "$MEMBER3_TOKEN" "$B5_ID" 3 "Interesting experiment but the fragmented structure was frustrating at times." > /dev/null 2>&1
rate_book "$MEMBER3_TOKEN" "$B15_ID" 5 "Changed my worldview. Harari makes complex ideas accessible and engaging." > /dev/null 2>&1

# Diego's ratings
rate_book "$MEMBER4_TOKEN" "$B12_ID" 5 "Practical wisdom for programmers at every level. The updated edition is excellent." > /dev/null 2>&1
rate_book "$MEMBER4_TOKEN" "$B22_ID" 4 "Hawking makes complex physics understandable. A must-read for the curious mind." > /dev/null 2>&1
rate_book "$MEMBER4_TOKEN" "$B20_ID" 4 "Great framework for thinking about innovation, though some examples feel dated now." > /dev/null 2>&1

echo "✅ 16 ratings with reviews created"

# =============================================================================
# Done!
# =============================================================================

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║  ✅ Seed Data Complete!                     ║"
echo "╠══════════════════════════════════════════════╣"
echo "║  Users:    6 (Admin, Librarian, 4 Members)  ║"
echo "║  Books:    25 (8 categories)                ║"
echo "║  Loans:    19 (16 returned, 3 active)       ║"
echo "║  Ratings:  16 (with reviews)                ║"
echo "╚══════════════════════════════════════════════╝"
echo ""
echo "📝 You can now explore the API:"
echo "   Swagger: $API_BASE_URL/swagger (if dev mode)"
echo "   Search:  curl -H 'Authorization: Bearer <token>' $API_BASE_URL/api/search/books"
echo "   Books:   curl -H 'Authorization: Bearer <token>' $API_BASE_URL/api/search/books?query=dune"
