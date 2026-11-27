# Working Rules — RFL

How the team and the Claude seats operate on this repo. Derived from Production Manual
`002/GSP/KT-XI-2026 Rev A` (Chapter 1 grammar, FM decisions, CD decisions). Binding for
humans and agents alike.

## Purpose and team

This repository is a practice project: it familiarises Rafif with professional,
team-grade development work: cards, branches, pull requests, review, gates and evidence.
The project is worked by two people only, Budi and Rafif. No one else contributes.

- **Rafif is the main developer trainee** and gives the project most of the working time:
  specs, tuning data, code, art and audio, playtests. He works hands-on, opens a pull request per card and responds to review.
- **Budi reviews and plans**: he plans the work timeline, writes and sequences the cards,
  reviews every pull request, merges into main and holds the gates. Budi's Claude seat
  supports that review and planning; it does not do Rafif's cards for him.

## Roles and seats

| Code | Who | Owns |
|---|---|---|
| RAF | Rafif | Main developer trainee and game director: design specs, tuning data, code, art, audio, playtests, one pull request per card |
| BUD | Budi | Timeline and card planning, pull-request review, merges, repository rules, triage, money, accounts, legal holder, Steamworks, gate approval |
| C-BUD | Claude seat on Budi's account | Planning drafts, review support and checks for Budi; acts only for Budi |

- There is one Claude Ultra seat, held by Budi personally. Rafif never types into
  Claude; he builds himself and Budi reviews. Accounts are not shared (Claude requires
  adult users, checked Nov 2026).
- A combined owner like `BUD + C-BUD` means Budi drives and his seat assists. The first
  code named in an owner cell is accountable for the step.
- The seat's output is reviewed by Budi before it reaches Rafif or the repository.
- Further people (for example art, audio or a producer) join only if the work needs
  them; each one is added here with an owner code before taking a card.

## Hours and rhythm

- Days are relative: Phase 0 D−28→D0, production D1→D30, demo live D30, review D44.
- Adults: at most 9 hours a day, 54 a week, 6 days in 7, rest days staggered.
- The project never stops: agents run queued cards overnight; the night queue loads at
  18:00 on Budi's seat, for planning and review tasks only.

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
- Only a human approves a gate: Budi commits `gate: Gn approved`, with Rafif as director.
  The gate auditor runs read-only on Budi's seat, writes `docs/reports/gate-Gn.md`, then
  stops. Agent-approved gates do not exist.
- Outcomes: pass, pass with conditions (SHOULD items only, at most 3), fix list, or stop.
- After feature freeze (G4) only S1 changes may land.

## Changes, decisions, questions

- Anyone — including a Claude agent via the open-questions file — may raise a change
  request. Budi triages within one working day: severity S1/S2/S3, whether it touches a
  framing decision, money, accounts or a gate; copy to
  `docs/decisions/changes/CR-nn.md`.
- Decisions needing Budi: framing defaults, money, accounts, gate approval. Recorded as `DN-nnn` in `docs/decisions/decision-log.md`: decider, options,
  evidence, choice, revisit condition.
- Severity timing: S1 same day and every reader told; S2 with the next daily build; S3
  held for Edition 2. Waiving a hero item or an S1/S2 defect needs Rafif and Budi together.
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

- First read by D−25: Rafif chapters 1, 2, 3, 5, 6; Budi 1, 2, 4, 5, 13 and sections
  1.7–1.8. Then each role opens its working chapters per the
  manual's Table 1.5; agents load section 1.2, Chapter 2, and only the chapter on the card.
- One 60-minute review closes the first reading; each reader confirms in one line:
  "I have read my path and know my first three tasks."

## Repository pair workflow

- `rfl` is the team repo: Rafif's branches live here, main is protected, merges only by
  pull request and only by Budi as admin. Rebase your own branch freely; never force-push main.
- `swarm` is information only: worked exclusively by the agent swarm as a benchmark
  against the team's output. The team never pushes, branches or merges there.
- Full commit, push, merge and tag rules: `docs/GIT-WORKFLOW.md`.
- Commit style: imperative subject under 72 chars ("Add determinism hash test"), body
  links the card ID. Gate approvals use the exact `gate: Gn approved` form.
