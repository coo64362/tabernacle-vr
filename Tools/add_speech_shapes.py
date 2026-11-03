import bpy,bmesh,math,json,argparse,sys
from mathutils import Vector
from pathlib import Path
parser=argparse.ArgumentParser(description='Add local speech shapes to the approved, rigged FemaleChild source. Run with Blender --background --python this_script -- --source SOURCE.blend --output OUTPUT.blend --fbx OUTPUT.fbx [--preview DIRECTORY].')
parser.add_argument('--source',required=True);parser.add_argument('--output',required=True);parser.add_argument('--fbx',required=True);parser.add_argument('--preview')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
Path(args.output).parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(Path(args.source).resolve()))
o=bpy.data.objects['GirlHeadDetail']
if o.data.shape_keys:raise ValueError('Use the approved rigged source before speech shapes were added.')
arm=next(x for x in bpy.context.scene.objects if x.type=='ARMATURE')
for p in arm.pose.bones:p.rotation_mode='QUATERNION';p.rotation_quaternion=(1,0,0,0)
# Split the existing lip seam, preserving all UVs and the approved outer face.
def seam(x):return 1.218+.004*(x/.036)**2
bm=bmesh.new();bm.from_mesh(o.data)
for v in bm.verts:v.co.z-=seam(v.co.x)
faces=[f for f in bm.faces if all(abs(v.co.x)<.044 and v.co.y<-.115 and abs(v.co.z)<.022 for v in f.verts)]
geom=set(faces)
for f in faces:geom.update(f.edges);geom.update(f.verts)
bmesh.ops.bisect_plane(bm,geom=list(geom),dist=.0000001,plane_co=(0,0,0),plane_no=(0,0,1),clear_inner=False,clear_outer=False)
edges=[e for e in bm.edges if all(abs(v.co.z)<.000001 and abs(v.co.x)<.035 and v.co.y<-.12 for v in e.verts)]
assert len(edges)>8, len(edges)
bmesh.ops.split_edges(bm,edges=edges)
lower=set()
for v in bm.verts:
 if abs(v.co.z)<.000001 and v.link_faces and sum(sum(w.co.z for w in f.verts)/len(f.verts) for f in v.link_faces)<0:lower.add(v)
for v in bm.verts:v.co.z+=seam(v.co.x)
bm.verts.index_update();loweridx={v.index for v in lower};bm.to_mesh(o.data);bm.free();o.data.update()
basis=o.shape_key_add(name='Basis');spec={'A':(.025,1,0),'I':(.010,1.16,0),'U':(.022,.62,.006),'E':(.013,1.13,0),'O':(.028,.70,.004),'N':(.003,1,0)}
for name,(gap,width,pucker) in spec.items():
 key=o.shape_key_add(name='Viseme_'+name)
 for i,v in enumerate(o.data.vertices):
  x,y,z=v.co;front=max(0,min(1,(-y-.09)/.04));lateral=max(0,1-(abs(x)/.035)**2);s=z-seam(x);below=s<-.000001 or i in loweridx
  # Jaw/cheek displacement tapers continuously into the unchanged face.
  fall=math.exp(-(abs(s)/(.041 if below else .015))**2);w=lateral*front*fall
  key.data[i].co.z+=(-gap*.82 if below else gap*.18)*w
  key.data[i].co.x+=x*(width-1)*front*fall*max(0,1-(abs(x)/.065)**2)
  key.data[i].co.y-=pucker*w
 key.slider_min=0;key.slider_max=1
# A small, enclosed oral cavity and upper tooth band, weighted to the existing head.
def mat(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*color,1);return m
cavity=mat('MouthInterior',(.075,.012,.014));teeth=mat('MouthTeeth',(.82,.77,.64));tongue=mat('MouthTongue',(.30,.055,.065))
def attach(obj,name,material):
 obj.name=name;obj.data.materials.clear();obj.data.materials.append(material);g=obj.vertex_groups.new(name='Head');g.add(list(range(len(obj.data.vertices))),1,'REPLACE');mod=obj.modifiers.new('Head skin','ARMATURE');mod.object=arm;obj.parent=arm
 for p in obj.data.polygons:p.use_smooth=True
bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=(0,-.130,1.210));obj=bpy.context.object;obj.scale=(.033,.017,.026);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);attach(obj,'MouthInterior',cavity)
# Curved strip sits behind the upper lip; no white line at rest.
verts=[]
for z in [1.215,1.220]:
 for i in range(17):
  x=(i/16-.5)*.051;verts.append((x,-.153+.012*(x/.026)**2,z+.003*(x/.026)**2))
mesh=bpy.data.meshes.new('Upper teeth strip');mesh.from_pydata(verts,[],[(i,i+1,i+18,i+17) for i in range(16)]);obj=bpy.data.objects.new('MouthTeeth',mesh);bpy.context.collection.objects.link(obj);attach(obj,'MouthTeeth',teeth)
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,location=(0,-.139,1.195));obj=bpy.context.object;obj.scale=(.020,.008,.005);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);attach(obj,'MouthTongue',tongue)
bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.output).resolve()))
bpy.ops.object.select_all(action='DESELECT')
for ob in bpy.context.scene.objects:
 if ob.type in {'MESH','ARMATURE','EMPTY'}:ob.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(Path(args.fbx).resolve()),use_selection=True,object_types={'MESH','ARMATURE','EMPTY'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=False,axis_forward='-Z',axis_up='Y',use_armature_deform_only=False,use_mesh_modifiers=False)
print('SPEECH_SHAPES',len(o.data.vertices),len(edges),list(spec),flush=True)
# Contact sheet renders use the same lighting/camera as the accepted face review.
if not args.preview:sys.exit(0)
Path(args.preview).mkdir(parents=True,exist_ok=True)
for ob in list(bpy.context.scene.objects):
 if ob.type in {'CAMERA','LIGHT'}:bpy.data.objects.remove(ob,do_unlink=True)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=600;scene.render.resolution_y=600;scene.render.resolution_percentage=100
world=bpy.data.worlds.new('Speech review');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.7,.72,.75,1);world.node_tree.nodes['Background'].inputs[1].default_value=.7
for name,pos,power,size in [('Key',(-1,-2,3),150,3),('Fill',(1,-1,2),75,2)]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=size;ob=bpy.data.objects.new(name,d);scene.collection.objects.link(ob);ob.location=pos;ob.rotation_euler=(Vector((0,0,1.3))-ob.location).to_track_quat('-Z','Y').to_euler()
cam=bpy.data.objects.new('Speech camera',bpy.data.cameras.new('Speech camera'));scene.collection.objects.link(cam);scene.camera=cam;cam.location=(0,-2.5,1.31);cam.rotation_euler=(Vector((0,0,1.31))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=.37
for name in ['Rest','A','I','U','O']:
 for k in o.data.shape_keys.key_blocks:k.value=1 if k.name=='Viseme_'+name else 0
 scene.render.filepath=str(Path(args.preview)/('girl-viseme-'+name+'.png'));bpy.ops.render.render(write_still=True)
