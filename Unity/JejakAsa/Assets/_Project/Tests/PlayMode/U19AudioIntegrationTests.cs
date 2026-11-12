#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public sealed class U19AudioIntegrationTests : InputTestFixture
{
    private const string KampungPath = "Assets/_Project/Scenes/Kampung.unity";
    private const string BattlePath = "Assets/_Project/Scenes/Battle.unity";
    private Keyboard keyboard;
    private readonly List<Behaviour> suspended = new List<Behaviour>();
    private Scene kampung;

    public override void Setup()
    {
        base.Setup();
        keyboard = InputSystem.AddDevice<Keyboard>();
        Suspend<AudioSource>();
        Suspend<AudioListener>();
        Suspend<Camera>();
        Suspend<EventSystem>();
        Suspend<SessionMenu>();
        Suspend<EncounterController>();
        Suspend<BattleTransition>();
        Suspend<PlayerInteraction>();
        Suspend<PlayerController>();
    }

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        Scene battle = SceneManager.GetSceneByPath(BattlePath);
        if (battle.IsValid() && battle.isLoaded)
            yield return SceneManager.UnloadSceneAsync(battle);
        if (kampung.IsValid() && kampung.isLoaded)
            yield return SceneManager.UnloadSceneAsync(kampung);
        foreach (Behaviour component in suspended)
            if (component != null) component.enabled = true;
        suspended.Clear();
    }

    [UnityTest]
    public IEnumerator BattleTransitionsAndRetryKeepOnlyTheCorrectMusicPlaying()
    {
        yield return SceneManager.LoadSceneAsync(KampungPath, LoadSceneMode.Additive);
        kampung = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        PlayerInteraction player = null;
        BattleTransition transition = null;
        AudioSource townAudio = null;
        foreach (GameObject root in kampung.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerInteraction found))
            {
                player = found;
                transition = root.GetComponent<BattleTransition>();
            }
            if (root.GetComponent<Camera>() != null)
                townAudio = root.GetComponent<AudioSource>();
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(transition, Is.Not.Null);
        Assert.That(townAudio, Is.Not.Null);
        transition.SetPostBattleDelayForTests(0f);
        yield return null;
        Assert.That(townAudio.isPlaying, Is.True);
        AssertOnePlayingSource();

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
        Assert.That(transition.IsInBattle, Is.True);
        Assert.That(townAudio.isPlaying, Is.False);
        AssertOnePlayingSource();
        BattleController battle = BattleController.Active;
        for (int i = 0; i < 4; i++)
            Assert.That(battle.PlayerAttack(), Is.True);
        deadline = Time.realtimeSinceStartup + 5f;
        while (transition.IsInBattle && Time.realtimeSinceStartup < deadline)
            yield return null;
        Assert.That(transition.IsInBattle, Is.False);
        Assert.That(townAudio.isPlaying, Is.True);
        AssertOnePlayingSource();
        player.ResetMissionSession();
        yield return null;
        Assert.That(townAudio.isPlaying, Is.True);
        AssertOnePlayingSource();
    }

    private void Suspend<T>() where T : Behaviour
    {
        foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!component.enabled) continue;
            suspended.Add(component);
            component.enabled = false;
        }
    }

    private IEnumerator Press(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    private static void AssertOnePlayingSource()
    {
        int playing = 0;
        foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (source.isActiveAndEnabled && source.isPlaying) playing++;
        Assert.That(playing, Is.EqualTo(1));
    }
}
#endif
