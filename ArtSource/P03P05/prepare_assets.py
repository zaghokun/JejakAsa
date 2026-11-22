"""Prepare existing character sources and an original quiet tension bed."""
import json,math,pathlib,random,shutil,struct,wave
ROOT=pathlib.Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/JejakAsa/Assets/_Project/Art/Characters/PrologueVillains'
OUT.mkdir(parents=True,exist_ok=True)
for name in ['Mr_Deddy','Mr_Gas_Oke','Preman']:
    shutil.copy2(ROOT/f'Blender/Animation_References/Mixamo_Rerig_2026/Rigged/{name}_Walk_WithSkin.fbx',OUT/(name+'.fbx'))
    raw=(ROOT/f'ArtSource/Characters/{name}_Meshy/{name}_Original.glb').read_bytes()
    offset=12;document=None;binary=None
    while offset<len(raw):
        length,kind=struct.unpack_from('<II',raw,offset);offset+=8
        data=raw[offset:offset+length];offset+=length
        if kind==0x4e4f534a:document=json.loads(data)
        elif kind==0x004e4942:binary=data
    index=document['materials'][0]['pbrMetallicRoughness']['baseColorTexture']['index']
    image=document['images'][document['textures'][index]['source']]
    view=document['bufferViews'][image['bufferView']]
    start=view.get('byteOffset',0);extension='.jpg' if image['mimeType']=='image/jpeg' else '.png'
    (OUT/(name+'_BaseColor'+extension)).write_bytes(binary[start:start+view['byteLength']])

audio=ROOT/'Unity/JejakAsa/Assets/_Project/Audio/Prologue';audio.mkdir(parents=True,exist_ok=True)
rate=22050;seconds=24;rng=random.Random(503);low=0;air=0;samples=bytearray()
for i in range(rate*seconds):
    t=i/rate;noise=rng.uniform(-1,1);low+=(noise-low)*.012;air+=(noise-air)*.14
    swell=.65+.22*math.sin(math.tau*t/12)
    sample=swell*(.065*math.sin(math.tau*49*t+.07*math.sin(t*.8))+.043*math.sin(math.tau*53.125*t)+.024*math.sin(math.tau*98*t)+.17*low+.025*(air-low))
    for at in [3.2,11.4,19.1]:
        dt=t-at
        if 0<=dt<.35:sample+=.035*math.exp(-dt*24)*math.sin(math.tau*173*dt)
    sample*=min(1,t/.18,(seconds-t)/.18)
    samples.extend(struct.pack('<h',int(max(-.9,min(.9,sample))*32767)))
with wave.open(str(audio/'UnderWatch.wav'),'wb') as out:
    out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate);out.writeframes(samples)
print('Prepared three existing humanoids and original 24-second tension bed.')
