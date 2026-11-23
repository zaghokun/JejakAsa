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

internal static class Editor_Level1_VisualLingkungan
{
    private const string ScenePath = "Assets/_Project/Scenes/Kampung.unity";
    private const string CharacterFolder = "Assets/_Project/Art/Characters/";
    private const string EnvironmentFolder = "Assets/_Project/Art/Environment/";
    private const string LandscapeFolder = "Assets/_Project/Art/Environment/Landscape/Materials/";
    private const string PoskoMatFolder = "Assets/_Project/Art/Environment/PoskoP02/Materials/";
    private const string PrologueMatFolder = "Assets/_Project/Art/Environment/Prologue/";
    private const string RootVisualName = "Level1_Visual_Environment";
    private const string RootHazardsName = "Level1_Strategic_Hazards";

    [MenuItem("Jejak Asa/★ TERAPKAN LEVEL 1 SEKARANG ★", false, 0)]
    [MenuItem("Jejak Asa/Level 1/Apply Level 1 Design & Visuals", false, 1)]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before applying Level 1 layout.");

        Log("Apply() called at " + DateTime.Now);

        Scene scene;
        bool opened = false;
        var active = SceneManager.GetActiveScene();

        if (active.IsValid() && active.isLoaded && active.name == "Kampung")
        {
            scene = active;
            Log("Using currently active Kampung scene.");
        }
        else
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Log("Opening Kampung.unity additively...");
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        var previousActive = SceneManager.GetActiveScene();
        if (scene.isDirty)
            throw new InvalidOperationException("Save the current Kampung edits before regenerating Level 1.");
        if (scene != previousActive)
            SceneManager.SetActiveScene(scene);

