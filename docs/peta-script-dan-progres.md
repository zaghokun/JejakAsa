# Peta script dan bahan laporan progres

Refactor 23 November 2026. Pengelompokan dan penamaan diperjelas; aturan permainan, field Inspector, callback tombol, namespace dan assembly tetap dipertahankan. Metadata `.meta` dipindahkan bersama setiap file.

## Cara membaca struktur

- `Scripts/`: kode yang dipakai saat game berjalan.
- `Editor/`: alat pembuat/pengimpor; tidak ikut berjalan pada executable. Hasilnya tersimpan dalam scene/aset.
- `Tests/PlayMode/`: verifikasi perilaku dengan Unity Test Framework; bukan fitur pemain.

## Script runtime: nama lama, lokasi baru dan tanggung jawab

| Nama lama | Lokasi baru di Scripts | Tanggung jawab |
| --- | --- | --- |
| `PlayerController.cs` | [Player/Player_Gerak.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Player/Player_Gerak.cs) | Input jalan/lari, gravitasi dan perlambatan genangan |
| `P01CharacterAnimator.cs` | [Characters/Character_AnimasiGerak.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Characters/Character_AnimasiGerak.cs) | Memilih idle, jalan, lari dan animasi bicara karakter |
| `FixedIsometricCamera.cs` | [Camera/Camera_IsometrikTetap.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Camera/Camera_IsometrikTetap.cs) | Mengikuti pemain dengan orientasi isometrik tetap |
| `CharacterSightline.cs` | [Camera/Camera_VisibilitasKarakter.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Camera/Camera_VisibilitasKarakter.cs) | Menyembunyikan atap yang menutupi karakter sambil mempertahankan collider |
| `HomeInteractionController.cs` | [Interaction/Interaction_Rumah.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Interaction/Interaction_Rumah.cs) | Pintu rumah, tempat tidur, simpan dan istirahat |
| `KampungDoorTrigger.cs` | [Interaction/Interaction_PintuKampung.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Interaction/Interaction_PintuKampung.cs) | Interaksi pintu kampung dan titik spawn antar-scene |
| `PoskoDistributionTrigger.cs` | [Interaction/Interaction_AmbilPaketPosko.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Interaction/Interaction_AmbilPaketPosko.cs) | Pengambilan paket setelah syarat briefing terpenuhi |
| `PoskoGateController.cs` | [Interaction/Interaction_GerbangPosko.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Interaction/Interaction_GerbangPosko.cs) | Pilihan berangkat/pulang dan alasan tujuan terkunci |
| `PlayerInteraction.cs` | [Mission/Mission_PengantaranBantuan.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Mission/Mission_PengantaranBantuan.cs) | Sesi pengantaran, paket, timer, penyerahan, hasil dan retry |
| `InteractionTarget.cs` | [Mission/Mission_PenerimaBantuan.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Mission/Mission_PenerimaBantuan.cs) | Identitas penerima, dialog dan status bantuan diterima |
| `PremanController.cs` | [Obstacles/Obstacle_Preman.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Obstacles/Obstacle_Preman.cs) | Deteksi, patroli, pengejaran, rotasi, penalti kontak dan reset |
| `PuddleZone.cs` | [Obstacles/Obstacle_Genangan.cs](../Unity/JejakAsa/Assets/_Project/Scripts/Obstacles/Obstacle_Genangan.cs) | Batas genangan dan efek perlambatan sesuai bentuknya |
| `CozyGuiTheme.cs` | [UI/Theme/UI_TemaVisual.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Theme/UI_TemaVisual.cs) | Warna, font, panel, ikon dan gaya visual UI bersama |
| `MenuUtama.cs` | [UI/Menus/UI_MenuUtama.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Menus/UI_MenuUtama.cs) | Tampilan tombol menu utama dan penerusan klik ke pengatur sesi |
| `SessionMenu.cs` | [UI/Menus/UI_MenuSesi.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Menus/UI_MenuSesi.cs) | Navigasi menu, pause, settings, konfirmasi dan pilihan save/load |
| `PeringatanPreman.cs` | [UI/HUD/UI_PeringatanPreman.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/HUD/UI_PeringatanPreman.cs) | Peringatan preman dekat dan umpan balik penalti waktu |
| `KartuHari.cs` | [UI/HUD/UI_PergantianHari.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/HUD/UI_PergantianHari.cs) | Kartu pemberitahuan saat hari bertambah |
| `LayarTamat.cs` | [UI/Ending/UI_PenutupCerita.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Ending/UI_PenutupCerita.cs) | Layar penutup setelah fase Tamat |
| `SceneStartFade.cs` | [UI/Transitions/UI_TransisiMasukScene.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Transitions/UI_TransisiMasukScene.cs) | Fade pembuka scene sebelum kendali diberikan |
| `StoryDialog_Tampilan.cs` | [UI/Dialogue/UI_DialogDanPotret.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/Dialogue/UI_DialogDanPotret.cs) | Presentasi dialog, potret pembicara dan animasi panel |
| `StoryObjective_TujuanAktif.cs` | [UI/HUD/UI_TujuanDanProgres.cs](../Unity/JejakAsa/Assets/_Project/Scripts/UI/HUD/UI_TujuanDanProgres.cs) | Tujuan sesuai progres, paket terantar, hari/wilayah dan timer |

