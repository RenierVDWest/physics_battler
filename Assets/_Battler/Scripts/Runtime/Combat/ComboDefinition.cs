using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>What a combo does when one of its links is blocked or stopped.</summary>
    public enum ComboBlocked
    {
        /// <summary>The combo ends with the stopped link.</summary>
        Stop,

        /// <summary>It keeps hammering: the next link comes as the stopped one rebounds.</summary>
        Continue,
    }

    /// <summary>Which bodies (fighting styles) use a combo.</summary>
    [System.Flags]
    public enum ComboBodies
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Large = 4,
        All = Small | Medium | Large,
    }

    /// <summary>One blow of a combo: the move and the parts it goes for.</summary>
    [System.Serializable]
    public struct ComboLink
    {
        public MoveStyle move;
        public AimParts prefer;

        public ComboLink(MoveStyle move, AimParts prefer)
        {
            this.move = move;
            this.prefer = prefer;
        }
    }

    /// <summary>
    /// Milestone 10: a combo, a string of chained blows. Each link after the first starts from the
    /// last one's follow-through (its link-out, just after contact) and skips part of its own wind-up
    /// (its link-in), so the links come about half a second apart instead of the seconds between
    /// single blows. Only the last link plays its recovery: a whole combo carries the commitment of
    /// one blow. The strings are built from the moves each weapon has (the plan's M9a set: body then
    /// head, double thrust, rompida, and a few more from the same moves).
    /// </summary>
    public sealed class ComboDefinition
    {
        public string name;
        public ComboBodies bodies = ComboBodies.All;
        public ComboLink[] links;
        public ComboBlocked ifBlocked = ComboBlocked.Stop;

        /// <summary>Kept for a full gather or a target that is reeling (its finisher).</summary>
        public bool finisher;

        /// <summary>The reference this string is drawn from (for the README and the log).</summary>
        public string reference;

        /// <summary>A link's move found among the weapon's moves (null if it hasn't it).</summary>
        public static MoveDefinition MoveFor(WeaponDefinition weapon, MoveStyle style)
        {
            if (weapon == null || weapon.moves == null)
            {
                return null;
            }

            string suffix = style.ToString();
            foreach (var move in weapon.moves)
            {
                if (move != null && (move.name == suffix || move.name.EndsWith("_" + suffix)))
                {
                    return move;
                }
            }

            return null;
        }

        /// <summary>Whether the weapon has every move of the string.</summary>
        public bool Fits(WeaponDefinition weapon)
        {
            foreach (var link in links)
            {
                if (MoveFor(weapon, link.move) == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The body a unit of this size fights as.</summary>
        public static ComboBodies BodyOf(float sizeFactor) => sizeFactor < 0.8f ? ComboBodies.Small : sizeFactor > 1.1f ? ComboBodies.Large : ComboBodies.Medium;

        /// <summary>The strings, every style's.</summary>
        public static readonly ComboDefinition[] All =
        {
            // Large (the boxer with the club).
            new ComboDefinition
            {
                name = "body then head", bodies = ComboBodies.Large, reference = "Boxing 1-6-3",
                links = new[] { new ComboLink(MoveStyle.ClubRising, AimParts.Belly | AimParts.Knees), new ComboLink(MoveStyle.ClubSwing, AimParts.Head) },
            },
            new ComboDefinition
            {
                name = "hammer", bodies = ComboBodies.Large, reference = "Boxing 1-2", ifBlocked = ComboBlocked.Continue,
                links = new[] { new ComboLink(MoveStyle.ClubSwing, AimParts.Head | AimParts.Chest), new ComboLink(MoveStyle.ClubSmash, AimParts.Head) },
            },
            new ComboDefinition
            {
                name = "body, head, smash", bodies = ComboBodies.Large, reference = "Melee Combo Ver. 1", finisher = true,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubRising, AimParts.Belly), new ComboLink(MoveStyle.ClubSwing, AimParts.Head),
                    new ComboLink(MoveStyle.ClubSmash, AimParts.Head | AimParts.Chest),
                },
            },

            new ComboDefinition
            {
                name = "hook, backhand, smash", bodies = ComboBodies.Large, reference = "Melee Combo Ver. 1", finisher = true,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head | AimParts.Chest), new ComboLink(MoveStyle.ClubBackhand, AimParts.Chest | AimParts.Neck),
                    new ComboLink(MoveStyle.ClubSmash, AimParts.Head | AimParts.Chest),
                },
            },
            new ComboDefinition
            {
                name = "2-3-2", bodies = ComboBodies.Large, reference = "Boxing 2-3-2", ifBlocked = ComboBlocked.Continue,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head), new ComboLink(MoveStyle.ClubBackhand, AimParts.Belly | AimParts.Chest),
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head | AimParts.Neck),
                },
            },

            // Medium (sword and shield).
            new ComboDefinition
            {
                name = "double thrust", bodies = ComboBodies.Medium, reference = "Melee Combo Ver. 3",
                links = new[]
                {
                    new ComboLink(MoveStyle.SwordThrust, AimParts.Chest | AimParts.Belly), new ComboLink(MoveStyle.SwordBackhand, AimParts.Head | AimParts.Neck | AimParts.Chest),
                    new ComboLink(MoveStyle.SwordThrust, AimParts.Belly | AimParts.Chest),
                },
            },
            new ComboDefinition
            {
                name = "forehand, backhand", bodies = ComboBodies.Medium, reference = "Sword and shield tactics", ifBlocked = ComboBlocked.Continue,
                links = new[] { new ComboLink(MoveStyle.SwordCut, AimParts.Head | AimParts.Neck), new ComboLink(MoveStyle.SwordBackhand, AimParts.Chest | AimParts.Belly) },
            },
            new ComboDefinition
            {
                name = "cut, backhand, cleave", bodies = ComboBodies.Medium, reference = "Melee Combo Ver. 2", finisher = true,
                links = new[]
                {
                    new ComboLink(MoveStyle.SwordCut, AimParts.Chest | AimParts.Neck), new ComboLink(MoveStyle.SwordBackhand, AimParts.Belly | AimParts.Chest),
                    new ComboLink(MoveStyle.SwordCleave, AimParts.Head | AimParts.Chest),
                },
            },

            // Small (eskrima with the stick).
            new ComboDefinition
            {
                name = "rompida", bodies = ComboBodies.Small, reference = "FMA glossary",
                links = new[] { new ComboLink(MoveStyle.ClubRising, AimParts.Chest | AimParts.Elbows), new ComboLink(MoveStyle.ClubSmash, AimParts.Head | AimParts.Chest) },
            },
            new ComboDefinition
            {
                name = "flowing", bodies = ComboBodies.Small, reference = "Dual Weapon Combo", ifBlocked = ComboBlocked.Continue,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head | AimParts.Elbows), new ComboLink(MoveStyle.ClubRising, AimParts.Elbows | AimParts.Belly),
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head | AimParts.Chest),
                },
            },
            new ComboDefinition
            {
                name = "sero-sero", bodies = ComboBodies.Small, reference = "Melee Combo Ver. 2", finisher = true,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Elbows | AimParts.Head), new ComboLink(MoveStyle.ClubRising, AimParts.Chest),
                    new ComboLink(MoveStyle.ClubSwing, AimParts.Head), new ComboLink(MoveStyle.ClubSmash, AimParts.Head | AimParts.Chest),
                },
            },
        };

        /// <summary>Milestone 10: the strings the club's new backhand and fan strikes make for the small body.</summary>
        public static readonly ComboDefinition[] SmallFans =
        {
            new ComboDefinition
            {
                name = "banda y banda", bodies = ComboBodies.Small, reference = "FMA glossary", ifBlocked = ComboBlocked.Continue,
                links = new[] { new ComboLink(MoveStyle.ClubFan, AimParts.Head | AimParts.ElbowR), new ComboLink(MoveStyle.ClubFanBackhand, AimParts.Head | AimParts.ElbowL) },
            },
            new ComboDefinition
            {
                name = "abanico flurry", bodies = ComboBodies.Small, reference = "Dual Weapon Combo", ifBlocked = ComboBlocked.Continue, finisher = true,
                links = new[]
                {
                    new ComboLink(MoveStyle.ClubFan, AimParts.Head), new ComboLink(MoveStyle.ClubFanBackhand, AimParts.Elbows),
                    new ComboLink(MoveStyle.ClubFan, AimParts.Head | AimParts.Neck), new ComboLink(MoveStyle.ClubFanBackhand, AimParts.Elbows | AimParts.Head),
                    new ComboLink(MoveStyle.ClubSmash, AimParts.Head | AimParts.Chest),
                },
            },
        };

        /// <summary>Every string.</summary>
        public static IEnumerable<ComboDefinition> Every
        {
            get
            {
                foreach (var c in All)
                {
                    yield return c;
                }

                foreach (var c in SmallFans)
                {
                    yield return c;
                }
            }
        }

        /// <summary>The strings a unit of this size with this weapon can throw.</summary>
        public static List<ComboDefinition> For(WeaponDefinition weapon, float sizeFactor)
        {
            var list = new List<ComboDefinition>();
            var body = BodyOf(sizeFactor);
            foreach (var combo in Every)
            {
                if ((combo.bodies & body) != 0 && combo.Fits(weapon))
                {
                    list.Add(combo);
                }
            }

            // (A weapon with none of its body's strings takes any that fit it.)
            if (list.Count == 0)
            {
                foreach (var combo in Every)
                {
                    if (combo.Fits(weapon))
                    {
                        list.Add(combo);
                    }
                }
            }

            return list;
        }

        /// <summary>Where a chained link starts (normalized time): three quarters of the way through its wind-up.</summary>
        public static float LinkIn(MoveDefinition move) => LinkInShare * move.WindUpEnd;

        /// <summary>Where a link is cut for the next (normalized time): a little into its follow-through.</summary>
        public static float LinkOut(MoveDefinition move, float contactTime) => contactTime + LinkOutShare * Mathf.Max(0f, move.SwingFollowEnd - contactTime);

        /// <summary>How much of the wind-up a chained link skips; how far into the follow-through a link is cut.</summary>
        public const float LinkInShare = 0.75f, LinkOutShare = 0.15f;
    }
}
