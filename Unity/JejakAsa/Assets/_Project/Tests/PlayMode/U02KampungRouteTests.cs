#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class U02KampungRouteTests : InputTestFixture
{
    private Scene loadedScene;
    private Keyboard keyboard;
    private float previousTimeScale;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    public override void TearDown()
    {
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator UnloadTestLevel()
    {
        if (loadedScene.IsValid() && loadedScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(loadedScene);
    }

    [UnityTest]
    public IEnumerator MainAndAlternateRoutesReachAllThreeRecipients()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Additive);
        loadedScene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Kampung.unity");
        Transform player = null;
        GameObject levelRoot = null;
        foreach (GameObject root in loadedScene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerController movement))
                player = movement.transform;
            if (root.name == "U02_Graybox")
                levelRoot = root;
        }
        Assert.That(player, Is.Not.Null, "The level must contain its playable character.");

        // 1. Jalur memutar (Gang_Memutar) harus tembus meski penghadang preman aktif
        yield return WalkRoute(player, new[] {
            new Vector2(-12f, -2.5f), new Vector2(-8f, -2.5f),
            new Vector2(-8f, 2f), new Vector2(0f, 2f), new Vector2(12f, 2f)
        });

        // 2. Setelah penghadang di Gang Utama diselesaikan, jalur utama juga dapat dilalui penuh
        if (levelRoot != null)
        {
            foreach (EncounterController enc in levelRoot.GetComponentsInChildren<EncounterController>(true))
            {
                enc.ResolveFromBattleVictory();
            }
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.position = new Vector3(-12f, 1f, -11f);
        controller.enabled = true;
        Physics.SyncTransforms();
        yield return null;

        yield return WalkRoute(player, new[] {
            new Vector2(-12f, -2.5f), new Vector2(0f, -2.5f),
            new Vector2(0f, 2f), new Vector2(12f, 2f)
        });
    }

    private IEnumerator WalkRoute(Transform player, Vector2[] waypoints)
    {
        foreach (Vector2 waypoint in waypoints)
        {
            Vector2 position = new Vector2(player.position.x, player.position.z);
            float deadline = Time.time + Vector2.Distance(position, waypoint) / 3.5f + 3f;
            while (Vector2.Distance(position, waypoint) > 0.25f)
            {
                Assert.That(Time.time, Is.LessThan(deadline), "Player stuck before waypoint " + waypoint);
                Vector2 remaining = waypoint - position;
                Key[] keys;
                if (Mathf.Abs(remaining.x) > 0.2f)
                    keys = remaining.x > 0f ? new[] { Key.S, Key.D } : new[] { Key.W, Key.A };
                else
                    keys = remaining.y > 0f ? new[] { Key.W, Key.D } : new[] { Key.S, Key.A };
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
                yield return null;
                position = new Vector2(player.position.x, player.position.z);
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }
    }
}
#endif
