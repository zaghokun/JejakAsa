#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JejakAsa;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class U07PriorityAndOutcomeTests : InputTestFixture
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
        root = new GameObject("U07 test world");

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

        GameObject canvas = new GameObject("Canvas", typeof(RectTransform));
        canvas.transform.SetParent(root.transform, false);
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject prompt = UiObject("Prompt", canvas.transform);
        TMP_Text promptText = UiObject("Prompt text", prompt.transform).AddComponent<TextMeshProUGUI>();

        GameObject dialogue = UiObject("Dialogue", canvas.transform);
        TMP_Text title = UiObject("Title", dialogue.transform).AddComponent<TextMeshProUGUI>();
        TMP_Text body = UiObject("Body", dialogue.transform).AddComponent<TextMeshProUGUI>();
        Button close = UiObject("Close", dialogue.transform).AddComponent<Button>();

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
    public IEnumerator LateDeliveryRejectedWhenTimeExpired()
    {
        yield return null;

        // Atur waktu menjadi habis (0s)
        interaction.SetTimeRemaining(0f);
        Assert.That(interaction.IsTimeExpired, Is.True);
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.Failed), "Misi harus berstatus Failed jika waktu habis dengan 0 paket.");

        // Dekati warga dan coba tekan E
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        // U07: Penyerahan terlambat harus ditolak!
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3), "Paket tidak boleh berkurang saat waktu sudah habis.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(0));
        Assert.That(interaction.Trust, Is.EqualTo(0));
        Assert.That(residentA.IsDelivered, Is.False, "Warga tidak boleh ditandai selesai.");
    }

    [UnityTest]
    public IEnumerator ValidDeliveryNotOverwrittenByLateTimerExpiry()
    {
        yield return null;

        // 1. Antar ke Warga A
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return CloseDialogueViaEsc();

        // 2. Antar ke Warga B
        TeleportPlayerTo(new Vector3(10f, 1f, 1.2f));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return CloseDialogueViaEsc();

        // 3. Antar ke Warga C (Paket ke-3 / Terakhir)
        TeleportPlayerTo(new Vector3(20f, 1f, 1.2f));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return CloseDialogueViaEsc();

        // U05 & U08: Selesai 3/3
        Assert.That(interaction.IsMissionCompleted, Is.True);
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.Success), "Hasil misi harus Success.");

        // U07: Coba simulasikan waktu habis sesudahnya
        interaction.SetTimeRemaining(0f);
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.Success), "U07 Guard: Keberhasilan sah TIDAK BOLEH ditimpa oleh event waktu habis!");
    }

    [UnityTest]
    public IEnumerator OutcomeEvaluatedCorrectlyForPartialSuccess()
    {
        yield return null;

        // Antar hanya 1 warga (Warga Pos)
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return CloseDialogueViaEsc();

        Assert.That(interaction.DeliveredCount, Is.EqualTo(1));

        // Simulasikan waktu habis saat baru 1 warga terantar
        interaction.SetTimeRemaining(0f);

        // U08: Harus berstatus Berhasil Sebagian (PartialSuccess)
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.PartialSuccess), "Misi harus PartialSuccess jika 1 warga terbantu dan waktu habis.");
        Assert.That(interaction.OutcomeReason, Does.Contain("habis"));

        List<string> names = interaction.GetDeliveredRecipientNames();
        Assert.That(names, Contains.Item("Warga Pos"));
    }

    [UnityTest]
    public IEnumerator ResetMissionSessionCleansAllDataAndRestoresSpawn()
    {
        yield return null;

        // 1. Antar 1 paket
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        yield return CloseDialogueViaEsc();

        // 2. Pindahkan pemain jauh dari spawn
        TeleportPlayerTo(new Vector3(15f, 1f, 15f));
        yield return null;
        Assert.That(Vector3.Distance(player.position, Vector3.zero), Is.GreaterThan(10f));

        // 3. Reset sesi misi (U08 / U17)
        interaction.ResetMissionSession();
        Assert.That(interaction.TimeRemaining, Is.EqualTo(180f), "Waktu harus langsung kembali 180 detik saat reset.");
        yield return null;

        // 4. Verifikasi reset bersih
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3), "Paket harus kembali 3.");
        Assert.That(interaction.DeliveredCount, Is.EqualTo(0), "Delivered count harus 0.");
        Assert.That(interaction.Trust, Is.EqualTo(0), "Trust harus 0.");
        Assert.That(interaction.TimeRemaining, Is.GreaterThan(179f).And.LessThanOrEqualTo(180f), "Waktu harus berada di sekitar 180 detik.");
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.InProgress), "Status harus kembali InProgress.");
        Assert.That(interaction.IsOutcomeOpen, Is.False, "Layar hasil harus tertutup.");
        Assert.That(residentA.IsDelivered, Is.False, "Warga harus kembali belum menerima paket.");

        // Posisi pemain harus kembali ke titik spawn awal (0, 1, 0)
        Assert.That(Vector3.Distance(player.position, new Vector3(0f, 1f, 0f)), Is.LessThan(0.25f), "Posisi pemain harus kembali ke spawn awal.");
    }

    private void TeleportPlayerTo(Vector3 targetPos)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = targetPos;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    private IEnumerator CloseDialogueViaEsc()
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    private static GameObject UiObject(string name, Transform parent)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        return result;
    }

    private static void SetReference(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, "Field " + fieldName + " not found on " + target);
        field.SetValue(target, value);
    }
}
#endif
