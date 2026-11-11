#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U05OrderTests : InputTestFixture
{
    private GameObject root;
    private Transform player;
    private PlayerInteraction interaction;
    private InteractionTarget residentA;
    private InteractionTarget residentB;
    private InteractionTarget residentC;
    private Keyboard keyboard;

    public override void Setup()
    {
        base.Setup();
        keyboard = InputSystem.AddDevice<Keyboard>();
        root = new GameObject("U05 test world");

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(50f, 1f, 50f);

        GameObject view = new GameObject("Test view");
        view.transform.SetParent(root.transform);

        GameObject actor = new GameObject("Test player");
        actor.SetActive(false);
        actor.transform.SetParent(root.transform);
        actor.transform.position = new Vector3(0f, 1f, 0f);
        player = actor.transform;

        CharacterController controller = actor.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;

        PlayerController movement = actor.AddComponent<PlayerController>();
        SetReference(movement, "movementCamera", view.transform);

        GameObject canvas = new GameObject("Canvas");
        canvas.transform.SetParent(root.transform);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject prompt = new GameObject("Prompt");
        prompt.transform.SetParent(canvas.transform);
        TMP_Text promptText = prompt.AddComponent<TextMeshProUGUI>();

        GameObject dialogue = new GameObject("Dialogue");
        dialogue.transform.SetParent(canvas.transform);
        TMP_Text title = dialogue.AddComponent<TextMeshProUGUI>();
        TMP_Text body = dialogue.AddComponent<TextMeshProUGUI>();
        Button close = dialogue.AddComponent<Button>();

        interaction = actor.AddComponent<PlayerInteraction>();
        SetReference(interaction, "promptPanel", prompt);
        SetReference(interaction, "promptText", promptText);
        SetReference(interaction, "dialoguePanel", dialogue);
        SetReference(interaction, "dialogueTitle", title);
        SetReference(interaction, "dialogueBody", body);
        SetReference(interaction, "closeButton", close);

        actor.SetActive(true);

        // Buat 3 Warga di posisi berbeda
        GameObject npcA = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcA.name = "Warga Pos";
        npcA.transform.SetParent(root.transform);
        npcA.transform.position = new Vector3(0f, 1f, 1.2f);
        residentA = npcA.AddComponent<InteractionTarget>();
        SetReference(residentA, "displayName", "Warga Pos");
        SetReference(residentA, "recipientId", "warga_pos");

        GameObject npcB = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcB.name = "Warga Gang Tengah";
        npcB.transform.SetParent(root.transform);
        npcB.transform.position = new Vector3(10f, 1f, 0f);
        residentB = npcB.AddComponent<InteractionTarget>();
        SetReference(residentB, "displayName", "Warga Gang Tengah");
        SetReference(residentB, "recipientId", "warga_tengah");

        GameObject npcC = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcC.name = "Warga Gang Timur";
        npcC.transform.SetParent(root.transform);
        npcC.transform.position = new Vector3(20f, 1f, 0f);
        residentC = npcC.AddComponent<InteractionTarget>();
        SetReference(residentC, "displayName", "Warga Gang Timur");
        SetReference(residentC, "recipientId", "warga_timur");

        Physics.SyncTransforms();
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(root);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator DeliveringInAnyOrderSucceedsAndCompletesMission()
    {
        yield return null;
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3));
        Assert.That(interaction.DeliveredCount, Is.EqualTo(0));
        Assert.That(interaction.Trust, Is.EqualTo(0));
        Assert.That(interaction.IsMissionCompleted, Is.False);

        // Uji urutan acak/terbalik: antar ke Warga C (Timur) duluan
        player.position = residentC.transform.position + Vector3.back * 1.2f;
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(residentC));

        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        Assert.That(interaction.PackagesRemaining, Is.EqualTo(2), "Paket harus sisa 2 setelah warga pertama.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1));
        Assert.That(interaction.Trust, Is.EqualTo(1));
        Assert.That(residentC.IsDelivered, Is.True);

        // Tutup dialog
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Antar ke Warga A (Pos) kedua
        player.position = residentA.transform.position + Vector3.back * 1.2f;
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(residentA));

        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        Assert.That(interaction.PackagesRemaining, Is.EqualTo(1), "Paket harus sisa 1 setelah warga kedua.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(2));
        Assert.That(interaction.Trust, Is.EqualTo(2));
        Assert.That(residentA.IsDelivered, Is.True);

        // Tutup dialog
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Antar ke Warga B (Tengah) terakhir
        player.position = residentB.transform.position + Vector3.back * 1.2f;
        yield return null;
        Assert.That(interaction.CurrentTarget, Is.SameAs(residentB));

        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Semua 3 warga selesai!
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(0), "Paket harus 0 setelah semua terkirim.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(3), "Delivered count harus 3.");
        Assert.That(interaction.Trust, Is.EqualTo(3), "Trust harus 3.");
        Assert.That(residentB.IsDelivered, Is.True);
        Assert.That(interaction.IsMissionCompleted, Is.True, "Misi harus tercatat selesai saat 3/3 terkirim.");

        // Tutup dialog
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Kunjungi kembali Warga A yang sudah selesai
        player.position = residentA.transform.position + Vector3.back * 1.2f;
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // Progres tidak boleh bertambah atau paket berkurang lagi
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(0));
        Assert.That(interaction.DeliveredCount, Is.EqualTo(3));
        Assert.That(interaction.Trust, Is.EqualTo(3));
    }

    private static void SetReference(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Field " + fieldName + " not found on " + target);
        field.SetValue(target, value);
    }
}
#endif

