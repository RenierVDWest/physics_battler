#!/usr/bin/env python3
"""Get-up review sheet: the animated pose through each get-up, seen from the side and from the front.

    dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll balance <dir>     (in Tools/CompileCheck)
    python3 Tools/Playtest/getups.py <dir>

Writes <dir>/getups.png: one row per get-up (each body, face down and face up), the pose every
0.2 s from the ragdoll's pose to standing, faded early and solid late.
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


def draw(ax, row, horizontal, alpha, color):
    for part, parent in PARENT.items():
        a = (row[f"{parent}.{horizontal}"], row[f"{parent}.y"])
        b = (row[f"{part}.{horizontal}"], row[f"{part}.y"])
        side = part.endswith("L") and part not in ("Pelvis",)
        ax.plot([a[0], b[0]], [a[1], b[1]], color=color if not side else "#c0392b", alpha=alpha, lw=1.6)
    ax.plot(row[f"Head.{horizontal}"], row["Head.y"], "o", color=color, alpha=alpha, ms=5)


def main():
    d = sys.argv[1]
    files = sorted(f for f in os.listdir(d) if f.endswith(".csv"))
    fig, axs = plt.subplots(len(files), 2, figsize=(16, 2.6 * len(files)), squeeze=False)
    for i, f in enumerate(files):
        df = pd.read_csv(os.path.join(d, f))
        up = df[df["state"] == "GettingUp"]
        end = df[df["state"] == "Standing"]
        times = np.arange(0.0, up["t"].max() + 1e-6, 0.2)
        for j, (ax, horizontal, title) in enumerate(((axs[i, 0], "z", "side (forward to the right)"), (axs[i, 1], "x", "front (right to the right)"))):
            for k, t in enumerate(times):
                row = up.iloc[(up["t"] - t).abs().argmin()]
                offset = 1.3 * k
                r = row.copy()
                for c in r.index:
                    if c.endswith("." + horizontal):
                        r[c] = r[c] + offset
                draw(ax, r, horizontal, 0.35 + 0.65 * k / max(1, len(times) - 1), "#202020")
                ax.text(offset, -0.15, f"{t:.1f}s", fontsize=7, ha="center")
            if not end.empty:
                r = end.iloc[-1].copy()
                offset = 1.3 * len(times)
                for c in r.index:
                    if c.endswith("." + horizontal):
                        r[c] = r[c] + offset
                draw(ax, r, horizontal, 1.0, "#1f6f3f")
                ax.text(offset, -0.15, "stood", fontsize=7, ha="center")
            ax.axhline(0, color="#999", lw=0.8)
            ax.set_aspect("equal")
            ax.set_xticks([])
            ax.set_title(f"{f[:-4]}: {title}", fontsize=9)
    fig.tight_layout()
    out = os.path.join(d, "getups.png")
    fig.savefig(out, dpi=80)
    print(out)


if __name__ == "__main__":
    main()
