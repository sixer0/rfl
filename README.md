# Robot Fighting League — Production Board

Companion board for the RFL Production Manual (`002/GSP/KT-XI-2026 Rev A`, Edition 1,
21 Nov 2026). The manual is binding; this repo holds the schedule, the ticket breakdown
and the working docs derived from it. The PDF sits at the repo root as the source of truth.

## Current milestone — MVP week (from 24 Nov 2026)

Deliver the **game menu layout** and a **training mode** that exercises every base
mechanic: health, stamina, block, parry, sidestep/dash, hit-stop, pushback, part damage,
groggy, OVERLOAD, juggle cap, throw, submission, corner break, round timer with carry-over.
Versus stays a stub until the MVP review passes. Assignees are intentionally blank; they
are filled at the Monday stand-up.

## Structure

| Folder | Holds | Lead |
|---|---|---|
| `art/` | Audio-visual: robot body, damage visuals, clips, arena, VFX, UI art, audio cues | Ory (ORY) |
| `story/` | Specs and tuning data: pitch, move list, fight rules, tuning set, training-mode spec, copy | Rafif (RAF) |
| `apps/` | Engineering: project skeleton, simulation core, combat systems, training mode, menus, HUD, CI, builds | Budi (BUD) |
| `docs/` | Timeline, task cards, decision log, evidence packs, provenance | Budi (BUD), Andy (AND) for gates |

Each folder carries one `TICKETS.md`: numbered tickets, each with its own work checklist
(one box per line) and an empty `Assignee:` field. `docs/TIMELINE.md` is the master schedule;
`docs/CODING-STANDARD.md` binds all code; `docs/WORKING-RULES.md` binds how work moves;
`docs/GLOSSARY.md` is the shared vocabulary; `docs/STRUCTURE.md` fixes the shape of every
artifact with filled samples.

## How work moves

- Days are relative: Phase 0 runs D−28 to D0, production D1 to D30, demo live D30, review D44.
- Gates G0–G7 divide the project (see `docs/TIMELINE.md`). Only a human approves a gate:
  Andy commits `gate: Gn approved` with Rafif as director. Agents never approve gates.
- Every ticket becomes one task card `C-nnnn` in `docs/cards/`. A card names its owner,
  the manual chapter it implements, and its stop point. Claude seats work only from cards
  on their holder's seat; the night queue loads at 18:00 owner-seat time.
- Changes to scope or locked values go through a change request `CR-nn` in
  `docs/decisions/changes/`; decisions land in `docs/decisions/decision-log.md` as `DN-nnn`.
- Gate evidence goes to `docs/reports/gate-Gn.md`, written by the gate auditor on Andy's seat.
- Severity: S1 lands same day, S2 with the next daily build, S3 waits for Edition 2.
  Only S1 changes may land after feature freeze at G4.

## Repository pair

`gsp` is the upstream repo; `swarm` is its fork for in-flight branches. Push working
branches to `swarm`, open pull requests from there into `gsp`. Never force-push `gsp` main.
