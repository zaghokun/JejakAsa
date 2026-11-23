#if UNITY_EDITOR
using System.Collections;
using System.Linq;
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
        foreach (GameObject root in loadedScene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out Player_Gerak movement))
                player = movement.transform;
        }
        Assert.That(player, Is.Not.Null, "The level must contain its playable character.");

        var recipients = loadedScene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Mission_PenerimaBantuan>())
            .Where(target => target.IsRecipient).OrderBy(target => target.transform.position.x).ToArray();
        Assert.That(recipients.Length, Is.EqualTo(3));

        Vector2[][] dryRoute = {
            new[] { new Vector2(-12f, -2.4f) },
            new[] { new Vector2(-16.8f, -2.4f), new Vector2(-16.8f, 7.5f),
                new Vector2(-9.4f, 7.5f), new Vector2(-9.4f, 16.6f),
                new Vector2(-5.6f, 16.6f), new Vector2(-5.6f, 4.8f), new Vector2(-.5f, 4.8f) },
            new[] { new Vector2(-3.7f, 4.7f), new Vector2(-3.7f, 12f),
                new Vector2(5.5f, 12f), new Vector2(5.5f, 16.6f),
                new Vector2(17.3f, 16.6f), new Vector2(17.3f, 4.3f), new Vector2(13f, 4.3f) }
        };
        Vector2[][] mainRoute = {
            new[] { new Vector2(-12f, -2.4f) },
            new[] { new Vector2(0f, -2.4f), new Vector2(0f, 2f) },
            new[] { new Vector2(0f, -4.3f), new Vector2(11.25f, -4.3f),
                new Vector2(11.25f, 4.3f), new Vector2(13f, 4.3f) }
        };
        var mission = player.GetComponent<Mission_PengantaranBantuan>();
        foreach (var route in new[] { dryRoute, mainRoute })
        {
            mission.ResetMissionSession();
            // U02 isolates traversability; the campaign test verifies the actual 50-second budget.
            mission.SetTimeRemaining(300f);
            yield return null;
            for (int i = 0; i < route.Length; i++)
            {
                yield return WalkRoute(player, route[i]);
                Vector3 distance = recipients[i].transform.position - player.position;
                distance.y = 0f;
                Assert.That(distance.magnitude, Is.LessThan(1.8f), recipients[i].DisplayName);
            }
        }
    }

    private IEnumerator WalkRoute(Transform player, Vector2[] waypoints)
    {
        foreach (Vector2 waypoint in waypoints)
        {
            Vector2 position = new Vector2(player.position.x, player.position.z);
            float deadline = Time.time + Vector2.Distance(position, waypoint) / 1.5f + 3f;
            while (Vector2.Distance(position, waypoint) > 0.25f)
            {
                if (Time.time >= deadline)
                    Assert.Fail("Player stuck before waypoint " + waypoint + " at " + player.position);
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
