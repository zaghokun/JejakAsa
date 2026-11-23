using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using JejakAsa;

// script tampilan menu utama (gaya hybrid: menu kampung + aksen pixel)
// logika menunya (pindah halaman, keyboard, save/load) tetep di UI_MenuSesi,
// script ini cuma nampilin + nerusin klik mouse ke UI_MenuSesi
public class UI_MenuUtama : MonoBehaviour
{
    public UI_MenuSesi sesi;

    // 5 tombol (dipake gantian tiap halaman)
    public Button[] tombol;
    public Text[] tulisanTombol;
    public Image[] lencana;      // kotak nomor
    public Text[] tulisanNomor;
    public Image[] ikon;         // ikon pixel di kanan tombol

    public Text tulisanPapan;    // papan miring di bawah logo
    public Text tulisanKeterangan;
    public Text tulisanPesan;
    public RectTransform penanda; // berlian yg naik turun

    public Sprite gambarKrem;
    public Sprite gambarTeal;
    public Sprite gambarTekan;

    // urutan ikon: 0 play, 1 lanjut, 2 muat, 3 gear, 4 pintu, 5 balik, 6 layar
    public Sprite[] gambarIkon;

    // warna aksen tiap tombol biar ga monoton
    // 0 kuning kunyit, 1 hijau daun, 2 ungu, 3 biru, 4 merah bata, 5 coklat (kembali)
    public Color[] warnaAksen;

    float[] posisiAwalX;
    Vector2 posisiMouseTadi;
    Color warnaKrem = new Color(0.98f, 0.96f, 0.90f);
    Color warnaTinta = new Color(0.27f, 0.20f, 0.16f);

    void Start()
    {
        if (sesi == null)
        {
            sesi = FindFirstObjectByType<UI_MenuSesi>();
        }
        sesi.pakaiMenuBaru = true; // biar menu OnGUI yg lama ga ikut kegambar

        // simpen posisi x tombol, soalnya yg kepilih digeser dikit ke kanan
        posisiAwalX = new float[tombol.Length];
        for (int i = 0; i < tombol.Length; i++)
        {
            posisiAwalX[i] = tombol[i].GetComponent<RectTransform>().anchoredPosition.x;
        }
    }

