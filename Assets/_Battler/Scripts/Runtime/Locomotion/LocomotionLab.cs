using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The locomotion lab in the game (branch locomotion-lab): one Medium on the board, walked by the
    /// new locomotion (<see cref="Locomotor"/> and <see cref="LocoBody"/>) round the lab's course, or
    /// driven by hand. Nothing of the fighting runs: the unit's own controller is taken off, and its
    /// body parts are carried along by the posed animation rig (kinematic for now; the physical body
    /// following it is the plan's stage 5).
    ///
    /// Turned on by <see cref="BattleSandbox.locomotionLab"/>. Keys: C course or driving, R restart,
    /// P pause, period one step. Driving: W A S D (against the camera), Shift run, Space hold the facing
    /// (side-steps and walking backward), V walking speed. Each run's measures and film are written to
    /// PlaytestLogs/locolab-(time)/ when Play stops.
    /// </summary>
    [DefaultExecutionOrder(-140)]
    public class LocomotionLab : MonoBehaviour
    {
        public UnitController prefab;
        public UnitController smallPrefab, mediumPrefab, largePrefab;

        [Tooltip("Which body walks (N: the next one).")]
        public BodySize body = BodySize.Medium;
        public OrbitCamera orbitCamera;
        public Material tint;

        [Tooltip("Walk the lab's course (off: drive it with W A S D).")]
        public bool course = true;

        [Tooltip("The body is physical (B): the legs carried exactly, so the feet stay planted; the torso, head and arms held by pins, swaying with the walk and pushed about by what hits them. Off: every part carried kinematically.")]
        public bool physical = true;

        [Tooltip("How hard a shove (G) is: the change in the body's speed (m/s).")]
        public float shove = 1.3f;

        [Tooltip("Show the footprints planted feet leave.")]
        public bool footprints = true;

        private GameObject unit;
        private UnitRig rig;
        private AnimRig anim;
        private Locomotor loco;
        private LocoBody poser;
        private LocoMeasures measures;
        private LocoCourse path;
        private LocoFilm film;
        private Authority.AuthorityController authority;
        private bool snapped;
        private int shoves;
        private bool holding;
        private readonly System.Random dice = new System.Random();
        private float time;
        private string leg = "";
        private bool paused, stepOnce;
        private float walkSpeed = 1.4f;
        private Vector3 origin;
        private readonly List<string> legs = new List<string>();
        private Rect panel = new Rect(12, 12, 330, 10);
        private string folder;

        public const float BaseRunSpeed = 3.6f;

        /// <summary>A Medium's speeds, for this body: its natural pace goes with the square root of its leg.</summary>
        private float PaceScale => loco != null ? Mathf.Sqrt(loco.Leg / 0.845f) : 1f;

        private float RunSpeed => BaseRunSpeed * PaceScale;

        public void Begin(UnitController small, UnitController medium, UnitController large, OrbitCamera cam, Material material, Vector3 boardOrigin)
        {
            smallPrefab = small;
            mediumPrefab = medium;
            largePrefab = large;
            orbitCamera = cam;
            tint = material;
            origin = boardOrigin;
            Build();
        }

        private void Build() => Build(null, 0f);

        /// <param name="standAt">Stand up here (after a knockdown: the same run goes on), not at the start.</param>
        private void Build(Vector3? standAt, float standFacing)
        {
            // (Each run is logged on its own: a restart or a switch writes the run before it.)
            if (!standAt.HasValue)
            {
                WriteLog();
                written = false;
                legs.Clear();
            }

            if (unit != null)
            {
                Destroy(unit);
            }

            LocoLibrary.Load();
            prefab = body == BodySize.Small ? smallPrefab : body == BodySize.Large ? largePrefab : mediumPrefab;
            if (prefab == null)
            {
                prefab = mediumPrefab;
            }

            if (prefab == null)
            {
                Debug.LogError("[LocomotionLab] no unit prefab: run Physics Battler > Setup > Run Project Setup.");
                enabled = false;
                return;
            }

            // Made under an inactive holder, its own controller taken off before it wakes.
            var holder = new GameObject("Spawning");
            holder.SetActive(false);
            var made = Instantiate(prefab, origin, Quaternion.identity, holder.transform);
            made.name = "Locomotion lab Medium";
            BattleSandbox.Tint(made, tint);
            unit = made.gameObject;
            for (int pass = 0; pass < 3; pass++)
            {
                // (Latest first: a component another one needs was usually added before it.)
                var behaviours = unit.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = behaviours.Length - 1; i >= 0; i--)
                {
                    var behaviour = behaviours[i];
                    if (behaviour != null && !(behaviour is UnitRig) && !(behaviour is BodyPart))
                    {
                        DestroyImmediate(behaviour);
                    }
                }
            }

            rig = unit.GetComponent<UnitRig>();
            unit.transform.SetParent(null, true);
            Destroy(holder);

            // The physical body set up as a unit sets up its own (its joints in their frames, its
            // limits, its friction), while it is still in its rest pose; the animation rig is copied
            // then too. The parts then leave the hierarchy, each its own body.
            var profile = prefab.Profile != null ? prefab.Profile : UnitProfile.CreateDefault();
            if (physical)
            {
                JointConventions.EnsureProbed();
                var bodyMaterial = PhysicsMaterials.Get("Body", profile.bodyFriction);
                var footMaterial = PhysicsMaterials.Get("Foot", profile.footFriction);
                foreach (var part in rig.Parts)
                {
                    UnitBuilder.ConfigureBody(part.body, profile);
                    part.limits = JointLimitTable.Physical(part.id);
                    if (part.joint != null)
                    {
                        part.jointFrame = JointFrames.For(rig, part.id);
                        JointFrames.Apply(part.joint, part.jointFrame);
                        JointMath.ApplyLimits(part.joint, part.limits);
                    }

                    if (part.partCollider != null)
                    {
                        part.partCollider.sharedMaterial = PartInfo.IsFoot(part.id) ? footMaterial : bodyMaterial;
                    }
                }
            }

            anim = new AnimRig(unit.transform, rig);
            foreach (var part in rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = true;
                    part.body.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }

            foreach (var id in PartInfo.All)
            {
                rig.Part(id).transform.SetParent(unit.transform, true);
            }

            authority = null;
            if (physical)
            {
                Physics.SyncTransforms();
                var parts = rig.Parts;
                for (int i = 0; i < parts.Length; i++)
                {
                    for (int j = i + 1; j < parts.Length; j++)
                    {
                        if (parts[i].partCollider != null && parts[j].partCollider != null)
                        {
                            Physics.IgnoreCollision(parts[i].partCollider, parts[j].partCollider, true);
                        }
                    }
                }

                // As in the fight: the legs carried exactly (the feet's plants are the walk's), the rest held.
                authority = new Authority.AuthorityController(rig, anim);
                authority.Set(Authority.BodyRegion.Legs, Authority.AuthorityLevel.Kinematic, 1f);
                authority.Set(Authority.BodyRegion.Torso, Authority.AuthorityLevel.Pinned, profile.torsoPin);
                authority.Set(Authority.BodyRegion.Head, Authority.AuthorityLevel.Pinned, profile.torsoPin);
                authority.Set(Authority.BodyRegion.ArmL, Authority.AuthorityLevel.Pinned, profile.armPin);
                authority.Set(Authority.BodyRegion.ArmR, Authority.AuthorityLevel.Pinned, profile.armPin);
            }

            snapped = false;
            down = 0f;
            usualOff = Vector3.zero;

            float legLength = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
            loco = new Locomotor(legLength);
            if (!loco.Ready)
            {
                Debug.LogError("[LocomotionLab] the clip library (Resources/Locomotion/LocoClips.json) is missing clips.");
                enabled = false;
                return;
            }

            poser = new LocoBody(anim, loco) { GroundY = origin.y };
            poser.SoleFrom(rig);
            measures = new LocoMeasures(anim, loco, poser);
            film = new LocoFilm(Time.fixedDeltaTime, Mathf.Max(1, Mathf.RoundToInt(0.04f / Time.fixedDeltaTime)));
            path = LocoCourse.Lab();
            path.SpeedScale = PaceScale;
            path.LegStarted += (i, name) =>
            {
                leg = name ?? "";
                legs.Add($"{time:0.00} s: {leg}");
            };
            if (standAt.HasValue)
            {
                loco.Place(standAt.Value, standFacing);
                path.Reset();
                return;
            }

            loco.Place(origin + (course ? path.Start : Vector3.zero), course ? path.StartYaw : 0f);
            path.Reset();
            time = 0f;
            ClearPrints();
        }

        private void FixedUpdate()
        {
            if (loco == null || (paused && !stepOnce))
            {
                return;
            }

            stepOnce = false;
            float dt = Time.fixedDeltaTime;
            time += dt;
            Vector3 want;
            float facing;
            if (course)
            {
                // (The course is in board metres: the walk's world is the scene's, the board's middle at origin.)
                path.Steer(dt, loco.Position - origin, loco.Facing, loco.Velocity, out want, out facing);
                var stop = path.StopPoint;
                loco.StopAt = stop.HasValue ? stop.Value + origin : (Vector3?)null;
            }
            else
            {
                Drive(out want, out facing);
                loco.StopAt = null;
                leg = "driving";
            }

            if (down > 0f)
            {
                // Knocked down: physics alone has the body; it is stood up again after a while.
                down -= dt;
                authority.Apply(dt);
                if (down <= 0f)
                {
                    // (Up again where it fell, facing the way its chest faces: the getting up itself
                    // is the game's, when the two are joined.)
                    var pelvis = rig.Part(PartId.Pelvis).transform;
                    Vector3 at = new Vector3(pelvis.position.x, origin.y, pelvis.position.z);
                    Vector3 chestForward = rig.Part(PartId.Chest).transform.forward;
                    float yaw = new Vector2(chestForward.x, chestForward.z).sqrMagnitude > 1e-4f ? Mathf.Atan2(chestForward.x, chestForward.z) * Mathf.Rad2Deg : loco.Facing;
                    // (The film and the clock go on; the measures start again with the new body.)
                    var keepFilm = film;
                    float keepTime = time;
                    Build(at, yaw);
                    film = keepFilm;
                    time = keepTime;
                    legs.Add($"{time:0.00} s: stood up again");
                }

                return;
            }

            Bumped(dt);
            loco.Step(dt, want, facing);
            poser.Pose(dt);
            measures.Sample(dt, time, leg);
            film.Add(time, leg, loco, poser, anim, origin);
            if (authority != null && snapped)
            {
                authority.Apply(dt);
                if (new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude > KnockdownSpeed)
                {
                    KnockDown("pushed faster than it can step");
                }
            }
            else
            {
                Carry();
                snapped = true;
            }

            Prints();
        }

        // ---- Bumps and knockdowns (the plan's stage 6) ---------------------------------------------------

        [Tooltip("Something that knocks the physical upper body this far off its pose (m) pushes the walk; it steps to follow.")]
        public float bumpFrom = 0.04f;

        [Tooltip("How hard the upper body's displacement pushes the walk (m/s added a second, per metre).")]
        public float bumpGain = 25f;

        [Tooltip("Faster than this (m/s) and the steps cannot catch the body: it goes down (a ragdoll), and stands again after a while.")]
        public float knockdownSpeed = 3.2f;

        private float KnockdownSpeed => Mathf.Max(knockdownSpeed, loco.Running ? RunSpeed * 1.2f : knockdownSpeed);

        private float down;
        private string note = "";
        private Vector3 usualOff;

        /// <summary>
        /// Contact: the chest's physical body pushed off its pose (by a ball, a body, a blow) pushes the
        /// walk the same way, so the feet step after the body instead of the body tipping over them.
        /// </summary>
        private void Bumped(float dt)
        {
            if (authority == null || !snapped)
            {
                return;
            }

            var chest = rig.Part(PartId.Chest).body;
            if (chest == null || chest.isKinematic)
            {
                return;
            }

            Vector3 off = chest.position - anim.Part(PartId.Chest).position;
            off.y = 0f;
            // (Against the chest's usual lag behind its pose, which the walk's own speeding up and
            // slowing down makes: only a knock off that counts, or the walk would push itself.)
            usualOff = Vector3.Lerp(usualOff, off, 1f - Mathf.Exp(-dt / 0.5f));
            Vector3 knock = off - usualOff;
            float beyond = knock.magnitude - bumpFrom;
            if (beyond > 0f)
            {
                loco.Nudge(knock.normalized * (beyond * bumpGain * dt));
            }

            if (off.magnitude > 0.45f)
            {
                KnockDown("upper body knocked " + (off.magnitude * 100f).ToString("0") + " cm off");
            }
        }

        private void KnockDown(string why)
        {
            if (authority == null || down > 0f)
            {
                return;
            }

            down = 3f;
            note = "Knocked down: " + why;
            legs.Add($"{time:0.00} s: knocked down ({why})");
            foreach (Authority.BodyRegion region in Enum.GetValues(typeof(Authority.BodyRegion)))
            {
                authority.Set(region, Authority.AuthorityLevel.Ragdoll, 0f);
            }

            // (The body keeps the speed it had.)
            foreach (var part in rig.Parts)
            {
                if (part.body != null && !part.body.isKinematic)
                {
                    continue;
                }

                if (part.body != null)
                {
                    part.body.isKinematic = false;
                    part.body.linearVelocity = loco.Velocity;
                }
            }
        }

        /// <summary>A heavy ball thrown at the chest from a random side (H): something physical to bump into it.</summary>
        public void Throw()
        {
            if (loco == null)
            {
                return;
            }

            float yaw = (float)(dice.NextDouble() * 360.0);
            Vector3 chest = anim.Part(PartId.Chest).position;
            Vector3 from = chest + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 3f + Vector3.up * 0.3f;
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Lab ball";
            ball.transform.position = from;
            ball.transform.localScale = Vector3.one * 0.3f;
            var body = ball.AddComponent<Rigidbody>();
            body.mass = ballMass;
            body.linearVelocity = (chest - from).normalized * ballSpeed + Vector3.up * 1.2f;
            Destroy(ball, 5f);
            legs.Add($"{time:0.00} s: ball thrown from {yaw:0} degrees");
        }

        [Tooltip("The thrown ball (H): its mass (kg) and speed (m/s).")]
        public float ballMass = 12f, ballSpeed = 8f;

        /// <summary>The body parts where the animation rig has them (each one kinematic, moved by its body).</summary>
        private void Carry()
        {
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                var target = anim.Part(id);
                if (part.body != null)
                {
                    part.body.MovePosition(target.position);
                    part.body.MoveRotation(target.rotation);
                }
                else
                {
                    part.transform.SetPositionAndRotation(target.position, target.rotation);
                }
            }
        }

        /// <summary>
        /// A shove from a random side: the chest is struck (the physical upper body takes it) and the walk
        /// steps to catch the body.
        /// </summary>
        public void Shove() => Shove(1f);

        public void Shove(float times)
        {
            if (loco == null)
            {
                return;
            }

            float yaw = (float)(dice.NextDouble() * 360.0);
            Vector3 way = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            loco.Push(way * shove * times);
            shoves++;
            legs.Add($"{time:0.00} s: shoved toward {yaw:0} degrees");
            if (authority != null)
            {
                var chest = rig.Part(PartId.Chest).body;
                if (chest != null && !chest.isKinematic)
                {
                    chest.AddForce(way * (shove * times * 2.5f), ForceMode.VelocityChange);
                }
            }
        }

        private void Drive(out Vector3 want, out float facing)
        {
            Vector3 input = Vector3.zero;
            if (SandboxInput.KeyHeld(KeyCode.W)) input.z += 1f;
            if (SandboxInput.KeyHeld(KeyCode.S)) input.z -= 1f;
            if (SandboxInput.KeyHeld(KeyCode.D)) input.x += 1f;
            if (SandboxInput.KeyHeld(KeyCode.A)) input.x -= 1f;
            float camYaw = orbitCamera != null ? orbitCamera.transform.eulerAngles.y : 0f;
            Vector3 way = Quaternion.Euler(0f, camYaw, 0f) * Vector3.ClampMagnitude(input, 1f);
            float speed = SandboxInput.KeyHeld(KeyCode.LeftShift) ? RunSpeed : walkSpeed * PaceScale;
            want = way * speed;
            bool hold = SandboxInput.KeyHeld(KeyCode.Space);
            if (hold != holding)
            {
                holding = hold;
                legs.Add($"{time:0.00} s: facing {(hold ? "held" : "let go")}");
            }

            facing = !hold && way.sqrMagnitude > 0.01f ? Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg : loco.Facing;
        }

        private void Update()
        {
            if (loco == null)
            {
                return;
            }

            if (SandboxInput.KeyPressed(KeyCode.R)) Build();
            if (SandboxInput.KeyPressed(KeyCode.C)) { course = !course; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.P)) paused = !paused;
            if (SandboxInput.KeyPressed(KeyCode.Period)) { paused = true; stepOnce = true; }
            if (SandboxInput.KeyPressed(KeyCode.B)) { physical = !physical; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.G)) Shove();
            if (SandboxInput.KeyPressed(KeyCode.H)) Throw();
            if (SandboxInput.KeyPressed(KeyCode.K)) Shove(2.5f);
            if (SandboxInput.KeyPressed(KeyCode.N)) { body = body == BodySize.Small ? BodySize.Medium : body == BodySize.Medium ? BodySize.Large : BodySize.Small; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.V)) walkSpeed = walkSpeed > 1.2f ? 0.7f : walkSpeed > 0.9f ? 1.4f : 1.0f;
        }

        private void LateUpdate()
        {
            if (loco != null && orbitCamera != null)
            {
                Vector3 follow = loco.Position + Vector3.up * 1.0f;
                orbitCamera.target = Vector3.Lerp(orbitCamera.target, follow, 1f - Mathf.Exp(-4f * Time.deltaTime));
            }
        }

        // ---- Footprints ---------------------------------------------------------------------------------

        private readonly List<GameObject> prints = new List<GameObject>();
        private int nextPrint;
        private readonly bool[] wasLocked = new bool[2];
        private Material[] printMaterials;

        private void ClearPrints()
        {
            foreach (var p in prints)
            {
                if (p != null)
                {
                    Destroy(p);
                }
            }

            prints.Clear();
            nextPrint = 0;
            wasLocked[0] = wasLocked[1] = false;
        }

        private void Prints()
        {
            for (int f = 0; f < 2; f++)
            {
                bool locked = poser.Feet[f].Locked;
                if (footprints && locked && !wasLocked[f])
                {
                    var foot = anim.Part(f == 0 ? PartId.FootL : PartId.FootR);
                    Vector3 heel = foot.position;
                    Vector3 toe = foot.position + foot.rotation * new Vector3(0f, -0.06f, 0.16f);
                    Vector3 along = toe - heel;
                    along.y = 0f;
                    var mark = PrintMark(f);
                    mark.transform.position = new Vector3(0.5f * (heel.x + toe.x), origin.y + 0.006f, 0.5f * (heel.z + toe.z));
                    mark.transform.rotation = along.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(along) : Quaternion.identity;
                    mark.SetActive(true);
                }

                wasLocked[f] = locked;
            }
        }

        private GameObject PrintMark(int foot)
        {
            if (printMaterials == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }

                printMaterials = new Material[2];
                for (int i = 0; i < 2; i++)
                {
                    printMaterials[i] = new Material(shader) { color = i == 0 ? new Color(0.3f, 0.5f, 0.9f) : new Color(0.9f, 0.4f, 0.3f) };
                }
            }

            const int Most = 24;
            GameObject mark;
            if (prints.Count < Most)
            {
                mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = "Footprint";
                var collider = mark.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                mark.transform.localScale = new Vector3(0.07f, 0.005f, 0.2f);
                prints.Add(mark);
            }
            else
            {
                mark = prints[nextPrint % Most];
            }

            nextPrint++;
            mark.GetComponent<MeshRenderer>().sharedMaterial = printMaterials[foot];
            return mark;
        }

        // ---- The panel and the log ----------------------------------------------------------------------

        private void OnGUI()
        {
            if (loco == null)
            {
                return;
            }

            panel = GUILayout.Window(7711, panel, DrawPanel, "Locomotion lab");
        }

        private void DrawPanel(int id)
        {
            GUILayout.Label(course ? $"Course: {leg}" : "Driving: W A S D, Shift run, Space hold facing, V walk speed");
            GUILayout.Label($"{loco.Current}{(loco.Shot != null ? " " + loco.Shot.Name : "")}   {new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude:0.00} m/s   run {loco.RunWeight:0.00}   phase {loco.Phase:0.00}");
            GUILayout.Label($"{body}: walking speed {walkSpeed * PaceScale:0.0} m/s, running {RunSpeed:0.0} m/s");
            GUILayout.BeginHorizontal();
            foreach (BodySize size in new[] { BodySize.Small, BodySize.Medium, BodySize.Large })
            {
                if (GUILayout.Button(body == size ? "> " + size + " <" : size.ToString()) && body != size)
                {
                    body = size;
                    Build();
                }
            }

            GUILayout.EndHorizontal();
            if (down > 0f)
            {
                GUILayout.Label(note + $" (up again in {down:0.0} s)");
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(course ? "Drive (C)" : "Course (C)")) { course = !course; Build(); }
            if (GUILayout.Button("Restart (R)")) Build();
            if (GUILayout.Button(paused ? "Play (P)" : "Pause (P)")) paused = !paused;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shove (G)")) Shove();
            if (GUILayout.Button("Hard (K)")) Shove(2.5f);
            if (GUILayout.Button("Ball (H)")) Throw();
            if (GUILayout.Button(physical ? "Kinematic body (B)" : "Physical body (B)")) { physical = !physical; Build(); }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed");
            foreach (float s in new[] { 0.25f, 0.5f, 1f })
            {
                if (GUILayout.Button(s.ToString("0.##") + "x"))
                {
                    Time.timeScale = s;
                }
            }

            GUILayout.EndHorizontal();
            footprints = GUILayout.Toggle(footprints, "Footprints");
            GUILayout.Label($"Planted feet moved at most {measures.PlantDriftWorst * 100f:0.0} cm over {measures.PlantsDone} plants; {measures.ForcedReleases} forced steps.");
            GUI.DragWindow();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
            }
        }

        private void OnApplicationQuit() => WriteLog();

        private void OnDestroy() => WriteLog();

        private bool written;

        /// <summary>The run's measures, its legs and its film, into PlaytestLogs/locolab-(time)/.</summary>
        private void WriteLog()
        {
            if (written || measures == null || time < 3f)
            {
                return;
            }

            written = true;
            try
            {
                folder = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PlaytestLogs")), "locolab-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + (course ? "-course" : "-driven"));
                Directory.CreateDirectory(folder);
                var md = new StringBuilder();
                md.AppendLine("# Locomotion lab");
                md.AppendLine();
                md.AppendLine($"{time:0.0} s, {(course ? "the course" : "driven by hand")}. Draw the film with `python3 Tools/Mocap/Locomotion/locofilm.py {Path.GetFileName(folder)}/locolab.json film.gif` (from PlaytestLogs/).");
                md.AppendLine();
                md.AppendLine("## Measures");
                md.AppendLine();
                md.AppendLine(measures.Report());
                md.AppendLine("## Legs");
                md.AppendLine();
                foreach (var l in legs)
                {
                    md.AppendLine("- " + l);
                }

                File.WriteAllText(Path.Combine(folder, "session.md"), md.ToString());
                File.WriteAllText(Path.Combine(folder, "locolab.json"), film.ToString());
                Debug.Log("[LocomotionLab] wrote " + folder);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LocomotionLab] could not write the log: " + e.Message);
            }
        }
    }
}
