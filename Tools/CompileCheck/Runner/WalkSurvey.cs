using System;
using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using UnityEngine;

/// <summary>
/// The walk's recordings set side by side (the plan "Convincing walking"): for every cycle and stand the standard
/// style walks on, its pace, stride, how long each foot is down, how high it carries its hips, where it keeps its
/// feet, how far it leans; and how far apart each two of them are as poses, at the same moment of the step. Two
/// cycles far apart are two the walk should not blend half and half.
/// </summary>
public static class WalkSurvey
{
    private const float Leg = 0.85f;

    /// <summary>
    /// Legs from other packs, tried for the guard's steps (the packs of 17 November; tools: pack_survey.py): each
    /// with its legs under the guard's own chest and arms, set against the guard's stand. What matters: which way it
    /// goes against where he looks, how far its hips are turned from that (the guard's: 55 to 70 degrees), how high
    /// it carries them, and how far its feet's own places are from the stand's.
    /// </summary>
    private static void Candidates(Locomotor.ClipSet set, Vector3[] stand)
    {
        var plain = System.Globalization.CultureInfo.InvariantCulture;
        const int samples = 24;
        string[] names =
        {
            "sns_walk", "sns_walk_back", "sns_step_left", "sns_step_right",
            "bow_walk_fwd", "bow_walk_back", "bow_walk_left", "bow_walk_right",
            "axe_walk_fwd", "axe_walk_back", "axe_walk_left", "axe_walk_right",
            "magic_walk_fwd", "magic_walk_back", "magic_walk_left", "magic_walk_right",
            "rifle_walk_fwd", "rifle_walk_fwd_right", "rifle_walk_right", "rifle_walk_back_right", "rifle_walk_back", "rifle_walk_back_left", "rifle_walk_left", "rifle_walk_fwd_left",
        };
        float standYaw = HipsYaw(stand);
        Console.WriteLine(string.Format(plain, "CANDIDATE the stand: its hips turned {0:0} deg from where he looks, {1:0.0} cm high", standYaw, 100f * stand[(int)LocoBone.Hips].y));
        Console.WriteLine("CANDIDATE name | goes (deg from where he looks) | pace m/s | cycle s | hips turned deg | hips high cm (against the stand) | its legs from the stand's cm | left foot (across, ahead) cm | right foot (across, ahead) cm");
        var at = new Vector3[LocoClip.BoneCount];
        foreach (string name in names)
        {
            var legs = LocoLibrary.Get(name);
            if (legs == null)
            {
                Console.WriteLine("CANDIDATE " + name + " | not in the library");
                continue;
            }

            var clip = name.StartsWith("sns_") ? legs : MoveStyle.LegsUnder(legs, set.StepL ?? set.WalkF, "tried_" + name) ?? legs;
            var mean = new Vector3[LocoClip.BoneCount];
            float yaw = 0f;
            for (int i = 0; i < samples; i++)
            {
                clip.Pose(clip.HasPhase ? clip.TimeAtPhase((i + 0.5f) / samples) : clip.Duration * (i + 0.5f) / samples, at, Leg);
                for (int k = 0; k < LocoClip.BoneCount; k++)
                {
                    mean[k] += at[k] / samples;
                }

                yaw += HipsYaw(at) / samples;
            }

            // (The legs alone: the hips and both legs' points.)
            LocoBone[] lower = { LocoBone.Hips, LocoBone.LeftUpLeg, LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeBase, LocoBone.RightUpLeg, LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeBase };
            float sum = 0f;
            foreach (var bone in lower)
            {
                sum += (mean[(int)bone] - stand[(int)bone]).sqrMagnitude;
            }

            Vector3 l = mean[(int)LocoBone.LeftFoot] - stand[(int)LocoBone.LeftFoot], r = mean[(int)LocoBone.RightFoot] - stand[(int)LocoBone.RightFoot];
            Console.WriteLine(string.Format(plain, "CANDIDATE {0} | {1:+0;-0} | {2:0.00} | {3:0.00} | {4:0} | {5:0.0} ({6:+0.0;-0.0}) | {7:0.0} | {8:+0;-0}, {9:+0;-0} | {10:+0;-0}, {11:+0;-0}",
                name, clip.Direction, clip.Speed * Leg, clip.Duration, yaw, 100f * mean[(int)LocoBone.Hips].y, 100f * (mean[(int)LocoBone.Hips].y - stand[(int)LocoBone.Hips].y), 100f * Mathf.Sqrt(sum / lower.Length), 100f * l.x, 100f * l.z, 100f * r.x, 100f * r.z));
        }
    }

