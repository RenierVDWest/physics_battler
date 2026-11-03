using System.Globalization;
using System.Text;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The locomotion lab's film: the posed body every few steps, with the walk's state, written as
    /// locolab.json for Tools/Mocap/Locomotion/locofilm.py to draw (the headless runner and the Unity
    /// lab both write it).
    /// </summary>
    public sealed class LocoFilm
    {
        private readonly StringBuilder film;
        private readonly int every;
        private readonly int most;
        private int steps, frames;

        /// <param name="dt">The step (seconds).</param>
        /// <param name="every">A frame every this many steps.</param>
        /// <param name="mostFrames">At most this many frames (the rest are dropped).</param>
        public LocoFilm(float dt, int every = 2, int mostFrames = 9000)
        {
            this.every = Mathf.Max(1, every);
            most = mostFrames;
            film = new StringBuilder("{\"dt\":" + (dt * this.every).ToString("0.###", CultureInfo.InvariantCulture) + ",\"frames\":[");
        }

        private static string V(Vector3 v) => string.Format(CultureInfo.InvariantCulture, "[{0:0.###},{1:0.###},{2:0.###}]", v.x, v.y, v.z);

        /// <param name="origin">Where the walk's world is in the scene (subtracted: the film is in board metres).</param>
        public void Add(float time, string leg, Locomotor loco, LocoBody poser, AnimRig anim, Vector3 origin = default)
        {
            if (steps++ % every != 0 || frames >= most)
            {
                return;
            }

            var sb = new StringBuilder(frames == 0 ? "{" : ",{");
            frames++;
            sb.Append(string.Format(CultureInfo.InvariantCulture, "\"t\":{0:0.00},\"leg\":\"{1}\",\"mode\":\"{2}\",\"shot\":\"{3}\",\"speed\":{4:0.00},\"run\":{5:0.00},\"phase\":{6:0.00},",
                time, (leg ?? "").Replace("\"", "'"), loco.Current, loco.Shot != null ? loco.Shot.Name : "", new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude, loco.RunWeight, loco.Phase));
            sb.Append($"\"root\":{V(loco.Position)},\"facing\":{loco.Facing.ToString("0.0", CultureInfo.InvariantCulture)},\"travel\":{loco.TravelAngle.ToString("0", CultureInfo.InvariantCulture)},\"strideScale\":{loco.StrideScale.ToString("0.00", CultureInfo.InvariantCulture)},\"want\":{V(loco.Wanted)},");
            sb.Append($"\"locks\":[{(poser.Feet[0].Locked ? 1 : 0)},{(poser.Feet[1].Locked ? 1 : 0)}],");
            sb.Append("\"pose\":{");
            bool first = true;
            foreach (var id in PartInfo.All)
            {
                sb.Append(first ? "" : ",").Append($"\"{id}\":{V(anim.Part(id).position - origin)}");
                first = false;
            }

            var sl = anim.SlotWeaponL;
            var sr = anim.SlotWeaponR;
            sb.Append($",\"HandTipL\":{V((sl != null ? sl.position : anim.Part(PartId.HandL).position) - origin)},\"HandTipR\":{V((sr != null ? sr.position : anim.Part(PartId.HandR).position) - origin)}");
            sb.Append($",\"ToeL\":{V(anim.Part(PartId.FootL).position + anim.Part(PartId.FootL).rotation * new Vector3(0f, -0.06f, 0.16f) - origin)}");
            sb.Append($",\"ToeR\":{V(anim.Part(PartId.FootR).position + anim.Part(PartId.FootR).rotation * new Vector3(0f, -0.06f, 0.16f) - origin)}");
            if (BladeLength > 0f && sr != null)
            {
                sb.Append($",\"BladeTipR\":{V(sr.position + sr.rotation * Vector3.forward * BladeLength - origin)}");
            }

            if (ShieldRadius > 0f && sl != null)
            {
                // (The shield's face is across the hand: its centre at the grip, its normal the hand's line.)
                Vector3 n = (anim.Part(PartId.HandL).rotation * anim.RestLocalPosition(PartId.HandL)).normalized;
                sb.Append($",\"ShieldC\":{V(sl.position - origin)},\"ShieldN\":{V(n)}");
            }

            sb.Append("}}");
            film.Append(sb);
        }

        public int Frames => frames;

        /// <summary>A carried weapon's length in the right hand and a shield's radius in the left (metres; 0: none), drawn in the film.</summary>
        public float BladeLength, ShieldRadius;

        public override string ToString() => film + "]}";
    }
}
