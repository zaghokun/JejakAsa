#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class U18MenuIntegrationTests : InputTestFixture
{
    private const string MenuPath = "Assets/_Project/Scenes/MainMenu.unity";
    private const string KampungPath = "Assets/_Project/Scenes/Kampung.unity";
    private const string BattlePath = "Assets/_Project/Scenes/Battle.unity";
    private Keyboard keyboard;
    private float oldTimeScale;

    public override void Setup()
    {
        base.Setup();
        oldTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    public override void TearDown()
    {
        Time.timeScale = oldTimeScale;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        Time.timeScale = 1f;
        Scene gameScene = SceneManager.GetActiveScene();
        Scene cleanScene = SceneManager.CreateScene("U18TestCleanup");
        SceneManager.SetActiveScene(cleanScene);
        if (gameScene.IsValid() && gameScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(gameScene);
    }

    [UnityTest]
    public IEnumerator MenuStartsSessionAndReturningFromBattleRequiresConfirmation()
    {
        yield return SceneManager.LoadSceneAsync(MenuPath, LoadSceneMode.Single);
        Object.FindFirstObjectByType<SessionMenu>().StartNewGame();
        yield return null;
        Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(KampungPath));
        PlayerInteraction player = Object.FindFirstObjectByType<PlayerInteraction>();
        Assert.That(player, Is.Not.Null);
        Assert.That(player.PackagesRemaining, Is.EqualTo(3));
        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = new Vector3(0f, 1f, -1.6f);
        controller.enabled = true;
        Physics.SyncTransforms();
        yield return null;
        yield return Press(Key.Digit2);
        float deadline = Time.realtimeSinceStartup + 5f;
        while ((BattleController.Active == null || !BattleController.Active.IsPlayerTurn) && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.That(BattleController.Active, Is.Not.Null);
        yield return Press(Key.Escape);
        Assert.That(Time.timeScale, Is.Zero);
        yield return Press(Key.Digit2);
        yield return Press(Key.Digit1);
        Assert.That(Time.timeScale, Is.Zero, "Membatalkan konfirmasi harus tetap dalam pause.");
        Assert.That(SceneManager.GetSceneByPath(BattlePath).isLoaded, Is.True);
        yield return Press(Key.Digit2);
        yield return Press(Key.Digit2);
        yield return null;
        Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(MenuPath));
        Assert.That(SceneManager.GetSceneByPath(BattlePath).isLoaded, Is.False);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
        Object.FindFirstObjectByType<SessionMenu>().StartNewGame();
        yield return null;
        Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(KampungPath));
        player = Object.FindFirstObjectByType<PlayerInteraction>();
        Assert.That(player.PackagesRemaining, Is.EqualTo(3));
        Assert.That(player.DeliveredCount, Is.Zero);
        Assert.That(player.Trust, Is.Zero);
        Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
    }

    [UnityTest]
    public IEnumerator OutcomeAcknowledgementRetriesExceptForCampaignSuccess()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.StartNewCampaign();
            flow.LastSpawnPoint = null;
            yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Single);
            PlayerInteraction player = Object.FindFirstObjectByType<PlayerInteraction>();
            Assert.That(player, Is.Not.Null);

            player.SetOutcome(MissionOutcome.Success, "Standalone");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Press(Key.Enter);
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(KampungPath));

            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.DiLevel });
            player.SetOutcome(MissionOutcome.PartialSuccess, "Retry");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Press(Key.Space);
            Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.DiLevel));

            player.SetOutcome(MissionOutcome.Success, "Complete");
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Press(Key.Enter);
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.PoskoLaporan));
            Assert.That(flow.Progress.isLevelCompleted, Is.True);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo("Assets/_Project/Scenes/Posko.unity"));
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    [UnityTest]
    public IEnumerator MenuLoadsHomeSavesAndProtectsExistingData()
    {
        string[] keys = new string[6];
        string[] values = new string[6];
        bool[] existed = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            keys[i] = "JejakAsa_SaveSlot_" + (i / 2 + 1) + (i % 2 == 1 ? "_Time" : "");
            existed[i] = PlayerPrefs.HasKey(keys[i]);
            values[i] = PlayerPrefs.GetString(keys[i]);
            PlayerPrefs.DeleteKey(keys[i]);
        }
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string spawn = flow.LastSpawnPoint;
        try
        {
            yield return SceneManager.LoadSceneAsync(MenuPath, LoadSceneMode.Single);
            SessionMenu menu = Object.FindFirstObjectByType<SessionMenu>();
            yield return Press(Key.Digit2);
            Assert.That(menu.MenuNotice, Does.Contain("Belum"));
            yield return Press(Key.Digit3);
            yield return Press(Key.Digit3);
            Assert.That(menu.MenuNotice, Does.Contain("kosong"));
            PlayerPrefs.SetString(keys[4], "not json");
            yield return Press(Key.Digit3);
            Assert.That(menu.MenuNotice, Does.Contain("tidak valid"));
            Assert.That(PlayerPrefs.GetString(keys[4]), Is.EqualTo("not json"));
            Assert.That(flow.LoadGame(3), Is.False);
            Assert.That(GameFlowManager.GetSlotMeta(3).exists, Is.True);
            Assert.That(flow.CurrentDay, Is.EqualTo(previous.currentDay));
            PlayerPrefs.SetString(keys[4], "{\"currentDay\":3,\"currentLevel\":3,\"currentPhase\":5}");
            Assert.That(flow.LoadGame(3), Is.False);
            Assert.That(flow.CurrentDay, Is.EqualTo(previous.currentDay));
            PlayerPrefs.SetString(keys[4], "not json");

            flow.SetStateForTesting(new GameProgressData { currentDay = 3, currentLevel = 3, currentPhase = DayPhase.RumahMalam });
            flow.SaveGame(2);
            string saved = PlayerPrefs.GetString(keys[2]);
            PlayerPrefs.SetString(keys[4], saved.Substring(0, saved.Length - 1));
            Assert.That(flow.LoadGame(3), Is.False);
            Assert.That(GameFlowManager.GetSlotMeta(3).day, Is.Zero);
            Assert.That(flow.CurrentDay, Is.EqualTo(3));
            PlayerPrefs.SetString(keys[4], "not json");
            yield return Press(Key.Digit2);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
            Assert.That(flow.CurrentDay, Is.EqualTo(3));
            HomeInteractionController home = Object.FindFirstObjectByType<HomeInteractionController>();
            Assert.That(home, Is.Not.Null);
            flow.StartNewCampaign();
            home.DoSave(2);
            Assert.That(PlayerPrefs.GetString(keys[2]), Is.EqualTo(saved));
            home.ConfirmSave();
            Assert.That(PlayerPrefs.GetString(keys[2]), Is.Not.EqualTo(saved));
            home.DoSave(3);
            Assert.That(PlayerPrefs.GetString(keys[4]), Is.EqualTo("not json"));
            yield return SceneManager.LoadSceneAsync(MenuPath, LoadSceneMode.Single);
            yield return Press(Key.Digit2);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
            Assert.That(flow.CurrentDay, Is.EqualTo(1));
        }
        finally
        {
            for (int i = 0; i < 6; i++)
            {
                if (existed[i]) PlayerPrefs.SetString(keys[i], values[i]);
                else PlayerPrefs.DeleteKey(keys[i]);
            }
            PlayerPrefs.Save();
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = spawn;
        }
    }

    [UnityTest]
    public IEnumerator NewCampaignRequiresConfirmationAndStartsAtHome()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string spawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentDay = 4, currentLevel = 4 });
            yield return SceneManager.LoadSceneAsync(MenuPath, LoadSceneMode.Single);
            yield return Press(Key.Digit1);
            yield return Press(Key.Digit1);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(MenuPath));
            Assert.That(flow.CurrentDay, Is.EqualTo(4));
            yield return Press(Key.Digit1);
            yield return Press(Key.Digit2);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Rumah"));
            Assert.That(flow.CurrentDay, Is.EqualTo(1));
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = spawn;
        }
    }

    [UnityTest]
    public IEnumerator SettingsChangeAndPersistVolume()
    {
        const string key = "JejakAsa_MasterVolume";
        bool existed = PlayerPrefs.HasKey(key);
        float oldPreference = PlayerPrefs.GetFloat(key);
        float oldVolume = AudioListener.volume;
        try
        {
            PlayerPrefs.SetFloat(key, 0.5f);
            yield return SceneManager.LoadSceneAsync(MenuPath, LoadSceneMode.Single);
            yield return Press(Key.Digit4);
            yield return Press(Key.Digit2);
            Assert.That(AudioListener.volume, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(PlayerPrefs.GetFloat(key), Is.EqualTo(0.6f).Within(0.001f));
            yield return Press(Key.Escape);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(MenuPath));
        }
        finally
        {
            if (existed) PlayerPrefs.SetFloat(key, oldPreference);
            else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            AudioListener.volume = oldVolume;
        }
    }

    [UnityTest]
    public IEnumerator EscapeClosesStoryDialogueBeforePausing()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        GameProgressData previous = flow.Progress.Clone();
        try
        {
            flow.StartNewCampaign();
            yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Single);
            SessionMenu menu = Object.FindFirstObjectByType<SessionMenu>();
            PlayerInteraction player = Object.FindFirstObjectByType<PlayerInteraction>();
            GameObject npcObject = new GameObject("Dialogue test");
            StoryNpcController npc = npcObject.AddComponent<StoryNpcController>();
            npc.playerTransform = player.transform;
            npc.OpenDialogue();
            yield return Press(Key.Escape);
            Assert.That(menu.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            yield return Press(Key.Escape);
            Assert.That(menu.IsPaused, Is.True);
            menu.Resume();
        }
        finally
        {
            flow.SetStateForTesting(previous);
        }
    }

    private IEnumerator Press(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }
}
#endif
