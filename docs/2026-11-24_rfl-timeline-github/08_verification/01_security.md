# Security review — G-SEC (W5 step 1)

Mission: 2026-11-24_rfl-timeline-github · Reviewed commit: f073158 on main · Reviewer: security-review · Date: 2026-11-24T10:42:08Z

## Scope

Commit f073158 "Add enterprise production timeline and comprehensive work tickets ..." — purely additive: 8 files, 223 insertions, 0 deletions (`git show --stat f073158`). Files: README.md, .gitignore, docs/TIMELINE.md, docs/TICKETS.md, art/TICKETS.md, story/TICKETS.md, apps/TICKETS.md, and the source PDF "RFL Production Manual 002-GSP-KT-XI-2026 RevA.pdf" (Bin 0 -> 34154654 bytes). All content is Markdown planning documentation plus one PDF; no application code.

## Risk profile

| Dimension | Assessment |
|-----------|------------|
| Data access | write (documentation files), transmit on publish to GitHub |
| Data sensitivity | internal planning docs; first names of four people tied to roles |
| External interaction | outbound only at publish time (GitHub, HTTPS); no URLs or API endpoints inside the content |
| Destructive potential | none — additive commit, revert is the rollback |
| Scope | local filesystem + one publish target |

Review level: L1 quick scan (read-only content + additive docs; no code, no config beyond `.gitignore`).

## Per-area results

### 1. Credential leak — PASS

- All seven Markdown files read in full: only plain prose and Markdown tables; no `sk-…`, `api_key=`, `password=`, token, connection-string or PRIVATE KEY blocks appear. Evidence: regex scan over `*.md` (`https?://|sk-[A-Za-z0-9]|api[_-]?key|password\s*[:=]|BEGIN .*PRIVATE KEY|export [A-Z_]+=`) returned only two hits, both false positives from hyphenated English words — docs/TICKETS.md:12 ("task-card") and docs/TICKETS.md:16; no credential values.
- Commit metadata carries the author email `60373859+sixer0@users.noreply.github.com` — GitHub's noreply format, designed for public commits. Low exposure, no action.
- PDF: scanned via `strings -a … | grep -Ei '<credential patterns>'` — zero matches. Coverage limit disclosed: PDF text lives mostly in Flate-compressed streams, so only uncompressed strings were searched; the compressed body was not exhaustively scanned (inline python-decompress attempts were refused by the shell guard, see "Checks not run"). Residual risk judged low for a production manual whose content mirrors the reviewed Markdown.

### 2. Destructive operation — PASS

- Commit is additive: `git show --stat f073158` reports 223 insertions, no deletions; touches no existing behaviour. Publish is a normal push to main; README.md:36 already states the guardrail "Never force-push `gsp` main." Rollback = revert the commit. Dry-run not needed.

### 3. External call — PASS

- No URLs of any kind inside the Markdown (grep above found no `http(s)://` matches). External services are named only as prose: GitHub runners / GameCI (apps/TICKETS.md:7-8), Steamworks onboarding (docs/TIMELINE.md:35), Claude seats (docs/TIMELINE.md:68, 98). The only runtime external call is the publish itself to GitHub over HTTPS; payload is this documentation, no secrets in it per check 1. No non-TLS endpoint receives anything.

### 4. Injection surface — PASS

- Content is plain Markdown tables and text; no raw HTML, no `<script>`, no templating directives. Commands mentioned as prose (`tools/run-tests.sh`, `git log -- docs/manual/` at docs/TICKETS.md:18) are fixed literals with static arguments, not built from untrusted input. GitHub's Markdown renderer receives ordinary table markup — nothing to escape or sanitize.

### 5. Permission escalation — PASS (with note)

- Workflow described grants no standing privileges: PRs flow fork→upstream with human gate approval only (README.md:22-23, 33-36); docs/TICKETS.md:14 sets a deny-edit rule for agent sessions in `.claude/settings.json` — least-privilege by design. Note for later: apps/TICKETS.md:7-8 plans a self-hosted Windows runner; self-hosted runners execute workflow jobs with host access, so when APP-002 lands, scope the runner to this repo's trusted branches only. Not a risk in this commit.

### Misconfiguration — ok (one forward-looking note)

- .gitignore:1 contains only `.DS_Store`. Correct for a docs-only repo today. When the Unity project arrives (APP-001), add Unity cache ignores (Library/, Temp/, Logs/) then — not now. The 34 MB PDF sits inside GitHub's limits without LFS. Neither affects this publish.

### Personal data handling — ok

- Only first names/honorifics bound to roles (Ory, Rafif, Budi, Andy — README.md:11-14); no addresses, phone numbers or other identifiers in file bodies. Minimisation holds: names are functionally required for ticket ownership. Retention/consent/deletion: planning records with no personal-data collection beyond role attribution; the git history itself is the access log. Nothing to change.

## Overall verdict: PASS

All five checklist areas pass or are n/a for pure documentation; the single evidence gap (PDF compressed-stream coverage) is disclosed above and is low-risk given the mirrored Markdown source. Safe to publish.

## Security review log

| # | Timestamp | Action | Level | Result | Decision | Reviewer |
|---|-----------|--------|-------|--------|----------|----------|
| 1 | 2026-11-24T10:42:08Z | Publish commit f073158 (docs + PDF) to GitHub | L1 | PASS | Proceed | security-review |

## Checks not run

- Exhaustive text scan of the PDF's Flate-compressed streams: inline-python attempts were refused by the shell guard (three calls, per its protected-location rule); fell back to `strings -a` over uncompressed content, which found no credential patterns. Partial coverage disclosed, not treated as a finding.