## Story P01–P05 dan data

| File di Scripts/Story | Tanggung jawab |
| --- | --- |
| `P01_VickyPulang.cs` | Dialog rumah dan kamera pembuka Vicky–Ibu |
| `P02_MendengarkanWarga.cs` | Percakapan kebutuhan warga bersama Bu Rina |
| `P03_SponsorDanPosko.cs` | Sponsor, montage penyiapan posko dan urutan pengenalan posko |
| `P04_PertemuanOphelia.cs` | Naskah dan penyelesaian pertemuan kembali |
| `P05_BayanganAncaman.cs` | Dua shot villain, dialog, audio, fade dan pemulihan kontrol |
| `System/StoryFlow_PengaturAlur.cs` | Fase campaign, gerbang, hari, save/load dan entry langsung level |
| `System/StoryState_DataProgres.cs` | Model progres dan fase harian yang diserialisasi |
| `System/StoryDialog_InteraksiNPC.cs` | Pemicu interaksi/dialog story dan callback penyelesaian |
| `Data/StoryData_BarisDialog.cs` | Struct `DialogueLine`: pasangan pembicara dan teks; tidak dipasang ke GameObject |

Tampilan dialog dan HUD sekarang berada di `UI/Dialogue` dan `UI/HUD`. Nama tipe data `DialogueLine`, `GameProgressData`, `DayPhase`, `MissionOutcome` dan enum lain tetap sama.

## Alat Editor

| Nama lama | Lokasi baru di Editor | Tanggung jawab |
| --- | --- | --- |
| `AturGambarHybrid.cs` | [UI/Editor_UI_ImporGambarHybrid.cs](../Unity/JejakAsa/Assets/_Project/Editor/UI/Editor_UI_ImporGambarHybrid.cs) | Aturan impor sprite hybrid dan border 9-slice |
| `JejakAsaUiTextureImport.cs` | [UI/Editor_UI_ImporTeksturGenerated.cs](../Unity/JejakAsa/Assets/_Project/Editor/UI/Editor_UI_ImporTeksturGenerated.cs) | Aturan impor tekstur UI Generated agar tetap tajam |
| `GateSafetyIntegration.cs` | [Environment/Editor_Gerbang_Keamanan.cs](../Unity/JejakAsa/Assets/_Project/Editor/Environment/Editor_Gerbang_Keamanan.cs) | Pijakan, batas keluar, collider gerbang dan arah label |
| `HouseDressingIntegration.cs` | [Environment/Editor_Rumah_Dekorasi.cs](../Unity/JejakAsa/Assets/_Project/Editor/Environment/Editor_Rumah_Dekorasi.cs) | Membangun dekorasi rumah dan collider furnitur |
| `LandscapeIntegration.cs` | [Environment/Editor_Lingkungan_Lanskap.cs](../Unity/JejakAsa/Assets/_Project/Editor/Environment/Editor_Lingkungan_Lanskap.cs) | Membangun tanah, vegetasi dan penutup tepi lingkungan |
| `PoskoSurfaceIntegration.cs` | [Environment/Editor_Posko_Permukaan.cs](../Unity/JejakAsa/Assets/_Project/Editor/Environment/Editor_Posko_Permukaan.cs) | Membangun relief dan detail permukaan posko |
| `PoskoP02Integration.cs` | [Environment/Editor_Posko_VisualDanWarga.cs](../Unity/JejakAsa/Assets/_Project/Editor/Environment/Editor_Posko_VisualDanWarga.cs) | Integrasi lingkungan posko, karakter dan pengikatan P02 |
| `Level1Integration.cs` | [Levels/Editor_Level1_VisualLingkungan.cs](../Unity/JejakAsa/Assets/_Project/Editor/Levels/Editor_Level1_VisualLingkungan.cs) | Membangun visual Level 1 dan menghubungkan layout challenge |
| `Level1ChallengeIntegration.cs` | [Levels/Editor_Level1_RuteDanRintangan.cs](../Unity/JejakAsa/Assets/_Project/Editor/Levels/Editor_Level1_RuteDanRintangan.cs) | Membangun rute, pagar, genangan dan konfigurasi patroli Level 1 |
| `P03P05_PoskoStoryIntegration.cs` | [Story/Editor_P03P05_IntegrasiStory.cs](../Unity/JejakAsa/Assets/_Project/Editor/Story/Editor_P03P05_IntegrasiStory.cs) | Membangun set dan mengikat referensi story P03–P05 |

