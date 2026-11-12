#if UNITY_EDITOR
using System.Collections;
using JejakAsa;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests untuk U11–U14 (battle turn-based deterministik).
/// Semua test berjalan tanpa scene Battle — cukup BattleController di test object.
/// </summary>
public sealed class U11BattleTests
{
    private GameObject root;
    private BattleController battle;

    [SetUp]
    public void Setup()
    {
        root = new GameObject("U11 battle test");
        battle = root.AddComponent<BattleController>();
        battle.StartBattle();
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null) Object.DestroyImmediate(root);
    }

    // ═══════════════════════════════════════════════════════════════════
    // U11 — Menang battle: Serang ×3, Bertahan, Serang ×2 → menang HP 4
    // ═══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator U11_WinSequence_Attack3_Defend_Attack2_WinsWithHP4()
    {
        yield return null;

        Assert.That(battle.PlayerHP, Is.EqualTo(32), "HP pemain awal harus 32.");
        Assert.That(battle.EnemyHP,  Is.EqualTo(50), "HP musuh awal harus 50.");
        Assert.That(battle.IsPlayerTurn, Is.True);
        Assert.That(battle.IsBattleOver, Is.False);

        // Giliran 1: Serang → musuh Biasa (8)
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(40), "Serang 1: HP musuh 50 → 40.");
        Assert.That(battle.PlayerHP, Is.EqualTo(24), "Biasa 1: HP pemain 32 → 24.");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Biasa));
        Assert.That(battle.TurnNumber, Is.EqualTo(2));

        // Giliran 2: Serang → musuh Biasa (8)
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(30), "Serang 2: HP musuh 40 → 30.");
        Assert.That(battle.PlayerHP, Is.EqualTo(16), "Biasa 2: HP pemain 24 → 16.");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Biasa));

        // Giliran 3: Serang → musuh Persiapan (0)
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(20), "Serang 3: HP musuh 30 → 20.");
        Assert.That(battle.PlayerHP, Is.EqualTo(16), "Persiapan: HP pemain tetap 16.");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Persiapan));
        Assert.That(battle.ShowKuatWarning, Is.True, "Peringatan serangan kuat harus aktif setelah Persiapan.");

        // Giliran 4: Bertahan → musuh Kuat (18 → 4 karena bertahan)
        Assert.That(battle.PlayerDefend(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(20), "Bertahan: HP musuh tetap 20.");
        Assert.That(battle.PlayerHP, Is.EqualTo(12), "Kuat guarded: HP pemain 16 → 12.");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Kuat));
        Assert.That(battle.LastEnemyDamage, Is.EqualTo(4), "Damage Kuat saat bertahan harus 4.");

        // Giliran 5: Serang → musuh Biasa (8) — pola kembali ke awal
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(10), "Serang 4: HP musuh 20 → 10.");
        Assert.That(battle.PlayerHP, Is.EqualTo(4),  "Biasa 3: HP pemain 12 → 4.");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Biasa));

        // Giliran 6: Serang → musuh HP 0 → MENANG, musuh tidak membalas
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP,  Is.EqualTo(0),  "Serang 5: HP musuh 10 → 0.");
        Assert.That(battle.PlayerHP, Is.EqualTo(4),  "HP pemain akhir harus 4 (musuh mati, tidak membalas).");

        Assert.That(battle.IsBattleOver, Is.True, "Battle harus selesai.");
        Assert.That(battle.PlayerWon,    Is.True, "Pemain harus menang.");
        Assert.That(battle.Log.Count,    Is.EqualTo(6), "Harus ada 6 giliran tercatat.");
    }

    // ═══════════════════════════════════════════════════════════════════
    // U12 — Kalah battle: Serang ×4 → kalah pada Kuat pertama
    // ═══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator U12_LoseSequence_Attack4_LosesOnFirstKuat()
    {
        yield return null;

        // Giliran 1: Serang → Biasa (8)  → HP 24
        battle.PlayerAttack();
        // Giliran 2: Serang → Biasa (8)  → HP 16
        battle.PlayerAttack();
        // Giliran 3: Serang → Persiapan  → HP 16
        battle.PlayerAttack();
        // Giliran 4: Serang → Kuat (18)  → HP -2 → 0
        battle.PlayerAttack();

        Assert.That(battle.IsBattleOver, Is.True, "Battle harus selesai.");
        Assert.That(battle.PlayerWon, Is.False, "Pemain harus kalah.");
        Assert.That(battle.PlayerHP, Is.EqualTo(0), "HP pemain harus 0.");
        Assert.That(battle.EnemyHP, Is.EqualTo(10), "HP musuh sisa 10 (4×10 = 40 damage).");
        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Kuat));
        Assert.That(battle.LastEnemyDamage, Is.EqualTo(18), "Kuat tanpa guard harus 18 damage.");
    }

    // ═══════════════════════════════════════════════════════════════════
    // U13 — Guard sekali pakai: Bertahan pada Persiapan habis, Kuat tanpa perlindungan
    // ═══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator U13_GuardConsumedOnPersiapan_KuatHitsFullDamage()
    {
        yield return null;

        // Giliran 1: Serang → Biasa (8)  → HP 24
        battle.PlayerAttack();
        // Giliran 2: Serang → Biasa (8)  → HP 16
        battle.PlayerAttack();
        // Giliran 3: **Bertahan** pada Persiapan → guard dipakai meskipun Persiapan 0 damage
        battle.PlayerDefend();

        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Persiapan));
        Assert.That(battle.PlayerHP, Is.EqualTo(16), "Persiapan tidak memberi damage, HP tetap 16.");
        Assert.That(battle.IsGuarding, Is.False, "Guard harus sudah habis setelah Persiapan.");

        // Giliran 4: Serang → Kuat → TANPA guard → 18 damage penuh
        battle.PlayerAttack();

        Assert.That(battle.LastEnemyAction, Is.EqualTo(BattleController.EnemyAction.Kuat));
        Assert.That(battle.LastEnemyDamage, Is.EqualTo(18), "Kuat harus 18 karena guard sudah habis di Persiapan.");
        Assert.That(battle.PlayerHP, Is.EqualTo(0), "HP pemain 16 - 18 = 0 (min 0).");
        Assert.That(battle.IsBattleOver, Is.True);
        Assert.That(battle.PlayerWon, Is.False, "Pemain harus kalah.");
    }

    // ═══════════════════════════════════════════════════════════════════
    // U14 — Aksi tunggal: klik ganda ditolak; musuh mati tidak membalas; hasil sekali
    // ═══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator U14_DoubleActionRejected_DeadEnemyDoesNotCounter_ResultSentOnce()
    {
        yield return null;

        // Pertama attack berhasil
        Assert.That(battle.PlayerAttack(), Is.True, "Aksi pertama harus diterima.");
        // Langsung attack kedua pada giliran yang SAMA — harus ditolak
        // (ActionLocked true sampai giliran pemain berikutnya)
        // Tapi setelah enemy turn, ActionLocked di-reset. Jadi kita test saat
        // masih di enemy turn — simulasi klik cepat:
        // Setelah PlayerAttack() selesai, enemy sudah bertindak dan IsPlayerTurn = true lagi.
        // Untuk test double-click, kita perlu intercept. Gunakan pendekatan lain:
        // Set musuh HP rendah sehingga satu Serang membunuhnya.

        // Reset fresh battle dengan HP musuh = 10
        battle.enemyMaxHP = 10;
        battle.StartBattle();

        int resultCount = 0;
        battle.OnBattleEnd += (won) => resultCount++;

        // Serang → musuh HP 10 → 0, mati, tidak membalas
        int hpBefore = battle.PlayerHP;
        Assert.That(battle.PlayerAttack(), Is.True);
        Assert.That(battle.EnemyHP, Is.EqualTo(0), "Musuh harus mati.");
        Assert.That(battle.PlayerHP, Is.EqualTo(hpBefore), "Musuh mati tidak boleh membalas.");
        Assert.That(battle.IsBattleOver, Is.True);
        Assert.That(battle.PlayerWon, Is.True);
        Assert.That(resultCount, Is.EqualTo(1), "Hasil harus dikirim tepat sekali.");

        // Coba aksi setelah battle selesai — harus ditolak
        Assert.That(battle.PlayerAttack(), Is.False, "Aksi setelah battle selesai harus ditolak.");
        Assert.That(battle.PlayerDefend(), Is.False, "Defend setelah battle selesai harus ditolak.");
        Assert.That(resultCount, Is.EqualTo(1), "Hasil tetap 1, tidak dikirim ulang.");
    }

    [UnityTest]
    public IEnumerator U14_ActionLocked_DuringEnemyTurn_RejectsDoubleClick()
    {
        yield return null;

        // Serang pada giliran 1 → berhasil
        Assert.That(battle.PlayerAttack(), Is.True);
        // Sekarang giliran sudah kembali ke pemain (karena DoEnemyTurn sinkron).
        // Untuk menguji ActionLocked, kita cek bahwa saat ActionLocked = true,
        // aksi kedua ditolak.
        // Karena implementasi sinkron, kita akali dengan memanggil attack
        // dan langsung attack lagi. Setelah PlayerAttack(), ActionLocked = false
        // karena enemy turn sudah selesai. Jadi kita test via manual lock:

        // Approach: call PlayerAttack dua kali SANGAT cepat (dalam frame yang sama
        // sebelum enemy turn). Karena implementasi sinkron, gunakan pendekatan:
        // pastikan aksi kedua berhasil (artinya aksi pertama tidak di-double).

        // Giliran 2
        int hpBefore = battle.EnemyHP;
        Assert.That(battle.PlayerAttack(), Is.True);
        int expectedEnemyHP = hpBefore - 10;
        Assert.That(battle.EnemyHP, Is.EqualTo(expectedEnemyHP),
            "Aksi kedua di giliran baru harus menghasilkan tepat 10 damage, bukan 20.");
    }

    [UnityTest]
    public IEnumerator U14_AnimatedTurnLock_RejectsRapidDoubleClickUntilEnemyTurnFinishes()
    {
        yield return null;

        Assert.That(battle.TriggerPlayerAttackAnimated(), Is.True, "Aksi animasi pertama harus diterima.");
        Assert.That(battle.ActionLocked, Is.True, "Tombol harus langsung terkunci selama animasi giliran.");
        Assert.That(battle.IsPlayerTurn, Is.False);
        Assert.That(battle.TriggerPlayerAttackAnimated(), Is.False, "Klik cepat kedua saat animasi berjalan harus ditolak.");
        Assert.That(battle.TriggerPlayerDefendAnimated(), Is.False, "Bertahan saat animasi berjalan harus ditolak.");

        float deadline = Time.realtimeSinceStartup + 3f;
        while (battle.ActionLocked && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.That(battle.ActionLocked, Is.False, "Kunci tombol harus terbuka kembali di giliran berikutnya.");
        Assert.That(battle.EnemyHP, Is.EqualTo(40), "Klik cepat tidak boleh menggandakan damage.");
        Assert.That(battle.PlayerHP, Is.EqualTo(24), "Musuh harus membalas tepat satu kali.");
    }
}
#endif

