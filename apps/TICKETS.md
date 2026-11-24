# Apps tickets — lead Budi (BUD), generation on C-BUD

Unity 6.3 LTS + URP, C#. The fight core is own deterministic code: Q48.16 fixed-point,
60 ticks/s, one MatchState block (~448 bytes), no engine physics, no floats or wall clocks
in the core. Tick budget ≤ 0.30 ms on the minimum PC (4-core, GTX 1060/RX 580 class, 8 GB).
REQ-019a: 1% low ≥ 58 fps. REQ-019b: press-to-screen ≤ 50 ms. Tests run through
`tools/run-tests.sh` on GitHub Linux runners (GameCI) and each Mac; the Windows 11 PC is a
self-hosted runner that builds the IL2CPP player.

| ID | Title | Owner | Window | Depends | Done when |
|---|---|---|---|---|---|
| APP-001 | Unity project skeleton: URP, assembly definitions per Chapter 13 paths, fixed display settings | BUD + C-BUD | D−28→D−21 | — | Project opens clean on Mac and Windows PC; zero console errors; scene with camera + light loads |
| APP-002 | CI pipeline: GameCI on Linux runners, per-Mac run, Windows IL2CPP self-hosted job, `tools/run-tests.sh` as the single entry | BUD + C-BUD | D−21→D−16 | APP-001 | Push runs all suites; badges on repo home; failing suite blocks merge |
| APP-003 | Q48.16 fixed-point type with EditMode tests on every operation | BUD + C-BUD | D−18→D−12 | APP-001 | Full test suite green on Mac and Windows; no UnityEngine.Random or System.Random in core |
| APP-004 | Determinism guard: same seed + same inputs → equal state hash after 10,000 ticks | BUD + C-BUD | D−12→D−9 | APP-003 | Hash equal on both machines in CI; result stored in the G0 pack |
| APP-005 | Data importer: `tuning.json` + `moves.csv` → read-only tables at load; validates clip length = startup+active+recovery | BUD + C-BUD | D−10→D−6 | APP-003 | Importer exits 0; report lists every value with source file and row; rejects a bad clip length in a fixture test |
| APP-006 | Input layer: one 16-bit frame per player per tick, buffer 8 f, chord 3 f, double-tap 12 f; polled once per tick before the sim step | BUD + C-BUD | D−9→D−4 | APP-003 | Pad and keyboard produce identical frames for the same presses; CPU writes the same format |
| APP-007 | Combat systems: health, stamina, block/parry, groggy, OVERLOAD, part damage, throw, corner break, juggle cap 2 | BUD + C-BUD | D−8→D4 | APP-005, APP-006 | Grey box passes G1: punch-block-sidestep exchange reads as fun; round ends on KO, submission or time-out per spec |
| APP-008 | Animation driver: plays clips at the exact sim frame, no Animator state machine, no root motion; travel distance from move data | BUD + C-BUD | D2→D6 | APP-007, ART-006 | Every clip in build matches its row; 0 importer rejections at G2 |
| APP-009 | CPU opponent: one profile, presets Easy/Normal/Hard moving only reaction (23/22/21 ticks), blocking (43/48/50%), aggression (45/50/55%); shuffle-bag habits with tells | BUD + C-BUD | D4→D10 | APP-007 | Damage identical across presets by rule; bot-vs-bot over 1,000 matches inside the bands of STO-008 |
| APP-010 | HUD + menus: bars, banners, pause, settings (screen-shake slider 0–100%, reduced-motion at 30%); all 16 screens | BUD + C-BUD | D8→D14 | APP-008 | Every screen reachable from boot to credits; string-table test reports 0 missing ID/EN strings |
| APP-011 | Tutorial + practice room wired to the story script (5 steps; frame-data display optional per cut list #1) | BUD + C-BUD | D12→D16 | APP-010, STO-006 | First-time player completes the five steps without a hint |
| APP-012 | Audio integration: layered hits, whooshes, parts, ring, UI, music; ≤ 45 ms after picture, ≤ 20 ms before | BUD + C-BUD | D14→D18 | APP-010, ART-010 | Cue count matches Chapter 19 list (45); sync limits hold in a captured playtest |
| APP-013 | Localisation: ID + EN tables, 265–300 keys, fallback rules | BUD + C-BUD | D16→D20 | APP-010 | Both locales complete; switch works at runtime |
| APP-014 | Performance pass: tick ≤ 0.30 ms, 60 fps on minimum PC, 1% low ≥ 58, press-to-screen ≤ 50 ms (maxQueuedFrames=1, vsync on, no triple buffering) | BUD | D20→D24 | APP-012, APP-013 | Profiler capture in the G3 pack meets budget; frame-time graph flat through hit-stop |
| APP-015 | Steam Deck check: 60 fps target, no Verified pass, text ≥ 10.4 px at 1280×800 | BUD | D22→D26 | APP-014 | Deck capture in the G5 pack |
| APP-016 | Release candidate: REL-nnn versioning, demo builds to Valve at D20 and D26, store page live by D16 | BUD + AND | D26→D30 | APP-014, APP-015 | G6: demo live on Steam, stable, watched; zero errors in Player.log on both machines |
| APP-017 | Post-launch fix window: triage reports D30→D44, S1 same day, others with the next build | BUD + C-BUD | D30→D44 | APP-016 | G7 pack lists every fix with its report ID; first-update scope decided at G7 |
