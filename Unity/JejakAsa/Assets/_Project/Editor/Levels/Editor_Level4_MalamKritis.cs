#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using JejakAsa;

internal static class Editor_Level4_MalamKritis
{
    private const string ScenePath = "Assets/_Project/Scenes/Kampung_L4.unity";
    private const string CharacterFolder = "Assets/_Project/Art/Characters/";
    private const string LandscapeFolder = "Assets/_Project/Art/Environment/Landscape/Materials/";
    private const string PoskoMatFolder = "Assets/_Project/Art/Environment/PoskoP02/Materials/";
    private const string PrologueMatFolder = "Assets/_Project/Art/Environment/Prologue/";
    private const string Level1PuddlesFolder = "Assets/_Project/Art/Environment/Level1Puddles/";
    private const string Level2PuddlesFolder = "Assets/_Project/Art/Environment/Level2Puddles/";
    private const string Level3PuddlesFolder = "Assets/_Project/Art/Environment/Level3Puddles/";
    private const string Level4PuddlesFolder = "Assets/_Project/Art/Environment/Level4Puddles/";
    private const string RootVisualName = "Level4_Visual_Environment";
    private const string RootChallengeName = "Level4_Challenge_Layout";

    [MenuItem("Jejak Asa/★ TERAPKAN LEVEL 4 SEKARANG ★", false, 30)]
    [MenuItem("Jejak Asa/Level 4/Apply Level 4 Night Flood, Electric Surge & Bridges", false, 31)]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before applying Level 4 layout.");

