# R01 — Kontrak Alur dan Arsitektur Siklus Harian (AB-D01)

**Status:** Kesepakatan Kontrak Alur A/B/C  
**Terkait:** [README.md](../README.md#L98) (R01, AB-D01), [storyboard-dialog-jejak-asa.md](storyboard-dialog-jejak-asa.md), [cd01-arah-visual.md](cd01-arah-visual.md)

---

## 1. Ringkasan Siklus Harian (Daily Loop)

Game Jejak Asa beralih dari sekadar graybox terisolasi menjadi **siklus hidup pemukiman yang kohesif ala Citampi Stories**:
1. **Interior Rumah** (`Rumah.unity`): Tempat bangun tidur, save game, interaksi orang tua, dan istirahat malam.
2. **Posko lapangan** (`Posko.unity`): Pemain tiba dari rumah melalui gerbang bersama, mengikuti briefing Pak Jonathan dan mengambil paket bantuan sebelum berangkat.
3. **Map Penyaluran / Misi Level** (`Kampung.unity`, `Kampung_L2.unity`–`Kampung_L5.unity`): Lima layout graybox untuk pengantaran bantuan.
4. **Kembali ke Posko dan Rumah**: Setelah penyaluran berhasil, pemain kembali ke posko untuk laporan Jonathan dan dialog Ophelia, lalu menggunakan gerbang yang sama untuk pulang ke interior rumah.

```mermaid
flowchart TD
    Prolog["Prolog (Permainan Baru)"] --> RumahPagi["Rumah (Pagi: Hari 1..5)"]
    RumahPagi -->|"Gerbang Rumah (Hanya ke Posko)"| PoskoBriefing["Posko (Pagi: Briefing Jonathan & Ophelia)"]
    PoskoBriefing -->|"Gerbang Posko: Berangkat ke Level"| DiLevel["Map Level Aktif (Antar Bansos)"]
    DiLevel -->|"Waktu Habis"| RetryLevel["Retry Level Aktif (Progres Hari Sebelumnya Tetap Aman)"]
    RetryLevel --> DiLevel
    DiLevel -->|"Misi Sukses (Semua Bansos)"| PoskoLaporan["Posko (Sore: Lapor Jonathan, Dialog Ophelia)"]
    PoskoLaporan -->|"Gerbang Posko: Pulang ke Rumah"| RumahMalam["Rumah (Malam: Bicara Ortu, Save Game Slot 1-3, Tidur)"]
    RumahMalam -->|"Istirahat Tidur (Hari = Hari + 1)"| RumahPagi
    RumahMalam -->|"Level 5 Selesai: Ending"| Tamat["Penutup Cerita (Tamat)"]
```

---

## 2. Definisi State Siklus Harian (`DayPhase`)

| State (`DayPhase`) | Lokasi Scene | Aksi Wajib Pemain | Akses Gerbang Keluar |
|---|---|---|---|
| `Prolog` | `Rumah` / Cutscene | Pengenalan Vicky, Ibu, dan ide posko | Hanya menuju Posko |
| `RumahPagi` | `Rumah.unity` | Bangun tidur, bicara orang tua (opsional) | Hanya menuju Posko (`Pulang` tidak relevan) |
| `PoskoBriefing` | `Posko.unity` | Bicara dengan Pak Jonathan dan ambil paket bantuan | Gerbang Level **terkunci** sampai briefing dan pengambilan paket selesai. Gerbang Rumah terkunci. |
| `DiLevel` | Lima scene level | Selesaikan 3 pengantaran bansos | Setelah hasil sukses dikonfirmasi, kembali ke Posko |
| `PoskoLaporan` | `Posko.unity` | Lapor hasil ke Jonathan, dialog Ophelia | Gerbang Rumah terbuka setelah lapor & dialog Ophelia selesai |
| `RumahMalam` | `Rumah.unity` | Simpan progres (Save Slot 1–3), istirahat/tidur | Pilihan tidur memajukan hari (`Hari++`) |
| `Tamat` | `Rumah.unity` | Penutup cerita dan kembali ke Menu Utama | Selesai |

---

## 3. Kontrak Data Persisten (`GameProgressData`)

Data yang dipersistensikan dan tidak boleh hilang saat transisi antar scene:
- **`currentDay`** (int 1..5): Hari aktif.
- **`currentLevel`** (int 1..5): Level aktif yang sedang dijalankan.
- **`currentPhase`** (`DayPhase`): Tahap alur harian aktif.
- **`isBriefingDone`** (bool): Menandai pemain sudah briefing dengan Jonathan di hari tersebut.
- **`isLevelCompleted`** (bool): Menandai misi level hari itu telah berhasil diselesaikan.
- **`isReportDone`** (bool): Menandai laporan misi ke Jonathan telah selesai.
- **`isOpheliaTalkDone`** (bool): Menandai dialog lanjutan dengan Ophelia telah selesai.
- **`hasClaimedReward`**, **`inventory`**, dan **`equippedWeapon`**: field save lama dipertahankan hanya agar data pengguna tidak hilang; tidak menyediakan fungsi reward, item, atau senjata.
- **`completedLevels`** (`List<int>`): Riwayat level yang telah diselesaikan.

---

## 4. Keputusan Terbuka yang Disepakati (AB-D01)

1. **Sistem Simpan / Muat (Save/Load):**
   - Disediakan **3 slot simpanan**.
   - Simpanan dilakukan di **Rumah** saat malam hari (sebelum tidur) atau melalui menu jeda saat berada di Rumah.
   - **Tidak ada autosave di tengah map level** agar alur retry dan evaluasi kegagalan tetap bersih, deterministik, dan bebas exploit.
2. **Aturan Kegagalan & Retry per Level:**
   - Jika waktu misi habis, pemain dapat memilih **Ulang Misi (Retry)**.
   - Retry me-reset data level aktif, tetapi **TIDAK MENGHAPUS** progres hari-hari sebelumnya.
3. **Rintangan graybox:**
   - Preman mengejar pemain dalam radius; kontak mengurangi timer dengan jeda antar penalti.
   - Genangan memperlambat jalan dan lari selama pemain berada di dalamnya; nilai dan penempatan final masih perlu kalibrasi.

---

## 5. Pembagian Tanggung Jawab Role

- **Role A (Gameplay & Integrasi):**
  - Mengimplementasikan `StoryFlow_PengaturAlur` (state machine alur harian & transisi scene).
  - Membuat scene graybox `Posko.unity` dan `Rumah.unity` dengan titik interaksi Jonathan, Ophelia, Ibu/Ayah, tempat tidur, dan gerbang bersama (AB-D02).
  - Mengatur transisi spawn dan persistensi data antar scene.
- **Role B (Verifikasi):**
  - Menguji kontrak transisi dan state machine lewat automated PlayMode tests.
  - Memverifikasi loop 3× retry, save/load slot, dan isolasi reset level aktif.
- **Role C (Visual, Level, UI):**
  - Menyediakan aset interior Rumah, lapangan Posko, dan 5 variasi layout level bertahap sesuai [C-D01](cd01-arah-visual.md).
  - Menyiapkan prefab UI dialog, menu gerbang posko, dan save/load menu (C-D05).

