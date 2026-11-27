# Apps tickets — MVP week milestone

Milestone (week of 24 Nov 2026): game menu layout + training mode carrying every base
mechanic. Assignees stay blank until the Monday stand-up. One ticket = one task card
`C-nnnn` in `docs/cards/`; tick the checklist boxes per line as work lands; the
"Done when" line is the acceptance check for the review pack.

Stack: Unity 6.3 LTS + URP, C#. Own deterministic core: Q48.16 fixed-point, 60 ticks/s,
one MatchState block (~448 bytes), no engine physics, no floats or wall clocks in the core.
Tick budget ≤ 0.30 ms on the minimum PC (4-core, GTX 1060/RX 580 class, 8 GB).
REQ-019a: 1% low ≥ 58 fps. REQ-019b: press-to-screen ≤ 50 ms. Tests run through
`tools/run-tests.sh`; CI: GameCI Linux + per-Mac + Windows IL2CPP self-hosted runner.

## APP-001 · Unity project skeleton
Window: day 1 · Depends: — · Done when: project opens clean on Mac and Windows PC, zero console errors, boot scene shows camera + light
Assignee:

- [ ] Create Unity 6.3 LTS project, URP active
- [ ] Assembly definitions per Chapter 13 paths (RFL.Sim, RFL.Data, RFL.Game, RFL.Editor)
- [ ] Boot scene: Main Camera + Directional Light
- [ ] Target frame rate 60, fixed timestep 1/60
- [ ] Linear color space, HDR on
- [ ] Verify on the Windows 11 PC: opens, loads boot scene, zero errors

## APP-002 · Test runner and CI
Window: day 1–2 · Depends: APP-001 · Done when: every push runs all suites; a failing suite blocks merge
Assignee:

- [ ] tools/run-tests.sh runs the EditMode suite headless
- [ ] GitHub Actions workflow with GameCI on Linux runners
- [ ] Per-Mac check job and Windows IL2CPP self-hosted job
- [ ] Status badge on repo home
- [ ] Branch protection: checks must pass before merge

## APP-003 · Q48.16 fixed-point type
Window: day 2 · Depends: APP-001 · Done when: EditMode tests green on Mac and Windows; no floats in core code
Assignee:

- [ ] Fix struct: fromInt, fromRaw, add, sub, mul, div, compare, toString
- [ ] Seeded deterministic RNG; no UnityEngine.Random or System.Random in core
- [ ] EditMode test per operation
- [ ] State-hash helper for snapshots

## APP-004 · Determinism guard
Window: day 2–3 · Depends: APP-003 · Done when: same seed + inputs give equal hash after 10,000 ticks on Mac and Windows CI
Assignee:

- [ ] Scripted match: fixed input frames, 10,000 ticks
- [ ] Emit state hash at tick 10,000
- [ ] CI compares hashes across both machines
- [ ] Result line stored in the MVP evidence pack

## APP-005 · Data importer
Window: day 3 · Depends: APP-003, STO-002, STO-005 · Done when: importer exits 0; report lists every value with source file and row
Assignee:

- [ ] Read tuning.json and moves.csv into read-only tables at load
- [ ] Validate clip length = startup + active + recovery per row
- [ ] Reject fixture: one bad clip length fails the import
- [ ] Import report prints file and row per value
- [ ] CI grep guard: 0 tuning numbers in Assets/RFL/Sim/

## APP-006 · Input layer
Window: day 3 · Depends: APP-003 · Done when: pad and keyboard produce identical frames for the same presses; CPU writes the same format
Assignee:

- [ ] One 16-bit input frame per player per tick
- [ ] Input polled once per tick, right before the sim step
- [ ] Buffer 8 f, chord 3 f, double-tap 12 f
- [ ] CPU opponent writes the same frame format

## APP-007 · Combat mechanics core
Window: day 3–4 · Depends: APP-005, APP-006 · Done when: punch-block-sidestep exchange reads as fun in a grey box
Assignee:

- [ ] Health and stamina pools; regen 2.5%/s, x1.5 while blocking
- [ ] Block with arm chip 15% of hit
- [ ] Parry: fresh press back/LT only, highs and mids, window 6/5/4 f, lockout 8 f
- [ ] Sidestep dodges straights on frames 4–12; dash 1.8 m in 16 f; steps and dashes cost stamina, walk/duck/block free
- [ ] Hit-stop: light 4 f, heavy 8 f, both robots freeze, render keeps drawing, round timer pauses
- [ ] Pushback: block 0.3 m, hit 0.4 m

