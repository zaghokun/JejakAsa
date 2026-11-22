#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class PoskoPrologueTests
{
    GameProgressData before;string spawn;
    [SetUp] public void Setup(){before=StoryFlow_PengaturAlur.Instance.Progress.Clone();spawn=StoryFlow_PengaturAlur.Instance.LastSpawnPoint;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        var s=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("PrologueCleanup"));
        if(s.IsValid()&&s.isLoaded)yield return SceneManager.UnloadSceneAsync(s);
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);StoryFlow_PengaturAlur.Instance.LastSpawnPoint=spawn;
    }
    static GameProgressData Intro()=>new GameProgressData{currentPhase=DayPhase.PoskoBriefing,requiresP02Listening=true,poskoPrologueVersion=1,isP02ListeningDone=true};

    [Test] public void CampaignMilestonesSurviveSaveAndDailyResetWithoutRewindingLegacyDelivery()
    {
        var p=Intro();Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.Sponsor));
        p.isPoskoEstablished=true;Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.Reunion));
        p.isOpheliaReunionDone=true;Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.Threat));
        p.isThreatForeshadowDone=true;p.ResetDailyFlags();
        p=JsonUtility.FromJson<GameProgressData>(JsonUtility.ToJson(p));
        Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.None));
        p=Intro();p.poskoPrologueVersion=0;p.isBriefingDone=true;
        Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.None),"Existing saves already delivering must not replay the introduction.");
        p=Intro();p.currentDay=2;Assert.That(P03_SponsorDanPosko.GetStage(p),Is.EqualTo(PoskoIntroStage.None));
    }

    [UnityTest] public IEnumerator SponsorRequiresAllLinesThenBuildsPoskoAndReunionDoesNotCompleteDailyBriefing()
    {
        var flow=StoryFlow_PengaturAlur.Instance;flow.SetStateForTesting(Intro());
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var jon=GameObject.Find("Jonathan_NPC").GetComponent<StoryDialog_InteraksiNPC>();
        jon.OpenDialogue();jon.CloseDialogue();yield return null;
        Assert.That(flow.Progress.isPoskoEstablished,Is.False);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        jon=GameObject.Find("Jonathan_NPC").GetComponent<StoryDialog_InteraksiNPC>();
        jon.OpenDialogue();Assert.That(flow.CompleteBriefing(),Is.False);
        for(int i=0;i<5;i++){jon.CloseDialogue();yield return null;Assert.That(flow.Progress.isPoskoEstablished,Is.False);}
        jon.CloseDialogue();yield return null;
        Assert.That(Object.FindFirstObjectByType<P03_SponsorDanPosko>().IsSettingUp,Is.True);
        yield return Until(()=>flow.Progress.isPoskoEstablished,9);
        yield return Until(()=>!Object.FindFirstObjectByType<P03_SponsorDanPosko>().IsSettingUp,3);
        Assert.That(GameObject.Find("P02_Logistics_Visuals"),Is.Not.Null);
        Assert.That(flow.Progress.isBriefingDone,Is.False);
        Assert.That(flow.CanDepartToLevel(out _),Is.False);
        var oph=GameObject.Find("Ophelia_NPC").GetComponent<StoryDialog_InteraksiNPC>();
        Assert.That(oph.enabled,Is.True);oph.OpenDialogue();
        for(int i=0;i<6;i++){oph.CloseDialogue();yield return null;Assert.That(flow.Progress.isOpheliaReunionDone,Is.False);}
        oph.CloseDialogue();yield return null;
        Assert.That(flow.Progress.isOpheliaReunionDone,Is.True);
        Assert.That(flow.Progress.isOpheliaTalkDone,Is.False,"Reunion must not replace the evening report conversation.");
    }

    [UnityTest] public IEnumerator ThreatCompletesOnlyAfterBothVillainsAndRestoresCameraLightsAndControl()
    {
        var p=Intro();p.isPoskoEstablished=true;
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(p);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var music=new GameObject("Review_Music_State").AddComponent<AudioSource>();music.volume=.63f;
        StoryFlow_PengaturAlur.Instance.Progress.isOpheliaReunionDone=true;
        var camera=Camera.main;float size=camera.orthographicSize;
        var movement=GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        var light=GameObject.Find("Directional Light").GetComponent<Light>();float intensity=light.intensity;
        var host=GameObject.Find("P05_DialogVillain").GetComponent<StoryDialog_InteraksiNPC>();
        yield return Until(()=>Read<bool>(host,"IsDialogueOpen"),7);
        Assert.That(P05_BayanganAncaman.IsActive,Is.True);Assert.That(movement.enabled,Is.False);Assert.That(camera.orthographic,Is.False);
        var menu=Object.FindFirstObjectByType<SessionMenu>();menu.Pause();Assert.That(menu.IsPaused,Is.False);
        for(int i=0;i<4;i++)
        {
            yield return new WaitForSecondsRealtime(1);
            Assert.That(StoryFlow_PengaturAlur.Instance.Progress.isThreatForeshadowDone,Is.False);
            if(i==3)Assert.That(Read<string>(host,"ActiveSpeaker"),Is.EqualTo("Mr. Gas Oke"));
            host.CloseDialogue();yield return null;
        }
        yield return Until(()=>StoryFlow_PengaturAlur.Instance.Progress.isThreatForeshadowDone,5);
        Assert.That(P05_BayanganAncaman.IsActive,Is.False);Assert.That(movement.enabled,Is.True);
        Assert.That(camera.orthographic,Is.True);Assert.That(camera.orthographicSize,Is.EqualTo(size).Within(.001));
        Assert.That(light.intensity,Is.EqualTo(intensity).Within(.001));
        Assert.That(music.volume,Is.EqualTo(.63f).Within(.001),"Scene audio volume must return after the cutscene.");
        Assert.That(camera.GetComponent<FixedIsometricCamera>().enabled,Is.True);
        Assert.That(StoryFlow_PengaturAlur.Instance.Progress.isBriefingDone,Is.False);
        Assert.That(StoryFlow_PengaturAlur.Instance.CompleteBriefing(),Is.True);
    }

    [UnityTest] public IEnumerator InterruptedThreatRestoresStateAndRemainsPendingAcrossSceneReload()
    {
        var p=Intro();p.isPoskoEstablished=true;p.isOpheliaReunionDone=true;
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(p);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var host=GameObject.Find("P05_DialogVillain").GetComponent<StoryDialog_InteraksiNPC>();
        yield return Until(()=>Read<bool>(host,"IsDialogueOpen"),7);
        var root=Object.FindFirstObjectByType<P03_SponsorDanPosko>();root.enabled=false;
        var threat=Object.FindFirstObjectByType<P05_BayanganAncaman>();threat.enabled=false;
        yield return null;
        Assert.That(P05_BayanganAncaman.IsActive,Is.False);
        Assert.That(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled,Is.True);
        Assert.That(Camera.main.orthographic,Is.True);
        Assert.That(StoryFlow_PengaturAlur.Instance.Progress.isThreatForeshadowDone,Is.False);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        Assert.That(P03_SponsorDanPosko.GetStage(StoryFlow_PengaturAlur.Instance.Progress),Is.EqualTo(PoskoIntroStage.Threat));
        yield return Until(()=>P05_BayanganAncaman.IsActive,4);
    }
    static IEnumerator Until(Func<bool> condition,float seconds)
    {
        float deadline=Time.realtimeSinceStartup+seconds;
        while(!condition()&&Time.realtimeSinceStartup<deadline)yield return null;
        Assert.That(condition(),Is.True,"Prologue operation timed out.");
    }
    static T Read<T>(StoryDialog_InteraksiNPC host,string property)=>(T)typeof(StoryDialog_InteraksiNPC).GetProperty(property,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(host);
}
#endif
