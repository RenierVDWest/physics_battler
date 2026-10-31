import struct, zlib, sys

class Node:
    def __init__(s, name, props, children):
        s.name, s.props, s.children = name, props, children
    def find(s, name):
        return [c for c in s.children if c.name == name]
    def first(s, name):
        for c in s.children:
            if c.name == name: return c
    def __repr__(s): return f"Node({s.name}, {s.props[:3]}, {len(s.children)})"

def read(path):
    data = open(path, 'rb').read()
    version = struct.unpack_from('<I', data, 23)[0]
    big = version >= 7500
    pos = 27
    def rnode(pos):
        if big:
            end, nprops, plen = struct.unpack_from('<QQQ', data, pos); pos += 24
        else:
            end, nprops, plen = struct.unpack_from('<III', data, pos); pos += 12
        nlen = data[pos]; pos += 1
        if end == 0: return None, pos
        name = data[pos:pos+nlen].decode('latin1'); pos += nlen
        props = []
        for _ in range(nprops):
            t = chr(data[pos]); pos += 1
            if t == 'Y': v = struct.unpack_from('<h', data, pos)[0]; pos += 2
            elif t == 'C': v = data[pos] != 0; pos += 1
            elif t == 'I': v = struct.unpack_from('<i', data, pos)[0]; pos += 4
            elif t == 'F': v = struct.unpack_from('<f', data, pos)[0]; pos += 4
            elif t == 'D': v = struct.unpack_from('<d', data, pos)[0]; pos += 8
            elif t == 'L': v = struct.unpack_from('<q', data, pos)[0]; pos += 8
            elif t in 'SR':
                l = struct.unpack_from('<I', data, pos)[0]; pos += 4
                v = data[pos:pos+l]; pos += l
                if t == 'S': v = v.decode('latin1')
            elif t in 'fdlib':
                n, enc, cl = struct.unpack_from('<III', data, pos); pos += 12
                raw = data[pos:pos+cl]; pos += cl
                if enc: raw = zlib.decompress(raw)
                fmt = {'f':'f','d':'d','l':'q','i':'i','b':'b'}[t]
                v = list(struct.unpack('<%d%s' % (n, fmt), raw))
            else: raise ValueError(t)
            props.append(v)
        children = []
        while pos < end:
            c, pos = rnode(pos)
            if c is None: break
            children.append(c)
        return Node(name, props, children), end
    top = []
    while pos < len(data):
        n, pos = rnode(pos)
        if n is None: break
        top.append(n)
    return version, Node('root', [], top)
