# Project Fighter intake — bringing Rafif's earlier work into rfl

Raised: 2026-11-27 by RAF, paired with AI-RAF (Claude Code, Claude Opus 5.5) · For: BUD
Status: proposal, waiting for Budi's cards and decisions

## Purpose

Before rfl existed, Rafif built a first playable slice in his own local repository
(`project fighter`, never pushed). This note maps that work to the rfl tickets so Budi
can write the cards, and lists every place it departs from the manual. Nothing moves
into rfl except through one card and one PR at a time, adapted to the manual as it lands.

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
| APP-003 | `Sim/Fix.cs`, the hasher inside `Sim/MatchState.cs` | **Drafted**: branch `app-003-fixed-point`, see below |
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
`.csproj` files, the planning pack, the research folder, `Game/DevTools/ShotRunner.cs`
(no ticket names it).

## Departures from the manual (CR candidates)

Each is fixed inside its own card unless Budi decides otherwise.

1. **Unity version.** Built on 6000.6.10f1; the standard says 6.3 LTS. 6000.3.28f1 is
   installed on Rafif's Mac and the APP-003 draft passes on it. Proposal: port to 6.3 LTS,
   no CR.
2. **Layout and assemblies.** Project sat in a subfolder with `Assets/RFL/Scripts/Sim/`.
   Proposal: follow Chapter 13 exactly (`Assets/RFL/Sim/` at the repo root), no CR.
3. **Values in code.** Move rows and robot stats are C# tables; balance constants sit in
   `Simulation.*.cs`. Several differ from the manual: jab startup 7 f (manual 8 f), light
   hit-stop 6 f (manual 4 f), juggle scaling 100/85/70/55/40 (manual 100/80/65/50/40/30).
   Proposal: STO-005 freezes the manual values as data; the old values are not carried over.
4. **Art licence and storage.** All robots and ring textures are AI-generated with local
   open models. Hunyuan3D-2.1's licence excludes the EU, UK and South Korea, and the FLUX
   licence caps company revenue. The manual says only paid tools with commercial terms
   ship, and lever L2 buys the robot art. Binaries also need Git LFS, which rfl does not
   have yet. Proposal: art stays out until Budi rules; it may serve as grey-box stand-ins
   only, never shipped.
5. **Scope.** Three robot types, Versus vs CPU, difficulty select and robot select were
   built. The MVP keeps one robot and Versus as a stub. Proposal: POWER and SPEED rows
   stored marked `future` (STO-005); the extra screens are left out.

## Proposed card order

APP-001 → APP-003 → APP-002 → APP-006 → STO-002, STO-005 → APP-005 → APP-007 → APP-008
→ APP-009 → APP-011 → APP-012 → APP-010 → APP-013. One branch and one PR per card. APP-003
is ready first but merges after APP-001, so Unity can compile it in rfl.

## APP-003 draft (ready for review once its card exists)

Branch `app-003-fixed-point`: `Fix` (Q48.16), `SeededRandom` (PCG32), `StateHasher`
(FNV-1a 64), 26 EditMode tests. Ported from `Fix.cs` and the hasher in `MatchState.cs`.
Helpers no card names yet (`FromRatio`, `FromMilli`, `Abs`, `Min`, `Max`, `Clamp`,
`RoundToInt`, `ToMilli`) were left out; they come back with the card that needs them.

## Decisions needed from Budi

| # | Decision | Proposal |
|---|---|---|
| 1 | Cards for the tickets above | Card order as proposed |
| 2 | Unity 6.3 LTS port, Chapter 13 layout | Yes, no CR needed |
| 3 | Old tuning values | Dropped; manual values via STO-005 |
| 4 | AI-generated art and Git LFS | Hold art; decide licence and LFS before ART cards |
| 5 | Three robots, Versus/CPU screens | Cut to MVP scope; POWER and SPEED stored `future` |
