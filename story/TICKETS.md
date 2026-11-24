# Story tickets — MVP week milestone

Milestone (week of 24 Nov 2026): story owns the specs and tuning data behind the menu
layout and training mode. Values become data (`tuning.json`, `moves.csv`), never code;
tuning happens in the tuning windows with evidence. Assignees stay blank until the Monday
stand-up. One ticket = one task card `C-nnnn` in `docs/cards/`; tick boxes per line.

## STO-001 · Pitch and pillars one-pager
Window: day 1 · Depends: — · Done when: every feature review this week ends yes/no against one pillar test
Assignee:

- [ ] Three-sentence pitch from the approved workbook
- [ ] Weight and impact test: a heavy hit reads from motion alone with sound muted
- [ ] Damage-you-can-see test: name the most damaged part at a paused frame without the HUD
- [ ] Indonesian identity test: name the detail a screen carries; if none, cut or annotate

## STO-002 · Move list with full frame data
Window: day 1–2 · Depends: STO-001 · Done when: every row complete; clip lengths derivable from rows alone
Assignee:

- [ ] 14 normals, 2 strings, 1 launcher, 1 throw, 3 tech moves
- [ ] Columns: startup, active, recovery, hit level, reach, on block, on hit, damage, stamina cost
- [ ] Launcher at −13 on block, punished by the 8 f jab
- [ ] Haymaker as the unblockable example against sidestep

## STO-003 · Risk chart and punish notes
Window: day 2–3 · Depends: STO-002 · Done when: no move is both high damage and safe; every move names one defence that beats it
Assignee:

- [ ] Plot damage against on-block for every move
- [ ] Name the fastest punish per row, or mark safe
- [ ] Flag the one-two-roundhouse on the watch list
- [ ] Check the chart has no unanswerable move

## STO-004 · Fight rules spec
Window: day 3 · Depends: STO-002 · Done when: rules match manual Tables 2.4–2.6 with no gaps
Assignee:

- [ ] Best of 5, first to 3, rounds of 90 s
- [ ] Round ends on KO, submission, or larger share of max HP at time-out; no judges
- [ ] HP, part damage and stamina carry between rounds
- [ ] Corner break heals 30% of lost HP; KO floor 40%
- [ ] Juggle cap 2 hits at 20% of max HP
- [ ] Parry fresh press only, 6/5/4 f by weight, lockout 8 f
- [ ] Round timer pauses during hit-stop; wake-up invulnerable first 6 f

## STO-005 · Tuning set frozen as data
Window: day 3–4 · Depends: STO-004 · Done when: values live in tuning.json and moves.csv; CI guard finds 0 tuning numbers in fight code
Assignee:

- [ ] Damage x1.5; STANDARD HP 1550, stamina pool 1000
- [ ] Stamina regen 2.5%/s, x1.5 blocking; low-stamina line 20%
- [ ] Groggy 40 f; OVERLOAD build 6–20%, drain 3%/s after 1.5 s, stall 90 f +25% damage taken
- [ ] Hit-stop light 4 f / heavy 8 f; pushback 0.3/0.4 m
- [ ] Part pools 35/50/25/25/35%; arm chip on block 15%
- [ ] POWER and SPEED rows stored marked future for the first update

## STO-006 · Training-mode spec
Window: day 4 · Depends: STO-004 · Done when: every base mechanic has a named way to show it inside training mode
Assignee:

- [ ] Infinite-round toggle
- [ ] Dummy presets: idle, blocks seen hits, punishes minus moves
- [ ] Frame-data panel fields: startup, active, recovery, on block, on hit
- [ ] Input history strip for both sides
- [ ] Instant reset behaviour defined
- [ ] Walk the APP-007 to APP-009 list and confirm each mechanic is demonstrable

## STO-007 · Menu copy in ID and EN
Window: day 5–6 · Depends: STO-001 · Done when: string-table test reports 0 missing keys in either language
Assignee:

- [ ] Title, mode names, settings labels
- [ ] Pause and credits lines
- [ ] Move names shown in the frame-data panel
- [ ] Controller prompt wording for pad and keyboard
- [ ] Every string in Bahasa Indonesia and English

## STO-008 · MVP playtest protocol
Window: day 7 · Depends: STO-006 · Done when: the review meeting runs on this form only
Assignee:

- [ ] One scripted round per pillar with yes/no notes
- [ ] Time-out rate about 18% over 1,000 bot matches (flag over 30%)
- [ ] Comeback rate about 24%
- [ ] OVERLOAD triggers about 2 per match (flag under 1)
- [ ] Mirror match wins ≤ 55% per side
- [ ] Optional blind A/B: heavy hit-stop 8 f vs 6 f, recorded with evidence
