# Structure Standard — RFL

The fixed shape of every artifact in this repo, each with a filled sample. The plan
workbook stays the master copy of game data (Decision #53); files here describe shape.
Sample numbers illustrate the format and match manual Rev A values where stated.

## Repository tree

```text
/                                  README.md, manual PDF, .gitignore
├─ art/                            TICKETS.md            art workstream
├─ story/                          TICKETS.md            specs and tuning data
├─ apps/                           TICKETS.md            engineering
├─ docs/
│  ├─ TIMELINE.md                  master schedule
│  ├─ CODING-STANDARD.md           binding for code
│  ├─ WORKING-RULES.md             binding for process
│  ├─ NAVIGATION.md                where anything lives, reading paths
│  ├─ GLOSSARY.md                  this vocabulary
│  ├─ STRUCTURE.md                 this file
│  ├─ manual/                      one Markdown per chapter, 01…37
│  ├─ cards/                       C-0001.md … one per ticket row
│  ├─ decisions/                   decision-log.md, changes/CR-nn.md, open-questions.md
│  ├─ reports/                     gate-G0.md … gate-G7.md, gate-MVP.md
│  └─ state/                       progress.md          generated, never typed
├─ Assets/RFL/
│  ├─ Sim/                         deterministic core (RFL.Sim)
│  ├─ Data/                        read-only tables (RFL.Data)
│  ├─ Game/                        presentation glue (RFL.Game)
│  └─ Editor/                      importer and tooling (RFL.Editor)
├─ asset_src/
│  ├─ data/                        tuning.json, moves.csv
│  ├─ ledger.csv                   asset census with AST ids
│  └─ Art/Generated/PROVENANCE.md  generated-asset ledger
└─ tools/                          run-tests.sh         single test entry point
```

Naming: folders as shown; card and CR files zero-pad as in their ID (`C-0007.md`,
`CR-03.md`); gate packs are lowercase `gate-G3.md`; decision entries carry no file of
their own, they append to `decision-log.md`.

## Task card — `docs/cards/C-0007.md`

```markdown
# C-0007 — Training mode

Ticket: APP-010 · Chapter: 17 · Owner: BUD + C-BUD
Stop point: after the instant-reset box is checked
Check: every base mechanic demonstrable in one session, in this order: health, stamina,
block, parry, sidestep, hit-stop, part damage, groggy, OVERLOAD, juggle cap, throw,
submission, corner break, timer carry-over
Status: in-progress · Started: D5

- [x] infinite-round toggle (timer off)
- [x] dummy presets: idle, blocks seen hits, punishes minus moves
- [ ] frame-data panel: startup, active, recovery, on block, on hit
- [ ] input history strip for both sides
- [ ] hitbox/hurtbox debug overlay
- [ ] instant reset: positions, meters, parts

Notes: panel reads the imported tables, no second number source.
```

Rules a card MUST satisfy: one ticket row per card; names the manual chapter; names the
stop point; the Check line is copied from the ticket's Done when, not reworded.

## Decision-log entry — append in `docs/decisions/decision-log.md`

```markdown
DN-014 | 2026-11-24 | decider: AND
Heavy hit-stop stays 8 f until the blind A/B before G2.
Options: keep 8 f (weight pillar) vs 6 f (Panduan range 2–6).
Evidence: none yet; A/B scheduled into the G2 pack.
Choice: keep 8 f as starting value, test against 6 f before G2.
Revisit: at G2 with the blind-test result; change by data edit only.
```

Newest on top. Every entry names decider, options, evidence, choice, revisit condition.

## Change request — `docs/decisions/changes/CR-03.md`

```markdown
# CR-03 — Drop second livery to a tint variant

Raised: 2026-11-26 by ORY via open-questions
Triage: S2 · touches scope: yes · needs AND: yes
What: ART-002 ships STANDARD + sparring-unit only; the second mirror-match livery
becomes a colour-tint of the first.
Why: material and clip budget for the MVP week; cut-list item 2.
Impact: ART-002 loses one checklist box; census liveries 3 → 2 (+tint).
Decision: approved — commit `change: CR-03 approved` (AND, D6).
Status: closed
```

## Open-question line — append in `docs/decisions/open-questions.md`

```text
2026-11-26 T09:12 | C-BUD | card C-0007 | Does the training-mode reset also refill
stamina and parts, or only positions? Manual silent. Card stopped at box "instant reset".
```

The line that answers it is appended below by Budi at the next triage; the agent never
answers for itself.

## Gate evidence pack — `docs/reports/gate-G1.md`

```markdown
# Gate G1 — Core loop · D6

Question: is the punch-block-sidestep exchange fun in a grey box?

## Checks
| Check | Command / source | Result |
|---|---|---|
| Determinism hash | tools/run-tests.sh --hash 10000 | equal hash Mac + Windows, quoted below |
| Tick budget | Profiler capture G1-06.png | 0.27 ms median on minimum PC |
| Exchange reads | Rafif playtest form PT-014 | yes/no per pillar, all yes |
| Ring fixed at 9.0 m | scene inspector | matches AST-002 row |

## Counts vs census (Table 2.8)
clips 13/24 · cues 18/45 · screens 6/16 · strings 90/265 — on track

## Outcome
pass with conditions (SHOULD only, max 3):
1. parry lockout feel to be re-checked after 20 more rounds (SHOULD)

Approver: Andy, commit `gate: G1 approved` · Director co-sign: Rafif
Next: G2 at D12 — vertical slice; blind hit-stop A/B lands here.
```

## Provenance row — `asset_src/Art/Generated/PROVENANCE.md`

```markdown
| File | Tool | Prompt or reference | Licence | Rights holder | Used in |
|---|---|---|---|---|---|
| hit_heavy_01.wav | ElevenLabs (paid tier) | "metallic clang, short, no reverb" | commercial terms | gsp (holder) | hit layer heavy |
| arena_malioboro_01.png | FLUX 3 Klein | ref crop: sheet A3, no text | commercial terms | gsp (holder) | arena backdrop |
```

Every generated or bought file gets exactly one row; licence never empty.

## Progress page — `docs/state/progress.md` (generated)

```markdown
Progress — generated 2026-11-27T07:00Z by tools/progress.sh — do not edit by hand

Cards: done 11 · in-progress 3 · blocked 1 · todo 35
Blocked: C-0012 (waiting CR-03 decision by AND)
Week target: menu layout + training mode with all base mechanics
Checklist boxes ticked: 84 of 231
```

## Commit message — sample

```text
Add training-mode reset with part refill

Card C-0007, chapter 17. Reset restores positions, meters and part states;
frame-data panel refreshed from the same import pass.

Check: tools/run-tests.sh -> EditMode 214 passed, determinism hash equal on both runners.
```

Imperative subject under 72 chars; body links the card; check output quoted. Gate
approvals are the only messages starting with `gate:`.

## Pull-request body — sample

```markdown
Closes C-0007 (training mode). Branch from swarm/main.

- Reset implemented per card stop point
- Debug overlay behind a settings toggle, off by default

Evidence: run-tests.sh output and hash line quoted above; capture G1-06.png attached.
Reviewer: BUD. Seat: C-BUD generated, BUD reviewed line by line.
```

## CLAUDE.md block — root file, added at D−7

```markdown
# Manual
The RFL Production Manual (002/GSP/KT-XI-2026) is binding.
Read docs/manual/01-using-this-manual.md once per session.
Then read only the chapter named on the task card, and Chapter 2 when the card touches fight data.
MUST, SHOULD and MAY follow RFC 2119 (see section 1.6).
Never edit anything under docs/manual/.
If the manual is silent or contradicts itself, write the question in docs/decisions/open-questions.md and stop.
Never write a commit message that starts with "gate:" except a human approval. Humans approve gates.
/gate-check writes the pack and stops.
```

Paired deny rule in `.claude/settings.json`: `"Edit(./docs/manual/)"` is refused.

## Tuning data — `asset_src/data/tuning.json`

```json
{
  "damage_multiplier": 1.5,
  "rounds": { "count": 5, "first_to": 3, "seconds": 90, "pause_in_hitstop": true },
  "carry_over": { "hp": true, "parts": true, "stamina": true, "ko_floor_pct": 40 },
  "corner_break": { "heal_pct_of_lost": 30 },
  "stamina": { "pool": 1000, "regen_pct_per_s": 2.5, "regen_mult_blocking": 1.5, "low_line_pct": 20 },
  "parry": { "windows_f": [6, 5, 4], "lockout_f": 8, "fresh_press_only": true },
  "input": { "buffer_f": 8, "chord_f": 3, "double_tap_f": 12 },
  "hitstop": { "light_f": 4, "heavy_f": 8 },
  "pushback": { "block_m": 0.3, "hit_m": 0.4 },
  "groggy": { "duration_f": 40, "immunity_after_s": 3 },
  "overload": { "build_pct_on_clean_hit": [6, 20], "drain_pct_per_s": 3, "idle_before_drain_s": 1.5, "stall_f": 90, "extra_damage_pct": 25 },
  "juggle": { "cap_hits": 2, "cap_pct_of_max_hp": 20, "scaling_pct": [100, 80, 65, 50, 40, 30] },
  "parts": { "pools_pct": [35, 50, 25, 25, 35], "chip_on_block_pct": 15 },
  "ring": { "inside_m": 9.0, "opening_distance_m": 2.4 },
  "movement": { "walk_mps_middle": 1.6, "dash_m": 1.8, "dash_f": 16, "sidestep_dodge_frames": [4, 12] }
}
```

One value per field, matching the manual's tables; the importer prints each value with its
source row, so the sheet and the file cannot drift silently.

## Move data — `asset_src/data/moves.csv`

```csv
move_id,startup_f,active_f,recovery_f,hit_level,reach_m,on_block,on_hit,blockstun_f,damage,stamina_cost,clip_f,note
ST_JAB,8,2,10,M,0.6,+1,+8,12,45,0,20,punish reference: startup < 13 punishes blocked launcher
ST_LAUNCHER,15,3,28,H,0.7,-13,LAUNCH,17,60,0,46,juggle starts here; cap 2 hits in MVP
ST_THROW,6,2,18,T,0.9,,,,55,0,26,same-side punch+kick within 3 f; break within 12 f
ST_HAYMAKER,26,3,31,U,,,,,,,,unblockable example; sidestep frames 4-12 dodge it
```

Every shipped move row carries all columns; a `future` starter (POWER, SPEED) keeps its
rows with the status field set so the importer skips them. The note column names the
fastest punish or the defence that beats the move — no unanswerable move ships.

## EditMode test — sample

```csharp
[Test]
public void Mul_ThreeTimesTwo_IsSix()
{
    Fix a = Fix.FromInt(3);
    Fix b = Fix.FromInt(2);
    Assert.AreEqual(Fix.FromInt(6), a * b);
}
```

Every `Fix` operation gets at least one such test; the suite runs via
`tools/run-tests.sh` on Linux CI, each Mac and the Windows runner.
