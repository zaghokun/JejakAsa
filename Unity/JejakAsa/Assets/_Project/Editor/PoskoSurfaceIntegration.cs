#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class PoskoSurfaceIntegration
{
    const string Folder="Assets/_Project/Art/Environment/PoskoSurface/";
    [Serializable] class Item {public string name;public float[] center;public string[] materials;public bool collision;}
    [Serializable] class Mat {public string name;public float[] color;public bool earth;}
    [Serializable] class Layout {public Item[] items;public Mat[] materials;}
    [MenuItem("Jejak Asa/Posko/Apply Surface Relief")]
    static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
        const string path="Assets/_Project/Scenes/Posko.unity";
        var scene=SceneManager.GetSceneByPath(path);var previous=SceneManager.GetActiveScene();
        if(scene.IsValid()&&scene.isDirty)throw new InvalidOperationException("Save Posko's unsaved edits first.");
        bool opened=!scene.IsValid()||!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var normalImporter=(TextureImporter)AssetImporter.GetAtPath(Folder+"brown_mud_dry_nor_gl_2k.jpg");
            normalImporter.textureType=TextureImporterType.NormalMap;normalImporter.maxTextureSize=2048;
            normalImporter.filterMode=FilterMode.Trilinear;normalImporter.anisoLevel=8;normalImporter.SaveAndReimport();
            var data=JsonUtility.FromJson<Layout>(File.ReadAllText(Folder+"PoskoSurface.json"));
            Directory.CreateDirectory(Folder+"Materials");AssetDatabase.Refresh();
            var materials=data.materials.ToDictionary(m=>m.name,m=>{
                string p=Folder+"Materials/"+m.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(p);
                if(material==null){material=new Material(Shader.Find(m.earth?"JejakAsa/Packed Earth":"Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,p);}
                material.SetColor("_BaseColor",new Color(m.color[0],m.color[1],m.color[2],1));material.SetFloat("_Smoothness",.05f);
                if(m.earth){material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Environment/Landscape/brown_mud_dry_diff_2k.jpg"));material.SetFloat("_DetailAlbedoMapScale",.48f);material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"brown_mud_dry_nor_gl_2k.jpg"));material.SetFloat("_BumpScale",.36f);material.EnableKeyword("_NORMALMAP");}
                EditorUtility.SetDirty(material);return material;
            });
            foreach(var old in scene.GetRootGameObjects().Where(o=>o.name=="Posko_Surface_Detail").ToArray())UnityEngine.Object.DestroyImmediate(old);
            var root=new GameObject("Posko_Surface_Detail");
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"PoskoSurface.fbx"),scene);
            instance.transform.SetParent(root.transform,false);PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            instance.transform.localRotation=Quaternion.Euler(0,180,0);
            foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())
            {
                var item=Array.Find(data.items,x=>x.name==renderer.name);
                if(item==null)throw new InvalidOperationException("Missing surface metadata: "+renderer.name);
                renderer.transform.position+=new Vector3(item.center[0],item.center[1],item.center[2])-renderer.bounds.center;
                renderer.sharedMaterials=item.materials.Select(n=>materials[n]).ToArray();
                if(item.collision){var collider=renderer.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=renderer.GetComponent<MeshFilter>().sharedMesh;}
            }
            var all=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).ToArray();
            foreach(var t in all.Where(t=>t.name=="Posko_Landscape_Garden_Soil"))t.GetComponent<Renderer>().enabled=false;
            var camera=all.Select(t=>t.GetComponent<Camera>()).First(c=>c!=null&&c.CompareTag("MainCamera"));
            var player=all.First(t=>t.CompareTag("Player"));
            camera.orthographicSize=4.35f;
            camera.transform.position=new Vector3(player.position.x,.2f,player.position.z+1.3f)-camera.transform.forward*20;
            GateSafetyIntegration.ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("Posko surface relief, one static ground collider and closer 4.35 camera applied.");
        }
        finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
}
#endif