## APP-008 · Damage model
Window: day 4 · Depends: APP-007 · Done when: a paused frame after round 2 shows the most damaged part without the HUD
Assignee:

- [ ] Part pools 35/50/25/25/35% of HP; a broken part slows that limb
- [ ] All-damage multiplier x1.5
- [ ] Groggy 40 f on counter-hit during startup; immune 3 s after
- [ ] OVERLOAD builds 6–20% on clean hits, drains 3%/s after 1.5 s unhit, stalls 90 f with +25% damage taken
- [ ] Juggle scaling 100/80/65/50/40/30%; cap 2 hits and 20% of max HP
- [ ] Corner break heals 30% of lost HP; KO'd robot starts the next round at ≥ 40% HP

## APP-009 · Round and match flow
Window: day 4–5 · Depends: APP-008 · Done when: a full best-of-5 ends on KO, submission or time-out with carry-over
Assignee:

- [ ] Best of 5, first to 3, rounds of 90 s
- [ ] Round ends on KO, submission, or larger share of max HP at time-out; no judges
- [ ] HP, part damage and stamina carry between rounds
- [ ] Submission: lock read, defender matches direction within 30 f, stamina check ±20 points
- [ ] Throw: same-side punch + kick within 3 f; break with matching punch within 12 f
- [ ] Wake-up invulnerable for the first 6 f

## APP-010 · Training mode
Window: day 5–6 · Depends: APP-007 to APP-009 · Done when: every base mechanic is demonstrable inside training mode, one by one
Assignee:

- [ ] Infinite-round toggle (timer off)
- [ ] Dummy presets: idle, blocks seen hits, punishes minus moves
- [ ] Frame-data panel: startup, active, recovery, on block, on hit per move
- [ ] Input history strip for both sides
- [ ] Hitbox/hurtbox debug overlay
- [ ] Instant reset: positions, meters, parts

## APP-011 · Menu layout
Window: day 5–6 · Depends: APP-001 · Done when: boot → title → mode select → settings → pause → credits all reachable; smallest text ≥ 10.4 px at 1280x800
Assignee:

- [ ] Title screen with league framing
- [ ] Mode select: Training first choice, Versus stub visible
- [ ] Settings: screen-shake slider 0–100%, reduced-motion sets 30%, language ID/EN
- [ ] Pause overlay with resume and quit
- [ ] Credits strip
- [ ] Controller prompts on every screen

## APP-012 · HUD
Window: day 6 · Depends: APP-011 · Done when: HUD readable through a full match without hiding the fight read
Assignee:

- [ ] Health bars with damage chip, drained after 400 ms
- [ ] Stamina bar; OVERLOAD gauge with vent glow
- [ ] Round pips and round timer
- [ ] Groggy and OVERLOAD status icons

## APP-013 · Audio integration hooks
Window: day 6–7 · Depends: APP-010, ART-009 · Done when: every cue lands ≤ 45 ms after picture in a captured playtest
Assignee:

- [ ] Play layered hit sounds from simulation events
- [ ] Pitch variation ±2 semitones on repeated cues
- [ ] Whooshes tied to startup frames
- [ ] Menu and UI click cues

## APP-014 · Performance pass
Window: day 7 · Depends: APP-013 · Done when: profiler capture meets budget on the minimum PC
Assignee:

- [ ] Tick ≤ 0.30 ms on a GTX 1060/RX 580 class machine
- [ ] 60 fps steady; 1% low ≥ 58
- [ ] Press-to-screen ≤ 50 ms (maxQueuedFrames=1, vsync on, no triple buffering)
- [ ] Frame-time graph flat through hit-stop

## APP-015 · MVP build and review pack
Window: day 7 · Depends: APP-014 · Done when: Budi reviews the build and commits the outcome
Assignee:

- [ ] IL2CPP Windows build from CI
- [ ] Smoke test: boot → training → full match → quit, zero errors in Player.log
- [ ] Evidence pack docs/reports/gate-MVP.md with counts and captures
- [ ] Review ends in pass, pass with conditions, fix list or stop
