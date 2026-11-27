# SDLC Mandatory Rules and Workflow — RFL

Binding for the whole development window (D−28 to D44) and every Claude session on
Budi's seat. Words MUST / SHOULD / MAY follow RFC 2119 as defined in CODING-STANDARD.md.
These rules decide what a gate checks; WORKING-RULES.md holds the surrounding process
detail, TIMELINE.md the dates. A rule here is broken only with a written waiver from Rafif
and Budi together (see rule 17).

## Mandatory rules

### Order of work

1. Work follows the phase order: Phase 0 (D−28→D0), Production (D1→D30), Launch watch
   (D30→D44). No production task starts before G0 passes.
2. Every gate runs on its day: G0 D0, G1 D6, G2 D12, G3 D20, G4 D24, G5 D28, G6 D30,
   G7 D44. Nothing skips a gate and nothing merges past an unheld gate without Budi's
   written stop decision.
3. Only a human approves a gate: Budi commits `gate: Gn approved`, Rafif co-signs as
   director. Agents never approve gates; the gate auditor writes the pack and stops.
4. A gate ends in exactly one outcome: pass, pass with conditions (SHOULD items only,
   maximum 3), fix list, or stop. More than three conditions becomes a fix list.
5. After feature freeze (G4) only S1 changes may land. Everything else waits for Edition 2.

### Cards and sessions

6. Every unit of work starts from one task card `docs/cards/C-nnnn.md`. No card, no work.
   The card names its ticket row, the manual chapter it implements, its stop point and its
   measurable check.
7. A session loads, in this order: section 1.2, Chapter 2 when the card touches fight
   data, then only the chapter named on the card. It reads the repository copy under
   `docs/manual/`, never a pasted excerpt.
8. When the manual is silent or contradicts itself, the agent writes a dated line to
   `docs/decisions/open-questions.md` and stops the card. Guessing is forbidden: blank
   beats wrong.
9. A diff that adds behaviour no card names is rejected at review; the question goes to
   open-questions, not into the diff.

### Evidence and checks

10. Nothing is called done without fresh evidence: the check output quoted in the PR body
    or the gate pack. A check that did not run is reported as not run.
11. Tests run only through `tools/run-tests.sh`; CI must be green on Linux (GameCI), each
    Mac, and the Windows IL2CPP self-hosted runner before merge.
12. The determinism hash MUST be equal on Mac and Windows after 10,000 ticks with the same
    seed and inputs. Unequal hash blocks merge until fixed.
13. Where a playtest and a written value disagree, the playtest wins; the correction goes
    through the CR path with its evidence attached.

### Data and code

14. Code holds the rules, data holds the values. Every tuning number lives in
    `asset_src/data/tuning.json` or `moves.csv`, one value per field. The CI grep guard
    fails any numeric tuning literal inside `Assets/RFL/Sim/`.
15. Tuning changes land only inside the tuning windows of the day plan, one change per
    retest, and bump the data version by 1.
16. Animation clips MUST match their move rows: clip length = startup + active + recovery,
    played at the exact simulation frame. The importer rejects mismatches.

### Changes and decisions

17. Any change to scope, locked values or standards goes through the CR path: raise →
    Budi triages within one working day (severity S1/S2/S3, needs-Budi yes/no) → written
    decision → branch → merged change note. Waiving a hero item or an S1/S2 defect needs
    Rafif and Budi together, in writing.
18. Decisions are recorded as `DN-nnn` in `docs/decisions/decision-log.md` with decider,
    options, evidence, choice and revisit condition; newest on top. An unrecorded decision
    does not exist.
19. Manual files under `docs/manual/` are edited only on a `manual-cr-nn` branch after an
    approved CR; the deny rule in `.claude/settings.json` enforces it. The PDF rebuilds
    from the same Markdown so print and repository cannot drift.

### Hygiene

20. No secret ever enters a commit, report or log line: name the location and type, the
    holder rotates it. Accounts stay with their adult holder; seats are not shared.
21. Every generated or bought asset gets its `PROVENANCE.md` row in the same commit that
    adds the file; licence column never empty; music passes a Content ID check.
22. Commit style follows STRUCTURE.md: imperative subject under 72 chars, body links the
    card, check output quoted. Only human gate approvals start with `gate:`.
23. `rfl` main is protected: branches live on `rfl`, merges only by pull request and
    only by the admin, no force-push to main. `swarm` is an agent-only benchmark; the team
    never pushes there. Link check runs at every gate; a broken relative link is an S2 CR.

## Workflow

### Phase flow

| Stage | Window | Entry | Exit criterion |
|---|---|---|---|
| Phase 0 run-up | D−28→D0 | Budi answers the four Director decisions of TIMELINE.md | G0 pack: baseline tagged `baseline-1`, CI green, cards exist, seats verified |
| Core loop build | D1→D6 | G0 passed | G1 pack: exchange reads as fun in grey box; ring fixed at 9.0 m |
| Vertical slice | D7→D12 | G1 passed | G2 pack: one complete match at final quality; hit-stop A/B settled |
| Content to alpha | D13→D20 | G2 passed | G3 pack: census counts complete, rough ok; first demo build to Valve |
| Beta and freeze | D21→D24 | G3 passed | G4 pack: feature freeze commit; only S1 after here |
| Release candidate | D25→D28 | G4 passed | G5 pack: RC build could ship as it is; Deck capture in pack |
| Launch | D29→D30 | G5 passed | G6 pack: demo live on Steam, watched, zero errors in Player.log |
| Review | D31→D44 | Demo live | G7 pack: 14-day metrics, fix log, first-update scope decided |

### Daily loop (every working day, D1→D30)

1. Morning: regenerate `docs/state/progress.md` from card statuses (script, never typed).
2. Pick the next card on your lane; open its session with the loads of rule 7.
3. Implement against the card's checklist boxes, one box per commit where possible.
4. Run `tools/run-tests.sh`; paste the tail into the PR body.
5. Budi reviews Rafif's diff line by line; reject anything outside the card.
6. Budi merges; Rafif ticks the boxes in the card; set status done or in-progress.
7. Evening: Budi plans the next card; planning and review tasks may queue on Budi's seat
   at 18:00 and run overnight.

### Gate-day loop

1. Auditor on Budi's seat writes `docs/reports/gate-Gn.md`: checks with quoted outputs,
   counts versus census, captures, proposed outcome.
2. Humans decide: Budi with Rafif approve, or issue the fix list, or stop.
3. Approval commit carries `gate: Gn approved`; conditions or fixes get dated owners.
4. Next stage starts only after the pack exists; an unheld gate pauses merges, not thinking.

### Stop conditions (any of these halts the line until fixed)

- Determinism hash unequal across machines (rule 12).
- Importer rejecting shipped clip rows (rule 16).
- Open S1 change request without a dated owner (rule 17).
- Director decision from TIMELINE.md unanswered past its due day (rule of Table 1.11).

## Enforcement

- Each gate pack lists these rules by number with pass/fail; a fail without waiver blocks
  the gate outcome to fix list at best.
- Severity mapping: breaking rules 1–5 or 10–13 is S1; rules 6–9, 14–16, 20–23 is S2;
  wording-only slips are S3.
- The rules themselves change only through the CR path of rule 17.
