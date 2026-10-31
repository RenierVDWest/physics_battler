"""Three unthemed base bodies (small, medium, large): their proportions, joint layout (the null
rig) and masses. The surfaces are sculpted in sculpt.py and cut into parts there.

Conventions (as the orc's): Blender Z up, the body faces -Y, its RIGHT side is on -X. Unity: Y up,
faces +Z, right is +X. 1 unit = 1 m, feet on z = 0. One null per part at its joint, identity
rotation in the rest pose; each part's mesh has its origin at that null.

Joints are ball joints: both parts carry a ball around the shared pivot (the child's a few percent
smaller), so any rotation about the pivot keeps the joint closed.
"""

import math

import bpy  # noqa: F401  (must be imported before mathutils when Blender runs as a module)
from mathutils import Vector

FWD = Vector((0, -1, 0))   # the body faces -Y
UP = Vector((0, 0, 1))


def side_name(s):
    return "R" if s < 0 else "L"


def align_z(direction):
    return Vector((0, 0, 1)).rotation_difference(direction.normalized()).to_matrix().to_4x4()


# ------------------------------------------------------------------------------- body specs

class Spec:
    """Proportions of one body. Lengths are multiples of the medium body's; girths multiply the
    medium body's radius keys (after the height scale)."""

    def __init__(self, name, height_scale, mass, **kw):
        self.name = name
        self.hs = height_scale
        self.mass = mass
        d = dict(head=1.0, neck=1.0, neck_len=1.0, chest=1.0, waist=1.0, hips=1.0,
                 upper_arm=1.0, forearm=1.0, hand=1.0, thigh=1.0, shin=1.0, foot=1.0,
                 shoulder_w=1.0, hip_w=1.0, arm_len=1.0, leg_len=1.0, torso_len=1.0,
                 arm_tilt=16.0, traps=1.0, lats=1.0, pecs=1.0, glutes=1.0, belly=1.0,
                 calf=1.0, deltoid=1.0, biceps=1.0)
        d.update(kw)
        self.__dict__.update(d)


SPECS = [
    # About 0.7 of human height (1.4 times the first small body): a big head, big hands and feet for
    # its size (but not past the medium body's), wiry limbs, narrow shoulders, a little belly.
    Spec("Small", 0.7, 38.0, head=1.45, neck=0.95, neck_len=0.75, chest=1.0, waist=1.05, hips=1.0,
         upper_arm=0.95, forearm=1.0, hand=1.25, thigh=1.0, shin=0.95, foot=1.2,
         shoulder_w=0.95, hip_w=1.0, arm_len=1.1, leg_len=0.78, torso_len=0.95, arm_tilt=18,
         traps=0.75, lats=0.8, pecs=0.7, glutes=0.9, belly=1.3, calf=0.85, deltoid=0.9, biceps=0.8),
    # Heroic human: broad shoulders, narrow waist.
    Spec("Medium", 1.0, 80.0),
    # Taller and much broader: huge traps, shoulders and arms, thick legs, a small head for its size.
    Spec("Large", 1.38, 280.0, head=0.85, neck=1.55, neck_len=0.45, chest=1.38, waist=1.3, hips=1.12,
         upper_arm=1.5, forearm=1.5, hand=1.3, thigh=1.45, shin=1.4, foot=1.2,
         shoulder_w=1.3, hip_w=1.2, arm_len=1.02, leg_len=0.9, torso_len=1.05, arm_tilt=24,
         traps=1.6, lats=1.25, pecs=1.3, glutes=1.15, belly=1.08, calf=0.95, deltoid=1.15, biceps=1.2),
]

# Mass shares (percent of the total), human-like but with hands and feet a little heavier than
# real, which keeps the joint chain stable in PhysX.
MASS_PCT = {"Pelvis": 14, "Spine": 12, "Chest": 20, "Head": 8,
            "UpperArm": 3.0, "Forearm": 1.8, "Hand": 1.0, "Thigh": 10, "Shin": 4.6, "Foot": 1.6}

