# Art tickets — lead Ory (ORY), generation on C-ORY

Counts come from the manual census (Table 2.8): 1 body, 3 liveries, ~21 moves in ~24 clips,
1 arena, 10 VFX, 45 sound cues from 104 files, 2 music tracks, 9 store assets. Every clip's
length equals startup + active + recovery frames or the importer rejects it; clips play at
the exact simulation frame, no Animator state machine, no root motion.

| ID | Title | Owner | Window | Depends | Done when |
|---|---|---|---|---|---|
| ART-001 | Art bible: palette, material rules, silhouette rules, one hero shot | ORY | D−28→D−24 | — | One A4 page signed by Rafif; every later asset is checked against it |
| ART-002 | STANDARD robot body + one skeleton (shared by all starter types) | ORY + C-ORY | D−24→D−16 | ART-001 | FBX imports with one mesh, one rig, zero scale warnings; appears in Unity at real size |
| ART-003 | Liveries: STANDARD in two colour schemes + sparring-unit livery for the tutorial | ORY | D−16→D−12 | ART-002 | Three variants render side by side in a test scene; names in ID and EN |
| ART-004 | Damage shader: 5-part mask (head, torso, two arms, legs) with cracked/spark/overload states | ORY + C-ORY | D−14→D−8 | ART-002 | Pause any frame after round 2: a viewer names the most damaged part without the HUD |
| ART-005 | Arena: Malioboro underworld ring, ropes, corners, static crowd cards | ORY | D−10→D−4 | ART-001 | Ring is 9.0 m inside the ropes; one lightmap bake passes; runs 60 fps with the hero robot |
| ART-006 | Animation set A: locomotion + first eight attacks (13 clips, made in Phase 0 under lever L1) | ORY + C-ORY | D−7→D0 | ART-002 | Importer reports 0 rejected clips; each length matches its move row exactly |
| ART-007 | Animation set B: defence, throw, hit reactions, tech moves, KO (about 11 clips) | ORY + C-ORY | D5→D16 | ART-006 | All about 24 demo clips in the build; string clips shared only where timing matches both moves |
| ART-008 | VFX: hits, block, parry, Groggy, OVERLOAD, part breaks, KO (10 effects) | ORY | D8→D18 | ART-004 | Each effect reads at 60 fps on the minimum PC; 1% low stays ≥ 58 fps |
| ART-009 | UI art: HUD bars, banners, menu frame, controller prompts | ORY | D10→D18 | ART-001 | Smallest text is ≥ 10.4 px at 1280×800 and readable on Steam Deck |
| ART-010 | Audio pass: layered hits (3–5 variants per layer, ±2 semitone pitch variation), whooshes, parts, ring bell/ropes/crowd, UI cues; 1 fight track + 1 menu loop | ORY + C-ORY | D12→D22 | ART-008 | Mix −24 LUFS home / −16 handheld, peaks ≤ −1 dBTP; every cue ≤ 45 ms after picture; paid-tool output on adult accounts with rights to the holder, Content ID check passes |
| ART-011 | Store assets: capsules, screenshots, trailer stills (9 items) | ORY | D14→D20 | ART-005 | Nine items exist at Steam sizes; page review at D7 uses only these |
| ART-012 | Asset ledger finalisation: every census row has AST ID, owner, target day, licence note | ORY | D22→D24 | ART-007, ART-010 | G3 count from `ledger.csv` matches Table 2.8 with 0 missing rows; frozen at G4 |
