#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class RumahBoundaryTests
{
    [UnityTest]
    public IEnumerator SouthPerimeterStopsPlayerAndKeepsFloorUnderfoot()
    {
        var flow = StoryFlow_PengaturAlur.Instance;
        var previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.RumahPagi });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Rumah.unity", LoadSceneMode.Single);
            var controller = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
            var floor = GameObject.Find("Room_Floor").GetComponent<Collider>();
            foreach (float x in new[] { -6f, -4f, -2f, 0f, 1f, 2f, 3f, 4f, 6f })
            {
                controller.enabled = false;
                Assert.That(Physics.Raycast(new Vector3(x, 0.5f, -4.4f), Vector3.down, out var hit, 1f), Is.True, $"Missing floor at x={x}");
                Assert.That(hit.collider, Is.EqualTo(floor), $"Unexpected floor at x={x}");
                controller.transform.position = new Vector3(x, 1f, -3.7f);
                controller.enabled = true;
                Physics.SyncTransforms();
                CollisionFlags flags = controller.Move(new Vector3(0f, -0.2f, -2f));
                Assert.That(flags & CollisionFlags.Sides, Is.Not.EqualTo(CollisionFlags.None), $"Open south edge at x={x}");
                Assert.That(controller.transform.position.z, Is.GreaterThan(-5f), $"Player crossed south edge at x={x}");
                Assert.That(controller.transform.position.y, Is.GreaterThan(0.8f), $"Player fell at x={x}");
            }
        }
        finally
        {
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    [UnityTearDown]
    public IEnumerator CleanUp()
    {
        Scene current = SceneManager.GetActiveScene();
        Scene empty = SceneManager.CreateScene("RumahBoundaryTestCleanup");
        SceneManager.SetActiveScene(empty);
        if (current.IsValid() && current.isLoaded)
            yield return SceneManager.UnloadSceneAsync(current);
    }
}
#endif
