# Story tickets — lead Rafif (RAF); values become data, never code

The story lane owns the game baseline: pitch, pillars, move list, fight rules, tuning set,
tutorial script, identity notes. Code holds the rules, data holds the values
(`tuning.json`, `moves.csv`); tuning happens in the day-plan tuning windows with evidence.

| ID | Title | Owner | Window | Depends | Done when |
|---|---|---|---|---|---|
| STO-001 | One-page pitch and three pillars with review tests | RAF | D−28→D−25 | — | Every feature review ends yes/no against one test: weight reads without sound or HUD; damage visible at a paused frame; an Indonesian detail named per feature |
| STO-002 | Move list: 14 normals, 2 strings, 1 launcher, 1 throw, 3 tech moves, full frame data | RAF | D−25→D−18 | STO-001 | Every row carries startup, active, recovery, hit level, reach, on block, on hit, damage, costs; clip lengths derivable from rows |
| STO-003 | Risk chart: plot every move by damage against on-block; name the fastest punish per row | RAF | D−18→D−14 | STO-002 | No move is both high damage and safe; one-two-roundhouse on the watch list; every move names at least one defence that beats it |
| STO-004 | Fight rules spec: best of 5, rounds of 90 s, KO/submission/time-out endings, carry-over, corner break, juggle cap 2 at 20% HP, parry fresh-press only (6/5/4 f, lockout 8 f), throw 3 f / break 12 f, submission lock read 30 f ±20 stamina | RAF | D−14→D−10 | STO-002 | Rules match Tables 2.4–2.6; round timer pauses during hit-stop; wake-up invulnerable first 6 f |
| STO-005 | Tuning set frozen as starting values: damage ×1.5, POWER ×1.10/+4 f, SPEED ×0.92/−1 f, HP 1550/1708/1386, stamina regen 2.5%/s, groggy 40 f, OVERLOAD stall 90 f +25%, hit-stop 4 f/8 f, pushback 0.3/0.4 m, part pools 35/50/25/25/35% | RAF | D−10→D−6 | STO-004 | Values entered in `tuning.json` + `moves.csv`; CI guard finds 0 tuning numbers in fight code |
| STO-006 | Tutorial script: five steps from movement to a short sparring round, with the sparring-unit partner | RAF | D−10→D−4 | STO-004 | A first-time player finishes all five steps without a hint; step strings ≤ 15 keys in ID and EN |
| STO-007 | Indonesian identity notes: league framing, ring place, two-language rule for every string | RAF | D−7→D0 | STO-001 | Every UI string exists in ID and EN; any feature that could sit unchanged anywhere else is cut or annotated with its Indonesian detail |
| STO-008 | Playtest protocol: forms, blind A/B of heavy hit-stop 8 f vs 6 f before G2, comeback-rate and time-out checks against the bands | RAF | before G2 | STO-005 | Hit-stop choice recorded with evidence in the decision log; time-outs ≈ 18% (flag > 30%), comeback rate ≈ 24%, OVERLOAD ≈ 2/match, mirror match ≤ 55% per side over 1,000 bot matches |
| STO-009 | Baseline freeze hand-off: approve importer report against Tables 2.4–2.6 | RAF + AND | D−2→D0 | STO-005, STO-006 | Andy commits `gate: G0 approved`; tag `baseline-1` on the G0 commit; POWER/SPEED rows marked future for the first update |
