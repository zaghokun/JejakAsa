"""Run through Blender MCP. Additive, seeded landscape sources for Posko and Rumah.

Existing scene content is linked for review, never rebuilt or modified. The exported
FBXs contain only dressing. Unity's existing walkable colliders remain authoritative.
"""
import bpy, math, random, json, os, shutil
from mathutils import Vector

ROOT = 'D:/project/grafkom-game'
OUT = ROOT + '/Unity/JejakAsa/Assets/_Project/Art/Environment/Landscape'
os.makedirs(OUT, exist_ok=True)
soil_path = ROOT + '/Blender/AssetLibrary_Staging/Ground_BrownMudDry/brown_mud_dry_diff_2k.jpg'
shutil.copy2(soil_path, OUT + '/brown_mud_dry_diff_2k.jpg')
image = bpy.data.images.load(soil_path, check_existing=True)
rng = random.Random(2319)

def mat(name, color, soil=False):
    name = 'Landscape_' + name
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Roughness'].default_value = .95
    if soil and not any(n.type == 'TEX_IMAGE' for n in m.node_tree.nodes):
        tex = m.node_tree.nodes.new('ShaderNodeTexImage'); tex.image = image
        mix = m.node_tree.nodes.new('ShaderNodeMixRGB'); mix.blend_type = 'MULTIPLY'
        mix.inputs[0].default_value = 1; mix.inputs[2].default_value = (*color, 1)
        m.node_tree.links.new(tex.outputs['Color'], mix.inputs[1])
        soft = m.node_tree.nodes.new('ShaderNodeMixRGB')
        soft.inputs[0].default_value = .24; soft.inputs[1].default_value = (*color, 1)
        m.node_tree.links.new(mix.outputs[0], soft.inputs[2])
        m.node_tree.links.new(soft.outputs[0], bsdf.inputs['Base Color'])
    return m

soil = mat('Packed_Earth', (.72, .61, .43), True)
garden = mat('Garden_Soil', (.42, .48, .28), True)
grass = [mat('Grass_' + str(i), c) for i,c in enumerate([
    (.34,.43,.20), (.40,.48,.24), (.46,.51,.28), (.37,.46,.22)])]
leaves = [mat('Leaf_' + str(i), c) for i,c in enumerate([
    (.16,.32,.18),(.23,.40,.21),(.34,.47,.23),(.42,.52,.27)])]
bark = mat('Bark', (.31,.21,.12)); stone = mat('Foundation', (.44,.42,.34))
rock = mat('River_Stone', (.56,.55,.43)); flower = mat('Bougainvillea', (.73,.31,.38))

def link(ob):
    for c in list(ob.users_collection): c.objects.unlink(ob)
    col.objects.link(ob)
    return ob

def box(name, loc, size, m):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o=link(bpy.context.object); o.name=name; o.dimensions=size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(m); return o

def mesh(name, vertices, faces, m):
    me=bpy.data.meshes.new(name); me.from_pydata(vertices, [], faces); me.materials.append(m)
    o=bpy.data.objects.new(name,me); col.objects.link(o)
    uv=me.uv_layers.new(name='WorldUV')
    for poly in me.polygons:
        for li in poly.loop_indices:
            v=me.vertices[me.loops[li].vertex_index].co
            uv.data[li].uv=(v.x/3.5,v.y/3.5)
    return o

def polygon(name, points, m, z=.022):
    return mesh(name, [(x,y,z) for x,y in points], [tuple(range(len(points)))],m)

def blob(name,x,y,sx,sy,m,z=.026):
    points=[]
    for i in range(13):
        a=i*math.tau/13; f=rng.uniform(.83,1.12)
        points.append((x+math.cos(a)*sx*f,y+math.sin(a)*sy*f))
    return polygon(name,points,m,z)

def ico(name,loc,scale,m,sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=loc)
    o=link(bpy.context.object);o.name=name;o.scale=scale
    o.rotation_euler.z=rng.random()*math.tau; o.data.materials.append(m);return o

