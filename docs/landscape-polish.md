# Tanah, vegetasi, dan keterbacaan karakter

19 November 2026. Berlaku untuk scene Posko dan Rumah.

## Aset dan source

Tanah menggunakan **Brown Mud Dry** oleh Rob Tuytel dari [Poly Haven](https://polyhaven.com/a/brown_mud_dry), lisensi [CC0](https://polyhaven.com/license). Berkas diffuse asli 2K diunduh ke `Blender/AssetLibrary_Staging/Ground_BrownMudDry/` dan disalin ke `Unity/JejakAsa/Assets/_Project/Art/Environment/Landscape/`. Tidak di-upscale. Material `JejakAsa/Packed Earth` mengurangi kekuatan tekstur agar tanah tetap tenang di samping model low-poly, dengan pencahayaan dan bayangan URP.

Vegetasi dan pondasi dibuat untuk proyek ini melalui `ArtSource/P02/build_landscape.py`. Sumber editable: `ArtSource/P02/P02_Landscape.blend`; salinan katalog lokal: `Blender/AssetLibrary_Staging/P02_Landscape/JejakAsa_Landscape_Library.blend`. Folder Blender diabaikan Git; source blend, FBX, tekstur, dan catatan ini tetap ikut proyek.

## Perubahan Unity

- Root `Landscape_Edge_Dressing` di kedua scene. Tanah membentang di bawah seluruh rumah sekitar posko; halaman rumah mendapat pondasi, kebun samping/belakang, dan batu pijakan menuju pintu.
- Pepohonan lebih tinggi di luar tepi map, semak lebih rendah dekat pandangan depan. Gerbang dan jalur utama tetap terbuka.
- Vegetasi dibatch per material menjadi 14 mesh Posko dan 13 mesh Rumah. Tidak ada collider vegetasi tambahan; floor, batas map, furniture dan trigger yang telah diuji tetap dipakai.
- Paving grid lama dimatikan renderernya. Aset lama dipertahankan.
- Kamera Posko 9 → 6.2, Rumah 7.5 → 5.6, dengan framing mengikuti pemain. SMAA High aktif pada kedua kamera; kompatibel dengan renderer Deferred+ proyek.
- Tekstur karakter tetap memakai sumber 2048×2048, mipmaps, filtering trilinear, anisotropic 4, dan kompresi berkualitas tinggi. Portrait/UI tidak diubah.
- `CharacterSightline` menjaga siluet pemain ketika atap naungan posko berada di depan kamera. Setelah revisi gerbang, palang dan tulisan gerbang selalu dirender.
- Game View editor disetel ke fixed 1920×1080 untuk pemeriksaan visual; tidak mengunci resolusi build pemain.

Integrasi ulang eksplisit lewat menu `Jejak Asa > Environment > Apply Landscape Polish`. Jangan menjalankan menu integrasi Posko awal setelah polish tanpa menerapkan ulang landscape. Script tidak mengubah scene saat editor dibuka.

## Verifikasi

13 tes unik Play Mode lulus: `P02ListeningTests` (2), `P02HomeNavigationTests` (1), `P01SceneIntegrationTests` (4), `RumahBoundaryTests` (1), `PoskoFlowIntegrationTests` (4), dan `EnvironmentReadabilityTests` (1). Tes prolog diperbarui untuk mengharapkan framing rumah 5.6; alur prolog, penyimpanan, dan pemulihan kamera tetap diuji. Tes keterbacaan memastikan palang membuka pandangan saat pemain datang, kembali terlihat ketika pemain menjauh, serta tidak mengubah collider. Tes terakhir visibilitas diulang setelah memasangkan label dengan palang.

Shader tanah terkompilasi tanpa error. Kedua scene disimpan; Unity ditinggalkan pada Posko, mode edit. Console masih mencatat error lama layanan Unity Generators `NoSubscription`; integrasi ini tidak menggunakan layanan tersebut.

Capture dari Unity pada 1920×1080:

- [Keseluruhan posko](../Previews/Landscape/posko-overview.png), kamera review yang lebih lebar.
- [Gameplay posko](../Previews/Landscape/posko-gameplay-1080.png), kamera permainan sebenarnya dengan UI.
- [Keseluruhan rumah](../Previews/Landscape/rumah-overview.png), kamera review yang lebih lebar.
- [Gameplay rumah](../Previews/Landscape/rumah-gameplay-1080.png), kamera permainan sebenarnya dengan UI.

## Revisi permukaan posko, kamera, dan hover menu

Permukaan posko kini menggunakan heightfield dengan relief 1.2–23.1 cm dan satu MeshCollider statis. Jalur masuk, jalur ke NPC, area furniture, dan tempat NPC berdiri dijaga landai. Normal map Brown Mud Dry 2K menambahkan detail mikro; jalur tanah padat bercabang menuju naungan dan bangku, batu di depan naungan serta gundukan taman di luar batas map memberi variasi bentuk.

Kamera gameplay Posko sekarang orthographic size **4.35**, sebelumnya 6.2: siluet karakter sekitar 43% lebih besar pada resolusi yang sama. Framing pusat mengikuti pemain dengan jarak pandang ke depan 1.3 m. Kamera Rumah tetap 5.6.

`SessionMenu` membaca posisi mouse langsung melalui Input System, memakai hit area yang sama dengan gambar menu, termasuk skala layar dan tinggi baris slot simpan. Mouse diam tidak mengambil kembali pilihan dari keyboard. Tombol krem pada menu dalam game mendapatkan sorotan teal ketika di-hover; label simpan menyesuaikan kontras. Menu jeda, pengaturan dan muat data tetap bekerja ketika time scale nol.

Source tambahan: `ArtSource/P02/P02_Posko_Surface.blend`, `build_posko_surface.py`, dan salinan asset library lokal `Blender/AssetLibrary_Staging/P02_Landscape/JejakAsa_Posko_Surface.blend`. Integrasi ulang: `Jejak Asa > Posko > Apply Surface Relief`, setelah integrasi landscape awal.

18 tes Play Mode lulus: `MenuPointerTests`, `EnvironmentReadabilityTests`, `P02ListeningTests`, `PoskoFlowIntegrationTests`, dan `U18MenuIntegrationTests`. Memeriksa hover mouse dan prioritas keyboard, submenu, relief dan collider, tempat berdiri NPC, visibilitas gerbang, P02, serta alur menu lama. Shader terkompilasi tanpa error.

Pratinjau revisi: [kamera gameplay](../Previews/PoskoSurface/posko-gameplay-close.png), [dekat NPC](../Previews/PoskoSurface/posko-npc-close.png), [hover menu jeda](../Previews/PoskoSurface/pause-mouse-hover.png), dan [overview relief](../Previews/PoskoSurface/posko-surface-overview.png). Overview menggunakan kamera review yang lebih jauh; pengaturan tersebut tidak dipakai saat bermain.

## Perbaikan gerbang dan collision pintu keluar

Gerbang dan tulisan `SELAMAT DATANG` dikeluarkan dari daftar `CharacterSightline`, sehingga tidak menghilang ketika didekati. Renderer tulisan FBX lama dipertahankan tetapi disembunyikan; `Gate_Welcome_Front` memakai mesh/material yang sama dengan orientasi diperbaiki 180 derajat di sekitar pusatnya. Tulisan sudah diperiksa melalui capture kamera gameplay dan terbaca dari kiri ke kanan.

Masalah jatuh di posko berasal dari celah di antara `Batas_Lapangan_1` dan `Batas_Lapangan_2`: `Gate_Exit_Trigger` merupakan trigger interaksi, bukan penghalang fisik, dan lantai lapangan berhenti pada z=-12. Root `Map_Exit_Collision` sekarang menambahkan `Gate_Exit_Stopper` yang bertumpang tindih dengan kedua sisi, collider palang atas, dan collision tanah yang mengikuti seluruh hamparan visual. Di Rumah, `Home_Exit_Stopper` menutup sambungan kedua kusen. Kedua scene memiliki lantai pendukung untuk tanah luar dan batas fisik di ujung hamparan agar tidak berjalan ke void.

Trigger E dan alur berpindah scene tetap dipakai. Perbaikan disimpan di scene melalui `GateSafetyIntegration`, menu `Jejak Asa > Environment > Repair Exit Collision And Gate`; integrasi landscape, surface dan P02 juga memanggil perbaikan ini agar regenerasi tidak mengembalikan bug.

15 tes Play Mode lulus: `ExitSafetyTests` (2), `EnvironmentReadabilityTests` (2), `RumahBoundaryTests` (1), `P01SceneIntegrationTests` (4), `PoskoFlowIntegrationTests` (4), dan `P02ListeningTests` (2). Tes baru menahan gerak ke arah gerbang dan sambungan kusen berkali-kali, memeriksa pijakan tanah luar, jangkauan interaksi gerbang, dan memastikan palang/tulisan terus terlihat. Pemeriksaan tambahan dalam Play Mode: 100 gerakan ke gerbang berhenti pada (0, 1.09, -11.22), grounded; 150 gerakan ke pintu rumah berhenti pada (2, 1.08, -4.12), grounded.

Capture: [gerbang dekat dan tulisan benar](../Previews/GateRepair/gate-solid-close.png), [gerbang dari posisi awal](../Previews/GateRepair/gate-visible-readable.png), [pintu rumah](../Previews/GateRepair/home-door-solid.png).
