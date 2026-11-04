import bpy,numpy as np,argparse,sys
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--source',required=True);p.add_argument('--output',required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);bpy.ops.wm.open_mainfile(filepath=a.source)
for o in [x for x in bpy.context.scene.objects if x.type=='MESH']:
 n=len(o.data.vertices);k=len(o.vertex_groups)
 if not n or not k:continue
 weights=np.zeros((n,k),dtype=np.float32)
 for v in o.data.vertices:
  for g in v.groups:weights[v.index,g.group]=g.weight

 from mathutils.kdtree import KDTree
 tree=KDTree(n)
 for v in o.data.vertices:tree.insert(v.co,v.index)
 tree.balance()
 mask=np.array([.70<v.co.z<1.15 and abs(v.co.x)>.14 for v in o.data.vertices])
 updated=weights.copy()
 for v in o.data.vertices:
  if not mask[v.index]:continue
  neighbors=[j for _,j,d in tree.find_n(v.co,24) if d<.018]
  if neighbors:updated[v.index]=weights[neighbors].mean(axis=0)
 weights=updated

 indices=[int(i) for i in np.flatnonzero(mask)]
 for group in o.vertex_groups:group.remove(indices)
 for v in o.data.vertices:
  if not mask[v.index]:continue
  ranked=np.argsort(weights[v.index])[-4:];total=weights[v.index,ranked].sum()
  if total<.001:continue
  for j in ranked:
   if weights[v.index,j]>.0001:o.vertex_groups[int(j)].add([v.index],float(weights[v.index,j]/total),'REPLACE')
bpy.ops.wm.save_as_mainfile(filepath=a.output)
