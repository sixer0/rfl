# Robot Fighting League — Production Board

Companion board for the RFL Production Manual (`002/GSP/KT-XI-2026 Rev A`, Edition 1,
21 Nov 2026). The manual is binding; this repo holds the schedule, the ticket breakdown
and the working docs derived from it. The PDF sits at the repo root as the source of truth.

## Structure

| Folder | Holds | Lead |
|---|---|---|
| `art/` | Audio-visual workstream: robot body, liveries, damage mask, arena, animation clips, VFX, UI art, audio | Ory (ORY) |
| `story/` | Design and narrative specs: pitch, pillars, move list, fight rules, tutorial script, identity notes | Rafif (RAF) |
| `apps/` | Engineering: Unity project, simulation core, combat systems, input, CPU, HUD, audio integration, performance, CI, builds | Budi (BUD) |
| `docs/` | Timeline, gates, decision log, change requests, evidence packs, provenance | Budi (BUD), Andy (AND) for gates |

Each folder carries one `TICKETS.md`: numbered work tickets with owner, day window,
dependencies and a measurable "done when" check. `docs/TIMELINE.md` is the master schedule.

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