def stem(a,b,r):
    a,b=Vector(a),Vector(b);d=b-a
    bpy.ops.mesh.primitive_cone_add(vertices=7,radius1=r,radius2=r*.55,depth=d.length,location=(a+b)/2)
    o=link(bpy.context.object);o.name='Garden_Branch';o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(bark)

def tree(x,y,s=1):
    stem((x,y,0),(x+.13*s,y,3.1*s),.16*s)
    for dx,dy,h,w in [(-.85,.1,2.7,1.3),(.9,-.1,2.95,1.35),(0,.45,3.8,1.45),(0,-.6,3.3,1.3)]:
        stem((x,y,1.5*s),(x+dx*s,y+dy*s,h*s),.07*s)
        ico('Garden_Broadleaf_Crown',(x+dx*s,y+dy*s,h*s),(w*s,w*.85*s,w*.7*s),rng.choice(leaves),2)
    blob('Tree_Ground_Cover',x,y,1.8*s,1.5*s,grass[0])

def shrub(x,y,s=1,bloom=False):
    for i in range(3):
        dx=rng.uniform(-.35,.35)*s;dy=rng.uniform(-.25,.25)*s
        ico('Garden_Hedge',(x+dx,y+dy,.35*s),(.65*s,.48*s,.43*s),rng.choice(leaves))
    if bloom:
        for i in range(4):
            ico('Garden_Flowers',(x+rng.uniform(-.4,.4)*s,y+rng.uniform(-.25,.25)*s,.65*s),(.13*s,.15*s,.12*s),flower)

def banana(x,y,s=1):
    stem((x,y,0),(x,y,1.8*s),.09*s)
    for i in range(7):
        a=i*math.tau/7+.2; d=Vector((math.cos(a),math.sin(a),0))
        p=Vector((x,y,1.5*s)); end=p+d*1.45*s+Vector((0,0,.05*s))
        mid=p+d*.7*s+Vector((0,0,.65*s)); side=Vector((-d.y,d.x,0))*.25*s
        mesh('Garden_Banana_Leaf',[tuple(p),tuple(mid-side),tuple(mid+Vector((0,0,.08*s))),tuple(mid+side),tuple(end)],[(0,1,2),(0,2,3),(1,4,2),(2,4,3)],leaves[i%3])

def pebble(x,y,s):
    ico('Garden_River_Stone',(x,y,.13*s),(.43*s,.33*s,.23*s),rock)

