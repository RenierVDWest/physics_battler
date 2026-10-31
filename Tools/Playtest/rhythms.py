#!/usr/bin/env python3
"""Fight Rhythm review sheets: each style's base and postures, drawn through time.

    dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll rhythms <dir>     (in Tools/CompileCheck)
    python3 Tools/Playtest/rhythms.py <dir>

Writes <dir>/rhythm_<body>.png: a row per posture (the base first), the animated pose at even
moments through it, from the side (facing right) and from the front; the weapon brown, the shield
blue, the left limbs red; open body parts named under each moment.
"""
import os
import sys

import numpy as np
import pandas as pd
import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt

PARENT = {
    "Spine": "Pelvis", "Chest": "Spine", "Head": "Chest",
    "UpperArmL": "Chest", "ForearmL": "UpperArmL", "HandL": "ForearmL",
    "UpperArmR": "Chest", "ForearmR": "UpperArmR", "HandR": "ForearmR",
    "ThighL": "Pelvis", "ShinL": "ThighL", "FootL": "ShinL",
    "ThighR": "Pelvis", "ShinR": "ThighR", "FootR": "ShinR",
}
ORDER = ["Base"]


def draw(ax, r, h, dx, shield):
    def p(name):
        return r[f"{name}.{h}"] + dx, r[f"{name}.y"]
    for part, parent in PARENT.items():
        a, b = p(parent), p(part)
        ax.plot([a[0], b[0]], [a[1], b[1]], color="#c0392b" if part.endswith("L") else "#202020", lw=1.4)
    hx, hy = p("Head")
    ax.plot(hx, hy, "o", color="#202020", ms=4)
    g, t = p("grip"), p("tip")
    ax.plot([g[0], t[0]], [g[1], t[1]], color="#8a5a2b", lw=2.6, solid_capstyle="round")
    if shield:
        s, n = p("sh"), p("shn")
        ax.plot([s[0], n[0]], [s[1], n[1]], color="#30638e", lw=3.5, alpha=0.7)


def main():
    d = sys.argv[1]
    for body in ("large", "medium", "small"):
        files = [f for f in os.listdir(d) if f.startswith(body + "_") and f.endswith(".csv")]
        if not files:
            continue
        files.sort(key=lambda f: (0 if f.endswith("_Base.csv") else 1, f))
        cols = 7
        fig, axs = plt.subplots(len(files), 2, figsize=(22, 2.9 * len(files)), squeeze=False)
        for i, f in enumerate(files):
            df = pd.read_csv(os.path.join(d, f))
            name = f[len(body) + 1:-4]
            shield = body == "medium"
            base = name == "Base"
            moments = np.linspace(0.0, df["t"].max(), cols) if base else np.linspace(0.0, 1.0, cols)
            for j, h in enumerate(("z", "x")):
                ax = axs[i, j]
                for k, m in enumerate(moments):
                    key = "t" if base else "u"
                    r = df.iloc[(df[key] - m).abs().argmin()]
                    dx = 1.1 * k - r[f"Pelvis.{h}"]
                    draw(ax, r, h, dx, shield)
                    label = f"{r['t']:.2f}s" if base else f"{m:.2f}"
                    op = str(r["open"]) if isinstance(r["open"], str) and r["open"] != "None" else ""
                    ax.text(1.1 * k, -0.12, label, ha="center", fontsize=7)
                    if op:
                        ax.text(1.1 * k, -0.24, op.replace("|", " "), ha="center", fontsize=6, color="#d1495b")
                ax.axhline(0, color="#bbb", lw=0.7)
                ax.set_aspect("equal")
                ax.set_xticks([])
                ax.set_ylim(-0.3, df[[c for c in df.columns if c.endswith(".y")]].max().max() + 0.15)
                ax.set_title(f"{body} {name}: {'side, facing right' if h == 'z' else 'front'}", fontsize=9)
        fig.tight_layout()
        out = os.path.join(d, f"rhythm_{body}.png")
        fig.savefig(out, dpi=70)
        print(out)


if __name__ == "__main__":
    main()
