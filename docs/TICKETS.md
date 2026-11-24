# Docs tickets — lead Budi (BUD), gate records by Andy's seat (AND, C-AND)

Docs holds the timeline, the manual's Markdown source, the decision trail and the gate
evidence. Repository paths follow Chapter 13 exactly; the path table there is the only
list of paths. The progress page is generated from cards, never typed.

| ID | Title | Owner | Window | Depends | Done when |
|---|---|---|---|---|---|
| DOC-001 | Repository skeleton: README, four workstream folders, `.gitignore`, `docs/` layout per Chapter 13 | BUD | D−28 | — | Fresh clone opens without warnings; four TICKETS.md present |
| DOC-002 | Import the Production Manual as Markdown under `docs/manual/`, one file per chapter (files 01–37) | BUD + C-BUD | D−27→D−25 | DOC-001 | Every chapter file loads; TOC links resolve; PDF rebuilds from the same files |
| DOC-003 | First reading round: each reader marks unclear lines, issues sorted by severity, one 60-minute review | ALL | D−26→D−25 | DOC-002 | Three "read and know my first three tasks" lines in the review minutes by D−25 |
| DOC-004 | Decision log and templates: `decision-log.md` (DN-nnn), `changes/CR-nn.md`, `open-questions.md`, task-card format C-nnnn | BUD | D−24→D−21 | DOC-001 | One test CR passes the full change path end to end by D−20; open-questions file exists and is empty at D0 |
| DOC-005 | Record the Director decisions and all 12 framing decisions with decider, options, evidence, choice, revisit condition | AND + BUD | D−21→D0 | DOC-004 | Entries exist for the four Table 1.11 items by their dates; G0 pack shows 12 of 12 FM rows confirmed or dated |
| DOC-006 | CLAUDE.md manual block + deny-edit rule in `.claude/settings.json` | BUD + C-BUD | by D−7 | DOC-002 | A test session editing `docs/manual/` is refused by the settings file; sessions load only the card's chapter |
| DOC-007 | Gate evidence packs: `docs/reports/gate-G0.md` … `gate-G7.md`, written by the gate auditor on Andy's seat, then stop | AND + C-AND | each gate | DOC-005 | Every gate closes with a pack; approval is a human commit `gate: Gn approved` |
| DOC-008 | Generated progress page from task-card status, with census counts from the asset ledger | BUD + C-BUD | weekly from D1 | DOC-004 | Page rebuilt by script at each gate; no hand-typed counts |
| DOC-009 | Asset provenance: every generated or bought asset logged in `PROVENANCE.md` with tool, prompt/reference, licence, holder | ORY | ongoing from Phase 0 | DOC-001 | Every file under Generated has one provenance row; licence column filled for all shipped items |
| DOC-010 | Change-note log: what, why, impact, CR number per change; S3 held for Edition 2 | BUD | ongoing | DOC-004 | Index lists every merged CR with its note; `git log -- docs/manual/` shows only `manual-cr-` merges |
| DOC-011 | Facts register upkeep: re-check every "to verify" row before D1; close or date each | AND + BUD | before D1 | DOC-005 | Annex G status column has no undated open rows at G0 |
| DOC-012 | Hand-over pack at G7: review minutes, metrics summary, first-update scope, next-window plan | ALL | D44 | DOC-007 | G7 pack complete; paid-release plan references it as its base |
