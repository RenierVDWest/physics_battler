# Brief for the subagent that makes a 3b2b

Renier's rule: the explainer is made by a subagent on **Opus at medium effort** (Agent tool: `model: "opus"`,
`effort: "medium"`), never on the main model. The subagent starts cold, so the brief has to stand alone. This is
the brief that worked on 19 November 2026; fill in the parts in CAPITALS. Write the update note first (the note you
would have sent him) and save it as a file: that file is the film's material.

---

Make a "3b2b": a short narrated, animated explainer that plays as one self-contained web page, published as an
artifact. It is for Renier, a game developer, about A STEP UPDATE / A PLAN on his Unity game "Physics Battler". You
start cold; everything you need is below.

## Where things are (absolute paths)
- The kit: REPO/Tools/Explainer/kit/ (read its SKILL.md first, and player/build.py, player/shots.js,
  player/player.js, player/shell.html, player/kit.svg).
- This project's own example to copy the format from: REPO/Tools/Explainer/example-lane/ (note.md, script.py,
  gen.py, helpers.py, data.json, scenes.svg). Generate scenes.svg and data.json from script.py + gen.py +
  helpers.py as it does.
- Work in a NEW folder outside the repository: WORKFOLDER. Do not write anything inside the repository (do not
  commit, push or change it).

## The material (authoritative)
- The note this film tells: NOTEFILE. Read it whole. Every number you say must come from it.
- Background only: the README entry "HEADING" near the end of REPO/README.md (search for the heading; never read
  the README whole). If the two disagree, the note wins.

## Audience and length
Renier himself. He asked to be told things simply, "as if I'm not smart and not really following things too
closely". Plain words, the high-level picture first, no jargon. About three minutes for a step update (5 or 6
scenes, 20 to 24 beats, 420 to 520 words; under four minutes); about five for a plan (6 to 8 scenes, about 30
beats, 600 to 700 words; under six). A beat is one or two sentences and one change on the stage.

## What it must do
LIST, IN ORDER, WHAT EACH SCENE HAS TO TELL: start from what he saw or already knows; one concrete picture that
moves for the main idea (old against new); the numbers, honestly; what it does not do yet; end on what he should do
(pull, which lab, which buttons, by their exact names on the panel) and one line on what is next.

Keep three kinds of claim visibly apart, in words and with a tag on the stage: measured in his log or said by him
(Unity); headless only (and "built blind" where nothing could draw it); planned, not built.

## Making it (follow the kit's SKILL.md; the essentials)
- Content is three files: data.json (pageTitle, kicker, title, intro, audience, scenes each with a title, the
  beats' narration and ctx = background only the question box uses, and sources = each claim and where it comes
  from), scenes.svg (one `<g class="scene">` per scene on a stage 1600 wide, 900 tall), and the generator.
- Keep a ledger of every number and claim with where it comes from in the note; that becomes "sources".
- Narration is for the ear: short sentences, as across a desk. No lists, no brackets.
- Drawing: plain ink on paper with one highlight colour. Nothing decorative. Every beat is a whole picture and every
  beat changes something visible. Simple stick or capsule figures for fighters, a disc for the shield, a line for
  the sword. Classes: s, thin, dash, f, ink, a. Text is 34; sm 29; m is small mono capitals at 22 (about 14 wide a
  letter: count before placing a label); i italic; big for a headline number. Attributes (beats count from 0 within
  the scene): data-b shows from that beat; data-off gone from; data-dim fades back from; data-acc takes the
  highlight from; data-hot highlight at that beat only; data-mv moves by its own --mx, --my from. Everything
  animated carries class el; add pop, draw (the path needs pathLength="1") or fly; --d delays a thing inside its
  beat.
- Traps: style rules do not reach inside a `<use>`; a narration line is at most 2,000 characters and there are at
  most 100 lines; a line that spans the stage runs under labels placed later; an inline opacity overrides the
  hidden state of a later beat.
- Build: `python3 <kit>/player/build.py <content folder> <out.html>`.
- Look at every beat: `node <kit>/player/shots.js <out.html> <folder>` (needs Node and Playwright; install if
  missing), open each contact sheet with the Read tool and really look, then again with `dark` at the end. Fix
  overlaps, text outside its box, labels on lines, beats where nothing changes. Rebuild and look again until clean.
- Check every claim in the narration against the ledger and the ledger against the note. Soften any word the
  numbers do not support.

## Publishing
Publish the built single page as a NEW artifact with the artifact tools (names begin `mcp__claude-artifacts__`;
load their schemas with ToolSearch, e.g. "select:mcp__claude-artifacts__publish_files,
mcp__claude-artifacts__describe_capabilities,mcp__claude-artifacts__declare_capabilities,
mcp__claude-artifacts__db_list,mcp__claude-artifacts__guide"). Before publishing call the guide with topic "pages"
and call describe_capabilities, because the page uses the capabilities `{"tts": {}, "sample": {}, "db": {},
"user": {}}`. The page must begin with `<!DOCTYPE html>`. Title: "Physics Battler: SUBJECT (DATE)". Icon word:
film. One self-contained index.html. After publishing, list the `notes` collection once to confirm the store
answers. Do not share it or change who can see it.

## Rules
Do not spawn subagents. Do not put any API key or email address into any file. Do not create Unity .meta files and
do not touch the repository.

## What to return
Only this: the published artifact's link; the running time and the number of scenes and beats; what you checked
(light and dark sheets looked at, claims checked against the note); what you could not check (the voice, the
question box and saving notes cannot be tried in the workspace). No summary of the content.

---

After it reports: the update to Renier is the film's link first, then a short note. Say that the voice, the question
box and the notes could not be tried. It takes about twelve minutes; wait for it before sending the update.
