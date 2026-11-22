import bpy,math,json,os,hashlib
from mathutils import Vector,Matrix
old=bpy.data.scenes.get('P02_Posko')
if old:
 for obj in list(old.objects):bpy.data.objects.remove(obj,do_unlink=True)
 bpy.data.scenes.remove(old)
scene=bpy.data.scenes.new('P02_Posko');bpy.context.window.scene=scene
art=bpy.data.collections.new('P02_Posko_Environment');scene.collection.children.link(art)
lib='D:/project/grafkom-game/Blender/AssetLibrary_Staging/JejakAsa_Environment_Catalog.blend'
names=['house_4','SM_Table_Dinning_1_06','SM_Chair_Dining_1_01','SM_Box_Closed_1_01','SM_White_board_1_01','SM_Plant_02','PP_Tree_02']
with bpy.data.libraries.load(lib,link=False) as (src,dst):dst.objects=list(names)
sources={n:o for n,o in zip(names,dst.objects)}
blockers=[];logistics=[]
def material(name,c):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*c,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.85;return m
clay=material('Posko_Terracotta',(.55,.23,.12));cream=material('Posko_Canvas',(.83,.74,.52));green=material('Posko_DeepGreen',(.13,.31,.23));wood=material('Posko_Timber',(.36,.23,.13));stone=[material('Paving_'+str(i),c) for i,c in enumerate([(.58,.56,.48),(.64,.62,.54),(.54,.54,.47)])]
def box(n,loc,size,mat,solid=False,stage=False):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=n;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for c in list(o.users_collection):c.objects.unlink(o)
 art.objects.link(o);o.data.materials.append(mat)
 if solid:blockers.append(o)
 if stage:logistics.append(n)
 return o
# One batched mesh for stone paving. Preserve generous circulation and the original floor collision.
verts=[];faces=[];mi=[]
for x in range(-14,14):
 for y in range(-12,12):
  k=len(verts);verts.extend([(x+.025,y+.025,.015),(x+.975,y+.025,.015),(x+.975,y+.975,.015),(x+.025,y+.975,.015)]);faces.append((k,k+1,k+2,k+3));mi.append((x*3+y)%3)
mesh=bpy.data.meshes.new('Community_Paving');mesh.from_pydata(verts,[],faces);mesh.materials.clear()
for m in stone:mesh.materials.append(m)
o=bpy.data.objects.new('P02_Community_Paving',mesh);art.objects.link(o)
for p,i in zip(mesh.polygons,mi):p.material_index=i
# Reuse selected staging models, normalizing them to metre-based scene sizes.
def asset(src,n,x,y,z,width=None,height=None,angle=0,solid=False,stage=False):
 original=sources[src];mesh=original.data.copy()
 if src!='house_4':mesh.transform(Matrix.Rotation(math.pi/2,4,'X'))
 pts=[v.co for v in mesh.vertices];mn=Vector([min(v[i] for v in pts) for i in range(3)]);mx=Vector([max(v[i] for v in pts) for i in range(3)])
 f=width/(mx.x-mn.x) if width else height/(mx.z-mn.z);center=Vector(((mn.x+mx.x)/2,(mn.y+mx.y)/2,mn.z))
 for v in mesh.vertices:v.co=(v.co-center)*f
 ob=bpy.data.objects.new(n,mesh);art.objects.link(ob);ob.location=(x,y,z);ob.rotation_euler.z=math.radians(angle)
 if solid:blockers.append(ob)
 if stage:logistics.append(ob.name)
 return ob
