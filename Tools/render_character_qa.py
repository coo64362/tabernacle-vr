import bpy,math,os,json
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parents[2];ART=BASE/'output/tabernacle-vr/character-art';OUT=ART/'rigged'
kind=os.environ.get('CHARACTER_QA','FemaleChild');bpy.ops.wm.open_mainfile(filepath=str(OUT/(kind+'.blend')))
arm=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
for o in bpy.context.scene.objects:
 if o.animation_data:o.animation_data_clear()
for p in arm.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=(1,0,0,0)
world=bpy.data.worlds.new('Studio World') if not bpy.context.scene.world else bpy.context.scene.world;bpy.context.scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.14,.14,.14,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.7
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True;scene.render.resolution_x=640;scene.render.resolution_y=900;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
def light(name,pos,power,size):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,.9))-o.location).to_track_quat('-Z','Y').to_euler()
light('Key',(-2,-3,4),350,3);light('Fill',(2,-1,2),180,3);light('Rim',(1,2,3),250,2)
d=bpy.data.cameras.new('QA camera');c=bpy.data.objects.new('QA camera',d);scene.collection.objects.link(c);c.location=(.15,-4,.85);c.rotation_euler=(Vector((0,0,.85))-c.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=1.9;scene.camera=c
scene.render.filepath=str(OUT/(kind+'-blender-rest.png'));bpy.ops.render.render(write_still=True)
# Mild reachable pose checks deformation without claiming extreme-pose correctness.
if 'RightUpperArm' in arm.pose.bones:
 arm.pose.bones['RightUpperArm'].rotation_mode='XYZ';arm.pose.bones['RightUpperArm'].rotation_euler.z=.35
 arm.pose.bones['RightLowerArm'].rotation_mode='XYZ';arm.pose.bones['RightLowerArm'].rotation_euler.z=.65
scene.render.filepath=str(OUT/(kind+'-blender-pose.png'));bpy.ops.render.render(write_still=True)
print('RIG_QA_RENDER_COMPLETE',kind)
