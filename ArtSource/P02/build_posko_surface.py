"""Run in Blender MCP with P02_Landscape.blend loaded; exports additive relief.
Scene and library are saved to new files, preserving the previous landscape source.
"""
import bpy, math, random, os, json, shutil
from mathutils import Vector
ROOT='D:/project/grafkom-game'
OUT=ROOT+'/Unity/JejakAsa/Assets/_Project/Art/Environment/PoskoSurface'
os.makedirs(OUT,exist_ok=True)
if bpy.data.scenes.get('Posko_Surface_Review'):raise RuntimeError('Review the existing surface scene before rebuilding.')
scene=bpy.data.scenes.new('Posko_Surface_Review');bpy.context.window.scene=scene
col=bpy.data.collections.new('Posko_Surface_Detail');scene.collection.children.link(col)
rng=random.Random(1911)

def material(name,c,earth=False):
    m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True
    b=m.node_tree.nodes['Principled BSDF'];b.inputs['Base Color'].default_value=(*c,1);b.inputs['Roughness'].default_value=.95
    if earth:
        image=bpy.data.images.load(ROOT+'/Blender/AssetLibrary_Staging/Ground_BrownMudDry/brown_mud_dry_diff_2k.jpg',check_existing=True);image.pack()
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=image
        multiply=m.node_tree.nodes.new('ShaderNodeMixRGB');multiply.blend_type='MULTIPLY';multiply.inputs[0].default_value=1;multiply.inputs[2].default_value=(*c,1)
        m.node_tree.links.new(t.outputs[0],multiply.inputs[1])
        mix=m.node_tree.nodes.new('ShaderNodeMixRGB');mix.inputs[0].default_value=.48;mix.inputs[1].default_value=(*c,1)
        m.node_tree.links.new(multiply.outputs[0],mix.inputs[2]);m.node_tree.links.new(mix.outputs[0],b.inputs['Base Color'])
        normal_image=bpy.data.images.load(ROOT+'/Blender/AssetLibrary_Staging/Ground_BrownMudDry/brown_mud_dry_nor_gl_2k.jpg',check_existing=True)
        normal_image.colorspace_settings.name='Non-Color';normal_image.pack()
        normal_texture=m.node_tree.nodes.new('ShaderNodeTexImage');normal_texture.image=normal_image
        normal=m.node_tree.nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.36
        m.node_tree.links.new(normal_texture.outputs['Color'],normal.inputs['Color']);m.node_tree.links.new(normal.outputs['Normal'],b.inputs['Normal'])
    return m
earth=material('Posko_Earth_Dry',(.73,.62,.46),True)
path=material('Posko_Earth_Compact',(.66,.52,.34),True)
stones=[material('Posko_Fieldstone_'+str(i),c) for i,c in enumerate([(.54,.53,.43),(.64,.61,.48),(.48,.49,.42)])]
grass=[material('Posko_Berm_'+str(i),c) for i,c in enumerate([(.34,.43,.20),(.40,.48,.24),(.45,.50,.27)])]

def mesh(name,verts,faces,mats,indices=None):
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces)
    for m in mats:me.materials.append(m)
    if indices:
        for p,i in zip(me.polygons,indices):p.material_index=i
    uv=me.uv_layers.new(name='WorldUV')
    for p in me.polygons:
        for li in p.loop_indices:
            v=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=(v.x/3.5,v.y/3.5)
    ob=bpy.data.objects.new(name,me);col.objects.link(ob);return ob

def segment_distance(x,y,a,b):
    dx=b[0]-a[0];dy=b[1]-a[1]
    t=max(0,min(1,((x-a[0])*dx+(y-a[1])*dy)/(dx*dx+dy*dy)))
    return math.hypot(x-a[0]-t*dx,y-a[1]-t*dy)
routes=[((0,-12),(-1.6,-3.5)),((-1.6,-3.5),(-5,4.6)),((-1.6,-3.5),(5,4.6))]
anchors=[(0,-8,1.5),(-1.6,-3.5,1.4),(-5,4.6,1.3),(5,4.6,1.3),(9,5.4,1.3)]
# Gentle real relief with flat circulation and protected furniture feet.
def height(x,y):
    mask=1.0
    for a,b in routes:mask*=max(0,min(1,(segment_distance(x,y,a,b)-1.0)/1.1))
    for ax,ay,r in anchors:mask*=max(0,min(1,(math.hypot(x-ax,y-ay)-r)/1.0))
    for ax,ay,rx,ry in [(-5,7,4.2,3),(8.5,7,4.8,1.8),(-11,-7.5,1.5,1.5),(11,-7.5,1.5,1.5),(-12,9.5,1.5,1.5),(11,9.5,1.5,1.5),(-9,-.5,1.9,1.7)]:
        distance=max(abs(x-ax)-rx,abs(y-ay)-ry)
        mask*=max(0,min(1,distance/.8))
    wave=.5+.27*math.sin(x*.65+y*.24)+.20*math.sin(y*.92-x*.18)
    edge=min(1,max(0,(14-abs(x))/.6),max(0,(12-abs(y))/.6))
    return .012+mask*edge*(.035+.19*wave)

verts=[];faces=[]
nx,ny=57,49
for j in range(ny):
    y=-12+j*.5
    for i in range(nx):
        x=-14+i*.5;verts.append((x,y,height(x,y)))
