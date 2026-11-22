using System;
using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The drawn body, baked (the plan "Clothes that never go into the body", step 1, 22 November): for each body part
    /// of a size, how far a point in the part's own space is from the part's drawn surface (negative inside), from a
    /// grid baked from the mesh files by <c>Tools/Cloth/bake_body.py</c>; and for the parts clothes cover, the mesh's
    /// triangles, for an exact inside-or-outside test that does not use the grid. Medium only for now. All in metres
    /// at the size's own scale 1 (a Large or a Small scales its queries).
    /// </summary>
    public sealed class BodyFields
    {
        [Serializable]
        private class FileData
        {
            public string body;
            public float cell;
            public PartData[] parts;
        }

        [Serializable]
        private class PartData
        {
            public string name;
            public float[] min;
            public int nx, ny, nz;
            public string sdf;
            public string tris;
            public float[] meshMin, meshMax;
        }

        /// <summary>One part's baked field and triangles.</summary>
        public sealed class Field
        {
            public PartId Part;
            public Vector3 Min, MeshMin, MeshMax;
            public float Cell;
            public int Nx, Ny, Nz;
            public short[] Grid; // tenths of a millimetre, z then y then x
            public float[] Tris; // 9 floats a triangle, part space
            public Vector3 Centre;
            public float Bound; // the radius round Centre that holds the whole grid

            public float At(int x, int y, int z) => Grid[(z * Ny + y) * Nx + x] * 0.0001f;

            /// <summary>Distance from the drawn surface (metres, negative inside), at a point in part space, by the grid.</summary>
            public float Distance(Vector3 p)
            {
                float fx = (p.x - Min.x) / Cell, fy = (p.y - Min.y) / Cell, fz = (p.z - Min.z) / Cell;
                // (Outside the grid: the grid's edge value plus the way out to it; the grid's margin is 4 cm, so a
                // point out there is outside the part.)
                float cx = Mathf.Clamp(fx, 0f, Nx - 1.001f), cy = Mathf.Clamp(fy, 0f, Ny - 1.001f), cz = Mathf.Clamp(fz, 0f, Nz - 1.001f);
                float extra = Cell * Mathf.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy) + (fz - cz) * (fz - cz));
                int ix = (int)cx, iy = (int)cy, iz = (int)cz;
                float tx = cx - ix, ty = cy - iy, tz = cz - iz;
                float c000 = At(ix, iy, iz), c100 = At(ix + 1, iy, iz), c010 = At(ix, iy + 1, iz), c110 = At(ix + 1, iy + 1, iz);
                float c001 = At(ix, iy, iz + 1), c101 = At(ix + 1, iy, iz + 1), c011 = At(ix, iy + 1, iz + 1), c111 = At(ix + 1, iy + 1, iz + 1);
                float c00 = c000 + (c100 - c000) * tx, c10 = c010 + (c110 - c010) * tx, c01 = c001 + (c101 - c001) * tx, c11 = c011 + (c111 - c011) * tx;
                float c0 = c00 + (c10 - c00) * ty, c1 = c01 + (c11 - c01) * ty;
                return c0 + (c1 - c0) * tz + extra;
            }

            /// <summary>The way out of the drawn surface at a point in part space (unit length; up if it cannot tell).</summary>
            public Vector3 Out(Vector3 p)
            {
                float h = Cell * 0.5f;
                var g = new Vector3(
                    Distance(p + new Vector3(h, 0f, 0f)) - Distance(p - new Vector3(h, 0f, 0f)),
                    Distance(p + new Vector3(0f, h, 0f)) - Distance(p - new Vector3(0f, h, 0f)),
                    Distance(p + new Vector3(0f, 0f, h)) - Distance(p - new Vector3(0f, 0f, h)));
                float m = g.magnitude;
                return m > 1e-6f ? g / m : Vector3.up;
            }

            /// <summary>
            /// Inside the drawn mesh, exactly: a ray along +X through the triangles, crossings counted (the meshes are
            /// closed). Not the grid: a separate test, so the cloth does not mark its own homework. False if this part has
            /// no triangles kept (the parts clothes do not cover).
            /// </summary>
            public bool InsideExactly(Vector3 p)
            {
                if (Tris == null || Tris.Length == 0 || p.x < MeshMin.x || p.y < MeshMin.y || p.z < MeshMin.z || p.x > MeshMax.x || p.y > MeshMax.y || p.z > MeshMax.z)
                {
                    return false;
                }

                float y = p.y + 1.234e-6f, z = p.z + 2.345e-6f;
                int crossings = 0;
                for (int k = 0; k < Tris.Length; k += 9)
                {
                    float ay = Tris[k + 1], az = Tris[k + 2], by = Tris[k + 4], bz = Tris[k + 5], cy = Tris[k + 7], cz = Tris[k + 8];
                    if ((ay > y && by > y && cy > y) || (ay < y && by < y && cy < y) || (az > z && bz > z && cz > z) || (az < z && bz < z && cz < z))
                    {
                        continue;
                    }

                    float d = (by - ay) * (cz - az) - (bz - az) * (cy - ay);
                    if (Mathf.Abs(d) < 1e-14f)
                    {
                        continue;
                    }

                    float l1 = (by - y) * (cz - z) - (bz - z) * (cy - y);
                    float l2 = (cy - y) * (az - z) - (cz - z) * (ay - y);
                    float l3 = (ay - y) * (bz - z) - (az - z) * (by - y);
                    if (d > 0f ? l1 < 0f || l2 < 0f || l3 < 0f : l1 > 0f || l2 > 0f || l3 > 0f)
                    {
                        continue;
                    }

                    float x = (l1 * Tris[k] + l2 * Tris[k + 3] + l3 * Tris[k + 6]) / d;
                    if (x > p.x)
                    {
                        crossings++;
                    }
                }

                return (crossings & 1) == 1;
            }
        }

        private readonly Field[] fields = new Field[PartInfo.Count];

        /// <summary>The size these fields are of ("Medium").</summary>
        public string Body { get; private set; }

        public Field Of(PartId p) => fields[(int)p];

        private static readonly Dictionary<string, BodyFields> loaded = new Dictionary<string, BodyFields>();

        /// <summary>The baked body of a size (Small, Medium, Large), read once; null if none is baked.</summary>
        public static BodyFields For(string body)
        {
            body = string.IsNullOrEmpty(body) ? "Medium" : char.ToUpperInvariant(body[0]) + body.Substring(1).ToLowerInvariant();
            if (loaded.TryGetValue(body, out var have))
            {
                return have;
            }

            BodyFields made = null;
            string text = LocoLibrary.ReadText("Locomotion/BodyFields_" + body, "_Battler/Resources/Locomotion/BodyFields_" + body + ".json");
            if (!string.IsNullOrEmpty(text))
            {
                var file = JsonUtility.FromJson<FileData>(text);
                if (file != null && file.parts != null)
                {
                    made = new BodyFields { Body = body };
                    foreach (var p in file.parts)
                    {
                        if (!PartInfo.TryParse(p.name, out var id))
                        {
                            continue;
                        }

                        var f = new Field
                        {
                            Part = id, Cell = file.cell, Nx = p.nx, Ny = p.ny, Nz = p.nz,
                            Min = new Vector3(p.min[0], p.min[1], p.min[2]),
                            MeshMin = new Vector3(p.meshMin[0], p.meshMin[1], p.meshMin[2]),
                            MeshMax = new Vector3(p.meshMax[0], p.meshMax[1], p.meshMax[2]),
                        };
                        byte[] raw = Convert.FromBase64String(p.sdf);
                        f.Grid = new short[raw.Length / 2];
                        Buffer.BlockCopy(raw, 0, f.Grid, 0, raw.Length);
                        if (!string.IsNullOrEmpty(p.tris))
                        {
                            byte[] t = Convert.FromBase64String(p.tris);
                            f.Tris = new float[t.Length / 4];
                            Buffer.BlockCopy(t, 0, f.Tris, 0, t.Length);
                        }

                        Vector3 size = new Vector3((f.Nx - 1) * f.Cell, (f.Ny - 1) * f.Cell, (f.Nz - 1) * f.Cell);
                        f.Centre = f.Min + 0.5f * size;
                        f.Bound = 0.5f * size.magnitude;
                        made.fields[(int)id] = f;
                    }
                }
            }

            loaded[body] = made;
            return made;
        }

        /// <summary>
        /// The bake against what Unity loaded: each part's drawn mesh bounds (in the part's space, through the mesh's own
        /// transform) against the bounds the bake read from the mesh file. The largest difference (metres), and which
        /// part. A large one means Unity places the meshes differently from what the bake assumed (X mirrored, nothing
        /// else).
        /// </summary>
        public string CheckAgainst(UnitRig rig, float size, PartId[] only = null)
        {
            float worst = 0f;
            string where = "none checked";
            int checkedParts = 0;
            foreach (var id in PartInfo.All)
            {
                // (Only the parts the clothes cover: a hand's first mesh may be the sword it holds, 788 mm "off" in his
                // log of 11:43.)
                var f = Of(id);
                var part = rig != null ? rig.Part(id) : null;
                if (f == null || part == null || (only != null && System.Array.IndexOf(only, id) < 0))
                {
                    continue;
                }

                foreach (var filter in part.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null || filter.GetComponentInParent<BodyPart>() != part)
                    {
                        continue;
                    }

                    var b = filter.sharedMesh.bounds;
                    var t = filter.transform;
                    // (The mesh's local bounds carried into the part's space, at scale 1.)
                    Vector3 lo = Vector3.one * float.MaxValue, hi = Vector3.one * float.MinValue;
                    for (int c = 0; c < 8; c++)
                    {
                        var corner = b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1));
                        var inPart = part.transform.InverseTransformPoint(t.TransformPoint(corner)) / Mathf.Max(0.01f, size);
                        lo = Vector3.Min(lo, inPart);
                        hi = Vector3.Max(hi, inPart);
                    }

                    float off = Mathf.Max((lo - f.MeshMin).magnitude, (hi - f.MeshMax).magnitude);
                    checkedParts++;
                    if (off > worst)
                    {
                        worst = off;
                        where = id.ToString();
                    }

                    break;
                }
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "the baked body against the meshes Unity loaded: {0} parts checked, the furthest off {1:0.0} mm ({2}){3}",
                checkedParts, 1000f * worst, where, worst > 0.004f ? " - NOT AS BAKED: the clothes' checks are not to be trusted" : " - as baked");
        }
    }
}
