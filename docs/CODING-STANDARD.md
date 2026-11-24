# Coding Standard — RFL

Binding for every commit in this repo, human or Claude seat. Derived from Production
Manual `002/GSP/KT-XI-2026 Rev A` (FM-09, Chapters 13/15/16); Chapter 13 Table 13.2 is
the authority for exact repository paths and assembly names. Requirement words follow
RFC 2119: MUST = hard requirement checked at the next gate; SHOULD = strong default,
waivable with a written reason; MAY = free choice.

## Stack and targets

- Unity 6.3 LTS, Universal Render Pipeline, C#. Patch changes only at gates.
- Fight logic runs in the team's own deterministic code at fixed 60 ticks/s.
- Minimum PC: 4-core CPU, GTX 1060 or RX 580 class, 8 GB RAM, 1080p Medium.
- Budgets: simulation tick ≤ 0.30 ms on the minimum PC (REQ-019 set); 1% low frame rate
  ≥ 58 fps (REQ-019a); press-to-screen ≤ 50 ms (REQ-019b); Steam Deck targets 60 fps.

## Repository layout

- `Assets/RFL/Sim/` — deterministic fight core. Assembly `RFL.Sim` references no
  UnityEngine types.
- `RFL.Data` — read-only tables built by the importer at load.
- `RFL.Game` — presentation, UI, audio glue, scene wiring.
- `RFL.Editor` — importer and editor tooling.
- `asset_src/` — source data: `data/tuning.json`, `data/moves.csv`, `ledger.csv`.
- `tools/run-tests.sh` — the single test entry point; CI and every local check call it.
- `docs/` — manual (`docs/manual/`), cards, decisions, reports, state. See WORKING-RULES.md.

## Simulation core (determinism rules)

These eight rules keep the same inputs producing the same result on every machine:

1. Fixed tick: exactly 60 steps per second behind an accumulator in the game loop. No
   `Time.deltaTime` inside the core.
2. Numbers: own Q48.16 fixed-point type (`Fix`) — a 64-bit integer, lowest 16 bits
   fraction, step about 0.015 mm. Floats are banned in the core.
3. No engine physics: hit and hurt boxes are own code; PhysX never decides a hit.
4. Random: one seeded generator passed into the match. No `UnityEngine.Random`, no
   `System.Random` in core code.
5. Collections: iterate arrays or sorted lists; never rely on dictionary order.
6. Inputs as data: one 16-bit input frame per player per tick; the CPU writes the same
   format a human input produces.
7. State: all fight values in one `MatchState` block (about 448 bytes), seeded random
   inside it. No static mutable fields.
8. Proof: same seed and inputs give the same state hash after 10,000 ticks, on the Mac
   and on the Windows PC. This runs in CI on every push.

## Data and tuning

Code holds the rules; data holds the values; playtest numbers decide them.

- Every tuning number lives in `asset_src/data/tuning.json` or `moves.csv`, one value per
  field. Changing a value is a data edit plus an importer run — never a code edit.
- A CI grep guard fails the build when a numeric tuning literal appears in
  `Assets/RFL/Sim/` outside constants files. Target: 0.
- Every move row MUST carry: startup, active, recovery, hit level, reach, on block,
  on hit, damage, stamina cost. A future-update robot keeps its rows marked `future`;
  the importer skips them.
- The importer validates: clip length = startup + active + recovery frames, or the clip
  is rejected. It prints a report listing every value with source file and row.
- Tuning changes land only inside the tuning windows of the day plan, with evidence
  (playtest form, bot-vs-bot run, bug ID), and bump the data version by 1.

## Gameplay implementation

- Animation: clips play at the exact simulation frame. No Animator state machine, no root
  motion. The animator keys the body in place and records travel distance; the move row
  carries it. Strings share a single-move clip only when timing matches both.
- Hit-stop freezes the simulation of both robots and pauses the round timer; the renderer
  keeps drawing. Light hits freeze 2–6 frames; heavy keeps 8 frames until a blind test
  against 6 frames settles it before G2. The frame-time graph stays flat through hit-stop.
- Juggles stop after 2 hits in the MVP demo, capped at 20% of max HP; a robot hitting the
  ropes during a juggle rebounds once per combo, never more.
- Camera shake uses trauma squared; on tester discomfort halve the amplitude and lengthen
  the duration. Settings ship a shake slider 0–100% and a reduced-motion switch at 30%.
- CPU difficulty changes how the CPU reads and reacts — reaction delay, blocking share,
  aggression — never its damage multiplier, which stays x1.0 for all presets. Habit
  variety comes from a shuffle bag, not pure chance.

## Audio

- Sync limits: audio after picture ≤ 45 ms; audio before picture ≤ 20 ms.
- Loudness: home mix −24 LUFS, handheld −16, trailer −14; peaks at or below −1 dBTP.
- Repeated cues get ±2 semitone pitch variation. Cue inventory follows the Chapter 19
  cue list; other chapters quote that count, never re-count it.

## UI and text

- Smallest text ≥ 10.4 px at 1280x800 (Steam Deck check).
- Every string exists in Bahasa Indonesia and English from day one; an EditMode
  string-table test fails on any missing key in either locale.
- "Blank beats wrong": an unknown value stays empty with a note how to get it, never guessed.

## Engine settings for the latency budget

`QualitySettings.maxQueuedFrames = 1`; VSync on, no triple buffering; input polled once
per tick just before the simulation step; rendering may run faster than 60 Hz but only
draws the latest state.

## C# style

- Follow Microsoft C# naming: PascalCase types/members, camelCase locals and parameters,
  `_camelCase` private fields, `IInterface` prefix. One type per file, file named after
  the type.
- Format with standalone `csharpier`; the CI runs it in check mode. No hand-aligned
  reformatting in feature diffs.
- Nullable reference types enabled; prefer `readonly` on immutable fields and struct
  `Fix` arithmetic helpers over heap wrappers.
- Public API surface stays minimal: sim exposes `MatchState`, `Fix`, tick entry, input
  frame; everything else internal.

## Tests

- Every `Fix` operation gets EditMode tests; run through `tools/run-tests.sh`.
- Determinism hash test on Mac and Windows in CI.
- Importer reject fixture: one deliberately bad clip length must fail the import.
- String-table test for ID/EN completeness.
- Check results are quoted in the PR body and the gate pack; "works on my machine" is
  not evidence.
