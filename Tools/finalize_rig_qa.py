import bpy,math,json,os
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[1];ART=Path(os.environ['TABERNACLE_ART']);OUT=ROOT/'Assets/Resources/Characters';REPORT={}
def load(kind):
 bpy.ops.wm.open_mainfile(filepath=str(ART/'rigged'/(kind+'.blend')))
 for o in bpy.context.scene.objects:
  if o.animation_data:o.animation_data_clear()
 arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
 for p in arm.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=(1,0,0,0)
 return arm

def repair_hands(arm):
 fingers=['Thumb','Index','Middle','Ring','Little'];chains={}
 for side in ['Left','Right']:
  chains[side]=[(b.name,arm.matrix_world@b.head_local,arm.matrix_world@b.tail_local) for b in arm.data.bones if any(b.name.startswith(side+f) for f in fingers)]
 for mesh in [o for o in bpy.context.scene.objects if o.type=='MESH']:
  for v in mesh.data.vertices:
   w=mesh.matrix_world@v.co;oldFinger=any(any(mesh.vertex_groups[g.group].name.startswith(side+f) for side in ['Left','Right'] for f in fingers) for g in v.groups)
   if not oldFinger and not (abs(w.x)>.26 and .54<w.z<.80):continue
   side='Left' if w.x>0 else 'Right';hand=arm.data.bones[side+'Hand'];wrist=arm.matrix_world@hand.head_local;elbow=arm.matrix_world@arm.data.bones[side+'LowerArm'].head_local;axis=(wrist-elbow).normalized()
   for g in list(v.groups):mesh.vertex_groups[g.group].remove([v.index])
   if (w-wrist).dot(axis)<.025:
    group=mesh.vertex_groups.get(side+'Hand') or mesh.vertex_groups.new(name=side+'Hand');group.add([v.index],1,'REPLACE');continue
   choices=[]
   for n,a,b in chains[side]:
    d=b-a;t=max(0,min(1,(w-a).dot(d)/d.length_squared));choices.append(((w-a-d*t).length,n))
   choices.sort();name=choices[0][1];g=mesh.vertex_groups.get(name) or mesh.vertex_groups.new(name=name);g.add([v.index],1,'REPLACE')

def export(kind,arm):
 report={'bones':len(arm.data.bones),'finger_bones':sum(any(f in b.name for f in ['Thumb','Index','Middle','Ring','Little']) for b in arm.data.bones),'unweighted':0,'weight_error':0,'triangles':0}
 boneNames=set(arm.data.bones.keys())
 for mesh in [o for o in bpy.context.scene.objects if o.type=='MESH']:
  if not any(m.type=='ARMATURE' for m in mesh.modifiers):continue
  report['triangles']+=sum(len(p.vertices)-2 for p in mesh.data.polygons)
  for v in mesh.data.vertices:
   groups=[g for g in v.groups if mesh.vertex_groups[g.group].name in boneNames];total=sum(g.weight for g in groups)
   if total<.0001:report['unweighted']+=1
   elif abs(total-1)>.0001:
    for g in groups:mesh.vertex_groups[g.group].add([v.index],g.weight/total,'REPLACE')
    report['weight_error']+=1
 assert report['unweighted']==0,report
 REPORT[kind]=report;bpy.ops.wm.save_as_mainfile(filepath=str(ART/'rigged'/(kind+'.blend')));bpy.ops.object.select_all(action='DESELECT')
 for o in bpy.context.scene.objects:
  if o.type in {'MESH','ARMATURE','EMPTY'}:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(kind+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE','EMPTY'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=False,axis_forward='-Z',axis_up='Y')
 bpy.ops.export_scene.gltf(filepath=str(ART/'rigged'/(kind+'-optimized.glb')),export_format='GLB',use_selection=True,export_animations=False)

def render(kind,arm):
 s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=12;s.cycles.use_denoising=True;s.render.resolution_x=640;s.render.resolution_y=900;s.render.resolution_percentage=100
 world=bpy.data.worlds.new('QA World');world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.16,.16,.16,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.7;s.world=world
 for pos,power in [((-2,-3,4),350),((2,-1,2),180),((1,2,3),250)]:
  d=bpy.data.lights.new('QA Light','AREA');d.energy=power;d.size=3;o=bpy.data.objects.new('QA Light',d);s.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,.9))-o.location).to_track_quat('-Z','Y').to_euler()
 d=bpy.data.cameras.new('QA Camera');c=bpy.data.objects.new('QA Camera',d);s.collection.objects.link(c);c.location=(.15,-4,.85);c.rotation_euler=(Vector((0,0,.85))-c.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=1.9;s.camera=c
 for pose in ['rest','pose']:
  if pose=='pose':
   for name,angle in [('RightUpperArm',.30),('RightLowerArm',.50)]:
    p=arm.pose.bones[name];world=arm.matrix_world@p.matrix;location=world.translation.copy();rotation=Matrix.Translation(location)@Matrix.Rotation(-angle,4,Vector((0,-1,0)))@Matrix.Translation(-location);p.matrix=arm.matrix_world.inverted()@rotation@world;bpy.context.view_layer.update()
   for f in ['Index','Middle','Ring','Little']:
    for j in [1,2,3]:
     p=arm.pose.bones['Right'+f+str(j)];world=arm.matrix_world@p.matrix;location=world.translation.copy();rotation=Matrix.Translation(location)@Matrix.Rotation(.2,4,Vector((0,-1,0)))@Matrix.Translation(-location);p.matrix=arm.matrix_world.inverted()@rotation@world;bpy.context.view_layer.update()
  s.render.filepath=str(ART/'rigged'/(kind+'-final-'+pose+'.png'));bpy.ops.render.render(write_still=True)
for kind in ['FemaleChild','SandstoneGuide']:
 arm=load(kind)
 if kind=='FemaleChild':repair_hands(arm)
 export(kind,arm);render(kind,arm)
(ART/'rigged/final-rig-verification.json').write_text(json.dumps(REPORT,indent=2));print('FINAL_RIG_QA_COMPLETE',REPORT)
