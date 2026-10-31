using System;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The rig description written by SourceArt/Bodies/Scripts/build_bodies.py (<body>_rig.json): every null
    /// with its parent and local position, and each body part's mass and collider. Unity axes,
    /// metres. Every null has identity rotation in the rest pose.
    /// </summary>
    [Serializable]
    public class RigFile
    {
        public string body;
        public float heightM;
        public float totalMassKg;
        public NullEntry[] nulls;
        public PartEntry[] parts;
        public MaterialEntry[] materials;

        public static RigFile Parse(string json)
        {
            var rig = JsonUtility.FromJson<RigFile>(json);
            if (rig == null || rig.nulls == null || rig.parts == null)
            {
                throw new FormatException("The rig file has no nulls or parts.");
            }

            return rig;
        }

        public NullEntry FindNull(string name)
        {
            foreach (var entry in nulls)
            {
                if (entry.name == name)
                {
                    return entry;
                }
            }

            return null;
        }

        public PartEntry FindPart(string name)
        {
            foreach (var entry in parts)
            {
                if (entry.name == name)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class NullEntry
    {
        public string name;
        public string parent;
        public float[] localPosition;

        public bool HasParent => !string.IsNullOrEmpty(parent);
        public Vector3 LocalPosition => RigMath.ToVector(localPosition);
    }

    [Serializable]
    public class PartEntry
    {
        public string name;
        public string mesh;
        public float massKg;
        public ColliderEntry collider;
    }

    [Serializable]
    public class ColliderEntry
    {
        public string type;
        public float[] center;
        public float[] size;
        public float radius;
        public float[] start;
        public float[] end;
        public float height;
    }

    [Serializable]
    public class MaterialEntry
    {
        public string name;
        public float[] color;

        public Color Color => color != null && color.Length >= 3 ? new Color(color[0], color[1], color[2]) : Color.gray;
    }

    /// <summary>
    /// The weapons file written by SourceArt/Weapons/Scripts/build_weapons.py (weapons.json): each
    /// weapon's length, mass, strike points and colliders, built for the medium body. Unity axes: the
    /// grip at the origin, blades along +Z, a sword's edges on +-Y, a shield's face along +Z.
    /// </summary>
    [Serializable]
    public class WeaponFile
    {
        public MaterialEntry[] materials;
        public WeaponFileEntry[] weapons;

        public static WeaponFile Parse(string json)
        {
            var file = JsonUtility.FromJson<WeaponFile>(json);
            if (file == null || file.weapons == null)
            {
                throw new FormatException("The weapons file has no weapons.");
            }

            return file;
        }

        public WeaponFileEntry Find(string name)
        {
            foreach (var entry in weapons)
            {
                if (entry.name == name)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class WeaponFileEntry
    {
        public string name;
        public string kind;
        public float lengthM;
        public float massKg;
        public float radius;
        public float[] faceCenter;
        public WeaponPoints points;
        public ColliderEntry[] colliders;
    }

    [Serializable]
    public class WeaponPoints
    {
        public float[] tip;
        public float[] edge;
        public float[] head;
    }

    public static class RigMath
    {
        public static Vector3 ToVector(float[] values)
        {
            if (values == null || values.Length < 3)
            {
                return Vector3.zero;
            }

            return new Vector3(values[0], values[1], values[2]);
        }
    }
}
