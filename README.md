# Jejak Asa

**Bantuan sampai, harapan tumbuh.**

Game tugas akhir Grafika Komputer: game eksplorasi 3D low-poly tentang lulusan baru yang kembali ke kampung fiktif di Jakarta Utara untuk membantu distribusi bantuan sosial. Pemain mengatur rute dan mengantar bantuan kepada warga sebelum waktu habis.

Tema utama adalah SDG 1 — Tanpa Kemiskinan, didukung SDG 11 — Kota dan Permukiman Berkelanjutan serta SDG 16 — Perdamaian, Keadilan, dan Kelembagaan yang Tangguh. Game tidak mengklaim bahwa bantuan sesaat menyelesaikan kemiskinan atau bahwa warga adalah penerima pasif.

## Sumber progres aktif

**README ini menjadi tempat pembaruan progres proyek mulai sekarang.** Dokumen pada `docs/` tetap menjadi acuan desain dan rencana kerja; catatan progres di dalam dokumen lama adalah snapshot, bukan tracker yang terus diperbarui. Pembaruan progres berikutnya tidak perlu mengubah DOCX/PDF tersebut.

Checkbox U adalah kriteria penerimaan game, bukan nomor hari atau checklist yang harus dikerjakan setiap role. D adalah target hari pada rencana. Milestone `AB-M` melacak pekerjaan kolaboratif A/B; `C-M` melacak hasil kerja C. Satu U atau milestone yang lulus tidak otomatis menyelesaikan seluruh hari, role, atau produk.

## Teknologi dan cakupan

| Bagian | Pilihan proyek |
| --- | --- |
| Editor | Unity 6.3 LTS **6000.3.27f1**, sama untuk semua anggota |
| Rendering | Universal Render Pipeline / URP |
| Platform | Windows, single-player, keyboard dan mouse |
| Kamera | Isometrik dengan orientasi tetap |
| Kontrol | Input System resmi; CharacterController untuk gerak, tahan Shift kiri/kanan untuk lari |
| Antarmuka | Unity UI / uGUI dan TextMeshPro; prompt/dialog U03 sudah tersedia |
| Cakupan graybox saat ini | Posko, rumah, lima map level, tiga penerima per level, timer, dialog, hasil, retry, preman dan genangan |
| Integrasi berikutnya | Cerita, visual karakter dan lingkungan, penempatan serta kalibrasi rintangan final |
| Waktu | Rencana awal 21 hari dan cadangan hingga 28 hari; estimasi untuk cakupan revisi perlu disusun ulang |

Urutan development **gameplay-first**: loop pengantaran, progres harian, preman dan genangan tersedia sebagai graybox; integrasi cerita, aset final dan pengujian akhir menyusul.

Tidak termasuk rancangan saat ini: sistem battle, item, weapon dan reward pertempuran; juga multiplayer, akun/backend, database server, crafting, serta chapter di luar lima level.

## Status saat ini

### Ringkasan revisi

| Kode | Kebutuhan | Status |
| --- | --- | --- |
| R01, R03–R06 | Alur harian, rumah, posko, lima map, pengantaran, timer, hasil dan retry | Core graybox tersedia; uji ulang setelah revisi |
| R02, R07 | Prolog dan dialog cerita berurutan | Naskah dan core dialog tersedia; integrasi cerita final menyusul |
| R08–R12 | Reward, item/weapon, boss battle, encounter pilihan dan bantuan warga untuk membuka jalan | Dikeluarkan dari cakupan revisi |
| R13–R14 | Save/load tiga slot, menu utama dan menu ingame | Core tersedia; verifikasi ulang setelah revisi |
| R15 | Regresi dan build | Uji ulang dan build revisi diperlukan |

**Rintangan graybox:** setiap level memiliki satu preman dan satu genangan. Preman mendeteksi dalam radius 7 m, mengejar dengan kecepatan 2,8 m/detik, mengurangi timer 10 detik saat kontak, lalu memiliki cooldown 2 detik. Setelah tiga kontak yang terkena penalti, preman menghilang sampai misi diulang. Pemain dapat melewati preman agar tidak terjebak, sementara preman tetap tertahan collider lingkungan. Genangan mengurangi kecepatan jalan dan lari menjadi 50% selama berada di dalamnya. Nilai dan penempatan ini sementara; perlu playtest dan kalibrasi.

**Perbaikan rumah P01:** collider tak terlihat menutup sisi lantai yang terbuka tanpa menghalangi interaksi pintu. Sampling animasi karakter dilakukan setelah gerak pada frame yang sama; klip jalan dan lari memakai loop pose. Pengujian Play Mode rumah lulus 5/5, termasuk jalur keluar dan kontinuitas state animasi pada simulasi 30/144 fps. Build `Builds/Windows-HouseFix/JejakAsa.exe` berhasil dibuat dan dibuka hingga Vicky bergerak di rumah; kelancaran visual tetap perlu ditinjau langsung.

Save lama tetap mempertahankan field inventori/reward untuk kompatibilitas data, tanpa fungsi gameplay. Aset dan visual final belum menjadi bagian penerimaan revisi ini.

## Pembagian tiga role

Dua anggota berkolaborasi pada A/B, satu anggota menangani C. Tidak ada penetapan nama di README. Setiap tugas/file tetap memiliki satu pemilik untuk mencegah konflik edit.