def kampung_house(i,x,y,angle):
 col=material('Kampung_Plaster_'+str(i),[(.65,.68,.48),(.67,.45,.32),(.36,.56,.50),(.65,.60,.42),(.48,.58,.64)][i])
 built=[]
 built.append(box('P02_Neighborhood_House_'+str(i),(0,0,1.4),(6,4,2.8),col))
 # Low pitched terracotta roof with simple faceted geometry.
 roofmesh=bpy.data.meshes.new('KampungRoof');roofmesh.from_pydata([(-3.3,-2.3,2.8),(3.3,-2.3,2.8),(-3.3,0,4),(3.3,0,4),(-3.3,2.3,2.8),(3.3,2.3,2.8)],[],[(0,1,3,2),(2,3,5,4),(0,2,4),(1,5,3)]);roofmesh.materials.append(clay)
 roof=bpy.data.objects.new('P02_Tiled_Roof',roofmesh);art.objects.link(roof);built.append(roof)
 built.append(box('P02_House_Door',(-.9,-2.025,1.05),(1.05,.08,2.1),wood))
 for dx in [1.2,2.15]:
  built.append(box('P02_Window_Shutter',(dx,-2.05,1.65),(.75,.1,.95),green))
 built.append(box('P02_Front_Awning',(0,-2.7,2.5),(6.3,1.5,.12),cream))
 for dx in [-2.8,2.8]:built.append(box('P02_Awning_Post',(dx,-3.3,1.25),(.1,.1,2.5),wood))
 built.append(box('P02_Threshold',(0,-2.6,.08),(5.5,1.2,.16),stone[1]))
 rot=Matrix.Rotation(math.radians(angle),4,'Z')
 for ob in built:ob.location=rot@ob.location+Vector((x,y,0));ob.rotation_euler.z+=math.radians(angle)
 blockers.append(built[0])
for i,(x,y,a) in enumerate([(-10,16,0),(0,17,0),(10,16,0),(-18,4,-90),(18,5,90)]):kampung_house(i,x,y,a)
# Quiet community shelter: no aid stock during the P02 listening beat.
box('P02_Shelter_Roof',(-5,7,3.35),(7.4,4.2,.18),cream)
box('P02_Roof_Trim',(-5,4.94,3.25),(7.5,.14,.25),green)
for x in [-8.4,-1.6]:
 for y in [5.2,8.8]:box('P02_Shelter_Pole', (x,y,1.65),(.16,.16,3.3),wood,solid=True)
asset('SM_Table_Dinning_1_06','P02_Community_Desk',-5,6.8,0,width=2.7,solid=True)
asset('SM_Chair_Dining_1_01','P02_Desk_Chair',-5,8.2,0,width=.6,solid=True)
asset('SM_White_board_1_01','P02_Notice_Board',-10,6.7,1.1,width=2,angle=0,solid=True)
for x in [-10.7,-9.3]:box('P02_Board_Post',(x,6.7,.7),(.09,.09,1.4),wood,solid=True)
# Neighbour seating and waiting edge; never across the gate or central spine.
for x in [7.0,10.1]:
 box('P02_Community_Bench',(x,7,.45),(2.3,.62,.12),wood,solid=True)
 for dx in [-.8,.8]:box('P02_Bench_Leg',(x+dx,7,.2),(.1,.5,.4),green)
for i,(x,y) in enumerate([(-11,-7.5),(11,-7.5),(11,9.5),(-12,9.5)]):
 box('P02_Tree_Trunk_'+str(i),(x,y,1.2),(.25,.25,2.4),wood)
 for j,(dx,dy,h,scale) in enumerate([(0,0,3,1.4),(-.7,.15,2.6,1),(.65,-.1,2.8,.95)]):
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=scale,location=(x+dx,y+dy,h));ob=bpy.context.object;ob.name='P02_Broadleaf_Canopy'
  for c in list(ob.users_collection):c.objects.unlink(ob)
  art.objects.link(ob);ob.data.materials.append(green)
 box('P02_Tree_Planter_'+str(i),(x,y,.22),(1.3,1.3,.44),clay,solid=True)
for i,(x,y) in enumerate([(6.2,8.8),(-9.8,3.3),(9,-2)]):asset('SM_Plant_02','P02_Potted_Plant_'+str(i),x,y,0,height=1.1)
# Empty registration clipboard and papers are visual, never colliders.
# Bu Rina's clipboard is attached to her model during Unity integration.
# Stock visuals are held back while the first listening conversation is pending.
for i,(x,y,z) in enumerate([(-9,-.5,0),(-8.2,-.5,0),(-9,.2,0),(-9,-.5,.5)]):asset('SM_Box_Closed_1_01','P02_Relief_Crate_'+str(i),x,y,z,width=.7,stage=True)
# Gate and low perimeter remain visually grounded while the existing triggers stay authoritative.
for x in [-2.2,2.2]:box('P02_Gate_Post',(x,-11.5,1.35),(.24,.24,2.7),green,solid=True)
box('P02_Gate_Header',(0,-11.5,2.75),(4.7,.18,.34),wood)
for x in [-8.1,8.1]:box('P02_Front_Planter_Wall',(x,-11.65,.35),(11.3,.3,.7),clay)
# Text is part of the 3D environment rather than gameplay UI.
def sign(text,loc,size,mat):
 curve=bpy.data.curves.new('Sign','FONT');curve.body=text;curve.align_x='CENTER';curve.size=size;curve.extrude=.002
 ob=bpy.data.objects.new('P02_Sign_'+text,curve);art.objects.link(ob);ob.location=loc;ob.rotation_euler=(math.pi/2,0,0);ob.data.materials.append(mat)
 bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.convert(target='MESH');ob.select_set(False)
