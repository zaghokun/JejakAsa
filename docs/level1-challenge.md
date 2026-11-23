# Level 1 — rute, patroli dan genangan

23 November 2026. Scene: `Assets/_Project/Scenes/Kampung.unity`. Penataan visual rumah, pohon dan perimeter dari update sebelumnya dipertahankan.

## Perubahan gameplay

- Tetap tiga penerima bantuan. Penerima pertama berada di jalur aman; dua berikutnya dapat dicapai lewat gang pendek yang dijaga atau jalur kering memutar.
- Pagar bercollider di barat dan timur menutup akses samping yang sebelumnya membuat seluruh rintangan mudah dilewati. Jalur kering mendapat paving; collider pagar mengikuti bentuknya. Dua papan petunjuk dihapus dari scene dan generator sesuai permintaan terbaru.
- Dua preman berpatroli. Kecepatan patroli 1,65 m/detik, pengejaran 4,1 m/detik; tubuh berputar mengikuti gerak nyata. Animasi berjalan dan berlari menyesuaikan kecepatan, dengan ambang lari khusus preman 3 m/detik.
- Deteksi Level 1 memerlukan pandangan tanpa penghalang dan tetap dibatasi wilayah asal. Setelah kehilangan pemain, preman kembali ke patroli. Dialog, jeda dan hasil misi menghentikan gerak; retry memulihkan posisi, arah dan indeks patroli.
- Preman Level 1 tidak hilang setelah tiga kontak. Kontak mengurangi 10 detik, memberi jeda mundur 1,6 detik dan cooldown 2,4 detik. Grace period bersama 1,1 detik mencegah dua preman memberi penalti bersamaan. Perilaku lama level lain tetap menjadi nilai default.
- Timer Level 1 50 detik, berhenti ketika dialog seperti sebelumnya. Reset/retry mengambil durasi dari konfigurasi level, sehingga tidak kembali ke 180 detik.
- Tiga genangan organik menggantikan kotak air lama. Dua gang utama memakai perlambatan 0,48; simpang memakai 0,65. Sprint di lumpur utama menjadi 3,36 m/detik, lebih lambat dari pengejar 4,1 m/detik. Area perlambatan mengikuti bentuk tepi lumpur, bukan seluruh sudut BoxCollider pemicu.

## Sumber aset