| Role | Tanggung jawab | Batas progres saat ini |
| --- | --- | --- |
| A — Gameplay dan integrasi | Gerak, interaksi, misi/timer/trust, integrasi scene dan build | Loop misi dan menu graybox tersedia; revisi perlu diuji. |
| B — Verifikasi dan rintangan | Regresi alur, timer, retry dan save/load; rintangan graybox | Pengejaran dan genangan tersedia; kalibrasi dan integrasi visual final menyusul. |
| C — Visual, level, UI, audio | Aset low-poly, layout, prefab visual/UI, animasi, suara, serta keterbacaan | Integrasi final dan pengujian bersama A/B masih diperlukan. |

## Tracking penerimaan dan revisi core

Hasil uji graybox lama tidak otomatis membuktikan revisi ini. Kriteria battle, boss, item/weapon, encounter pilihan, dan reward sebelumnya tidak berlaku lagi.

- [ ] **R01, R03–R06 — Siklus dan misi:** uji rumah → posko → level → laporan → rumah untuk lima hari, tiga penerima per map, timer, hasil dan retry.
- [ ] **R02, R07 — Cerita:** hubungkan prolog dan dialog sesuai [storyboard](docs/storyboard-dialog-jejak-asa.md); jaga urutan dialog setelah retry dan load.
- [ ] **Preman pengejar — graybox:** uji radius, pengejaran, pengurangan waktu, pause dan retry; kalibrasi kesulitan setelah integrasi.
- [ ] **Genangan — graybox:** uji perlambatan dan pemulihan kecepatan; kalibrasi penempatan setelah integrasi.
- [ ] **R13–R14 — Save/load dan menu:** uji tiga slot, konfirmasi overwrite/load/keluar, serta menu ingame Lanjutkan, Pengaturan, Muat dan Keluar.
- [ ] **R15, U19–U21 — Integrasi dan build:** uji seluruh alur, kamera, audio, UI, build Windows dan komputer kedua setelah integrasi akhir.

Mr. Deddy dan Mr. Gas Oke tetap tokoh cerita, bukan boss battle. Detail penempatan rintangan, jumlah penerima final tiap level, aset dan dialog final masih perlu disepakati.

## Paket delivery revisi A/B

Paket **AB-D01–AB-D08** melacak pengerjaan core revisi secara terpisah dari U01–U21 dan AB-M lama. A dan B dikerjakan kolaboratif oleh dua anggota; pembagian berikut adalah **tanggung jawab role, bukan penetapan nama**. Tentukan satu pemilik file/scene untuk setiap pekerjaan agar perubahan tidak saling menimpa. Checklist `[ ]` ditutup hanya setelah hasilnya diuji dan bukti dicatat.

### AB-D01 — Kontrak alur dan pembagian kerja · R01

- [x] **A:** petakan transisi `rumah → posko → map level → posko → rumah`, state siang/istirahat, titik masuk/keluar, serta kondisi berhasil dan gagal; terdokumentasi di [docs/r01-kontrak-alur.md](docs/r01-kontrak-alur.md) dan diterapkan di `GameFlowManager.cs`.
- [ ] **B:** verifikasi kontrak hasil misi, retry, dan save/load setelah revisi.
- [x] **A/B + C:** sepakati skala, penempatan gerbang, batas area interaksi, kebutuhan visual/animasi, dan kepemilikan scene/file berdasarkan [C-D01](docs/cd01-arah-visual.md).
- [x] **A/B:** tiga slot save manual di rumah; tidak ada autosave tengah level.
- [ ] **Bukti terima:** verifikasi ulang kontrak [R01](docs/r01-kontrak-alur.md) setelah revisi.

### AB-D02 — Posko, rumah, dan gerbang graybox · R03–R04

- [x] **A:** buat satu map posko berupa lapangan terbuka graybox (`Posko.unity`, 28×24 m); rumah tetap berupa map interior (`Rumah.unity`, 14×10 m), tanpa map luar rumah.
- [x] **A:** sediakan satu gerbang bersama di posko (`PoskoGateController.cs`); saat didekati pemain dapat memilih `Pulang ke Rumah`, `Berangkat ke Level`, atau `Batal`. Tujuan yang belum tersedia menunjukkan alasan sesuai progres.
- [x] **A:** perpindahan rumah → posko memakai pintu/gerbang (`HomeInteractionController.cs`); dari rumah tidak ada akses langsung ke level.
- [x] **A:** sediakan titik interaksi Jonathan, Ophelia, Ibu, tempat tidur (istirahat/save) dan titik transisi menggunakan graybox via `StoryNpcController.cs`.
- [x] **B:** uji kedua arah transisi, batal, tujuan terkunci, input/fokus, collision serta spawn agar tidak memicu perpindahan berulang atau kehilangan state.
- [x] **Bukti terima:** `Rumah.unity` dan `Posko.unity` terdaftar di Build Settings; transisi gerbang, dialog Pak Jonathan, Ophelia, dan Ibu berfungsi dengan kontrol WASD & E.
- **Pembaruan posko:** kontrol Shift kiri/kanan menggandakan kecepatan jalan; animasi karakter lari dari lab belum terhubung ke gameplay. Area interaksi memilih objek terdekat, dan briefing serta pengambilan paket diperlukan sebelum berangkat.
- **Uji terarah Play Mode:** `PoskoFlowIntegrationTests` membuktikan rumah → briefing dan ambil paket → tiga penyerahan → laporan Jonathan/Ophelia → rumah; uji alur memindahkan pemain ke titik interaksi. Uji rute terpisah memakai input gerak + Shift untuk mencapai semua titik posko; `U02MovementTests` memeriksa kecepatan kedua Shift dan kembali berjalan.

