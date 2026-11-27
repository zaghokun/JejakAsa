#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class Editor_Level2_PascaHujan
{
    private const string ScenePath = "Assets/_Project/Scenes/Kampung_L2.unity";
    private const string CharacterFolder = "Assets/_Project/Art/Characters/";
    private const string LandscapeFolder = "Assets/_Project/Art/Environment/Landscape/Materials/";
    private const string PoskoMatFolder = "Assets/_Project/Art/Environment/PoskoP02/Materials/";
    private const string PrologueMatFolder = "Assets/_Project/Art/Environment/Prologue/";
    private const string Level1PuddlesFolder = "Assets/_Project/Art/Environment/Level1Puddles/";
    private const string Level2PuddlesFolder = "Assets/_Project/Art/Environment/Level2Puddles/";
    private const string RootVisualName = "Level2_Visual_Environment";
    private const string RootChallengeName = "Level2_Challenge_Layout";

    private struct ResidentSpec
    {
        public string objectName;
        public string visualName;
        public string fbx;
        public string mat;
        public float height;
        public string speaker;
        public string displayName;
        public string dialogue;
    }

    [MenuItem("Jejak Asa/★ TERAPKAN LEVEL 2 SEKARANG ★", false, 10)]
    [MenuItem("Jejak Asa/Level 2/Apply Level 2 Post-Rain Design & Visuals", false, 11)]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before applying Level 2 layout.");

        RunOnLevel2Scene(scene =>
        {
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=cyan><b>[Jejak Asa] Level 2 (Pasca-Hujan, Jembatan Papan & Patroli) berhasil diterapkan ke Kampung_L2.unity!</b></color>");
        });
    }

    [MenuItem("Jejak Asa/Level 2/Revert Level 2 to Graybox", false, 12)]
    public static void Revert()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before reverting Level 2.");

        RunOnLevel2Scene(scene =>
        {
            RemoveFromScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=yellow><b>[Jejak Asa] Level 2 dikembalikan ke tampilan Graybox.</b></color>");
        });
    }

    private static void RunOnLevel2Scene(Action<Scene> action)
    {
        Scene scene = SceneManager.GetActiveScene();
        bool opened = false;
        Scene previousActive = scene;

        if (!scene.IsValid() || !scene.isLoaded || scene.name != "Kampung_L2")
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        if (scene.isDirty)
            throw new InvalidOperationException("Save current Kampung_L2 edits before running Level 2 tool.");
        if (scene != previousActive)
            SceneManager.SetActiveScene(scene);

        try
        {
            action(scene);
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

    internal static void ApplyToScene(Scene scene)
    {
        EnsureFolderExists(Level2PuddlesFolder);

        foreach (var old in scene.GetRootGameObjects()
            .Where(o => o.name == RootVisualName || o.name == RootChallengeName)
            .ToArray())
        {
            UnityEngine.Object.DestroyImmediate(old);
        }

        Func<string, GameObject> find = name =>
            scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == name)?.gameObject;

        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "P01/Ibu.fbx");
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "P01/Vicky.fbx");
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "PoskoP02/PakArif.fbx");
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "PoskoP02/BuRina.fbx");
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "PrologueVillains/Preman.fbx");

        Editor_Level1_RuteDanRintangan.ConfigureTextures();
        var mud = LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_MudBank.mat");
        var water = LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Water.mat");
        var ripple = LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Ripple.mat");
        var routeTimber = LoadRequiredMaterial(Level1PuddlesFolder + "Route_Timber.mat");
        var routeFence = LoadRequiredMaterial(Level1PuddlesFolder + "Route_Fence.mat");
        var guardMaterial = LoadRequiredMaterial(Level1PuddlesFolder + "Preman_Level1_Daylight.mat");

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

        var plankMat = GetOrCreateMaterial(Level2PuddlesFolder + "Level2_BridgePlank.mat", new Color(0.46f, 0.31f, 0.17f), 0.22f);
        var sandbagMat = GetOrCreateMaterial(Level2PuddlesFolder + "Level2_Sandbag.mat", new Color(0.68f, 0.60f, 0.44f), 0.10f);
        var warningMat = GetOrCreateMaterial(Level2PuddlesFolder + "Level2_FloodWarning.mat", new Color(0.76f, 0.42f, 0.14f), 0.25f);

        var fbxMeshes = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Environment/PoskoP02/PoskoP02.fbx")
            .OfType<Mesh>()
            .GroupBy(m => m.name)
            .ToDictionary(g => g.Key, g => g.First());
        var fallbackCanopyMesh = Editor_Level1_VisualLingkungan.CreateFacetedIcosahedronMesh(1.15f);

        SetupCharacters(find, guardMaterial);

        var visualRoot = new GameObject(RootVisualName);
        SceneManager.MoveGameObjectToScene(visualRoot, scene);
        BuildVisualEnvironment(visualRoot.transform, find, plasterMat0, plasterMat1, terracottaRoof, timberMat, concreteMat, ironMat, pavingMat, earthMat, leafMat0, leafMat1, flowerMat, barkMat, deepGreenMat, fbxMeshes, fallbackCanopyMesh);

        var challengeRoot = new GameObject(RootChallengeName);
        SceneManager.MoveGameObjectToScene(challengeRoot, scene);
        BuildChallengeLayout(challengeRoot.transform, find, mud, water, ripple, routeTimber, routeFence, plankMat, sandbagMat, warningMat, guardMaterial);
    }

    private static void SetupCharacters(Func<string, GameObject> find, Material guardMaterial)
    {
        var player = find("Player");
        if (player != null)
        {
            Editor_Level1_VisualLingkungan.AddCharacterVisual(player, "Vicky_Visual",
                CharacterFolder + "P01/Vicky.fbx", CharacterFolder + "P01/Vicky.mat", 1.70f, "Vicky");
        }

        ResidentSpec[] residents =
        {
            new ResidentSpec
            {
                objectName = "Warga_01_Graybox", visualName = "Ibu_Visual",
                fbx = CharacterFolder + "P01/Ibu.fbx", mat = CharacterFolder + "P01/Ibu.mat", height = 1.62f,
                speaker = "Ibu Nur", displayName = "Ibu Nur (Warga Pos)",
                dialogue = "Syukurlah kamu datang lagi hari ini, Nak. Semalam hujan deras bikin gang tengah terendam dan ditutup barikade. Kalau mau ke rumah Pak Arif, lewat jembatan papan di barat ya!"
            },
            new ResidentSpec
            {
                objectName = "Warga_02_Graybox", visualName = "PakArif_Visual",
                fbx = CharacterFolder + "PoskoP02/PakArif.fbx", mat = CharacterFolder + "PoskoP02/PakArif.mat", height = 1.72f,
                speaker = "Pak Arif", displayName = "Pak Arif (Warga Tengah)",
                dialogue = "Terima kasih bantuannya, Nak! Banjir semalam bikin lumpurnya dalam sekali, untung warga sudah pasang jembatan papan di gang sebelah. Tetap awasi preman yang patroli di jalan ya!"
            },
            new ResidentSpec
            {
                objectName = "Warga_03_Graybox", visualName = "BuRina_Visual",
                fbx = CharacterFolder + "PoskoP02/BuRina.fbx", mat = CharacterFolder + "PoskoP02/BuRina.mat", height = 1.63f,
                speaker = "Bu Rina", displayName = "Bu Rina (Warga Timur)",
                dialogue = "Alhamdulillah paket hari kedua sampai juga. Air masih menggenang di gang depan, hati-hati melangkah di pinggir jalan supaya tidak terjebak lumpur."
            }
        };

        foreach (var r in residents)
        {
            var go = find(r.objectName);
            if (go == null) continue;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Editor_Level1_VisualLingkungan.AddCharacterVisual(go, r.visualName, r.fbx, r.mat, r.height, r.speaker);
            var recipient = go.GetComponent<Mission_PenerimaBantuan>();
            if (recipient != null)
            {
                var so = new SerializedObject(recipient);
                so.FindProperty("displayName").stringValue = r.displayName;
                so.FindProperty("dialogue").stringValue = r.dialogue;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.SetActive(false);
            preman.transform.SetPositionAndRotation(new Vector3(-5.0f, 1f, 4.4f), Quaternion.Euler(0f, 180f, 0f));
            Editor_Level1_VisualLingkungan.AddCharacterVisual(preman, "Preman_Visual",
                CharacterFolder + "PrologueVillains/Preman.fbx", PrologueMatFolder + "Preman_Threat.mat", 1.80f, "Preman");
            ApplyGuardMaterials(preman, guardMaterial);
            preman.SetActive(true);
        }
    }

    private static void BuildVisualEnvironment(
        Transform visualRoot,
        Func<string, GameObject> find,
        Material plasterMat0,
        Material plasterMat1,
        Material terracottaRoof,
        Material timberMat,
        Material concreteMat,
        Material ironMat,
        Material pavingMat,
        Material earthMat,
        Material leafMat0,
        Material leafMat1,
        Material flowerMat,
        Material barkMat,
        Material deepGreenMat,
        System.Collections.Generic.Dictionary<string, Mesh> fbxMeshes,
        Mesh fallbackCanopyMesh)
    {
        var plane = find("Plane");
        if (plane != null)
        {
            plane.transform.localScale = new Vector3(12f, 1f, 12f);
            if (earthMat != null)
                plane.GetComponent<MeshRenderer>().sharedMaterial = earthMat;
        }

        foreach (string roadName in new[] { "Gang_Awal", "Gang_Selatan", "Gang_Memutar", "Gang_Utama", "Gang_Utara", "Gang_RumahVicky" })
        {
            var road = find(roadName);
            if (road != null && pavingMat != null)
                road.GetComponent<MeshRenderer>().sharedMaterial = pavingMat;
        }

        // Dress houses aligned with each house collider in Kampung_L2.unity
        DressExistingHouse(visualRoot, find("Rumah_01"), "Rumah01_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat);
        DressExistingHouse(visualRoot, find("Rumah_02"), "Rumah02_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat);
        DressExistingHouse(visualRoot, find("Rumah_03"), "Rumah03_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat);
        DressExistingHouse(visualRoot, find("Rumah_Vicky"), "RumahVicky_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat);

        Editor_Level1_VisualLingkungan.DressPosBantuan(visualRoot, find("PosBantuan"), timberMat, concreteMat);

        foreach (Vector3 lampPos in new[]
        {
            new Vector3(-12.5f, 0f, -2.5f),
            new Vector3(-6.5f, 0f, 2.8f),
            new Vector3(6.5f, 0f, 2.8f),
            new Vector3(9.5f, 0f, -2.5f)
        })
        {
            Editor_Level1_VisualLingkungan.AddStreetLamp(visualRoot, lampPos, timberMat);
        }

        Editor_Level1_VisualLingkungan.AddPropCrate(visualRoot, new Vector3(-14.2f, 0.45f, -11.8f), new Vector3(0.9f, 0.9f, 0.9f), timberMat);
        Editor_Level1_VisualLingkungan.AddPropCrate(visualRoot, new Vector3(-14.2f, 1.25f, -11.8f), new Vector3(0.7f, 0.7f, 0.7f), timberMat);
        Editor_Level1_VisualLingkungan.AddPropCrate(visualRoot, new Vector3(-15.1f, 0.45f, -11.8f), new Vector3(0.85f, 0.9f, 0.85f), timberMat);

        Editor_Level1_VisualLingkungan.DressBarricade(visualRoot, find("Pembatas_Gang_Kiri"), timberMat, ironMat, leafMat0, flowerMat);
        Editor_Level1_VisualLingkungan.DressBarricade(visualRoot, find("Pembatas_Gang_Kanan"), timberMat, ironMat, leafMat1, flowerMat);

        Editor_Level1_VisualLingkungan.DressPerimeter(visualRoot, find, timberMat, ironMat, concreteMat, leafMat0, flowerMat, barkMat, fbxMeshes, terracottaRoof, deepGreenMat, fallbackCanopyMesh);

        Vector3[] villageTreePositions =
        {
            new Vector3(-18.0f, 0f, -9.0f),
            new Vector3(-18.2f, 0f, 7.5f),
            new Vector3(-5.0f, 0f, 12.0f),
            new Vector3(8.5f, 0f, 12.5f),
            new Vector3(17.8f, 0f, 7.5f),
            new Vector3(14.5f, 0f, -12.0f),
            new Vector3(-3.5f, 0f, -11.5f)
        };
        for (int i = 0; i < villageTreePositions.Length; i++)
        {
            Editor_Level1_VisualLingkungan.CreateAuthenticTree(
                visualRoot, villageTreePositions[i], 1.0f, i, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        // Cool overcast post-rain lighting (distinct from Level 1's warm orange sunset)
        var sunLight = find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            sunLight.color = new Color(0.80f, 0.88f, 1.0f);
            sunLight.intensity = 0.98f;
            sunLight.shadows = LightShadows.Soft;
        }
    }

    private static void DressExistingHouse(Transform root, GameObject houseBox, string visualName, Material wallMat, Material roofMat, Material timberMat, Material concreteMat)
    {
        if (houseBox == null) return;
        Vector3 p = houseBox.transform.position;
        Vector3 s = houseBox.transform.localScale;
        Editor_Level1_VisualLingkungan.DressHouse(root, houseBox, visualName, wallMat, roofMat, timberMat, concreteMat,
            new Vector3(p.x, 0f, p.z), new Vector3(s.x, 3.2f, s.z));
    }

    private static void BuildChallengeLayout(
        Transform root,
        Func<string, GameObject> find,
        Material mud,
        Material water,
        Material ripple,
        Material routeTimber,
        Material routeFence,
        Material plankMat,
        Material sandbagMat,
        Material warningMat,
        Material guardMaterial)
    {
        var legacyWater = find("Genangan_Graybox");
        if (legacyWater != null) legacyWater.SetActive(false);
        var legacyMarker = find("Penanda_Intimidasi_BelumAktif");
        if (legacyMarker != null) legacyMarker.SetActive(false);

        // Paving paths guiding Day 2 routes cleanly
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Warga_Barat", new Vector3(-14f, 0.022f, -0.5f), new Vector3(3.0f, 0.012f, 6.0f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Kanan", new Vector3(13.2f, 0.022f, -0.2f), new Vector3(3.0f, 0.012f, 6.2f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Utara", new Vector3(8.2f, 0.022f, 2.5f), new Vector3(2.4f, 0.012f, 5.2f), routeTimber);

        // Flood barricades: center road closed, outer edges closed
        Editor_Level1_RuteDanRintangan.Fence(root, "Barikade_Banjir_Tengah", new Vector3(0f, 0f, 0f), new Vector3(3.2f, 1.35f, 0.5f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Box(root, "Palang_Peringatan_Tengah_Selatan", new Vector3(0f, 0.92f, -0.28f), new Vector3(2.9f, 0.22f, 0.06f), warningMat);
        Editor_Level1_RuteDanRintangan.Box(root, "Palang_Peringatan_Tengah_Utara", new Vector3(0f, 0.92f, 0.28f), new Vector3(2.9f, 0.22f, 0.06f), warningMat);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Barat", new Vector3(-18.5f, 0f, 3.5f), new Vector3(3.2f, 1.35f, 0.35f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Timur", new Vector3(18.0f, 0f, 3.5f), new Vector3(4.0f, 1.35f, 0.35f), routeTimber, routeFence);

        // Sandbag stacks reinforcing flood barriers and bridge heads
        foreach (Vector3 pos in new[]
        {
            new Vector3(-1.0f, 0f, -0.5f), new Vector3(1.0f, 0f, -0.5f),
            new Vector3(-1.0f, 0f, 0.5f), new Vector3(1.0f, 0f, 0.5f),
            new Vector3(-10.85f, 0f, -2.35f), new Vector3(-9.15f, 0f, -2.35f),
            new Vector3(-10.85f, 0f, 3.35f), new Vector3(-9.15f, 0f, 3.35f),
            new Vector3(-13.8f, 0f, -10.8f)
        })
        {
            CreateSandbagStack(root, pos, sandbagMat);
        }

        // Visual continuous mud/water bed under the wooden bridge (non-trigger so bridge stays dry)
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L2_AlasJembatan", new Vector3(-10.0f, 0f, 0.5f), 4.6f, 4.4f, 1.0f, 19, mud, water, ripple, Level2PuddlesFolder, interactiveTrigger: false, addWater: true);

        // Interactive flanking puddles (slipping off planks slows movement to 0.48x)
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L2_Barat_Kiri", new Vector3(-11.6f, 0f, 0.5f), 1.7f, 4.2f, 0.48f, 23, mud, water, ripple, Level2PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L2_Barat_Kanan", new Vector3(-8.4f, 0f, 0.5f), 1.7f, 4.2f, 0.48f, 31, mud, water, ripple, Level2PuddlesFolder, interactiveTrigger: true, addWater: true);

        // Flood puddle in front of blocked center barricade and wide east puddle
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L2_Simpang", new Vector3(0.0f, 0f, -2.2f), 4.2f, 2.5f, 0.65f, 41, mud, water, ripple, Level2PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L2_Timur", new Vector3(9.7f, 0f, -0.3f), 3.8f, 4.4f, 0.48f, 57, mud, water, ripple, Level2PuddlesFolder, interactiveTrigger: true, addWater: true);

        // Wooden plank bridge crossing the flooded western alley
        BuildWoodenPlankBridge(root, new Vector3(-10.0f, 0f, 0.5f), plankMat, routeTimber);

        // Patrols (2 Preman guarding North Boulevard & East Approach)
        var original = find("Preman_Graybox");
        if (original != null)
        {
            original.SetActive(false);
            original.transform.SetPositionAndRotation(new Vector3(-5.0f, 1f, 4.4f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureLevel2Guard(original, new[] { new Vector3(-7.5f, 1f, 4.4f), new Vector3(-1.2f, 1f, 4.4f) });
            original.SetActive(true);

            var patrolEast = UnityEngine.Object.Instantiate(original, root);
            patrolEast.name = "Preman_02_PatroliTimur";
            patrolEast.transform.SetPositionAndRotation(new Vector3(11.5f, 1f, -2.2f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureLevel2Guard(patrolEast, new[] { new Vector3(8.5f, 1f, -2.2f), new Vector3(14.5f, 1f, -2.2f) });
            ApplyGuardMaterials(patrolEast, guardMaterial);
        }

        // Calibrated Day 2 timer (60 seconds)
        var player = find("Player")?.GetComponent<Mission_PengantaranBantuan>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            so.FindProperty("initialTimeSeconds").floatValue = 60f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureLevel2Guard(GameObject go, Vector3[] patrolRoute)
    {
        var guard = go.GetComponent<Obstacle_Preman>();
        if (guard == null) return;
        var so = new SerializedObject(guard);
        so.FindProperty("detectionRadius").floatValue = 5.8f;
        so.FindProperty("moveSpeed").floatValue = 3.9f;
        so.FindProperty("patrolSpeed").floatValue = 1.65f;
        so.FindProperty("turnSpeed").floatValue = 540f;
        so.FindProperty("timePenalty").floatValue = 10f;
        so.FindProperty("contactCooldown").floatValue = 2.4f;
        so.FindProperty("leashDistance").floatValue = 6.8f;
        so.FindProperty("detectFromHome").boolValue = true;
        so.FindProperty("requireLineOfSight").boolValue = true;
        so.FindProperty("retireAfterContacts").intValue = 0;
        so.FindProperty("retreatAfterContactSeconds").floatValue = 1.6f;
        so.FindProperty("sharedContactGraceSeconds").floatValue = 1.1f;
        var points = so.FindProperty("patrolPoints");
        points.arraySize = patrolRoute.Length;
        for (int i = 0; i < patrolRoute.Length; i++)
            points.GetArrayElementAtIndex(i).vector3Value = patrolRoute[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        foreach (var driver in go.GetComponentsInChildren<Character_AnimasiGerak>(true))
        {
            var animSo = new SerializedObject(driver);
            animSo.FindProperty("runThreshold").floatValue = 3.0f;
            animSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ApplyGuardMaterials(GameObject guardGo, Material mat)
    {
        if (guardGo == null || mat == null) return;
        foreach (var renderer in guardGo.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            renderer.sharedMaterials = Enumerable.Repeat(mat, renderer.sharedMaterials.Length).ToArray();
    }

    private static void BuildWoodenPlankBridge(Transform root, Vector3 center, Material plankMat, Material timberMat)
    {
        var bridge = new GameObject("Jembatan_Papan_Barat");
        bridge.transform.SetParent(root, false);
        bridge.transform.position = center;

        // Longitudinal support stringers
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Balok_Kiri", new Vector3(-0.48f, 0.052f, 0f), new Vector3(0.14f, 0.05f, 5.1f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Balok_Kanan", new Vector3(0.48f, 0.052f, 0f), new Vector3(0.14f, 0.05f, 5.1f), timberMat);

        // 11 transverse weathered wooden planks
        const int plankCount = 11;
        for (int i = 0; i < plankCount; i++)
        {
            float z = -2.30f + i * 0.46f;
            float xJitter = (i % 2 == 0 ? 0.02f : -0.02f);
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_{i:D2}", new Vector3(xJitter, 0.072f, z), new Vector3(1.36f, 0.032f, 0.38f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.4f, 0f);
        }

        // Approach ramps & low wooden guide curbs
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Ramp_Selatan", new Vector3(0f, 0.048f, -2.72f), new Vector3(1.42f, 0.03f, 0.36f), plankMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Ramp_Utara", new Vector3(0f, 0.048f, 2.72f), new Vector3(1.42f, 0.03f, 0.36f), plankMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "List_Kiri", new Vector3(-0.66f, 0.11f, 0f), new Vector3(0.08f, 0.07f, 4.9f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "List_Kanan", new Vector3(0.66f, 0.11f, 0f), new Vector3(0.08f, 0.07f, 4.9f), timberMat);
    }

    private static void CreateSandbagStack(Transform root, Vector3 pos, Material sandbagMat)
    {
        var stack = new GameObject("KarungPasir");
        stack.transform.SetParent(root, false);
        stack.transform.position = pos;

        Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Bawah1", new Vector3(-0.18f, 0.11f, 0f), new Vector3(0.42f, 0.22f, 0.32f), sandbagMat);
        Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Bawah2", new Vector3(0.22f, 0.11f, 0.03f), new Vector3(0.42f, 0.22f, 0.32f), sandbagMat);
        var top = Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Atas", new Vector3(0.02f, 0.31f, 0.01f), new Vector3(0.44f, 0.20f, 0.30f), sandbagMat);
        top.transform.localRotation = Quaternion.Euler(0f, 6f, 0f);
    }

    internal static void RemoveFromScene(Scene scene)
    {
        foreach (var old in scene.GetRootGameObjects()
            .Where(o => o.name == RootVisualName || o.name == RootChallengeName)
            .ToArray())
        {
            UnityEngine.Object.DestroyImmediate(old);
        }

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

        foreach (string name in new[] { "Rumah_01", "Rumah_02", "Rumah_03", "Rumah_Vicky", "PosBantuan", "Batas_Utara", "Batas_Selatan", "Batas_Barat", "Batas_Timur", "Pembatas_Gang_Kiri", "Pembatas_Gang_Kanan" })
        {
            var go = find(name);
            if (go != null && go.GetComponent<MeshRenderer>() != null)
                go.GetComponent<MeshRenderer>().enabled = true;
        }

        var plane = find("Plane");
        if (plane != null) plane.transform.localScale = new Vector3(4f, 1f, 4f);

        var legacyWater = find("Genangan_Graybox");
        if (legacyWater != null) legacyWater.SetActive(true);
        var legacyMarker = find("Penanda_Intimidasi_BelumAktif");
        if (legacyMarker != null) legacyMarker.SetActive(true);

        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.transform.SetPositionAndRotation(new Vector3(0f, 1f, -3f), Quaternion.identity);
            var guard = preman.GetComponent<Obstacle_Preman>();
            if (guard != null)
            {
                var so = new SerializedObject(guard);
                so.FindProperty("detectionRadius").floatValue = 7f;
                so.FindProperty("moveSpeed").floatValue = 2.8f;
                so.FindProperty("patrolPoints").arraySize = 0;
                so.FindProperty("leashDistance").floatValue = 0f;
                so.FindProperty("detectFromHome").boolValue = false;
                so.FindProperty("requireLineOfSight").boolValue = false;
                so.FindProperty("retireAfterContacts").intValue = 3;
                so.FindProperty("retreatAfterContactSeconds").floatValue = 0f;
                so.FindProperty("sharedContactGraceSeconds").floatValue = 0f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var player = find("Player")?.GetComponent<Mission_PengantaranBantuan>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            so.FindProperty("initialTimeSeconds").floatValue = 180f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static Material LoadRequiredMaterial(string path)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
            throw new InvalidOperationException($"Material not found at {path}. Apply Level 1 routes/puddles first.");
        return mat;
    }

    private static Material GetOrCreateMaterial(string path, Color color, float smoothness)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void EnsureFolderExists(string assetFolder)
    {
        string trimmed = assetFolder.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(trimmed)) return;
        string parent = Path.GetDirectoryName(trimmed).Replace('\\', '/');
        string folderName = Path.GetFileName(trimmed);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
