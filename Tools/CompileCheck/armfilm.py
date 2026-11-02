"""Draws `armfilm` output as stick figures: for each blow, the attacker (blue, weapon arm red, weapon
black) at its wind-up's end, contact and follow-through's end, seen from the side, from the front
(behind the attacker, looking at the target) and from above, the striking part's trail in orange and
the aim in red; plus a strip of the whole blow from the side. Usage: python3 armfilm.py film_*.json"""
import json, sys, os
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

BONES = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"), ("Head", "HeadTop"),
         ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
         ("Chest", "UpperArmR"),
         ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]
ARM = [("UpperArmR", "ForearmR"), ("ForearmR", "HandR")]
# views: (name, x index, y index, x sign)
VIEWS = [("side (from the right)", 2, 1, 1), ("front (from behind)", 0, 1, 1), ("top", 0, 2, 1)]


def draw(ax, frame, xi, yi, alpha=1.0, target=None):
    p = frame["pose"]
    for a, b in BONES:
        ax.plot([p[a][xi], p[b][xi]], [p[a][yi], p[b][yi]], color=(0.3, 0.45, 0.85, alpha), lw=2)
    for a, b in ARM:
        ax.plot([p[a][xi], p[b][xi]], [p[a][yi], p[b][yi]], color=(0.85, 0.2, 0.2, alpha), lw=3)
    ax.plot([p["HandR"][xi], frame["grip"][xi]], [p["HandR"][yi], frame["grip"][yi]], color=(0.85, 0.2, 0.2, alpha), lw=2)
    ax.plot([frame["grip"][xi], frame["tip"][xi]], [frame["grip"][yi], frame["tip"][yi]], color=(0, 0, 0, alpha), lw=2.5)
    ax.scatter([p["ForearmR"][xi]], [p["ForearmR"][yi]], color=(0.85, 0.2, 0.2, alpha), s=12)


def nearest(frames, t):
    return min(frames, key=lambda f: abs(f["t"] - t))


def main(paths):
    for path in paths:
        d = json.load(open(path))
        frames = d["frames"]
        keys = [("wind-up", d["windUp"]), ("contact", d["contact"]), ("follow-through", d["follow"])]
        fig, axes = plt.subplots(4, 3, figsize=(13, 16))
        for col, (name, t) in enumerate(keys):
            f = nearest(frames, t)
            for row, (vname, xi, yi, _) in enumerate(VIEWS):
                ax = axes[row][col]
                tp = d["target"]
                for a, b in BONES + ARM + [("HandR", "HandR")]:
                    ax.plot([tp[a][xi], tp[b][xi]], [tp[a][yi], tp[b][yi]], color=(0.6, 0.6, 0.6), lw=1)
                ax.plot([g["strike"][xi] for g in frames], [g["strike"][yi] for g in frames], color="orange", lw=1)
                draw(ax, f, xi, yi)
                ax.scatter([d["aim"][xi]], [d["aim"][yi]], color="red", s=30, zorder=5)
                ax.set_aspect("equal")
                ax.set_title(f"{name} t={f['t']:.2f} {vname}\nwrist {f['wrist']} elbow {f['swivel']}", fontsize=8)
                ax.tick_params(labelsize=6)
        ax = axes[3][0]
        gs = axes[3][0].get_gridspec()
        for a in axes[3]:
            a.remove()
        ax = fig.add_subplot(gs[3, :])
        n = 10
        for k in range(n):
            f = frames[int(k * (len(frames) - 1) / (n - 1))]
            off = k * 1.2
            shifted = {"pose": {kk: [v[0], v[1], v[2] + off] for kk, v in f["pose"].items()},
                       "grip": [f["grip"][0], f["grip"][1], f["grip"][2] + off], "tip": [f["tip"][0], f["tip"][1], f["tip"][2] + off]}
            draw(ax, shifted, 2, 1)
            ax.text(off + f["pose"]["Pelvis"][2], -0.1, f"{f['t']:.2f}", fontsize=7, ha="center")
        ax.set_aspect("equal")
        ax.set_title("the blow, side view, left to right", fontsize=8)
        fig.suptitle(f"{d['move']} ({d['body']} at a Medium's {d['part']}): arc {d['arc']}°, contact {d['contactSpeed']} m/s")
        fig.tight_layout()
        out = os.path.splitext(path)[0] + ".png"
        fig.savefig(out, dpi=80)
        plt.close(fig)
        print(out)


if __name__ == "__main__":
    main(sys.argv[1:])