### AB-D03 — Progres, lima map, dan retry · R05–R06

- [ ] **A:** buat lima map level yang terpisah dari posko, rumah dan satu sama lain. Aset modular boleh digunakan ulang, tetapi tiap map memiliki layout dan penempatan sendiri.
- [ ] **A:** alirkan level aktif, progres, hari, spawn dan hasil antara posko, level dan rumah; level berikutnya mengikuti progres dan istirahat.
- [ ] **A/B:** uji penyerahan, timer, trust dan hasil pada setiap level. Tiga pengantaran tepat waktu menyelesaikan level.
- [ ] **B:** uji berhasil, waktu habis, retry berulang dan kembali ke posko tanpa menghapus progres sebelumnya.
- [ ] **Bukti terima:** lima map dapat diakses sesuai progres; transisi, hasil, timer, penerima, dan retry lulus pada map nyata. Layout final divalidasi ulang bersama C-D03.

### AB-D04 — Prolog dan dialog · R02, R07

- [ ] **A:** hubungkan prolog dan dialog mengikuti [storyboard](docs/storyboard-dialog-jejak-asa.md).
- [ ] **A/B:** uji briefing → level → laporan Jonathan → dialog Ophelia → pulang dan penutup level 5; pastikan tahap dialog tetap benar setelah retry/load.
- [ ] **A/B + C:** tentukan pemicu, pose/animasi dan handoff dialog.

### AB-D05 — Preman pengejar

- [ ] Uji pengejaran, kontak dan reset pada lima level; sesuaikan rute dan nilai setelah playtest bersama.

### AB-D06 — Rintangan permukaan

- [ ] Uji perlambatan jalan/lari dan keluar dari genangan; sesuaikan penempatan dan visual final bersama Role C.

### AB-D07 — Save/load dan main menu · R13–R14

- [ ] **A:** simpan dan muat hari, level terbuka/selesai dan tahap cerita, termasuk aturan istirahat di rumah.
- [ ] **A:** ubah Main Menu menjadi `Mulai`, `Lanjutkan`, `Load`, dan `Setting`; kondisi tanpa save, pemilihan slot, overwrite, dan setting mengikuti keputusan AB-D01.
- [ ] **B:** uji simpan → tutup → buka → lanjut, pemilihan save, game baru dan konfirmasi overwrite; gagal menyimpan/memuat tidak boleh diam-diam menghapus data lama atau membuka hari berikutnya.
- [ ] **A/B + C:** hubungkan menu/save UI C-D05 setelah state dan input-nya disepakati; periksa kamera/audio saat berganti scene.
- [ ] **Bukti terima:** progres yang disimpan pulih pada executable dan semua tombol yang ditampilkan berfungsi.

### AB-D08 — Integrasi, regresi, dan build · R15

- [ ] **A/B + C:** gabungkan aset C-D02–C-D07 secara bertahap; pertahankan controller/collider/trigger dan periksa jalur setiap map setelah penggantian visual.
- [ ] **B:** jalankan regresi terintegrasi dari prolog sampai akhir level 5, termasuk transisi map, pause, retry, save/load dan pergantian hari.
- [ ] **A/B + C:** periksa kamera/audio, keterbacaan dan fokus UI pada 1280×720 serta 1920×1080, performa dan daftar lisensi/aset yang benar-benar dipakai.
- [ ] **A/B:** buat build Windows dari source yang sama dan uji menu, misi, save/load, retry, keluar, serta offline pada komputer kedua; catat hasil dan masalah.
- [ ] **Bukti terima:** bukti uji dan build terintegrasi tersedia; tidak ada blocker. Kriteria final C-D08 dan U19–U21 tetap diperiksa ulang, bukan otomatis lulus.

**Cara melacak:** pada tiap handoff catat ID paket, perubahan/versi, file dan scene terdampak, dependensi C, hasil uji, bukti screenshot/video/log, serta masalah yang masih terbuka. Waktu pelaksanaan revisi perlu diestimasi ulang; label D pada milestone lama bukan jadwal pasti untuk delapan paket baru.


## Milestone awal graybox dan Role C

Tabel ini mempertahankan rencana dan bukti awal; status dan hari D tidak otomatis berlaku untuk revisi lima level. Gunakan **AB-D01–AB-D08** untuk delivery A/B revisi dan **C-D01–C-D08** untuk delivery aset C. Estimasi jadwal revisi perlu disusun ulang.

`Selesai` memerlukan bukti pada kolom kriteria. `Sebagian` berarti baru sebagian output milestone yang lulus; kriteria tersisa tetap harus dikerjakan. `Belum` pada A/B berarti hasilnya belum tersedia atau belum dinyatakan selesai. `Belum dilaporkan` pada C berarti belum ada bukti dari pemiliknya, **bukan klaim bahwa C belum bekerja**. Tidak ada nama anggota yang ditetapkan di tabel ini.

### Jalur A/B — Sistem gameplay dan verifikasi

