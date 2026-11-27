# Git Workflow — RFL

Commit, branch, push and merge rules for this project, binding from D−28 to D44 and for
every Claude seat. The repository pair is described in README.md; process context lives in
WORKING-RULES.md and SDLC.md. MUST / SHOULD / MAY follow RFC 2119 as in CODING-STANDARD.md.

## Remotes

| Name | URL | Used for |
|---|---|---|
| `origin` | https://github.com/sixer0/rfl.git | the team repository: every developer branch, push and pull request; main moves only by merged pull requests plus gate approvals |
| `swarm` | https://github.com/sixer0/swarm.git | information only: worked exclusively by the agent swarm as a benchmark |

Team developers set `origin` only:

```bash
git remote add origin https://github.com/sixer0/rfl.git
```

Daily work happens on branches in `origin`; `origin/main` only takes merges and gate
approvals.

### The `swarm` repository is a benchmark, not a work remote

`sixer0/swarm` holds the same project built by the agent swarm alone. It exists so the
team's output in `rfl` can be compared against the agents' output for the same cards.

- Team developers MUST NOT push, branch, open pull requests or merge in `swarm`.
- Work from `swarm` never flows into `rfl` by merge, cherry-pick or copy. If an idea seen
  there is worth having, it enters `rfl` as a normal card, built by the team.
- Reading it for comparison is allowed. Optionally add it as a fetch-only remote:

```bash
git remote add swarm https://github.com/sixer0/swarm.git
git remote set-url --push swarm DISABLED
```

- Benchmark comparisons cite both sides by commit: `rfl@<sha>` versus `swarm@<sha>`.

## Branches and worktrees

Every work row runs in its own branch, in its own worktree. Two rows never share a branch;
one branch never carries boxes from two cards. This keeps each overnight queue isolated:
a seat can run three cards at once without stashing or mixing diffs.

- One branch per ticket row (one card `C-nnnn`), named `<area>-<ticket>-<slug>`, lowercase,
  hyphenated: `app-010-training-reset`, `art-004-locomotion-clips`,
  `story-005-tuning-freeze`, `doc-003-decision-trail`. Manual edits keep the fixed form
  `manual-cr-nn` (SDLC rule 19).
- Branches start from the newest upstream, together with their worktree:

```bash
git fetch origin
git worktree add ../wt-app-010-reset -b app-010-training-reset origin/main
```

- Work the whole card inside that worktree; push at natural breaks.
- After merge, clean up the worktree and the branch, locally and on `origin`:

```bash
git worktree remove ../wt-app-010-reset
git branch -d app-010-training-reset
```

- A branch lives exactly as long as its card; no long-lived branches except `main`.
  Rebase your own branch freely; it belongs to one card only.

## Commits

- Subject: imperative, ≤ 72 characters, no trailing period. Body links the card ID and
  quotes the check output from `tools/run-tests.sh` (SDLC rule 10).
- One checklist box per commit where possible; never mix code and generated evidence in
  one commit.
- Tuning changes commit the data file and the importer report line together, and bump the
  data version by 1 in the same commit.
- Only a human gate approval may start a subject with `gate:` (rule 3). Agents never do.
- No secret value ever enters a message or diff; name location and type only (rule 20).

Sample commit:

```text
Add training-mode reset with part refill

Card C-0007, chapter 17. Reset restores positions, meters and part states;
frame-data panel refreshed from the same import pass.

Check: tools/run-tests.sh -> EditMode 214 passed, determinism hash equal on both runners.
```

## Pushing

- Push your branch to `origin` at each natural break; the night queue only sees pushed
  cards. Evening push lands by 18:00 seat time so the queue can run overnight.
- Update from upstream with `git pull --rebase origin main`; keep your branch linear.
- Force-push is allowed only on your own branch, only with `--force-with-lease`. Never
  force-push `origin main`.
- Never push to `swarm` (see Remotes).
- Do not push during a gate review window; the auditor snapshots the tree at gate time.

## Pull requests

- Open against `origin/main` from your card branch on `origin`. Title follows
  commit-subject style.
- `main` is protected by the `protect-main` ruleset: developers open pull requests but
  cannot merge them or push to `main`; only the repository admin (Budi, `sixer0`) merges.
- Body per the STRUCTURE.md sample: closes-card line, bullets of what changed, evidence
  block quoting checks, the pair agent used (tool and model), reviewer name (Budi).
- One PR per card; split oversized PRs before review.
- Rafif authors the pull request; Budi reviews line by line; behaviour not named by the card is rejected at
  review (rule 9).
- CI green is a merge requirement (rule 11). The pipeline watchdog runs on every push
  and pull request through `.github/workflows/ci.yml`: relative links resolve, ticket
  files keep the standard shape, the mission pointer is set, and the latest G-SEC verdict
  is PASS or an accepted CAUTION. Run it locally before pushing: `bash tools/watchdog.sh`.
  Unity suites attach through `tools/run-tests.sh` once APP-002 lands.
- Merge requires a clean review: zero HIGH and zero CRITICAL findings across code review
  (structure, duplication, dependency impact) and security review (secrets, injection,
  permission escalation). Both verdicts are quoted in the PR body; an open HIGH or
  CRITICAL keeps the PR out of main.
- Reversible by standard: one concern per PR, small enough to revert as a single commit.
  Rename-only changes land separately from behaviour changes; a revert of one card must
  not touch another card's output. If a PR cannot be described in one sentence of "what
  it changes", split it before review.
- MINOR findings never block the merge: each one becomes a bullet in
  `docs/state/backlog.md` with its file:line and finder, one line per finding. Backlog
  items are triaged in the next tuning window and must be closed or dated before the
  next gate.

## Merging

- Squash-merge: one card lands as exactly one commit on main, subject keeps the card ID.
- Merge only when every checklist box of the card is ticked and evidence is quoted in the
  PR body.
- Gate approvals are the exception to the PR path: Budi commits `gate: Gn approved`
  directly on main, Rafif co-signs in the body.
- After feature freeze (G4) every merge body names its S1 item; anything else waits for
  Edition 2 (rule 5).

## Tags

- `baseline-1` on the G0 approval commit — the frozen baseline every lane builds against.
- Releases are `REL-nnn` on RC builds: `REL-001` at G5; the first content update gets
  `REL-002`. Annotated tags carry the gate ID:

```bash
git tag -a REL-001 -m "gate: G5 release candidate"
git push origin REL-001
```

## Daily loop with git

1. Morning: `git fetch origin`, rebase the open branch onto `origin/main`.
2. Work the card; commit per checklist box; run `tools/run-tests.sh` before each push.
3. Evening: push the branch to `origin`, open or update the PR, queue the next card at
   18:00 seat time.
4. At merge: delete the branch locally and on `origin`, regenerate
   `docs/state/progress.md`.
