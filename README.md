# Robot Fighting League — Production Board

Companion board for the RFL Production Manual (`002/GSP/KT-XI-2026 Rev A`, Edition 1,
21 Nov 2026). The manual is binding; this repo holds the schedule, the ticket breakdown
and the working docs derived from it. The PDF sits at the repo root as the source of truth.

## Who works here

This repository is a practice project for familiarising Rafif with professional
development work. The team is two people, no one else:

- **Rafif (RAF)** does the majority of the work: specs, tuning, code, art, audio and
  playtests, one pull request per card.
- **Budi (BUD)** plans the work timeline and the cards, reviews every pull request and
  manages main (merges, gates).

Roles in full: [docs/WORKING-RULES.md](docs/WORKING-RULES.md).

## Current milestone — MVP week (from 24 Nov 2026)

Deliver the **game menu layout** and a **training mode** that exercises every base
mechanic: health, stamina, block, parry, sidestep/dash, hit-stop, pushback, part damage,
groggy, OVERLOAD, juggle cap, throw, submission, corner break, round timer with
carry-over. Versus stays a stub until the MVP review passes. Rafif builds; Budi plans the
order and reviews. Assignees are filled when Budi sequences the cards.

## Start here

| Order | File | Answers |
|---|---|---|
| 1 | [docs/TIMELINE.md](docs/TIMELINE.md) | When what happens: phases, gates G0–G7, Steam track, capacity model |
| 2 | Your folder's TICKETS.md: [art](art/TICKETS.md) · [story](story/TICKETS.md) · [apps](apps/TICKETS.md) · [docs](docs/TICKETS.md) | What to do next: numbered tickets with work checklists |
| 3 | [docs/CODING-STANDARD.md](docs/CODING-STANDARD.md) | How code must look: determinism, data-vs-code, budgets, style, tests |
| 4 | [docs/WORKING-RULES.md](docs/WORKING-RULES.md) | How work moves: roles and seats, hours, cards, gates, CR path, evidence |
| 5 | [docs/SDLC.md](docs/SDLC.md) | Mandatory rules and workflow order that no one may break during development |
| 6 | [docs/STRUCTURE.md](docs/STRUCTURE.md) | Exact shape of every artifact, with filled samples |
| ref | [docs/GLOSSARY.md](docs/GLOSSARY.md) | Meaning of any term used here |
| ref | [docs/NAVIGATION.md](docs/NAVIGATION.md) | Where anything lives; full navigation rules |
| ref | [docs/GIT-WORKFLOW.md](docs/GIT-WORKFLOW.md) | Commit, branch, push and merge rules for the repo pair |

## Folder map

| Folder | Holds | Lead |
|---|---|---|
| `art/` | Audio-visual: body, damage visuals, clips, arena, VFX, UI art, audio cues | Rafif (RAF), reviewed by Budi |
| `story/` | Specs and tuning data: pitch, move list, fight rules, tuning set, training-mode spec, copy | Rafif (RAF), reviewed by Budi |
| `apps/` | Engineering: skeleton, simulation core, combat systems, training mode, menus, HUD, CI, builds | Rafif (RAF), reviewed by Budi |
| `docs/` | Timeline, standards, glossary, cards, decisions, evidence packs, provenance index | Budi (BUD), gates included |

Each folder carries one `TICKETS.md`: numbered tickets, each with its own work checklist
(one box per line) and an empty `Assignee:` field. The canonical repository tree lives in
`docs/STRUCTURE.md`.

## How work moves (summary)

- Days are relative: Phase 0 D−28→D0, production D1→D30, demo live D30, review D44.
- Gates G0–G7 divide the project; only a human approves, via commit `gate: Gn approved`.
- Every ticket row becomes one task card `C-nnnn` in `docs/cards/`; agents work only from cards.
- Changes go through `CR-nn` in `docs/decisions/changes/`; decisions log as `DN-nnn`.
- Severity S1 same day, S2 next build, S3 next edition; after G4 only S1.
- Every commit stays green on the pipeline watchdog (`bash tools/watchdog.sh`), run by CI
  on push and pull request; merge needs zero HIGH/CRITICAL findings, minors to backlog.

Full rules: `docs/WORKING-RULES.md`. Vocabulary: `docs/GLOSSARY.md`.

## Repository pair

`rfl` is the team repo: Rafif pushes his branches here and opens pull requests into
`rfl` main, which only Budi as admin merges. Never force-push `rfl` main. `swarm` is for
information only: it is worked exclusively by the agent swarm as a benchmark against the
team's output, and the team never pushes or merges there. Details: `docs/GIT-WORKFLOW.md`.
