#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U03KampungInteractionTests : InputTestFixture
{
    private Scene loadedScene;

    [UnityTearDown]
    public IEnumerator UnloadTestLevel()
    {
        if (loadedScene.IsValid() && loadedScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(loadedScene);
    }

    [UnityTest]
    public IEnumerator ResidentDialogueCanBeOpenedWithEAndClosedByMouse()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        Mouse mouse = InputSystem.AddDevice<Mouse>();
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Additive);
        loadedScene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Kampung.unity");
        GameObject player = null;
        InteractionTarget resident = null;
        Button close = null;
        foreach (GameObject root in loadedScene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerInteraction candidate))
                player = candidate.gameObject;
            if (root.name == "U02_Graybox")
                resident = root.GetComponentInChildren<InteractionTarget>();
            if (root.name == "InteractionCanvas")
                close = root.GetComponentInChildren<Button>(true);
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(resident, Is.Not.Null);
        Assert.That(close, Is.Not.Null);
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = new Vector3(resident.transform.position.x, 1f, resident.transform.position.z - 1.5f);
        controller.enabled = true;
        yield return null;
        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        Assert.That(interaction.CurrentTarget, Is.SameAs(resident));
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        Assert.That(interaction.DialogueTarget, Is.SameAs(resident));
        Assert.That(player.GetComponent<PlayerController>().enabled, Is.False);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());

        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, close.GetComponent<RectTransform>().position);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 1 });
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 0 });
        yield return null;
        yield return null;
        Assert.That(interaction.IsDialogueOpen, Is.False, "Clicking Tutup must close the actual game UI.");
        Assert.That(player.GetComponent<PlayerController>().enabled, Is.True);
    }
    [UnityTest]
    public IEnumerator ConfiguredDialogueAdvancesByKeyboardAndMouseWithoutUnlockingEarly()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        Mouse mouse = InputSystem.AddDevice<Mouse>();
        yield return SceneManager.LoadSceneAsync("Assets/_Project/Scenes/Kampung.unity", LoadSceneMode.Additive);
        loadedScene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Kampung.unity");
        PlayerInteraction player = null;
        InteractionTarget resident = null;
        Button button = null;
        foreach (GameObject root in loadedScene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out PlayerInteraction candidate)) player = candidate;
            if (root.name == "U02_Graybox") resident = root.GetComponentInChildren<InteractionTarget>();
            if (root.name == "InteractionCanvas") button = root.GetComponentInChildren<Button>(true);
        }
        Assert.That(player, Is.Not.Null);
        Assert.That(resident, Is.Not.Null);
        Assert.That(button, Is.Not.Null);
        var fields = new SerializedObject(resident);
        var lines = fields.FindProperty("deliveredDialogueLines");
        resident.MarkAsDelivered();
        lines.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var line = lines.GetArrayElementAtIndex(i);
            line.FindPropertyRelative("speaker").stringValue = "Speaker " + i;
            line.FindPropertyRelative("text").stringValue = "Line " + i;
        }
        fields.ApplyModifiedPropertiesWithoutUndo();
        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = new Vector3(resident.transform.position.x, 1f, resident.transform.position.z - 1.5f);
        controller.enabled = true;
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        Assert.That(player.IsDialogueOpen, Is.True);
        TMP_Text[] labels = button.transform.parent.GetComponentsInChildren<TMP_Text>(true);
        Assert.That(System.Array.Exists(labels, x => x.text == "Line 0"), Is.True);
        float time = player.TimeRemaining;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter));
        yield return null;
        Assert.That(System.Array.Exists(labels, x => x.text == "Line 1"), Is.True);
        Assert.That(player.IsDialogueOpen, Is.True);
        Assert.That(player.GetComponent<PlayerController>().enabled, Is.False);
        Assert.That(player.TimeRemaining, Is.EqualTo(time));
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, button.GetComponent<RectTransform>().position);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 1 });
        yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        yield return null;
        Assert.That(System.Array.Exists(labels, x => x.text == "Line 2"), Is.True);
        Assert.That(player.IsDialogueOpen, Is.True);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        Assert.That(player.IsDialogueOpen, Is.False);
        Assert.That(player.GetComponent<PlayerController>().enabled, Is.True);
    }
}
#endif
