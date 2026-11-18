---
name: explainer
description: Make a short narrated, animated explainer (a page in the Claude app that draws itself while a voice reads) from long or technical material - a plan, a report, a codebase, a pile of notes - so someone can get the overview in five minutes without reading it all. Use when asked for an explainer, a brief or a presentation of some material, or to distil something long for a person who is short of time or not a specialist.
---

# Explainers

An explainer is a four to six minute film that plays inside Claude as an artifact: plain vector drawings that change as a voice reads, in the manner of a good explainer video. The viewer can stop on a scene, ask a question about it, and pin a note that a later session reads. Nothing is rendered to video: it is one web page.

## What's in this folder

| | |
|---|---|
| `player/player.js`, `player/player.css` | The player: the stage, the bar of beats, Play, the voice, captions, the question box, the note box. The same for every explainer. |
| `player/kit.svg` | Stock drawings: an arrowhead and symbols (person, PC, cloud, eye, machine, check, lock, link, file, speaker, bubble, database, plug). |
| `player/shell.html` | The page around the stage. |
| `player/build.py` | Joins one explainer's content with the player into a single page. |
| `player/shots.js` | A picture of every beat, and contact sheets of twelve, to look at before publishing (needs Node and Playwright). |
| `examples/how-explainers-work/` | A whole explainer to copy from. |

An explainer's own content is three files in a folder:

- `data.json`: the page's title, who it's for (`audience`), the scenes (each a title, the beats' narration, and background for the question box), and where each claim comes from.
- `scenes.svg`: one group per scene, drawn on a stage 1600 wide and 900 tall.
- `draw.js` (optional): drawings easier to generate than to type.

## The shape

- **Scenes and beats.** Five to nine scenes; three to five beats to a scene; a beat is one or two sentences and **one change on the stage**. About 30 beats and 650 words is four and a half minutes. Stay under six minutes unless asked.
- **Every beat is a whole picture.** Each drawn thing says from which beat of its scene it shows, dims, takes the highlight colour, moves or goes. So any beat can be jumped to and is drawn right.
- **The voice leads.** The app's read-aloud speaks the beats and tells the page as each one starts; the page then shows that beat. With no voice, a timer and captions do the same.

## Making one

1. **Gather, and keep a ledger.** Read the material. List every number and every factual claim you mean to say, each with where it comes from. That list becomes the page's "Where each claim comes from". If two sources disagree, the newer wins and the ledger says so.
2. **Outline in scenes.** One idea per scene. Start from something the viewer already knows, put a concrete case before the general rule, and where there's a likely wrong assumption show it first and then correct it. End with what the viewer should do or decide.
3. **Write the narration for the ear.** Short sentences, said the way you'd say them across a desk. No lists, no brackets. Jargon gets a picture or a plain word, never a definition read aloud. The longer explanation goes in each scene's background (`ctx`), which only the question box uses.
4. **Draw.** Abstract each idea into a picture that could carry it without the words. Nothing decorative: if the voice never talks about a mark, it isn't on the stage. Point at the one thing being discussed with the highlight colour and dim the rest.
5. **Build:** `python3 player/build.py <content folder> <out.html>`.
6. **Look at every beat:** `node player/shots.js <out.html> <folder>`, then open each sheet (again with `dark` at the end for dark mode). Fix overlaps, text outside its box, and any beat where nothing visibly changes.
7. **Check every claim** in the narration against the ledger and the ledger against the material.
8. **Publish** the built page with the Artifact tool as a new artifact, with the capabilities `{"tts": {}, "sample": {}, "db": {}, "user": {}}` and a title that names the subject. If the page will be shared, keep the owner's notes private with the rules `{"path": "notes", "read": "admin", "write": "admin"}` and the same for `questions`.
9. **Say in two lines** what it covers and how long it is. Don't summarise it in the chat: the film is the summary.

## Drawing

The stage is 1600 by 900. The look is ink on paper with one highlight colour; change the colours and typefaces at the top of `player/player.css` to suit the project.

Classes: `s` an inked outline; `thin` a lighter line; `dash` dashed; `f` a washed box; `ink` a filled shape; `a` the highlight colour. Text is 34; `sm` is 29; `m` is small capitals in mono at 22 (about 14 wide a letter: count before placing a label); `i` italic; `big` a headline number.

| Attribute | Meaning (beats count from 0 within the scene) |
|---|---|
| `data-b="2"` | shows from beat 2 |
| `data-off="4"` | gone again from beat 4 |
| `data-dim="3"` | fades back from beat 3 |
| `data-acc="1"` | takes the highlight colour from beat 1 |
| `data-hot="2"` | takes the highlight colour at beat 2 only |
| `data-mv="2"` | moves by its own `--mx` and `--my` from beat 2 |

Everything animated also carries the class `el`. Add `pop` to grow in, `draw` to be drawn along its length (the path also needs its pathLength attribute set to 1), `fly` to arrive from `--mx`, `--my`. `--d` delays a thing inside its beat (`style="--d:.8s"`), so one beat can build in the order the sentence says it.

Use the kit's symbols with `<use href="#k-person">` and the like; colour one with `style="--k:var(--signal)"`.

## Passing one on

- **A public link** (the page's Share control): the drawings and captions play; the Claude voice is only offered to the owner's own account or organisation.
- **The file:** `python3 player/build.py <content folder> <out.html> --standalone` makes a complete web page that opens in any browser and reads with the computer's own voice.
- **In the receiver's own Claude:** they give Claude that file and ask it to publish it as an artifact with reading aloud, asking Claude and a database. Then the page is theirs.

## Traps

- Style rules don't reach inside a `<use>`: the kit's symbols carry their styles inline, and a new symbol must too, or it draws solid black.
- Colour changes need the stage's id in front (`#stage .el.s.acc`), or the base rule wins.
- The voice reads a line at a time and can't jump inside one. It takes at most 2,000 characters a line and 100 lines; the build stops a script that breaks either.
- The workspace can prove the drawings and the timing, not the voice. Say so when handing one over.
- Publish one self-contained page. Don't split the player into separate files beside it.

## What the viewer has said

Keep this section. When the person you make explainers for reacts (too fast, too bare, more of this), write it here in their words: it outranks the rules above.