        try
        {
            ApplyLayoutAndVisuals(scene);
            Editor_Level1_RuteDanRintangan.ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Log("SUCCESS: Level 1 Layout & Visuals successfully applied and saved!");
            Debug.Log("<color=green><b>[Jejak Asa] Level 1 Layout, Boundaries, Foliage & NPC Facing successfully updated in Kampung.unity!</b></color>");
        }
        catch (Exception ex)
        {
            Log("ERROR during ApplyLayoutAndVisuals: " + ex);
            throw;
        }
        finally
        {
            if (opened)
            {
                if (previousActive.IsValid() && previousActive.isLoaded)
                    SceneManager.SetActiveScene(previousActive);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    [MenuItem("Jejak Asa/Level 1/Revert to Graybox Layout", false, 2)]
    public static void Revert()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before reverting Level 1.");

        Log("Revert() called at " + DateTime.Now);

        Scene scene;
        bool opened = false;
        var active = SceneManager.GetActiveScene();

        if (active.IsValid() && active.isLoaded && active.name == "Kampung")
        {
            scene = active;
        }
        else
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        var previousActive = SceneManager.GetActiveScene();
        if (scene != previousActive)
            SceneManager.SetActiveScene(scene);

        try
        {
            RevertLayoutAndVisuals(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Log("SUCCESS: Reverted to Graybox layout.");
            Debug.Log("<color=yellow><b>[Jejak Asa] Level 1 reverted to original Graybox layout.</b></color>");
        }
        finally
        {
            if (opened)
            {
                if (previousActive.IsValid() && previousActive.isLoaded)
                    SceneManager.SetActiveScene(previousActive);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static void ApplyLayoutAndVisuals(Scene scene)
    {
        Log("ApplyLayoutAndVisuals: Cleaning old objects...");
        foreach (var old in scene.GetRootGameObjects().Where(o => o.name == RootVisualName || o.name == RootHazardsName).ToArray())
            UnityEngine.Object.DestroyImmediate(old);

        Func<string, GameObject> find = name =>
            scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == name)?.gameObject;

        Log("ApplyLayoutAndVisuals: Checking Humanoid configuration...");
        ConfigureHumanoid(CharacterFolder + "P01/Ibu.fbx");
        ConfigureHumanoid(CharacterFolder + "P01/Vicky.fbx");
        ConfigureHumanoid(CharacterFolder + "PoskoP02/PakArif.fbx");
        ConfigureHumanoid(CharacterFolder + "PoskoP02/BuRina.fbx");
        ConfigureHumanoid(CharacterFolder + "PrologueVillains/Preman.fbx");

        var waterMat = GetOrCreateWaterMaterial();
        var plasterMat0 = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Kampung_Plaster_0.mat");
        var plasterMat1 = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Kampung_Plaster_1.mat");
        var terracottaRoof = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_Terracotta.002.mat");
        var timberMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_Timber.002.mat");
        var concreteMat = AssetDatabase.LoadAssetAtPath<Material>(PrologueMatFolder + "Threat_Concrete.mat");
        var ironMat = AssetDatabase.LoadAssetAtPath<Material>(PrologueMatFolder + "Threat_Iron.mat");
        var pavingMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Paving_0.002.mat");
        var earthMat = AssetDatabase.LoadAssetAtPath<Material>(LandscapeFolder + "Landscape_Packed_Earth.mat");
        var leafMat0 = AssetDatabase.LoadAssetAtPath<Material>(LandscapeFolder + "Landscape_Leaf_0.mat");
        var leafMat1 = AssetDatabase.LoadAssetAtPath<Material>(LandscapeFolder + "Landscape_Leaf_1.mat");
        var flowerMat = AssetDatabase.LoadAssetAtPath<Material>(LandscapeFolder + "Landscape_Bougainvillea.mat");
        var barkMat = AssetDatabase.LoadAssetAtPath<Material>(LandscapeFolder + "Landscape_Bark.mat");
        var deepGreenMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_DeepGreen.002.mat");

        var fbxMeshes = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Environment/PoskoP02/PoskoP02.fbx")
            .OfType<Mesh>()
            .GroupBy(m => m.name)
            .ToDictionary(g => g.Key, g => g.First());

        var fallbackCanopyMesh = CreateFacetedIcosahedronMesh(1.15f);

        // 2. Character Setup (With proper facing rotation: facing South toward street & Vicky)
        Log("ApplyLayoutAndVisuals: Setting up Preman...");
        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.transform.position = new Vector3(0.5f, 1f, -2.2f);
            preman.transform.rotation = Quaternion.Euler(0, 180, 0); // Faces South towards the approaching player
            AddCharacterVisual(preman, "Preman_Visual",
                CharacterFolder + "PrologueVillains/Preman.fbx",
                PrologueMatFolder + "Preman_Threat.mat", 1.80f, "Preman");

            var pc = preman.GetComponent<Obstacle_Preman>();
            if (pc != null)
            {
                var so = new SerializedObject(pc);
                so.FindProperty("detectionRadius").floatValue = 7f;
                so.FindProperty("moveSpeed").floatValue = 2.7f;
                so.FindProperty("timePenalty").floatValue = 10f;
                so.FindProperty("contactCooldown").floatValue = 2f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        Log("ApplyLayoutAndVisuals: Setting up Warga 1, 2, 3 (Facing south toward road)...");
        var warga1 = find("Warga_01_Graybox");
        if (warga1 != null)
        {
            warga1.transform.localRotation = Quaternion.Euler(0, 180, 0); // Faces South toward street / Vicky
            AddCharacterVisual(warga1, "Ibu_Visual",
                CharacterFolder + "P01/Ibu.fbx",
                CharacterFolder + "P01/Ibu.mat", 1.62f, "Ibu Nur");

            var it = warga1.GetComponent<Mission_PenerimaBantuan>();
            if (it != null)
            {
                var so = new SerializedObject(it);
                so.FindProperty("displayName").stringValue = "Ibu Nur (Warga Pos)";
                so.FindProperty("dialogue").stringValue = "Terima kasih banyak, Nak! Kami sekeluarga sangat terbantu dengan bantuan sembako ini. Hati-hati ya, gang sebelah sana banyak preman berkeliaran...";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var warga2 = find("Warga_02_Graybox");
        if (warga2 != null)
        {
            warga2.transform.localRotation = Quaternion.Euler(0, 180, 0); // Faces South toward street
            AddCharacterVisual(warga2, "PakArif_Visual",
                CharacterFolder + "PoskoP02/PakArif.fbx",
                CharacterFolder + "PoskoP02/PakArif.mat", 1.72f, "Pak Arif");

            var it = warga2.GetComponent<Mission_PenerimaBantuan>();
            if (it != null)
            {
                var so = new SerializedObject(it);
                so.FindProperty("displayName").stringValue = "Pak Arif (Warga Tengah)";
                so.FindProperty("dialogue").stringValue = "Alhamdulillah berasnya sampai juga. Tadi lewat depan aman kan? Hati-hati kalau lewat gang utama, preman suka memalak dan banyak genangan becek!";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var warga3 = find("Warga_03_Graybox");
        if (warga3 != null)
        {
            warga3.transform.localRotation = Quaternion.Euler(0, 180, 0); // Faces South toward street
            AddCharacterVisual(warga3, "BuRina_Visual",
                CharacterFolder + "PoskoP02/BuRina.fbx",
                CharacterFolder + "PoskoP02/BuRina.mat", 1.63f, "Bu Rina");

            var it = warga3.GetComponent<Mission_PenerimaBantuan>();
            if (it != null)
            {
                var so = new SerializedObject(it);
                so.FindProperty("displayName").stringValue = "Bu Rina (Warga Timur)";
                so.FindProperty("dialogue").stringValue = "Terima kasih sudah rela mengantar sampai ujung gang sini, Nak. Hujan kemarin bikin jalan becek, untung kardus sembakonya aman.";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        Log("ApplyLayoutAndVisuals: Setting up Player...");
        var player = find("Player");
        if (player != null)
        {
            AddCharacterVisual(player, "Vicky_Visual",
                CharacterFolder + "P01/Vicky.fbx",
                CharacterFolder + "P01/Vicky.mat", 1.70f, "Vicky");
        }

        // 3. Strategic Level Layout & Puddles
        Log("ApplyLayoutAndVisuals: Setting up puddles and hazards...");
        var genanganUtama = find("Genangan_Graybox");
        if (genanganUtama != null)
        {
            genanganUtama.transform.position = new Vector3(0f, 0f, -0.2f);
            var box = genanganUtama.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = new Vector3(0, 0.6f, 0);
                box.size = new Vector3(2.8f, 1.2f, 2.6f);
            }
            var pz = genanganUtama.GetComponent<Obstacle_Genangan>();
            if (pz != null)
            {
                var so = new SerializedObject(pz);
                so.FindProperty("speedMultiplier").floatValue = 0.5f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var surface = genanganUtama.transform.Find("Permukaan_Genangan");
            if (surface != null)
            {
                surface.localPosition = new Vector3(0, 0.05f, 0);
                surface.localScale = new Vector3(2.8f, 0.02f, 2.6f);
                var mr = surface.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = waterMat;
            }
        }

        var hazardsRoot = new GameObject(RootHazardsName);
        SceneManager.MoveGameObjectToScene(hazardsRoot, scene);

        CreatePuddle(hazardsRoot.transform, "Genangan_Barat", new Vector3(-8f, 0f, 0.2f), new Vector3(2.6f, 1.2f, 2.2f), 0.65f, waterMat);
        CreatePuddle(hazardsRoot.transform, "Genangan_Timur", new Vector3(6f, 0f, -3.8f), new Vector3(2.4f, 1.2f, 2.0f), 0.65f, waterMat);

        // 4. Visual Environment & Expansive Indonesian Kampung Dressing
        Log("ApplyLayoutAndVisuals: Setting up environment visuals...");
        var visualRoot = new GameObject(RootVisualName);
        SceneManager.MoveGameObjectToScene(visualRoot, scene);

        // A. Expand ground plane so it is no longer a small box
        var plane = find("Plane");
        if (plane != null)
        {
            plane.transform.localScale = new Vector3(12f, 1f, 12f); // 120x120m expansive terrain
            if (earthMat != null)
                plane.GetComponent<MeshRenderer>().sharedMaterial = earthMat;
        }

        // B. Roads
        foreach (string roadName in new[] { "Gang_Awal", "Gang_Selatan", "Gang_Utama", "Gang_Utara", "Gang_Memutar", "Gang_RumahVicky" })
        {
            var road = find(roadName);
            if (road != null && pavingMat != null)
                road.GetComponent<MeshRenderer>().sharedMaterial = pavingMat;
        }

        // C. Houses with authentic Indonesian architectural details
        DressHouse(visualRoot.transform, find("Rumah_01"), "Rumah01_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat, new Vector3(-12f, 0f, 4f), new Vector3(6f, 3.2f, 5f));
        DressHouse(visualRoot.transform, find("Rumah_02"), "Rumah02_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat, new Vector3(0f, 0f, 8f), new Vector3(6f, 3.2f, 5f));
        DressHouse(visualRoot.transform, find("Rumah_03"), "Rumah03_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat, new Vector3(12f, 0f, 8f), new Vector3(6f, 3.2f, 5f));
        DressHouse(visualRoot.transform, find("Rumah_Vicky"), "RumahVicky_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat, new Vector3(8f, 0f, -12f), new Vector3(6f, 3.2f, 5f));

        DressPosBantuan(visualRoot.transform, find("PosBantuan"), timberMat, concreteMat);

        // D. Street lamps at crossroads & corners
        AddStreetLamp(visualRoot.transform, new Vector3(1.8f, 0f, -1.8f), timberMat);
        AddStreetLamp(visualRoot.transform, new Vector3(-6.2f, 0f, -2.5f), timberMat);
        AddStreetLamp(visualRoot.transform, new Vector3(6.5f, 0f, 1.8f), timberMat);
        AddStreetLamp(visualRoot.transform, new Vector3(-10.2f, 0f, -0.2f), timberMat);

        AddPropCrate(visualRoot.transform, new Vector3(-14.2f, 0.45f, -11.8f), new Vector3(0.9f, 0.9f, 0.9f), timberMat);
        AddPropCrate(visualRoot.transform, new Vector3(-14.2f, 1.25f, -11.8f), new Vector3(0.7f, 0.7f, 0.7f), timberMat);
        AddPropCrate(visualRoot.transform, new Vector3(-15.1f, 0.45f, -11.8f), new Vector3(0.85f, 0.9f, 0.85f), timberMat);

        // E. Replace bare concrete barriers with authentic wooden/tin fences, foliage & Bougainvillea!
        DressBarricade(visualRoot.transform, find("Pembatas_Gang_Kiri"), timberMat, ironMat, leafMat0, flowerMat);
        DressBarricade(visualRoot.transform, find("Pembatas_Gang_Kanan"), timberMat, ironMat, leafMat1, flowerMat);

        // F. Replace 4 bare boundary walls with lush perimeter greenery, tree lines, fences & power poles!
        DressPerimeter(visualRoot.transform, find, timberMat, ironMat, concreteMat, leafMat0, flowerMat, barkMat, fbxMeshes, terracottaRoof, deepGreenMat, fallbackCanopyMesh);

        // Authentic Indonesian Low-Poly Village Trees (Matching Posko broadleaf models with faceted canopies)
        CreateAuthenticTree(visualRoot.transform, new Vector3(-18f, 0f, -9f), 1.05f, 0, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(-16.5f, 0f, 6.5f), 1.0f, 1, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(5.5f, 0f, 11f), 1.1f, 2, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(16.5f, 0f, 6.5f), 1.0f, 3, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(14.5f, 0f, -12f), 1.0f, 0, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(-3.5f, 0f, -11f), 0.95f, 1, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        CreateAuthenticTree(visualRoot.transform, new Vector3(3.5f, 0f, 11f), 0.95f, 2, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);

        // G. Warm Afternoon Atmospheric Lighting
        var sunLight = find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            sunLight.color = new Color(1.0f, 0.95f, 0.88f);
            sunLight.intensity = 1.15f;
            sunLight.shadows = LightShadows.Soft;
        }
        Log("ApplyLayoutAndVisuals: Complete.");
    }

    private static void RevertLayoutAndVisuals(Scene scene)
    {
        Editor_Level1_RuteDanRintangan.RemoveFromScene(scene);
        foreach (var old in scene.GetRootGameObjects().Where(o => o.name == RootVisualName || o.name == RootHazardsName).ToArray())
            UnityEngine.Object.DestroyImmediate(old);

        Func<string, GameObject> find = name =>
            scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == name)?.gameObject;

        foreach (string charName in new[] { "Preman_Graybox", "Warga_01_Graybox", "Warga_02_Graybox", "Warga_03_Graybox", "Player" })
        {
            var go = find(charName);
            if (go == null) continue;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = true;
            foreach (Transform child in go.transform.Cast<Transform>().ToArray())
                if (child.name.EndsWith("_Visual"))
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        foreach (string houseName in new[] { "Rumah_01", "Rumah_02", "Rumah_03", "Rumah_Vicky" })
        {
            var go = find(houseName);
            if (go != null && go.GetComponent<MeshRenderer>() != null)
                go.GetComponent<MeshRenderer>().enabled = true;
        }

        foreach (string batasName in new[] { "Batas_Utara", "Batas_Selatan", "Batas_Barat", "Batas_Timur", "Pembatas_Gang_Kiri", "Pembatas_Gang_Kanan" })
        {
            var batas = find(batasName);
            if (batas != null && batas.GetComponent<MeshRenderer>() != null)
                batas.GetComponent<MeshRenderer>().enabled = true;
        }

        var plane = find("Plane");
        if (plane != null) plane.transform.localScale = new Vector3(4f, 1f, 4f);

        var genangan = find("Genangan_Graybox");
        if (genangan != null)
        {
            genangan.transform.position = new Vector3(-12f, 0f, -6f);
            var box = genangan.GetComponent<BoxCollider>();
            if (box != null) box.size = new Vector3(3.2f, 1.2f, 2.4f);
        }

        var preman = find("Preman_Graybox");
        if (preman != null)
            preman.transform.position = new Vector3(0f, 1f, -3f);
    }

    private static void ConfigureHumanoid(string fbxPath)
    {
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null && importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
        }
    }

    private static void AddCharacterVisual(GameObject actor, string visualName, string fbxPath, string matPath, float height, string speakerName)
    {
        if (actor == null) return;

        var oldRenderer = actor.GetComponent<MeshRenderer>();
        if (oldRenderer != null) oldRenderer.enabled = false;

        Transform oldVisual = actor.transform.Find(visualName);
        if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);

        var wrapper = new GameObject(visualName);
        wrapper.transform.SetParent(actor.transform, false);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (prefab == null)
        {
            Debug.LogWarning($"FBX model not found at {fbxPath}");
            return;
        }

        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        model.transform.SetParent(wrapper.transform, false);

        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            if (b.size.y > 0.001f)
                wrapper.transform.localScale = Vector3.one * (height / b.size.y);

            b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            wrapper.transform.position += new Vector3(actor.transform.position.x - b.center.x, -b.min.y, actor.transform.position.z - b.center.z);
        }

        if (!string.IsNullOrEmpty(matPath))
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null)
            {
                foreach (var r in renderers)
                    r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();
            }
        }

        var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
        animator.avatar = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<Avatar>().FirstOrDefault(a => a.isHuman);
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/P01/P01Character.controller");
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (!string.IsNullOrEmpty(speakerName))
        {
            var driver = model.GetComponent<Character_AnimasiGerak>() ?? model.AddComponent<Character_AnimasiGerak>();
            var so = new SerializedObject(driver);
            so.FindProperty("characterName").stringValue = speakerName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static GameObject CreatePuddle(Transform parent, string name, Vector3 pos, Vector3 colliderSize, float speedMult, Material waterMat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, colliderSize.y * 0.5f, 0);
        col.size = colliderSize;

        var pz = go.AddComponent<Obstacle_Genangan>();
        var so = new SerializedObject(pz);
        so.FindProperty("speedMultiplier").floatValue = speedMult;
        so.ApplyModifiedPropertiesWithoutUndo();

        var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "Permukaan_Air";
        surface.transform.SetParent(go.transform, false);
        surface.transform.localPosition = new Vector3(0, 0.05f, 0);
        surface.transform.localScale = new Vector3(colliderSize.x, 0.02f, colliderSize.z);
        UnityEngine.Object.DestroyImmediate(surface.GetComponent<Collider>());
        surface.GetComponent<MeshRenderer>().sharedMaterial = waterMat;

        return go;
    }

    private static void DressHouse(Transform root, GameObject houseBox, string visualName, Material wallMat, Material roofMat, Material timberMat, Material foundationMat, Vector3 pos, Vector3 size)
    {
        if (houseBox != null && houseBox.GetComponent<MeshRenderer>() != null)
            houseBox.GetComponent<MeshRenderer>().enabled = false;

        var house = new GameObject(visualName);
        house.transform.SetParent(root, false);
        house.transform.position = pos;

        CreateCube(house.transform, "Foundation", new Vector3(0, 0.2f, 0), new Vector3(size.x + 0.4f, 0.4f, size.z + 0.4f), foundationMat);
        CreateCube(house.transform, "Main_Walls", new Vector3(0, size.y * 0.5f, 0), new Vector3(size.x, size.y, size.z), wallMat);
        CreateCube(house.transform, "Roof_Base", new Vector3(0, size.y + 0.35f, 0), new Vector3(size.x + 0.8f, 0.3f, size.z + 0.8f), roofMat);
        CreateCube(house.transform, "Roof_Ridge", new Vector3(0, size.y + 0.75f, 0), new Vector3(size.x + 0.2f, 0.5f, size.z * 0.5f), roofMat);
        CreateCube(house.transform, "Porch_Canopy", new Vector3(0, 2.2f, -size.z * 0.5f - 0.6f), new Vector3(size.x * 0.65f, 0.15f, 1.4f), roofMat);

        CreateCube(house.transform, "Post_Left", new Vector3(-size.x * 0.3f, 1.1f, -size.z * 0.5f - 1.2f), new Vector3(0.12f, 2.2f, 0.12f), timberMat);
        CreateCube(house.transform, "Post_Right", new Vector3(size.x * 0.3f, 1.1f, -size.z * 0.5f - 1.2f), new Vector3(0.12f, 2.2f, 0.12f), timberMat);
        CreateCube(house.transform, "Front_Door", new Vector3(0, 1.1f, -size.z * 0.5f - 0.02f), new Vector3(1.2f, 2.0f, 0.08f), timberMat);

        CreateCube(house.transform, "Window_L", new Vector3(-1.8f, 1.5f, -size.z * 0.5f - 0.02f), new Vector3(1.0f, 1.0f, 0.06f), timberMat);
        CreateCube(house.transform, "Window_R", new Vector3(1.8f, 1.5f, -size.z * 0.5f - 0.02f), new Vector3(1.0f, 1.0f, 0.06f), timberMat);

        var lightGo = new GameObject("Porch_Light");
        lightGo.transform.SetParent(house.transform, false);
        lightGo.transform.localPosition = new Vector3(0, 2.3f, -size.z * 0.5f - 0.7f);
        var pt = lightGo.AddComponent<Light>();
        pt.type = LightType.Point;
        pt.color = new Color(1.0f, 0.75f, 0.45f);
        pt.intensity = 35f;
        pt.range = 6f;
    }

    private static void DressPosBantuan(Transform root, GameObject posBantuanBox, Material timberMat, Material concreteMat)
    {
        if (posBantuanBox != null && posBantuanBox.GetComponent<MeshRenderer>() != null)
            posBantuanBox.GetComponent<MeshRenderer>().enabled = false;

        var shelter = new GameObject("PosBantuan_Visual");
        shelter.transform.SetParent(root, false);
        shelter.transform.position = new Vector3(-16f, 0f, -13f);

        CreateCube(shelter.transform, "Platform", new Vector3(0, 0.15f, 0), new Vector3(6.4f, 0.3f, 3.4f), concreteMat);
        CreateCube(shelter.transform, "Canopy", new Vector3(0, 2.8f, 0), new Vector3(6.6f, 0.2f, 3.6f), timberMat);

        foreach (float x in new[] { -3.0f, 3.0f })
            foreach (float z in new[] { -1.5f, 1.5f })
                CreateCube(shelter.transform, "Post", new Vector3(x, 1.4f, z), new Vector3(0.18f, 2.6f, 0.18f), timberMat);
    }

    private static void AddStreetLamp(Transform root, Vector3 pos, Material timberMat)
    {
        var lamp = new GameObject("StreetLamp");
        lamp.transform.SetParent(root, false);
        lamp.transform.position = pos;

        CreateCube(lamp.transform, "Pole", new Vector3(0, 1.6f, 0), new Vector3(0.14f, 3.2f, 0.14f), timberMat);
        CreateCube(lamp.transform, "Arm", new Vector3(0.3f, 3.1f, 0), new Vector3(0.7f, 0.1f, 0.1f), timberMat);
        CreateCube(lamp.transform, "Lantern", new Vector3(0.6f, 2.95f, 0), new Vector3(0.25f, 0.25f, 0.25f), timberMat);

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(lamp.transform, false);
        lightGo.transform.localPosition = new Vector3(0.6f, 2.85f, 0);
        var pt = lightGo.AddComponent<Light>();
        pt.type = LightType.Point;
        pt.color = new Color(1.0f, 0.8f, 0.5f);
        pt.intensity = 45f;
        pt.range = 7f;
    }

    private static void AddPropCrate(Transform root, Vector3 pos, Vector3 size, Material woodMat)
    {
        CreateCube(root, "Crate", pos, size, woodMat);
    }

    private static void DressBarricade(Transform root, GameObject pembatasBox, Material timberMat, Material ironMat, Material leafMat, Material flowerMat)
    {
        if (pembatasBox == null) return;
        var mr = pembatasBox.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        Vector3 pos = pembatasBox.transform.position;
        Vector3 size = pembatasBox.transform.localScale;

        var barricade = new GameObject(pembatasBox.name + "_Visual");
        barricade.transform.SetParent(root, false);
        barricade.transform.position = pos;

        float width = size.x;
        int postCount = Mathf.CeilToInt(width / 1.2f) + 1;
        float startX = -width * 0.5f;
        float step = width / (postCount - 1);

        for (int i = 0; i < postCount; i++)
        {
            float x = startX + i * step;
            CreateCube(barricade.transform, "Post", new Vector3(x, 0.7f, 0), new Vector3(0.14f, 1.4f, 0.14f), timberMat);
        }

        CreateCube(barricade.transform, "Rail_Top", new Vector3(0, 1.15f, 0), new Vector3(width, 0.1f, 0.08f), timberMat);
        CreateCube(barricade.transform, "Rail_Bottom", new Vector3(0, 0.35f, 0), new Vector3(width, 0.1f, 0.08f), timberMat);

        int panelCount = Mathf.FloorToInt(width / 0.8f);
        for (int i = 0; i < panelCount; i++)
        {
            float x = startX + 0.4f + i * 0.8f;
            Material panelMat = (i % 2 == 0) ? ironMat : timberMat;
            CreateCube(barricade.transform, "Panel", new Vector3(x, 0.75f, (i % 2 == 0 ? 0.04f : -0.04f)), new Vector3(0.72f, 1.1f, 0.04f), panelMat);
        }

        int bushCount = Mathf.CeilToInt(width / 0.9f);
        for (int i = 0; i < bushCount; i++)
        {
            float x = startX + 0.45f + i * 0.9f;
            float bushH = 0.55f + (i % 3) * 0.12f;
            CreateCube(barricade.transform, "Bush", new Vector3(x, bushH * 0.5f, 0.25f), new Vector3(0.75f, bushH, 0.45f), leafMat);
            if (flowerMat != null && i % 2 == 0)
                CreateCube(barricade.transform, "Flower", new Vector3(x + 0.1f, bushH * 0.8f, 0.32f), new Vector3(0.35f, 0.3f, 0.2f), flowerMat);
        }

        CreateCube(barricade.transform, "Crate_L", new Vector3(startX - 0.4f, 0.35f, 0), new Vector3(0.7f, 0.7f, 0.7f), timberMat);
        CreateCube(barricade.transform, "Crate_R", new Vector3(-startX + 0.4f, 0.35f, 0), new Vector3(0.7f, 0.7f, 0.7f), timberMat);
    }

    private static void DressPerimeter(
        Transform root, 
        Func<string, GameObject> find, 
        Material timberMat, 
        Material ironMat, 
        Material concreteMat, 
        Material leafMat, 
        Material flowerMat, 
        Material barkMat,
        Dictionary<string, Mesh> fbxMeshes,
        Material planterMat,
        Material deepGreenMat,
        Mesh fallbackCanopyMesh)
    {
        foreach (string batasName in new[] { "Batas_Utara", "Batas_Selatan", "Batas_Barat", "Batas_Timur" })
        {
            var batas = find(batasName);
            if (batas != null && batas.GetComponent<MeshRenderer>() != null)
                batas.GetComponent<MeshRenderer>().enabled = false;
        }

        var perimeter = new GameObject("Kampung_Perimeter_Environment");
        perimeter.transform.SetParent(root, false);

        int treeIndex = 0;
        for (float x = -20f; x <= 20f; x += 3.5f)
        {
            Material pMat = (Mathf.Abs(x) % 2 < 1) ? ironMat : timberMat;
            CreateCube(perimeter.transform, "Fence_North", new Vector3(x, 0.75f, 20.2f), new Vector3(3.2f, 1.5f, 0.15f), pMat);
            CreateAuthenticTree(perimeter.transform, new Vector3(x, 0f, 22.5f), 1.0f, treeIndex++, fbxMeshes, planterMat, timberMat, deepGreenMat, fallbackCanopyMesh);
            if (flowerMat != null && Mathf.Abs(x) % 7 < 3.5f)
                CreateCube(perimeter.transform, "Bougainvillea", new Vector3(x, 1.2f, 20.0f), new Vector3(1.2f, 1.0f, 0.6f), flowerMat);
        }

        for (float x = -20f; x <= 20f; x += 4f)
        {
            CreateCube(perimeter.transform, "Fence_South", new Vector3(x, 0.75f, -20.2f), new Vector3(3.6f, 1.5f, 0.15f), timberMat);
            CreateAuthenticTree(perimeter.transform, new Vector3(x, 0f, -22.5f), 1.0f, treeIndex++, fbxMeshes, planterMat, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        for (float z = -20f; z <= 20f; z += 4f)
        {
            CreateCube(perimeter.transform, "Fence_West", new Vector3(-20.2f, 0.75f, z), new Vector3(0.15f, 1.5f, 3.6f), ironMat);
            CreateAuthenticTree(perimeter.transform, new Vector3(-22.5f, 0f, z), 1.0f, treeIndex++, fbxMeshes, planterMat, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        for (float z = -20f; z <= 20f; z += 4f)
        {
            CreateCube(perimeter.transform, "Fence_East", new Vector3(20.2f, 0.75f, z), new Vector3(0.15f, 1.5f, 3.6f), timberMat);
            CreateAuthenticTree(perimeter.transform, new Vector3(22.5f, 0f, z), 1.0f, treeIndex++, fbxMeshes, planterMat, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        AddPowerPole(perimeter.transform, new Vector3(-18f, 0f, 18f), timberMat);
        AddPowerPole(perimeter.transform, new Vector3(18f, 0f, 18f), timberMat);
        AddPowerPole(perimeter.transform, new Vector3(-18f, 0f, -18f), timberMat);
        AddPowerPole(perimeter.transform, new Vector3(18f, 0f, -18f), timberMat);
    }

    private static void AddPowerPole(Transform root, Vector3 pos, Material timberMat)
    {
        var pole = new GameObject("UtilityPowerPole");
        pole.transform.SetParent(root, false);
        pole.transform.position = pos;

        CreateCube(pole.transform, "MainPole", new Vector3(0, 3.5f, 0), new Vector3(0.22f, 7.0f, 0.22f), timberMat);
        CreateCube(pole.transform, "CrossArm_Top", new Vector3(0, 6.4f, 0), new Vector3(1.6f, 0.12f, 0.12f), timberMat);
        CreateCube(pole.transform, "CrossArm_Low", new Vector3(0, 5.6f, 0), new Vector3(1.2f, 0.12f, 0.12f), timberMat);
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = localScale;
        UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
        if (mat != null)
            cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return cube;
    }

    private static GameObject CreateAuthenticTree(
        Transform parent,
        Vector3 rootPosition,
        float scale,
        int variant,
        Dictionary<string, Mesh> fbxMeshes,
        Material planterMat,
        Material trunkMat,
        Material leafMat,
        Mesh fallbackCanopyMesh)
    {
        var tree = new GameObject("Tree_Authentic");
        tree.transform.SetParent(parent, false);
        tree.transform.position = rootPosition;
        tree.transform.localScale = Vector3.one * scale;

        int pIdx = Mathf.Abs(variant) % 4;
        string planterName = $"P02_Tree_Planter_{pIdx}";
        string trunkName = $"P02_Tree_Trunk_{pIdx}";

        string centerCanopyName = pIdx == 0 ? "P02_Broadleaf_Canopy" : $"P02_Broadleaf_Canopy.{pIdx * 3:D3}";
        string leftCanopyName = $"P02_Broadleaf_Canopy.{pIdx * 3 + 1:D3}";
        string rightCanopyName = $"P02_Broadleaf_Canopy.{pIdx * 3 + 2:D3}";

        // 1. Terracotta Planter Base
        Mesh planterMesh = null;
        if (fbxMeshes != null && (!fbxMeshes.TryGetValue(planterName, out planterMesh) || planterMesh == null))
            fbxMeshes.TryGetValue("P02_Tree_Planter_0", out planterMesh);

        if (planterMesh != null)
            CreateMeshObject(tree.transform, "Planter", planterMesh, planterMat, new Vector3(0, 0.22f, 0), Vector3.one, Quaternion.identity);
        else
            CreateCube(tree.transform, "Planter", new Vector3(0, 0.22f, 0), new Vector3(1.1f, 0.44f, 1.1f), planterMat);

        // 2. Trunk
        Mesh trunkMesh = null;
        if (fbxMeshes != null && (!fbxMeshes.TryGetValue(trunkName, out trunkMesh) || trunkMesh == null))
            fbxMeshes.TryGetValue("P02_Tree_Trunk_0", out trunkMesh);

        if (trunkMesh != null)
            CreateMeshObject(tree.transform, "Trunk", trunkMesh, trunkMat, new Vector3(0, 1.2f, 0), Vector3.one, Quaternion.identity);
        else
            CreateCube(tree.transform, "Trunk", new Vector3(0, 1.2f, 0), new Vector3(0.24f, 2.0f, 0.24f), trunkMat);

        // 3. Faceted Broadleaf Canopy Clusters
        Mesh cCenter = null;
        if (fbxMeshes != null && (!fbxMeshes.TryGetValue(centerCanopyName, out cCenter) || cCenter == null))
            fbxMeshes.TryGetValue("P02_Broadleaf_Canopy", out cCenter);
        CreateMeshObject(tree.transform, "Canopy_Center", cCenter ?? fallbackCanopyMesh, leafMat, new Vector3(0, 3.0f, 0), Vector3.one, Quaternion.identity);

        Mesh cLeft = null;
        if (fbxMeshes != null && (!fbxMeshes.TryGetValue(leftCanopyName, out cLeft) || cLeft == null))
            fbxMeshes.TryGetValue("P02_Broadleaf_Canopy.001", out cLeft);
        CreateMeshObject(tree.transform, "Canopy_Left", cLeft ?? fallbackCanopyMesh, leafMat, new Vector3(-0.70f, 2.60f, 0.15f), Vector3.one, Quaternion.identity);

        Mesh cRight = null;
        if (fbxMeshes != null && (!fbxMeshes.TryGetValue(rightCanopyName, out cRight) || cRight == null))
            fbxMeshes.TryGetValue("P02_Broadleaf_Canopy.002", out cRight);
        CreateMeshObject(tree.transform, "Canopy_Right", cRight ?? fallbackCanopyMesh, leafMat, new Vector3(0.65f, 2.80f, -0.10f), Vector3.one, Quaternion.identity);

        return tree;
    }

    private static GameObject CreateMeshObject(Transform parent, string name, Mesh mesh, Material mat, Vector3 localPos, Vector3 localScale, Quaternion localRot)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;
        if (mesh != null)
        {
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
        }
        if (mat != null)
        {
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
        }
        return go;
    }

    private static Mesh CreateFacetedIcosahedronMesh(float radius)
    {
        var mesh = new Mesh();
        mesh.name = "Procedural_Faceted_Canopy";

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        Vector3[] v = new Vector3[]
        {
            new Vector3(-1,  t,  0).normalized * radius,
            new Vector3( 1,  t,  0).normalized * radius,
            new Vector3(-1, -t,  0).normalized * radius,
            new Vector3( 1, -t,  0).normalized * radius,

            new Vector3( 0, -1,  t).normalized * radius,
            new Vector3( 0,  1,  t).normalized * radius,
            new Vector3( 0, -1, -t).normalized * radius,
            new Vector3( 0,  1, -t).normalized * radius,

            new Vector3( t,  0, -1).normalized * radius,
            new Vector3( t,  0,  1).normalized * radius,
            new Vector3(-t,  0, -1).normalized * radius,
            new Vector3(-t,  0,  1).normalized * radius
        };

        int[] triIndices = new int[]
        {
            0, 11, 5,   0, 5, 1,    0, 1, 7,    0, 7, 10,   0, 10, 11,
            1, 5, 9,    5, 11, 4,   11, 10, 2,  10, 7, 6,   7, 1, 8,
            3, 9, 4,    3, 4, 2,    3, 2, 6,    3, 6, 8,    3, 8, 9,
            4, 9, 5,    2, 4, 11,   6, 2, 10,   8, 6, 7,    9, 8, 1
        };

        Vector3[] flatVertices = new Vector3[triIndices.Length];
        Vector3[] flatNormals = new Vector3[triIndices.Length];
        int[] flatTriangles = new int[triIndices.Length];

        for (int i = 0; i < triIndices.Length; i += 3)
        {
            Vector3 v0 = v[triIndices[i]];
            Vector3 v1 = v[triIndices[i + 1]];
            Vector3 v2 = v[triIndices[i + 2]];

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            flatVertices[i] = v0;
            flatVertices[i + 1] = v1;
            flatVertices[i + 2] = v2;

            flatNormals[i] = normal;
            flatNormals[i + 1] = normal;
            flatNormals[i + 2] = normal;

            flatTriangles[i] = i;
            flatTriangles[i + 1] = i + 1;
            flatTriangles[i + 2] = i + 2;
        }

        mesh.vertices = flatVertices;
        mesh.normals = flatNormals;
        mesh.triangles = flatTriangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Material GetOrCreateWaterMaterial()
    {
        const string path = "Assets/_Project/Materials/Level1_WaterPuddle.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            mat.SetColor("_BaseColor", new Color(0.24f, 0.28f, 0.26f, 0.88f));
            mat.SetFloat("_Smoothness", 0.94f);
            mat.SetFloat("_Metallic", 0.15f);
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static void Log(string message)
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "../apply_log.txt");
            File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
        }
        catch { }
    }
}
#endif