    void Update()
    {
        if (sesi == null) return;

        string halaman = sesi.HalamanSekarang;
        int jumlah = sesi.JumlahPilihan;
        int dipilih = sesi.PilihanSekarang;

        // ===== papan miring =====
        if (halaman == "Main") tulisanPapan.text = "Bantuan sampai, harapan tumbuh.";
        else if (halaman == "Load") tulisanPapan.text = "Muat permainan";
        else if (halaman == "Settings") tulisanPapan.text = "Pengaturan";
        else if (halaman == "ConfirmNew") tulisanPapan.text = "Mulai dari awal?";
        else if (halaman == "ConfirmQuit") tulisanPapan.text = "Keluar dari game?";
        else if (halaman == "ConfirmLoad") tulisanPapan.text = "Muat slot ini?";
        else tulisanPapan.text = "";

        tulisanKeterangan.text = sesi.KeteranganHalaman;
        tulisanPesan.text = sesi.MenuNotice;

        // ===== tombol2 =====
        for (int i = 0; i < tombol.Length; i++)
        {
            if (i >= jumlah)
            {
                tombol[i].gameObject.SetActive(false);
                continue;
            }
            tombol[i].gameObject.SetActive(true);

            string label = sesi.LabelPilihan(i);
            label = label.Replace("     ", "  "); // spasinya kebanyakan
            tulisanTombol[i].text = label;
            tulisanNomor[i].text = (i + 1).ToString();

            int aksen = WarnaTombol(halaman, i);
            lencana[i].color = warnaAksen[aksen];
            ikon[i].sprite = gambarIkon[IkonTombol(halaman, i)];

            Image gambar = tombol[i].GetComponent<Image>();
            RectTransform rt = tombol[i].GetComponent<RectTransform>();
            if (i == dipilih)
            {
                gambar.sprite = gambarTeal;
                tulisanTombol[i].color = warnaKrem;
                ikon[i].color = warnaKrem;
                // geser dikit ke kanan
                float x = Mathf.Lerp(rt.anchoredPosition.x, posisiAwalX[i] + 14, Time.unscaledDeltaTime * 14);
                rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
            }
            else
            {
                gambar.sprite = gambarKrem;
                tulisanTombol[i].color = warnaTinta;
                ikon[i].color = warnaAksen[aksen];
                float x = Mathf.Lerp(rt.anchoredPosition.x, posisiAwalX[i], Time.unscaledDeltaTime * 14);
                rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
            }

            SpriteState ss = tombol[i].spriteState;
            ss.highlightedSprite = gambarTeal;
            ss.selectedSprite = gambarTeal;
            ss.pressedSprite = gambarTekan;
            tombol[i].spriteState = ss;
        }

        // ===== mouse =====
        // mouse digeser ke atas tombol -> tombolnya jadi kepilih
        if (Mouse.current != null)
        {
            Vector2 posMouse = Mouse.current.position.ReadValue();
            if (posMouse != posisiMouseTadi)
            {
                for (int i = 0; i < jumlah && i < tombol.Length; i++)
                {
                    RectTransform rt = tombol[i].GetComponent<RectTransform>();
                    if (RectTransformUtility.RectangleContainsScreenPoint(rt, posMouse, null))
                    {
                        sesi.SorotPilihan(i);
                    }
                }
            }
            posisiMouseTadi = posMouse;
        }

        // jangan biarin EventSystem nyimpen tombol kepilih,
        // soalnya Enter udah diurus UI_MenuSesi (nanti kepencet 2x)
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // ===== berlian penanda =====
        if (dipilih >= 0 && dipilih < jumlah && dipilih < tombol.Length)
        {
            RectTransform rtDipilih = tombol[dipilih].GetComponent<RectTransform>();
            Vector3[] pojok = new Vector3[4];
            rtDipilih.GetWorldCorners(pojok);
            float tengahY = (pojok[0].y + pojok[1].y) / 2 + 4;
            float naikTurun = Mathf.Sin(Time.unscaledTime * 4) * 4;
            penanda.position = new Vector3(pojok[0].x - 26, tengahY + naikTurun, 0);
            penanda.gameObject.SetActive(true);
        }
        else
        {
            penanda.gameObject.SetActive(false);
        }
    }

    // dipanggil dari OnClick tombol (angkanya diisi di inspector)
    public void KlikTombol(int nomor)
    {
        Debug.Log("klik tombol " + nomor);
        sesi.PilihDariTombol(nomor);
    }

    // warna aksen tombol tergantung halaman
    int WarnaTombol(string halaman, int i)
    {
        if (halaman == "Main") return i;
        if (halaman == "Load")
        {
            if (i == 3) return 5;
            return 2;
        }
        if (halaman == "Settings")
        {
            if (i == 3) return 5;
            return 3;
        }
        if (halaman == "ConfirmNew")
        {
            if (i == 0) return 5;
            return 0;
        }
        if (halaman == "ConfirmQuit")
        {
            if (i == 0) return 5;
            return 4;
        }
        if (halaman == "ConfirmLoad")
        {
            if (i == 0) return 5;
            return 2;
        }
        return 3;
    }

    // ikon tombol tergantung halaman
    int IkonTombol(string halaman, int i)
    {
        if (halaman == "Main") return i; // play, lanjut, muat, gear, pintu
        if (halaman == "Load")
        {
            if (i == 3) return 5;
            return 2;
        }
        if (halaman == "Settings")
        {
            if (i == 2) return 6;
            if (i == 3) return 5;
            return 3;
        }
        if (i == 0) return 5; // halaman konfirmasi: tombol pertama selalu "Kembali"
        if (halaman == "ConfirmNew") return 0;
        if (halaman == "ConfirmQuit") return 4;
        return 2;
    }
}
