#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class Editor_Level3_BlokadeDeddy
{
    private const string ScenePath = "Assets/_Project/Scenes/Kampung_L3.unity";
    private const string CharacterFolder = "Assets/_Project/Art/Characters/";
    private const string LandscapeFolder = "Assets/_Project/Art/Environment/Landscape/Materials/";
    private const string PoskoMatFolder = "Assets/_Project/Art/Environment/PoskoP02/Materials/";
    private const string PrologueMatFolder = "Assets/_Project/Art/Environment/Prologue/";
    private const string Level1PuddlesFolder = "Assets/_Project/Art/Environment/Level1Puddles/";
    private const string Level3PuddlesFolder = "Assets/_Project/Art/Environment/Level3Puddles/";
    private const string RootVisualName = "Level3_Visual_Environment";
    private const string RootChallengeName = "Level3_Challenge_Layout";

    [MenuItem("Jejak Asa/★ TERAPKAN LEVEL 3 SEKARANG ★", false, 20)]
    [MenuItem("Jejak Asa/Level 3/Apply Level 3 Mr. Deddy Blockade", false, 21)]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before applying Level 3 layout.");

        RunOnLevel3Scene(scene =>
        {
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=orange><b>[Jejak Asa] Level 3 (Blokade Mr. Deddy, Patroli Segitiga & Senja) berhasil diterapkan ke Kampung_L3.unity!</b></color>");
        });
    }

    [MenuItem("Jejak Asa/Level 3/Revert Level 3 to Graybox", false, 22)]
    public static void Revert()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before reverting Level 3.");

        RunOnLevel3Scene(scene =>
        {
            RemoveFromScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=yellow><b>[Jejak Asa] Level 3 dikembalikan ke tampilan Graybox.</b></color>");
        });
    }

    private static void RunOnLevel3Scene(Action<Scene> action)
    {
        Scene scene = SceneManager.GetActiveScene();
        bool opened = false;
        Scene previousActive = scene;

        if (!scene.IsValid() || !scene.isLoaded || scene.name != "Kampung_L3")
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        if (scene.isDirty)
            throw new InvalidOperationException("Save current Kampung_L3 edits before running Level 3 tool.");
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
        Editor_Level2_PascaHujan.EnsureFolderExists(Level3PuddlesFolder);

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
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "PrologueVillains/Mr_Deddy.fbx");

        Editor_Level1_RuteDanRintangan.ConfigureTextures();
        var mud = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_MudBank.mat");
        var water = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Water.mat");
        var ripple = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Ripple.mat");
        var routeTimber = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Timber.mat");
        var routeFence = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Fence.mat");
        var guardMaterial = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Preman_Level1_Daylight.mat");

        var deddyMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level3PuddlesFolder + "Mr_Deddy_Level3_Dusk.mat", new Color(0.92f, 0.88f, 0.82f), 0.16f);
        var deddyTex = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterFolder + "PrologueVillains/Mr_Deddy_BaseColor.jpg");
        if (deddyTex != null)
        {
            deddyMat.SetTexture("_BaseMap", deddyTex);
            deddyMat.SetTexture("_MainTex", deddyTex);
        }

        var drumMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level3PuddlesFolder + "Level3_DrumLogam.mat", new Color(0.62f, 0.24f, 0.16f), 0.35f);

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
        var fallbackCanopyMesh = Editor_Level1_VisualLingkungan.CreateFacetedIcosahedronMesh(1.15f);

        SetupCharacters(find, guardMaterial);

        var visualRoot = new GameObject(RootVisualName);
        SceneManager.MoveGameObjectToScene(visualRoot, scene);
        BuildVisualEnvironment(visualRoot.transform, find, plasterMat0, plasterMat1, terracottaRoof, timberMat, concreteMat, ironMat, pavingMat, earthMat, leafMat0, leafMat1, flowerMat, barkMat, deepGreenMat, fbxMeshes, fallbackCanopyMesh);

        var challengeRoot = new GameObject(RootChallengeName);
        SceneManager.MoveGameObjectToScene(challengeRoot, scene);
        BuildChallengeLayout(challengeRoot.transform, find, mud, water, ripple, routeTimber, routeFence, ironMat, timberMat, drumMat, guardMaterial, deddyMat);
    }

    private static void SetupCharacters(Func<string, GameObject> find, Material guardMaterial)
    {
        var player = find("Player");
        if (player != null)
        {
            Editor_Level1_VisualLingkungan.AddCharacterVisual(player, "Vicky_Visual",
                CharacterFolder + "P01/Vicky.fbx", CharacterFolder + "P01/Vicky.mat", 1.70f, "Vicky");
        }

        Editor_Level2_PascaHujan.ResidentSpec[] residents =
        {
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_01_Graybox", visualName = "Ibu_Visual",
                fbx = CharacterFolder + "P01/Ibu.fbx", mat = CharacterFolder + "P01/Ibu.mat", height = 1.62f,
                speaker = "Ibu Nur", displayName = "Ibu Nur (Warga Pos)",
                dialogue = "Terima kasih, Nak! Hati-hati ya, Mr. Deddy turun langsung memblokade lorong tengah pakai barikade seng dan lumpur pekat!"
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_02_Graybox", visualName = "PakArif_Visual",
                fbx = CharacterFolder + "PoskoP02/PakArif.fbx", mat = CharacterFolder + "PoskoP02/PakArif.mat", height = 1.72f,
                speaker = "Pak Arif", displayName = "Pak Arif (Warga Tengah)",
                dialogue = "Syukur kamu berhasil lolos dari kepungan Mr. Deddy! Manfaatkan barikade tengah untuk berlindung dari pandangannya sebelum menuju ke timur."
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_03_Graybox", visualName = "BuRina_Visual",
                fbx = CharacterFolder + "PoskoP02/BuRina.fbx", mat = CharacterFolder + "PoskoP02/BuRina.mat", height = 1.63f,
                speaker = "Bu Rina", displayName = "Bu Rina (Warga Timur)",
                dialogue = "Alhamdulillah akhirnya sampai juga, Nak! Meski gang timur dijaga ketat anak buah Mr. Deddy, bantuan ini sangat berarti buat kami."
            }
        };

        Editor_Level2_PascaHujan.ApplyResidents(find, residents);

        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.SetActive(false);
            preman.transform.SetPositionAndRotation(new Vector3(-8.5f, 1f, -2.5f), Quaternion.Euler(0f, 180f, 0f));
            Editor_Level1_VisualLingkungan.AddCharacterVisual(preman, "Preman_Visual",
                CharacterFolder + "PrologueVillains/Preman.fbx", PrologueMatFolder + "Preman_Threat.mat", 1.80f, "Preman");
            Editor_Level2_PascaHujan.ApplyGuardMaterials(preman, guardMaterial);
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
        Dictionary<string, Mesh> fbxMeshes,
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

        Editor_Level2_PascaHujan.DressExistingHouse(visualRoot, find("Rumah_01"), "Rumah01_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat);
        Editor_Level2_PascaHujan.DressExistingHouse(visualRoot, find("Rumah_02"), "Rumah02_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat);
        Editor_Level2_PascaHujan.DressExistingHouse(visualRoot, find("Rumah_03"), "Rumah03_Visual", plasterMat0, terracottaRoof, timberMat, concreteMat);
        Editor_Level2_PascaHujan.DressExistingHouse(visualRoot, find("Rumah_Vicky"), "RumahVicky_Visual", plasterMat1, terracottaRoof, timberMat, concreteMat);

        Editor_Level1_VisualLingkungan.DressPosBantuan(visualRoot, find("PosBantuan"), timberMat, concreteMat);

        foreach (Vector3 lampPos in new[]
        {
            new Vector3(-13.8f, 0f, -2.5f),
            new Vector3(-6.2f, 0f, 3.8f),
            new Vector3(6.8f, 0f, 3.8f),
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
            new Vector3(-17.8f, 0f, 11.5f),
            new Vector3(-4.5f, 0f, 14.5f),
            new Vector3(10.5f, 0f, 14.2f),
            new Vector3(18.2f, 0f, 11.5f),
            new Vector3(14.5f, 0f, -12.0f),
            new Vector3(-3.5f, 0f, -11.5f)
        };
        for (int i = 0; i < villageTreePositions.Length; i++)
        {
            Editor_Level1_VisualLingkungan.CreateAuthenticTree(
                visualRoot, villageTreePositions[i], 1.0f, i, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        // Dramatic warm-dusk golden lighting signaling heightened threat
        var sunLight = find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            sunLight.color = new Color(1.0f, 0.80f, 0.62f);
            sunLight.intensity = 0.95f;
            sunLight.shadows = LightShadows.Soft;
        }
    }

    private static void BuildChallengeLayout(
        Transform root,
        Func<string, GameObject> find,
        Material mud,
        Material water,
        Material ripple,
        Material routeTimber,
        Material routeFence,
        Material ironMat,
        Material timberMat,
        Material drumMat,
        Material guardMaterial,
        Material deddyMat)
    {
        var legacyWater = find("Genangan_Graybox");
        if (legacyWater != null) legacyWater.SetActive(false);
        var legacyMarker = find("Penanda_Intimidasi_BelumAktif");
        if (legacyMarker != null) legacyMarker.SetActive(false);

        // Paving paths along the tight syndicate corridors
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Warga_Barat", new Vector3(-13.2f, 0.022f, 0.8f), new Vector3(3.0f, 0.012f, 7.2f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Kanan", new Vector3(14.0f, 0.022f, -0.4f), new Vector3(3.0f, 0.012f, 7.0f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Hubung_Timur", new Vector3(10.6f, 0.022f, 3.8f), new Vector3(5.2f, 0.012f, 3.0f), routeTimber);

        // Seal open outer grass yards & rear house lawns so players must navigate tight alleys
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_BaratLuar", new Vector3(-15.5f, 0f, 1.0f), new Vector3(0.45f, 1.35f, 11.6f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Barat", new Vector3(-17.5f, 0f, 9.0f), new Vector3(5.2f, 1.35f, 0.40f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_TimurLuar", new Vector3(16.5f, 0f, 0.5f), new Vector3(0.45f, 1.35f, 11.0f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Timur", new Vector3(18.5f, 0f, 8.0f), new Vector3(3.2f, 1.35f, 0.40f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Selatan_Tengah", new Vector3(0.0f, 0f, -6.2f), new Vector3(19.0f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sekat_Vicky_Kanan", new Vector3(13.8f, 0f, -6.2f), new Vector3(5.8f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Barat", new Vector3(-4.5f, 0f, 11.2f), new Vector3(9.2f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Timur", new Vector3(8.5f, 0f, 10.2f), new Vector3(5.4f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);

        // Internal zig-zag chicane walls forcing tactical weaving & blocking straight sprints
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Seng_Barat_1", new Vector3(-10.6f, 0f, -1.6f), new Vector3(0.45f, 1.35f, 4.4f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_ZigZag_Barat_Atas", new Vector3(-13.2f, 0f, 3.0f), new Vector3(4.2f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Chicane_Tengah", new Vector3(1.5f, 0f, 4.4f), new Vector3(5.4f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Seng_Timur_Bawah", new Vector3(11.2f, 0f, -2.0f), new Vector3(0.45f, 1.35f, 3.6f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Seng_Timur_Tengah", new Vector3(14.2f, 0f, 1.6f), new Vector3(4.4f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);

        // Blockade drums & crates marking tight bottleneck corners
        foreach (Vector3 pos in new[]
        {
            new Vector3(-1.8f, 0f, -0.5f), new Vector3(1.8f, 0f, -0.5f),
            new Vector3(-11.0f, 0f, 2.5f), new Vector3(-1.4f, 0f, 4.4f),
            new Vector3(4.4f, 0f, 4.4f), new Vector3(11.8f, 0f, 1.6f)
        })
        {
            CreateBlockadeCluster(root, pos, drumMat, timberMat);
        }

        // 5 heavy organic 3-layer PBR mud puddles placed right at corners and bottlenecks
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L3_BaratBawah", new Vector3(-13.1f, 0f, -0.8f), 3.4f, 3.2f, 0.44f, 13, mud, water, ripple, Level3PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L3_BaratAtas", new Vector3(-8.5f, 0f, 3.6f), 3.2f, 2.8f, 0.44f, 27, mud, water, ripple, Level3PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L3_Utama_Pekat", new Vector3(0.0f, 0f, -1.6f), 3.4f, 3.2f, 0.40f, 37, mud, water, ripple, Level3PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L3_Timur", new Vector3(8.4f, 0f, 1.8f), 3.0f, 3.4f, 0.42f, 61, mud, water, ripple, Level3PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L3_Timur_Kanan", new Vector3(13.8f, 0f, -1.5f), 3.4f, 2.8f, 0.46f, 79, mud, water, ripple, Level3PuddlesFolder, interactiveTrigger: true, addWater: true);

        // 4 Guards: 3 Tactical Henchmen + Fast Aggressive Mr. Deddy orbiting the center chicane
        var original = find("Preman_Graybox");
        if (original != null)
        {
            original.SetActive(false);
            original.transform.SetPositionAndRotation(new Vector3(-9.5f, 1f, 2.0f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureLevel3Guard(original,
                new[] { new Vector3(-13.6f, 1f, 1.6f), new Vector3(-8.5f, 1f, 1.8f), new Vector3(-8.5f, 1f, -2.5f) },
                radius: 5.8f, chaseSpeed: 4.2f, patrolSpeed: 1.85f, leash: 7.0f, penalty: 10f, retreatSeconds: 1.4f);
            original.SetActive(true);

            // 2. Southeast Henchman guarding the east lower loop
            var patrolEast = UnityEngine.Object.Instantiate(original, root);
            patrolEast.name = "Preman_02_PatroliTimur";
            patrolEast.transform.SetPositionAndRotation(new Vector3(13.8f, 1f, -0.5f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureLevel3Guard(patrolEast,
                new[] { new Vector3(13.8f, 1f, 0.6f), new Vector3(13.8f, 1f, -3.2f), new Vector3(8.2f, 1f, -3.2f) },
                radius: 5.8f, chaseSpeed: 4.2f, patrolSpeed: 1.85f, leash: 7.0f, penalty: 10f, retreatSeconds: 1.4f);
            Editor_Level2_PascaHujan.ApplyGuardMaterials(patrolEast, guardMaterial);

            // 3. Northeast Gatekeeper guarding the squeeze toward Bu Rina
            var patrolNorthEast = UnityEngine.Object.Instantiate(original, root);
            patrolNorthEast.name = "Preman_03_PatroliUtara";
            patrolNorthEast.transform.SetPositionAndRotation(new Vector3(7.5f, 1f, 5.8f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureLevel3Guard(patrolNorthEast,
                new[] { new Vector3(6.2f, 1f, 5.2f), new Vector3(10.5f, 1f, 5.2f), new Vector3(10.5f, 1f, 2.5f) },
                radius: 5.6f, chaseSpeed: 4.2f, patrolSpeed: 1.85f, leash: 6.8f, penalty: 10f, retreatSeconds: 1.4f);
            Editor_Level2_PascaHujan.ApplyGuardMaterials(patrolNorthEast, guardMaterial);

            // 4. Boss Enforcer — Mr. Deddy orbiting Sekat_Chicane_Tengah (4.9 m/s, 7.5m sight, -15s penalty!)
            var deddy = UnityEngine.Object.Instantiate(original, root);
            deddy.name = "Mr_Deddy_Boss";
            deddy.SetActive(false);
            var oldVis = deddy.transform.Find("Preman_Visual");
            if (oldVis != null) UnityEngine.Object.DestroyImmediate(oldVis.gameObject);
            deddy.transform.SetPositionAndRotation(new Vector3(-2.5f, 1f, 2.2f), Quaternion.Euler(0f, 180f, 0f));
            Editor_Level1_VisualLingkungan.AddCharacterVisual(deddy, "MrDeddy_Visual",
                CharacterFolder + "PrologueVillains/Mr_Deddy.fbx", PrologueMatFolder + "Mr_Deddy_Threat.mat", 1.88f, "Mr. Deddy");
            Editor_Level2_PascaHujan.ApplyGuardMaterials(deddy, deddyMat);
            ConfigureLevel3Guard(deddy,
                new[] { new Vector3(-2.5f, 1f, 2.2f), new Vector3(5.6f, 1f, 2.2f), new Vector3(5.6f, 1f, 6.0f), new Vector3(-2.5f, 1f, 6.0f) },
                radius: 7.5f, chaseSpeed: 4.9f, patrolSpeed: 2.15f, leash: 8.2f, penalty: 15f, retreatSeconds: 0.9f);
            deddy.SetActive(true);
        }

        // Tight calibrated Day 3 timer (55 seconds)
        var player = find("Player")?.GetComponent<Mission_PengantaranBantuan>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            so.FindProperty("initialTimeSeconds").floatValue = 55f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureLevel3Guard(GameObject go, Vector3[] patrolRoute, float radius, float chaseSpeed, float patrolSpeed, float leash, float penalty, float retreatSeconds)
    {
        var guard = go.GetComponent<Obstacle_Preman>();
        if (guard == null) return;
        var so = new SerializedObject(guard);
        so.FindProperty("detectionRadius").floatValue = radius;
        so.FindProperty("moveSpeed").floatValue = chaseSpeed;
        so.FindProperty("patrolSpeed").floatValue = patrolSpeed;
        so.FindProperty("turnSpeed").floatValue = 540f;
        so.FindProperty("timePenalty").floatValue = penalty;
        so.FindProperty("contactCooldown").floatValue = 2.4f;
        so.FindProperty("leashDistance").floatValue = leash;
        so.FindProperty("detectFromHome").boolValue = true;
        so.FindProperty("requireLineOfSight").boolValue = true;
        so.FindProperty("retireAfterContacts").intValue = 0;
        so.FindProperty("retreatAfterContactSeconds").floatValue = retreatSeconds;
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

    private static void CreateBlockadeCluster(Transform root, Vector3 pos, Material drumMat, Material timberMat)
    {
        var cluster = new GameObject("Blokade_DrumPeti");
        cluster.transform.SetParent(root, false);
        cluster.transform.position = pos;

        var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drum.name = "Drum_Besi";
        drum.transform.SetParent(cluster.transform, false);
        drum.transform.localPosition = new Vector3(-0.32f, 0.45f, 0f);
        drum.transform.localScale = new Vector3(0.56f, 0.45f, 0.56f);
        UnityEngine.Object.DestroyImmediate(drum.GetComponent<Collider>());
        if (drumMat != null) drum.GetComponent<Renderer>().sharedMaterial = drumMat;

        Editor_Level1_RuteDanRintangan.Box(cluster.transform, "Peti_Blokade", new Vector3(0.32f, 0.35f, 0.05f), new Vector3(0.68f, 0.70f, 0.68f), timberMat);
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
                so.FindProperty("timePenalty").floatValue = 10f;
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
}
#endif