BODY_PARTS = ["Pelvis", "Spine", "Chest", "Head",
              "UpperArm_R", "Forearm_R", "Hand_R", "UpperArm_L", "Forearm_L", "Hand_L",
              "Thigh_R", "Shin_R", "Foot_R", "Thigh_L", "Shin_L", "Foot_L"]


class Body:
    """Joint layout and part meshes of one spec."""

    def __init__(self, spec):
        self.spec = sp = spec
        h = sp.hs
        self.ankle_z = 0.085 * h * sp.foot ** 0.5
        self.knee_z = self.ankle_z + 0.415 * h * sp.leg_len
        self.hip_z = self.knee_z + 0.43 * h * sp.leg_len
        self.pelvis_z = self.hip_z + 0.055 * h * sp.torso_len
        self.waist_z = self.pelvis_z + 0.14 * h * sp.torso_len
        self.chest_z = self.waist_z + 0.17 * h * sp.torso_len
        self.neck_z = self.chest_z + 0.255 * h * sp.torso_len
        self.hip_x = 0.095 * h * sp.hip_w
        self.shoulder = (0.205 * h * sp.shoulder_w, self.neck_z - 0.085 * h * sp.torso_len * sp.traps ** 0.3)
        self.upper_arm = 0.30 * h * sp.arm_len
        self.forearm = 0.27 * h * sp.arm_len
        self.fist = 0.085 * h * sp.hand
        self.tilt = math.radians(sp.arm_tilt)
        hs = 0.235 * h * sp.head          # head height, chin to crown
        self.head_size = hs
        self.neck_top = self.neck_z + 0.06 * h * sp.neck_len
        self.head_c = Vector((0, -0.02 * hs / 0.235, self.neck_top + 0.42 * hs))
        self.height = self.head_c.z + 0.5 * hs

        self.nulls = {"Root": (None, Vector((0, 0, 0))),
                      "Pelvis": ("Root", Vector((0, 0, self.pelvis_z))),
                      "Spine": ("Pelvis", Vector((0, 0, self.waist_z))),
                      "Chest": ("Spine", Vector((0, 0, self.chest_z))),
                      "Head": ("Chest", Vector((0, 0, self.neck_z))),
                      "Aim_Chest": ("Chest", Vector((0, -0.11 * h * sp.chest, self.chest_z + 0.15 * h * sp.torso_len))),
                      "Aim_Belly": ("Spine", Vector((0, -0.12 * h * sp.belly, self.waist_z + 0.08 * h * sp.torso_len))),
                      "Aim_Neck": ("Head", Vector((0, -0.01 * h, self.neck_z + 0.04 * h * sp.neck_len))),
                      "Aim_Head": ("Head", self.head_c + Vector((0, -0.05 * hs / 0.235, 0.01 * hs)))}
        for s in (-1, 1):
            n = side_name(s)
            sh = Vector((s * self.shoulder[0], 0, self.shoulder[1]))
            el = sh + self.arm_dir(s) * self.upper_arm
            wr = el + self.arm_dir(s) * self.forearm
            self.nulls[f"UpperArm_{n}"] = ("Chest", sh)
            self.nulls[f"Forearm_{n}"] = (f"UpperArm_{n}", el)
            self.nulls[f"Hand_{n}"] = (f"Forearm_{n}", wr)
            self.nulls[f"Slot_Weapon_{n}"] = (f"Hand_{n}", wr + self.arm_dir(s) * self.fist)
            self.nulls[f"Thigh_{n}"] = ("Pelvis", Vector((s * self.hip_x, 0, self.hip_z)))
            self.nulls[f"Shin_{n}"] = (f"Thigh_{n}", Vector((s * self.hip_x, -0.004 * h, self.knee_z)))
            self.nulls[f"Foot_{n}"] = (f"Shin_{n}", Vector((s * self.hip_x, 0.006 * h, self.ankle_z)))

    def arm_dir(self, s):
        return Vector((s * math.sin(self.tilt), 0.03, -math.cos(self.tilt))).normalized()

    def P(self, name):
        return self.nulls[name][1].copy()

    def mass_of(self, part):
        total = sum(MASS_PCT[p.split("_")[0]] for p in BODY_PARTS)
        return round(self.spec.mass * MASS_PCT[part.split("_")[0]] / total, 3)
