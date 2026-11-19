#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class PoskoP02Integration
{
    const string Folder="Assets/_Project/Art/Environment/PoskoP02/";
    const string Characters="Assets/_Project/Art/Characters/PoskoP02/";
    [Serializable] class Item {public string name;public float[] center;public string[] materials;public bool logistics;public float[] size;}
    [Serializable] class Mat {public string name;public float[] color;public string texture;}
    [Serializable] class Layout {public Item[] items;public Item[] colliders;public Mat[] materials;}
    static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
    [MenuItem("Jejak Asa/Posko/Apply P02")]
    static void Apply()
    {
        string path="Assets/_Project/Scenes/Posko.unity";
        var scene=SceneManager.GetSceneByPath(path);
        if(scene.IsValid()&&scene.isDirty){Debug.LogWarning("Save Posko before applying P02.");return;}
        bool opened=!scene.IsValid()||!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        foreach(var existing in scene.GetRootGameObjects().Where(o=>o.name=="P02_Posko_Environment").ToArray())UnityEngine.Object.DestroyImmediate(existing);
        Func<string,GameObject> find=n=>scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==n)?.gameObject;
        foreach(string actorName in new[]{"Player","Jonathan_NPC","Ophelia_NPC"})
        {
            var actor=find(actorName);if(actor==null)continue;
            foreach(Transform child in actor.transform.Cast<Transform>().ToArray())if(child.name.EndsWith("_Visual"))UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        var house=SceneManager.GetSceneByPath("Assets/_Project/Scenes/Rumah.unity");
        if(house.IsValid() && house.isLoaded)
        {
            var playerInHouse=house.GetRootGameObjects().FirstOrDefault(o=>o.CompareTag("Player"));
            if(playerInHouse!=null)foreach(Transform child in playerInHouse.transform.Cast<Transform>().ToArray())
                if(child.name=="Vicky_Visual" && child.GetComponent<Animator>()==null && child.childCount>0)UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        var oldActive=SceneManager.GetActiveScene();SceneManager.SetActiveScene(scene);
        foreach(string n in new[]{"BuRina","Jonathan","Ophelia","PakArif"})
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Characters+n+".fbx");
            if(importer.animationType!=ModelImporterAnimationType.Human){importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.SaveAndReimport();}
        }
        var root=new GameObject("P02_Posko_Environment");
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PoskoP02.fbx"),scene);visual.name="Visuals";visual.transform.SetParent(root.transform,false);PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var data=JsonUtility.FromJson<Layout>(File.ReadAllText(Folder+"PoskoLayout.json"));
        var materials=new Dictionary<string,Material>();
        Directory.CreateDirectory(Folder+"Materials");AssetDatabase.Refresh();
        foreach(var m in data.materials)
        {
            string materialPath=Folder+"Materials/"+m.name.Replace('/','_')+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,materialPath);}
            mat.SetColor("_BaseColor",new Color(m.color[0],m.color[1],m.color[2],1));mat.SetFloat("_Smoothness",.1f);
            if(!string.IsNullOrEmpty(m.texture))mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Textures/"+m.texture));
            materials[m.name]=mat;
        }
        var logistics=new GameObject("P02_Logistics_Visuals");logistics.transform.SetParent(root.transform,false);
        foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>())
        {
            var item=Array.Find(data.items,o=>o.name==renderer.name);if(item==null)continue;
            renderer.transform.position+=V(item.center)-renderer.bounds.center;
            renderer.sharedMaterials=item.materials.Select(n=>materials[n]).ToArray();
            if(item.logistics)renderer.transform.SetParent(logistics.transform,true);
        }
        var solidRoot=new GameObject("Navigation_Colliders");solidRoot.transform.SetParent(root.transform,false);
        foreach(var item in data.colliders)
        {
            var go=new GameObject(item.name);go.transform.SetParent(solidRoot.transform,false);go.transform.localPosition=V(item.center);go.AddComponent<BoxCollider>().size=V(item.size);
        }
        // Retain graybox triggers, floor and perimeter collisions; replace their visual renderers only.
        var gray=find("Lapangan_Posko_Graybox");
        if(gray!=null)foreach(var renderer in gray.GetComponentsInChildren<MeshRenderer>())renderer.enabled=false;
        foreach(string n in new[]{"Lapangan_Ground","Paving_Road","Kanopi_Briefing","Posko_Logistics_Desk","Posko_Relief_Boxes","Area_Distribusi","Jalur_Utama","Palang_Gerbang"})
        {var go=find(n);if(go!=null && go.GetComponent<MeshRenderer>()!=null)go.GetComponent<MeshRenderer>().enabled=false;}
        var player=find("Player");
        AddCharacter(player,"Vicky","Assets/_Project/Art/Characters/P01/Vicky.fbx","Assets/_Project/Art/Characters/P01/Vicky_BaseColor.jpg",1.7f);
        var jonathan=find("Jonathan_NPC");AddCharacter(jonathan,"Pak Jonathan",Characters+"Jonathan.fbx",Characters+"Jonathan_BaseColor.jpg",1.75f);
        var ophelia=find("Ophelia_NPC");AddCharacter(ophelia,"Ophelia",Characters+"Ophelia.fbx",Characters+"Ophelia_BaseColor.jpg",1.65f);
        var rina=new GameObject("BuRina_NPC");rina.transform.SetParent(root.transform,false);rina.transform.position=new Vector3(-1.6f,1,-3.5f);
        var capsule=rina.AddComponent<CapsuleCollider>();capsule.radius=.32f;capsule.height=1.7f;capsule.center=new Vector3(0,-.15f,0);
        var npc=rina.AddComponent<StoryNpcController>();npc.npcType=StoryNpcType.BuRina;npc.characterName="Bu Rina";npc.playerTransform=player.transform;npc.interactionRadius=2.2f;
        AddCharacter(rina,"Bu Rina",Characters+"BuRina.fbx",Characters+"BuRina_BaseColor.jpg",1.62f);
        var clipboard=GameObject.CreatePrimitive(PrimitiveType.Cube);clipboard.name="Daftar_Kebutuhan";clipboard.transform.SetParent(rina.transform,false);clipboard.transform.localPosition=new Vector3(0,.03f,.3f);clipboard.transform.localScale=new Vector3(.26f,.34f,.025f);UnityEngine.Object.DestroyImmediate(clipboard.GetComponent<Collider>());
        clipboard.GetComponent<Renderer>().sharedMaterial=materials.Values.First();
        var arif=new GameObject("PakArif_Warga");arif.transform.SetParent(root.transform,false);arif.transform.position=new Vector3(9,1,5.4f);AddCharacter(arif,"Pak Arif",Characters+"PakArif.fbx",Characters+"PakArif_BaseColor.jpg",1.7f);
        var story=rina.AddComponent<P02Listening>();var serialized=new SerializedObject(story);
        var npcs=serialized.FindProperty("briefingNpcs");npcs.arraySize=2;npcs.GetArrayElementAtIndex(0).objectReferenceValue=jonathan.GetComponent<StoryNpcController>();npcs.GetArrayElementAtIndex(1).objectReferenceValue=ophelia.GetComponent<StoryNpcController>();
        serialized.FindProperty("logisticsVisuals").objectReferenceValue=logistics;serialized.ApplyModifiedPropertiesWithoutUndo();
        GateSafetyIntegration.ApplyToScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        SceneManager.SetActiveScene(oldActive);if(opened)EditorSceneManager.CloseScene(scene,true);
        Debug.Log("P02: Posko environment, five character visuals and Bu Rina listening sequence integrated.");
    }
    static void AddCharacter(GameObject actor,string name,string modelPath,string texturePath,float height)
    {
        if(actor==null)throw new Exception("Missing actor "+name);
        var old=actor.GetComponent<MeshRenderer>();if(old!=null)old.enabled=false;
        var wrapper=new GameObject(name.Replace(" ","")+"_Visual");wrapper.transform.SetParent(actor.transform,false);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));model.transform.SetParent(wrapper.transform,false);
        var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        wrapper.transform.localScale=Vector3.one*(height/bounds.size.y);
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        wrapper.transform.position+=new Vector3(actor.transform.position.x-bounds.center.x,-bounds.min.y,actor.transform.position.z-bounds.center.z);
        string materialPath=Characters+name.Replace(" ","")+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.SetFloat("_Smoothness",.15f);AssetDatabase.CreateAsset(material,materialPath);}
        foreach(var r in renderers)r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
        var animator=model.GetComponent<Animator>();if(animator==null)animator=model.AddComponent<Animator>();
        animator.avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault(a=>a.isHuman);
        animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/P01/P01Character.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        if(animator.avatar!=null)
        {var driver=model.AddComponent<P01CharacterAnimator>();var so=new SerializedObject(driver);so.FindProperty("characterName").stringValue=name;so.ApplyModifiedPropertiesWithoutUndo();}
        else Debug.LogWarning("P02: Avatar needs checking for "+name);
    }
}
#endif