def build(kind):
    global col
    scene_name=kind+'_Landscape_Review'
    if bpy.data.scenes.get(scene_name):
        raise RuntimeError('Landscape already exists; keep it and explicitly remove/review before rebuilding.')
    scene=bpy.data.scenes.new(scene_name);bpy.context.window.scene=scene
    col=bpy.data.collections.new(kind+'_Landscape');scene.collection.children.link(col)
    # Continuous earth reaches well beyond every background house and camera boundary.
    polygon('Continuous_Ground',[(-55,-55),(55,-55),(55,55),(-55,55)],soil,z=-.025 if kind=='Posko' else -.075)
    if kind=='Posko':
        # Grass borders describe a courtyard; broad earth at the gate is kept open.
        polygon('West_Garden',[(-55,-55),(-5,-55),(-5,-18),(-14.4,-12),(-14.4,12),(-12.8,13.5),(-14,24),(-55,55)],grass[1])
        polygon('East_Garden',[(55,-55),(55,55),(14,24),(13.8,13),(14.4,11),(14.4,-12),(5,-18),(5,-55)],grass[2])
        polygon('Rear_Garden',[(-55,20),(-14,21),(-7,21),(-5,22),(5,22),(7,21),(14,21),(55,20),(55,55),(-55,55)],grass[0])
        for side in [-1,1]:
            for y in [-13,-9,-5,-1,3,8,12,19,24]:
                x=side*rng.uniform(14.9,16.5)
                blob('Border_Groundcover',x,y,1.7,2.1,rng.choice(grass))
                shrub(x,y,rng.uniform(1.2,1.7),y in [-9,3,12])
                for j in range(2):pebble(x+side*rng.uniform(.3,1),y+rng.uniform(-1,1),rng.uniform(.5,1))
            for x,y,s in [(23,-13,1.2),(23,-5,1.1),(25,5,1.3),(24,16,1.1),(20,23,1.3),(13,26,1.15),(5,26,1.2)]:tree(side*x,y,s)
            banana(side*14.7,10.8,1.2);banana(side*21,1,1.2)
            # Foreground only low shrubs, keeping the gate and player silhouette visible.
            for x in [6.5,9.5,12,16,20]:shrub(side*x,-16-rng.random()*3,1.1,x==9.5)
        for x,y,angle in [(-10,16,0),(0,17,0),(10,16,0),(-18,4,90),(18,5,90)]:
            ob=box('House_Stone_Plinth',(x,y,-.1),(6.2,4.2,.2),stone);ob.rotation_euler.z=math.radians(angle)
            for dx in [-2.7,2.7]:shrub(x+dx,y+2.7,1.1,True)
        for x in range(-12,14,3):shrub(x,23+rng.uniform(-.5,.5),1.5, x%2==0)
        # Sparse grass interrupts hard wall edges without entering the central routes.
        for side in [-1,1]:
            for y in [-9,-3,3,10]:blob('Courtyard_Edge_Patch',side*13.4,y,.65,1.8,garden,z=.018)
    else:
        box('Rumah_Continuous_Foundation',(0,0,-.32),(14.25,10.25,.5),stone)
        polygon('West_Lawn',[(-55,-55),(-4,-55),(-4,-16),(-6,-8),(-7.3,-5.1),(-7.3,5.1),(-55,55)],grass[1],z=-.06)
        polygon('East_Lawn',[(55,-55),(55,55),(7.3,5.1),(7.3,-5.1),(5,-8),(4,-16),(4,-55)],grass[2],z=-.06)
        polygon('Back_Lawn',[(-55,5.2),(55,5.2),(55,55),(-55,55)],grass[0],z=-.06)
        for side in [-1,1]:
            for y in [-5.7,-3.8,-1.8,.6,2.7,4.7,6.7]:
                shrub(side*8.25,y,.9,y in [-3.8,2.7]);pebble(side*7.5,y,.55)
            tree(side*12,5.7,1.15);tree(side*15,12,1.35);banana(side*9.2,7.8,1.1)
            for x,y in [(11,-8),(16,-6),(18,0),(18,8)]:
                blob('Garden_Lawn_Patch',side*x,y,2.7,2.4,rng.choice(grass),z=-.055)
                shrub(side*x,y,1.1)
        for x in [-8,-5,-2,1,4,7]:shrub(x,6.8,1.05,True)
        for x in [-8,-2,5,11]:tree(x,11,1.1)
        # Gentle arrival path, matching the existing exit at Unity x=2,z=-4.85.
        for i in range(10):
            y=-5.7-i*.85;x=2-math.sin(i*.3)*.7
            ob=box('Rumah_Garden_Stepping_Stone',(x,y,-.025),(.85,.58,.09),rock);ob.rotation_euler.z=rng.uniform(-.13,.13)
        for side in [-1,1]:
            for x in [6,9,12,15]:shrub(side*x,-10-rng.uniform(0,4),.65,True)
    # Consolidate by material: terrain, foliage and trunks use a few draw calls.
    meshes=list(col.objects)
    groups={}
    for ob in meshes:
        key=ob.data.materials[0].name;groups.setdefault(key,[]).append(ob)
    for key,objects in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for ob in objects:ob.select_set(True)
        bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join()
        objects[0].name=kind+'_'+key
        bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        if key.startswith('Landscape_Grass_'):objects[0].location.z+=int(key.rsplit('_',1)[1])*.018
    bpy.context.view_layer.update()
    items=[]
    for ob in col.objects:
        b=[ob.matrix_world@Vector(v) for v in ob.bound_box]
        center=[(min(v[i] for v in b)+max(v[i] for v in b))/2 for i in range(3)]
        items.append({'name':ob.name,'center':[center[0],center[2],center[1]],'material':ob.data.materials[0].name})
    mats=[]
    for m in {o.data.materials[0] for o in col.objects}:
        mats.append({'name':m.name,'color':list(m.diffuse_color),'texture':'brown_mud_dry_diff_2k.jpg' if m in (soil,garden) else ''})
    with open(OUT+'/'+kind+'Landscape.json','w') as f:json.dump({'items':items,'materials':mats},f,indent=2)
    bpy.ops.object.select_all(action='DESELECT')
    for ob in col.objects:ob.select_set(True)
    bpy.ops.export_scene.fbx(filepath=OUT+'/'+kind+'Landscape.fbx',use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='AUTO')
    # Link the pre-existing model collection for a complete, editable review source.
    source = bpy.data.collections.get('P02_Posko_Environment') if kind=='Posko' else None
    if source:scene.collection.children.link(source)
    if kind=='Posko':
        layer=scene.view_layers[0].layer_collection.children.get(source.name) if source else None
        # Old paving is only hidden in this review scene through a separate wrapper copy.
        if source:
            scene.collection.children.unlink(source)
            review=bpy.data.collections.new('Posko_Existing_Review');scene.collection.children.link(review)
            for original in source.objects:
                if original.name=='P02_Community_Paving':continue
                review.objects.link(original)
    if kind=='Rumah':
        p=ROOT+'/ArtSource/P01/P02_Rumah_Dressed.blend'
        with bpy.data.libraries.load(p,link=True) as (src,dst):dst.collections=[n for n in src.collections if not n.startswith('Source')]
        for c in dst.collections:
            if c:scene.collection.children.link(c)
    # Camera for Blender source review; Unity captures remain the visual acceptance check.
    bpy.ops.object.camera_add(location=(23,-28,25) if kind=='Posko' else (15,-18,15))
    camera=bpy.context.object;camera.name=kind+'_Landscape_Camera';camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=48 if kind=='Posko' else 29;scene.camera=camera
    scene.world=bpy.data.worlds.new(kind+'_Landscape_World');scene.world.color=(.65,.70,.62)
    scene.render.engine='BLENDER_EEVEE_NEXT'
    scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    return scene

