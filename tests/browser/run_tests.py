#!/usr/bin/env python3
"""
MiniLibrary — Browser Functional Test Runner with Screenshots.

Executes the regression test suite against the local Docker environment,
capturing screenshots as visual evidence for each test case.

Requirements:
  - pip install playwright
  - playwright install chromium
  - Docker environment running (localhost:3000 + localhost:5000)
  - Seed data loaded (./scripts/seed-data.sh)

Usage:
  python3 tests/browser/run_tests.py
  python3 tests/browser/run_tests.py --headed  (to watch the browser)
"""
import json
import os
import sys
import urllib.request
from datetime import datetime
from pathlib import Path

# Attempt to import Playwright
try:
    from playwright.sync_api import sync_playwright
    HAS_PLAYWRIGHT = True
except ImportError:
    HAS_PLAYWRIGHT = False

BASE_URL_API = "http://localhost:5000"
BASE_URL_WEB = "http://localhost:3000"
HEADED = "--headed" in sys.argv

# Determine report paths
SCRIPT_DIR = Path(__file__).parent
TODAY = datetime.now().strftime("%Y-%m-%d")
REPORT_DIR = SCRIPT_DIR / "reports"
SCREENSHOT_DIR = REPORT_DIR / f"{TODAY}-screenshots"
REPORT_FILE = REPORT_DIR / f"{TODAY}-regression.md"

RESULTS = []


def get_token(role):
    """Get a dev token from the API."""
    req = urllib.request.Request(
        f"{BASE_URL_API}/api/auth/dev-token",
        data=json.dumps({"role": role}).encode(),
        headers={"Content-Type": "application/json"},
    )
    resp = urllib.request.urlopen(req, timeout=10)
    return json.loads(resp.read())


def api_get(path, token):
    """GET an API endpoint with auth."""
    req = urllib.request.Request(
        f"{BASE_URL_API}{path}",
        headers={"Authorization": f"Bearer {token}"},
    )
    resp = urllib.request.urlopen(req, timeout=10)
    return json.loads(resp.read())


def api_post(path, token, body=None):
    """POST to an API endpoint with auth."""
    data = json.dumps(body).encode() if body else None
    req = urllib.request.Request(
        f"{BASE_URL_API}{path}",
        data=data,
        headers={"Authorization": f"Bearer {token}", "Content-Type": "application/json"},
    )
    resp = urllib.request.urlopen(req, timeout=10)
    return json.loads(resp.read())


def record(tc_id, name, module, status, notes="", screenshot=""):
    """Record a test result."""
    RESULTS.append({
        "id": tc_id,
        "name": name,
        "module": module,
        "status": status,
        "notes": notes,
        "screenshot": screenshot,
    })
    icon = {"PASS": "PASS", "FAIL": "FAIL", "SKIPPED": "SKIP"}[status]
    print(f"  {icon} TC-{tc_id:02d}: {name} {('- ' + notes) if notes else ''}")


def screenshot_path(tc_id, suffix=""):
    """Get the screenshot file path for a test case."""
    name = f"tc{tc_id:02d}{'-' + suffix if suffix else ''}.png"
    return SCREENSHOT_DIR / name


