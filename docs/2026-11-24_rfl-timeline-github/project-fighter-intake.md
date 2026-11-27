# Project Fighter intake — bringing Rafif's earlier work into rfl

Raised: 2026-11-27 by RAF, paired with AI-RAF (Claude Code, Claude Opus 5.5) · For: BUD
Status: proposal, waiting for Budi's cards and decisions

## Purpose

Before rfl existed, Rafif built a first playable slice in his own local repository
(`project fighter`, never pushed). This note maps that work to the rfl tickets so Budi
can write the cards, and lists every place it departs from the manual. The proven work is
up for review as four stacked draft PRs (see "Port PRs" below): ported as built and changed
only where the repository standard requires it. Each departure from the manual is then
fixed by its own card.

## What exists

| Part | Content | State |
|---|---|---|
| Unity project | ~7,300 lines of C# in 51 files: deterministic sim core, menus, grey-box fight, HUD, localization, build scripts | Built on Unity 6000.6.10f1; Rafif's sprint note of 2026-11-24 records 51 EditMode and 5 PlayMode tests passing and macOS and Windows builds made |
| Art | 3 rigged robots (STANDARD, POWER, SPEED) and a league ring, 1.4 GB sources, 124 MB in Unity | AI-generated locally (FLUX 3 Klein, Hunyuan3D-2.1); own asset ledger |
| Planning pack | GDD, PRD, technical spec, research, schedule model | Written against Rafif's own spec, not the manual |

Built and tested there: walk, crouch, dash, backdash, jump; X/Y/A/B limbs and one string;
block high/low; hit-stop; pushback; light/heavy/launch reactions; juggle cap 2; counter-hit
groggy; OVERLOAD stall; part damage with at most 2 broken; best of 5 at 90 s with
carry-over and corner break; title, main menu, settings (language ID/EN), pause, results.

Not built: parry, sidestep, submission, tech moves, CPU behaviour, animation clips (poses
are procedural), the seeded random generator, a tuning importer, `tools/run-tests.sh`.

## Mapping to tickets

| Ticket | Source files in `project fighter` | Port work |
|---|---|---|
| APP-001 | `ProjectSettings/`, `Packages/`, `Assets/Settings/`, scenes, asmdefs, `.gitignore`, `.gitattributes`, `Editor/ProjectSetup.cs` | New 6.3 LTS project at the repo root per Chapter 13; assembly `RFL.Authoring` becomes `RFL.Editor` |
| APP-002 | `Editor/BuildScript.cs` | Write `tools/run-tests.sh` and the CI jobs; nothing to port |
| APP-003 | `Sim/Fix.cs`, the hasher inside `Sim/MatchState.cs` | Ported in #6, see below |
| APP-004 | `MatchState.Hash()`, `SimulationTests.cs` | Add the 10,000-tick scripted run; none exists yet |
| APP-005 | `Editor/CsvTable.cs`, `Data/StarterMoves.cs`, `Data/StarterTypes.cs` | Values leave C# for `tuning.json` and `moves.csv`; importer written new |
| APP-006 | `Sim/InputFrame.cs`, `Game/Input/*`, input actions asset | Check the 16-bit frame layout and the buffer 8 / chord 3 / double-tap 12 values |
| APP-007 | `Sim/Simulation.cs`, `Simulation.Movement.cs`, `Simulation.Combat.cs` | Add parry and sidestep; constants move to data |
| APP-008 | `Sim/Simulation.Damage.cs`, `Sim/StatusRules.cs` | Align scaling and OVERLOAD values with the manual |
| APP-009 | `Sim/Simulation.Rounds.cs` | Add throw break and submission |
| APP-010 | `Game/GreyBox/*` | Check each ticket box (dummy presets, frame-data panel, input history, hitbox overlay, instant reset); build what is missing |
| APP-011 | `Game/UI/*`, `UI/*.uxml`, `Localization/`, `Data/Strings/ui_strings.csv`, `Game/Profile/*` | Cut to the MVP screens; add the shake slider, reduced motion and credits |
| APP-012 | `Game/GreyBox/FightHud.cs` | Check against the ticket: 400 ms damage chip, stamina and OVERLOAD bars, round pips, status icons |
| APP-013 | `Game/Audio/*` | UI cues only so far |
| STO-007 | `Data/Strings/ui_strings.csv` | Copy check in both languages |
| ART-002, ART-006 | `art/ROB/RFL_ROB_Standard`, `art/ARN/RFL_ARN_LeagueRing` | Held: see decision 4 |