posko=build('Posko');rumah=build('Rumah')
image.pack()
for name in ['Posko_Landscape','Rumah_Landscape']:
    c=bpy.data.collections[name];c.asset_mark();c.asset_data.author='Jejak Asa'
    c.asset_data.description='Continuous soil, foundations and garden border. Ground diffuse: Rob Tuytel / Poly Haven, CC0. Collision authored in Unity.'
for m in [soil,garden]:
    m.asset_mark();m.asset_data.author='Jejak Asa / Rob Tuytel'
    m.asset_data.description='Brown Mud Dry by Rob Tuytel, Poly Haven, CC0. Original 2K diffuse packed; subtle material styling.'
bpy.context.window.scene=posko
source=ROOT+'/ArtSource/P02/P02_Landscape.blend'
bpy.ops.wm.save_as_mainfile(filepath=source)
stage=ROOT+'/Blender/AssetLibrary_Staging/P02_Landscape';os.makedirs(stage,exist_ok=True)
shutil.copy2(source,stage+'/JejakAsa_Landscape_Library.blend')
print(json.dumps({'source':source,'scenes':[posko.name,rumah.name],'exports':OUT,'mesh_counts':[len(bpy.data.collections[k+'_Landscape'].objects) for k in ['Posko','Rumah']]}))
