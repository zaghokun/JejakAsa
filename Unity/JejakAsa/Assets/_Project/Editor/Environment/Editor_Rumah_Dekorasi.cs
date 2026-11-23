#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

internal static class Editor_Rumah_Dekorasi
{
    [System.Serializable] private class Item { public string name; public float[] center; public float[] size; }
    [System.Serializable] private class Data { public Item[] items; }
    [MenuItem("Jejak Asa/House/Apply Dressing")]
    private static void Apply()
    {
        const string folder="Assets/_Project/Art/Environment/P02/";
        const string scenePath="Assets/_Project/Scenes/Rumah.unity";
        var scene=SceneManager.GetSceneByPath(scenePath);
        if (scene.IsValid() && scene.isDirty) { Debug.LogWarning("P02: Rumah has unsaved edits. Save the scene before applying dressing."); return; }
        bool opened=!scene.IsValid() || !scene.isLoaded;
        if(opened) scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);
        foreach(var go in scene.GetRootGameObjects()) if(go.name=="P02_Home_Dressing") { if(opened)EditorSceneManager.CloseScene(scene,true);return; }
        var root=new GameObject("P02_Home_Dressing"); SceneManager.MoveGameObjectToScene(root,scene);
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"P02_Home_Dressing.fbx");
        if(asset==null) throw new System.Exception("P02 furnishing FBX was not imported.");
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(asset,scene); visual.name="Visuals";visual.transform.SetParent(root.transform,false);
        var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+"HomeDressingAtlas.mat");
        if(mat==null)
        {
            mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"P02_Home_Dressing.fbm/Main_Texture.png"));
            mat.SetFloat("_Smoothness",.15f);AssetDatabase.CreateAsset(mat,folder+"HomeDressingAtlas.mat");
        }
        var placements=JsonUtility.FromJson<Data>(File.ReadAllText(folder+"HomeDressingPlacements.json"));
        foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>())
        {
            var mats=new Material[renderer.sharedMaterials.Length];for(int i=0;i<mats.Length;i++)mats[i]=mat;renderer.sharedMaterials=mats;
            foreach(var item in placements.items) if(renderer.gameObject.name==item.name)
            {
                Vector3 expected=new Vector3(item.center[0],item.center[1],item.center[2]);
                renderer.transform.position+=expected-renderer.bounds.center;
                break;
            }
        }
        var blockers=new GameObject("Navigation_Colliders");blockers.transform.SetParent(root.transform,false);
        var data=JsonUtility.FromJson<Data>(File.ReadAllText(folder+"HomeDressingColliders.json"));
        foreach(var item in data.items)
        {
            var go=new GameObject(item.name);go.transform.SetParent(blockers.transform,false);
            go.transform.localPosition=new Vector3(item.center[0],item.center[1],item.center[2]);
            var box=go.AddComponent<BoxCollider>();box.size=new Vector3(item.size[0],item.size[1],item.size[2]);
        }
        // Keep old hand-tuned perimeter, doors, triggers and player bindings.
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("P02: Added 17 furnishing props and 8 grouped navigation colliders to Rumah.");
        if(opened)EditorSceneManager.CloseScene(scene,true);
    }
}
#endif
