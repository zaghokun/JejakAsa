#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class MenuPointerTests : InputTestFixture
{
    Mouse pointer;Keyboard keyboard;
    GameProgressData before;string spawn;float timeScale;
    public override void Setup()
    {
        base.Setup();pointer=InputSystem.AddDevice<Mouse>();keyboard=InputSystem.AddDevice<Keyboard>();
        before=GameFlowManager.Instance.Progress.Clone();spawn=GameFlowManager.Instance.LastSpawnPoint;timeScale=Time.timeScale;
    }
    public override void TearDown()
    {
        Time.timeScale=timeScale;GameFlowManager.Instance.SetStateForTesting(before);GameFlowManager.Instance.LastSpawnPoint=spawn;base.TearDown();
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        var s=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("PointerCleanup"));
        if(s.IsValid()&&s.isLoaded)yield return SceneManager.UnloadSceneAsync(s);
    }
    [UnityTest] public IEnumerator PauseHoverFollowsMouseAndStationaryPointerAllowsKeyboard()
    {
        GameFlowManager.Instance.SetStateForTesting(new GameProgressData{currentPhase=DayPhase.PoskoBriefing});
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Posko.unity");
        var menu=Object.FindFirstObjectByType<SessionMenu>();menu.Pause();
        Set(pointer.position,new Vector2(2,2));yield return new WaitForSecondsRealtime(.25f);
        MoveToRow(2);yield return null;Assert.That(menu.PilihanSekarang,Is.EqualTo(2));
        Press(keyboard.downArrowKey);yield return null;Release(keyboard.downArrowKey);yield return null;
        yield return new WaitForSecondsRealtime(.1f);
        Assert.That(menu.PilihanSekarang,Is.EqualTo(3),"Stationary pointer must not override keyboard selection.");
        MoveToRow(1);yield return null;Assert.That(menu.PilihanSekarang,Is.EqualTo(1));
        menu.PilihDariTombol(1);Assert.That(menu.HalamanSekarang,Is.EqualTo("Settings"));
        MoveToRow(2);yield return null;Assert.That(menu.PilihanSekarang,Is.EqualTo(2));
        menu.PilihDariTombol(3);menu.PilihDariTombol(2);Assert.That(menu.HalamanSekarang,Is.EqualTo("Load"));
        MoveToRow(1,76);yield return null;Assert.That(menu.PilihanSekarang,Is.EqualTo(1),"Save slots use a taller row hit area.");
        Assert.That(Time.timeScale,Is.Zero);menu.Resume();Assert.That(Time.timeScale,Is.EqualTo(1));
    }
    void MoveToRow(int row,float height=62)
    {
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
        float sw=Screen.width/scale,sh=Screen.height/scale;
        float x=(sw-720)/2+60+300,y=(sh-536)/2+214+row*height+22;
        Set(pointer.position,new Vector2(x*scale,Screen.height-y*scale));
    }
}
#endif
