# RFL Master Timeline — Edition 1

Derived from Production Manual `002/GSP/KT-XI-2026 Rev A`. Relative days; the worked
calendar example puts D−28 on Sun 6 Dec 2026, D1 on Mon 4 Jan 2027, D30 on Tue 2 Feb 2027,
review D44 on Tue 16 Feb 2027. Move the start by shifting D−28; no procedure changes.

## Phases

| Phase | Window | Purpose | Exit |
|---|---|---|---|
| Phase 0 — run-up | D−28 → D0 | Standards, concept lock, scope lock, repository + Unity + pipeline setup, Steam onboarding, 13 of the demo clips | G0 ready to build |
| Production | D1 → D30 | Build the D30 demo: core loop, systems, combat feel, remaining art and audio, integration, QA | G6 launch |
| Launch watch | D30 → D44 | Demo watched, wishlist ladder, fixes, first-update planning | G7 review |

## Gates

| Gate | Day | Question the gate answers | Approver |
|---|---|---|---|
| G0 Ready to build | D0 | People, machines, accounts, repository and baseline ready for D1? All 12 framing decisions confirmed | AND + RAF |
| G1 Core loop | D6 | Is the punch-block-sidestep exchange fun in a grey box? Ring size fixed here | AND + RAF |
| G2 Vertical slice | D12 | Does one complete match look, sound and feel like the demo? Heavy hit-stop A/B settled | AND + RAF |
| G3 Content complete, alpha | D20 | Every demo asset and screen in the build, even rough? First demo build to Valve | AND + RAF |
| G4 Feature freeze, beta | D24 | Every feature finished, so only fixes remain? | AND + RAF |
| G5 Release candidate | D28 | Could this build ship as it is? Final demo build went at D26 | AND + RAF |
| G6 Launch | D30 | Demo live, stable, watched | AND + RAF |
| G7 Post-launch review | D44 | What did the first 14 days teach; what comes next | AND + RAF |

Gate outcomes are one of: pass, pass with conditions (SHOULD items only, max 3), fix list, stop.
Evidence lives in `docs/reports/gate-Gn.md`.

## Steam track (fixed days)

| Day | Step |
|---|---|
| D−28 | Steamworks onboarding starts; legal holder named (paperwork can take 15–30 days) |
| D−5 | Final game name and trademark search closed (Decision #60) |
| D7 | Store page submitted to Valve |
| D16 | Page public as Coming Soon; wishlist ladder T1–T5 starts |
| D20 | First demo build to Valve review (at G3) |
| D26 | Final demo build to Valve |
| D30 | Free demo live |
| post-D44 | Paid release at a Steam event chosen from wishlist results (Steam Fighting Fest 26 Apr–3 May 2027 or a Next Fest edition; dates to verify) |

## Workstream lanes

- **story/** — specs and tuning data must exist before the system that reads them: pitch and
  pillars by D−25, move list by D−18, tuning set by D−10, tutorial script by D−4. Baseline
  enters data files D−2, frozen at G0.
- **apps/** — skeleton and CI from D−28; simulation core and determinism through Phase 0;
  combat systems D−8 → D4; integration, HUD, localization D8 → D20; performance and Deck
  check D20 → D26; RC and launch D26 → D30.
- **art/** — art bible and body in Phase 0 (13 clips made there under lever L1); remaining
  clips D5 → D16; VFX, UI art, audio D8 → D22; store assets by D20.
- **docs/** — repository skeleton D−28, manual import D−27→D−25, CLAUDE.md block D−7,
  decision log open from D−21, evidence packs at every gate, provenance ongoing.

## Capacity model

The demo as first framed (3 starters, ~67 clips, 3 difficulties) needs about 186 working
days and does not fit. The plan ships the smaller D30 demo: one robot type, mirror match,
full combat systems, ~24 clips, Versus, practice room, 5-step tutorial. It fits only if
three levers are pulled by D−21:

| Lever | Content | Owner |
|---|---|---|
| L1 | Budi and Ory work full time in Phase 0 (13 clips made before D1) | BUD, ORY |
| L2 | Robot art is bought, not in-house | AND |
| L3 | Usage credits back the three Claude seats | AND |

With L1–L3 the demo fits in about 64% of simulated runs; Andy pre-approving cuts 1–5 at
D−21 raises it to about 80%. Hours cap: adults 9 h/day, 54 h/week, 6 days in 7; Rafif up to
3 h/day in holiday (6 h only on a lawyer's written opinion); agents run queued cards overnight.

## Cut list (pre-approvable at D−21, in order)

1. Practice-room frame-data display (−20 string keys)
2. Second livery → single livery with colour-tint variant
3. Crowd cards → flat backdrop
4. Menu loop → reuse fight track at lower layer count
5. Tutorial step 5 (short sparring round) → four steps

## Risk register (top)

| ID | Risk | Early sign | Response |
|---|---|---|---|
| RSK-01 | Scope grows one reasonable item at a time | A card names Story, Ladder or named robots | Refuse at triage; any addition names what it replaces |
| RSK-02 | Steam paperwork overruns | Onboarding not started at D−28 | Holder named day one; bank name matches exactly |
| RSK-03 | A Claude seat lost | Cards pile on one seat overnight | Redistribute at next gate; queue stays per-owner |
| RSK-04 | School term limits Rafif hours D31–D44 | No school dates by D−10 | MUST-ASK family; day plan follows school-term limits |
| RSK-05 | Manual drifts from repository copy | Diff between PDF and `docs/manual/` | Rebuild from Markdown at every gate |
| RSK-06 | Hit-stop or shake causes discomfort | Testers report nausea in blind A/B | Halve amplitude, lengthen duration; ship the slider |

## Director decisions needed (Table 1.11)

| # | Decision | Default | Needed by |
|---|---|---|---|
| 1 | Who holds Steam, accounts, IP | Existing PT Andy names; Andy personally as fallback | D−28 |
| 2 | Who holds the three Claude seats | Budi, Ory, Andy, each in their own name; never Rafif | D−28 |
| 3 | Rafif's lawful hours and school dates | Up to 3 h/day supervised; 6 h only on lawyer's opinion | D−10 |
| 4 | Scope, levers, pre-approved cuts | D30 demo, L1–L3, cuts 1–5 | D−21 |
