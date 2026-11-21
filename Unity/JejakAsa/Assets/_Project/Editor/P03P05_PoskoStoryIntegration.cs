#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class P03P05_PoskoStoryIntegration
{
    const string Characters="Assets/_Project/Art/Characters/PrologueVillains/";
    const string Materials="Assets/_Project/Art/Environment/Prologue/";
    [MenuItem("Jejak Asa/Story/Apply P03-P05")]
    static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
        const string path="Assets/_Project/Scenes/Posko.unity";
        var scene=SceneManager.GetSceneByPath(path);var previous=SceneManager.GetActiveScene();
        if(scene.IsValid()&&scene.isDirty)throw new InvalidOperationException("Save unsaved Posko edits first.");
        bool opened=!scene.IsValid()||!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            System.IO.Directory.CreateDirectory(Materials);AssetDatabase.Refresh();
            foreach(string name in new[]{"Mr_Deddy","Mr_Gas_Oke","Preman"})
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(Characters+name+".fbx");
                importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport();
            }
            foreach(var old in scene.GetRootGameObjects().Where(o=>o.name=="P03-P05_PrologPosko").ToArray())UnityEngine.Object.DestroyImmediate(old);
            Func<string,GameObject> find=n=>scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).First(t=>t.name==n).gameObject;
            var player=find("Player");var rina=find("BuRina_NPC").GetComponent<StoryDialog_InteraksiNPC>();
            var root=new GameObject("P03-P05_PrologPosko");var prologue=root.AddComponent<P03_SponsorDanPosko>();var threat=root.AddComponent<P05_BayanganAncaman>();
            var registration=new GameObject("P03_DaftarWarga");registration.transform.SetParent(root.transform,false);
            var paper=Mat("Register_Paper",new Color(.78f,.73f,.55f));var ink=Mat("Register_Ink",new Color(.13f,.26f,.23f));
            for(int i=0;i<3;i++)
            {
                Cube(registration.transform,"Daftar_Warga",new Vector3(-5.7f+i*.55f,1.04f,6.65f),new Vector3(.40f,.012f,.55f),paper);
                for(int line=0;line<5;line++)Cube(registration.transform,"Catatan",new Vector3(-5.7f+i*.55f,1.049f,6.47f+line*.07f),new Vector3(.26f,.003f,.009f),ink);
            }
            registration.transform.position=Vector3.up*(find("P02_Community_Desk").GetComponent<Renderer>().bounds.max.y+.01f-1.04f);
            var host=new GameObject("P05_DialogVillain");host.transform.SetParent(root.transform,false);
            var narrator=host.AddComponent<StoryDialog_InteraksiNPC>();narrator.npcType=StoryNpcType.Villain;narrator.characterName="Mr. Deddy";narrator.interactionRadius=0;narrator.playerTransform=player.transform;
            var sets=new GameObject("P05_BayanganAncaman_Set");sets.transform.SetParent(root.transform,false);
            var alley=new GameObject("P05A_GangDeddy");alley.transform.SetParent(sets.transform,false);
            var room=new GameObject("P05B_RuangGasOke");room.transform.SetParent(sets.transform,false);
            var wall=Mat("Threat_Concrete",new Color(.13f,.17f,.19f));var floor=Mat("Threat_Ground",new Color(.09f,.10f,.095f));
            var metal=Mat("Threat_Iron",new Color(.045f,.055f,.064f));var wood=Mat("Threat_Timber",new Color(.16f,.095f,.05f));
            var map=Mat("Threat_Map",new Color(.50f,.46f,.32f));var route=Mat("Threat_Routes",new Color(.42f,.055f,.035f));
            Cube(alley.transform,"Alley_Ground",new Vector3(-90,-.1f,20),new Vector3(9,.2f,18),floor);
            Cube(alley.transform,"Alley_Left",new Vector3(-93.4f,2.2f,20),new Vector3(.4f,4.4f,18),wall);
            Cube(alley.transform,"Alley_Back",new Vector3(-90,2.2f,25),new Vector3(7,4.4f,.4f),wall);
            Cube(alley.transform,"Alley_Right",new Vector3(-86.6f,2.2f,22),new Vector3(.4f,4.4f,12),wall);
            for(int i=0;i<8;i++)Cube(alley.transform,"Fence_Shadow",new Vector3(-93.1f+i*.10f,1.4f,17.4f),new Vector3(.045f,2.8f,.06f),metal);
            Cube(alley.transform,"Closed_Door",new Vector3(-88.8f,1.2f,24.77f),new Vector3(1.7f,2.4f,.08f),metal);
            Cube(alley.transform,"Crate",new Vector3(-92.5f,.45f,23.3f),new Vector3(1.1f,.9f,.85f),wood);
            Character(alley.transform,"Deddy","Mr_Deddy",new Vector3(-90,1,20),1.84f,new Color(.32f,.34f,.40f),"Mr. Deddy");
            Character(alley.transform,"Anak_Buah","Preman",new Vector3(-92,1,22),1.77f,new Color(.20f,.22f,.25f),"Anak buah");
            Spot(alley.transform,"Alley_Cold_Rim",new Vector3(-87.7f,3.9f,22),new Vector3(-90,1.4f,20),new Color(.30f,.48f,.72f),210,12,55);
            Spot(alley.transform,"Alley_Distant_Lamp",new Vector3(-91.5f,3.8f,17.5f),new Vector3(-90,1.1f,20),new Color(.93f,.57f,.26f),85,11,45);
            Point(alley.transform,"Alley_Low_Fill",new Vector3(-90,2.8f,22),new Color(.15f,.21f,.30f),55,8);
            Cube(room.transform,"Room_Floor",new Vector3(-90,-.1f,50),new Vector3(9,.2f,12),floor);
            Cube(room.transform,"Room_Back",new Vector3(-90,2.2f,54),new Vector3(9,4.4f,.4f),wall);
            Cube(room.transform,"Room_Left",new Vector3(-94.3f,2.2f,50),new Vector3(.3f,4.4f,12),wall);
            Cube(room.transform,"Room_Right",new Vector3(-85.7f,2.2f,52),new Vector3(.3f,4.4f,9),wall);
            Cube(room.transform,"Map_Desk",new Vector3(-90,.85f,50),new Vector3(3.2f,.15f,1.7f),wood);
            foreach(float x in new[]{-91.3f,-88.7f})Cube(room.transform,"Desk_Leg",new Vector3(x,.4f,50),new Vector3(.16f,.8f,1.35f),wood);
            Cube(room.transform,"Map_Of_Routes",new Vector3(-90,.932f,49.85f),new Vector3(2.55f,.012f,1.2f),map);
            for(int i=0;i<6;i++)Cube(room.transform,"Map_Block",new Vector3(-90.8f+(i%3)*.8f,.943f,49.53f+(i/3)*.55f),new Vector3(.52f,.008f,.32f),wall);
            foreach(float x in new[]{-90.4f,-89.6f})Cube(room.transform,"Route_Marker",new Vector3(x,.954f,49.85f),new Vector3(.025f,.009f,1.04f),route);
            Cube(room.transform,"Controlled_Route",new Vector3(-90,.955f,49.83f),new Vector3(2.2f,.009f,.025f),route);
            for(int i=0;i<5;i++)Cube(room.transform,"Window_Blind",new Vector3(-87.5f,2.15f+i*.18f,53.76f),new Vector3(1.5f,.075f,.1f),metal);
            Cube(room.transform,"Desk_Lamp_Base",new Vector3(-91.3f,.98f,50.2f),new Vector3(.30f,.06f,.25f),metal);
            Cube(room.transform,"Desk_Lamp_Arm",new Vector3(-91.3f,1.3f,50.2f),new Vector3(.045f,.65f,.045f),metal);
            Cube(room.transform,"Desk_Lamp_Shade",new Vector3(-91.3f,1.6f,50.2f),new Vector3(.45f,.14f,.4f),metal);
            Character(room.transform,"Gas_Oke","Mr_Gas_Oke",new Vector3(-90,1,51.3f),1.88f,new Color(.40f,.38f,.33f),"Mr. Gas Oke");
            Spot(room.transform,"Desk_Light",new Vector3(-91.25f,2.35f,49.4f),new Vector3(-90,1,50.6f),new Color(1,.66f,.31f),38,8,60);
            Spot(room.transform,"Room_Cold_Edge",new Vector3(-87.4f,3.6f,53.1f),new Vector3(-90,1.35f,51.3f),new Color(.24f,.41f,.65f),110,12,55);
            Point(room.transform,"Room_Low_Fill",new Vector3(-90,2.8f,51.5f),new Color(.15f,.21f,.30f),65,8);
            Transform dc=Anchor(sets.transform,"P05A_KameraDeddy",new Vector3(-91.25f,1.6f,15.2f));
            Transform dl=Anchor(sets.transform,"P05A_FokusDeddy",new Vector3(-90,1.25f,20));
            Transform gc=Anchor(sets.transform,"P05B_KameraGasOke",new Vector3(-87.6f,1.65f,46.6f));
            Transform gl=Anchor(sets.transform,"P05B_FokusGasOke",new Vector3(-90,1.32f,51));
            var audio=root.AddComponent<AudioSource>();audio.playOnAwake=false;audio.loop=true;audio.spatialBlend=0;audio.volume=.42f;
            audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Prologue/UnderWatch.wav");
            var ts=new SerializedObject(threat);
            Set(ts,"narrator",narrator);Set(ts,"player",player.transform);Set(ts,"cinematicSets",sets);
            Set(ts,"deddyCamera",dc);Set(ts,"deddyLookAt",dl);Set(ts,"gasCamera",gc);Set(ts,"gasLookAt",gl);Set(ts,"tensionAudio",audio);ts.ApplyModifiedPropertiesWithoutUndo();
            var ps=new SerializedObject(prologue);
            Set(ps,"jonathan",find("Jonathan_NPC").GetComponent<StoryDialog_InteraksiNPC>());Set(ps,"ophelia",find("Ophelia_NPC").GetComponent<StoryDialog_InteraksiNPC>());Set(ps,"resident",rina);
            Set(ps,"logisticsVisuals",find("P02_Logistics_Visuals"));Set(ps,"registrationDetails",registration);Set(ps,"communityHelper",find("PakArif_Warga").transform);Set(ps,"threat",threat);ps.ApplyModifiedPropertiesWithoutUndo();
            var listening=new SerializedObject(rina.GetComponent<P02_MendengarkanWarga>());Set(listening,"poskoPrologue",prologue);listening.ApplyModifiedPropertiesWithoutUndo();
            sets.SetActive(false);
            RefreshRoofTitle(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("P03-P05: sponsor/setup, Ophelia reunion and two villain reveal sets integrated.");
        }
        finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
    internal static void RefreshRoofTitle(Scene scene)
    {
        var root=scene.GetRootGameObjects().FirstOrDefault(o=>o.name=="P03-P05_PrologPosko");if(root==null)return;
        var original=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshRenderer>(true)).FirstOrDefault(r=>r.name.StartsWith("P02_Sign_RUANG"));if(original==null)return;
        var old=root.transform.Find("Posko_Warga_Title");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        original.enabled=true;
        var sign=UnityEngine.Object.Instantiate(original.gameObject,root.transform,true);sign.name="Posko_Warga_Title";
        var renderer=sign.GetComponent<MeshRenderer>();Vector3 center=renderer.bounds.center;sign.transform.RotateAround(center,Vector3.up,180);sign.transform.position+=center-renderer.bounds.center;renderer.enabled=true;
        original.enabled=false;
        foreach(var sightline in scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<CharacterSightline>(true)))
        {
            var so=new SerializedObject(sightline);var scenery=so.FindProperty("overheadScenery");var details=so.FindProperty("attachedDetails");
            for(int i=0;i<scenery.arraySize;i++)
            {var r=scenery.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;if(r!=null&&r.name=="P02_Shelter_Roof"&&i<details.arraySize)details.GetArrayElementAtIndex(i).objectReferenceValue=renderer;}
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    static void Set(SerializedObject so,string name,UnityEngine.Object value)=>so.FindProperty(name).objectReferenceValue=value;
    static Material Mat(string name,Color color)
    {
        string path=Materials+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(m);return m;
    }
    static GameObject Cube(Transform root,string name,Vector3 position,Vector3 size,Material material)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.position=position;go.transform.localScale=size;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }
    static Transform Anchor(Transform root,string name,Vector3 position)
    {var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.position=position;return go.transform;}
    static void Spot(Transform root,string name,Vector3 position,Vector3 target,Color color,float intensity,float range,float angle)
    {
        var t=Anchor(root,name,position);t.LookAt(target);var light=t.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.color=color;light.intensity=intensity;light.range=range;light.spotAngle=angle;light.shadows=LightShadows.Soft;light.shadowBias=.04f;
    }
    static void Point(Transform root,string name,Vector3 position,Color color,float intensity,float range)
    {var t=Anchor(root,name,position);var light=t.gameObject.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;}
    static void Character(Transform root,string name,string modelName,Vector3 position,float height,Color tint,string speaker)
    {
        var actor=Anchor(root,name,position);actor.rotation=Quaternion.Euler(0,180,0);
        var wrapper=new GameObject("Visual").transform;wrapper.SetParent(actor,false);
        string path=Characters+modelName+".fbx";
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.transform.SetParent(wrapper,false);
        var renderers=model.GetComponentsInChildren<Renderer>();Bounds b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
        wrapper.localScale=Vector3.one*(height/b.size.y);
        b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
        wrapper.position+=new Vector3(position.x-b.center.x,-b.min.y,position.z-b.center.z);
        var material=Mat(modelName+"_Threat",tint);
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Characters+modelName+"_BaseColor.jpg")??AssetDatabase.LoadAssetAtPath<Texture2D>(Characters+modelName+"_BaseColor.png");
        material.SetTexture("_BaseMap",texture);
        foreach(var r in renderers)r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
        var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();
        animator.avatar=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().First(a=>a.isHuman);
        animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/P01/P01Character.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var driver=model.GetComponent<P01CharacterAnimator>()??model.AddComponent<P01CharacterAnimator>();var so=new SerializedObject(driver);so.FindProperty("characterName").stringValue=speaker;so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