| ID | Target | Output dan pembagian kerja | Kriteria selesai / bukti | Status |
| --- | --- | --- | --- | --- |
| AB-M01 | D1 | Setup proyek, repository, clone, dan build awal. Satu anggota menyiapkan; anggota lain memeriksa. | Proyek 6000.3.27f1 hasil clone dan build awal berjalan. Bukti U01 berdasarkan konfirmasi anggota A/B; tidak menunggu laporan C untuk menutup milestone A/B ini. | Selesai |
| AB-M02 | D2–D3 | A: graybox navigasi, gerak, collision, kamera. B: regresi dan uji jalur. | U02 lulus: lima tes Play Mode serta pengujian ulang jalur setelah perubahan pembatas. Tidak mencakup input lock state atau loop misi. | Selesai |
| AB-M03 | D2–D3 | Arena battle lama | Dihapus dari cakupan revisi. | Tidak berlaku |
| AB-M04 | D4–D7 | Interaksi, penerima, paket/trust, timer, hasil, pause, retry. | Uji ulang dengan aturan pengantaran revisi. | Sebagian |
| AB-M05–AB-M06 | D3–D10 | Battle dan encounter lama | Dihapus dari cakupan revisi; pengejaran dan perlambatan adalah rencana terpisah. | Tidak berlaku |
| AB-M07 | D11 | MainMenu, navigasi dan konfirmasi keluar sesi. | Uji ulang menu dan keluar pada build revisi. | Sebagian |
| AB-M08 | D12–D14 | Integrasi aset/UI/audio dari C, pengujian alpha, dan pembekuan fitur. | Semua fitur wajib tersambung; U19/U20 diuji bersama C pada build terintegrasi. Belum cukup jika hanya scene percobaan yang bekerja. | Belum |
| AB-M09 | D15–D18 | Playtest, perbaikan, kalibrasi angka, regresi, dan release candidate. | Catatan playtest nyata; tidak ada blocker atau tombol kosong; kandidat rilis cocok dengan source dan aturan terbaru. | Belum |
| AB-M10 | D19–D21 | Stabilisasi, build bersih, pengujian komputer kedua, dan pengumpulan source/build. | U21 lulus; build final, source, bukti uji, serta known issues non-blocking siap. | Belum |

**Batas status saat ini:** bukti Play Mode lama bukan bukti revisi terbaru. UI dan audio masih graybox; penerimaan visual/audio final serta uji komputer kedua belum dilakukan.

### Jalur C — Visual, level, UI, dan audio

**C-D01 dalam pengerjaan:** [arah visual dan hasil uji skala](docs/cd01-arah-visual.md) serta [checklist delivery Role C](docs/daftar-aset-role-c.md). Pengguna menyetujui arah visual dan model Meshy Vicky serta memberi referensi tokoh lain. Jalur map perlu diuji ulang setelah perubahan.

**Katalog lingkungan lokal:** `Blender/AssetLibrary_Staging/JejakAsa_Environment_Catalog.blend` berisi 340 entri sumber/variasi sebelumnya serta **20 objek rumah** yang di-append pengguna. Rumah `house_1`–`house_20` dikelompokkan dalam collection `House20`; tekstur rumah sudah dikemas. Sebagian parent aset lain disesuaikan untuk display katalog, **bukan skala final Unity**; 20 rumah mempertahankan skala dan posisi saat di-append. Model MC dan arsip asli tidak diubah.

**Vegetasi pilihan lokal:** `Blender/AssetLibrary_Staging/JejakAsa_Environment_Vegetasi.blend` adalah salinan katalog berisi collection `Vegetasi_Terpilih`: lima pohon (tiga berdaun dan dua pinus), satu semak, dan tiga rumpun rumput. Objek sumber forest tetap ada; salinan memakai mesh/material terpisah dan ukuran tampilan usulan. Aset belum diekspor/diuji di Unity, belum dinilai cocok sebagai spesies Jakarta, dan lisensi paket forest masih perlu diverifikasi.

