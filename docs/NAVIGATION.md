# Navigation Rules — RFL

How to find anything in this repo in one hop, and how to keep it findable. The README is
the only entry point; this file is the map behind it. Binding for humans and Claude seats.

## Principles

1. **One entry point.** Start at `README.md`; every other file is reachable from its tables.
2. **One owner per fact.** Each value lives in exactly one file; everywhere else links to
   it. Numbers: `story/TICKETS.md` (STO-005) and `asset_src/data/tuning.json`. Dates and
   gates: `docs/TIMELINE.md`. Process: `docs/WORKING-RULES.md`. Shape: `docs/STRUCTURE.md`.
   Terms: `docs/GLOSSARY.md`. Code rules: `docs/CODING-STANDARD.md`.
3. **Follow the ladder, not your memory.** README → TICKETS.md row → card `C-nnnn` → gate
   pack `gate-Gn.md` → decision `DN-nnn`. Each level cites the one above by ID.
4. **Relative links only.** From the root use `docs/TIMELINE.md`, from inside `docs/` use
   `TIMELINE.md`. Never absolute paths, so the checkout works anywhere.

## Where things live

| Looking for… | Go to | Named how |
|---|---|---|
| Schedule, gates, Steam dates | `docs/TIMELINE.md` | G0–G7, D−28…D44 |
| Work to do this week | `<area>/TICKETS.md` | ART-, STO-, APP-, DOC-nnn |
| One ticket's running state | `docs/cards/C-nnnn.md` | zero-padded four digits |
| How code must look | `docs/CODING-STANDARD.md` | sections by layer |
| How work moves, roles, hours | `docs/WORKING-RULES.md` | owner codes RAF/ORY/BUD/AND |
| Non-negotiable process rules | `docs/SDLC.md` | numbered rules 1–23 |
| Artifact templates with samples | `docs/STRUCTURE.md` | one section per artifact |
| Any term or abbreviation | `docs/GLOSSARY.md` | alphabetical-ish groups |
| Why something was decided | `docs/decisions/decision-log.md` | DN-nnn, newest on top |
| A pending change request | `docs/decisions/changes/CR-nn.md` | one file per CR |
| What an agent got stuck on | `docs/decisions/open-questions.md` | dated lines, oldest first |
| Gate evidence | `docs/reports/gate-Gn.md` | lowercase `gate-` prefix |
| Week progress numbers | `docs/state/progress.md` | generated, never typed |
| Asset census and licences | `asset_src/ledger.csv`, `asset_src/Art/Generated/PROVENANCE.md` | AST ids |
| Fight values | `asset_src/data/tuning.json`, `asset_src/data/moves.csv` | one value per field |
| Manual chapter text | `docs/manual/NN-chapter-name.md` | file number = chapter number |
| Current mission folder | `docs/.active-mission` | one line: mission id |

## Reading paths by role (first pass, by D−25)

| Reader | Read first | Then, by D−10 | Open daily |
|---|---|---|---|
| Rafif | Chapters 1, 2, 3, 5 | 6, 9, 12, 14, 17, 20, 21 | 14 (master schedule), 17 |
| Ory | Chapters 1, 2, 5, 6 | 8, 11, 18, 19, 20, 26 | 18, 19, Annex D |
| Budi | Chapters 1, 2, 4, 5, 13 | Parts II, III, IV in full | 14, 15, 22 |
| Andy | Sections 1.7–1.8, chapters 2, 5 | 6, 10, 24, 25, 29 | 29, Annex G |
| Claude seats | Section 1.2, chapter 2 | only the chapter named on the card | the card's chapter, Annex B |

Chapter numbers refer to files in `docs/manual/`; the manual itself is the source, this
table mirrors its Table 1.5.

## Citation style inside files

- Tickets cite the manual by chapter and table number: `(Chapter 5, Table 5.6)`.
- Cards cite their ticket row ID and chapter: `Ticket: APP-010 · Chapter: 17`.
- Gate packs cite checks by command and capture name: `tools/run-tests.sh --hash 10000`.
- Decisions cite the option set and the revisit condition, never a pasted excerpt.
- Where two files state the same number, the lower layer wins on drift: data file >
  TICKETS.md > TIMELINE.md > README summary; fix the upper one through a CR.

## Keeping the map true

- A new top-level folder or standing document gets a row in the tables above in the same
  commit that adds it.
- Renames or moves update every relative link that targets them; CI grep over `]( ` paths
  catches misses.
- Never move content between the five docs above; extend the owning file instead.
- Scratch notes go in the mission folder named by `docs/.active-mission`, not beside the
  standards. Close-out reports live under `docs/<mission-id>/` phases.
- Link check runs at every gate: every relative link in README and docs resolves from the
  repo root; broken link = S2 change request.
