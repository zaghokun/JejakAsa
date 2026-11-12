#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class U16PauseIntegrationTests : InputTestFixture
{
    private const string KampungPath = "Assets/_Project/Scenes/Kampung.unity";
    private const string BattlePath = "Assets/_Project/Scenes/Battle.unity";
    private Keyboard keyboard;
    private Scene kampung;
    private float oldTimeScale;

    public override void Setup()
    {
        base.Setup();
        oldTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        Time.timeScale = 1f;
        Scene battle = SceneManager.GetSceneByPath(BattlePath);
        if (battle.IsValid() && battle.isLoaded)
            yield return SceneManager.UnloadSceneAsync(battle);
        if (kampung.IsValid() && kampung.isLoaded)
            yield return SceneManager.UnloadSceneAsync(kampung);
        Time.timeScale = oldTimeScale;
    }

    [UnityTest]
    public IEnumerator PauseStopsPendingBattleDamageAndResumeCompletesExactlyOneTurn()
    {
        yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Additive);
        kampung = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        PlayerInteraction player = null;
        SessionMenu menu = null;
        BattleTransition transition = null;
        foreach (GameObject root in kampung.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerInteraction found))
            {
                player = found;
                menu = root.GetComponent<SessionMenu>();
                transition = root.GetComponent<BattleTransition>();
            }
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(menu, Is.Not.Null);
        Assert.That(transition, Is.Not.Null);
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
        BattleController battle = BattleController.Active;
        Assert.That(battle, Is.Not.Null);
        Assert.That(transition.IsInBattle, Is.True);
        Assert.That(battle.TriggerPlayerAttackAnimated(), Is.True);
        yield return Press(Key.Escape);
        Assert.That(menu.IsPaused, Is.True);
        Assert.That(Time.timeScale, Is.Zero);
        int playerHP = battle.PlayerHP;
        int enemyHP = battle.EnemyHP;
        int turn = battle.TurnNumber;
        yield return new WaitForSecondsRealtime(0.8f);
        Assert.That(battle.PlayerHP, Is.EqualTo(playerHP));
        Assert.That(battle.EnemyHP, Is.EqualTo(enemyHP));
        Assert.That(battle.TurnNumber, Is.EqualTo(turn));
        Assert.That(battle.TriggerPlayerAttackAnimated(), Is.False);
        yield return Press(Key.Escape);
        Assert.That(menu.IsPaused, Is.False);
        deadline = Time.realtimeSinceStartup + 5f;
        while (battle.ActionLocked && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.That(battle.ActionLocked, Is.False);
        Assert.That(battle.IsPlayerTurn, Is.True);
        Assert.That(battle.PlayerHP, Is.EqualTo(24));
        Assert.That(battle.EnemyHP, Is.EqualTo(40));
        Assert.That(battle.TurnNumber, Is.EqualTo(2));
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
