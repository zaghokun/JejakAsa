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
        yield return Press(Key.Digit1);
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
        yield return Press(Key.Digit1);
        yield return null;
        Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(KampungPath));
        player = Object.FindFirstObjectByType<PlayerInteraction>();
        Assert.That(player.PackagesRemaining, Is.EqualTo(3));
        Assert.That(player.DeliveredCount, Is.Zero);
        Assert.That(player.Trust, Is.Zero);
        Assert.That(player.Outcome, Is.EqualTo(MissionOutcome.InProgress));
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
