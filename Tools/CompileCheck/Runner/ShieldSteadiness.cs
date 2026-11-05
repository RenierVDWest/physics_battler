// How steady the shield is while a blow comes (the playtest: it moved and turned back and forth as the blade came):
// the board's centre and face in the body's own frame, from the blow first coming to its end, against the straight move.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PhysicsBattler.Locomotion;
using UnityEngine;

internal sealed class ShieldSteadiness
{
    private bool open;
    private Vector3 startC, startN, lastC, lastN, lastD;
    private float path, turn;
    private int reversals, turnBacks;
    private Vector3 lastTurnAxis;

    /// <summary>Per blow: the board's path over its straight move (1: straight), how far the face turned over its net turn (degrees), and how often it went back on itself.</summary>
    public readonly List<(float path, float net, float turn, float netTurn, int reversals, int turnBacks)> Blows = new List<(float, float, float, float, int, int)>();

    private bool done;
    private int lastSerial = -1;
    private int contactsWere = -1;

    /// <summary>One step: from the blow first coming to its first contact (the knock after it is the blow's, not the shield's own moving).</summary>
    public void Step(ClipFighter d)
    {
        bool coming = d.IncomingBlade.HasValue;
        int contacts = d.Blocks + d.Hits;
        bool contacted = contactsWere >= 0 && contacts != contactsWere;
        contactsWere = contacts;
        int serial = d.Blow.HasValue ? d.Blow.Value.Serial : -1;
        if (!coming || serial != lastSerial)
        {
            done = false;
            if (serial != lastSerial)
            {
                Close();
            }
        }

        lastSerial = serial;

        if (!coming || done || contacted || !d.ShieldBoard(out var c, out var n, out _))
        {
            Close();
            done |= contacted;
            return;
        }

        var back = Quaternion.Inverse(Quaternion.Euler(0f, d.Loco.Facing, 0f));
        c = back * (c - d.Loco.Position);
        n = back * n;
        if (!open)
        {
            open = true;
            startC = lastC = c;
            startN = lastN = n;
            lastD = Vector3.zero;
            lastTurnAxis = Vector3.zero;
            path = turn = 0f;
            reversals = turnBacks = 0;
            return;
        }

        Vector3 dc = c - lastC;
        path += dc.magnitude;
        if (dc.magnitude > 0.003f && lastD.magnitude > 0.003f && Vector3.Dot(dc, lastD) < 0f)
        {
            reversals++;
        }

        if (dc.magnitude > 0.003f)
        {
            lastD = dc;
        }

        float a = Vector3.Angle(lastN, n);
        turn += a;
        Vector3 axis = Vector3.Cross(lastN, n);
        if (a > 0.5f && lastTurnAxis.sqrMagnitude > 0f && Vector3.Dot(axis, lastTurnAxis) < 0f)
        {
            turnBacks++;
        }

        if (a > 0.5f)
        {
            lastTurnAxis = axis;
        }

        lastC = c;
        lastN = n;
    }

    public void Close()
    {
        if (open && path > 0.01f)
        {
            Blows.Add((path, Vector3.Distance(startC, lastC), turn, Vector3.Angle(startN, lastN), reversals, turnBacks));
            if (Environment.GetEnvironmentVariable("SHIELD_STEADY_EACH") != null)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    steadiness: moved {0:0.00} m to get {1:0.00}, back {2}; turned {3:0} deg to turn {4:0}, back {5}", path, Vector3.Distance(startC, lastC), reversals, turn, Vector3.Angle(startN, lastN), turnBacks));
            }
        }

        open = false;
    }

    public static void Print(IEnumerable<ShieldSteadiness> all)
    {
        var ci = CultureInfo.InvariantCulture;
        var blows = all.SelectMany(s => s.Blows).ToList();
        if (blows.Count == 0)
        {
            return;
        }

        float Median(IEnumerable<float> xs) { var s = xs.OrderBy(x => x).ToList(); return s[s.Count / 2]; }
        Console.WriteLine(string.Format(ci,
            "The shield's steadiness over {0} blows: the board moved median {1:0.00} m to get {2:0.00} m (most {3:0.00} to {4:0.00}), went back on itself median {5:0}, most {6} times; the face turned median {7:0} deg to turn {8:0} (most {9:0} to {10:0}), turned back median {11:0}, most {12} times",
            blows.Count, Median(blows.Select(b => b.path)), Median(blows.Select(b => b.net)), blows.Max(b => b.path), blows.OrderBy(b => b.path).Last().net,
            Median(blows.Select(b => (float)b.reversals)), blows.Max(b => b.reversals),
            Median(blows.Select(b => b.turn)), Median(blows.Select(b => b.netTurn)), blows.Max(b => b.turn), blows.OrderBy(b => b.turn).Last().netTurn,
            Median(blows.Select(b => (float)b.turnBacks)), blows.Max(b => b.turnBacks)));
    }
}
