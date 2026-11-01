// `windows`: every move's timing as a defender sees it (milestone 7a, for the dodge design in Plan 2
// of the combat-depth plan). For each move on each body: the whole blow, the end of its wind-up,
// the start of its drive (the body turns forward: today's lunge steps), and contact, in seconds,
// and the time left from each of those to contact. A dodge needs about 0.2-0.3 s to take a head
// off the line, so a blow whose drive-to-contact is shorter can only be dodged before the drive.
using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Units;
using UnityEngine;

internal static class WindowReport
{
    public static int Run()
    {
        var sword = SwingReport.Sword();
        var club = SwingReport.Club();
        Console.WriteLine("move             body   | blow s | wind-up end s | drive s | contact s | wind-up end->contact s | drive->contact s");
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            var unit = SwingReport.Build(body, Vector3.zero, 0f, 0);
            float size = unit.Body.SizeFactor;
            foreach (var weapon in new[] { sword, club })
            {
                foreach (var move in weapon.moves)
                {
                    float duration = move.Duration * Mathf.Sqrt(size);
                    float contact = move.ContactTime();
                    float drive = MovePlanner.DriveStart(move, contact);
                    float windUp = move.WindUpEnd;
                    Console.WriteLine($"{move.name,-16} {body,-6} | {duration,6:0.00} | {windUp * duration,13:0.00} | {drive * duration,7:0.00} | {contact * duration,9:0.00} | "
                                      + $"{(contact - windUp) * duration,22:0.00} | {(contact - drive) * duration,16:0.00}");
                }
            }

            UnityEngine.Object.DestroyImmediate(unit.gameObject);
        }

        return 0;
    }
}
