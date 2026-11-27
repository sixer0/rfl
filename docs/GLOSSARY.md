# Glossary — RFL

Every term this board uses, one line each. Wordings follow Production Manual
`002/GSP/KT-XI-2026 Rev A`; where the manual gives a number, the number is here.

## People and seats

- **Team** — Budi and Rafif only. The repository is Rafif's practice ground for
  professional development work.
- **RAF** — Rafif, main developer trainee and game director: does the majority of the work (specs,
  tuning data, code, art, audio, playtests) and opens one pull request per card. Never
  types into Claude.
- **BUD** — Budi, planner and reviewer: plans the timeline and cards, reviews and merges
  pull requests, repository rules, triage, money, accounts, legal holder, gate approval.
- **C-BUD** — the Claude session on Budi's seat; supports planning and review, acts only for Budi.
- **Seat** — one Claude Ultra subscription held by one named adult; accounts are not shared.
- **Director** — Rafif for game matters; Budi is the gate for money and scope.

## Time and phases

- **D-notation** — relative days: Phase 0 runs D−28 to D0, production D1 to D30, launch
  watch D30 to D44. D0 is the day before D1. A day is a calendar day.
- **Phase 0** — the run-up: standards, locks, repository and pipeline setup, Steam onboarding.
- **Worked example** — D−28 = Sun 6 Dec 2026, D1 = Mon 4 Jan 2027, D30 = Tue 2 Feb 2027,
  D44 = Tue 16 Feb 2027. Shift the start by moving D−28; procedures never change.
- **Night queue** — cards queued to run overnight on each owner's seat; loads at 18:00 seat time.
- **Tuning window** — scheduled slot in the day plan where tuning data may change.

## Gates

- **G0 Ready to build (D0)** — people, machines, accounts, repository, baseline ready.
- **G1 Core loop (D6)** — punch-block-sidestep exchange fun in a grey box; ring size fixed.
- **G2 Vertical slice (D12)** — one complete match looks, sounds and feels like the demo.
- **G3 Content complete / alpha (D20)** — every demo asset and screen in the build, rough ok.
- **G4 Feature freeze / beta (D24)** — features finished; only fixes remain. Only S1 after here.
- **G5 Release candidate (D28)** — build could ship as it is.
- **G6 Launch (D30)** — demo live, stable, watched.
- **G7 Review (D44)** — what the first 14 days taught; next scope decided.
- **Gate outcome** — exactly one of: pass, pass with conditions (SHOULD items only, max 3),
  fix list, stop.
- **Evidence pack** — `docs/reports/gate-Gn.md`, written by the gate auditor, approved by a human commit.
- **Approve** — a human commits `gate: Gn approved`. Agents never approve gates.

## Identifiers

- **FM-nn** — framing decision (FM-01 to FM-12): team, seats, hours, hardware, demo scope,
  launch, calendar, toolchain, gates, owner codes, document control. Confirmed by Budi at G0.
- **CD-nn** — editorial consistency decision (CD-01 to CD-32): one rule per conflict between
  chapters; every chapter follows the numbered list.
- **DN-nnn** — new decision entry in `docs/decisions/decision-log.md`.
- **CR-nn** — change request file in `docs/decisions/changes/`.
- **C-nnnn** — task card file in `docs/cards/`, one per ticket row.
- **REL-nnn** — release tag number.
- **REQ-nnn** — requirement from the plan workbook (REQ-019a frame floor, REQ-019b latency).
- **RSK-nn** — risk-register row. **AST-nnn** — asset-ledger row.
- **S1 / S2 / S3** — change severity: same day / next build / next edition.

## Process terms

- **Binding** — the text is followed as written; deviations need an approved CR.
- **Blank beats wrong** — an unknown value stays empty with a note how to get it, never guessed.
- **Done when** — the measurable check that closes a ticket or step; a stranger must get the
  same result running it.
- **Stop point** — where a card halts and writes its question to the open-questions file.
- **Open questions** — `docs/decisions/open-questions.md`; agents write there when the manual
  is silent or contradicts itself, then stop the card.
- **Waiver** — skipping a SHOULD with a written reason; hero items and S1/S2 defects need
  Rafif and Budi together.
- **Grey box** — playable blockout without finished art, used to judge the loop.
- **Vertical slice** — one complete match at final quality in all channels.
- **Provenance** — one ledger row per generated asset: tool, prompt or reference, licence, holder.
- **MUST / SHOULD / MAY** — RFC 2119 words; checked as in CODING-STANDARD.md.

