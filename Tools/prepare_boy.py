"""Run with official Blender in background. Export skinned FBX, textures and pose QA."""
import bpy,math,json,os
from mathutils import Vector
from pathlib import Path
BASE=Path(__file__).resolve().parents[2];ART=Path(os.environ.get('TABERNACLE_ART',str(BASE/'output/tabernacle-vr/character-art')));PROJECT=Path(__file__).resolve().parents[1];OUT=PROJECT/'Assets/Resources/Characters';OUT.mkdir(parents=True,exist_ok=True)
REPORT={}
def reset():bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def bones_world(arm):return {b.name:list(arm.matrix_world@b.head_local) for b in arm.data.bones}
def save_textures(kind):
 for m in bpy.data.materials:
  if not m.use_nodes:continue
  for n in m.node_tree.nodes:
   if n.type=='TEX_IMAGE' and n.image:
    if any(link.to_socket.name=='Base Color' for link in n.outputs['Color'].links):
     im=n.image;im.filepath_raw=str(OUT/(kind+'_BaseColor.png'));im.file_format='PNG';im.save()
def export(kind,arm):
 if kind=='MaleChild':
  # Keep the original head/hair topology as a separate skinned submesh.
  import bmesh
  source=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.ops.object.select_all(action='DESELECT');source.select_set(True);bpy.context.view_layer.objects.active=source;bpy.ops.object.mode_set(mode='EDIT');bm=bmesh.from_edit_mesh(source.data)
  for f in bm.faces:f.select_set(all(v.co.z>1.035 for v in f.verts))
  bmesh.update_edit_mesh(source.data);bpy.ops.mesh.separate(type='SELECTED');bpy.ops.object.mode_set(mode='OBJECT')
  for o in bpy.context.selected_objects:
   if o!=source and o.type=='MESH':o.name='BoyHeadDetail'
 for o in list(bpy.context.scene.objects):
  if o.type=='MESH' and len(o.data.polygons)>25000 and o.name!='BoyHeadDetail':
   bpy.context.view_layer.objects.active=o;mod=o.modifiers.new('Quest mesh budget','DECIMATE');mod.ratio=min(1,42000/(sum(len(p.vertices)-2 for p in o.data.polygons)));mod.delimit=set()
   while o.modifiers.find(mod.name)>0:bpy.ops.object.modifier_move_up(modifier=mod.name)
   bpy.ops.object.modifier_apply(modifier=mod.name)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];tri=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
 for o in meshes:
  for p in o.data.polygons:p.use_smooth=True
 save_textures(kind)
 REPORT[kind]={'bones':bones_world(arm),'triangles':tri,'mesh_count':len(meshes),'finger_bones':sum(any(f in b.name for f in ['Thumb','Index','Middle','Ring','Little']) for b in arm.data.bones)}
 bpy.ops.wm.save_as_mainfile(filepath=str(ART/'rigged'/(kind+'.blend')))
 bpy.ops.object.select_all(action='DESELECT')
 for o in bpy.context.scene.objects:
  if o.type in {'MESH','ARMATURE','EMPTY'}:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(kind+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE','EMPTY'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=False,axis_forward='-Z',axis_up='Y',use_armature_deform_only=False)
 bpy.ops.export_scene.gltf(filepath=str(ART/'rigged'/(kind+'-optimized.glb')),export_format='GLB',use_selection=True,export_animations=False)
def refine_hands(arm):
 # Fit finger chains in each hand's rest plane, then assign distal hand vertices
 # to the nearest finger segment. Wrist/palm weights stay on the hand bone.
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.vertex_groups]
 for sign,side in [(1,'Left'),(-1,'Right')]:
  if side+'Hand' not in arm.data.bones:continue
  hand=arm.data.bones[side+'Hand'];forearm=arm.data.bones[side+'LowerArm'];wrist=arm.matrix_world@hand.head_local;elbow=arm.matrix_world@forearm.head_local;direction=(wrist-elbow).normalized();spread=Vector((sign,0,0));spread=(spread-direction*spread.dot(direction)).normalized()
  chains=[];bpy.context.view_layer.objects.active=arm;bpy.ops.object.mode_set(mode='EDIT');inv=arm.matrix_world.inverted()
  for finger,offset,length in [('Index',-.019,.064),('Middle',-.005,.073),('Ring',.009,.068),('Little',.021,.055),('Thumb',-.038,.05)]:
   axis=direction if finger!='Thumb' else (direction*.8-spread*.65).normalized();base=wrist+direction*(.039 if finger!='Thumb' else .018)+spread*offset;parent=side+'Hand'
   for joint in range(3):
    a=base+axis*length*(joint/3);b=base+axis*length*((joint+1)/3);name=side+finger+str(joint+1);bone=arm.data.edit_bones.new(name);bone.head=inv@a;bone.tail=inv@b;bone.parent=arm.data.edit_bones[parent];parent=name;chains.append((name,a,b,finger))
  bpy.ops.object.mode_set(mode='OBJECT')
  for mesh in meshes:
   handgroup=mesh.vertex_groups.get(side+'Hand')
   if not handgroup:continue
   groups={name:mesh.vertex_groups.get(name) or mesh.vertex_groups.new(name=name) for name,_,_,_ in chains}
   for v in mesh.data.vertices:
    strength=next((g.weight for g in v.groups if g.group==handgroup.index),0)
    if strength<.5:continue
    w=mesh.matrix_world@v.co
    if (w-wrist).dot(direction)<.025:continue
    candidates=[]
    for name,a,b,finger in chains:
     ab=b-a;t=max(0,min(1,(w-a).dot(ab)/ab.length_squared));candidates.append(((w-(a+ab*t)).length,name,finger))
    candidates.sort();nearest=candidates[0];same=[c for c in candidates if c[2]==nearest[2]][:2];total=sum(1/max(.002,c[0])**3 for c in same)
    for g in list(v.groups):mesh.vertex_groups[g.group].remove([v.index])
    for distance,name,_ in same:groups[name].add([v.index],(1/max(.002,distance)**3)/total,'REPLACE')
