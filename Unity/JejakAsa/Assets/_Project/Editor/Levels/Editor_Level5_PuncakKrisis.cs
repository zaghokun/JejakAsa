#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class Editor_Level5_PuncakKrisis
{
    private const string ScenePath = "Assets/_Project/Scenes/Kampung_L5.unity";
    private const string CharacterFolder = "Assets/_Project/Art/Characters/";
    private const string LandscapeFolder = "Assets/_Project/Art/Environment/Landscape/Materials/";
    private const string PoskoMatFolder = "Assets/_Project/Art/Environment/PoskoP02/Materials/";
    private const string PrologueMatFolder = "Assets/_Project/Art/Environment/Prologue/";
    private const string Level1PuddlesFolder = "Assets/_Project/Art/Environment/Level1Puddles/";
    private const string Level2PuddlesFolder = "Assets/_Project/Art/Environment/Level2Puddles/";
    private const string Level3PuddlesFolder = "Assets/_Project/Art/Environment/Level3Puddles/";
    private const string Level4PuddlesFolder = "Assets/_Project/Art/Environment/Level4Puddles/";
    private const string Level5PuddlesFolder = "Assets/_Project/Art/Environment/Level5Puddles/";
    private const string RootVisualName = "Level5_Visual_Environment";
    private const string RootChallengeName = "Level5_Challenge_Layout";

    [MenuItem("Jejak Asa/★ TERAPKAN LEVEL 5 SEKARANG ★", false, 40)]
    [MenuItem("Jejak Asa/Level 5/Apply Level 5 Gas Sabotage & Finale", false, 41)]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before applying Level 5 layout.");

        RunOnLevel5Scene(scene =>
        {
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#FF7744><b>[Jejak Asa] Level 5 (Puncak Krisis: Sabotase Tabung Gas & Final Boss Mr. Gas Oke) berhasil diterapkan ke Kampung_L5.unity!</b></color>");
        });
    }

    [MenuItem("Jejak Asa/Level 5/Revert Level 5 to Graybox", false, 42)]
    public static void Revert()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before reverting Level 5.");

        RunOnLevel5Scene(scene =>
        {
            RemoveFromScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=yellow><b>[Jejak Asa] Level 5 dikembalikan ke tampilan Graybox.</b></color>");
        });
    }

    private static void RunOnLevel5Scene(Action<Scene> action)
    {
        Scene scene = SceneManager.GetActiveScene();
        bool opened = false;
        Scene previousActive = scene;

        if (!scene.IsValid() || !scene.isLoaded || scene.name != "Kampung_L5")
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        if (scene.isDirty)
            throw new InvalidOperationException("Save current Kampung_L5 edits before running Level 5 tool.");
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
        Editor_Level2_PascaHujan.EnsureFolderExists(Level5PuddlesFolder);

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
        Editor_Level1_VisualLingkungan.ConfigureHumanoid(CharacterFolder + "PrologueVillains/Mr_Gas_Oke.fbx");

        Editor_Level1_RuteDanRintangan.ConfigureTextures();
        var mud = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_MudBank.mat");
        var water = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Water.mat");
        var ripple = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Ripple.mat");
        var routeTimber = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Timber.mat");
        var routeFence = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Fence.mat");
        var guardMaterial = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Preman_Level1_Daylight.mat");

        var gasBossMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level5PuddlesFolder + "Mr_Gas_Oke_Level5.mat", new Color(0.92f, 0.90f, 0.86f), 0.18f);
        var gasBossTex = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterFolder + "PrologueVillains/Mr_Gas_Oke_BaseColor.jpg");
        if (gasBossTex != null)
        {
            gasBossMat.SetTexture("_BaseMap", gasBossTex);
            gasBossMat.SetTexture("_MainTex", gasBossTex);
        }

        var plankMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level5PuddlesFolder + "Level5_DawnPlank.mat", new Color(0.44f, 0.28f, 0.15f), 0.22f);
        var sandbagMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level2PuddlesFolder + "Level2_Sandbag.mat", new Color(0.68f, 0.60f, 0.44f), 0.10f);
        var warningMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level2PuddlesFolder + "Level2_FloodWarning.mat", new Color(0.84f, 0.44f, 0.12f), 0.25f);
        var drumMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level3PuddlesFolder + "Level3_DrumLogam.mat", new Color(0.62f, 0.24f, 0.16f), 0.35f);
        var tarpBlueMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_TendaTerpalBiru.mat", new Color(0.16f, 0.38f, 0.62f), 0.22f);
        var cableMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_KabelHitam.mat", new Color(0.10f, 0.10f, 0.12f), 0.15f);
        var lpgTankMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level5PuddlesFolder + "Level5_TabungElpiji.mat", new Color(0.14f, 0.56f, 0.28f), 0.45f);
        var electricSafeMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_ListrikAman.mat", new Color(0.14f, 0.20f, 0.26f, 0.45f), 0.4f);
        var electricWarnMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_ListrikPeringatan.mat", new Color(0.95f, 0.68f, 0.12f, 0.90f), 0.6f);
        var electricActiveMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_ListrikAktif.mat", new Color(0.24f, 0.92f, 1.0f, 0.95f), 0.85f);
        var gasSafeMat = Editor_Level4_MalamKritis.CreateEmissiveMaterial(
            Level5PuddlesFolder + "Level5_GasAman.mat", new Color(0.18f, 0.25f, 0.18f, 0.45f), new Color(0.03f, 0.08f, 0.04f), 0.35f);
        var gasWarnMat = Editor_Level4_MalamKritis.CreateEmissiveMaterial(
            Level5PuddlesFolder + "Level5_GasPeringatan.mat", new Color(0.94f, 0.62f, 0.14f, 0.90f), new Color(0.95f, 0.58f, 0.08f) * 1.3f, 0.55f);
        var gasActiveMat = Editor_Level4_MalamKritis.CreateEmissiveMaterial(
            Level5PuddlesFolder + "Level5_GasAktif.mat", new Color(0.30f, 0.92f, 0.38f, 0.92f), new Color(0.24f, 0.88f, 0.32f) * 2.1f, 0.65f);

        var plasterMat0 = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Kampung_Plaster_0.mat");
        var plasterMat1 = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Kampung_Plaster_1.mat");
        var terracottaRoof = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_Terracotta.002.mat");
        var timberMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_Timber.002.mat");
        var canvasMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Posko_Canvas.002.mat");
        var crateTexMat = AssetDatabase.LoadAssetAtPath<Material>(PoskoMatFolder + "Textures.014.mat");
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
        BuildChallengeLayout(challengeRoot.transform, find, mud, water, ripple, routeTimber, routeFence, ironMat, timberMat, plankMat, sandbagMat, warningMat, drumMat, tarpBlueMat, canvasMat, terracottaRoof, crateTexMat ?? timberMat, barkMat ?? routeTimber, deepGreenMat ?? leafMat0, cableMat, lpgTankMat, electricSafeMat, electricWarnMat, electricActiveMat, gasSafeMat, gasWarnMat, gasActiveMat, guardMaterial, gasBossMat, fbxMeshes, fallbackCanopyMesh);
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
                dialogue = "Ini hari terakhir krisis, Nak! Orang-orang Mr. Gas Oke sengaja membocorkan tabung elpiji di gerobak depan biar relawan tidak berani lewat!"
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_02_Graybox", visualName = "PakArif_Visual",
                fbx = CharacterFolder + "PoskoP02/PakArif.fbx", mat = CharacterFolder + "PoskoP02/PakArif.mat", height = 1.72f,
                speaker = "Pak Arif", displayName = "Pak Arif (Warga Tengah)",
                dialogue = "Terima kasih, Nak! Di gerbang timur sana Mr. Gas Oke sendiri yang menjaga tumpukan elpiji bocor dan jembatan banjir menuju rumah Bu Rina. Tetap fokus!"
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_03_Graybox", visualName = "BuRina_Visual",
                fbx = CharacterFolder + "PoskoP02/BuRina.fbx", mat = CharacterFolder + "PoskoP02/BuRina.mat", height = 1.63f,
                speaker = "Bu Rina", displayName = "Bu Rina (Warga Timur)",
                dialogue = "Akhirnya bantuan kelima sampai dan pengepungan Mr. Gas Oke terpatahkan! Terima kasih Vicky dan tim posko, kampung kita pulih lagi!"
            }
        };

        Editor_Level2_PascaHujan.ApplyResidents(find, residents);

        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.SetActive(false);
            preman.transform.SetPositionAndRotation(new Vector3(-7.0f, 1f, 3.0f), Quaternion.Euler(0f, 180f, 0f));
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
            new Vector3(-15.5f, 0f, 2.2f),
            new Vector3(-9.0f, 0f, 6.6f),
            new Vector3(-2.0f, 0f, 4.2f),
            new Vector3(5.8f, 0f, 2.6f),
            new Vector3(10.2f, 0f, -4.6f),
            new Vector3(15.8f, 0f, 7.2f)
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
            new Vector3(-17.8f, 0f, 13.5f),
            new Vector3(-4.5f, 0f, 14.8f),
            new Vector3(8.5f, 0f, 14.8f),
            new Vector3(18.2f, 0f, 13.5f),
            new Vector3(14.5f, 0f, -12.0f),
            new Vector3(-3.5f, 0f, -11.5f)
        };
        for (int i = 0; i < villageTreePositions.Length; i++)
        {
            Editor_Level1_VisualLingkungan.CreateAuthenticTree(
                visualRoot, villageTreePositions[i], 1.0f, i, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        // Dramatic crisp crimson-amber dawn lighting for Day 5 Finale
        var sunLight = find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            sunLight.color = new Color(1.0f, 0.72f, 0.52f);
            sunLight.intensity = 0.94f;
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
        Material plankMat,
        Material sandbagMat,
        Material warningMat,
        Material drumMat,
        Material tarpBlueMat,
        Material canvasMat,
        Material terracottaRoof,
        Material crateMat,
        Material barkMat,
        Material leafMat,
        Material cableMat,
        Material lpgTankMat,
        Material electricSafeMat,
        Material electricWarnMat,
        Material electricActiveMat,
        Material gasSafeMat,
        Material gasWarnMat,
        Material gasActiveMat,
        Material guardMaterial,
        Material gasBossMat,
        Dictionary<string, Mesh> fbxMeshes,
        Mesh fallbackCanopyMesh)
    {
        var legacyWater = find("Genangan_Graybox");
        if (legacyWater != null) legacyWater.SetActive(false);
        var legacyMarker = find("Penanda_Intimidasi_BelumAktif");
        if (legacyMarker != null) legacyMarker.SetActive(false);

        // Approach connectors
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Warga_Barat_L5", new Vector3(-12.0f, 0.022f, 4.8f), new Vector3(3.4f, 0.012f, 3.2f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Warga_Timur_L5", new Vector3(14.0f, 0.022f, 6.8f), new Vector3(3.2f, 0.012f, 2.8f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Masuk", new Vector3(12.6f, 0.022f, -1.6f), new Vector3(2.8f, 0.012f, 3.0f), routeTimber);

        // Maze fences & corrugated barricades channeling players across West Bridge -> Center Electrical -> East Gas & Bridge
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Barat_Selatan", new Vector3(-17.8f, 0f, -1.5f), new Vector3(0.45f, 1.35f, 11.2f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Barat_Utara", new Vector3(-17.2f, 0f, 6.2f), new Vector3(0.45f, 1.35f, 5.2f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Selatan_Tengah", new Vector3(0.0f, 0f, -6.2f), new Vector3(18.6f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sekat_Vicky_Timur", new Vector3(14.2f, 0f, -6.2f), new Vector3(6.6f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Timur_Luar", new Vector3(16.8f, 0f, 2.5f), new Vector3(0.45f, 1.35f, 17.5f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Barat", new Vector3(-7.5f, 0f, 11.4f), new Vector3(7.0f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Tengah", new Vector3(8.0f, 0f, 11.4f), new Vector3(6.5f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_BaratAtas_Labirin", new Vector3(-7.6f, 0f, 8.2f), new Vector3(0.45f, 1.35f, 4.0f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Chicane_Timur", new Vector3(9.2f, 0f, 3.6f), new Vector3(0.45f, 1.35f, 4.4f), routeTimber, ironMat ?? routeFence);

        // 1. Disaster & Hoarding Shelters (authentic props, ZERO gray boxes)
        Editor_Level4_MalamKritis.BuildDisasterTent(root, "Tenda_Penimbunan_Barat", new Vector3(-14.8f, 0f, -1.8f), new Vector3(2.0f, 1.6f, 1.6f), timberMat, tarpBlueMat, canvasMat, sandbagMat, crateMat, warningMat, fbxMeshes);
        Editor_Level4_MalamKritis.BuildLogisticsPost(root, "Pos_Logistik_Tengah", new Vector3(-3.6f, 0f, 2.4f), new Vector3(2.0f, 1.4f, 1.4f), timberMat, terracottaRoof ?? warningMat, drumMat, sandbagMat, crateMat, warningMat, fbxMeshes);
        Editor_Level4_MalamKritis.BuildDisasterTent(root, "Pos_Timbunan_Timur", new Vector3(7.8f, 0f, 7.4f), new Vector3(2.0f, 1.5f, 1.5f), timberMat, terracottaRoof ?? tarpBlueMat, canvasMat, sandbagMat, crateMat, warningMat, fbxMeshes);

        // 2. Two Storm Fallen Trees & Two Drum-Crate Chicanes
        Editor_Level4_MalamKritis.BuildFallenTree(root, "Pohon_Tumbang_Barat", new Vector3(-15.2f, 0f, 5.2f), yawDegrees: 18f, barkMat, timberMat, leafMat, fbxMeshes, fallbackCanopyMesh);
        Editor_Level4_MalamKritis.BuildFallenTree(root, "Pohon_Tumbang_Timur", new Vector3(11.2f, 0f, 7.2f), yawDegrees: -158f, barkMat, timberMat, leafMat, fbxMeshes, fallbackCanopyMesh);

        Editor_Level4_MalamKritis.BuildChicaneCluster(root, "Chicane_Selatan_Barat", new Vector3(-10.2f, 0f, -6.0f), new Vector3(1.3f, 1.0f, 0.8f), drumMat, timberMat, warningMat, sandbagMat, crateMat, fbxMeshes);
        Editor_Level4_MalamKritis.BuildChicaneCluster(root, "Chicane_Selatan_Timur", new Vector3(4.5f, 0f, -4.6f), new Vector3(1.3f, 1.0f, 0.8f), drumMat, timberMat, warningMat, sandbagMat, crateMat, fbxMeshes);

        // 3. Two Non-Trigger Flood Beds under West Bridge & East L-Bridge
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_BaratBanjir", new Vector3(-12.0f, 0f, 1.5f), 5.4f, 3.6f, 1.0f, 13, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: false, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_AlasTimur", new Vector3(13.2f, 0f, 2.2f), 5.4f, 4.8f, 1.0f, 19, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: false, addWater: true);

        // 4. Seven Interactive Mud Puddles (0.36x planks' flanks + 0.42x alley traps)
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Barat_Kiri", new Vector3(-13.8f, 0f, 1.5f), 2.2f, 3.6f, 0.36f, 23, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Barat_Kanan", new Vector3(-10.2f, 0f, 1.5f), 2.2f, 3.6f, 0.36f, 29, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Selatan_Barat", new Vector3(-5.2f, 0f, -4.0f), 3.0f, 1.6f, 0.42f, 37, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Utama_Utara", new Vector3(-1.5f, 0f, 4.2f), 2.4f, 1.6f, 0.42f, 43, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Timur_Dalam", new Vector3(10.8f, 0f, 2.0f), 2.2f, 3.4f, 0.36f, 53, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Timur_Kanan", new Vector3(15.6f, 0f, 2.2f), 1.8f, 3.8f, 0.36f, 61, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L5_Timur_Atas", new Vector3(11.8f, 0f, 5.2f), 2.4f, 1.4f, 0.36f, 71, mud, water, ripple, Level5PuddlesFolder, interactiveTrigger: true, addWater: true);

        // 5. Two Narrow 0.95m Timber Plank Bridges
        BuildWestFloodBridge(root, plankMat, timberMat, sandbagMat);
        BuildEastMazeLBridge(root, plankMat, timberMat, sandbagMat);

        // 6. Signature Day 5 LPG Gas Leak Hazards (2) + 1 Electric Storm Wire Hazard in center alley
        BuildGasLeakHazard(root, "KebocoranGas_Barat", new Vector3(-12.0f, 0f, -2.0f), cartOffset: new Vector3(-1.35f, 0f, 0f), phaseOffset: 0.0f,
            timberMat, ironMat ?? drumMat, lpgTankMat, warningMat, gasSafeMat, gasWarnMat, gasActiveMat);
        BuildGasLeakHazard(root, "KebocoranGas_Timur", new Vector3(9.8f, 0f, -2.4f), cartOffset: new Vector3(0f, 0f, -1.35f), phaseOffset: 2.0f,
            timberMat, ironMat ?? drumMat, lpgTankMat, warningMat, gasSafeMat, gasWarnMat, gasActiveMat);

        Editor_Level4_MalamKritis.BuildElectricShortHazard(root, "Korslet_Tengah", new Vector3(0.0f, 0f, -0.4f), poleOffset: new Vector3(-1.35f, 0f, 0f), phaseOffset: 1.0f,
            timberMat, warningMat, cableMat, electricSafeMat, electricWarnMat, electricActiveMat, Level5PuddlesFolder);

        // 7. 2 Patrol Henchmen + Final Boss Mr. Gas Oke
        var original = find("Preman_Graybox");
        if (original != null)
        {
            original.SetActive(false);
            original.transform.SetPositionAndRotation(new Vector3(-7.0f, 1f, 3.0f), Quaternion.Euler(0f, 180f, 0f));
            Editor_Level4_MalamKritis.ConfigureGuard(original,
                new[] { new Vector3(-8.2f, 1f, -0.8f), new Vector3(-8.2f, 1f, 3.2f), new Vector3(-5.2f, 1f, 3.2f) },
                radius: 5.6f, chaseSpeed: 4.2f, patrolSpeed: 1.90f, leash: 7.2f, penalty: 10f, retreatSeconds: 1.2f);
            original.SetActive(true);

            var patrolEast = UnityEngine.Object.Instantiate(original, root);
            patrolEast.name = "Preman_02_PenjagaTimur";
            patrolEast.transform.SetPositionAndRotation(new Vector3(11.0f, 1f, 5.4f), Quaternion.Euler(0f, 180f, 0f));
            Editor_Level4_MalamKritis.ConfigureGuard(patrolEast,
                new[] { new Vector3(8.4f, 1f, 5.2f), new Vector3(13.6f, 1f, 6.2f), new Vector3(10.5f, 1f, 2.2f) },
                radius: 5.8f, chaseSpeed: 4.3f, patrolSpeed: 1.95f, leash: 7.5f, penalty: 10f, retreatSeconds: 1.2f);
            Editor_Level2_PascaHujan.ApplyGuardMaterials(patrolEast, guardMaterial);

            var boss = UnityEngine.Object.Instantiate(original, root);
            boss.name = "Mr_Gas_Oke_Boss";
            boss.SetActive(false);
            var oldVis = boss.transform.Find("Preman_Visual");
            if (oldVis != null) UnityEngine.Object.DestroyImmediate(oldVis.gameObject);
            boss.transform.SetPositionAndRotation(new Vector3(11.0f, 1f, -3.5f), Quaternion.Euler(0f, 270f, 0f));
            Editor_Level1_VisualLingkungan.AddCharacterVisual(boss, "MrGasOke_Visual",
                CharacterFolder + "PrologueVillains/Mr_Gas_Oke.fbx", PrologueMatFolder + "Mr_Gas_Oke_Threat.mat", 1.90f, "Mr. Gas Oke");
            Editor_Level2_PascaHujan.ApplyGuardMaterials(boss, gasBossMat);
            Editor_Level4_MalamKritis.ConfigureGuard(boss,
                new[] { new Vector3(6.2f, 1f, -3.6f), new Vector3(12.2f, 1f, -3.6f), new Vector3(12.2f, 1f, 0.2f) },
                radius: 7.0f, chaseSpeed: 4.8f, patrolSpeed: 2.10f, leash: 8.2f, penalty: 15f, retreatSeconds: 1.0f);
            boss.SetActive(true);
        }

        // Day 5 intense climax timer (60 seconds across maze + 2 bridges + 3 timed hazards + Mr. Gas Oke)
        var player = find("Player")?.GetComponent<Mission_PengantaranBantuan>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            so.FindProperty("initialTimeSeconds").floatValue = 60f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void BuildWestFloodBridge(Transform root, Material plankMat, Material timberMat, Material sandbagMat)
    {
        var bridge = new GameObject("Jembatan_Darurat_Barat");
        bridge.transform.SetParent(root, false);

        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_L", new Vector3(-12.34f, 0.052f, 1.5f), new Vector3(0.11f, 0.048f, 3.8f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_R", new Vector3(-11.66f, 0.052f, 1.5f), new Vector3(0.11f, 0.048f, 3.8f), timberMat);
        for (int i = 0; i < 9; i++)
        {
            float z = -0.35f + i * 0.46f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_Barat_{i:D2}", new Vector3(-12.0f, 0.072f, z), new Vector3(0.95f, 0.032f, 0.36f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(-12.75f, 0f, -0.55f), sandbagMat);
        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(-11.25f, 0f, -0.55f), sandbagMat);
        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(-12.75f, 0f, 3.65f), sandbagMat);
        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(-11.25f, 0f, 3.65f), sandbagMat);
    }

    private static void BuildEastMazeLBridge(Transform root, Material plankMat, Material timberMat, Material sandbagMat)
    {
        var bridge = new GameObject("Jembatan_Sempit_Timur");
        bridge.transform.SetParent(root, false);

        // Long South-North plank runway at x = 12.6, z: 0.2 .. 4.1 (0.95m narrow width)
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN1_L", new Vector3(12.26f, 0.052f, 2.15f), new Vector3(0.11f, 0.048f, 4.0f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN1_R", new Vector3(12.94f, 0.052f, 2.15f), new Vector3(0.11f, 0.048f, 4.0f), timberMat);
        for (int i = 0; i < 9; i++)
        {
            float z = 0.25f + i * 0.46f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_SN1_{i:D2}", new Vector3(12.6f, 0.072f, z), new Vector3(0.95f, 0.032f, 0.36f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        // East elbow plank span at z = 4.0, x: 12.4 .. 14.3
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_B", new Vector3(13.4f, 0.052f, 3.66f), new Vector3(2.2f, 0.048f, 0.11f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_T", new Vector3(13.4f, 0.052f, 4.34f), new Vector3(2.2f, 0.048f, 0.11f), timberMat);
        for (int i = 0; i < 5; i++)
        {
            float x = 12.55f + i * 0.43f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_EW_{i:D2}", new Vector3(x, 0.072f, 4.0f), new Vector3(0.35f, 0.032f, 0.95f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        // Final exit runway at x = 14.2, z: 3.9 .. 5.8 onto dry Bu Rina paving
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN2_L", new Vector3(13.86f, 0.052f, 4.85f), new Vector3(0.11f, 0.048f, 2.0f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN2_R", new Vector3(14.54f, 0.052f, 4.85f), new Vector3(0.11f, 0.048f, 2.0f), timberMat);
        for (int i = 0; i < 5; i++)
        {
            float z = 3.95f + i * 0.44f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_SN2_{i:D2}", new Vector3(14.2f, 0.072f, z), new Vector3(0.95f, 0.032f, 0.35f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(11.85f, 0f, -0.05f), sandbagMat);
        Editor_Level4_MalamKritis.CreateSandbagStack(bridge.transform, new Vector3(13.35f, 0f, -0.05f), sandbagMat);
    }

    private static void BuildGasLeakHazard(
        Transform root,
        string name,
        Vector3 center,
        Vector3 cartOffset,
        float phaseOffset,
        Material woodMat,
        Material steelMat,
        Material tankMat,
        Material warningMat,
        Material safeMat,
        Material warnMat,
        Material activeMat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = center;

        // Hoarder handcart loaded with 3 leaking green LPG gas cylinders next to walkway
        var cart = new GameObject("Gerobak_TabungGas");
        cart.transform.SetParent(go.transform, false);
        cart.transform.localPosition = cartOffset;
        Vector3 faceDir = -cartOffset.normalized;
        cart.transform.localRotation = Quaternion.LookRotation(faceDir);

        Editor_Level1_RuteDanRintangan.Box(cart.transform, "Rangka_Gerobak", new Vector3(0f, 0.24f, 0f), new Vector3(1.1f, 0.10f, 0.72f), steelMat);
        Editor_Level1_RuteDanRintangan.Box(cart.transform, "Pegangan_Kayu", new Vector3(0f, 0.62f, -0.38f), new Vector3(1.05f, 0.09f, 0.09f), woodMat);
        Editor_Level1_RuteDanRintangan.Box(cart.transform, "Rambu_BahayaGas", new Vector3(0f, 0.78f, 0.38f), new Vector3(0.96f, 0.16f, 0.05f), warningMat);

        foreach (float x in new[] { -0.32f, 0f, 0.32f })
        {
            var tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tank.name = "Tabung_Elpiji";
            tank.transform.SetParent(cart.transform, false);
            tank.transform.localPosition = new Vector3(x, 0.56f, 0.04f);
            tank.transform.localScale = new Vector3(0.28f, 0.30f, 0.28f);
            UnityEngine.Object.DestroyImmediate(tank.GetComponent<Collider>());
            tank.GetComponent<Renderer>().sharedMaterial = tankMat;

            Editor_Level1_RuteDanRintangan.Box(cart.transform, "Katup_Gas", new Vector3(x, 0.90f, 0.04f), new Vector3(0.14f, 0.08f, 0.14f), warningMat);
        }

        // Flat hazard ring on the road
        var ringGo = new GameObject("Cincin_Gas");
        ringGo.transform.SetParent(go.transform, false);
        ringGo.transform.localPosition = new Vector3(0f, 0.052f, 0f);
        var ringMesh = Editor_Level4_MalamKritis.CreateHazardRingMesh(name + "_RingMesh", 1.18f, 1.45f, 36, Level5PuddlesFolder);
        ringGo.AddComponent<MeshFilter>().sharedMesh = ringMesh;
        var ringRenderer = ringGo.AddComponent<MeshRenderer>();
        ringRenderer.sharedMaterial = safeMat;
        ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Billowing green gas plume during Active phase
        var plume = new GameObject("Awan_SemburanGas");
        plume.transform.SetParent(go.transform, false);
        plume.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        for (int i = 0; i < 4; i++)
        {
            float a = i * Mathf.PI * 0.5f;
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = $"Kabut_{i}";
            puff.transform.SetParent(plume.transform, false);
            puff.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.42f, (i % 2) * 0.16f, Mathf.Sin(a) * 0.42f);
            puff.transform.localScale = Vector3.one * 0.58f;
            UnityEngine.Object.DestroyImmediate(puff.GetComponent<Collider>());
            var pr = puff.GetComponent<Renderer>();
            pr.sharedMaterial = activeMat;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        plume.SetActive(false);

        var lightGo = new GameObject("Lampu_Gas");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 4.5f;
        light.enabled = false;

        var hazard = go.AddComponent<Obstacle_KebocoranGas>();
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardRadius").floatValue = 1.45f;
        so.FindProperty("warningRadius").floatValue = 4.2f;
        so.FindProperty("safeDuration").floatValue = 2.0f;
        so.FindProperty("warningDuration").floatValue = 0.85f;
        so.FindProperty("activeDuration").floatValue = 1.40f;
        so.FindProperty("phaseOffset").floatValue = phaseOffset;
        so.FindProperty("timePenalty").floatValue = 4f;
        so.FindProperty("shockSlowMultiplier").floatValue = 0.30f;
        so.FindProperty("shockSlowDuration").floatValue = 1.0f;
        so.FindProperty("shockCooldown").floatValue = 1.7f;
        so.FindProperty("pushDistance").floatValue = 0.55f;
        so.FindProperty("ringRenderer").objectReferenceValue = ringRenderer;
        so.FindProperty("sparkVisual").objectReferenceValue = plume;
        so.FindProperty("pulseLight").objectReferenceValue = light;
        so.FindProperty("safeMaterial").objectReferenceValue = safeMat;
        so.FindProperty("warningMaterial").objectReferenceValue = warnMat;
        so.FindProperty("activeMaterial").objectReferenceValue = activeMat;
        so.ApplyModifiedPropertiesWithoutUndo();
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
