#!/usr/bin/env python3
"""Build one explainer page from the shared player and one explainer's content.

usage: python3 build.py <content_dir> <out.html> [--standalone]
<content_dir> holds data.json (title, script, sources), scenes.svg (one <g class="scene"> per scene)
and, optionally, draw.js (generates repetitive drawings; defines EXPLAINER.draw(mk, byId)).
The result is a single self-contained file to publish with the Artifact tool,
with capabilities {"tts": {}, "sample": {}, "db": {}}.
With --standalone the result is a complete web page instead: it opens in any browser by itself
and reads with the computer's own voice. That is the copy to send to someone.
"""
import html, json, pathlib, sys

here = pathlib.Path(__file__).resolve().parent
src, out = pathlib.Path(sys.argv[1]), pathlib.Path(sys.argv[2])
data = json.loads((src / "data.json").read_text(encoding="utf-8"))

problems = []
scenes_svg = (src / "scenes.svg").read_text(encoding="utf-8")
if scenes_svg.count('class="scene"') != len(data["scenes"]):
    problems.append("scenes.svg has %d scene groups, data.json has %d scenes"
                    % (scenes_svg.count('class="scene"'), len(data["scenes"])))
words = 0
for i, s in enumerate(data["scenes"]):
    for j, b in enumerate(s["beats"]):
        words += len(b.split())
        if len(b) > 2000:
            problems.append("scene %d beat %d is over 2,000 characters (the voice's limit per line)" % (i + 1, j + 1))
if sum(len(s["beats"]) for s in data["scenes"]) > 100:
    problems.append("over 100 beats (the voice's limit per reading)")
if problems:
    sys.exit("not built:\n- " + "\n- ".join(problems))

draw = src / "draw.js"
page = (here / "shell.html").read_text(encoding="utf-8")
for key, val in {
    "{{PAGETITLE}}": html.escape(data["pageTitle"]),
    "{{KICKER}}": html.escape(data["kicker"]),
    "{{TITLE}}": html.escape(data["title"]),
    "{{INTRO}}": html.escape(data["intro"]),
    "{{CSS}}": (here / "player.css").read_text(encoding="utf-8"),
    "{{KIT}}": (here / "kit.svg").read_text(encoding="utf-8"),
    "{{SCENES}}": scenes_svg,
    "{{DATA}}": json.dumps(data, ensure_ascii=False).replace("</", "<\\/"),
    "{{DRAW}}": draw.read_text(encoding="utf-8") if draw.exists() else "",
    "{{JS}}": (here / "player.js").read_text(encoding="utf-8"),
}.items():
    page = page.replace(key, val)
if "--standalone" in sys.argv:
    page = ('<!doctype html>\n<html lang="en"><head><meta charset="utf-8">'
            '<meta name="viewport" content="width=device-width,initial-scale=1">'
            '<style>body{margin:0}img{max-width:100%}[hidden]{display:none!important}</style></head><body>\n'
            + page + '\n</body></html>\n')
out.write_text(page, encoding="utf-8")
beats = sum(len(s["beats"]) for s in data["scenes"])
print("built %s: %d scenes, %d beats, %d words (about %.1f minutes), %d KB"
      % (out.name, len(data["scenes"]), beats, words, words / 140.0, len(page.encode("utf-8")) // 1024))
