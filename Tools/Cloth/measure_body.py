"""measure_body.py: the Medium measured from its mesh files in the rest pose (girths, lengths), for the patterns.
Run from the repository root with python3 -I."""
import json, sys, numpy as np
sys.path.insert(0, 'Tools/Cloth')
from fbxread import read
r = json.load(open('Assets/_Battler/Data/Rigs/medium_rig.json'))
nulls = {n['name']: n for n in r['nulls']}
def world(name):
    p = np.zeros(3); n = nulls[name]
    while n:
        p += np.array(n['localPosition']); n = nulls.get(n['parent']) if n['parent'] else None
    return p
for k in ['Pelvis','Spine','Chest','Head','UpperArm_R','Forearm_R','Hand_R','Thigh_R','Shin_R','Foot_R']:
    if k in nulls: print(k, np.round(world(k),3))
parts = {}
for p in r['parts']:
    v = read(f"Assets/_Battler/Art/Bodies/Medium/{p['mesh']}.fbx")["Vertices"][0].reshape(-1,3).astype(float).copy()
    v[:,0] *= -1
    parts[p['name']] = v + world(p['name'])
allv = np.vstack(list(parts.values()))
print('height', allv[:,1].max().round(3), 'min y', allv[:,1].min().round(3))
def girth(y, names, tol=0.01):
    pts = np.vstack([parts[n][np.abs(parts[n][:,1]-y)<tol] for n in names])
    if len(pts) < 3: return None
    from scipy.spatial import ConvexHull
    h = ConvexHull(pts[:,[0,2]]); return h.area if False else h.volume and sum(np.linalg.norm(pts[h.vertices][i,[0,2]]-pts[h.vertices][i-1,[0,2]]) for i in range(len(h.vertices))), pts[:,0].min(), pts[:,0].max(), pts[:,2].min(), pts[:,2].max()
trunk = ['Pelvis','Spine','Chest']
for y in np.arange(0.85, 1.62, 0.05):
    g = girth(y, trunk)
    if g: print('trunk y %.2f girth %.3f  x %.3f..%.3f  z %.3f..%.3f' % (y, *g))
# neck: chest top + head bottom
print('chest top', parts['Chest'][:,1].max().round(3), 'head bottom', parts['Head'][:,1].min().round(3))
for n in ['UpperArm_R','Forearm_R','Hand_R','Thigh_R','Shin_R','Foot_R']:
    v = parts[n]; print(n, 'min', v.min(0).round(3), 'max', v.max(0).round(3))
from scipy.spatial import ConvexHull
def hullgirth(pts):
    h = ConvexHull(pts); v = pts[h.vertices]; return sum(np.linalg.norm(v[i]-v[i-1]) for i in range(len(v)))
def slice_parts(names, y, tol=0.008):
    return np.vstack([parts[n][np.abs(parts[n][:,1]-y)<tol] for n in names if (np.abs(parts[n][:,1]-y)<tol).sum()>0])
print('--- neck')
for y in [1.53,1.55,1.57,1.59]:
    p = slice_parts(['Chest','Head'], y); print(y, round(hullgirth(p[:,[0,2]]),3), len(p))
print('--- shoulder: chest top outline x at y')
for y in [1.48,1.50,1.52]:
    p = slice_parts(['Chest'], y); print(y, p[:,0].min().round(3), p[:,0].max().round(3))
ua = parts['UpperArm_R']; print('upperarm top', ua[ua[:,1].argmax()].round(3))
# arm girths perpendicular approx by slicing along arm axis
def limb_girth(name, a, b, ts):
    v = parts[name]; a=np.array(a); b=np.array(b); ax=(b-a)/np.linalg.norm(b-a)
    for t in ts:
        c = a + ax*t*np.linalg.norm(b-a); d = (v-c)@ax; sl = v[np.abs(d)<0.008]
        if len(sl)<5: print(name,t,'-'); continue
        u = np.cross(ax,[0,0,1]); u/=np.linalg.norm(u); w=np.cross(ax,u)
        q = np.c_[(sl-c)@u,(sl-c)@w]; print(name, t, round(hullgirth(q),3))
limb_girth('UpperArm_R', world('UpperArm_R'), world('Forearm_R'), [0.1,0.3,0.6,0.9])
limb_girth('Forearm_R', world('Forearm_R'), world('Hand_R'), [0.1,0.5,0.9])
limb_girth('Thigh_R', world('Thigh_R'), world('Shin_R'), [0.05,0.2,0.5,0.9])
limb_girth('Shin_R', world('Shin_R'), world('Foot_R'), [0.1,0.5,0.9])
print('arm length shoulder->wrist', np.linalg.norm(world('Forearm_R')-world('UpperArm_R'))+np.linalg.norm(world('Hand_R')-world('Forearm_R')))
th = parts['Thigh_R']; print('thigh inner min x at', th[th[:,0].argmin()].round(3))
pv = parts['Pelvis']; print('pelvis min y', pv[:,1].min().round(3), 'at', pv[pv[:,1].argmin()].round(3))