Material lumpur 2K menggunakan [Ground025 dari ambientCG](https://ambientcg.com/view?id=Ground025), oleh Lennart Demes, [CC0-1.0](https://docs.ambientcg.com/license/). Sumber dipilih dari hasil photogrammetry, tidak dibuat dengan AI. Mesh tepi lumpur, air dan riak dibuat di proyek ini.

- Registri lokal dan sumber `.blend`: `Blender/AssetLibrary_Staging/Level1_Puddles/ASSETS.md` dan subfolder `Ground025/`.
- Manifest terlacak beserta hash runtime: `ArtSource/Level1/puddle-source.json`.
- Catatan sumber: `ArtSource/Level1/ASSETS.md`.
- Asset runtime: `Assets/_Project/Art/Environment/Level1Puddles/`.

Folder Blender diabaikan Git; material, tekstur, mesh Unity dan manifest ikut repository. Penyiapan texture: `ArtSource/Level1/prepare_puddle_assets.py`. Library Blender packed terpisah berada di `Blender/AssetLibrary_Staging/Level1_Puddles/Level1_Puddles_Library.blend`, berisi tiga collection varian dan dua material berlabel asset dalam katalog `JejakAsa/Level1/Puddles`.

## Pengeditan

Saat scene `Kampung` dibuka langsung dengan Play tanpa sesi campaign, `Mission_PengantaranBantuan.Awake` meminta `StoryFlow_PengaturAlur.EnsureLevelSceneSession` menyiapkan fase pengantaran, tiga paket dan nomor wilayah. Ini menyelaraskan HUD dengan misi aktif, sehingga penghitung paket dan timer tampil. Entry langsung `Kampung_L2`–`Kampung_L5` mengikuti nomor levelnya. Campaign yang sudah dimulai, state uji eksplisit dan progres yang dimuat dari save tidak ditimpa. Tidak ada autosave baru.

HUD level memakai judul “MISI PENGANTARAN”, sementara kartu kanan menunjukkan hari dan wilayah. HUD rumah/posko tetap mengikuti fase ceritanya.

Perbaikan entry/HUD diverifikasi dengan 30/30 tes Play Mode: `StandaloneLevelEntryTests`, `R01FlowContractTests`, `PoskoFlowIntegrationTests`, `ObjectiveHudTests`, `U18MenuIntegrationTests`, dan `CampaignFiveLevelsTests`. Entry alami dari Play di Kampung juga ditinjau langsung tanpa menyuntikkan state: fase `DiLevel`, 3 paket, 0 terantar, timer berjalan, dan 0 papan petunjuk. [Bukti HUD entry langsung](../Previews/Level1Challenge/direct-entry-mission-hud.png).

Utility `Editor_Level1_RuteDanRintangan.cs` membangun root `Level1_Challenge_Layout`. Jalankan menu `Jejak Asa > Level 1 > Apply Routes, Patrols & Puddles` setelah menyimpan scene. Menu penataan visual lama juga memasang ulang bagian challenge. Penerapan otomatis setiap reload script telah dihapus agar penataan manual tidak tertimpa tanpa sengaja. Revert graybox membersihkan root challenge dan mengembalikan nilai dasar.

`Obstacle_Preman.cs` mengatur gerak, rotasi, deteksi, patroli dan kontak; `Obstacle_Genangan.cs` mengatur area perlambatan. `Character_AnimasiGerak.cs` tetap dipakai bersama karakter lain dengan ambang lari default yang sama seperti sebelumnya.

## Verifikasi

37/37 tes Play Mode lulus sebelum kalibrasi timer akhir: `Level1ChallengeTests`, `PremanControllerTests`, `PuddleMovementTests`, `CampaignFiveLevelsTests`, `PoskoFlowIntegrationTests`, `R01FlowContractTests`, `U17RetryIntegrationTests`, dan `P01SceneIntegrationTests`. Memeriksa rotasi/patroli/animasi lari, pause/dialog, retry dua preman, perlambatan yang sesuai bentuk, kontak tanpa hilangnya penjaga, grace period bersama, akses penerima pada kelima level dan alur lima hari.

Lintasan kering penuh memakai input keyboard otomatis dengan urutan jalan yang sudah diketahui selesai sekitar 16 detik. Angka ini baseline teknis; tidak mencakup waktu pemain baru mencari jalan atau membaca situasi. Setelah pengukuran, batas awal diturunkan dari 65 menjadi 50 detik. Kalibrasi kesulitan subjektif tetap memerlukan playtest pemain.

Sesudah kalibrasi akhir, 7/7 tes terkait diulang dan lulus: tiga tes challenge, tiga tes campaign dan satu tes retry. Penyelesaian jalur kering penuh pada konfigurasi 50 detik menghabiskan 15,93 detik dan menyisakan 34,07 detik. Sesudah pembersihan visual terakhir, tiga tes challenge dan satu tes penyelesaian jalur kering diulang lagi, 4/4 lulus. Total 37 tes unik terkait lulus; pengujian ulang beririsan dengan suite sebelumnya. Scene final diperiksa tanpa Missing Script. Build Windows baru belum dibuat.

Pratinjau Game View: [layout lengkap](../Previews/Level1Challenge/level1-layout.png), [detail genangan dan preman](../Previews/Level1Challenge/puddle-detail.png). Panel UI disembunyikan sementara dalam pengambilan pratinjau ini; kamera gameplay tetap memakai framing yang sudah ada.
