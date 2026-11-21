#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class EnvironmentReadabilityTests
{
    [UnityTest]
    public IEnumerator CourtyardReliefHasWalkableCollisionAndClearNpcStandingAreas()
    {
        var flow=StoryFlow_PengaturAlur.Instance;var before=flow.Progress.Clone();string spawn=flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase=DayPhase.PoskoBriefing });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
            var surface=GameObject.Find("Posko_Courtyard_Relief").GetComponent<MeshCollider>();
            Assert.That(surface,Is.Not.Null);
            Assert.That(surface.bounds.max.y-surface.bounds.min.y,Is.GreaterThan(.15f),"Courtyard must have genuine height variation.");
            foreach(var point in new[]{new Vector3(0,3,-8),new Vector3(-1.6f,3,-3.5f),new Vector3(-5,3,4.6f),new Vector3(5,3,4.6f),new Vector3(0,3,-10.5f)})
            {
                RaycastHit hit;Assert.That(surface.Raycast(new Ray(point,Vector3.down),out hit,5),Is.True);
                Assert.That(hit.point.y,Is.LessThan(.045f),"NPC feet and the entrance must stay near the original floor.");
                Assert.That(hit.normal.y,Is.GreaterThan(.95f));
            }
            Assert.That(Camera.main.orthographicSize,Is.LessThan(4.5f));
        }
        finally{flow.SetStateForTesting(before);flow.LastSpawnPoint=spawn;}
    }
    [UnityTest]
    public IEnumerator GateAndWelcomeSignRemainVisibleWhileApproaching()
    {
        var flow = StoryFlow_PengaturAlur.Instance;
        var before = flow.Progress.Clone(); string spawn = flow.LastSpawnPoint;
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.PoskoBriefing });
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
            yield return null;
            var player = GameObject.FindGameObjectWithTag("Player");
            var header = GameObject.Find("P02_Gate_Header").GetComponent<Renderer>();
            var welcome=GameObject.Find("Gate_Welcome_Front").GetComponent<Renderer>();
            Assert.That(header.shadowCastingMode, Is.EqualTo(ShadowCastingMode.On));
            int colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
            var camera = Camera.main;
            float projectedHeight = camera.WorldToViewportPoint(player.transform.position + Vector3.up * .7f).y
                - camera.WorldToViewportPoint(player.transform.position - Vector3.up).y;
            Assert.That(projectedHeight, Is.GreaterThan(.095f), "Character silhouette must occupy enough of the viewport to read at 1080p.");
            player.GetComponent<CharacterController>().enabled = false;
            foreach(float z in new[]{-8f,-9.4f,-10.2f,-11.1f,-4f})
            {
                player.transform.position=new Vector3(0,1,z);
                yield return null;yield return null;
                Assert.That(header.enabled,Is.True);Assert.That(welcome.enabled,Is.True);
                Assert.That(header.shadowCastingMode,Is.EqualTo(ShadowCastingMode.On),"Gate must remain visible near the player.");
                Assert.That(welcome.shadowCastingMode,Is.EqualTo(ShadowCastingMode.On),"Welcome text must remain visible near the player.");
            }
            Assert.That(Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length, Is.EqualTo(colliders), "Sightline changes must not change collision.");
        }
        finally { flow.SetStateForTesting(before); flow.LastSpawnPoint = spawn; }
    }

    [UnityTearDown] public IEnumerator Cleanup()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(SceneManager.CreateScene("EnvironmentReadabilityCleanup"));
        if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
    }
}
#endif