def rig_girl():
 reset();bpy.ops.import_scene.gltf(filepath=str(ART/'models/male-child-trellis-direct.glb'));mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 # Apply imported world transforms and normalize to 1.5 m, feet at the origin.
 bpy.context.view_layer.objects.active=mesh;mesh.select_set(True);world=[mesh.matrix_world@v.co for v in mesh.data.vertices];lo=Vector(tuple(min(v[i] for v in world) for i in range(3)));hi=Vector(tuple(max(v[i] for v in world) for i in range(3)));scale=1.5/(hi.z-lo.z)
 for v,w in zip(mesh.data.vertices,world):v.co=Vector((w.x,(w.y),w.z-lo.z))*scale
 mesh.parent=None;mesh.matrix_world.identity()

 data=bpy.data.armatures.new('PilgrimBoySkeleton');arm=bpy.data.objects.new('MaleChild',data);bpy.context.collection.objects.link(arm);bpy.context.view_layer.objects.active=arm;bpy.ops.object.mode_set(mode='EDIT');defs=[]
 def bone(name,head,tail,parent=None):
  b=data.edit_bones.new(name);b.head=head;b.tail=tail
  if parent:b.parent=data.edit_bones[parent]
  defs.append(name);return name
 bone('Root',(0,0,0),(0,0,.1));bone('Hips',(0,0,.66),(0,0,.78),'Root');bone('Spine',(0,0,.78),(0,0,.94),'Hips');bone('Chest',(0,0,.94),(0,0,1.10),'Spine');bone('Neck',(0,0,1.10),(0,0,1.19),'Chest');bone('Head',(0,0,1.19),(0,0,1.44),'Neck')
 for sign,side in [(1,'Left'),(-1,'Right')]:
  bone(side+'Shoulder',(sign*.08,0,1.08),(sign*.18,0,1.065),'Chest');bone(side+'UpperArm',(sign*.18,0,1.065),(sign*.255,-.005,.88),side+'Shoulder');bone(side+'LowerArm',(sign*.255,-.005,.88),(sign*.335,-.01,.72),side+'UpperArm');bone(side+'Hand',(sign*.335,-.01,.72),(sign*.36,-.01,.65),side+'LowerArm')
  bone(side+'UpperLeg',(sign*.085,0,.66),(sign*.085,0,.36),'Hips');bone(side+'LowerLeg',(sign*.085,0,.36),(sign*.085,0,.085),side+'UpperLeg');bone(side+'Foot',(sign*.085,0,.085),(sign*.085,-.105,.045),side+'LowerLeg')
 bpy.ops.object.mode_set(mode='OBJECT');bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);arm.select_set(True);bpy.context.view_layer.objects.active=arm
 mesh.parent=arm;mod=mesh.modifiers.new('Body rig','ARMATURE');mod.object=arm
 # The long robe is weighted as cloth, not as exposed legs. The veil/hair follows the head.
 groups={n:mesh.vertex_groups.get(n) or mesh.vertex_groups.new(name=n) for n in defs}
 if not any(m.type=='ARMATURE' for m in mesh.modifiers):mod=mesh.modifiers.new('Body rig','ARMATURE');mod.object=arm
 mesh.parent=arm
 image=None
 for mat in mesh.data.materials:
  if mat and mat.use_nodes:
   for n in mat.node_tree.nodes:
    if n.type=='TEX_IMAGE' and n.image and any(l.to_socket.name=='Base Color' for l in n.outputs['Color'].links):image=n.image
 colors={}
 if image and mesh.data.uv_layers.active:
  import numpy as np
  pix=np.array(image.pixels[:]).reshape(image.size[1],image.size[0],4)
  for poly in mesh.data.polygons:
   for li in poly.loop_indices:
    uv=mesh.data.uv_layers.active.data[li].uv;x=min(image.size[0]-1,max(0,int(uv.x*image.size[0])));y=min(image.size[1]-1,max(0,int(uv.y*image.size[1])));colors[mesh.data.loops[li].vertex_index]=pix[y,x,:3]
 for v in mesh.data.vertices:
  x,y,z=v.co;ax=abs(x);side='Left' if x>0 else 'Right'
  # Use the source color to keep the rust mantle attached to the torso.
  rgb=colors.get(v.index,(1,1,1));mantle=rgb[0]>rgb[1]*1.95 and rgb[0]>rgb[2]*2.4
  # Protect the head and hanging veil from being pulled into the shoulders.
  if z>1.18 or (z>1.02 and y>.055):weights={'Head':1}
  elif ax>.305 and .54<z<.80:weights={side+'Hand':1}
  elif (mantle or (sum(rgb)<.05 and ax<.30)) and z<1.10:weights={'Chest' if z>.88 else 'Spine' if z>.70 else 'Hips':1}
  elif z<.76 and ax<.305:weights={'Hips' if z<.70 else 'Spine':1}
  elif (ax>.155 and z>.80) or (ax>.225 and z>.56):
   if z>.94:weights={side+'UpperArm':1}
   elif z>.74:weights={side+'LowerArm':1}
   else:weights={side+'Hand':1}
   # Blend elbow and wrist bands continuously.
   if abs(z-.88)<.045:
    t=max(0,min(1,(.925-z)/.09));weights={side+'UpperArm':1-t,side+'LowerArm':t}
   if abs(z-.72)<.03:
    t=max(0,min(1,(.75-z)/.06));weights={side+'LowerArm':1-t,side+'Hand':t}
  elif z<.12:weights={side+'Foot':1}
  elif z<.68:weights={'Hips':1} # Robe hangs intact instead of splitting down its center.
  elif z<.9:weights={'Spine':1}
  elif z<1.08:weights={'Chest':1}
  else:weights={'Neck':1}
  for g in v.groups:mesh.vertex_groups[g.group].remove([v.index])
  for name,w in weights.items():
   if w>0:groups[name].add([v.index],w,'REPLACE')
 refine_hands(arm);export('MaleChild',arm)
rig_girl()
(ART/"rigged/boy-rig-report.json").write_text(json.dumps(REPORT,indent=2))
