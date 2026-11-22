#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Explicit scene authoring. The interaction trigger and the physical boundary
// serve different purposes: pressing E changes scene; walking never enters void.
internal static class GateSafetyIntegration
{
    const string RootName="Map_Exit_Collision";
    [MenuItem("Jejak Asa/Environment/Repair Exit Collision And Gate")]
    static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before repairing scenes.");
        foreach(string name in new[]{"Rumah","Posko"})
        {
            var loaded=SceneManager.GetSceneByPath("Assets/_Project/Scenes/"+name+".unity");
            if(loaded.IsValid()&&loaded.isDirty)throw new InvalidOperationException("Save "+name+" edits first.");
        }
        var previous=SceneManager.GetActiveScene();
        foreach(string name in new[]{"Rumah","Posko"})
        {
            var scene=SceneManager.GetSceneByPath("Assets/_Project/Scenes/"+name+".unity");
            bool opened=!scene.IsValid()||!scene.isLoaded;
            if(opened)scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/"+name+".unity",OpenSceneMode.Additive);
            try{SceneManager.SetActiveScene(scene);ApplyToScene(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
            finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        Debug.Log("Gate repair: persistent gate, readable welcome sign, solid exit boundaries and continuous ground collision in Posko + Rumah.");
    }

    internal static void ApplyToScene(Scene scene)
    {
        if(scene.name!="Posko"&&scene.name!="Rumah")return;
        foreach(var old in scene.GetRootGameObjects().Where(o=>o.name==RootName).ToArray())UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject(RootName);SceneManager.MoveGameObjectToScene(root,scene);
        var all=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).ToArray();
        // Match the continuous visual earth beneath the entire dressed landscape.
        // Interior/relief floors stay above this foundation and retain priority.
        var ground=all.FirstOrDefault(t=>t.name==scene.name+"_Landscape_Packed_Earth")?.GetComponent<Renderer>();
        if(ground!=null)
        {
            Bounds b=ground.bounds;float top=b.min.y;
            Box(root,"Continuous_Ground_Collision",new Vector3(b.center.x,top-.2f,b.center.z),new Vector3(b.size.x,.4f,b.size.z));
            const float thickness=.5f,height=4f;
            foreach(float x in new[]{b.min.x,b.max.x})Box(root,"Landscape_Edge_X",new Vector3(x,top+height/2,b.center.z),new Vector3(thickness,height,b.size.z+thickness));
            foreach(float z in new[]{b.min.z,b.max.z})Box(root,"Landscape_Edge_Z",new Vector3(b.center.x,top+height/2,z),new Vector3(b.size.x+thickness,height,thickness));
        }
        if(scene.name=="Rumah")
        {
            // Overlap both jambs; don't leave an exact-edge seam behind the door.
            Box(root,"Home_Exit_Stopper",new Vector3(2,1.5f,-5.1f),new Vector3(2.2f,3,.5f));
            return;
        }
        Box(root,"Gate_Exit_Stopper",new Vector3(0,1.5f,-12.05f),new Vector3(4.6f,3,.5f));
        var header=all.First(t=>t.name=="P02_Gate_Header").GetComponent<MeshRenderer>();
        header.enabled=true;header.forceRenderingOff=false;header.shadowCastingMode=ShadowCastingMode.On;
        Box(root,"Gate_Header_Collider",header.bounds.center,header.bounds.size);

        // The original FBX text faces the wrong side. Preserve that source node,
        // then create a correctly oriented copy with unchanged world centre/size.
        var original=all.First(t=>t.name.StartsWith("P02_Sign_SELAMAT")).GetComponent<MeshRenderer>();
        original.enabled=true;
        var sign=UnityEngine.Object.Instantiate(original.gameObject,root.transform,true);
        sign.name="Gate_Welcome_Front";
        var renderer=sign.GetComponent<MeshRenderer>();Vector3 center=renderer.bounds.center;
        sign.transform.RotateAround(center,Vector3.up,180);
        sign.transform.position+=center-renderer.bounds.center;
        renderer.enabled=true;renderer.forceRenderingOff=false;renderer.shadowCastingMode=ShadowCastingMode.On;
        original.enabled=false;

        // Only the shelter may open a sightline. The gate and its lettering are
        // permanent landmarks, including when the player stands underneath.
        foreach(var sightline in scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<JejakAsa.CharacterSightline>(true)))
        {
            var so=new SerializedObject(sightline);var scenery=so.FindProperty("overheadScenery");var details=so.FindProperty("attachedDetails");
            for(int i=scenery.arraySize-1;i>=0;i--)
            {
                var r=scenery.GetArrayElementAtIndex(i).objectReferenceValue as Renderer;
                if(r==null||r==header)
                {
                    RemoveElement(scenery,i);
                    if(i<details.arraySize)RemoveElement(details,i);
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        P03P05_PoskoStoryIntegration.RefreshRoofTitle(scene);
    }
    static void RemoveElement(SerializedProperty array,int index)
    {
        array.GetArrayElementAtIndex(index).objectReferenceValue=null;
        array.DeleteArrayElementAtIndex(index);
    }
    static void Box(GameObject root,string name,Vector3 center,Vector3 size)
    {
        var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.position=center;
        go.AddComponent<BoxCollider>().size=size;
    }
}
#endif
