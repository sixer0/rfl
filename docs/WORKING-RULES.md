# Working Rules — RFL

How the team and the Claude seats operate on this repo. Derived from Production Manual
`002/GSP/KT-XI-2026 Rev A` (Chapter 1 grammar, FM decisions, CD decisions). Binding for
humans and agents alike.

## Roles and seats

| Code | Who | Owns |
|---|---|---|
| RAF | Rafif | Game direction, design specs, tuning data, playtests, diff review with BUD |
| ORY | Ory | Art, animation, VFX, UI art, audio |
| BUD | Budi | Architecture, simulation core, tools, CI, builds, repository rules, triage |
| AND | Andy | Money, accounts, legal holder, Steamworks, gate approval |
| C-BUD / C-ORY / C-AND | Claude seat on that adult's account | Generates only for its holder's cards |

- The three Claude Ultra seats are held by Budi, Ory and Andy personally; Rafif never
  types into Claude — he decides, specifies and reviews with Budi while Budi's seat
  generates. Accounts are not shared (Claude requires adult users, checked Nov 2026).
- A combined owner like `BUD + C-BUD` means Budi drives and Budi's seat generates. The
  first code named in an owner cell is accountable for the step.
- A seat acts only for the adult who holds it; its output is reviewed by that adult.

## Hours and rhythm

- Days are relative: Phase 0 D−28→D0, production D1→D30, demo live D30, review D44.
- Adults: at most 9 hours a day, 54 a week, 6 days in 7, rest days staggered.
- Rafif: up to 3 hours a day supervised, outside school time; 6 hours only on a lawyer's
  written opinion. School dates are a MUST-ASK to the family by D−10.
- The project never stops: agents run queued cards overnight; the night queue loads at
  18:00 on each card owner's seat. Andy's seat runs only sessions Andy starts himself.

## Task cards

- Every ticket row becomes one card file `docs/cards/C-nnnn.md`.
- A card names: its owner code, the manual chapter it implements, its stop point, and
  the measurable check from the ticket's Done when line.
- Agents work only from cards. A diff that adds anything no card names is rejected; the
  question goes to `docs/decisions/open-questions.md` and the card stops.
- Progress page `docs/state/progress.md` is generated from card statuses by script,
  never typed by hand.

## Gates

- Eight gates: G0 ready to build (D0), G1 core loop (D6), G2 vertical slice (D12),
  G3 content complete alpha (D20), G4 feature freeze beta (D24), G5 release candidate
  (D28), G6 launch (D30), G7 review (D44).
- Only a human approves a gate: Andy commits `gate: Gn approved`, with Rafif as director.
  The gate auditor runs read-only on Andy's seat, writes `docs/reports/gate-Gn.md`, then
  stops. Agent-approved gates do not exist.
- Outcomes: pass, pass with conditions (SHOULD items only, at most 3), fix list, or stop.
- After feature freeze (G4) only S1 changes may land.

## Changes, decisions, questions

- Anyone — including a Claude agent via the open-questions file — may raise a change
  request. Budi triages within one working day: severity S1/S2/S3, whether it touches a
  framing decision, money, accounts, a minor's hours or a gate; copy to
  `docs/decisions/changes/CR-nn.md`.
- Decisions needing Andy: framing defaults, money, accounts, a minor's hours, gate
  approval. Recorded as `DN-nnn` in `docs/decisions/decision-log.md`: decider, options,
  evidence, choice, revisit condition.
- Severity timing: S1 same day and every reader told; S2 with the next daily build; S3
  held for Edition 2. Waiving a hero item or an S1/S2 defect needs Rafif and Andy together.
- Manual edits happen only on a `manual-cr-nn` branch after an approved CR; the folder
  `docs/manual/` is deny-edited in `.claude/settings.json`. The PDF rebuilds from the
  same Markdown files so print and repository cannot drift.

## Evidence before done

- A ticket closes only when its Done when check ran and the output is quoted in the PR
  body or the gate pack. Tests run through `tools/run-tests.sh`; CI must be green.
- Where a playtest and a written value disagree, the playtest wins and the change goes
  through the CR path.
- Every generated or bought asset gets one row in
  `Assets/_Project/Art/Generated/PROVENANCE.md`: tool, prompt or reference image, licence,
  rights holder. Paid AI tools ship only with commercial terms, on adult accounts, rights
  assigned to the legal holder; every track passes a Content ID check; free tiers and
  beta features never ship.

## Reading paths

- First read by D−25: Rafif chapters 1, 2, 3, 5; Ory 1, 2, 5, 6; Budi 1, 2, 4, 5, 13; Andy
  sections 1.7–1.8 and chapters 2, 5. Then each role opens its working chapters per the
  manual's Table 1.5; agents load section 1.2, Chapter 2, and only the chapter on the card.
- One 60-minute review closes the first reading; each reader confirms in one line:
  "I have read my path and know my first three tasks."

## Repository pair workflow

- `rfl` is upstream: main is protected, merges only by pull request from `swarm`.
- `swarm` holds in-flight branches; rebase freely, never force-push `rfl` main.
- Commit style: imperative subject under 72 chars ("Add determinism hash test"), body
  links the card ID. Gate approvals use the exact `gate: Gn approved` form.
