#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests untuk U15 (dan integrasi U11–U14 ke scene Kampung &amp; Battle.unity):
/// masuk battle dari Kampung lewat opsi Bertengkar, kembali dengan timer/paket/trust/penerima tetap,
/// tidak ada kamera/AudioListener ganda, musuh selesai tidak muncul lagi, serta alur kalah -> retry bersih.
/// </summary>
public sealed class U15BattleIntegrationTests : InputTestFixture
{
    private const string KampungScenePath = "Assets/_Project/Scenes/Kampung.unity";
    private const string BattleScenePath = "Assets/_Project/Scenes/Battle.unity";

    private Keyboard keyboard;
    private float previousTimeScale;
    private Scene kampungScene;
    private PlayerInteraction interaction;
    private EncounterController encounter;
    private InteractionTarget firstResident;
    private BattleTransition transition;

    public override void Setup()
    {
        base.Setup();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        keyboard = InputSystem.AddDevice<Keyboard>();
    }

    public override void TearDown()
    {
        Time.timeScale = previousTimeScale;
        base.TearDown();
    }

    [UnityTearDown]
    public IEnumerator UnloadLoadedScenes()
    {
        Scene battleScene = SceneManager.GetSceneByPath(BattleScenePath);
        if (battleScene.IsValid() && battleScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(battleScene);

        if (kampungScene.IsValid() && kampungScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(kampungScene);

        interaction = null;
        encounter = null;
        firstResident = null;
        transition = null;
    }

    [UnityTest]
    public IEnumerator WinningAdditiveBattlePreservesMissionStateDisablesEnemyAndKeepsSingleAudioListener()
    {
        yield return LoadKampung();
        transition.SetPostBattleDelayForTests(0f);

        // 1. Antar 1 paket ke warga pertama sebelum masuk battle -> Paket 2, Terkirim 1, Trust 1
        TeleportPlayer(interaction, new Vector3(firstResident.transform.position.x, 1f, firstResident.transform.position.z - 1.5f));
        yield return null;
        yield return PressKey(Key.E);
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1));
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(2));
        Assert.That(interaction.Trust, Is.EqualTo(1));
        yield return PressKey(Key.Escape);

