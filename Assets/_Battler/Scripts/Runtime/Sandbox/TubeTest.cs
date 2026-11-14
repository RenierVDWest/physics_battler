using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The tube test: fighters that only stand there, each struck by a tube with a weight and a speed that are set,
    /// to see what a blow of a known size does to a body, apart from the fight. A tube is a bar a metre and a bit
    /// long lying level; it waits a little way off and, launched, goes straight at the fighter broadside, as a bat
    /// swung level does, its end reaching as far across the fighter as it is set to. The fighter is turned to take
    /// it on the front, a side or the back. How hard it hits is its
    /// momentum (N·s, the measure the fight's own blows are logged in: a Medium's sword cut is about 45, a Large's
    /// club 100 to 300), which with its speed sets its weight. It does not turn and does not fall: all it brings is
    /// that push. It is a body of the physics like any other, so what it does to a fighter is what the physics does;
    /// <see cref="AsBlow"/> on, the fighter also takes it as the fight's blow of that momentum on the part the tube
    /// first meets (its recoil, its arm knocked, its stumble or fall).
    /// </summary>
    public class TubeTest
    {
        /// <summary>How hard each tube hits: its momentum (N·s).</summary>
        public float Impulse = 60f;

        /// <summary>How fast it goes (m/s); with <see cref="Impulse"/> this sets its weight.</summary>
        public float Speed = 8f;

        /// <summary>How high it goes over the ground (metres).</summary>
        public float Height = 1.2f;

        /// <summary>How far its end reaches past the middle of the fighter (metres; negative: it stops short of the middle, and clips the fighter's near side or misses).</summary>
        public float Reach = 0.15f;

        /// <summary>
        /// The fighters are turned this far from facing the side their tube comes from (degrees, + to their right): 0
        /// struck on the front, 90 on their left side, 180 on the back, 270 on their right side.
        /// </summary>
        public float Turned;

        /// <summary>The way a fighter faces (level, world) for <see cref="Turned"/>, the tubes lying along <paramref name="toFighter"/>.</summary>
        public Vector3 Facing(Vector3 toFighter)
        {
            Vector3 along = new Vector3(toFighter.x, 0f, toFighter.z).normalized;
            // (The tubes come from the side opposite the way they travel.)
            Vector3 from = -Vector3.Cross(Vector3.up, along);
            return Quaternion.Euler(0f, Turned, 0f) * from;
        }

        /// <summary>The side of the fighter the tube comes at, in words.</summary>
        public string Side
        {
            get
            {
                float t = Mathf.Repeat(Turned, 360f);
                return t < 30f || t > 330f ? "the front" : t < 60f ? "the front left" : t <= 120f ? "the left side" : t < 150f ? "the back left"
                    : t <= 210f ? "the back" : t < 240f ? "the back right" : t <= 300f ? "the right side" : "the front right";
            }
        }

        /// <summary>The fighter also takes the tube as a blow of the fight's, of that momentum on the part it first meets.</summary>
        public bool AsBlow;

        /// <summary>
        /// The fall test: the fighter goes down along the tube's way whatever its legs say (it is also taken as a blow).
        /// A fall on demand, from the side the tube comes from.
        /// </summary>
        public bool Fells;

        /// <summary>A tube's length and thickness (its radius), how far to the side it waits, and how long after its launch it is taken away (metres, metres, metres, seconds).</summary>
        public const float Length = 1.2f, Radius = 0.04f, WaitsAside = 1.3f, Lives = 2.5f;

        private class Tube
        {
            public GameObject Object;
            public Rigidbody Body;
            public UnitController At;
            public Vector3 Spot, Along, Across;
            public bool Launched, Met;
            public float Age;
        }

        private readonly List<Tube> tubes = new List<Tube>();

        /// <summary>What the last tubes met, for the panel and the logs.</summary>
        public string Report = "";

        /// <summary>For the reports: where the first tube is, how fast it goes, and how near it last was to its fighter's nearest part (metres; negative: into it).</summary>
        public string State => tubes.Count == 0 || tubes[0].Object == null ? "no tube"
            : $"tube at ({tubes[0].Body.position.x:0.00},{tubes[0].Body.position.y:0.00},{tubes[0].Body.position.z:0.00}) going {tubes[0].Body.linearVelocity.magnitude:0.0} m/s, {100f * lastGap:0} cm from the nearest part ({lastNear})";

        private float lastGap = 99f;
        private PartId lastNear;

        /// <summary>Tubes are waiting or flying.</summary>
        public bool Armed => tubes.Count > 0;

        /// <summary>A tube is waiting to be sent.</summary>
        public bool Waiting => tubes.Exists(t => !t.Launched && t.Object != null);

        /// <summary>Its weight now (kg).</summary>
        public float Mass => Impulse / Mathf.Max(0.5f, Speed);

        /// <summary>
        /// A tube set waiting beside each of these fighters, where its settings have it. <paramref name="toFighter"/>: the
        /// way an attacker would face (level). Tubes already there are taken away first.
        /// </summary>
        public void Arm(IList<UnitController> fighters, Vector3 toFighter, Material look = null)
        {
            Clear();
            Vector3 along = new Vector3(toFighter.x, 0f, toFighter.z).normalized;
            Vector3 across = Vector3.Cross(Vector3.up, along);
            foreach (var unit in fighters)
            {
                if (unit == null || unit.Clip == null || !unit.IsAlive)
                {
                    continue;
                }

                var tube = new Tube { At = unit, Spot = unit.TargetDummy ? unit.DummySpot : unit.Clip.Loco.Position, Along = along, Across = across };
                tube.Object = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tube.Object.name = "Tube";
                // (A cylinder stands two units tall and one across: lain along the way to the fighter, as long and as thick as a tube is.)
                tube.Object.transform.localScale = new Vector3(2f * Radius, 0.5f * Length, 2f * Radius);
                tube.Object.transform.rotation = Quaternion.FromToRotation(Vector3.up, along);
                var pelvis = unit.Rig != null ? unit.Rig.Part(PartId.Pelvis) : null;
                if (pelvis != null)
                {
                    // (On the layer a body's parts are on: what meets a body meets it.)
                    tube.Object.layer = pelvis.gameObject.layer;
                }

                var shown = tube.Object.GetComponent<Renderer>();
                if (shown != null && look != null)
                {
                    shown.sharedMaterial = look;
                }

                tube.Body = tube.Object.AddComponent<Rigidbody>();
                tube.Body.isKinematic = true;
                tube.Body.useGravity = false;
                tube.Body.freezeRotation = true;
                tube.Body.linearDamping = 0f;
                tube.Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                tubes.Add(tube);
            }

            Place();
        }

        /// <summary>The waiting tubes put where the settings have them now (a slider moved); those launched are left to fly.</summary>
        public void Place()
        {
            foreach (var tube in tubes)
            {
                if (tube.Launched || tube.Object == null)
                {
                    continue;
                }

                Vector3 end = tube.Spot + tube.Along * Reach;
                Vector3 middle = end - tube.Along * (0.5f * Length) - tube.Across * WaitsAside;
                middle.y = tube.Spot.y + Height;
                tube.Object.transform.position = middle;
                tube.Body.position = middle;
            }
        }

        /// <summary>Every waiting tube is sent across its fighter, with the weight and speed the settings give it.</summary>
        public int Launch()
        {
            int sent = 0;
            foreach (var tube in tubes)
            {
                if (tube.Launched || tube.Object == null)
                {
                    continue;
                }

                tube.Launched = true;
                tube.Body.mass = Mass;
                tube.Body.isKinematic = false;
                tube.Body.linearVelocity = tube.Across * Speed;
                sent++;
            }

            Report = "";
            return sent;
        }

        /// <summary>
        /// A step of the game: what each flying tube first meets is noted (and, <see cref="AsBlow"/>, taken as a blow), and a
        /// tube is taken away <see cref="Lives"/> seconds after its launch.
        /// </summary>
        public void Step(float dt)
        {
            for (int i = tubes.Count - 1; i >= 0; i--)
            {
                var tube = tubes[i];
                if (tube.Object == null)
                {
                    tubes.RemoveAt(i);
                    continue;
                }

                if (!tube.Launched)
                {
                    continue;
                }

                tube.Age += dt;
                if (!tube.Met)
                {
                    Meet(tube);
                }

                if (tube.Age > Lives)
                {
                    UnityEngine.Object.Destroy(tube.Object);
                    tubes.RemoveAt(i);
                }
            }
        }

        /// <summary>Which of its fighter's parts the tube is against, if any: the nearest to the tube's line within reach of it.</summary>
        private void Meet(Tube tube)
        {
            var unit = tube.At;
            if (unit == null || unit.Rig == null || unit.Clip == null)
            {
                return;
            }

            Vector3 middle = tube.Body.position, half = tube.Along * (0.5f * Length);
            Vector3 a = middle - half, b = middle + half;
            PartId? met = null;
            float nearest = float.MaxValue;
            Vector3 where = middle;
            foreach (PartId id in System.Enum.GetValues(typeof(PartId)))
            {
                var part = unit.Rig.Part(id);
                if (part == null || part.partCollider == null || unit.Rig.IsSevered(id))
                {
                    continue;
                }

                // (The point of the tube's line nearest the part, then the part's point nearest that.)
                Vector3 centre = part.transform.position;
                Vector3 ab = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(centre - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                Vector3 onTube = a + ab * t;
                Vector3 onPart = part.partCollider.ClosestPoint(onTube);
                float gap = Vector3.Distance(onTube, onPart) - Radius;
                if (gap < nearest)
                {
                    nearest = gap;
                    met = id;
                    where = onPart;
                }
            }

            if (tube == tubes[0] && met.HasValue)
            {
                lastGap = nearest;
                lastNear = met.Value;
            }

            if (!met.HasValue || nearest > 0.03f)
            {
                return;
            }

            tube.Met = true;
            float goes = tube.Body.linearVelocity.magnitude;
            Report += $"{(Report.Length > 0 ? "; " : "")}{unit.name}: the {met.Value}";
            PlaytestTrace.Note(unit, "tube", $"a tube of {Mass:0.0} kg at {Speed:0.0} m/s ({Impulse:0} N·s) met its {met.Value} {where.y - tube.Spot.y:0.00} m up, on {Side} (turned {Turned:0} degrees){(Fells ? "; the fall test: it goes down" : AsBlow ? "; taken as a blow" : "; the physics only")}");
            if (!AsBlow && !Fells)
            {
                return;
            }

            // (As the fight resolves a blow of that momentum on that part: what it costs in poise, how it is reacted to.)
            Vector3 impulse = (tube.Across + Vector3.up * 0.15f).normalized * Impulse;
            float poise = DamageRules.PoiseScale * Impulse;
            var clip = unit.Clip;
            var reaction = clip.Poise - poise <= 0f ? Reaction.Stagger : poise >= DamageRules.FlinchShare * clip.MaxPoise ? Reaction.Flinch : Reaction.Recoil;
            clip.TakeHit(met.Value, false, impulse, DamageRules.PartShare(met.Value), poise, reaction, where);
            if (Fells && !clip.Fallen)
            {
                // (As the fight's own knock-down sends it: along the blow, off its feet.)
                clip.KnockOver(tube.Across, 0.8f, $"the fall test: a tube of {Impulse:0} N·s on its {met.Value}, on {Side}");
            }
        }

        /// <summary>Every tube taken away.</summary>
        public void Clear()
        {
            foreach (var tube in tubes)
            {
                if (tube.Object != null)
                {
                    UnityEngine.Object.Destroy(tube.Object);
                }
            }

            tubes.Clear();
        }
    }
}
