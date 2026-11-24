# Art tickets — MVP week milestone

Milestone (week of 24 Nov 2026): art supports exactly what the menu layout and training
mode show, nothing more. Assignees stay blank until the Monday stand-up. One ticket = one
task card `C-nnnn` in `docs/cards/`; tick boxes per line as work lands.

Counts for the week: 1 body + skeleton with sparring-unit livery, about 24 clips, 1 arena,
about 10 VFX, about 45 sound cues from 104 files, UI art for the MVP screens. Every clip's
length equals startup + active + recovery frames or the importer rejects it; clips play at
the exact simulation frame, no Animator state machine, no root motion.

## ART-001 · Art bible for the MVP
Window: day 1 · Depends: — · Done when: one A4 page signed by the director; every later asset is checked against it
Assignee:

- [ ] Palette: ring canvas, robot steel, HUD accent
- [ ] Material rules for the STANDARD body: metal plates, joints, paint
- [ ] Silhouette rule: hero robot reads at the side-camera distance
- [ ] One hero shot reference in the page

## ART-002 · STANDARD body and skeleton
Window: day 1–3 · Depends: ART-001 · Done when: FBX imports with one mesh, one rig, zero scale warnings; real size about 1.9 m
Assignee:

- [ ] Blockout body first; refine only after the grey box passes review
- [ ] One skeleton shared by all starter types
- [ ] Sparring-unit livery on the same body for the training dummy
- [ ] Pivot at foot base; import scale normalized

## ART-003 · Damage state visuals
Window: day 3–4 · Depends: ART-002 · Done when: a paused frame after round 2 shows the most damaged part without the HUD
Assignee:

- [ ] Five-part mask: head, torso, two arms, legs
- [ ] Cracked, spark and vent-open states per part
- [ ] OVERLOAD vent glow tied to the gauge value
- [ ] Break states slow that limb visibly

## ART-004 · Animation set A: locomotion and attacks
Window: day 2–5 · Depends: ART-002 · Done when: importer reports 0 rejected clips; each length matches its move row exactly
Assignee:

- [ ] Walk forward and back, sidestep, dash, crouch, get-up
- [ ] Jab, straight, hook, uppercut launcher, haymaker
- [ ] Hold key poses 3–4 frames on heavy attacks
- [ ] Write the travel distance per clip into the move row

## ART-005 · Animation set B: defence, reactions, tech
Window: day 5–7 · Depends: ART-004 · Done when: all about 24 demo clips import clean
Assignee:

- [ ] Block and parry poses
- [ ] Throw, escape and submission hold
- [ ] Hit reactions light/heavy, groggy, KO
- [ ] Three tech moves
- [ ] Share clips between a string and its single moves where timing matches both

## ART-006 · Arena: Malioboro underworld ring
Window: day 4–6 · Depends: ART-001 · Done when: ring is 9.0 m inside the ropes; one lightmap bake passes
Assignee:

- [ ] Ring floor, ropes, corner pads
- [ ] Static crowd cards behind the ropes
- [ ] Backdrop readable from the side camera
- [ ] Bake once; 60 fps with the hero robot in frame

## ART-007 · VFX
Window: day 5–7 · Depends: ART-003 · Done when: every effect reads at 60 fps; 1% low stays ≥ 58 fps
Assignee:

- [ ] Hit sparks light and heavy
- [ ] Block flash and parry flash
- [ ] Groggy stars and OVERLOAD vent burst
- [ ] Part-break debris and KO burst

## ART-008 · UI art for the MVP screens
Window: day 5–7 · Depends: ART-001 · Done when: smallest text ≥ 10.4 px at 1280x800 and readable on a handheld
Assignee:

- [ ] HUD bars, round pips, timer frame
- [ ] Menu frame and button styles
- [ ] Controller prompt icons for pad and keyboard
- [ ] Banner styles for round start and end

## ART-009 · Audio cues for the MVP
Window: day 6–7 · Depends: ART-007 · Done when: mix −24 LUFS home, peaks ≤ −1 dBTP; every cue ≤ 45 ms after picture
Assignee:

- [ ] Layered hits: transient + body + tail, 3–5 variants per layer
- [ ] Whooshes per limb weight
- [ ] Block, parry, throw, escape, submission clicks
- [ ] Part breaks, groggy thud, OVERLOAD chime
- [ ] Ring bell and crowd loop
- [ ] UI confirm, back and error clicks
- [ ] One menu loop track; paid-tool output on adult accounts with rights to the holder, Content ID check passes

Deferred after the MVP review: extra liveries, store capsules and screenshots, trailer
stills, extra arenas.
