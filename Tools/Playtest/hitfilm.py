#!/usr/bin/env python3
"""hitfilm.py <folder> <out.mp4> <blow> [setup ...]

Draws the hit experiment's blows (the plan "A different approach", step 3) as a film: the body as a stick figure
from the side and from the front, one row a setup, in slow motion (a quarter speed) from just before the ball meets
the body to a second after. <folder> is what `HIT_FILM=<folder> ... physx-hit` wrote; <blow> and the setups are
parts of the files' names (for the blow: "chest_from_the_front__3_kg_at_8").
"""
import csv, glob, os, subprocess, sys, tempfile
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

BONES = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"),
         ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
         ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"),
         ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]
LEFT = {"UpperArmL", "ForearmL", "HandL", "ThighL", "ShinL", "FootL"}


def read(path):
    steps = {}
    with open(path) as f:
        for row in csv.DictReader(f):
            step = int(row["step"])
            steps.setdefault(step, {"met": int(row["met"]), "at": {}})["at"][row["thing"]] = (float(row["x"]), float(row["y"]), float(row["z"]))
    return steps


def main():
    if len(sys.argv) < 4:
        print(__doc__)
        return 1
    folder, out, blow = sys.argv[1], sys.argv[2], sys.argv[3]
    wanted = sys.argv[4:]
    files = sorted(p for p in glob.glob(os.path.join(folder, "*.csv")) if blow in os.path.basename(p))
    if wanted:
        files = [p for w in wanted for p in files if w in os.path.basename(p).split("+")[0]]
    if not files:
        print("no file for that blow")
        return 1
    runs = [(os.path.basename(p).split("+")[0].replace("_", " ").strip(), read(p)) for p in files]
    # Each run from 12 steps before the ball met the body to 60 after.
    spans = []
    for _, steps in runs:
        met = min((s - v["met"] for s, v in steps.items() if v["met"] >= 0), default=None)
        if met is None:
            print("a run in which the ball never met the body")
            return 1
        spans.append(met)
    frames = range(-12, 61)
    tmp = tempfile.mkdtemp()
    stand = [steps[max(min(steps), met - 12)]["at"]["Pelvis"] for (_, steps), met in zip(runs, spans)]
    for n, k in enumerate(frames):
        fig, axes = plt.subplots(len(runs), 2, figsize=(7.2, 3.3 * len(runs)), squeeze=False)
        for r, ((name, steps), met) in enumerate(zip(runs, spans)):
            now = steps.get(met + k)
            for c, (ax, (h, title)) in enumerate(zip(axes[r], ((2, "from its right side"), (0, "from the front")))):
                ax.set_xlim(-1.1, 1.1)
                ax.set_ylim(-0.05, 2.0)
                ax.set_aspect("equal")
                ax.axhline(0, color="#888", lw=1)
                ax.set_xticks([])
                ax.set_yticks([])
                ax.set_title(f"{name}\n{title}" if c == 0 else title, fontsize=8, loc="left")
                if now is None:
                    continue
                at = now["at"]
                x0 = stand[r][h]
                for a, b in BONES:
                    if a in at and b in at:
                        colour = "#c0504d" if b in LEFT else "#1f3a5f"
                        ax.plot([at[a][h] - x0, at[b][h] - x0], [at[a][1], at[b][1]], color=colour, lw=3, solid_capstyle="round")
                if "Head" in at:
                    ax.add_patch(plt.Circle((at["Head"][h] - x0, at["Head"][1] + 0.11), 0.1, color="#1f3a5f"))
                if "Ball" in at:
                    ax.add_patch(plt.Circle((at["Ball"][h] - x0, at["Ball"][1]), 0.06, color="#e0a030"))
                # (Where it stood, to see how far its feet took it.)
                ax.plot([0, 0], [0, 0.04], color="#888", lw=1)
        fig.suptitle(f"{k / 60:+.2f} s (a quarter speed)", fontsize=9)
        fig.tight_layout()
        fig.savefig(os.path.join(tmp, f"f{n:04d}.png"), dpi=110)
        plt.close(fig)
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-framerate", "15", "-i", os.path.join(tmp, "f%04d.png"),
                    "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2", "-pix_fmt", "yuv420p", out], check=True)
    print("wrote", out)
    return 0


if __name__ == "__main__":
    sys.exit(main())
