#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
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
}
#endif
