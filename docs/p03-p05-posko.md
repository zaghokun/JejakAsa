# P03–P05 — Posko dan bayangan mengancam

Tanggal integrasi: 21 November 2026. Cakupan: P03/SB-03 sampai P05/SB-05 dari `storyboard-dialog-jejak-asa.md`.

File cerita sekarang dikelompokkan di `Assets/_Project/Scripts/Story/`: `P01_VickyPulang.cs`, `P02_MendengarkanWarga.cs`, `P03_SponsorDanPosko.cs`, `P04_PertemuanOphelia.cs`, dan `P05_BayanganAncaman.cs`. [Peta file dan objek Hierarchy](../Unity/JejakAsa/Assets/_Project/Scripts/Story/README.md). Rename mempertahankan GUID `.meta` dan nama field save; P04 dipisahkan dari file P03 tanpa mengubah naskah atau urutan.

Sesudah penamaan ulang, 14/14 tes Play Mode lulus: `P01OpeningTests`, `P01SceneIntegrationTests`, `P02ListeningTests`, dan `PoskoPrologueTests`.

Script pengatur bersama juga dikelompokkan di `Assets/_Project/Scripts/Story/System/`: `StoryFlow_PengaturAlur.cs`, `StoryState_DataProgres.cs`, `StoryDialog_InteraksiNPC.cs`, `StoryDialog_Tampilan.cs`, dan `StoryObjective_TujuanAktif.cs`. File pertama mengatur fase harian, gerbang dan penyimpanan; file data tetap memakai field save yang sama. Peta nama lama dan baru ada di README folder Story.

Sesudah rename sistem bersama, 41/41 tes Play Mode lulus: pengenalan P01–P05, kontrak fase harian, posko, tujuan aktif, campaign lima hari, akses penerima pada lima level, dan menu/save/load. Kelima GUID script tetap sama.

## Cara mencoba

1. Buka `MainMenu` dan pilih Permainan Baru. Selesaikan P01 di rumah, lalu P02 bersama Bu Rina di posko.
2. Temui Pak Jonathan. Enam baris percakapan sponsor dilanjutkan montage warga menyiapkan posko, persediaan dan daftar penerima.
3. Ikuti tujuan untuk menyapa Ophelia dekat bangku. Selesaikan tujuh baris pertemuan kembali.
4. P05 berjalan otomatis: Deddy dan anak buah di gang, kemudian Gas Oke di meja peta. Enter/Spasi/Esc atau klik petunjuk melanjutkan dialog; setiap baris memiliki jeda minimum agar tidak terlewati oleh input yang sama.
5. Seusai fade kembali ke posko, temui Jonathan untuk briefing hari pertama, ambil paket dan lanjutkan pengantaran melalui gerbang.

Progres baru ditandai sesudah tahap terkait selesai. Keluar dari scene di tengah dialog/montage/cutscene tidak menyelesaikan tahap yang masih berjalan. Flag campaign ikut serialisasi dan bertahan setelah reset harian; tidak ada autosave baru. Save lama yang sudah memulai pengantaran serta hari kedua dan seterusnya mempertahankan alurnya.

## Implementasi

`P03_SponsorDanPosko` mengatur urutan interaksi dan tujuan. Pertemuan Ophelia ini terpisah dari percakapan laporan sore yang sudah tersedia. Briefing, pengambilan paket dan keberangkatan ditahan sampai pengenalan posko selesai.

`P05_BayanganAncaman` memakai dua set 3D terpisah di luar area permainan dalam `Posko.unity`. Kamera perspektif bergerak pelan mendekat, dengan sisi wajah diterangi lampu hangat, cahaya tepi dingin, kabut, bar hitam dan caption. Suara rendah asli sepanjang 24 detik mendampingi adegan; audio lingkungan dikecilkan sementara. Pemain tetap di posko. Penyelesaian maupun pembatalan memulihkan kamera, follow, gerak, pencahayaan, kabut dan volume sumber audio.

Set dan pengikatan referensi dapat dibangun ulang melalui `Jejak Asa > Story > Apply P03-P05` saat tidak bermain dan scene Posko sudah disimpan. Utility hanya mengganti root `P03-P05_PrologPosko`, bukan lingkungan P02. Label atap “RUANG WARGA” dibetulkan arahnya dan mengikuti hilangnya atap saat menghalangi karakter; perbaikan gerbang yang sudah ada dipertahankan.

## Sumber aset

- Deddy, Gas Oke dan anak buah memakai model Meshy lokal milik proyek dan rig Mixamo yang sudah ada di `Blender/Animation_References/Mixamo_Rerig_2026/Rigged/`. Tekstur diambil dari GLB sumber, bukan dibuat ulang.
- Penyiapan model dan audio: `ArtSource/P03P05/prepare_assets.py`. Audio `UnderWatch.wav` dibuat dengan sintesis nada rendah dan noise deterministik; tidak memakai rekaman pihak ketiga.
- Set gang, ruangan dan meja peta dibentuk dari geometri Unity oleh `P03P05_PoskoStoryIntegration.cs`. Material URP dan model diimpor di bawah `Assets/_Project/Art/`.
- Registri lokal: `Blender/AssetLibrary_Staging/P03P05/ASSETS.md`. Folder Blender diabaikan Git; model Unity, source dan catatan ini tetap ikut repository. Tidak ada unduhan baru.

## Verifikasi

28 tes unik Play Mode lulus: `PoskoPrologueTests`, `P02ListeningTests`, `PoskoFlowIntegrationTests`, `R01FlowContractTests`, `ObjectiveHudTests`, `ExitSafetyTests`, dan `P01SceneIntegrationTests`. Putaran pertama 24/24; sesudah penyesuaian visual dan pemulihan audio, putaran kedua 10/10, dengan enam tes beririsan. Mencakup urutan tahap, dialog tidak lengkap, reload, flag campaign, kompatibilitas save lama, pemulihan cutscene, rumah, gerbang dan alur pengantaran lama.

Review Game View 1920×1080: [sponsor](../Previews/P03P05/p03-sponsor.png), [penyiapan posko](../Previews/P03P05/p03-setup.png), [Ophelia](../Previews/P03P05/p04-reunion.png), [Deddy](../Previews/P03P05/p05-deddy.png), [Gas Oke](../Previews/P03P05/p05-gas.png).

Build Windows baru dan story setelah SB-05 belum dikerjakan dalam integrasi ini.