        RunOnLevel4Scene(scene =>
        {
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#66AAFF><b>[Jejak Asa] Level 4 (Malam Kritis: Banjir Luapan, Kabel Korslet, Pohon Tumbang & 2 Jembatan Papan) berhasil diterapkan ke Kampung_L4.unity!</b></color>");
        });
    }

    [MenuItem("Jejak Asa/Level 4/Revert Level 4 to Graybox", false, 32)]
    public static void Revert()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before reverting Level 4.");

        RunOnLevel4Scene(scene =>
        {
            RemoveFromScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=yellow><b>[Jejak Asa] Level 4 dikembalikan ke tampilan Graybox.</b></color>");
        });
    }

    private static void RunOnLevel4Scene(Action<Scene> action)
    {
        Scene scene = SceneManager.GetActiveScene();
        bool opened = false;
        Scene previousActive = scene;

        if (!scene.IsValid() || !scene.isLoaded || scene.name != "Kampung_L4")
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                opened = true;
            }
        }

        if (scene.isDirty)
            throw new InvalidOperationException("Save current Kampung_L4 edits before running Level 4 tool.");
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
        Editor_Level2_PascaHujan.EnsureFolderExists(Level4PuddlesFolder);

        foreach (var old in scene.GetRootGameObjects()
            .Where(o => o.name == RootVisualName || o.name == RootChallengeName)
            .ToArray())
        {
            UnityEngine.Object.DestroyImmediate(old);
        }

        // Remove legacy graybox / orphaned puddle mesh assets from earlier revisions
        foreach (string obsolete in new[]
        {
            Level4PuddlesFolder + "Level4_BlindSpotSteel.mat",
            Level4PuddlesFolder + "Genangan_L4_Utara_Tengah_Lumpur.asset",
            Level4PuddlesFolder + "Genangan_L4_Utara_Tengah_Air.asset",
            Level4PuddlesFolder + "Genangan_L4_Utara_Tengah_Riak0.asset",
            Level4PuddlesFolder + "Genangan_L4_Utara_Tengah_Riak1.asset"
        })
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(obsolete) != null)
                AssetDatabase.DeleteAsset(obsolete);
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
        var mud = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_MudBank.mat");
        var water = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Water.mat");
        var ripple = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Puddle_Ripple.mat");
        var routeTimber = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Timber.mat");
        var routeFence = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Route_Fence.mat");
        var guardMaterial = Editor_Level2_PascaHujan.LoadRequiredMaterial(Level1PuddlesFolder + "Preman_Level1_Daylight.mat");

        var plankMat = Editor_Level2_PascaHujan.GetOrCreateMaterial(
            Level4PuddlesFolder + "Level4_NightPlank.mat", new Color(0.45f, 0.30f, 0.16f), 0.24f);
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
        var electricSafeMat = CreateEmissiveMaterial(
            Level4PuddlesFolder + "Level4_ListrikAman.mat", new Color(0.14f, 0.20f, 0.26f, 0.45f), new Color(0.03f, 0.06f, 0.10f), 0.4f);
        var electricWarnMat = CreateEmissiveMaterial(
            Level4PuddlesFolder + "Level4_ListrikPeringatan.mat", new Color(0.95f, 0.68f, 0.12f, 0.90f), new Color(1.0f, 0.64f, 0.08f) * 1.4f, 0.6f);
        var electricActiveMat = CreateEmissiveMaterial(
            Level4PuddlesFolder + "Level4_ListrikAktif.mat", new Color(0.24f, 0.92f, 1.0f, 0.95f), new Color(0.22f, 0.88f, 1.0f) * 2.6f, 0.85f);

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
        BuildChallengeLayout(challengeRoot.transform, find, mud, water, ripple, routeTimber, routeFence, ironMat, timberMat, concreteMat, plankMat, sandbagMat, warningMat, drumMat, tarpBlueMat, canvasMat, terracottaRoof, crateTexMat ?? timberMat, barkMat ?? routeTimber, deepGreenMat ?? leafMat0, cableMat, electricSafeMat, electricWarnMat, electricActiveMat, guardMaterial, fbxMeshes, fallbackCanopyMesh);
    }

    internal static Material CreateEmissiveMaterial(string path, Color baseColor, Color emissionColor, float smoothness)
    {
        var mat = Editor_Level2_PascaHujan.GetOrCreateMaterial(path, baseColor, smoothness);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor);
        EditorUtility.SetDirty(mat);
        return mat;
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
                dialogue = "Malam ini air bah meluap lagi ke gang utama, Nak! Hati-hati ada tiang kabel listrik korslet nyala-mati di tengah genangan dan pohon tumbang di depan rumah."
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_02_Graybox", visualName = "PakArif_Visual",
                fbx = CharacterFolder + "PoskoP02/PakArif.fbx", mat = CharacterFolder + "PoskoP02/PakArif.mat", height = 1.72f,
                speaker = "Pak Arif", displayName = "Pak Arif (Warga Tengah)",
                dialogue = "Terima kasih, Nak! Lewat papan titian tadi aman kan? Di sebelah timur masih ada kabel korslet di genangan dan jembatan kayu sempit menuju rumah Bu Rina!"
            },
            new Editor_Level2_PascaHujan.ResidentSpec
            {
                objectName = "Warga_03_Graybox", visualName = "BuRina_Visual",
                fbx = CharacterFolder + "PoskoP02/BuRina.fbx", mat = CharacterFolder + "PoskoP02/BuRina.mat", height = 1.63f,
                speaker = "Bu Rina", displayName = "Bu Rina (Warga Timur)",
                dialogue = "Alhamdulillah kamu selamat melewati banjir, kabel listrik putus, dan jembatan sempit malam ini! Bantuan darurat ini sangat berarti buat keluarga kami."
            }
        };

        Editor_Level2_PascaHujan.ApplyResidents(find, residents);

        var preman = find("Preman_Graybox");
        if (preman != null)
        {
            preman.SetActive(false);
            preman.transform.SetPositionAndRotation(new Vector3(-11.0f, 1f, -3.5f), Quaternion.Euler(0f, 90f, 0f));
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

        // Warm night street lamps positioned neatly along sidewalks without clipping obstacles
        foreach (Vector3 lampPos in new[]
        {
            new Vector3(-16.6f, 0f, -2.2f),
            new Vector3(-9.5f, 0f, 8.8f),
            new Vector3(-4.2f, 0f, 8.8f),
            new Vector3(3.6f, 0f, 9.0f),
            new Vector3(10.4f, 0f, -4.6f),
            new Vector3(15.6f, 0f, 6.4f)
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
            new Vector3(-16.5f, 0f, 10.5f),
            new Vector3(-6.8f, 0f, 14.8f),
            new Vector3(4.5f, 0f, 14.8f),
            new Vector3(17.8f, 0f, 13.5f),
            new Vector3(14.5f, 0f, -12.0f),
            new Vector3(-3.5f, 0f, -11.5f)
        };
        for (int i = 0; i < villageTreePositions.Length; i++)
        {
            Editor_Level1_VisualLingkungan.CreateAuthenticTree(
                visualRoot, villageTreePositions[i], 1.0f, i, fbxMeshes, terracottaRoof, timberMat, deepGreenMat, fallbackCanopyMesh);
        }

        // Cool blue moonlight night atmosphere paired with warm street & tent lanterns
        var sunLight = find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            sunLight.color = new Color(0.50f, 0.62f, 0.88f);
            sunLight.intensity = 0.70f;
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
        Material concreteMat,
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
        Material electricSafeMat,
        Material electricWarnMat,
        Material electricActiveMat,
        Material guardMaterial,
        Dictionary<string, Mesh> fbxMeshes,
        Mesh fallbackCanopyMesh)
    {
        var legacyWater = find("Genangan_Graybox");
        if (legacyWater != null) legacyWater.SetActive(false);
        var legacyMarker = find("Penanda_Intimidasi_BelumAktif");
        if (legacyMarker != null) legacyMarker.SetActive(false);

        // Approach paving connectors
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Warga_Barat", new Vector3(-15.0f, 0.022f, -1.8f), new Vector3(3.0f, 0.012f, 4.8f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Selatan", new Vector3(14.2f, 0.022f, -1.8f), new Vector3(2.8f, 0.012f, 4.6f), routeTimber);
        Editor_Level1_RuteDanRintangan.Road(root, "Paving_Jalur_Timur_Atas", new Vector3(13.2f, 0.022f, 5.9f), new Vector3(3.6f, 0.012f, 2.4f), routeTimber);

        // Perimeter & flood gate fences channeling players through authentic disaster zones
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Barat_Selatan", new Vector3(-17.8f, 0f, -1.5f), new Vector3(0.45f, 1.35f, 8.2f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Barat_Luar", new Vector3(-19.0f, 0f, 5.0f), new Vector3(2.2f, 1.35f, 0.40f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Selatan_Tengah", new Vector3(0.0f, 0f, -6.2f), new Vector3(18.6f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sekat_Vicky_Timur", new Vector3(14.2f, 0f, -6.2f), new Vector3(6.6f, 1.35f, 0.45f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Tutup_Timur_Luar", new Vector3(16.8f, 0f, 2.5f), new Vector3(0.45f, 1.35f, 17.5f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Pagar_Sisi_Timur", new Vector3(18.5f, 0f, 11.0f), new Vector3(3.4f, 1.35f, 0.40f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Barat", new Vector3(-8.5f, 0f, 11.4f), new Vector3(13.2f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Blokade_Utara_Timur", new Vector3(5.0f, 0f, 11.4f), new Vector3(8.4f, 1.35f, 0.45f), routeTimber, ironMat ?? routeFence);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Banjir_Tengah_Blokir", new Vector3(0.0f, 0f, 0.0f), new Vector3(3.2f, 1.35f, 0.50f), routeTimber, routeFence);
        Editor_Level1_RuteDanRintangan.Box(root, "Palang_Banjir_Tengah_S", new Vector3(0f, 0.92f, -0.28f), new Vector3(2.9f, 0.22f, 0.06f), warningMat);
        Editor_Level1_RuteDanRintangan.Box(root, "Palang_Banjir_Tengah_U", new Vector3(0f, 0.92f, 0.28f), new Vector3(2.9f, 0.22f, 0.06f), warningMat);
        Editor_Level1_RuteDanRintangan.Fence(root, "Sekat_Bawah_Timur", new Vector3(9.5f, 0f, -0.8f), new Vector3(0.45f, 1.35f, 3.2f), routeTimber, ironMat ?? routeFence);

        // 1. Authentic Disaster Relief Tents & Sandbag Logistics Posts (replaces all gray boxes!)
        BuildDisasterTent(root, "Tenda_Darurat_Barat", new Vector3(-10.8f, 0f, 3.0f), new Vector3(2.0f, 1.8f, 1.6f), timberMat, tarpBlueMat, canvasMat, sandbagMat, crateMat, warningMat, fbxMeshes);
        BuildLogisticsPost(root, "Pos_Logistik_Tengah", new Vector3(-2.2f, 0f, 3.0f), new Vector3(2.0f, 1.4f, 1.4f), timberMat, terracottaRoof ?? warningMat, drumMat, sandbagMat, crateMat, warningMat, fbxMeshes);
        BuildDisasterTent(root, "Pos_Logistik_Timur", new Vector3(6.8f, 0f, 3.6f), new Vector3(2.0f, 1.6f, 1.5f), timberMat, terracottaRoof ?? tarpBlueMat, canvasMat, sandbagMat, crateMat, warningMat, fbxMeshes);

        // 2. Fallen Storm Trees across road shoulders forcing tactical weaving
        BuildFallenTree(root, "Pohon_Tumbang_BaratUtara", new Vector3(-4.8f, 0f, 3.8f), yawDegrees: 14f, barkMat, timberMat, leafMat, fbxMeshes, fallbackCanopyMesh);
        BuildFallenTree(root, "Pohon_Tumbang_Utara", new Vector3(1.5f, 0f, 8.1f), yawDegrees: -165f, barkMat, timberMat, leafMat, fbxMeshes, fallbackCanopyMesh);

        // 3. Physical drum + crate + warning bar chicanes at key bottlenecks
        BuildChicaneCluster(root, "Chicane_Gang_Awal", new Vector3(-10.3f, 0f, -5.8f), new Vector3(1.3f, 1.0f, 0.8f), drumMat, timberMat, warningMat, sandbagMat, crateMat, fbxMeshes);
        BuildChicaneCluster(root, "Chicane_Simpang_Timur", new Vector3(8.6f, 0f, 1.8f), new Vector3(1.2f, 1.0f, 0.9f), drumMat, timberMat, warningMat, sandbagMat, crateMat, fbxMeshes);

        // 4. Two Non-Trigger Flood Beds under the two narrow 0.95m wooden plank bridges
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_AlasUtara", new Vector3(-8.1f, 0f, 6.7f), 5.8f, 3.8f, 1.0f, 11, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: false, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_AlasJembatanTimur", new Vector3(13.8f, 0f, 2.5f), 5.4f, 5.2f, 1.0f, 17, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: false, addWater: true);

        // 5. Eight Interactive Night Mud Puddles (flanking both narrow bridges + alley corners + under shorted wires)
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Barat_Gang", new Vector3(-12.6f, 0f, -0.4f), 3.0f, 2.8f, 0.42f, 23, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_KorsletBarat", new Vector3(-7.4f, 0f, 1.6f), 3.0f, 2.8f, 0.45f, 31, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Utara_Kiri", new Vector3(-9.8f, 0f, 6.7f), 2.8f, 3.4f, 0.38f, 37, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Utara_Atas", new Vector3(-6.2f, 0f, 8.3f), 3.0f, 1.5f, 0.38f, 41, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_KorsletTimur", new Vector3(4.8f, 0f, 6.8f), 3.0f, 2.6f, 0.45f, 47, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Timur_Kanan", new Vector3(15.7f, 0f, 1.9f), 1.6f, 3.8f, 0.38f, 53, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Timur_Dalam", new Vector3(12.1f, 0f, 1.6f), 2.6f, 2.6f, 0.38f, 59, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);
        Editor_Level1_RuteDanRintangan.Puddle(root, "Genangan_L4_Timur_Atas", new Vector3(11.6f, 0f, 4.8f), 2.4f, 1.2f, 0.38f, 67, mud, water, ripple, Level4PuddlesFolder, interactiveTrigger: true, addWater: true);

        // 6. Two Narrow 0.95m Timber Bridges + Two Timed Electric Shorted-Wire Hazards
        BuildNorthSpillwayBridge(root, plankMat, timberMat, sandbagMat);
        BuildNarrowLBridge(root, plankMat, timberMat, sandbagMat);

        BuildElectricShortHazard(root, "Korslet_Utara_Barat", new Vector3(-7.4f, 0f, 1.6f), poleOffset: new Vector3(-1.35f, 0f, 0f), phaseOffset: 0.0f,
            timberMat, warningMat, cableMat, electricSafeMat, electricWarnMat, electricActiveMat);
        BuildElectricShortHazard(root, "Korslet_Utara_Timur", new Vector3(4.8f, 0f, 6.8f), poleOffset: new Vector3(0f, 0f, 1.55f), phaseOffset: 2.15f,
            timberMat, warningMat, cableMat, electricSafeMat, electricWarnMat, electricActiveMat);

        // Sandbag reinforcements around Flood Gate Tengah
        CreateSandbagStack(root, new Vector3(-1.1f, 0f, -0.55f), sandbagMat);
        CreateSandbagStack(root, new Vector3(1.1f, 0f, -0.55f), sandbagMat);
        CreateSandbagStack(root, new Vector3(-1.1f, 0f, 0.55f), sandbagMat);
        CreateSandbagStack(root, new Vector3(1.1f, 0f, 0.55f), sandbagMat);

        // 7. ONLY 2 Night Patrol Guards (supporting tension rather than "preman doang")
        var original = find("Preman_Graybox");
        if (original != null)
        {
            original.SetActive(false);
            original.transform.SetPositionAndRotation(new Vector3(-11.0f, 1f, -3.5f), Quaternion.Euler(0f, 90f, 0f));
            ConfigureGuard(original,
                new[] { new Vector3(-11.0f, 1f, -3.5f), new Vector3(-2.5f, 1f, -3.5f), new Vector3(-2.5f, 1f, -1.8f) },
                radius: 5.5f, chaseSpeed: 4.2f, patrolSpeed: 1.85f, leash: 7.5f, penalty: 10f, retreatSeconds: 1.3f);
            original.SetActive(true);

            var patrolEast = UnityEngine.Object.Instantiate(original, root);
            patrolEast.name = "Preman_02_JagaJembatanTimur";
            patrolEast.transform.SetPositionAndRotation(new Vector3(11.8f, 1f, -2.5f), Quaternion.Euler(0f, 180f, 0f));
            ConfigureGuard(patrolEast,
                new[] { new Vector3(11.0f, 1f, -3.2f), new Vector3(14.5f, 1f, -1.8f), new Vector3(11.2f, 1f, 1.8f) },
                radius: 6.0f, chaseSpeed: 4.5f, patrolSpeed: 1.95f, leash: 7.8f, penalty: 12f, retreatSeconds: 1.1f);
            Editor_Level2_PascaHujan.ApplyGuardMaterials(patrolEast, guardMaterial);
        }

        // Calibrated Day 4 emergency timer (55 seconds for multi-hazard traversal & timing)
        var player = find("Player")?.GetComponent<Mission_PengantaranBantuan>();
        if (player != null)
        {
            var so = new SerializedObject(player);
            so.FindProperty("initialTimeSeconds").floatValue = 55f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    internal static void ConfigureGuard(GameObject go, Vector3[] patrolRoute, float radius, float chaseSpeed, float patrolSpeed, float leash, float penalty, float retreatSeconds)
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

    internal static void BuildDisasterTent(
        Transform root,
        string name,
        Vector3 pos,
        Vector3 colliderSize,
        Material timberMat,
        Material tarpMat,
        Material canvasMat,
        Material sandbagMat,
        Material crateMat,
        Material warningMat,
        Dictionary<string, Mesh> fbxMeshes)
    {
        var tent = new GameObject(name);
        tent.transform.SetParent(root, false);
        tent.transform.position = pos;

        var col = tent.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, colliderSize.y * 0.5f, 0f);
        col.size = colliderSize;

        float hx = colliderSize.x * 0.42f;
        float hz = colliderSize.z * 0.40f;
        foreach (float px in new[] { -hx, hx })
            foreach (float pz in new[] { -hz, hz })
                Editor_Level1_RuteDanRintangan.Box(tent.transform, "Tiang_Tenda", new Vector3(px, 0.95f, pz), new Vector3(0.11f, 1.9f, 0.11f), timberMat);

        var tarp = Editor_Level1_RuteDanRintangan.Box(tent.transform, "Atap_Terpal", new Vector3(0f, 1.98f, 0f), new Vector3(colliderSize.x + 0.25f, 0.08f, colliderSize.z + 0.25f), tarpMat);
        tarp.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        Editor_Level1_RuteDanRintangan.Box(tent.transform, "Tirai_Belakang", new Vector3(0f, 1.15f, hz), new Vector3(colliderSize.x * 0.88f, 1.3f, 0.06f), canvasMat ?? tarpMat);

        PlaceCrateCluster(tent.transform, Vector3.zero, crateMat, fbxMeshes);
        CreateSandbagStack(tent.transform, new Vector3(-hx * 0.55f, 0f, -hz * 0.85f), sandbagMat);
        CreateSandbagStack(tent.transform, new Vector3(hx * 0.55f, 0f, -hz * 0.85f), sandbagMat);
        Editor_Level1_RuteDanRintangan.Box(tent.transform, "Papan_Peringatan", new Vector3(0f, 0.72f, -hz - 0.02f), new Vector3(colliderSize.x * 0.72f, 0.16f, 0.05f), warningMat);

        var lanternGo = new GameObject("Lentera_Tenda");
        lanternGo.transform.SetParent(tent.transform, false);
        lanternGo.transform.localPosition = new Vector3(0f, 1.68f, 0f);
        var light = lanternGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.76f, 0.42f);
        light.intensity = 22f;
        light.range = 5.5f;
    }

    internal static void BuildLogisticsPost(
        Transform root,
        string name,
        Vector3 pos,
        Vector3 colliderSize,
        Material timberMat,
        Material trimMat,
        Material drumMat,
        Material sandbagMat,
        Material crateMat,
        Material warningMat,
        Dictionary<string, Mesh> fbxMeshes)
    {
        var post = new GameObject(name);
        post.transform.SetParent(root, false);
        post.transform.position = pos;

        var col = post.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, colliderSize.y * 0.5f, 0f);
        col.size = colliderSize;

        Editor_Level1_RuteDanRintangan.Box(post.transform, "Palet_Kayu", new Vector3(0f, 0.06f, 0f), new Vector3(colliderSize.x * 0.9f, 0.12f, colliderSize.z * 0.85f), timberMat);
        PlaceCrateCluster(post.transform, new Vector3(-0.22f, 0.12f, 0f), crateMat, fbxMeshes);
        AddBarrel(post.transform, new Vector3(colliderSize.x * 0.30f, 0f, 0.18f), drumMat);
        AddBarrel(post.transform, new Vector3(colliderSize.x * 0.30f, 0f, -0.34f), trimMat);
        CreateSandbagStack(post.transform, new Vector3(-0.35f, 0f, -colliderSize.z * 0.42f), sandbagMat);
        CreateSandbagStack(post.transform, new Vector3(0.35f, 0f, colliderSize.z * 0.42f), sandbagMat);
        Editor_Level1_RuteDanRintangan.Box(post.transform, "Rambu_Barikade", new Vector3(0f, 0.95f, -colliderSize.z * 0.44f), new Vector3(colliderSize.x * 0.78f, 0.18f, 0.06f), warningMat);
    }

    internal static void BuildFallenTree(
        Transform root,
        string name,
        Vector3 pos,
        float yawDegrees,
        Material barkMat,
        Material timberMat,
        Material leafMat,
        Dictionary<string, Mesh> fbxMeshes,
        Mesh fallbackCanopyMesh)
    {
        var fallen = new GameObject(name);
        fallen.transform.SetParent(root, false);
        fallen.transform.position = pos;

        var col = fallen.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.62f, 0f);
        col.size = new Vector3(2.1f, 1.25f, 0.9f);

        // Root stump on one side
        var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stump.name = "Tunggul_Pohon";
        stump.transform.SetParent(fallen.transform, false);
        stump.transform.localPosition = new Vector3(-0.85f, 0.25f, 0f);
        stump.transform.localScale = new Vector3(0.42f, 0.28f, 0.42f);
        UnityEngine.Object.DestroyImmediate(stump.GetComponent<Collider>());
        if (barkMat != null) stump.GetComponent<Renderer>().sharedMaterial = barkMat;

        // Fallen trunk resting diagonally across road shoulder
        var logPivot = new GameObject("Batang_Rebah");
        logPivot.transform.SetParent(fallen.transform, false);
        logPivot.transform.localPosition = new Vector3(-0.05f, 0.34f, 0f);
        logPivot.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);

        var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        log.name = "Log_Utama";
        log.transform.SetParent(logPivot.transform, false);
        log.transform.localRotation = Quaternion.Euler(0f, 0f, 84f);
        log.transform.localScale = new Vector3(0.34f, 0.95f, 0.34f);
        UnityEngine.Object.DestroyImmediate(log.GetComponent<Collider>());
        if (barkMat != null) log.GetComponent<Renderer>().sharedMaterial = barkMat;

        Mesh canopyMesh = null;
        if (fbxMeshes != null)
            fbxMeshes.TryGetValue("P02_Broadleaf_Canopy", out canopyMesh);
        canopyMesh = canopyMesh ?? fallbackCanopyMesh;

        for (int i = 0; i < 2; i++)
        {
            var leaves = new GameObject($"Daun_Runtuh_{i}");
            leaves.transform.SetParent(logPivot.transform, false);
            leaves.transform.localPosition = new Vector3(0.55f + i * 0.35f, 0.24f, (i == 0 ? -0.18f : 0.22f));
            leaves.transform.localScale = Vector3.one * (0.62f - i * 0.1f);
            leaves.AddComponent<MeshFilter>().sharedMesh = canopyMesh;
            var mr = leaves.AddComponent<MeshRenderer>();
            if (leafMat != null) mr.sharedMaterial = leafMat;
        }

        Editor_Level1_RuteDanRintangan.Box(logPivot.transform, "Dahan_Patah", new Vector3(0.25f, 0.22f, 0.24f), new Vector3(0.55f, 0.12f, 0.12f), timberMat);
    }

    internal static void BuildChicaneCluster(
        Transform root,
        string name,
        Vector3 pos,
        Vector3 colliderSize,
        Material drumMat,
        Material timberMat,
        Material warningMat,
        Material sandbagMat,
        Material crateMat,
        Dictionary<string, Mesh> fbxMeshes)
    {
        var cluster = new GameObject(name);
        cluster.transform.SetParent(root, false);
        cluster.transform.position = pos;

        var col = cluster.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, colliderSize.y * 0.5f, 0f);
        col.size = colliderSize;

        AddBarrel(cluster.transform, new Vector3(-0.32f, 0f, 0f), drumMat);
        PlaceFbxCrate(cluster.transform, new Vector3(0.28f, 0f, 0.05f), crateMat, timberMat, fbxMeshes, "P02_Relief_Crate_0", 0.56f);
        CreateSandbagStack(cluster.transform, new Vector3(0f, 0f, -0.35f), sandbagMat);
        Editor_Level1_RuteDanRintangan.Box(cluster.transform, "Palang_Chicane", new Vector3(0f, 0.78f, 0f), new Vector3(1.1f, 0.14f, 0.06f), warningMat);
    }

    internal static void PlaceCrateCluster(Transform parent, Vector3 offset, Material crateMat, Dictionary<string, Mesh> fbxMeshes)
    {
        PlaceFbxCrate(parent, offset + new Vector3(-0.34f, 0f, -0.10f), crateMat, crateMat, fbxMeshes, "P02_Relief_Crate_0", 0.58f);
        PlaceFbxCrate(parent, offset + new Vector3(0.18f, 0f, 0.12f), crateMat, crateMat, fbxMeshes, "P02_Relief_Crate_1", 0.54f);
        PlaceFbxCrate(parent, offset + new Vector3(-0.12f, 0.54f, 0.02f), crateMat, crateMat, fbxMeshes, "P02_Relief_Crate_2", 0.48f);
    }

    internal static void PlaceFbxCrate(Transform parent, Vector3 targetBottomCenter, Material fbxMat, Material fallbackMat, Dictionary<string, Mesh> fbxMeshes, string meshKey, float fallbackSize)
    {
        if (fbxMeshes != null && fbxMeshes.TryGetValue(meshKey, out var mesh) && mesh != null)
        {
            var go = new GameObject(meshKey);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = fbxMat;
            float maxSpan = Mathf.Max(0.01f, Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.z));
            float uniformScale = fallbackSize / maxSpan;
            go.transform.localScale = Vector3.one * uniformScale;
            Vector3 c = mesh.bounds.center;
            Vector3 bottomCenterOffset = new Vector3(c.x * uniformScale, mesh.bounds.min.y * uniformScale, c.z * uniformScale);
            go.transform.localPosition = targetBottomCenter - bottomCenterOffset;
            return;
        }

        Editor_Level1_RuteDanRintangan.Box(parent, "Crate_Fallback", targetBottomCenter + Vector3.up * (fallbackSize * 0.5f), Vector3.one * fallbackSize, fallbackMat);
    }

    internal static void AddBarrel(Transform parent, Vector3 localPos, Material mat)
    {
        var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drum.name = "Drum_Besi";
        drum.transform.SetParent(parent, false);
        drum.transform.localPosition = localPos + Vector3.up * 0.44f;
        drum.transform.localScale = new Vector3(0.54f, 0.44f, 0.54f);
        UnityEngine.Object.DestroyImmediate(drum.GetComponent<Collider>());
        if (mat != null) drum.GetComponent<Renderer>().sharedMaterial = mat;
    }

    internal static void CreateSandbagStack(Transform root, Vector3 pos, Material sandbagMat)
    {
        var stack = new GameObject("KarungPasir");
        stack.transform.SetParent(root, false);
        stack.transform.localPosition = pos;

        Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Bawah1", new Vector3(-0.18f, 0.11f, 0f), new Vector3(0.42f, 0.22f, 0.32f), sandbagMat);
        Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Bawah2", new Vector3(0.22f, 0.11f, 0.03f), new Vector3(0.42f, 0.22f, 0.32f), sandbagMat);
        var top = Editor_Level1_RuteDanRintangan.Box(stack.transform, "Karung_Atas", new Vector3(0.02f, 0.31f, 0.01f), new Vector3(0.44f, 0.20f, 0.30f), sandbagMat);
        top.transform.localRotation = Quaternion.Euler(0f, 6f, 0f);
    }

    private static void BuildNorthSpillwayBridge(Transform root, Material plankMat, Material timberMat, Material sandbagMat)
    {
        var bridge = new GameObject("Jembatan_Titian_Utara");
        bridge.transform.SetParent(root, false);
        bridge.transform.position = Vector3.zero;

        // Approach South-North plank span at x = -7.6, z: 4.5 .. 6.9 (0.95m narrow width)
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN_L", new Vector3(-7.94f, 0.052f, 5.70f), new Vector3(0.11f, 0.048f, 2.5f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN_R", new Vector3(-7.26f, 0.052f, 5.70f), new Vector3(0.11f, 0.048f, 2.5f), timberMat);
        for (int i = 0; i < 6; i++)
        {
            float z = 4.55f + i * 0.45f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_Utara_SN_{i:D2}", new Vector3(-7.6f, 0.072f, z), new Vector3(0.95f, 0.032f, 0.36f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        // Exit East-West plank span at z = 6.7, x: -7.9 .. -4.8 (0.95m narrow width)
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_B", new Vector3(-6.35f, 0.052f, 6.36f), new Vector3(3.2f, 0.048f, 0.11f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_T", new Vector3(-6.35f, 0.052f, 7.04f), new Vector3(3.2f, 0.048f, 0.11f), timberMat);
        for (int i = 0; i < 7; i++)
        {
            float x = -7.65f + i * 0.46f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_Utara_EW_{i:D2}", new Vector3(x, 0.072f, 6.7f), new Vector3(0.36f, 0.032f, 0.95f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        CreateSandbagStack(bridge.transform, new Vector3(-8.35f, 0f, 4.4f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(-6.85f, 0f, 4.4f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(-4.65f, 0f, 5.9f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(-4.65f, 0f, 7.5f), sandbagMat);
    }

    private static void BuildNarrowLBridge(Transform root, Material plankMat, Material timberMat, Material sandbagMat)
    {
        var bridge = new GameObject("Jembatan_Sempit_L");
        bridge.transform.SetParent(root, false);
        bridge.transform.position = Vector3.zero;

        // South-North leg at x = 14.2, z: 0.2 .. 5.0 (0.95m narrow plank width connecting cleanly to north paving)
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN_L", new Vector3(13.86f, 0.052f, 2.60f), new Vector3(0.11f, 0.048f, 4.9f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_SN_R", new Vector3(14.54f, 0.052f, 2.60f), new Vector3(0.11f, 0.048f, 4.9f), timberMat);
        for (int i = 0; i < 11; i++)
        {
            float z = 0.25f + i * 0.46f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_SN_{i:D2}", new Vector3(14.2f, 0.072f, z), new Vector3(0.95f, 0.032f, 0.36f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        // East-West elbow leg at z = 3.5, x: 11.4 .. 14.5 (0.95m narrow plank width)
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_B", new Vector3(12.95f, 0.052f, 3.16f), new Vector3(3.1f, 0.048f, 0.11f), timberMat);
        Editor_Level1_RuteDanRintangan.Box(bridge.transform, "Stringer_EW_T", new Vector3(12.95f, 0.052f, 3.84f), new Vector3(3.1f, 0.048f, 0.11f), timberMat);
        for (int i = 0; i < 7; i++)
        {
            float x = 11.45f + i * 0.45f;
            var plank = Editor_Level1_RuteDanRintangan.Box(bridge.transform, $"Papan_EW_{i:D2}", new Vector3(x, 0.072f, 3.5f), new Vector3(0.36f, 0.032f, 0.95f), plankMat);
            plank.transform.localRotation = Quaternion.Euler(0f, (i % 3 - 1) * 1.2f, 0f);
        }

        CreateSandbagStack(bridge.transform, new Vector3(13.45f, 0f, 0.0f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(14.95f, 0f, 0.0f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(11.15f, 0f, 2.75f), sandbagMat);
        CreateSandbagStack(bridge.transform, new Vector3(11.15f, 0f, 4.25f), sandbagMat);
    }

    internal static void BuildElectricShortHazard(
        Transform root,
        string name,
        Vector3 center,
        Vector3 poleOffset,
        float phaseOffset,
        Material poleMat,
        Material warningMat,
        Material cableMat,
        Material safeMat,
        Material warnMat,
        Material activeMat,
        string ringAssetFolder = Level4PuddlesFolder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = center;

        // Leaning utility pole beside the flooded road with dangling broken wire
        var poleRoot = new GameObject("Tiang_Miring");
        poleRoot.transform.SetParent(go.transform, false);
        poleRoot.transform.localPosition = poleOffset;
        Vector3 tiltDir = -poleOffset.normalized;
        poleRoot.transform.localRotation = Quaternion.FromToRotation(Vector3.up, (Vector3.up * 2.4f + tiltDir * 0.7f).normalized);

        Editor_Level1_RuteDanRintangan.Box(poleRoot.transform, "Batang_Tiang", new Vector3(0f, 1.85f, 0f), new Vector3(0.18f, 3.7f, 0.18f), poleMat);
        Editor_Level1_RuteDanRintangan.Box(poleRoot.transform, "Trafo_Kuning", new Vector3(0f, 2.75f, 0.15f), new Vector3(0.34f, 0.42f, 0.26f), warningMat);
        Editor_Level1_RuteDanRintangan.Box(poleRoot.transform, "Palang_Atas", new Vector3(0f, 3.45f, 0f), new Vector3(0.75f, 0.10f, 0.10f), cableMat);

        // Two draped cable segments connecting the tilted pole top down to water center
        Vector3 tip = poleRoot.transform.TransformPoint(new Vector3(0f, 3.40f, 0f));
        Vector3 waterDrop = go.transform.position + Vector3.up * 0.08f;
        Vector3 midPoint = Vector3.Lerp(tip, waterDrop, 0.5f) - Vector3.up * 0.55f;
        AddCableSegment(go.transform, tip, midPoint, cableMat);
        AddCableSegment(go.transform, midPoint, waterDrop, cableMat);

        // Glowing flat ring on top of the puddle showing safe / warning / active phases
        var ringGo = new GameObject("Cincin_Listrik");
        ringGo.transform.SetParent(go.transform, false);
        ringGo.transform.localPosition = new Vector3(0f, 0.068f, 0f);
        var ringMesh = CreateHazardRingMesh(name + "_RingMesh", 1.18f, 1.45f, 36, ringAssetFolder);
        ringGo.AddComponent<MeshFilter>().sharedMesh = ringMesh;
        var ringRenderer = ringGo.AddComponent<MeshRenderer>();
        ringRenderer.sharedMaterial = safeMat;
        ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Crossed glowing lightning arcs inside ring during Active phase
        var sparks = new GameObject("Percikan_Kilat");
        sparks.transform.SetParent(go.transform, false);
        sparks.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        for (int i = 0; i < 4; i++)
        {
            var bolt = Editor_Level1_RuteDanRintangan.Box(sparks.transform, $"Kilat_{i}", Vector3.zero, new Vector3(2.2f, 0.05f, 0.08f), activeMat);
            bolt.transform.localRotation = Quaternion.Euler(0f, i * 45f, 0f);
            bolt.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        sparks.SetActive(false);

        var lightGo = new GameObject("Lampu_Korslet");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 4.5f;
        light.enabled = false;

        var hazard = go.AddComponent<Obstacle_KorsletingListrik>();
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardRadius").floatValue = 1.45f;
        so.FindProperty("warningRadius").floatValue = 4.2f;
        so.FindProperty("safeDuration").floatValue = 2.1f;
        so.FindProperty("warningDuration").floatValue = 0.85f;
        so.FindProperty("activeDuration").floatValue = 1.35f;
        so.FindProperty("phaseOffset").floatValue = phaseOffset;
        so.FindProperty("timePenalty").floatValue = 3f;
        so.FindProperty("shockSlowMultiplier").floatValue = 0.30f;
        so.FindProperty("shockSlowDuration").floatValue = 1.0f;
        so.FindProperty("shockCooldown").floatValue = 1.7f;
        so.FindProperty("pushDistance").floatValue = 0.55f;
        so.FindProperty("ringRenderer").objectReferenceValue = ringRenderer;
        so.FindProperty("sparkVisual").objectReferenceValue = sparks;
        so.FindProperty("pulseLight").objectReferenceValue = light;
        so.FindProperty("safeMaterial").objectReferenceValue = safeMat;
        so.FindProperty("warningMaterial").objectReferenceValue = warnMat;
        so.FindProperty("activeMaterial").objectReferenceValue = activeMat;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddCableSegment(Transform parent, Vector3 worldStart, Vector3 worldEnd, Material mat)
    {
        Vector3 delta = worldEnd - worldStart;
        var seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        seg.name = "Kabel_Juntai";
        seg.transform.SetParent(parent, true);
        seg.transform.position = (worldStart + worldEnd) * 0.5f;
        seg.transform.up = delta.normalized;
        seg.transform.localScale = new Vector3(0.035f, delta.magnitude * 0.5f, 0.035f);
        UnityEngine.Object.DestroyImmediate(seg.GetComponent<Collider>());
        if (mat != null) seg.GetComponent<Renderer>().sharedMaterial = mat;
    }

    internal static Mesh CreateHazardRingMesh(string meshName, float innerRadius, float outerRadius, int segments, string ringAssetFolder = Level4PuddlesFolder)
    {
        string path = ringAssetFolder + meshName + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = new Mesh();
            AssetDatabase.CreateAsset(mesh, path);
        }
        mesh.Clear();
        mesh.name = meshName;

        var verts = new Vector3[segments * 2];
        var uv = new Vector2[segments * 2];
        var tris = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            verts[i * 2] = dir * innerRadius;
            verts[i * 2 + 1] = dir * outerRadius;
            uv[i * 2] = new Vector2(0f, (float)i / segments);
            uv[i * 2 + 1] = new Vector2(1f, (float)i / segments);

            int next = (i + 1) % segments;
            int t = i * 6;
            tris[t] = i * 2;
            tris[t + 1] = next * 2 + 1;
            tris[t + 2] = i * 2 + 1;
            tris[t + 3] = i * 2;
            tris[t + 4] = next * 2;
            tris[t + 5] = next * 2 + 1;
        }
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
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