    /// <summary>
    /// Two sets of four set side by side: for each pair of neighbouring ways (forward and left, forward and right,
    /// back and left, back and right), how far apart the two recordings are as poses at the same moment of the step,
    /// and how far apart their carriages alone.
    /// </summary>
    private static void Sets(Locomotor.ClipSet set)
    {
        var plain = System.Globalization.CultureInfo.InvariantCulture;
        const int samples = 24;
        LocoClip Legs(string name) => name.StartsWith("sns_") ? LocoLibrary.Get(name) : MoveStyle.LegsUnder(LocoLibrary.Get(name), set.StepL ?? set.WalkF, "set_" + name);
        var sets = new (string name, string[] four)[]
        {
            ("today's (the pack's own walk forward and back)", new[] { "sns_walk", "sns_walk_back", "sns_step_left", "sns_step_right" }),
            ("with the archer's walk forward and back", new[] { "bow_walk_fwd", "bow_walk_back", "sns_step_left", "sns_step_right" }),
            ("the archer's four", new[] { "bow_walk_fwd", "bow_walk_back", "bow_walk_left", "bow_walk_right" }),
            ("the rifleman's four", new[] { "rifle_walk_fwd", "rifle_walk_back", "rifle_walk_left", "rifle_walk_right" }),
        };
        Console.WriteLine("SET name | neighbours apart as poses cm (forward-left, forward-right, back-left, back-right; mean) | their carriages apart cm (mean) | hips' heights cm (forward, back, left, right) | hips turned deg (forward, back, left, right)");
        foreach (var (name, four) in sets)
        {
            var poses = new Vector3[4][][];
            var means = new Vector3[4][];
            var yaws = new float[4];
            bool whole = true;
            for (int c = 0; c < 4; c++)
            {
                var clip = Legs(four[c]);
                if (clip == null || !clip.HasPhase)
                {
                    whole = false;
                    break;
                }

                poses[c] = new Vector3[samples][];
                means[c] = new Vector3[LocoClip.BoneCount];
                for (int i = 0; i < samples; i++)
                {
                    poses[c][i] = new Vector3[LocoClip.BoneCount];
                    clip.Pose(clip.TimeAtPhase((i + 0.5f) / samples), poses[c][i], Leg);
                    for (int k = 0; k < LocoClip.BoneCount; k++)
                    {
                        means[c][k] += poses[c][i][k] / samples;
                    }

                    yaws[c] += HipsYaw(poses[c][i]) / samples;
                }
            }

            if (!whole)
            {
                Console.WriteLine("SET " + name + " | a recording is missing or has no step");
                continue;
            }

            int[][] pairs = { new[] { 0, 2 }, new[] { 0, 3 }, new[] { 1, 2 }, new[] { 1, 3 } };
            var apart = new float[4];
            float meanApart = 0f, meanCarriage = 0f;
            for (int p = 0; p < 4; p++)
            {
                int a = pairs[p][0], b = pairs[p][1];
                float carriage = 0f;
                for (int k = 0; k < LocoClip.BoneCount; k++)
                {
                    carriage += (means[a][k] - means[b][k]).sqrMagnitude;
                }

                meanCarriage += Mathf.Sqrt(carriage / LocoClip.BoneCount) / 4f;
                for (int i = 0; i < samples; i++)
                {
                    float sum = 0f;
                    for (int k = 0; k < LocoClip.BoneCount; k++)
                    {
                        sum += (poses[a][i][k] - poses[b][i][k]).sqrMagnitude;
                    }

                    apart[p] += Mathf.Sqrt(sum / LocoClip.BoneCount) / samples;
                }

                meanApart += apart[p] / 4f;
            }

            Console.WriteLine(string.Format(plain, "SET {0} | {1:0.0}, {2:0.0}, {3:0.0}, {4:0.0}; {5:0.0} | {6:0.0} | {7:0.0}, {8:0.0}, {9:0.0}, {10:0.0} | {11:0}, {12:0}, {13:0}, {14:0}",
                name, 100f * apart[0], 100f * apart[1], 100f * apart[2], 100f * apart[3], 100f * meanApart, 100f * meanCarriage,
                100f * means[0][(int)LocoBone.Hips].y, 100f * means[1][(int)LocoBone.Hips].y, 100f * means[2][(int)LocoBone.Hips].y, 100f * means[3][(int)LocoBone.Hips].y, yaws[0], yaws[1], yaws[2], yaws[3]));
        }
    }