sign('RUANG WARGA',(-5,4.84,3.21),.3,cream)
sign('SELAMAT DATANG',(0,-11.62,2.66),.21,cream)
bpy.context.view_layer.update()
output='D:/project/grafkom-game/Unity/JejakAsa/Assets/_Project/Art/Environment/PoskoP02';os.makedirs(output+'/Textures',exist_ok=True)
items=[];mats={};images={}
for ob in art.objects:
 bb=[ob.matrix_world@Vector(v) for v in ob.bound_box];mn=[min(v[i] for v in bb) for i in range(3)];mx=[max(v[i] for v in bb) for i in range(3)]
 items.append({'name':ob.name,'center':[(mn[0]+mx[0])/2,(mn[2]+mx[2])/2,(mn[1]+mx[1])/2],'materials':[m.name if m else '' for m in ob.data.materials],'logistics':ob.name in logistics})
 for m in ob.data.materials:
  if not m or m.name in mats:continue
  entry={'name':m.name,'color':list(m.diffuse_color),'texture':''}
  if m.use_nodes:
   bsdf=next((n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
   if bsdf:
    entry['color']=list(bsdf.inputs['Base Color'].default_value)
    links=bsdf.inputs['Base Color'].links
    if links and links[0].from_node.type=='TEX_IMAGE':
     im=links[0].from_node.image
     if im:
      key='atlas_'+hashlib.md5(im.name.encode()).hexdigest()[:8]+'.png'
      if key not in images:
       try:im.save_render(output+'/Textures/'+key);images[key]=True
       except:pass
      if key in images:entry['texture']=key;entry['color']=[1,1,1,1]
  mats[m.name]=entry
colliders=[]
for ob in blockers:
 bb=[ob.matrix_world@Vector(v) for v in ob.bound_box];mn=[min(v[i] for v in bb) for i in range(3)];mx=[max(v[i] for v in bb) for i in range(3)]
 colliders.append({'name':ob.name+'_Collider','center':[(mn[0]+mx[0])/2,(mn[2]+mx[2])/2,(mn[1]+mx[1])/2],'size':[mx[0]-mn[0],mx[2]-mn[2],max(.12,mx[1]-mn[1])]})
json.dump({'items':items,'colliders':colliders,'materials':list(mats.values())},open(output+'/PoskoLayout.json','w'),indent=2)
bpy.ops.object.select_all(action='DESELECT')
for ob in art.objects:ob.select_set(True)
bpy.context.view_layer.objects.active=next(iter(art.objects))
bpy.ops.export_scene.fbx(filepath=output+'/PoskoP02.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,path_mode='STRIP',add_leaf_bones=False)
# Preview camera and a warm afternoon sun.
bpy.ops.object.camera_add(location=(29,-35,32));cam=bpy.context.object;cam.rotation_euler=(Vector((0,1,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=43;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(0,-2,18));bpy.context.object.data.energy=2300;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=20
scene.world=bpy.data.worlds.new('Posko_Daylight');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.7,.8,.9,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.7
scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath='D:/project/grafkom-game/Previews/P02/posko-blender.png'
bpy.ops.render.render(write_still=True)
os.makedirs('D:/project/grafkom-game/ArtSource/P02',exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath='D:/project/grafkom-game/ArtSource/P02/P02_Posko.blend')
print(json.dumps({'objects':len(items),'colliders':len(colliders),'textures':len(images)}))
