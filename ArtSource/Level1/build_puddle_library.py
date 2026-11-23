"""Build a separate, packed Blender asset library from the registered CC0 maps."""
from pathlib import Path
import bpy,math

root=Path(__file__).resolve().parents[2]
folder=root/'Blender/AssetLibrary_Staging/Level1_Puddles'
maps=folder/'Ground025'
bpy.ops.wm.read_factory_settings(use_empty=True)
catalog='ed5756f8-523f-4a65-bf08-033542f80bed'
(folder/'blender_assets.cats.txt').write_text('VERSION 1\n'+catalog+':JejakAsa/Level1/Puddles:Puddles\n')

mud=bpy.data.materials.new('L1_MudBank_Ground025_CC0');mud.use_nodes=True
nodes=mud.node_tree.nodes;links=mud.node_tree.links;bsdf=nodes.get('Principled BSDF')
for suffix,input_name in [('Color','Base Color'),('Roughness','Roughness')]:
    tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(maps/f'Ground025_2K-JPG_{suffix}.jpg'));tex.image.pack()
    if suffix!='Color':tex.image.colorspace_settings.name='Non-Color'
    links.new(tex.outputs['Color'],bsdf.inputs[input_name])
tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(maps/'Ground025_2K-JPG_NormalGL.jpg'));tex.image.colorspace_settings.name='Non-Color';tex.image.pack()
normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.32;links.new(tex.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],bsdf.inputs['Normal'])
water=bpy.data.materials.new('L1_Puddle_Water_Project');water.use_nodes=True
water.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.10,.19,.17,1)
water.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.06
water.node_tree.nodes['Principled BSDF'].inputs['Metallic'].default_value=.12
for material in (mud,water):material.asset_mark();material.asset_data.catalog_id=catalog

for index,(name,width,length,seed) in enumerate([('GangUtama',3.15,7,11),('GangTimur',4.3,7.5,29),('Simpang',3.6,2.7,47)]):
    collection=bpy.data.collections.new('L1_Puddle_'+name);bpy.context.scene.collection.children.link(collection)
    points=[]
    for i in range(36):
        a=i*math.tau/36;r=.92+.045*math.sin(a*5+seed)+.025*math.sin(a*9+seed*.3)
        points.append((math.cos(a)*width*.5*r,math.sin(a)*length*.5*r))
    for label,scale,height,material in [('MudBank',1,.045,mud),('Water',.77,.058,water)]:
        vertices=[(0,0,height)]+[(x*scale,y*scale,height) for x,y in points]
        mesh=bpy.data.meshes.new(name+'_'+label);mesh.from_pydata(vertices,[],[(0,i+1,(i+1)%36+1) for i in range(36)]);mesh.update()
        obj=bpy.data.objects.new(name+'_'+label,mesh);collection.objects.link(obj);obj.location.x=index*7;mesh.materials.append(material)
        uv=mesh.uv_layers.new(name='UVMap')
        for polygon in mesh.polygons:
            for loop_index in polygon.loop_indices:
                vertex=mesh.vertices[mesh.loops[loop_index].vertex_index].co;uv.data[loop_index].uv=(vertex.x*.45,vertex.y*.45)
    collection.asset_mark();collection.asset_data.catalog_id=catalog
    collection.asset_data.description='CC0 ambientCG Ground025 mud bank and original project water geometry. Gameplay footprint is configured in Unity.'
bpy.ops.wm.save_as_mainfile(filepath=str(folder/'Level1_Puddles_Library.blend'))
print('Packed standalone Level1 puddle library saved without modifying the existing environment catalog.')
