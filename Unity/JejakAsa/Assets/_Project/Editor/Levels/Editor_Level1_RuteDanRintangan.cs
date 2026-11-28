#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class Editor_Level1_RuteDanRintangan
{
    const string Folder = "Assets/_Project/Art/Environment/Level1Puddles/";
    const string RootName = "Level1_Challenge_Layout";

    [MenuItem("Jejak Asa/Level 1/Apply Routes, Patrols & Puddles")]
    static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        var scene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Kampung.unity");
        var previous = SceneManager.GetActiveScene();
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Kampung.unity", OpenSceneMode.Additive);
        if (scene.isDirty) throw new InvalidOperationException("Save current Kampung edits before applying the challenge layout.");
        SceneManager.SetActiveScene(scene);
        try { ApplyToScene(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); }
        finally { if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); if(opened) EditorSceneManager.CloseScene(scene,true); }
    }

    internal static void ApplyToScene(Scene scene)
    {
        foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName).ToArray()) UnityEngine.Object.DestroyImmediate(old);
        Func<string,GameObject> find = n => scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name==n).gameObject;
        var root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root,scene);
        find("Genangan_Graybox").SetActive(false);
        find("Level1_Strategic_Hazards").SetActive(false);
        var legacyMarker = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name=="Penanda_Intimidasi_BelumAktif");
        if(legacyMarker!=null)legacyMarker.gameObject.SetActive(false);
        ConfigureTextures();
        var mud = Material("Puddle_MudBank", new Color(.42f,.34f,.23f), .30f);
        mud.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Ground025_Color.jpg"));
        mud.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Ground025_NormalGL.jpg"));
        mud.SetFloat("_BumpScale",.32f); mud.EnableKeyword("_NORMALMAP");
        mud.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Ground025_MetallicSmoothness.png"));
        mud.EnableKeyword("_METALLICSPECGLOSSMAP");
        var water = Material("Puddle_Water",new Color(.16f,.25f,.23f),.94f);
        water.SetFloat("_Metallic",.12f);
        water.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Ground025_NormalGL.jpg"));
        water.SetFloat("_BumpScale",.035f); water.EnableKeyword("_NORMALMAP");
        var ripple = Material("Puddle_Ripple",new Color(.25f,.37f,.34f),.82f);
        var timber = Material("Route_Timber",new Color(.22f,.14f,.08f),.12f);
        var panel = Material("Route_Fence",new Color(.28f,.32f,.25f),.18f);
        var guardMaterial=Material("Preman_Level1_Daylight",new Color(.85f,.82f,.75f),.16f);
        guardMaterial.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/PrologueVillains/Preman_BaseColor.jpg"));

        Fence(root.transform,"Gang_Barat_Panjang",new Vector3(-8.3f,0,7.5f),new Vector3(.28f,1.35f,15),timber,panel);
        Fence(root.transform,"Sambungan_Pagar_Barat",new Vector3(-7.35f,0,0),new Vector3(2.4f,1.25f,.30f),timber,panel);
        Fence(root.transform,"Gang_Timur_Panjang",new Vector3(7.2f,0,7.5f),new Vector3(.28f,1.35f,15),timber,panel);
        Fence(root.transform,"Gerbang_Timur_Kiri",new Vector3(8.1f,0,0),new Vector3(1.8f,1.25f,.30f),timber,panel);
        Fence(root.transform,"Gerbang_Timur_Kanan",new Vector3(16.7f,0,0),new Vector3(6.0f,1.25f,.30f),timber,panel);
        // These paths explain the choice before the player enters either guarded shortcut.
        Road(root.transform,"Jalur_Kering_Barat_1",new Vector3(-16.8f,.024f,7),new Vector3(2.3f,.012f,18),timber);
        Road(root.transform,"Jalur_Kering_Barat_2",new Vector3(-12,.024f,16.5f),new Vector3(11.8f,.012f,2.3f),timber);
        Road(root.transform,"Jalur_Kering_Barat_3",new Vector3(-5.6f,.024f,10),new Vector3(2.3f,.012f,13),timber);
        Road(root.transform,"Jalur_Kering_Timur_1",new Vector3(5.5f,.024f,10),new Vector3(2.2f,.012f,13),timber);
        Road(root.transform,"Jalur_Kering_Timur_2",new Vector3(11.3f,.024f,16.5f),new Vector3(13.8f,.012f,2.3f),timber);
        Road(root.transform,"Jalur_Kering_Timur_3",new Vector3(17.3f,.024f,10),new Vector3(2.2f,.012f,13),timber);
        Puddle(root.transform,"Genangan_Gang_Utama",new Vector3(0,0,-.1f),3.15f,7.0f,.48f,11,mud,water,ripple);
        Puddle(root.transform,"Genangan_Gang_Timur",new Vector3(11.25f,0,.5f),4.3f,7.5f,.48f,29,mud,water,ripple);
        Puddle(root.transform,"Genangan_Simpang",new Vector3(4.9f,0,-3.8f),3.6f,2.7f,.65f,47,mud,water,ripple);
        var original = find("Preman_Graybox");
        original.SetActive(false);
        original.transform.SetPositionAndRotation(new Vector3(.35f,1,-1.8f),Quaternion.Euler(0,180,0));
        ConfigureGuard(original,new[]{new Vector3(.35f,1,-2.9f),new Vector3(.35f,1,1.8f)},6.4f,7.2f);
        original.SetActive(true);
        var patrol = UnityEngine.Object.Instantiate(original,root.transform);
        patrol.name = "Preman_02_PatroliTimur";
        patrol.transform.SetPositionAndRotation(new Vector3(11.25f,1,-1.8f),Quaternion.Euler(0,180,0));
        ConfigureGuard(patrol,new[]{new Vector3(11.25f,1,-2.8f),new Vector3(11.25f,1,2.8f)},6.0f,7.0f);
        foreach(var renderer in original.GetComponentsInChildren<SkinnedMeshRenderer>(true).Concat(patrol.GetComponentsInChildren<SkinnedMeshRenderer>(true)))
            renderer.sharedMaterials=Enumerable.Repeat(guardMaterial,renderer.sharedMaterials.Length).ToArray();
        foreach(var driver in original.GetComponentsInChildren<Character_AnimasiGerak>(true).Concat(patrol.GetComponentsInChildren<Character_AnimasiGerak>(true)))
        {var so=new SerializedObject(driver);so.FindProperty("runThreshold").floatValue=3.0f;so.ApplyModifiedPropertiesWithoutUndo();}
        var mission=new SerializedObject(find("Player").GetComponent<Mission_PengantaranBantuan>());
        mission.FindProperty("initialTimeSeconds").floatValue=50;
        mission.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
    }

    internal static void RemoveFromScene(Scene scene)
    {
        foreach(var root in scene.GetRootGameObjects().Where(g=>g.name==RootName).ToArray())UnityEngine.Object.DestroyImmediate(root);
        var transforms=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        var guard=transforms.FirstOrDefault(t=>t.name=="Preman_Graybox");
        if(guard!=null)
        {
            var so=new SerializedObject(guard.GetComponent<Obstacle_Preman>());
            so.FindProperty("detectionRadius").floatValue=7;so.FindProperty("moveSpeed").floatValue=2.8f;
            so.FindProperty("patrolPoints").arraySize=0;so.FindProperty("leashDistance").floatValue=0;
            so.FindProperty("detectFromHome").boolValue=false;so.FindProperty("requireLineOfSight").boolValue=false;
            so.FindProperty("retireAfterContacts").intValue=3;so.FindProperty("retreatAfterContactSeconds").floatValue=0;
            so.FindProperty("sharedContactGraceSeconds").floatValue=0;so.ApplyModifiedPropertiesWithoutUndo();
        }
        var oldWater=transforms.FirstOrDefault(t=>t.name=="Genangan_Graybox");if(oldWater!=null)oldWater.gameObject.SetActive(true);
        var legacyMarker=transforms.FirstOrDefault(t=>t.name=="Penanda_Intimidasi_BelumAktif");if(legacyMarker!=null)legacyMarker.gameObject.SetActive(true);
        var player=transforms.First(t=>t.name=="Player").GetComponent<Mission_PengantaranBantuan>();
        var ps=new SerializedObject(player);ps.FindProperty("initialTimeSeconds").floatValue=180;ps.ApplyModifiedPropertiesWithoutUndo();
    }

    internal static void ConfigureGuard(GameObject go,Vector3[] route,float radius,float leash)
    {
        var so=new SerializedObject(go.GetComponent<Obstacle_Preman>());
        so.FindProperty("detectionRadius").floatValue=radius;
        so.FindProperty("moveSpeed").floatValue=4.1f;
        so.FindProperty("patrolSpeed").floatValue=1.65f;
        so.FindProperty("turnSpeed").floatValue=540;
        so.FindProperty("timePenalty").floatValue=10;
        so.FindProperty("contactCooldown").floatValue=2.4f;
        so.FindProperty("leashDistance").floatValue=leash;
        so.FindProperty("detectFromHome").boolValue=true;
        so.FindProperty("requireLineOfSight").boolValue=true;
        so.FindProperty("retireAfterContacts").intValue=0;
        so.FindProperty("retreatAfterContactSeconds").floatValue=1.6f;
        so.FindProperty("sharedContactGraceSeconds").floatValue=1.1f;
        var points=so.FindProperty("patrolPoints");points.arraySize=route.Length;
        for(int i=0;i<route.Length;i++)points.GetArrayElementAtIndex(i).vector3Value=route[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    internal static void ConfigureTextures()
    {
        foreach(string name in new[]{"Ground025_Color.jpg","Ground025_NormalGL.jpg","Ground025_MetallicSmoothness.png"})
        {
            var imp=AssetImporter.GetAtPath(Folder+name) as TextureImporter;
            if(imp==null)throw new InvalidOperationException("Run ArtSource/Level1/prepare_puddle_assets.py first.");
            imp.textureType=name.Contains("Normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
            imp.sRGBTexture=name.Contains("Color");imp.maxTextureSize=2048;imp.mipmapEnabled=true;imp.anisoLevel=8;
            imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();
        }
    }
    internal static Material Material(string name,Color color,float smoothness)
    {
        var path=Folder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smoothness);EditorUtility.SetDirty(m);return m;
    }
    internal static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,Material mat,bool solid=false)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;
        if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g;
    }
    internal static void Fence(Transform parent,string name,Vector3 p,Vector3 size,Material timber,Material panel)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.position=p;
        var col=g.AddComponent<BoxCollider>();col.center=Vector3.up*size.y*.5f;col.size=size;
        bool alongX=size.x>size.z;float length=alongX?size.x:size.z;
        for(float d=-length*.5f;d<=length*.5f+.01f;d+=1.5f)
            Box(g.transform,"Tiang_Pagar",new Vector3(alongX?d:0,.73f,alongX?0:d),new Vector3(.16f,1.46f,.16f),timber);
        Box(g.transform,"Papan_Pagar",Vector3.up*.73f,new Vector3(alongX?length:.18f,1.05f,alongX?.18f:length),panel);
        Box(g.transform,"Balok_Atas",Vector3.up*1.28f,new Vector3(alongX?length:.22f,.10f,alongX?.22f:length),timber);
    }
    internal static void Road(Transform parent,string name,Vector3 p,Vector3 size,Material fallback)
    {
        var paving=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Environment/PoskoP02/Materials/Paving_0.002.mat");
        Box(parent,name,p,size,paving!=null?paving:fallback);
    }
    internal static void Puddle(Transform parent,string name,Vector3 p,float width,float length,float multiplier,int seed,Material mud,Material water,Material ripple,string assetFolder=Folder,bool interactiveTrigger=true,bool addWater=true)
    {
        var g=new GameObject(name);g.SetActive(false);g.transform.SetParent(parent,false);g.transform.position=p;
        const int n=36;var points=new Vector2[n];
        for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;float r=.92f+.045f*Mathf.Sin(a*5+seed)+.025f*Mathf.Sin(a*9+seed*.3f);points[i]=new Vector2(Mathf.Cos(a)*width*.5f*r,Mathf.Sin(a)*length*.5f*r);}
        if(interactiveTrigger)
        {
            var body=g.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
            var col=g.AddComponent<BoxCollider>();col.isTrigger=true;col.center=Vector3.up*.6f;col.size=new Vector3(width,1.2f,length);
            var zone=g.AddComponent<Obstacle_Genangan>();var so=new SerializedObject(zone);so.FindProperty("speedMultiplier").floatValue=multiplier;
            var footprint=so.FindProperty("footprint");footprint.arraySize=n;for(int i=0;i<n;i++)footprint.GetArrayElementAtIndex(i).vector2Value=points[i];so.ApplyModifiedPropertiesWithoutUndo();
        }
        Surface(g.transform,name+"_Lumpur",points,1,interactiveTrigger?.045f:.043f,mud,assetFolder);
        if(addWater)
        {
            Surface(g.transform,name+"_Air",points,.77f,.058f,water,assetFolder);
            for(int r=0;r<2;r++)
            {
                var verts=new Vector3[44];var uv=new Vector2[44];var tris=new int[42*3];
                for(int i=0;i<22;i++)
                {float a=(i/21f*1.3f+.1f+r)*Mathf.PI;var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var center=new Vector3(width*(r==0?-.1f:.1f),.061f,length*(r==0?-.12f:.13f));float radius=.24f+r*.12f;verts[i*2]=center+dir*radius;verts[i*2+1]=center+dir*(radius+.009f);if(i<21){int t=i*6;tris[t]=i*2;tris[t+1]=i*2+2;tris[t+2]=i*2+1;tris[t+3]=i*2+1;tris[t+4]=i*2+2;tris[t+5]=i*2+3;}}
                MeshObject(g.transform,name+"_Riak"+r,verts,uv,tris,ripple,assetFolder);
            }
        }
        g.SetActive(true);
    }
    static void Surface(Transform parent,string name,Vector2[] outline,float scale,float y,Material mat,string assetFolder)
    {
        int n=outline.Length;var v=new Vector3[n+1];var uv=new Vector2[n+1];var tris=new int[n*3];v[0]=Vector3.up*y;
        for(int i=0;i<n;i++){var p=outline[i]*scale;v[i+1]=new Vector3(p.x,y,p.y);uv[i+1]=p*.45f;tris[i*3]=0;tris[i*3+1]=(i+1)%n+1;tris[i*3+2]=i+1;}
        MeshObject(parent,name,v,uv,tris,mat,assetFolder);
    }
    static void MeshObject(Transform parent,string name,Vector3[] vertices,Vector2[] uv,int[] triangles,Material mat,string assetFolder)
    {
        if(!AssetDatabase.IsValidFolder(assetFolder.TrimEnd('/')))
        {
            string parentDir=Path.GetDirectoryName(assetFolder.TrimEnd('/')).Replace('\\','/');
            string folderName=Path.GetFileName(assetFolder.TrimEnd('/'));
            AssetDatabase.CreateFolder(parentDir,folderName);
        }
        string path=assetFolder+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.name=name;mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}
#endif
