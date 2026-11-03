import bpy,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];ART=ROOT.parent/'output/tabernacle-vr/character-art';OUT=ROOT/'Assets/Resources/Characters'
for kind in ['FemaleChild','SandstoneGuide']:
 bpy.ops.wm.open_mainfile(filepath=str(ART/'rigged'/(kind+'.blend')))
 for o in bpy.context.scene.objects:
  if o.animation_data:o.animation_data_clear()
 for level,ratio in [(1,.45),(2,.40)]:
  for o in list(bpy.context.scene.objects):
   if o.type!='MESH' or len(o.data.polygons)<10000:continue
   bpy.context.view_layer.objects.active=o;m=o.modifiers.new('Distance LOD','DECIMATE');m.ratio=ratio;m.vertex_group=''
   while o.modifiers.find(m.name)>0:bpy.ops.object.modifier_move_up(modifier=m.name)
   bpy.ops.object.modifier_apply(modifier=m.name)
  bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=str(OUT/(kind+'_LOD'+str(level)+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE','EMPTY'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=False,axis_forward='-Z',axis_up='Y',use_armature_deform_only=False)
print('DISTANCE_LODS_EXPORTED')
