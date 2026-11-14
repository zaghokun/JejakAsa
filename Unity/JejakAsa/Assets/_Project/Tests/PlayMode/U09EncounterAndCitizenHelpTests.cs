#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U09EncounterAndCitizenHelpTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerController movement;
    private PlayerInteraction interaction;
    private InteractionTarget residentA;
    private EncounterController encounter;
    private GameObject premanVisual;
    private GameObject obstacle;
    private GameObject marker;
    private Keyboard keyboard;
    private float previousTimeScale;
    private Random.State previousRandomState;
    private GameProgressData previousProgress;
    private string previousSpawn;
    private Scene loadedScene;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        previousRandomState = Random.state;
        previousProgress = GameFlowManager.Instance.Progress.Clone();
        previousSpawn = GameFlowManager.Instance.LastSpawnPoint;
        GameFlowManager.Instance.StartNewCampaign();
        GameFlowManager.Instance.LastSpawnPoint = null;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();

        root = new GameObject("U09 test world");

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(30f, 1f, 30f);

        GameObject view = new GameObject("Test view");
        view.transform.SetParent(root.transform);

        GameObject actor = new GameObject("Test player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = new Vector3(0f, 1f, -4f);
        player = actor.transform;

        CharacterController controller = actor.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;

        movement = actor.AddComponent<PlayerController>();
        SetReference(movement, "movementCamera", view.transform);

        GameObject canvas = new GameObject("Canvas", typeof(RectTransform));
        canvas.transform.SetParent(root.transform, false);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject prompt = UiObject("Prompt", canvas.transform);
        TMP_Text promptText = UiObject("Prompt text", prompt.transform).AddComponent<TextMeshProUGUI>();

        GameObject dialogue = UiObject("Dialogue", canvas.transform);
        TMP_Text title = UiObject("Title", dialogue.transform).AddComponent<TextMeshProUGUI>();
        TMP_Text body = UiObject("Body", dialogue.transform).AddComponent<TextMeshProUGUI>();
        Button close = UiObject("Close", dialogue.transform).AddComponent<Button>();

        interaction = actor.AddComponent<PlayerInteraction>();
        SetReference(interaction, "promptPanel", prompt);
        SetReference(interaction, "promptText", promptText);
        SetReference(interaction, "dialoguePanel", dialogue);
        SetReference(interaction, "dialogueTitle", title);
        SetReference(interaction, "dialogueBody", body);
        SetReference(interaction, "closeButton", close);

        actor.SetActive(true);

        GameObject npcA = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcA.name = "Warga Pos";
        npcA.transform.SetParent(root.transform);
        npcA.transform.position = new Vector3(-4f, 1f, -4f);
        residentA = npcA.AddComponent<InteractionTarget>();
        SetReference(residentA, "displayName", "Warga Pos");
        SetReference(residentA, "recipientId", "warga_pos");

        GameObject encounterPoint = new GameObject("EncounterPoint");
        encounterPoint.transform.SetParent(root.transform);
        encounterPoint.transform.position = Vector3.zero;

        premanVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        premanVisual.name = "Preman_Graybox";
        premanVisual.transform.SetParent(encounterPoint.transform);
        premanVisual.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Penghalang_Gang_Utama";
        obstacle.transform.SetParent(encounterPoint.transform);
        obstacle.transform.localPosition = new Vector3(0f, 0.35f, 0.35f);
        obstacle.transform.localScale = new Vector3(3f, 0.7f, 0.35f);

        marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Penanda_Intimidasi_BelumAktif";
        marker.transform.SetParent(encounterPoint.transform);
        marker.transform.localPosition = new Vector3(0f, 0.04f, 0f);

        encounter = encounterPoint.AddComponent<EncounterController>();
        encounter.ConfigureForTests(interaction, obstacle, premanVisual, marker);

        Physics.SyncTransforms();
    }

    public override void TearDown()
    {
        if (root != null)
            Object.DestroyImmediate(root);
        Time.timeScale = previousTimeScale;
        Random.state = previousRandomState;
        GameFlowManager.Instance.SetStateForTesting(previousProgress);
        GameFlowManager.Instance.LastSpawnPoint = previousSpawn;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator UnloadKampungIfLoaded()
    {
        if (loadedScene.IsValid() && loadedScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(loadedScene);
    }

    [UnityTest]
    public IEnumerator EnteringRadiusOpensEncounterFreezesMovementAndTimer()
    {
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.False);

        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;

        Assert.That(encounter.IsEncounterOpen, Is.True, "Encounter harus terbuka saat memasuki radius 1,8 m.");
        Assert.That(movement.enabled, Is.False, "Gerak pemain harus berhenti saat pilihan intimidasi terbuka.");

        float frozenTime = interaction.TimeRemaining;
        yield return new WaitForSeconds(0.2f);
        Assert.That(interaction.TimeRemaining, Is.EqualTo(frozenTime), "Timer misi harus membeku saat pilihan intimidasi aktif.");
    }

    [UnityTest]
    public IEnumerator AvoidOptionRestoresControlWithoutImmediateRetriggerUntilExiting25m()
    {
        yield return null;
        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.True);

        SetAvoidResult(true);
        encounter.AvoidEncounter();

        Assert.That(encounter.IsEncounterOpen, Is.False, "Panel harus menutup setelah Menghindar.");
        Assert.That(encounter.IsResolved, Is.False, "Preman tetap berjaga setelah Menghindar.");
        Assert.That(obstacle.activeSelf, Is.True, "Penghalang tetap aktif saat hanya menghindar.");
        Assert.That(movement.enabled, Is.True, "Kendali gerak harus kembali setelah Menghindar.");
        Assert.That(encounter.IsAwaitingExit, Is.True);

        // Tetap di dalam radius <= 2,5 m beberapa frame tidak boleh memicu ulang otomatis
        TeleportPlayerTo(new Vector3(0f, 1f, -2.0f));
        yield return null;
        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.False, "Encounter tidak boleh aktif lagi sebelum keluar radius 2,5 m.");

        // Keluar radius 2,5 m lalu masuk kembali ke radius 1,8 m
        TeleportPlayerTo(new Vector3(0f, 1f, -2.8f));
        yield return null;
        Assert.That(encounter.IsAwaitingExit, Is.False, "Trigger harus siap lagi setelah melewati radius 2,5 m.");

        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.True, "Encounter harus aktif kembali saat pemain mendekat ulang.");
    }

    [UnityTest]
    public IEnumerator FailedAvoidRequestsBattleOnceAndKeepsMissionState()
    {
        yield return null;
        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.True);
        int battles = 0;
        encounter.BattleRequested += _ => battles++;
        int packages = interaction.PackagesRemaining;
        SetAvoidResult(false);
        yield return PressKey(Key.Digit1);
        Assert.That(battles, Is.EqualTo(1));
        Assert.That(encounter.HasBattleRequestPending, Is.True);
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(packages));
        Assert.That(movement.enabled, Is.False);
    }

    [UnityTest]
    public IEnumerator CitizenHelpLockedAtZeroTrustAndUnlockedAfterOneDeliveryDisablesBarrierWithoutBattle()
    {
        yield return null;
        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;

        // Trust 0 -> Bantuan Warga terkunci (U10)
        Assert.That(interaction.Trust, Is.EqualTo(0));
        Assert.That(encounter.IsCitizenHelpAvailable, Is.False);
        Assert.That(encounter.TryUseCitizenHelp(), Is.False, "Bantuan Warga harus ditolak saat Kepercayaan 0.");
        Assert.That(encounter.IsEncounterOpen, Is.True);
        Assert.That(obstacle.activeSelf, Is.True);

        // Menghindar lalu antar bantuan ke Warga Pos -> Trust jadi 1
        SetAvoidResult(true);
        encounter.AvoidEncounter();
        TeleportPlayerTo(new Vector3(-4f, 1f, -2.8f));
        yield return null;
        yield return PressKey(Key.E);
        Assert.That(interaction.Trust, Is.EqualTo(1));
        yield return PressKey(Key.Escape);

        // Kembali hadapi preman -> Bantuan Warga tersedia & membuka jalan tanpa battle
        TeleportPlayerTo(new Vector3(0f, 1f, -1.5f));
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.True);
        Assert.That(encounter.IsCitizenHelpAvailable, Is.True);

        yield return PressKey(Key.Digit3);

        Assert.That(encounter.IsResolved, Is.True, "Encounter harus selesai setelah Bantuan Warga.");
        Assert.That(encounter.IsEncounterOpen, Is.False);
        Assert.That(obstacle.activeSelf, Is.False, "Penghalang harus nonaktif setelah Bantuan Warga.");
        Assert.That(encounter.HasBattleRequestPending, Is.False, "Bantuan Warga tidak boleh masuk battle.");
        Assert.That(interaction.Trust, Is.EqualTo(1), "Kepercayaan tidak boleh berkurang saat memakai Bantuan Warga.");
        Assert.That(interaction.IsDialogueOpen, Is.True, "Dialog bantuan warga harus tampil.");

        // Tutup dialog bantuan warga -> kendali kembali ke eksplorasi
        yield return PressKey(Key.Escape);
        Assert.That(interaction.IsDialogueOpen, Is.False);
        Assert.That(movement.enabled, Is.True, "Kendali eksplorasi harus kembali setelah dialog ditutup.");
    }

    [UnityTest]
    public IEnumerator KampungScenePremanEncounterAvoidAndCitizenHelpEndToEnd()
    {
        // Nonaktifkan test world terisolasi agar PlayerInteraction dari scene Kampung yang aktif sendiri
        root.SetActive(false);

        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Additive);
        loadedScene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Kampung.unity");

        PlayerInteraction sceneInteraction = null;
        EncounterController sceneEncounter = null;
        InteractionTarget firstResident = null;

        foreach (GameObject go in loadedScene.GetRootGameObjects())
        {
            if (go.TryGetComponent(out PlayerInteraction candidateInteraction))
                sceneInteraction = candidateInteraction;
            if (go.name == "U02_Graybox")
            {
                sceneEncounter = go.GetComponentInChildren<EncounterController>(true);
                firstResident = go.GetComponentInChildren<InteractionTarget>(true);
            }
        }

        Assert.That(sceneInteraction, Is.Not.Null);
        Assert.That(sceneEncounter, Is.Not.Null, "Kampung.unity harus memiliki EncounterController pada EncounterPoint.");
        Assert.That(sceneEncounter.PremanVisual, Is.Not.Null, "Kampung.unity harus memiliki Preman_Graybox.");
        Assert.That(sceneEncounter.ObstacleObject, Is.Not.Null, "Kampung.unity harus memiliki Penghalang_Gang_Utama.");
        Assert.That(sceneEncounter.ObstacleObject.activeSelf, Is.True);
        Assert.That(firstResident, Is.Not.Null);

        yield return null;

        // 1. Antar ke warga pertama sebelum menghadapi preman -> Trust 1
        CharacterController cc = sceneInteraction.GetComponent<CharacterController>();
        cc.enabled = false;
        sceneInteraction.transform.position = new Vector3(firstResident.transform.position.x, 1f, firstResident.transform.position.z - 1.5f);
        cc.enabled = true;
        Physics.SyncTransforms();
        yield return null;

        yield return PressKey(Key.E);
        Assert.That(sceneInteraction.Trust, Is.EqualTo(1));
        yield return PressKey(Key.Escape);

        // 2. Dekati EncounterPoint (0, 0, 0) -> terbuka -> coba Menghindar
        cc.enabled = false;
        sceneInteraction.transform.position = new Vector3(0f, 1f, -1.6f);
        cc.enabled = true;
        Physics.SyncTransforms();
        yield return null;
        Assert.That(sceneEncounter.IsEncounterOpen, Is.True);

        SetAvoidResult(true);
        yield return PressKey(Key.Escape);
        Assert.That(sceneEncounter.IsEncounterOpen, Is.False);

        // Keluar > 2.5 m lalu dekati kembali
        cc.enabled = false;
        sceneInteraction.transform.position = new Vector3(0f, 1f, -2.8f);
        cc.enabled = true;
        Physics.SyncTransforms();
        yield return null;

        cc.enabled = false;
        sceneInteraction.transform.position = new Vector3(0f, 1f, -1.6f);
        cc.enabled = true;
        Physics.SyncTransforms();
        yield return null;
        Assert.That(sceneEncounter.IsEncounterOpen, Is.True);

        // 3. Pakai Bantuan Warga -> tutup dialog -> berjalan menembus (0, 0) hingga z > 1.2
        Assert.That(sceneEncounter.TryUseCitizenHelp(), Is.True);
        yield return PressKey(Key.Escape);
        Assert.That(sceneEncounter.IsResolved, Is.True);
        Assert.That(sceneEncounter.ObstacleObject.activeSelf, Is.False);

        float deadline = Time.time + 4f;
        while (sceneInteraction.transform.position.z < 1.2f)
        {
            Assert.That(Time.time, Is.LessThan(deadline), "Pemain gagal berjalan lurus melewati gang utama setelah penghalang dibuka.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D));
            yield return null;
        }
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // 4. Reset misi bersih mengaktifkan kembali preman & penghalang (U08/U17)
        sceneInteraction.ResetMissionSession();
        yield return null;
        Assert.That(sceneEncounter.IsResolved, Is.False);
        Assert.That(sceneEncounter.ObstacleObject.activeSelf, Is.True);
        Assert.That(sceneEncounter.PremanVisual.activeSelf, Is.True);
    }

    private static void SetAvoidResult(bool success)
    {
        for (int seed = 0; seed < 100; seed++)
        {
            Random.InitState(seed);
            if ((Random.value < 0.5f) != success)
                continue;
            Random.InitState(seed);
            return;
        }
        Assert.Fail("Could not choose a deterministic avoidance outcome.");
    }

    private void TeleportPlayerTo(Vector3 targetPos)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = targetPos;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    private IEnumerator PressKey(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    private static GameObject UiObject(string name, Transform parent)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        return result;
    }

    private static void SetReference(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Field " + fieldName + " not found on " + target);
        field.SetValue(target, value);
    }
}
#endif