        // 2. Dekati EncounterPoint -> panel muncul -> pilih [2] Bertengkar
        TeleportPlayer(interaction, new Vector3(0f, 1f, -1.6f));
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.True, "Encounter harus terbuka saat mendekati preman.");

        float timeBeforeBattle = interaction.TimeRemaining;
        yield return PressKey(Key.Digit2);

        // Tunggu Battle.unity selesai di-load dan StartBattle() sudah berjalan
        float deadline = Time.realtimeSinceStartup + 5f;
        while ((BattleController.Active == null || !BattleController.Active.IsPlayerTurn) && Time.realtimeSinceStartup < deadline)
            yield return null;

        BattleController battle = BattleController.Active;
        Assert.That(battle, Is.Not.Null, "BattleController dari Battle.unity harus aktif setelah memilih Bertengkar.");
        Assert.That(transition.IsInBattle, Is.True);
        Assert.That(encounter.IsEncounterOpen, Is.False, "Panel encounter harus tertutup selama battle.");
        Assert.That(CountEnabledComponents<AudioListener>(), Is.EqualTo(1), "Hanya boleh ada tepat 1 AudioListener aktif saat battle.");
        Assert.That(CountEnabledComponents<Camera>(), Is.EqualTo(1), "Hanya boleh ada tepat 1 Camera aktif saat battle.");

        // Pastikan timer misi membeku selama battle
        yield return new WaitForSeconds(0.15f);
        Assert.That(interaction.TimeRemaining, Is.EqualTo(timeBeforeBattle), "Timer misi tidak boleh berkurang selama battle.");

        // 3. Menangkan battle sesuai urutan U11: Serang x3, Bertahan, Serang x2 -> sisa HP 4
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.PlayerDefend(), Is.True);
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.IsBattleOver, Is.True);
        Assert.That(battle.PlayerWon, Is.True);
        Assert.That(battle.PlayerHP, Is.EqualTo(4));

        // Tunggu Battle.unity di-unload dan Kampung aktif kembali
        deadline = Time.realtimeSinceStartup + 5f;
        while (transition.IsInBattle && Time.realtimeSinceStartup < deadline)
            yield return null;
        yield return null;

        Scene battleScene = SceneManager.GetSceneByPath(BattleScenePath);
        Assert.That(battleScene.isLoaded, Is.False, "Battle.unity harus sudah di-unload setelah selesai.");
        Assert.That(CountEnabledComponents<AudioListener>(), Is.EqualTo(1), "Hanya boleh ada tepat 1 AudioListener aktif setelah kembali ke Kampung.");
        Assert.That(CountEnabledComponents<Camera>(), Is.EqualTo(1), "Hanya boleh ada tepat 1 Camera aktif setelah kembali ke Kampung.");

        // Verifikasi U15: seluruh data misi tetap utuh
        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.InProgress));
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1));
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(2));
        Assert.That(interaction.Trust, Is.EqualTo(1));
        Assert.That(firstResident.IsDelivered, Is.True);
        Assert.That(interaction.TimeRemaining, Is.EqualTo(timeBeforeBattle).Within(0.1f));

        // Musuh & penghalang selesai untuk sisa sesi dan tidak memicu encounter ulang
        Assert.That(encounter.IsResolved, Is.True);
        Assert.That(encounter.ObstacleObject.activeSelf, Is.False);
        Assert.That(encounter.PremanVisual.activeSelf, Is.False);

        // Tutup dialog "Menang Battle!" lalu uji melewati posisi encounter
        if (interaction.IsDialogueOpen)
            yield return PressKey(Key.Escape);

        Assert.That(interaction.GetComponent<PlayerController>().enabled, Is.True, "Kendali eksplorasi harus kembali sesudah battle.");
        TeleportPlayer(interaction, Vector3.zero);
        yield return null;
        Assert.That(encounter.IsEncounterOpen, Is.False, "Musuh yang sudah dikalahkan tidak boleh muncul/memicu encounter lagi.");
    }

    [UnityTest]
    public IEnumerator LosingAdditiveBattleShowsPartialOutcomePreservesDeliveriesAndRetriesCleanly()
    {
        yield return LoadKampung();
        transition.SetPostBattleDelayForTests(0f);

        // Antar 1 bantuan terlebih dahulu -> harus jadi Berhasil Sebagian saat kalah battle
        TeleportPlayer(interaction, new Vector3(firstResident.transform.position.x, 1f, firstResident.transform.position.z - 1.5f));
        yield return null;
        yield return PressKey(Key.E);
        yield return PressKey(Key.Escape);

        // Masuk battle lewat Bertengkar
        TeleportPlayer(interaction, new Vector3(0f, 1f, -1.6f));
        yield return null;
        yield return PressKey(Key.Digit2);

        float deadline = Time.realtimeSinceStartup + 5f;
        while ((BattleController.Active == null || !BattleController.Active.IsPlayerTurn) && Time.realtimeSinceStartup < deadline)
            yield return null;

        BattleController battle = BattleController.Active;
        Assert.That(battle, Is.Not.Null);

        // Kalah sesuai U12: 4x Serang beruntun
        battle.PlayerAttack();
        battle.PlayerAttack();
        battle.PlayerAttack();
        battle.PlayerAttack();
        Assert.That(battle.IsBattleOver, Is.True);
        Assert.That(battle.PlayerWon, Is.False);

        deadline = Time.realtimeSinceStartup + 5f;
        while (transition.IsInBattle && Time.realtimeSinceStartup < deadline)
            yield return null;
        yield return null;

        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.PartialSuccess));
        Assert.That(interaction.OutcomeReason, Does.Contain("Kalah battle"));
        Assert.That(interaction.IsOutcomeOpen, Is.True);
        Assert.That(interaction.DeliveredCount, Is.EqualTo(1), "Pengantaran sebelum kalah battle tetap tercatat.");

        // Uji retry bersih sesudah kalah battle (U08 / U17)
        interaction.ResetMissionSession();
        yield return null;

        Assert.That(interaction.Outcome, Is.EqualTo(MissionOutcome.InProgress));
        Assert.That(interaction.IsOutcomeOpen, Is.False);
        Assert.That(interaction.DeliveredCount, Is.EqualTo(0));
        Assert.That(interaction.PackagesRemaining, Is.EqualTo(3));
        Assert.That(interaction.Trust, Is.EqualTo(0));
        Assert.That(interaction.TimeRemaining, Is.GreaterThan(179f).And.LessThanOrEqualTo(180f));
        Assert.That(firstResident.IsDelivered, Is.False);
        Assert.That(encounter.IsResolved, Is.False);
        Assert.That(encounter.HasBattleRequestPending, Is.False);
        Assert.That(encounter.ObstacleObject.activeSelf, Is.True);
        Assert.That(encounter.PremanVisual.activeSelf, Is.True);
        Assert.That(interaction.GetComponent<PlayerController>().enabled, Is.True);
    }

    private IEnumerator LoadKampung()
    {
        yield return SceneManager.LoadSceneAsync(KampungScenePath, LoadSceneMode.Additive);
        kampungScene = SceneManager.GetSceneByPath(KampungScenePath);

        interaction = null;
        encounter = null;
        firstResident = null;
        transition = null;

        foreach (GameObject go in kampungScene.GetRootGameObjects())
        {
            if (go.TryGetComponent(out PlayerInteraction candidateInteraction))
            {
                interaction = candidateInteraction;
                transition = go.GetComponent<BattleTransition>();
            }
            if (go.name == "U02_Graybox")
            {
                encounter = go.GetComponentInChildren<EncounterController>(true);
                firstResident = go.GetComponentInChildren<InteractionTarget>(true);
            }
        }

        Assert.That(interaction, Is.Not.Null);
        Assert.That(transition, Is.Not.Null, "Player di Kampung.unity harus memiliki komponen BattleTransition.");
        Assert.That(encounter, Is.Not.Null);
        Assert.That(firstResident, Is.Not.Null);
        yield return null;
    }

    private static void TeleportPlayer(PlayerInteraction interaction, Vector3 pos)
    {
        CharacterController cc = interaction.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        interaction.transform.position = pos;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    private IEnumerator PressKey(Key key)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    private static int CountEnabledComponents<T>() where T : Behaviour
    {
        T[] all = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int count = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].isActiveAndEnabled)
                count++;
        }
        return count;
    }
}
#endif

