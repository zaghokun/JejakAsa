#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class StandaloneLevelEntryTests
{
    GameProgressData before;
    string spawn;
    [SetUp] public void Setup()
    {before=StoryFlow_PengaturAlur.Instance.Progress.Clone();spawn=StoryFlow_PengaturAlur.Instance.LastSpawnPoint;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        var current=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("StandaloneEntryCleanup"));
        yield return SceneManager.UnloadSceneAsync(current);
        StoryFlow_PengaturAlur.Instance.SetStateForTesting(before);StoryFlow_PengaturAlur.Instance.LastSpawnPoint=spawn;
    }

    [UnityTest] public IEnumerator FreshLevelEntryInitializesDeliveryProgressAndMissionHud()
    {
        foreach(string name in new[]{"Kampung","Kampung_L3"})
        {
            Object.DestroyImmediate(StoryFlow_PengaturAlur.Instance.gameObject);
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/"+name+".unity");yield return null;
            var flow=StoryFlow_PengaturAlur.Instance;int level=name=="Kampung"?1:3;
            Assert.That(flow.CurrentPhase,Is.EqualTo(DayPhase.DiLevel));
            Assert.That(flow.CurrentDay,Is.EqualTo(level));Assert.That(flow.CurrentLevel,Is.EqualTo(level));
            Assert.That(flow.Progress.hasPickedUpBansos,Is.True);Assert.That(flow.Progress.isBriefingDone,Is.True);
            var mission=Object.FindFirstObjectByType<Mission_PengantaranBantuan>();
            Assert.That(mission.IsMissionActive,Is.True);Assert.That(mission.PackagesRemaining,Is.EqualTo(3));
            Assert.That(mission.DeliveredCount,Is.Zero);Assert.That(mission.TimeRemaining,Is.GreaterThan(name=="Kampung"?48:178));
            Assert.That(mission.GetComponent<UI_TujuanDanProgres>().enabled,Is.True);
            var objective=typeof(UI_TujuanDanProgres).GetMethod("GetObjective",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.That(objective.Invoke(null,new object[]{flow.Progress}),Is.EqualTo("Antarkan bantuan ke warga"));
            if(level==1){Assert.That(GameObject.Find("Petunjuk_Barat"),Is.Null);Assert.That(GameObject.Find("Petunjuk_Timur"),Is.Null);}
        }
    }

    [UnityTest] public IEnumerator ExistingCampaignStateIsPreservedWhenALevelSceneLoads()
    {
        var flow=StoryFlow_PengaturAlur.Instance;
        flow.SetStateForTesting(new GameProgressData{currentDay=4,currentLevel=4,currentPhase=DayPhase.PoskoBriefing,isP02ListeningDone=true,isThreatForeshadowDone=true});
        flow.LastSpawnPoint=null;string expected=JsonUtility.ToJson(flow.Progress);
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung_L4.unity");yield return null;
        Assert.That(JsonUtility.ToJson(flow.Progress),Is.EqualTo(expected));
        Assert.That(Object.FindFirstObjectByType<Mission_PengantaranBantuan>().IsMissionActive,Is.False);
    }
}
#endif