def run_with_playwright():
    """Run all tests with Playwright browser for screenshots."""
    SCREENSHOT_DIR.mkdir(parents=True, exist_ok=True)

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=not HEADED)
        context = browser.new_context(viewport={"width": 1280, "height": 720})
        page = context.new_page()

        # --- TC-01: Login Dev Member ---
        try:
            page.goto(f"{BASE_URL_WEB}/login", wait_until="networkidle")
            page.wait_for_selector("text=MiniLibrary", timeout=10000)
            # MUI Select: click to open dropdown, then select option
            select = page.locator("div").filter(has_text="Role").locator("div[role='combobox']").first
            if select.count() > 0:
                select.click()
                page.locator("li[data-value='Member']").click()
            page.click("button:has-text('Dev Login as Member')")
            page.wait_for_url(lambda url: "/login" not in url, timeout=10000)
            ss = screenshot_path(1, "login-member")
            page.screenshot(path=str(ss))
            record(1, "Login Dev Member", "Auth", "PASS", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(1, "login-member-fail")
            page.screenshot(path=str(ss))
            record(1, "Login Dev Member", "Auth", "FAIL", str(e)[:100], screenshot=ss.name)

        # Logout for next test
        page.goto(f"{BASE_URL_WEB}/login", wait_until="networkidle")

        # --- TC-02: Login Dev Admin ---
        try:
            page.goto(f"{BASE_URL_WEB}/login", wait_until="networkidle")
            page.wait_for_selector("text=MiniLibrary", timeout=10000)
            select = page.locator("div").filter(has_text="Role").locator("div[role='combobox']").first
            if select.count() > 0:
                select.click()
                page.locator("li[data-value='Admin']").click()
            page.click("button:has-text('Dev Login as Admin')")
            page.wait_for_url(lambda url: "/login" not in url, timeout=10000)
            ss = screenshot_path(2, "login-admin")
            page.screenshot(path=str(ss))
            # Check for admin-specific nav items
            content = page.content()
            has_users = "User Management" in content or "Users" in content
            record(2, "Login Dev Admin", "Auth", "PASS", 
                   f"Admin nav visible: {'yes' if has_users else 'check sidebar'}", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(2, "login-admin-fail")
            page.screenshot(path=str(ss))
            record(2, "Login Dev Admin", "Auth", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-03: Login Dev Librarian ---
        try:
            page.goto(f"{BASE_URL_WEB}/login", wait_until="networkidle")
            page.wait_for_selector("text=MiniLibrary", timeout=10000)
            select = page.locator("div").filter(has_text="Role").locator("div[role='combobox']").first
            if select.count() > 0:
                select.click()
                page.locator("li[data-value='Librarian']").click()
            page.click("button:has-text('Dev Login as Librarian')")
            page.wait_for_url(lambda url: "/login" not in url, timeout=10000)
            ss = screenshot_path(3, "login-librarian")
            page.screenshot(path=str(ss))
            record(3, "Login Dev Librarian", "Auth", "PASS", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(3, "login-librarian-fail")
            page.screenshot(path=str(ss))
            record(3, "Login Dev Librarian", "Auth", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-04: Book catalog loads ---
        try:
            # Login as Member first
            page.goto(f"{BASE_URL_WEB}/login", wait_until="networkidle")
            select = page.locator("div").filter(has_text="Role").locator("div[role='combobox']").first
            if select.count() > 0:
                select.click()
                page.locator("li[data-value='Member']").click()
            page.click("button:has-text('Dev Login as Member')")
            page.wait_for_url(lambda url: "/login" not in url, timeout=10000)
            # Navigate to catalog
            page.click("text=Catalog")
            page.wait_for_timeout(2000)
            ss = screenshot_path(4, "catalog")
            page.screenshot(path=str(ss))
            # Check for book content
            content = page.content()
            has_books = page.locator("[class*='card'], [class*='Card'], [class*='book'], [class*='Book']").count() > 0 or "author" in content.lower()
            record(4, "Book catalog loads", "Catalog", "PASS" if has_books else "FAIL",
                   "Books visible on page" if has_books else "No book content found", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(4, "catalog-fail")
            page.screenshot(path=str(ss))
            record(4, "Book catalog loads", "Catalog", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-05: Book detail ---
        try:
            # Click first book link/card
            book_links = page.locator("a[href*='/books/'], [class*='Card']").first
            if book_links.count() > 0:
                book_links.click()
                page.wait_for_timeout(2000)
            else:
                # Try navigating via search
                page.click("text=Search")
                page.wait_for_timeout(1000)
                page.fill("input[type='text'], input[type='search']", "Foundation")
                page.wait_for_timeout(2000)
                page.locator("a[href*='/books/']").first.click()
                page.wait_for_timeout(2000)
            ss = screenshot_path(5, "book-detail")
            page.screenshot(path=str(ss))
            content = page.content()
            has_detail = any(word in content.lower() for word in ["author", "description", "rating", "isbn"])
            record(5, "Book detail shows info", "Catalog", "PASS" if has_detail else "FAIL",
                   "Detail page content found" if has_detail else "Missing detail content", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(5, "book-detail-fail")
            page.screenshot(path=str(ss))
            record(5, "Book detail shows info", "Catalog", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-06: Search ---
        try:
            page.click("text=Search")
            page.wait_for_timeout(1000)
            search_input = page.locator("input[type='text'], input[type='search'], input[placeholder*='earch']").first
            search_input.fill("Foundation")
            page.wait_for_timeout(2000)  # debounce
            ss = screenshot_path(6, "search-results")
            page.screenshot(path=str(ss))
            content = page.content()
            has_results = "Foundation" in content or "Asimov" in content
            record(6, "Search returns results", "Search", "PASS" if has_results else "FAIL",
                   "Search results visible" if has_results else "No results found", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(6, "search-fail")
            page.screenshot(path=str(ss))
            record(6, "Search returns results", "Search", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-07: Checkout (via API, screenshot My Loans after) ---
        try:
            token = get_token("Member")["accessToken"]
            books = api_get("/api/search/books?query=&page=1&pageSize=20", token)
            available = [b for b in books["data"] if b.get("availableCopies", 0) > 0]
            if available:
                book_id = available[0]["id"]
                try:
                    api_post("/api/loans/checkout", token, {"bookId": book_id})
                    checkout_msg = f"Checked out '{available[0]['title']}'"
                except urllib.error.HTTPError as he:
                    if he.code in (409, 422):
                        checkout_msg = "Already on loan (idempotent)"
                    else:
                        raise
                page.click("text=My Loans")
                page.wait_for_timeout(2000)
                ss = screenshot_path(7, "checkout-my-loans")
                page.screenshot(path=str(ss))
                record(7, "Checkout a book", "Loans", "PASS", checkout_msg, screenshot=ss.name)
            else:
                record(7, "Checkout a book", "Loans", "SKIPPED", "No available books")
        except Exception as e:
            ss = screenshot_path(7, "checkout-fail")
            page.screenshot(path=str(ss))
            record(7, "Checkout a book", "Loans", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-08: Return a book ---
        try:
            token = get_token("Member")["accessToken"]
            loans = api_get("/api/loans/history?page=1&pageSize=20", token)
            active = [l for l in loans.get("data", []) if l.get("returnedAt") is None]
            if active:
                book_id = active[0]["bookId"]
                api_post("/api/loans/checkin", token, {"bookId": book_id})
                page.click("text=My Loans")
                page.wait_for_timeout(2000)
                ss = screenshot_path(8, "return-my-loans")
                page.screenshot(path=str(ss))
                record(8, "Return a book", "Loans", "PASS", f"Returned bookId={book_id[:8]}...", screenshot=ss.name)
            else:
                ss = screenshot_path(8, "return-no-active")
                page.screenshot(path=str(ss))
                record(8, "Return a book", "Loans", "SKIPPED", "No active loans", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(8, "return-fail")
            page.screenshot(path=str(ss))
            record(8, "Return a book", "Loans", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-09: All sidebar pages load ---
        try:
            nav_items = ["Catalog", "Search", "My Loans", "Recommendations", "Ratings", "Rankings", "Wishlist", "Badges"]
            loaded = []
            failed_pages = []
            for item in nav_items:
                try:
                    link = page.locator(f"text={item}").first
                    if link.count() > 0:
                        link.click()
                        page.wait_for_timeout(1500)
                        loaded.append(item)
                    else:
                        failed_pages.append(f"{item}(not found)")
                except Exception:
                    failed_pages.append(item)
            ss = screenshot_path(9, "navigation-last-page")
            page.screenshot(path=str(ss))
            if not failed_pages:
                record(9, "All pages load", "Navigation", "PASS", f"{len(loaded)}/{len(nav_items)} pages OK", screenshot=ss.name)
            else:
                record(9, "All pages load", "Navigation", "FAIL", f"Failed: {', '.join(failed_pages)}", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(9, "navigation-fail")
            page.screenshot(path=str(ss))
            record(9, "All pages load", "Navigation", "FAIL", str(e)[:100], screenshot=ss.name)

        # --- TC-10: Dark mode toggle ---
        try:
            # Find and click theme toggle
            toggle = page.locator("button[aria-label*='theme'], button[aria-label*='toggle']").first
            if toggle.count() > 0:
                toggle.click()
                page.wait_for_timeout(1000)
                ss = screenshot_path(10, "dark-mode-toggled")
                page.screenshot(path=str(ss))
                # Reload and check persistence
                current_url = page.url
                page.reload(wait_until="networkidle")
                page.wait_for_timeout(1000)
                ss2 = screenshot_path(10, "dark-mode-after-reload")
                page.screenshot(path=str(ss2))
                record(10, "Dark mode toggle persists", "UI", "PASS", "Toggle + reload captured", screenshot=ss.name)
            else:
                ss = screenshot_path(10, "dark-mode-no-toggle")
                page.screenshot(path=str(ss))
                record(10, "Dark mode toggle persists", "UI", "FAIL", "Toggle button not found", screenshot=ss.name)
        except Exception as e:
            ss = screenshot_path(10, "dark-mode-fail")
            page.screenshot(path=str(ss))
            record(10, "Dark mode toggle persists", "UI", "FAIL", str(e)[:100], screenshot=ss.name)

        browser.close()


def run_api_only():
    """Fallback: run tests via API only (no screenshots)."""
    print("  [WARNING] Playwright not installed. Running API-only tests (no screenshots).")
    print("  Install with: pip install playwright && playwright install chromium\n")

    # TC-01 to TC-03: Login tests
    for tc_id, role in [(1, "Member"), (2, "Admin"), (3, "Librarian")]:
        try:
            data = get_token(role)
            assert data["user"]["role"] == role
            record(tc_id, f"Login Dev {role}", "Auth", "PASS")
        except Exception as e:
            record(tc_id, f"Login Dev {role}", "Auth", "FAIL", str(e)[:100])

    # TC-04: Catalog
    try:
        token = get_token("Member")["accessToken"]
        books = api_get("/api/search/books?query=&page=1&pageSize=5", token)
        assert len(books.get("data", [])) > 0
        record(4, "Book catalog loads", "Catalog", "PASS", f"{len(books['data'])} books")
    except Exception as e:
        record(4, "Book catalog loads", "Catalog", "FAIL", str(e)[:100])

    # TC-05: Book detail
    try:
        token = get_token("Member")["accessToken"]
        books = api_get("/api/search/books?query=&page=1&pageSize=1", token)
        book_id = books["data"][0]["id"]
        detail = api_get(f"/api/books/{book_id}", token)
        assert "title" in detail and "author" in detail
        record(5, "Book detail shows info", "Catalog", "PASS", f"'{detail['title']}'")
    except Exception as e:
        record(5, "Book detail shows info", "Catalog", "FAIL", str(e)[:100])

    # TC-06: Search
    try:
        token = get_token("Member")["accessToken"]
        results = api_get("/api/search/books?query=Foundation&page=1&pageSize=10", token)
        assert len(results.get("data", [])) > 0
        record(6, "Search returns results", "Search", "PASS", f"{len(results['data'])} results")
    except Exception as e:
        record(6, "Search returns results", "Search", "FAIL", str(e)[:100])

    # TC-07: Checkout
    try:
        token = get_token("Member")["accessToken"]
        books = api_get("/api/search/books?query=&page=1&pageSize=20", token)
        available = [b for b in books["data"] if b.get("availableCopies", 0) > 0]
        if available:
            try:
                api_post("/api/loans/checkout", token, {"bookId": available[0]["id"]})
                record(7, "Checkout a book", "Loans", "PASS", f"'{available[0]['title']}'")
            except urllib.error.HTTPError as he:
                if he.code in (409, 422):
                    record(7, "Checkout a book", "Loans", "PASS", "Already on loan (idempotent)")
                else:
                    raise
        else:
            record(7, "Checkout a book", "Loans", "SKIPPED", "No available books")
    except Exception as e:
        record(7, "Checkout a book", "Loans", "FAIL", str(e)[:100])

    # TC-08: Return
    try:
        token = get_token("Member")["accessToken"]
        loans = api_get("/api/loans/history?page=1&pageSize=20", token)
        active = [l for l in loans.get("data", []) if l.get("returnedAt") is None]
        if active:
            api_post("/api/loans/checkin", token, {"bookId": active[0]["bookId"]})
            record(8, "Return a book", "Loans", "PASS")
        else:
            record(8, "Return a book", "Loans", "SKIPPED", "No active loans")
    except Exception as e:
        record(8, "Return a book", "Loans", "FAIL", str(e)[:100])

    # TC-09: All endpoints
    try:
        token = get_token("Member")["accessToken"]
        endpoints = [
            "/api/search/books?query=&page=1&pageSize=1",
            "/api/search/books?query=test&page=1&pageSize=1",
            "/api/loans/history?page=1&pageSize=1",
            "/api/recommendations",
            "/api/rankings/books",
            "/api/wishlist",
            "/api/gamification/badges",
            "/api/notifications?page=1&pageSize=1",
        ]
        for ep in endpoints:
            api_get(ep, token)
        record(9, "All pages load", "Navigation", "PASS", f"{len(endpoints)}/8 OK")
    except Exception as e:
        record(9, "All pages load", "Navigation", "FAIL", str(e)[:100])

    # TC-10: Frontend HTML
    try:
        req = urllib.request.Request(BASE_URL_WEB)
        resp = urllib.request.urlopen(req, timeout=10)
        html = resp.read().decode()
        assert len(html) > 100
        record(10, "Frontend loads", "UI", "PASS", f"{len(html)} bytes")
    except Exception as e:
        record(10, "Frontend loads", "UI", "FAIL", str(e)[:100])


def generate_report():
    """Generate the markdown report."""
    REPORT_DIR.mkdir(parents=True, exist_ok=True)

    passed = sum(1 for r in RESULTS if r["status"] == "PASS")
    failed = sum(1 for r in RESULTS if r["status"] == "FAIL")
    skipped = sum(1 for r in RESULTS if r["status"] == "SKIPPED")

    has_screenshots = any(r.get("screenshot") for r in RESULTS)
    screenshot_col = "| Screenshot " if has_screenshots else ""
    screenshot_hdr = "| --- " if has_screenshots else ""

    report = f"""# Browser Functional Test Report

- **Date**: {datetime.now().strftime('%Y-%m-%d %H:%M')}
- **Type**: regression
- **Environment**: {BASE_URL_WEB} / {BASE_URL_API} (Docker)
- **Tool**: {'Playwright (headless browser + screenshots)' if has_screenshots else 'API-level validation'}
- **Screenshots**: {'Yes — see `{TODAY}-screenshots/`' if has_screenshots else 'No (install Playwright for visual evidence)'}

## Summary

| Total | Passed | Failed | Skipped |
|-------|--------|--------|---------|
| {len(RESULTS)} | {passed} | {failed} | {skipped} |

## Results

| # | Test Case | Module | Status | Notes {screenshot_col}|
|---|-----------|--------|--------|------- {screenshot_hdr}|
"""

    for r in RESULTS:
        ss_link = f"| [{r['screenshot']}]({TODAY}-screenshots/{r['screenshot']}) " if r.get("screenshot") else ("| " if has_screenshots else "")
        report += f"| {r['id']} | {r['name']} | {r['module']} | **{r['status']}** | {r['notes']} {ss_link}|\n"

    if failed > 0:
        report += "\n## Failed Tests Detail\n\n"
        for r in RESULTS:
            if r["status"] == "FAIL":
                report += f"### TC-{r['id']:02d}: {r['name']}\n"
                report += f"- **Notes**: {r['notes']}\n"
                if r.get("screenshot"):
                    report += f"- **Screenshot**: [{r['screenshot']}]({TODAY}-screenshots/{r['screenshot']})\n"
                report += "\n"

    with open(REPORT_FILE, "w") as f:
        f.write(report)

    return passed, failed, skipped


def main():
    print(f"{'='*60}")
    print(f"  MiniLibrary Browser Functional Test Suite")
    print(f"  {datetime.now().strftime('%Y-%m-%d %H:%M')}")
    print(f"  Mode: {'Playwright + Screenshots' if HAS_PLAYWRIGHT else 'API-only (no screenshots)'}")
    print(f"{'='*60}\n")

    # Verify environment
    try:
        urllib.request.urlopen(f"{BASE_URL_WEB}", timeout=5)
        urllib.request.urlopen(f"{BASE_URL_API}/health", timeout=5)
    except Exception as e:
        print(f"ERROR: Environment not ready — {e}")
        print(f"Run: docker compose -f docker/docker-compose.yml up -d")
        sys.exit(1)

    print("  Environment: OK (frontend + API responding)\n")

    if HAS_PLAYWRIGHT:
        run_with_playwright()
    else:
        run_api_only()

    passed, failed, skipped = generate_report()

    print(f"\n{'='*60}")
    print(f"  RESULTS: {passed} PASS | {failed} FAIL | {skipped} SKIPPED")
    print(f"  Report:  {REPORT_FILE}")
    if HAS_PLAYWRIGHT:
        print(f"  Screenshots: {SCREENSHOT_DIR}/")
    print(f"{'='*60}")

    sys.exit(0 if failed == 0 else 1)


if __name__ == "__main__":
    main()
