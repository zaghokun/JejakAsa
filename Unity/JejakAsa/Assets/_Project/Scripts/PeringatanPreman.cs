using UnityEngine;
using JejakAsa;

// peringatan kalo preman lagi deket + tulisan "-10 detik" kalo ketangkep
// cuma NGEBACA posisi preman & sisa waktu, ga ngubah PremanController
public class PeringatanPreman : MonoBehaviour
{
    static PeringatanPreman yangAda;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bikin()
    {
        if (yangAda != null) return;
        GameObject go = new GameObject("PeringatanPreman");
        yangAda = go.AddComponent<PeringatanPreman>();
        DontDestroyOnLoad(go);
    }

    public float jarakAwas = 7f; // samain sama detectionRadius preman

    PlayerInteraction pemain;
    PremanController[] premanPreman = new PremanController[0];
    float cariLagi = 0f;
    float waktuTadi = -1f;
    float waktuKetangkep = -100f;
    float detikHilang = 0f;
    bool lagiDeket = false;

    void Update()
    {
        cariLagi -= Time.unscaledDeltaTime;
        if (cariLagi <= 0)
        {
            cariLagi = 1f;
            pemain = FindFirstObjectByType<PlayerInteraction>();
            premanPreman = FindObjectsByType<PremanController>(FindObjectsSortMode.None);
        }

        lagiDeket = false;
        if (pemain == null || !pemain.IsMissionActive)
        {
            waktuTadi = -1f;
            return;
        }

        // cek preman paling deket
        for (int i = 0; i < premanPreman.Length; i++)
        {
            if (premanPreman[i] == null || !premanPreman[i].isActiveAndEnabled) continue;
            float jarak = Vector3.Distance(premanPreman[i].transform.position, pemain.transform.position);
            if (jarak < jarakAwas) lagiDeket = true;
        }

        // kalo waktunya tiba2 kepotong banyak berarti kena preman
        float sekarang = pemain.TimeRemaining;
        if (waktuTadi > 0 && waktuTadi - sekarang >= 5f)
        {
            detikHilang = Mathf.Round(waktuTadi - sekarang);
            waktuKetangkep = Time.unscaledTime;
        }
        waktuTadi = sekarang;
    }

    void OnGUI()
    {
        if (pemain == null) return;
        if (SessionMenu.InputBlocked || pemain.IsDialogueOpen || pemain.IsOutcomeOpen) return;
        bool barusKetangkep = Time.unscaledTime - waktuKetangkep < 2.2f;
        if (!lagiDeket && !barusKetangkep) return;

        CozyGuiTheme.EnsureInitialized();
        GUI.depth = -60;
        Matrix4x4 lamaMatrix = GUI.matrix;
        float skala = CozyGuiTheme.Scale;
        GUI.matrix = lamaMatrix * Matrix4x4.Scale(new Vector3(skala, skala, 1));
        float lebarLayar = Screen.width / skala;
        float tinggiLayar = Screen.height / skala;

        if (barusKetangkep)
        {
            // pinggiran layar merah dikit
            float a = 1f - (Time.unscaledTime - waktuKetangkep) / 2.2f;
            Color merah = new Color(0.77f, 0.33f, 0.23f, 0.35f * a);
            CozyGuiTheme.Fill(new Rect(0, 0, lebarLayar, 14), merah);
            CozyGuiTheme.Fill(new Rect(0, tinggiLayar - 14, lebarLayar, 14), merah);
            CozyGuiTheme.Fill(new Rect(0, 0, 14, tinggiLayar), merah);
            CozyGuiTheme.Fill(new Rect(lebarLayar - 14, 0, 14, tinggiLayar), merah);

            Rect kotak = new Rect((lebarLayar - 420) / 2, 120, 420, 74);
            CozyGuiTheme.Card(kotak);
            GUIStyle besar = new GUIStyle(CozyGuiTheme.Title);
            besar.fontSize = 30;
            besar.alignment = TextAnchor.MiddleCenter;
            besar.normal.textColor = CozyGuiTheme.Clay;
            GUI.Label(new Rect(kotak.x, kotak.y + 6, kotak.width, 36), "-" + detikHilang + " DETIK!", besar);
            GUIStyle kecil = new GUIStyle(CozyGuiTheme.Small);
            kecil.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(kotak.x, kotak.y + 40, kotak.width, 20), "Kamu kena hadang preman. Cari jalan lain!", kecil);
        }
        else
        {
            // banner kecil "awas" kedip2
            float kedip = 0.75f + Mathf.Sin(Time.unscaledTime * 6f) * 0.25f;
            Rect banner = new Rect((lebarLayar - 380) / 2, 40, 380, 52);
            Color lama = GUI.color;
            GUI.color = new Color(1, 1, 1, kedip);
            CozyGuiTheme.Card(banner);
            GUI.color = lama;
            CozyGuiTheme.Ikon(new Rect(banner.x + 18, banner.y + 12, 22, 22), "awas", CozyGuiTheme.Clay);
            GUIStyle tulisan = new GUIStyle(CozyGuiTheme.LabelStyle);
            tulisan.normal.textColor = CozyGuiTheme.Clay;
            GUI.Label(new Rect(banner.x + 50, banner.y + 4, banner.width - 60, 36), "Awas! Preman mendekat", tulisan);
        }

        GUI.matrix = lamaMatrix;
    }
}