    /// <summary>How far the hips are turned from the clip's front (degrees, + to his right).</summary>
    private static float HipsYaw(Vector3[] pose)
    {
        Vector3 across = pose[(int)LocoBone.RightUpLeg] - pose[(int)LocoBone.LeftUpLeg];
        return Mathf.Atan2(-across.z, across.x) * Mathf.Rad2Deg;
    }

    public static int Run()
    {
        LocoLibrary.Load();
        var set = MoveStyle.Standard.Clips();
        var clips = new List<(string job, LocoClip clip)>
        {
            ("stand", set.Idle), ("walk forward", set.WalkF), ("walk back", set.WalkB), ("step left", set.StepL), ("step right", set.StepR),
            ("slow left", set.SlowL), ("slow right", set.SlowR), ("sneak left", set.SneakL), ("sneak right", set.SneakR),
            ("jog forward", set.RunF), ("jog back", set.RunB), ("jog left", set.RunL), ("jog right", set.RunR),
        };
        clips.RemoveAll(c => c.clip == null);
        var plain = System.Globalization.CultureInfo.InvariantCulture;
        const int samples = 24;
        var poses = new Dictionary<LocoClip, Vector3[][]>();
        Console.WriteLine("CLIP job | name | pace m/s | stride m | cycle s | down L,R | hips high cm (low..high) | lean deg | feet apart cm | left foot ahead cm | chest turned deg");
        foreach (var (job, clip) in clips)
        {
            var at = new Vector3[samples][];
            float low = 1e9f, high = -1e9f, mean = 0f, lean = 0f, apart = 0f, ahead = 0f, turned = 0f;
            for (int i = 0; i < samples; i++)
            {
                at[i] = new Vector3[LocoClip.BoneCount];
                float t = clip.HasPhase ? clip.TimeAtPhase((i + 0.5f) / samples) : clip.Duration * (i + 0.5f) / samples;
                clip.Pose(t, at[i], Leg);
                Vector3 hips = at[i][(int)LocoBone.Hips], chest = at[i][(int)LocoBone.Spine2];
                low = Mathf.Min(low, hips.y);
                high = Mathf.Max(high, hips.y);
                mean += hips.y / samples;
                Vector3 up = chest - hips;
                lean += Mathf.Atan2(up.z, up.y) * Mathf.Rad2Deg / samples;
                Vector3 l = at[i][(int)LocoBone.LeftFoot], r = at[i][(int)LocoBone.RightFoot];
                apart += Mathf.Abs(r.x - l.x) / samples;
                ahead += (l.z - r.z) / samples;
                Vector3 across = at[i][(int)LocoBone.RightArm] - at[i][(int)LocoBone.LeftArm];
                turned += Mathf.Atan2(-across.z, across.x) * Mathf.Rad2Deg / samples;
            }

            poses[clip] = at;
            Console.WriteLine(string.Format(plain, "CLIP {0} | {1} | {2:0.00} | {3:0.00} | {4:0.00} | {5:0.00},{6:0.00} | {7:0.0} ({8:0.0}..{9:0.0}) | {10:0} | {11:0} | {12:0} | {13:0}",
                job, clip.Name, clip.Speed * Leg, clip.Stride * Leg, clip.Duration, clip.Duty[0], clip.Duty[1], 100f * mean, 100f * low, 100f * high, lean, 100f * apart, 100f * ahead, turned));
        }

        // (How far apart two are as poses: every point against the root, at the same moment of the step, the mean over
        // the round of the root-mean-square over the points; and the furthest any one point is apart at any moment.)
        Console.WriteLine("APART a | b | mean cm | most cm (which point) | hips' height apart cm");
        for (int a = 0; a < clips.Count; a++)
        {
            for (int b = a + 1; b < clips.Count; b++)
            {
                var pa = poses[clips[a].clip];
                var pb = poses[clips[b].clip];
                bool phased = clips[a].clip.HasPhase && clips[b].clip.HasPhase;
                float best = 1e9f, bestMost = 0f, bestHips = 0f;
                int bestBone = 0;
                // (A stand has no step: it is set against the cycle's nearest moment, shift by shift.)
                for (int shift = 0; shift < (phased ? 1 : samples); shift++)
                {
                    float mean = 0f, most = 0f, hips = 0f;
                    int bone = 0;
                    for (int i = 0; i < samples; i++)
                    {
                        float sum = 0f;
                        var x = pa[phased ? i : 0];
                        var y = pb[(i + shift) % samples];
                        for (int k = 0; k < LocoClip.BoneCount; k++)
                        {
                            float d = (x[k] - y[k]).magnitude;
                            sum += d * d;
                            if (d > most)
                            {
                                most = d;
                                bone = k;
                            }
                        }

                        mean += Mathf.Sqrt(sum / LocoClip.BoneCount) / samples;
                        hips += (x[(int)LocoBone.Hips].y - y[(int)LocoBone.Hips].y) / samples;
                        if (!phased)
                        {
                            // (One moment of the cycle against the stand.)
                            mean *= samples;
                            hips *= samples;
                            break;
                        }
                    }

                    if (mean < best)
                    {
                        best = mean;
                        bestMost = most;
                        bestBone = bone;
                        bestHips = hips;
                    }
                }

                Console.WriteLine(string.Format(plain, "APART {0} | {1} | {2:0.0} | {3:0.0} ({4}) | {5:+0.0;-0.0}", clips[a].job, clips[b].job, 100f * best, 100f * bestMost, (LocoBone)bestBone, 100f * bestHips));
            }
        }

        // (How much of that is carriage, how much the step itself: each recording's own mean place of every point over
        // its round taken off first, so only what moves is set side by side. What is left is what a walk rebuilt on
        // one carriage would still have to blend.)
        var means = new Dictionary<LocoClip, Vector3[]>();
        foreach (var (job, clip) in clips)
        {
            var mean = new Vector3[LocoClip.BoneCount];
            foreach (var pose in poses[clip])
            {
                for (int k = 0; k < LocoClip.BoneCount; k++)
                {
                    mean[k] += pose[k] / samples;
                }
            }

            means[clip] = mean;
        }

        Console.WriteLine("MOTION a | b | as recorded cm | their carriages apart cm | the steps alone cm");
        for (int a = 1; a < clips.Count; a++)
        {
            for (int b = a + 1; b < clips.Count; b++)
            {
                if (!clips[a].clip.HasPhase || !clips[b].clip.HasPhase)
                {
                    continue;
                }

                var pa = poses[clips[a].clip];
                var pb = poses[clips[b].clip];
                var ma = means[clips[a].clip];
                var mb = means[clips[b].clip];
                float whole = 0f, moving = 0f, carriage = 0f;
                for (int k = 0; k < LocoClip.BoneCount; k++)
                {
                    carriage += (ma[k] - mb[k]).sqrMagnitude;
                }

                carriage = Mathf.Sqrt(carriage / LocoClip.BoneCount);
                for (int i = 0; i < samples; i++)
                {
                    float sw = 0f, sm = 0f;
                    for (int k = 0; k < LocoClip.BoneCount; k++)
                    {
                        sw += (pa[i][k] - pb[i][k]).sqrMagnitude;
                        sm += ((pa[i][k] - ma[k]) - (pb[i][k] - mb[k])).sqrMagnitude;
                    }

                    whole += Mathf.Sqrt(sw / LocoClip.BoneCount) / samples;
                    moving += Mathf.Sqrt(sm / LocoClip.BoneCount) / samples;
                }

                Console.WriteLine(string.Format(plain, "MOTION {0} | {1} | {2:0.0} | {3:0.0} | {4:0.0}", clips[a].job, clips[b].job, 100f * whole, 100f * carriage, 100f * moving));
            }
        }

        Candidates(set, means[clips[0].clip]);
        Sets(set);

        // (And the stand against each cycle's own carriage: what setting off and stopping have to cross.)
        {
            var stand = means[clips[0].clip];
            Console.WriteLine("STANDTO cycle | the stand from its carriage cm | hips cm | left foot (across, ahead) cm | right foot (across, ahead) cm");
            for (int a = 1; a < clips.Count; a++)
            {
                var m = means[clips[a].clip];
                float sum = 0f;
                for (int k = 0; k < LocoClip.BoneCount; k++)
                {
                    sum += (m[k] - stand[k]).sqrMagnitude;
                }

                Vector3 l = m[(int)LocoBone.LeftFoot] - stand[(int)LocoBone.LeftFoot], r = m[(int)LocoBone.RightFoot] - stand[(int)LocoBone.RightFoot];
                Console.WriteLine(string.Format(plain, "STANDTO {0} | {1:0.0} | {2:+0.0;-0.0} | {3:+0;-0}, {4:+0;-0} | {5:+0;-0}, {6:+0;-0}", clips[a].job, 100f * Mathf.Sqrt(sum / LocoClip.BoneCount), 100f * (m[(int)LocoBone.Hips].y - stand[(int)LocoBone.Hips].y), 100f * l.x, 100f * l.z, 100f * r.x, 100f * r.z));
            }
        }

        return 0;
    }
}
