"""Copy CC0 scanned mud and pack Unity smoothness from the downloaded source."""
from pathlib import Path
from PIL import Image,ImageOps
import shutil,json,hashlib

root=Path(__file__).resolve().parents[2]
source=root/'ArtSource/Level1/External/Ground025'
staging=root/'Blender/AssetLibrary_Staging/Level1_Puddles/Ground025'
runtime=root/'Unity/JejakAsa/Assets/_Project/Art/Environment/Level1Puddles'
staging.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
if source.exists():
    for path in source.iterdir():
        if path.is_file():shutil.copy2(path,staging/path.name)
for suffix,target in [('Color','Ground025_Color.jpg'),('NormalGL','Ground025_NormalGL.jpg')]:
    shutil.copy2(staging/f'Ground025_2K-JPG_{suffix}.jpg',runtime/target)
rough=Image.open(staging/'Ground025_2K-JPG_Roughness.jpg').convert('L')
black=Image.new('L',rough.size,0)
Image.merge('RGBA',(black,black,black,ImageOps.invert(rough))).save(runtime/'Ground025_MetallicSmoothness.png')
manifest={'asset':'Ground025','author':'Lennart Demes / ambientCG','source':'https://ambientcg.com/view?id=Ground025','download':'https://ambientcg.com/get?file=Ground025_2K-JPG.zip','license':'CC0-1.0','license_url':'https://docs.ambientcg.com/license/','downloaded':'2026-11-23','technique':'surface photogrammetry','modifications':'Color and OpenGL normal copied unchanged; Unity metallic/smoothness map packs black metallic and inverted roughness in alpha. Puddle geometry and water/ripple meshes are project originals.','runtime_files':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in runtime.iterdir() if p.is_file() and p.suffix!='.meta'}}
(root/'ArtSource/Level1/puddle-source.json').write_text(json.dumps(manifest,indent=2)+'\n')
(staging.parent/'SOURCE.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('Prepared registered 2K CC0 puddle material.')
