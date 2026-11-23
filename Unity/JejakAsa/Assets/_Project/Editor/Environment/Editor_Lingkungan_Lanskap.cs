#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Explicit menu action: importing this file never edits an open scene.
internal static class Editor_Lingkungan_Lanskap
{
    const string Folder = "Assets/_Project/Art/Environment/Landscape/";
    const string RootName = "Landscape_Edge_Dressing";
    [Serializable] class Item { public string name; public float[] center; public string material; }
    [Serializable] class Mat { public string name; public float[] color; public string texture; }
    [Serializable] class Layout { public Item[] items; public Mat[] materials; }

    [MenuItem("Jejak Asa/Environment/Apply Landscape Polish")]
    static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before applying landscape.");
        foreach (string name in new[] { "Posko", "Rumah" })
        {
            var scene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/" + name + ".unity");
            if (scene.IsValid() && scene.isDirty) throw new InvalidOperationException("Save unsaved " + name + " edits before applying landscape.");
        }
        var soil = (TextureImporter)AssetImporter.GetAtPath(Folder + "brown_mud_dry_diff_2k.jpg");
        soil.maxTextureSize = 2048; soil.filterMode = FilterMode.Trilinear; soil.anisoLevel = 8;
        soil.wrapMode = TextureWrapMode.Repeat; soil.mipmapEnabled = true;
        soil.textureCompression = TextureImporterCompression.CompressedHQ; soil.SaveAndReimport();
        // Preserve genuine source resolution; no upscaling of the original 2K character art.
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art/Characters" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!Path.GetFileNameWithoutExtension(path).EndsWith("BaseColor")) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.maxTextureSize = 2048; importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4; importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        foreach (string name in new[] { "Posko", "Rumah" }) ApplyScene(name);
        AssetDatabase.SaveAssets();
        Debug.Log("Landscape: continuous soil, garden borders and house foundations applied to Posko + Rumah; closer cameras with high SMAA.");
    }

    static void ApplyScene(string name)
    {
        string path = "Assets/_Project/Scenes/" + name + ".unity";
        var scene = SceneManager.GetSceneByPath(path);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        var previous = SceneManager.GetActiveScene();
        if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            foreach (var old in scene.GetRootGameObjects().Where(o => o.name == RootName).ToArray()) UnityEngine.Object.DestroyImmediate(old);
            var root = new GameObject(RootName);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + "Landscape.fbx");
            if (prefab == null) throw new InvalidOperationException("Missing landscape FBX: " + name);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            visual.name = "Ground_And_Garden"; visual.transform.SetParent(root.transform, false);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            // Unity's FBX conversion reverses Blender's planar X/Y here. Correct
            // the whole batched mesh before matching the exported world centres.
            visual.transform.localRotation = Quaternion.Euler(0, 180, 0);
            var data = JsonUtility.FromJson<Layout>(File.ReadAllText(Folder + name + "Landscape.json"));
            Directory.CreateDirectory(Folder + "Materials"); AssetDatabase.Refresh();
            var materials = data.materials.ToDictionary(m => m.name, m => CreateMaterial(m));
            foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                var item = Array.Find(data.items, o => o.name == renderer.name);
                if (item == null) throw new InvalidOperationException("Unregistered landscape mesh: " + renderer.name);
                renderer.transform.position += new Vector3(item.center[0], item.center[1], item.center[2]) - renderer.bounds.center;
                renderer.sharedMaterial = materials[item.material];
            }
            // The new dressing supplies no colliders: retain the hand-tested floor,
            // house boundaries, furniture and trigger colliders exactly as authored.
            if (name == "Posko")
                foreach (var r in scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>(true)))
                    if (r.name == "P02_Community_Paving") r.enabled = false;
            var camera = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Camera>(true)).First(c => c.CompareTag("MainCamera"));
            var player = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Transform>(true)).First(t => t.CompareTag("Player"));
            camera.orthographicSize = name == "Posko" ? 4.35f : 5.6f;
            var focus = new Vector3(player.position.x, .2f, player.position.z + (name == "Posko" ? 2.2f : 1.4f));
            camera.transform.position = focus - camera.transform.forward * 20f;
            var additional = camera.GetUniversalAdditionalCameraData();
            additional.renderPostProcessing = true;
            // This project uses Deferred+: SMAA is supported here, unlike MSAA.
            additional.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            additional.antialiasingQuality = AntialiasingQuality.High;
            if (name == "Posko")
            {
                var visibility = camera.GetComponent<JejakAsa.Camera_VisibilitasKarakter>() ?? camera.gameObject.AddComponent<JejakAsa.Camera_VisibilitasKarakter>();
                var so = new SerializedObject(visibility);
                so.FindProperty("subject").objectReferenceValue = player;
                var overhead = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>(true))
                    .Where(r => r.name == "P02_Shelter_Roof").ToArray();
                var list = so.FindProperty("overheadScenery"); list.arraySize = overhead.Length;
                var details = so.FindProperty("attachedDetails"); details.arraySize = overhead.Length;
                for (int i = 0; i < overhead.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = overhead[i];
                var renderers = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
                for (int i = 0; i < overhead.Length; i++)
                    details.GetArrayElementAtIndex(i).objectReferenceValue = renderers.FirstOrDefault(r => r.name.StartsWith(overhead[i].name == "P02_Gate_Header" ? "P02_Sign_SELAMAT" : "P02_Sign_RUANG"));
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Editor_Gerbang_Keamanan.ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        finally
        {
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
    }

    static Material CreateMaterial(Mat source)
    {
        string path = Folder + "Materials/" + source.name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = Shader.Find(string.IsNullOrEmpty(source.texture) ? "Universal Render Pipeline/Lit" : "JejakAsa/Packed Earth");
        mat.SetColor("_BaseColor", new Color(source.color[0], source.color[1], source.color[2], 1));
        mat.SetFloat("_Smoothness", .04f);
        if (!string.IsNullOrEmpty(source.texture))
        {
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + source.texture));
            mat.SetColor("_BaseColor", source.name.EndsWith("Packed_Earth") ? new Color(.72f,.61f,.43f,1) : new Color(.42f,.48f,.28f,1));
            mat.SetFloat("_DetailAlbedoMapScale", .24f);
        }
        if (source.name.StartsWith("Landscape_Leaf")) mat.SetFloat("_Cull", 0);
        EditorUtility.SetDirty(mat); return mat;
    }
}
#endif
