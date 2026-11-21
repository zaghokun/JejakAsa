#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class P02ListeningTests
{
    GameProgressData before;string spawn;
    [SetUp] public void Setup(){before=StoryFlow_PengaturAlur.Instance.Progress.Clone();spawn=StoryFlow_PengaturAlur.Instance.LastSpawnPoint;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);StoryFlow_PengaturAlur.Instance.LastSpawnPoint=spawn;
        var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("P02ListeningCleanup"));
        if(scene.IsValid()&&scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);
    }
    [UnityTest] public IEnumerator FirstArrivalRequiresAllFiveLinesAndSurvivesSceneReload()
    {
        var flow=StoryFlow_PengaturAlur.Instance;
        flow.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.PoskoBriefing,requiresP02Listening=true});
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var rina=GameObject.Find("BuRina_NPC").GetComponent<StoryDialog_InteraksiNPC>();
        var story=rina.GetComponent<P02_MendengarkanWarga>();
        Assert.That(flow.CompleteBriefing(),Is.False);Assert.That(flow.CanDepartToLevel(out _),Is.False);
        Assert.That(Object.FindFirstObjectByType<PlayerController>().enabled,Is.True);
        Assert.That(story.BeginConversation(),Is.True);
        Assert.That(flow.Progress.isP02ListeningDone,Is.False);
        rina.CloseDialogue();yield return null;
        var player=GameObject.FindGameObjectWithTag("Player");
        var capsule=player.GetComponent<CharacterController>();capsule.enabled=false;player.transform.position=new Vector3(-.3f,1,-4.7f);capsule.enabled=true;
        var camera=Camera.main;var follow=camera.GetComponent<FixedIsometricCamera>();if(follow!=null)follow.enabled=false;
        camera.orthographicSize=4.7f;camera.transform.position=new Vector3(-1,0,-3.8f)-camera.transform.forward*18;
        yield return new WaitForSecondsRealtime(.4f);
        System.IO.Directory.CreateDirectory("Library/P02Captures");
        ScreenCapture.CaptureScreenshot("Library/P02Captures/p02-dialogue.png");
        yield return new WaitForSecondsRealtime(.2f);
        Assert.That(flow.Progress.isP02ListeningDone,Is.False);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        Assert.That(flow.Progress.isP02ListeningDone,Is.False,"Unloading a partial conversation must not finish it.");
        rina=GameObject.Find("BuRina_NPC").GetComponent<StoryDialog_InteraksiNPC>();story=rina.GetComponent<P02_MendengarkanWarga>();
        Assert.That(story.BeginConversation(),Is.True);
        for(int i=0;i<4;i++){rina.CloseDialogue();yield return null;Assert.That(flow.Progress.isP02ListeningDone,Is.False);}
        rina.CloseDialogue();yield return null;
        Assert.That(flow.Progress.isP02ListeningDone,Is.True);Assert.That(flow.Progress.isBriefingDone,Is.False);
        Assert.That(Object.FindFirstObjectByType<PlayerController>().enabled,Is.True);
        Assert.That(flow.CompleteBriefing(),Is.False,"P03-P05 must finish before the first delivery briefing.");
        var restored=JsonUtility.FromJson<GameProgressData>(JsonUtility.ToJson(flow.Progress));
        restored.ResetDailyFlags();Assert.That(restored.isP02ListeningDone,Is.True);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        Assert.That(GameObject.Find("Jonathan_NPC").GetComponent<StoryDialog_InteraksiNPC>().enabled,Is.True);
        Assert.That(flow.Progress.isP02ListeningDone,Is.True);
    }
    [UnityTest] public IEnumerator CharactersAndEnvironmentAreIntegratedAtHumanScale()
    {
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.PoskoBriefing});
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        Assert.That(GameObject.Find("P02_Posko_Environment"),Is.Not.Null);
        yield return new WaitForSecondsRealtime(.5f);
        foreach(string name in new[]{"Player","BuRina_NPC","Jonathan_NPC","Ophelia_NPC","PakArif_Warga"})
        {
            var actor=GameObject.Find(name);Assert.That(actor,Is.Not.Null,name);
            var meshes=actor.GetComponentsInChildren<SkinnedMeshRenderer>();Assert.That(meshes.Length,Is.GreaterThan(0),name);
            var animator=actor.GetComponentInChildren<Animator>();Assert.That(animator.avatar,Is.Not.Null,name);Assert.That(animator.avatar.isHuman,Is.True,name);
            Assert.That(meshes[0].sharedMaterial.mainTexture,Is.Not.Null,name);
        }
    }
}
#endif
