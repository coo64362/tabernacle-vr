"""Fit finger pivots to the actual hand plane (the source hands are edge-on)."""
import bpy,os,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];ART=Path(os.environ['TABERNACLE_ART']);bpy.ops.wm.open_mainfile(filepath=str(ART/'rigged/FemaleChild.blend'));arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
for o in bpy.context.scene.objects:
 if o.animation_data:o.animation_data_clear()
for p in arm.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=(1,0,0,0)
bpy.context.view_layer.objects.active=arm;bpy.ops.object.mode_set(mode='EDIT');inv=arm.matrix_world.inverted()
for sign,side in [(1,'Left'),(-1,'Right')]:
 hand=arm.data.edit_bones[side+'Hand'];hand.head=inv@Vector((sign*.335,-.110,.72));hand.tail=inv@Vector((sign*.354,-.110,.67));arm.data.edit_bones[side+'LowerArm'].tail=hand.head;arm.data.edit_bones[side+'LowerArm'].head.y=-.06;arm.data.edit_bones[side+'UpperArm'].tail=arm.data.edit_bones[side+'LowerArm'].head
 for finger,y,length in [('Index',-.143,.058),('Middle',-.120,.074),('Ring',-.100,.067),('Little',-.079,.052),('Thumb',-.145,.047)]:
  base=Vector((sign*(.349 if finger!='Thumb' else .323),y,.677 if finger!='Thumb' else .692));axis=Vector((sign*(.012 if finger!='Thumb' else -.018),-.002,-length));parent=side+'Hand'
  for j in range(3):
   b=arm.data.edit_bones[side+finger+str(j+1)];b.head=inv@(base+axis*(j/3));b.tail=inv@(base+axis*((j+1)/3));b.parent=arm.data.edit_bones[parent];parent=b.name
bpy.ops.object.mode_set(mode='OBJECT')
chains={side:[(b.name,arm.matrix_world@b.head_local,arm.matrix_world@b.tail_local) for b in arm.data.bones if any(b.name.startswith(side+f) for f in ['Index','Middle','Ring','Little','Thumb'])] for side in ['Left','Right']}
for mesh in [o for o in bpy.context.scene.objects if o.type=='MESH']:
 for v in mesh.data.vertices:
  w=mesh.matrix_world@v.co
  if not(abs(w.x)>.26 and .54<w.z<.80):continue
  side='Left' if w.x>0 else 'Right';wrist=arm.matrix_world@arm.data.bones[side+'Hand'].head_local
  for g in list(v.groups):mesh.vertex_groups[g.group].remove([v.index])
  if w.z>.690:
   group=mesh.vertex_groups.get(side+'Hand') or mesh.vertex_groups.new(name=side+'Hand');group.add([v.index],1,'REPLACE');continue
  scores=[]
  for name,a,b in chains[side]:
   ab=b-a;t=max(0,min(1,(w-a).dot(ab)/ab.length_squared));scores.append(((w-a-ab*t).length,name))
  scores.sort();finger=scores[0][1][:-1];same=[s for s in scores if s[1][:-1]==finger][:2];total=sum(1/max(.002,d)**3 for d,n in same)
  for distance,name in same:
   g=mesh.vertex_groups.get(name) or mesh.vertex_groups.new(name=name);g.add([v.index],(1/max(.002,distance)**3)/total,'REPLACE')
bpy.ops.wm.save_as_mainfile(filepath=str(ART/'rigged/FemaleChild.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.context.scene.objects:
 if o.type in {'MESH','ARMATURE','EMPTY'}:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Resources/Characters/FemaleChild.fbx'),use_selection=True,object_types={'MESH','ARMATURE','EMPTY'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=False,axis_forward='-Z',axis_up='Y')
# Reuse the studio pose renderer without rerunning its export loop.
helper=ROOT/'Tools/finalize_rig_qa.py';code=helper.read_text().split("for kind in ['FemaleChild','SandstoneGuide']:")[0];scope={'__file__':str(helper)};exec(compile(code,str(helper),'exec'),scope);scope['render']('FemaleChild',arm)
print('FINGER_PLANE_REPAIR_COMPLETE')
