using System;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The rig description written by SourceArt/Orc/Scripts/build_orc.py (orc_rig.json): every null
    /// with its parent and local position, each body part's mass and collider, and the weapon's
    /// markers and colliders. Unity axes, metres. Every null has identity rotation in the rest pose.
    /// </summary>
    [Serializable]
    public class RigFile
    {
        public float heightM;
        public float totalMassKg;
        public NullEntry[] nulls;
        public PartEntry[] parts;
        public MaterialEntry[] materials;
        public WeaponEntry weapon;

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

    [Serializable]
    public class WeaponEntry
    {
        public string name;
        public string mesh;
        public string slot;
        public string attachTo;
        public float lengthM;
        public float physicsMassKg;
        public float gameplayMassKg;
        public WeaponMarkers markers;
        public ColliderEntry[] colliders;
    }

    [Serializable]
    public class WeaponMarkers
    {
        public float[] pommel_end;
        public float[] sweet_spot;
        public float[] tip;
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
