using UnityEngine;
using JejakAsa;

// kartu "HARI 2 / 5" yang muncul bentar tiap ganti hari (abis tidur di rumah)
// ga perlu ditaruh di scene, kebikin sendiri pas game jalan
public class KartuHari : MonoBehaviour
{
    static KartuHari yangAda;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bikin()
    {
        if (yangAda != null) return;
        GameObject go = new GameObject("KartuHari");
        yangAda = go.AddComponent<KartuHari>();
        DontDestroyOnLoad(go);
    }

    int hariTadi = -1;
    int hariMuncul = 1;
    float waktuMuncul = -100f;
    float cekLagi = 0f;
    float lamaTampil = 3.2f;

    void Update()
    {
        // ngecek hari tiap setengah detik aja
        cekLagi -= Time.unscaledDeltaTime;
        if (cekLagi > 0) return;
        cekLagi = 0.5f;

        GameFlowManager gm = FindFirstObjectByType<GameFlowManager>();
        if (gm == null) return;

        int hari = gm.CurrentDay;
        if (hariTadi != -1 && hari > hariTadi)
        {
            // hari nambah -> munculin kartu
            hariMuncul = hari;
            waktuMuncul = Time.unscaledTime;
        }
        hariTadi = hari;
    }

    void OnGUI()
    {
        float lama = Time.unscaledTime - waktuMuncul;
        if (lama > lamaTampil) return;
        if (SessionMenu.InputBlocked) return;

        CozyGuiTheme.EnsureInitialized();
        GUI.depth = -900;

        // muncul pelan, ilang pelan
        float alpha = 1f;
        if (lama < 0.3f) alpha = lama / 0.3f;
        if (lama > lamaTampil - 0.6f) alpha = (lamaTampil - lama) / 0.6f;

        Matrix4x4 lamaMatrix = GUI.matrix;
        float skala = CozyGuiTheme.Scale;
        GUI.matrix = lamaMatrix * Matrix4x4.Scale(new Vector3(skala, skala, 1));
        float lebarLayar = Screen.width / skala;
        float tinggiLayar = Screen.height / skala;

        Color warnaLama = GUI.color;
        GUI.color = new Color(1, 1, 1, alpha);

        // pita gelap tipis di tengah layar
        CozyGuiTheme.Fill(new Rect(0, tinggiLayar / 2 - 120, lebarLayar, 240), new Color(0.10f, 0.07f, 0.04f, 0.55f * alpha));

        Rect kartu = new Rect((lebarLayar - 460) / 2, tinggiLayar / 2 - 100 + (1 - alpha) * 12, 460, 200);
        CozyGuiTheme.Card(kartu);

        GUIStyle gayaBesar = new GUIStyle(CozyGuiTheme.Title);
        gayaBesar.fontSize = 52;
        gayaBesar.alignment = TextAnchor.MiddleCenter;
        gayaBesar.normal.textColor = CozyGuiTheme.Green;
        GUI.Label(new Rect(kartu.x, kartu.y + 18, kartu.width, 64), "HARI " + hariMuncul, gayaBesar);

        GUIStyle gayaKecil = new GUIStyle(CozyGuiTheme.Small);
        gayaKecil.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(kartu.x, kartu.y + 80, kartu.width, 24), "dari 5 hari perjalanan", gayaKecil);

        // titik2 hari (pake gaya kotak nomor)
        float mulaiX = kartu.x + (kartu.width - (5 * 44 - 12)) / 2;
        for (int i = 0; i < 5; i++)
        {
            Rect kotak = new Rect(mulaiX + i * 44, kartu.y + 120, 32, 32);
            Color warnaKotak = CozyGuiTheme.Linen;
            if (i + 1 < hariMuncul) warnaKotak = CozyGuiTheme.Green;
            if (i + 1 == hariMuncul) warnaKotak = CozyGuiTheme.Clay;
            CozyGuiTheme.Fill(new Rect(kotak.x, kotak.y + 4, kotak.width, kotak.height), new Color(0, 0, 0, 0.18f * alpha));
            warnaKotak.a = alpha;
            CozyGuiTheme.Fill(kotak, warnaKotak);
            GUIStyle angka = new GUIStyle(CozyGuiTheme.ButtonLabel);
            angka.alignment = TextAnchor.MiddleCenter;
            angka.fontSize = 18;
            angka.normal.textColor = (i + 1 <= hariMuncul) ? CozyGuiTheme.Paper : CozyGuiTheme.Muted;
            GUI.Label(kotak, (i + 1).ToString(), angka);
        }

        GUI.color = warnaLama;
        GUI.matrix = lamaMatrix;
    }
}