Folder `Blender/` diabaikan Git; katalog dan arsip tersebut **tidak otomatis ikut push/clone**. Delapan FBX ASCII rumah suburban tidak didukung importer Blender, sehingga katalog memakai versi GLB dari [paket Kenney yang sama di Poly Pizza](https://poly.pizza/bundle/Suburban-Houses-Pack-dZQwy8vcDv), tercantum CC0. Lisensi aset lain, khususnya paket Urban City yang dokumennya menyebut pembelian, perlu diverifikasi sebelum dibagikan atau dipakai dalam build.

| ID | Target | Output yang diserahkan C | Kriteria selesai / bukti | Status |
| --- | --- | --- | --- | --- |
| C-M01 | D1–D2 | Setup proyek C, skala/palet, daftar kebutuhan aset, serta sumber/lisensi. | C dapat membuka proyek yang sama; daftar kebutuhan dan sumber aset dapat ditinjau. U01 A/B tidak otomatis membuktikan setup C. | Belum dilaporkan |
| C-M02 | D2 | Rancangan layout dan penempatan tujuan bersama A/B. | Pos awal, tujuan, gang dan rintangan yang direncanakan disepakati. | Belum dilaporkan |
| C-M03 | D3–D5 | Prefab visual pemain, warga dan tokoh cerita low-poly. | Preview karakter tersedia; integrasi dan penerimaan final masih diperlukan. | Sebagian |
| C-M04 | D4–D6 | Prefab briefing, HUD, dialog, pause dan hasil/retry. | Teks, anchor, fokus, tombol dan state lengkap; referensi pengikatan controller disertakan. | Belum dilaporkan |
| C-M05 | D5–D8 | Lingkungan modular, bangunan, jalan, paket, penanda penerima, dan dekor minimum. | Prefab diserahkan untuk mengganti graybox; A/B memasangnya dan memeriksa ulang jalur/collider. C tidak menimpa logika atau scene integrasi tanpa handoff. | Belum dilaporkan |
| C-M06 | D7–D10 | Idle/jalan/lari, berbicara, memberi/menerima bantuan dan animasi cerita. | Preview lokal memiliki klip bersama; integrasi animasi ke gameplay perlu diuji. | Sebagian |
| C-M07 | D11 | Prefab MainMenu, musik eksplorasi, SFX tindakan utama dan pencahayaan. | Prefab dan sumber/lisensi siap; suara tidak bertumpuk saat transisi. | Belum dilaporkan |
| C-M08 | D12–D14 | Pemeriksaan visual/UI/audio pada build terintegrasi. | U19/U20 diperiksa bersama A/B: tampilan 1920×1080 dan 1280×720, fokus UI, prompt, penanda, serta audio transisi benar. | Belum dilaporkan |
| C-M09 | D15–D18 | Perbaikan keterbacaan dan performa visual berdasarkan playtest. | Masalah yang ditemukan pengguna ditutup, tampilan tidak menghambat navigasi, dan hasil pemeriksaan dicatat pada kandidat rilis. | Belum dilaporkan |
| C-M10 | D19–D21 | Kredit aset, screenshot build aktual, dan materi demo final. | Kredit sesuai aset yang benar-benar dipakai; gambar/demo cocok dengan build rilis A/B, bukan hanya render konsep. | Belum dilaporkan |

**Preview animasi C-M06:** `Unity/JejakAsa/Assets/_Project/AnimationLab/RerigLocal/RerigPreview.unity` adalah scene preview final dengan tombol **Lari** untuk 10 karakter Humanoid menggunakan satu klip Mixamo *Running* in-place yang berulang. Shift kiri/kanan menggerakkan controller gameplay lebih cepat, tetapi animasi lari belum dihubungkan ke karakter gameplay. Folder `AnimationLab/` dan sumber `Blender/` diabaikan Git, sehingga aset preview ini belum ikut push/clone dan perlu handoff terpisah sebelum integrasi A/B.

**Handoff karakter lokal:** seluruh dependensi preview final berada di `Unity/JejakAsa/Assets/_Project/AnimationLab/RerigLocal/`, termasuk `Materials/`, `Textures/`, FBX, controller dan scene. Untuk berbagi di luar Git, sertakan seluruh folder tersebut bersama semua file `.meta`, termasuk `AnimationLab.meta` dan `RerigLocal.meta`, lalu ekstrak pada path yang sama. Versi preview dan rig lama yang tidak dipakai telah dihapus dari `AnimationLab`; sumber `Blender/` dan `ArtSource/` tidak diubah.

### Titik handoff dan penerimaan bersama

- **D2:** A/B dan C menyepakati layout, skala, penempatan penanda dan kontrak logika misi.
- **D6:** C menyerahkan prefab UI awal. A/B menghubungkannya ke logika nyata. Jika visual belum final, A/B tetap membuktikan loop dengan UI sederhana yang berfungsi; tidak menunggu semua aset selesai.
- **D10–D12:** visual, animasi, UI dan audio digabungkan dengan sistem misi. Perubahan layout atau collider memerlukan uji jalur kembali.
- **D14:** alpha dan pembekuan fitur diperiksa bersama. U19/U20 hanya dicentang sesudah hasil integrasi diuji, bukan ketika prefab diserahkan.
- **D21:** kedua jalur bertemu pada build/source final yang sama. Kebutuhan kontribusi C tidak dihapus hanya karena milestone A/B sudah selesai, dan sebaliknya.
- **D22–D28:** cadangan bersama hanya untuk pengujian, perbaikan bug dan latihan presentasi.

### Pekerjaan berikutnya

1. A/B dan C meninjau **AB-D01** bersama: kontrak level, gerbang, save, pembagian file, serta layout C-D01.
2. A/B menyiapkan posko/rumah dan progres dalam graybox melalui AB-D02–AB-D03; C menyiapkan aset secara paralel setelah kebutuhan tiap paket jelas.
3. Uji manual graybox sebelumnya tetap berguna sebagai baseline; ulangi skenario yang terdampak setelah revisi dan integrasi aset. U19–U21 final tidak otomatis lulus.

**Referensi karakter dari pengguna:** [Vicky](docs/visual/asset-guides/refs/Vicky.jpg), [Jonathan](docs/visual/asset-guides/refs/Jonathan.jpg), [Ophelia](docs/visual/asset-guides/refs/Ophelia.jpg), [Preman](docs/visual/asset-guides/refs/Preman.jpg), [Mr. Deddy](docs/visual/asset-guides/refs/Mr.Deddy.jpg), [Mr. Gas Oke](<docs/visual/asset-guides/refs/Mr. Gas Oke.jpg>), [Ibu](docs/visual/asset-guides/refs/Ibu.jpg), [Ayah](docs/visual/asset-guides/refs/Ayah.jpg), [warga perempuan](docs/visual/asset-guides/refs/Warga-Female.jpg), dan [warga laki-laki](docs/visual/asset-guides/refs/Warga-Male.jpg). Gambar memperlihatkan depan, tiga-perempat, dan belakang; bukan model 3D.

**Status produksi karakter:** pengguna memilih model MC Meshy yang sudah ada. Jonathan, Ophelia, Mr. Gas Oke, Mr. Deddy, Ayah, Ibu, Preman, warga perempuan, dan warga laki-laki telah digenerate dari potongan depan referensi masing-masing. GLB sumber di bawah masih mentah; salinan ber-rig dan animasi tersedia dalam preview Unity lokal, tetapi belum diintegrasikan ke scene gameplay atau diterima sebagai aset final.

### Sembilan karakter Meshy — sumber

| Karakter | File GLB sumber | Tinggi | Triangle |
| --- | --- | ---: | ---: |
| Jonathan | [Jonathan_Original.glb](ArtSource/Characters/Jonathan_Meshy/Jonathan_Original.glb) | 1,75 m | 307.837 |
| Ophelia | [Ophelia_Original.glb](ArtSource/Characters/Ophelia_Meshy/Ophelia_Original.glb) | 1,60 m | 576.285 |
| Mr. Gas Oke | [Mr_Gas_Oke_Original.glb](ArtSource/Characters/Mr_Gas_Oke_Meshy/Mr_Gas_Oke_Original.glb) | 1,70 m | 364.347 |
| Mr. Deddy | [Mr_Deddy_Original.glb](ArtSource/Characters/Mr_Deddy_Meshy/Mr_Deddy_Original.glb) | 1,75 m | 277.207 |
| Ayah | [Ayah_Original.glb](ArtSource/Characters/Ayah_Meshy/Ayah_Original.glb) | 1,70 m | 208.318 |
| Ibu | [Ibu_Original.glb](ArtSource/Characters/Ibu_Meshy/Ibu_Original.glb) | 1,65 m | 752.454 |
| Preman | [Preman_Original.glb](ArtSource/Characters/Preman_Meshy/Preman_Original.glb) | 1,75 m | 555.115 |
| Warga perempuan | [Warga_Perempuan_Original.glb](ArtSource/Characters/Warga_Perempuan_Meshy/Warga_Perempuan_Original.glb) | 1,65 m | 385.569 |
| Warga laki-laki | [Warga_Laki_Original.glb](ArtSource/Characters/Warga_Laki_Meshy/Warga_Laki_Original.glb) | 1,75 m | 481.975 |

Setiap GLB diperiksa mempunyai mesh dan tiga gambar tekstur tertanam. Blender menampilkan peringatan sampler tekstur saat ekspor; tampilan saat reimport atau di Unity masih harus diuji. Setiap model memiliki satu UV map, belum memiliki armature, dan memiliki boundary edges; kualitas deformasi dan performa belum dibuktikan. Angka triangle tersebut tinggi untuk penggunaan bersamaan, sehingga jangan menganggap file mentah otomatis siap untuk build.

Sembilan generasi Meshy selesai dalam tiga tahap (termasuk warga laki-laki yang dialihkan ke Meshy setelah percobaan Tripo terkendala saldo kredit); saldo dan biaya aktual tidak dilaporkan tool, sehingga harus dilihat di dashboard Meshy. Salinan scene Blender terbaru dengan sembilan karakter tersedia secara lokal di `Blender/Generated_Characters_03/JejakAsa_Character_Set_03.blend`, collection `JA_Karakter_Generasi`; salinan scene sebelumnya dan `Blender/MC.blend` asli tidak ditimpa. Folder `Blender/` diabaikan Git, sedangkan GLB di `ArtSource/` berada di luar folder tersebut.

### MC Meshy — hasil dan batasan

- [GLB sumber hasil ekspor Blender](ArtSource/Characters/MC_Meshy/JejakAsa_MC_Original.glb): tinggi sekitar 1,70 m, 653.749 triangle, satu UV map dan tiga tekstur tertanam. Ekspor GLB mengeluarkan peringatan tentang sampler tekstur; periksa ulang bila menggunakan format ini di pipeline lain.
- Salinan uji memakai Decimate non-destruktif di Blender dan diekspor sebagai [FBX preview Unity](Unity/JejakAsa/Assets/_Project/Art/Characters/MC/JejakAsa_MC_Unity_Test.fbx): 32.687 triangle; tinggi terukur 1,70 m setelah koreksi skala impor. Model asli tidak dikurangi.
- [Prefab preview Unity](Unity/JejakAsa/Assets/_Project/Art/Characters/MC/MC_Meshy_Preview.prefab) memakai material URP/Lit dan base-color 2048×2048. Normal serta metallic/roughness asli tetap ada di GLB sumber, tetapi **belum dipetakan pada material preview**.
- [Screenshot Play Mode Unity](docs/visual/meshy-mc-unity-preview.png): preview dipasang sementara sebagai child Player, collider asli dipertahankan, dan tidak ada collider baru pada visual. Pada smoke test terpisah, karakter bergerak dan satu penyerahan ke warga mengubah terkirim/trust dari 0 ke 1. Scene Kampung dikembalikan bersih dan Player tersimpan tetap capsule.
- FBX **preview MC pada bagian ini** belum ber-rig atau beranimasi. Rig dan animasi terpisah sudah dicoba di `AnimationLab` lokal, tetapi belum diintegrasikan ke gameplay dan folder tersebut diabaikan Git. Sumber memiliki 50.113 boundary edges; Decimate bukan retopology dan dapat merusak UV/deformasi. Preview 32.687 triangle masih di atas target karakter awal dan belum diprofilkan.
- Tidak ada job Meshy tambahan untuk optimasi atau rigging. Verifikasi biaya sebenarnya pada dashboard Meshy; jangan menyimpan API key dalam repository. Hak penggunaan aset generasi bergantung ketentuan akun yang digunakan.

## Membuka dan menjalankan proyek

1. Clone repository ini. Di Unity Hub pilih **Add project from disk**, lalu pilih `Unity/JejakAsa`, bukan root repository atau folder `Assets`.
2. Buka menggunakan Editor **6000.3.27f1**. Tunggu import dan pemulihan package dari `Packages/` selesai; pembukaan pertama dapat memerlukan internet.
3. Untuk menguji alur lengkap, buka [`MainMenu.unity`](Unity/JejakAsa/Assets/_Project/Scenes/MainMenu.unity), tekan **Play**, lalu klik tab **Game**. Memulai langsung dari Kampung tetap berguna untuk debugging.
4. Pilih **Permainan Baru**, lalu konfirmasi. Bergerak dengan WASD atau panah; tahan Shift kiri/kanan untuk berlari.
5. Keluar dari rumah dengan E. Di posko, bicara dengan Jonathan, ambil paket bantuan dengan E, lalu gunakan gerbang untuk berangkat ke level.
6. Dekati penerima dan tekan E. Gunakan Enter, Spasi, Esc, atau tombol dialog untuk maju atau menutup sesuai jumlah baris.
7. Setelah misi berhasil, konfirmasi hasil untuk kembali ke posko, lapor kepada Jonathan, bicara dengan Ophelia, lalu pulang. Di tempat tidur rumah, simpan pada salah satu dari tiga slot atau istirahat untuk memajukan hari; penimpaan slot meminta konfirmasi.
8. Esc saat eksplorasi membuka menu ingame: Lanjutkan, Pengaturan, Muat, atau Keluar. Memuat dan keluar meminta konfirmasi. Save disimpan lokal melalui PlayerPrefs, bukan dalam repository.

### Pengujian manual U02

- Coba arah lurus, diagonal, tombol searah bersamaan, dan arah berlawanan.
- Jalan menuju sisi rumah; pemain harus tertahan. Input diagonal ke dinding harus tetap memungkinkan bergerak menyusuri sisi dinding.
- Lewati celah tengah dan gang memutar di kiri pembatas, lalu dekati ketiga penanda warga.
- Pastikan pemain tidak jatuh menembus lantai dan kamera tidak berputar mengikuti arah hadap.

Visual masih sederhana. Kamera belum memiliki penanganan occlusion bangunan atau pembatas posisi di tepi peta.

### Pengujian manual U03

1. Dekati capsule warga berwarna jingga. Dalam radius horizontal 1,8 meter muncul prompt `[E] Serahkan Bansos ...` untuk penerima yang belum dibantu; di luar jarak prompt menghilang.
2. Tekan E sekali: nama dan pesan warga muncul pada satu panel dialog. Pemain tidak berjalan selama panel terbuka.
3. Tahan E: dialog tidak terbuka berulang dan tidak berpindah ke warga lain. Tombol baru dianggap penekanan berikutnya setelah E dilepas dan ditekan kembali.
4. Tutup dengan klik Tutup, Enter, atau Esc, lalu pastikan pemain bisa berjalan lagi. E yang masih ditahan ketika menutup dialog tidak langsung membukanya kembali.
5. Ulangi pada ketiga warga. Pada misi saat ini E juga menyerahkan satu paket kepada penerima yang belum mendapat bantuan.

Pemilihan target berdasarkan jarak pada bidang XZ dan status aktif, bukan arah hadap pemain. Skenario beberapa warga berdekatan, jarak tepat di batas, dan target yang hilang diuji otomatis; tidak perlu mengubah layout utama untuk mencobanya.

### Pengujian manual pada build revisi

Build lama tidak mencerminkan revisi. Buat build baru, lalu catat langkah, hasil, serta screenshot bila ada masalah:

1. Uji menu, jeda timer, dialog dan kembali ke sesi dengan mouse serta keyboard.
2. Antar bantuan pada lima level; periksa hasil dan pergantian hari.
3. Biarkan waktu habis setelah satu penyerahan, pilih ulang misi tiga kali, dan periksa paket, penerima, timer serta progres level sebelumnya.
4. Uji save/load dan konfirmasi penggantian slot, kamera, audio serta UI pada 1920×1080 dan 1280×720.
5. Salin seluruh folder build ke komputer Windows lain dan uji tanpa Editor atau internet, termasuk keluar aplikasi.

### Musik sementara untuk graybox

Eksplorasi: [Loop Town — Fupi](https://opengameart.org/content/loop-town), CC0. Periksa kembali kredit aset yang benar-benar dipakai pada build final.


### Pengujian otomatis Play Mode

Di Unity buka **Window → General → Test Runner**, pilih **PlayMode**, lalu jalankan tes terkait perubahan. Sebelumnya 49/49 tes terarah lulus; setelah perbaikan rumah, 5/5 tes rumah dan animasi terarah lulus. Hasil ini bukan bukti integrasi aset final atau pengujian komputer kedua.

Tes mengirim input keyboard sintetis ke controller sebenarnya. Tes integrasi memuat Kampung aktual untuk menelusuri rute atau membuka dialog dan mengklik Tutup dengan input mouse. Scene percobaan dibongkar dan input fixture dipulihkan setelah selesai. Jangan menghentikan runner atau mengubah scene di tengah tes.

Jika layout berubah, tinjau waypoint pada tes rute serta buktikan semua penerima tetap dapat dijangkau. Kelulusan lama bukan bukti bahwa perubahan baru sudah aman.

## Membuat build Windows terbaru

1. Keluar dari Play Mode dan simpan scene.
2. Buka **File → Build Profiles** dan aktifkan Windows.
3. Pastikan scene aktif meliputi `MainMenu`, `Rumah`, `Posko`, `Kampung`, dan `Kampung_L2`–`Kampung_L5`; scene contoh tidak perlu diaktifkan.
4. Build ke folder baru di `Unity/JejakAsa/Builds/`, bukan ke dalam `Assets`.
5. Jalankan executable di luar Editor dan bagikan seluruh folder hasil build jika diperlukan, bukan `.exe` saja.

Folder build dan cache Unity tidak di-commit. Build yang akan dibagikan harus dibuat dari source yang sama dengan commit rilisnya.

## Struktur penting

```text
README.md                         Deskripsi dan progres aktif
Unity/JejakAsa/
  Assets/_Project/
    Scenes/Kampung.unity           Scene gameplay awal
    Scripts/                      Gerak, kamera, target NPC, dan interaksi
    Materials/                    Material graybox
    Tests/PlayMode/               Regresi gerak, misi, alur dan menu
  Assets/TextMesh Pro/             Resource font/settings/shader bawaan TMP
  Packages/                       Manifest dan lock file package
  ProjectSettings/                Konfigurasi dan versi Editor
  Builds/                         Hasil build lokal; diabaikan Git
docs/                             Acuan desain, rencana, presentasi, bukti visual
```

- [PlayerController](Unity/JejakAsa/Assets/_Project/Scripts/PlayerController.cs)
- [FixedIsometricCamera](Unity/JejakAsa/Assets/_Project/Scripts/FixedIsometricCamera.cs)
- [Tes gerak/collision](Unity/JejakAsa/Assets/_Project/Tests/PlayMode/U02MovementTests.cs)
- [Tes jalur Kampung](Unity/JejakAsa/Assets/_Project/Tests/PlayMode/U02KampungRouteTests.cs)
- [PlayerInteraction](Unity/JejakAsa/Assets/_Project/Scripts/PlayerInteraction.cs)
- [InteractionTarget](Unity/JejakAsa/Assets/_Project/Scripts/InteractionTarget.cs)
- [Tes interaksi](Unity/JejakAsa/Assets/_Project/Tests/PlayMode/U03InteractionTests.cs)
- [Tes dialog dan mouse di Kampung](Unity/JejakAsa/Assets/_Project/Tests/PlayMode/U03KampungInteractionTests.cs)
- [Desain game](docs/desain-game.md)
- [Rencana kerja awal (historis)](docs/rencana-development-jejak-asa.md) dan [presentasi awal](<docs/Kelompok Jejak Asa.pptx>); cakupan gameplay mengikuti revisi pada README dan storyboard.

### Resource UI dan lisensi

U03 mengimpor **TMP Essential Resources** dari package uGUI/TextMeshPro yang sudah terpasang, bukan memasang package baru. Folder `Assets/TextMesh Pro` dan file `.meta` harus ikut Git agar font serta UI tampil pada hasil clone.

Font Liberation Sans disertai [SIL Open Font License 1.1](<Unity/JejakAsa/Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt>). Pertahankan pemberitahuan hak cipta dan lisensinya pada distribusi aset/build. Ini bukan penetapan lisensi untuk seluruh repository.

## Aturan kolaborasi dan pembaruan tracker

- Pull sebelum mengedit; satu pemilik per scene/file. A/B membagi tugas dengan jelas meskipun saling membantu. C menyerahkan prefab visual/UI kepada pemilik scene integrasi.
- Commit `Assets` beserta `.meta`, `Packages`, dan `ProjectSettings`. Jangan commit `Library`, `Temp`, cache, kredensial, atau hasil build. Jangan mengabaikan seluruh file `.meta`.
- Update checkbox di README hanya setelah implementasi dan skenario terkait terbukti. Sebutkan apakah bukti berasal dari laporan anggota, tes Editor, atau build; jangan menyamakan ketiganya.
- Ketika perubahan mematahkan kriteria yang sebelumnya lulus, buka kembali status terkait sampai diperbaiki dan diuji. Gambar konsep, prefab kosong, atau tombol tanpa efek bukan fitur selesai.
- Catat batas yang belum diuji, terutama build terbaru, komputer kedua, aset/lisensi, dan kontribusi C yang belum dikonfirmasi. Jangan menetapkan nama atau persentase progres tanpa bukti.
- MCP for Unity dipakai untuk otomasi Editor lokal, bukan backend game. Jika digunakan, server tetap di localhost dan scene disimpan sebelum perubahan; jangan membuka endpoint ke internet.