for j in range(ny-1):
    for i in range(nx-1):
        a=j*nx+i
        if (i+j)%2:faces.extend([(a,a+1,a+nx),(a+1,a+nx+1,a+nx)])
        else:faces.extend([(a,a+1,a+nx+1),(a,a+nx+1,a+nx)])
surface=mesh('Posko_Courtyard_Relief',verts,faces,[earth])
# Shallow shoulders and a meandering worn path conform to the relief.
for index,(start,end) in enumerate(routes):
    v=[];f=[];steps=28
    delta=Vector((end[0]-start[0],end[1]-start[1]));side=Vector((-delta.y,delta.x)).normalized()
    for i in range(steps+1):
        t=i/steps;center=Vector(start).lerp(Vector(end),t)+side*(math.sin(t*math.pi*2)*.18)
        width=.82+.12*math.sin(t*7+index)
        for sign in [-1,1]:
            p=center+side*sign*width;v.append((p.x,p.y,height(p.x,p.y)+.012))
        if i:
            a=(i-1)*2;f.extend([(a,a+2,a+3),(a,a+3,a+1)])
    mesh('Posko_Worn_Walkway_'+str(index),v,f,[path])
# Uneven flagstones give the shelter an apron and the route a visible edge.
stone_objects=[]
for row in range(4):
    for i in range(9):
        x=-8.1+i*.78+rng.uniform(-.025,.025);y=4.8+row*.8
        points=[]
        for k in range(8):
            a=math.tau*k/8;r=rng.uniform(.93,1.02)
            points.append((x+math.cos(a)*.43*r,y+math.sin(a)*.43*r,.027+rng.uniform(0,.004)))
        stone_objects.append(mesh('Posko_Shelter_Flagstone',points,[tuple(range(8))],[rng.choice(stones)]))
for a,b in routes[1:]:
    d=Vector(b)-Vector(a);side=Vector((-d.y,d.x)).normalized()
    for i in range(2,12,2):
        t=i/13;p=Vector(a).lerp(Vector(b),t)+side*1.04
        points=[(p.x+math.cos(k*math.tau/7)*.19,p.y+math.sin(k*math.tau/7)*.14,height(p.x,p.y)+.023) for k in range(7)]
        stone_objects.append(mesh('Posko_Path_Edge_Stone',points,[tuple(range(7))],[rng.choice(stones)]))
# Broad planted banks outside the walkable perimeter, clear of all five houses.
for index,(x,y,rx,ry,h) in enumerate([(-24,-12,6,7,1.1),(24,-11,6,7,1.25),(-28,6,7,9,1.7),(28,8,7,9,1.55),(-20,26,9,6,1.6),(20,27,9,7,1.9),(0,29,13,6,1.4)]):
    v=[(x,y,h+.03)];f=[];rings=5;segments=24
    for ring in range(1,rings+1):
        r=ring/rings
        for k in range(segments):
            a=k*math.tau/segments;rr=r*(1+.055*math.sin(k*2.7+index))
            v.append((x+math.cos(a)*rx*rr,y+math.sin(a)*ry*rr,.03+h*(1-r*r)**2))
    for k in range(segments):f.append((0,1+k,1+(k+1)%segments))
    for ring in range(1,rings):
        a=1+(ring-1)*segments;b=1+ring*segments
        for k in range(segments):
            n=(k+1)%segments;f.extend([(a+k,b+k,b+n),(a+k,b+n,a+n)])
    mesh('Posko_Planted_Bank_'+str(index),v,f,[grass[index%3]])

# Batch only small decorative stones by material; keep the collision surface separate.
stone_groups={m:[o for o in stone_objects if o.data.materials[0]==m] for m in stones}
for m,objects in stone_groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();objects[0].name=m.name
bpy.context.view_layer.update()
items=[]
for o in col.objects:
    b=[o.matrix_world@Vector(v) for v in o.bound_box]
    c=[(min(p[i] for p in b)+max(p[i] for p in b))/2 for i in range(3)]
    items.append({'name':o.name,'center':[c[0],c[2],c[1]],'materials':[m.name for m in o.data.materials],'collision':o==surface})
materials=[{'name':m.name,'color':list(m.diffuse_color),'earth':m in [earth,path]} for m in [earth,path]+stones+grass]
with open(OUT+'/PoskoSurface.json','w') as f:json.dump({'items':items,'materials':materials},f,indent=2)
bpy.ops.object.select_all(action='DESELECT')
for o in col.objects:o.select_set(True)
bpy.context.view_layer.objects.active=surface
bpy.ops.export_scene.fbx(filepath=OUT+'/PoskoSurface.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
for n in ['Posko_Landscape','Posko_Existing_Review']:
    c=bpy.data.collections.get(n)
    if c:scene.collection.children.link(c)
col.asset_mark();col.asset_data.author='Jejak Asa';col.asset_data.description='Slightly raised courtyard, worn paths, flagstone apron and planted earth banks. Ground texture: Poly Haven / Rob Tuytel, CC0.'
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/ArtSource/P02/P02_Posko_Surface.blend')
stage=ROOT+'/Blender/AssetLibrary_Staging/P02_Landscape'
shutil.copy2(ROOT+'/ArtSource/P02/P02_Posko_Surface.blend',stage+'/JejakAsa_Posko_Surface.blend')
print(json.dumps({'objects':len(col.objects),'triangles':len(surface.data.polygons),'height_range':[min(v[2] for v in verts),max(v[2] for v in verts)]}))
