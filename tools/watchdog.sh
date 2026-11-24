#!/usr/bin/env bash
# Pipeline watchdog: every commit must keep these checks green.
# Runs on every push and pull request via .github/workflows/ci.yml,
# and locally before pushing: bash tools/watchdog.sh
set -u
cd "$(dirname "$0")/.." || exit 1
fail=0
ok() { printf 'ok   %s\n' "$1"; }
bad() { printf 'FAIL %s\n' "$1"; fail=1; }

# 1. Every relative markdown link resolves from its own directory.
if python3 - <<'PY'
import re, os, sys
root = os.getcwd()
bad = []
for d in ["", "docs", "art", "story", "apps"]:
    full = os.path.join(root, d)
    if not os.path.isdir(full):
        continue
    for f in sorted(os.listdir(full)):
        if not f.endswith(".md"):
            continue
        path = os.path.join(full, f)
        text = open(path, encoding="utf-8").read()
        for m in re.finditer(r'\[[^\]]*\]\(([^)\s]+)\)', text):
            link = m.group(1)
            if link.startswith(("http://", "https://", "#", "mailto:")):
                continue
            target = os.path.normpath(os.path.join(full, link.split("#")[0]))
            if not os.path.exists(target):
                bad.append(path + ": " + link)
for line in bad:
    print(line)
sys.exit(1 if bad else 0)
PY
then ok "links resolve"; else bad "links resolve"; fi

# 2. Ticket files keep the standard shape: Assignee field and checklist boxes.
for f in art/TICKETS.md story/TICKETS.md apps/TICKETS.md docs/TICKETS.md; do
    if grep -q 'Assignee:' "$f" && grep -q -- '- \[ \]' "$f"; then
        ok "ticket shape $f"
    else
        bad "ticket shape $f"
    fi
done

# 3. Mission pointer exists and is exactly one line.
if [ -f docs/.active-mission ] && [ "$(grep -c . docs/.active-mission)" = "1" ]; then
    ok "active mission set"
else
    bad "active mission set"
fi

# 4. Latest G-SEC verdict is PASS, or CAUTION accepted by the person.
verdicts=$(find docs -path '*08_verification/verdicts.md' | sort | tail -n1)
if [ -n "$verdicts" ]; then
    last=$(grep '^- G-SEC' "$verdicts" | tail -n1)
    case "$last" in
        *'G-SEC PASS'*) ok "latest verdict PASS" ;;
        *'G-SEC CAUTION'*'| accepted:'*) ok "latest verdict CAUTION accepted" ;;
        *) bad "latest verdict ($last)" ;;
    esac
else
    bad "verdicts.md found"
fi

# 5. Unity suites once they exist (APP-002); skipped while docs-only.
if [ -x tools/run-tests.sh ]; then
    if tools/run-tests.sh >/dev/null 2>&1; then ok "run-tests.sh"; else bad "run-tests.sh"; fi
else
    ok "run-tests.sh not present yet (skipped)"
fi

exit $fail