## Fight mechanics

- **Tick** — one simulation step; exactly 60 per second, independent of render frame rate.
- **Startup / active / recovery** — the three animation phases of a move in frames; clip
  length equals their sum.
- **Frame advantage** — who can act first after a move; positive favors the attacker.
- **On block / on hit** — advantage values after the move is blocked or lands.
- **Blockstun / hitstun** — frames the defender cannot act after blocking or being hit.
- **Guaranteed punish** — a move with startup shorter than the defender's advantage gap.
- **Frame trap** — follow-up with a gap just small enough that a pressed button gets counter-hit.
- **Counter-hit** — hitting during the opponent's startup; causes GROGGY for 40 f.
- **Groggy** — 40-frame stun after a counter-hit; immune 3 s after it ends.
- **OVERLOAD** — threat meter under pressure; full means a 90-f stall with +25% damage taken.
- **String** — fixed button sequence; shares clips with its single moves when timing matches.
- **Launcher** — move that pops the defender airborne; the punish reference is −13 on block.
- **Juggle** — airborne combo; capped at 2 hits and 20% of max HP in the MVP demo.
- **Throw** — same-side punch + kick within 3 f; breaks with a matching punch within 12 f.
- **Submission** — round win below 20% HP (or broken part below 40%); lock read matched
  within 30 f, stamina check ±20 points. Wins the round, not the match.
- **Parry** — fresh press of back or LT on highs and mids; window 6/5/4 f by weight class,
  lockout 8 f on a miss.
- **Sidestep** — lateral move dodging straight attacks on frames 4 to 12; costs stamina.
- **Pushback** — distance the defender slides: 0.3 m blocked, 0.4 m hit.
- **Corner break** — pressure at the ropes heals the defender 30% of lost HP; the comeback lever.
- **Carry-over** — HP, part damage and stamina persist between rounds; a KO'd robot starts
  the next round at ≥ 40% HP.
- **Hit-stop** — simulation freeze on impact: light 4 f, heavy 8 f pending the blind A/B;
  the round timer pauses, the renderer keeps drawing.
- **Part pools** — damage distribution head/torso/arm/arm/legs = 35/50/25/25/35% of HP;
  blocking an arm chip 15%.
- **Mirror match** — same robot type on both sides; the D30 demo format.
- **Livery** — colour scheme on the shared body; the dummy wears the sparring-unit livery.
- **Comeback rate** — share of matches won by the round-one loser; target about 24%.
- **Time-to-KO** — seconds to empty a health bar; about 113 s at the x1.5 multiplier.

## Engineering terms

- **Q48.16 (`Fix`)** — own fixed-point type: 64-bit integer, lowest 16 bits fraction, step
  about 0.015 mm. The only numeric type in the sim core.
- **Determinism** — same seed and inputs give the same result everywhere; proven by the
  hash test after 10,000 ticks on Mac and Windows CI.
- **MatchState** — all fight values in one block of about 448 bytes, seeded random inside.
- **Input frame** — the single 16-bit word per player per tick carrying button state; the
  CPU writes the same format.
- **Importer** — editor code that turns `tuning.json` and `moves.csv` into read-only tables
  at load and rejects clip lengths that do not match frame data.
- **CI grep guard** — build step failing when a tuning literal appears in sim code.
- **Self-hosted runner** — the Windows 11 PC that produces IL2CPP player builds.

## Audio and performance

- **LUFS / dBTP** — loudness units and true-peak ceiling: −24 LUFS home, −16 handheld,
  −14 trailer; peaks ≤ −1 dBTP.
- **Press-to-screen** — latency from button press to first visible change; ≤ 50 ms on the
  minimum PC.
- **1% low** — the frame-rate floor statistic; ≥ 58 fps (REQ-019a).
- **Trauma** — camera-shake accumulator; shake equals trauma squared, KO adds +1.0.
- **maxQueuedFrames = 1** — engine setting that keeps one frame queued for the latency budget.

## Assets and licensing

- **Content ID check** — every music track passes it before shipping.
- **Paid-tool rule** — paid AI tools only for shipped assets, on adult accounts, rights
  assigned to the legal holder; free tiers and beta features never ship.
- **CC0 / CC-BY** — asset licences; CC-BY requires crediting the creator.
- **Bought art (L2)** — robot art purchased rather than in-house, to fit the week's capacity.