Menu Unity yang sudah ada tetap sama. Script Editor dapat membangun ulang objek di bawah root yang dikelolanya; simpan pekerjaan sebelum menjalankan generator.

## Pengujian

Nama class tes dipertahankan untuk pelacakan hasil lama. Kode U02–U18 adalah ID kebutuhan/penerimaan. Foldernya dikelompokkan sebagai berikut:

- `Tests/PlayMode/Campaign/`: `CampaignFiveLevelsTests`, `R01FlowContractTests`, `StandaloneLevelEntryTests`, `PoskoFlowIntegrationTests`.
- `Tests/PlayMode/Story/`: `P01OpeningTests`, `P01FadeTests`, `P01SceneIntegrationTests`, `P02ListeningTests`, `PoskoPrologueTests`.
- `Tests/PlayMode/Movement/`: `U02MovementTests`, `U02KampungRouteTests`, `P02HomeNavigationTests`.
- `Tests/PlayMode/Interaction/`: `U03InteractionTests`, `U03KampungInteractionTests`.
- `Tests/PlayMode/Mission/`: `U04DeliveryTests`, `U05OrderTests`, `U06TimerTests`, `U07PriorityAndOutcomeTests`, `U17RetryIntegrationTests`.
- `Tests/PlayMode/Obstacles/`: `PremanControllerTests`, `PuddleMovementTests`.
- `Tests/PlayMode/Environment/`: `RumahBoundaryTests`, `ExitSafetyTests`, `EnvironmentReadabilityTests`, `Level1ChallengeTests`.
- `Tests/PlayMode/UI/`: `GuiPresentationTests`, `MenuPointerTests`, `ObjectiveHudTests`, `U18MenuIntegrationTests`.

## Bahan menjelaskan progres

| Fitur | Penjelasan saat ditanya | File acuan |
| --- | --- | --- |
| Gerak | Input WASD/Shift menggerakkan CharacterController; kecepatan dipengaruhi genangan | `Player/Player_Gerak.cs` |
| Pengantaran | Pemain mengambil paket, mendekati penerima, menyerahkan bantuan dan mendapat hasil | `Mission/Mission_PengantaranBantuan.cs`, `Mission/Mission_PenerimaBantuan.cs` |
| Progres cerita | Flag menentukan urutan P01–P05, briefing, laporan, istirahat dan hari berikutnya | `Story/System/StoryFlow_PengaturAlur.cs` dan file P01–P05 |
| Tantangan Level 1 | Jalur aman lebih panjang; genangan memperlambat dan preman mengejar serta memberi penalti waktu | `Obstacles/`, `Editor/Levels/Editor_Level1_RuteDanRintangan.cs` |
| Map dan collider | Generator membuat lingkungan; scene menyimpan objek dan collider untuk dimainkan | `Editor/Environment/`, `Editor/Levels/` |
| Kamera dan karakter | Kamera mengikuti pemain; atap yang menghalangi disembunyikan; animasi mengikuti gerak | `Camera/`, `Characters/` |
| UI | Tampilan menu, dialog, potret, tujuan, timer dan notifikasi membaca status game | `UI/` |
| Bukti verifikasi | Hasil tes harus disebut sesuai kasus yang diuji; tidak otomatis membuktikan build atau playtest manusia | `Tests/PlayMode/` dan catatan progres README |

Contoh jawaban: “Saya menata ulang rute Level 1 melalui alat Editor, sedangkan patroli dan rotasi preman diatur script runtime. Hasil map disimpan ke Kampung.unity. Saya memverifikasi jalur, kontak, timer, retry dan progres menggunakan Play Mode tests.”

Bahan laporan ini menjelaskan fungsi kode. Pembagian kontribusi tetap mengikuti bagian yang dikerjakan tiap anggota.

## Hasil verifikasi refactor

92 kasus tes Play Mode unik terverifikasi lulus setelah dua acuan tes lama disesuaikan. Delapan scene memiliki 0 Missing Script; callback menu tetap terhubung. [Rincian pengujian dan batas verifikasi](reviews/2026-11-23-script-refactor.md).
