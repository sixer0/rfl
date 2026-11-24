# Docs tickets — MVP week milestone

Milestone (week of 24 Nov 2026): docs carries the board mechanics around the MVP: cards,
decision trail, evidence pack, provenance. Repository paths follow Chapter 13 exactly;
the progress page is generated from cards, never typed. Assignees stay blank until the
Monday stand-up. One ticket = one task card `C-nnnn` in `docs/cards/`; tick boxes per line.

## DOC-001 · Repository skeleton and board
Window: day 1 · Depends: — · Done when: fresh clone opens without warnings; four TICKETS.md present
Assignee:

- [x] Four workstream folders with one TICKETS.md each
- [x] README with structure and milestone statement
- [x] Push upstream to rfl, mirror to swarm
- [ ] Link the board from the repo description

## DOC-002 · Task cards from tickets
Window: day 1 · Depends: — · Done when: every ticket row has one card file naming its chapter and stop point
Assignee:

- [ ] One file per ticket under docs/cards/ as C-nnnn
- [ ] Card names the manual chapter it implements
- [ ] Card names its stop point and its check
- [ ] Night queue loads cards at 18:00 owner-seat time

## DOC-003 · Decision trail
Window: day 1–2 · Depends: — · Done when: one test CR passes the full change path end to end
Assignee:

- [ ] docs/decisions/decision-log.md with DN-nnn entries
- [ ] docs/decisions/changes/CR-nn.md template
- [ ] docs/decisions/open-questions.md exists and stays empty at day close
- [ ] Change note line for every merged CR: what, why, impact, CR number

## DOC-004 · Agent configuration
Window: day 2 · Depends: DOC-002 · Done when: a test session editing docs/manual/ is refused by the settings file
Assignee:

- [ ] CLAUDE.md manual block per Listing 1.1
- [ ] Deny Edit(./docs/manual/) in .claude/settings.json
- [ ] Sessions load section 1.2, Chapter 2, and only the card's chapter
- [ ] Gate approval stays human: agents write the pack and stop

## DOC-005 · Evidence pack template and MVP pack
Window: day 3, filled at day 7 · Depends: DOC-003 · Done when: docs/reports/gate-MVP.md holds counts, captures and a human outcome
Assignee:

- [ ] Pack template: counts, captures, outcome, approver line
- [ ] Counts versus census: clips, cues, string keys, screens
- [ ] Profiler capture link from APP-014
- [ ] Outcome committed by a human: pass, pass with conditions, fix list or stop

## DOC-006 · Generated progress page
Window: day 2 onward, every morning · Depends: DOC-002 · Done when: page rebuilt by script; no hand-typed counts
Assignee:

- [ ] Script counts card statuses in docs/cards/
- [ ] Runs each morning on the owner's seat
- [ ] Writes docs/state/progress.md

## DOC-007 · Asset provenance
Window: ongoing from day 2 · Depends: — · Done when: every generated or bought asset has one row
Assignee:

- [ ] Assets/_Project/Art/Generated/PROVENANCE.md created
- [ ] Row per file: tool, prompt or reference, licence, rights holder
- [ ] Licence filled for every shipped item
