#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class GuiPresentationTests
{
    [UnityTest]
    public IEnumerator DialogueHasNoOpaqueBackingAndOutcomeRenders()
    {
        var before = StoryFlow_PengaturAlur.Instance.Progress.Clone();
        try
        {
            StoryFlow_PengaturAlur.Instance.StartNewCampaign();
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity");
            var player = Object.FindFirstObjectByType<PlayerInteraction>();
            player.OpenSystemDialogue("Vicky", "Akhirnya pulang juga. Rasanya gang ini lebih sempit daripada yang kuingat.");
            yield return new WaitForSecondsRealtime(.5f);
            var field = typeof(PlayerInteraction).GetField("dialoguePanel", BindingFlags.Instance | BindingFlags.NonPublic);
            var panel = (GameObject)field.GetValue(player);
            Assert.That(panel.GetComponent<Image>().color.a, Is.Zero, "The illustration must not have an opaque rectangle behind it.");
            Assert.That(panel.transform.Find("PortraitMount"), Is.Null, "Portraits should have no rectangular backing.");
            ScreenCapture.CaptureScreenshot("Library/GuiRedesignCaptures/06-dialogue.png");
            yield return new WaitForSecondsRealtime(.3f);
            typeof(PlayerInteraction).GetMethod("CloseDialogue", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player,null);
            player.SetOutcome(MissionOutcome.Success,"Semua bantuan telah sampai kepada warga.");
            yield return new WaitForSecondsRealtime(.5f);
            Assert.That(player.IsOutcomeOpen, Is.True);
            ScreenCapture.CaptureScreenshot("Library/GuiRedesignCaptures/07-result.png");
            yield return new WaitForSecondsRealtime(.3f);
            yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/MainMenu.unity");
            var menu = Object.FindFirstObjectByType<SessionMenu>();
            typeof(SessionMenu).GetMethod("Activate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(menu,new object[]{0});
            yield return new WaitForSecondsRealtime(.5f);
            ScreenCapture.CaptureScreenshot("Library/GuiRedesignCaptures/08-confirm.png");
            yield return new WaitForSecondsRealtime(.3f);
        }
        finally { StoryFlow_PengaturAlur.Instance.SetStateForTesting(before); Time.timeScale=1; }
    }
}
#endif