Stays out of rfl: `Library/`, `Builds/`, `Logs/`, `UserSettings/`, `Review/`, the generated
`.csproj` files, the planning pack, the research folder, and `Editor/ArtImport.cs` (art
tooling, held with the art). `Game/DevTools/ShotRunner.cs` has no ticket but ships in #8,
because `GameServices` calls it.

## Departures from the manual (CR candidates)

Each is fixed inside its own card unless Budi decides otherwise.

1. **Unity version.** Built on 6000.6.10f1; the standard says 6.3 LTS. 6000.3.28f1 is
   installed on Rafif's Mac. Done in #5–#8: ported to 6000.3.28f1, all tests pass on it.
2. **Layout and assemblies.** Project sat in a subfolder with `Assets/RFL/Scripts/Sim/`.
   Done in #5–#8: Chapter 13 layout at the repo root, `RFL.Authoring` renamed `RFL.Editor`.
3. **Values in code.** Move rows and robot stats are C# tables; balance constants sit in
   `Simulation.*.cs`. Several differ from the manual: jab startup 7 f (manual 8 f), light
   hit-stop 6 f (manual 4 f), juggle scaling 100/85/70/55/40 (manual 100/80/65/50/40/30).
   #7 carries them as built. Proposal: STO-005 freezes the manual values as data and
   APP-005 loads them; the old values are not kept.
4. **Art licence and storage.** All robots and ring textures are AI-generated with local
   open models. Hunyuan3D-2.1's licence excludes the EU, UK and South Korea, and the FLUX
   licence caps company revenue. The manual says only paid tools with commercial terms
   ship, and lever L2 buys the robot art. Binaries also need Git LFS, which rfl does not
   have yet. Proposal: art stays out until Budi rules; it may serve as grey-box stand-ins
   only, never shipped.
5. **Scope.** Three robot types, Versus vs CPU, difficulty select and robot select were
   built, and #7 and #8 carry them as built. The MVP keeps one robot and Versus as a stub.
   Proposal: POWER and SPEED rows stored marked `future` (STO-005); the APP-011 card cuts
   the extra screens.

## Proposed card order

The port PRs merge first, in stack order #5 → #6 → #7 → #8. Then one card per fix:
APP-002 → STO-002, STO-005 → APP-005 → APP-004 → APP-007 (parry, sidestep) → APP-009
(submission) → APP-011 (MVP screens) → APP-010 → APP-012 → APP-013. One branch and one PR
per card.

## Port PRs (as built, repository standard applied)

| PR | Branch | Carries | RFL tests (Unity 6.3, macOS) |
|---|---|---|---|
| #5 | `app-001-unity-skeleton` | APP-001: settings, packages, URP assets, six assembly definitions | opens with 0 errors |
| #6 | `app-003-fixed-point-v2` | APP-003: `Fix`, `SeededRandom`, `StateHasher` (replaces #4) | 26 EditMode |
| #7 | `app-007-combat-core` | APP-006 frame, APP-007, APP-008, APP-009: one partial `Simulation` class | 70 EditMode |
| #8 | `app-010-training-and-menus` | APP-006 devices, APP-010 to APP-013, STO-007: one `RFL.Game` assembly | 84 EditMode, 5 PlayMode |

Each PR is based on the one before it, so its diff shows only its own slice, and each
passes on its own with only the PRs below it. The changes for the repository standard:
one type per file, Microsoft naming (symbol-aware renames), nullable enabled with zero
warnings, csharpier formatted. Planning-pack citations read `PF-REQ-nnn` so they cannot be
mistaken for manual REQ numbers. The profile save-file keys and the ring JSON keep their
spelling, with the reason commented at the field.

## Decisions needed from Budi

| # | Decision | Proposal |
|---|---|---|
| 1 | Cards for the tickets above | Card order as proposed |
| 2 | Unity 6.3 LTS port, Chapter 13 layout | Done in #5–#8; confirm, no CR needed |
| 3 | Old tuning values | Dropped; manual values via STO-005 |
| 4 | AI-generated art and Git LFS | Hold art; decide licence and LFS before ART cards |
| 5 | Three robots, Versus/CPU screens | Cut to MVP scope; POWER and SPEED stored `future` |
