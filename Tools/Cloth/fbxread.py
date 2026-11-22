"""fbxread.py file: a minimal binary FBX reader (vertices, polygon indices, unit scale, the node transform), for measuring the body meshes headless."""
import struct, zlib, sys
import numpy as np

def read(path):
    b = open(path, 'rb').read()
    assert b[:20] == b'Kaydara FBX Binary  ', 'not binary FBX'
    ver = struct.unpack_from('<I', b, 23)[0]
    wide = ver >= 7500
    out = {'Vertices': [], 'PolygonVertexIndex': [], 'UnitScaleFactor': None, 'Lcl': []}
    def prop(o):
        t = chr(b[o]); o += 1
        if t in 'YCILFD':
            fmt = {'Y': '<h', 'C': '<?', 'I': '<i', 'L': '<q', 'F': '<f', 'D': '<d'}[t]
            return struct.unpack_from(fmt, b, o)[0], o + struct.calcsize(fmt)
        if t in 'fdlib':
            n, enc, clen = struct.unpack_from('<III', b, o); o += 12
            raw = b[o:o + clen]; o += clen
            if enc == 1: raw = zlib.decompress(raw)
            dt = {'f': '<f4', 'd': '<f8', 'l': '<i8', 'i': '<i4', 'b': '<u1'}[t]
            return np.frombuffer(raw, dtype=dt, count=n), o
        if t in 'SR':
            n = struct.unpack_from('<I', b, o)[0]; o += 4
            return b[o:o + n], o + n
        raise ValueError(t)
    def node(o, depth):
        if wide:
            end, nprops, plen = struct.unpack_from('<QQQ', b, o); o += 24
        else:
            end, nprops, plen = struct.unpack_from('<III', b, o); o += 12
        if end == 0: return None
        nl = b[o]; o += 1
        name = b[o:o + nl].decode('latin1'); o += nl
        props = []
        for _ in range(nprops):
            v, o = prop(o); props.append(v)
        if name in ('Vertices', 'PolygonVertexIndex'):
            out[name].append(props[0])
        if name == 'P' and props and props[0] == b'UnitScaleFactor':
            out['UnitScaleFactor'] = props[-1]
        if name == 'P' and props and props[0] in (b'Lcl Translation', b'Lcl Rotation', b'Lcl Scaling'):
            out['Lcl'].append((props[0].decode(), props[-3:]))
        while o < end - (25 if wide else 13):
            r = node(o, depth + 1)
            if r is None: break
            o = r
        return end
    o = 27
    while o < len(b) - 200:
        r = node(o, 0)
        if r is None: break
        o = r
    return out

if __name__ == '__main__':
    r = read(sys.argv[1])
    v = r['Vertices'][0].reshape(-1, 3)
    print(sys.argv[1].split('/')[-1], 'verts', len(v), 'unit', r['UnitScaleFactor'], 'min', v.min(0).round(4), 'max', v.max(0).round(4), r['Lcl'][:3])
