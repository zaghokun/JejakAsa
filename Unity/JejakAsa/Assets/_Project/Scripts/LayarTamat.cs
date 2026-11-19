using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using JejakAsa;

// layar penutup pas kelima hari udah selesai (fase Tamat)
public class LayarTamat : MonoBehaviour
{
    static LayarTamat yangAda;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bikin()
    {
        if (yangAda != null) return;
        GameObject go = new GameObject("LayarTamat");
        yangAda = go.AddComponent<LayarTamat>();
        DontDestroyOnLoad(go);
    }

    bool lagiBuka = false;
    bool udahDitutup = false;
    float waktuBuka;
    string sceneTadi = "";

    void Update()
    {
        // kalo pindah scene, boleh muncul lagi
        string scene = SceneManager.GetActiveScene().path;
        if (scene != sceneTadi)
        {
            sceneTadi = scene;
            udahDitutup = false;
            lagiBuka = false;
        }

        GameFlowManager gm = FindFirstObjectByType<GameFlowManager>();
        if (gm == null) return;
        if (scene.Contains("MainMenu")) return;

        if (!lagiBuka && !udahDitutup && gm.CurrentPhase == DayPhase.Tamat)
        {
            // tunggu menu lain ketutup dulu
            if (!HomeInteractionController.AnyMenuOpen && !StoryNpcController.AnyDialogueOpen && !SessionMenu.InputBlocked)
            {
                lagiBuka = true;
                waktuBuka = Time.unscaledTime;
            }
        }

        if (lagiBuka && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) KeMenuUtama();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Tutup();
        }
    }

    void KeMenuUtama()
    {
        lagiBuka = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Assets/_Project/Scenes/MainMenu.unity");
    }

    void Tutup()
    {
        lagiBuka = false;
        udahDitutup = true;
    }

    void OnGUI()
    {
        if (!lagiBuka || SessionMenu.InputBlocked) return;

        CozyGuiTheme.EnsureInitialized();
        GUI.depth = -950;
        Matrix4x4 lamaMatrix = GUI.matrix;
        float skala = CozyGuiTheme.Scale;
        GUI.matrix = lamaMatrix * Matrix4x4.Scale(new Vector3(skala, skala, 1));
        float lebarLayar = Screen.width / skala;
        float tinggiLayar = Screen.height / skala;

        float muncul = Mathf.Clamp01((Time.unscaledTime - waktuBuka) / 0.4f);
        GUI.DrawTexture(new Rect(0, 0, lebarLayar, tinggiLayar), CozyGuiTheme.DimTex);

        Rect kartu = new Rect((lebarLayar - 660) / 2, (tinggiLayar - 560) / 2 + (1 - muncul) * 16, 660, 560);
        CozyGuiTheme.Header(kartu, "PERJALANAN SELESAI", "Lima hari, lima jejak harapan.", "Bantuan sudah sampai ke warga kampung. Terima kasih sudah ikut melangkah bersama Vicky.");

        // hari 1-5 semua udah selesai
        CozyGuiTheme.Journey(new Rect(kartu.x + 56, kartu.y + 214, kartu.width - 112, 100), 5, 5);

        // hati2 kecil
        for (int i = 0; i < 3; i++)
        {
            CozyGuiTheme.Ikon(new Rect(kartu.x + kartu.width / 2 - 42 + i * 30, kartu.y + 330, 20, 20), "hati", CozyGuiTheme.Clay);
        }

        GUIStyle kredit = new GUIStyle(CozyGuiTheme.Small);
        kredit.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(kartu.x, kartu.y + 360, kartu.width, 22), "Jejak Asa  -  Kelompok 5  -  Grafika Komputer", kredit);

        if (CozyGuiTheme.Button(new Rect(kartu.x + 56, kartu.y + 404, kartu.width - 112, 54), "1. Kembali ke menu utama", CozyGuiTheme.TealBtnStyle)) KeMenuUtama();
        if (CozyGuiTheme.Button(new Rect(kartu.x + 56, kartu.y + 468, kartu.width - 112, 46), "2. Lihat-lihat rumah dulu", CozyGuiTheme.AmberBtnStyle)) Tutup();
        GUI.Label(new Rect(kartu.x, kartu.y + 522, kartu.width, 20), "1 / Enter  Menu utama     2  Tutup", CozyGuiTheme.HintStyle);

        GUI.matrix = lamaMatrix;
    }
}
