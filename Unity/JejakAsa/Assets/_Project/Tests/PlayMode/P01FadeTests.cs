#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class P01FadeTests
{
    [UnityTest]
    public IEnumerator MainMenuStartsPrologWithUnscaledFade()
    {
        StoryFlow_PengaturAlur flow = StoryFlow_PengaturAlur.Instance;
        GameProgressData previous = flow.Progress.Clone();
        string previousSpawn = flow.LastSpawnPoint;
        float previousTimeScale = Time.timeScale;
        try
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/MainMenu.unity", LoadSceneMode.Single);
            Object.FindFirstObjectByType<SessionMenu>().StartCampaignFromHome();
            yield return null;
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo("Assets/_Project/Scenes/Rumah.unity"));
            Assert.That(flow.CurrentPhase, Is.EqualTo(DayPhase.Prolog));
            SceneStartFade fade = Object.FindFirstObjectByType<SceneStartFade>();
            Assert.That(fade, Is.Not.Null);
            Assert.That(fade.IsFinished, Is.False);
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(1.3f);
            yield return null;
            Assert.That(fade.IsFinished, Is.True);
            Assert.That(fade.enabled, Is.False);
        }
        finally
        {
            Time.timeScale = previousTimeScale;
            flow.SetStateForTesting(previous);
            flow.LastSpawnPoint = previousSpawn;
        }
    }

    [Test]
    public void ProgressedCampaignDoesNotFade()
    {
        StoryFlow_PengaturAlur flow = StoryFlow_PengaturAlur.Instance;
        GameProgressData previous = flow.Progress.Clone();
        GameObject camera = new GameObject("Fade test camera");
        try
        {
            flow.SetStateForTesting(new GameProgressData { currentPhase = DayPhase.RumahPagi });
            SceneStartFade fade = camera.AddComponent<SceneStartFade>();
            Assert.That(fade.IsFinished, Is.True);
            Assert.That(fade.enabled, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(camera);
            flow.SetStateForTesting(previous);
        }
    }
}
#endif
