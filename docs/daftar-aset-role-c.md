# Jejak Asa — Daftar Aset dan Delivery Role C

**Status: rencana produksi, belum disetujui atau selesai.** Daftar ini mengikuti [storyboard dan dialog draft](storyboard-dialog-jejak-asa.md) serta tracker revisi core R01–R15 di [README](../README.md). Centang item hanya setelah aset diterima sesuai kriteria, bukan hanya setelah gambar referensi atau file sumber dibuat. Perbarui progres di repository ini; milestone lama C-M01–C-M10 pada README tetap menjadi catatan rencana sebelumnya.

## 1. Arah dan batas produksi

- **Visual target:** game **3D anime stylized dengan geometri low-poly dan sedikit sentuhan realistis**. Bentuk wajah, rambut, proporsi, pakaian, dan warna tetap memiliki karakter; tidak harus tampak seperti kubus atau permukaan polos seluruhnya.
- **Latar:** kampung fiktif di Jakarta Utara. Perlihatkan lingkungan hidup, aktivitas, dan identitas warganya dengan hormat; kemiskinan bukan dekorasi atau karakterisasi penduduk.
- **Cara berhemat:** setiap level tetap mempunyai map sendiri, tetapi bagian bangunan, prop, material, rig, dan animasi boleh digunakan ulang antar-map. Lima level juga tidak mewajibkan 15 model warga unik.
- **Gambar referensi bukan model:** PNG/JPG concept art dapat membantu mendesain atau menjadi masukan generator, tetapi tidak dapat langsung menggantikan karakter/map 3D yang harus terlihat dari berbagai sudut. Aset final perlu mesh, material, dan bila diperlukan rig/animasi yang kompatibel dengan Unity.
- **Keputusan yang masih terbuka:** nama boss terakhir (Mr. Gas Oke/Mr. Oke Gas), jumlah penerima final, jenis/efek item dan weapon, daftar pengaturan, serta detail layout masing-masing level. Sepakati detail aset bersama A/B sebelum produksi final.
- **Tanpa biaya otomatis:** penggunaan generator/asset store harus dilihat biaya, hak pakai, dan kuotanya terlebih dahulu. Daftar ini tidak memberi izin untuk memakai kredit berbayar tanpa arahan pemilik akun.

### Referensi yang tersedia

- [Storyboard dan seluruh dialog draft](storyboard-dialog-jejak-asa.md).
- Referensi karakter yang diberikan pengguna: [Vicky](visual/asset-guides/refs/Vicky.jpg), [Jonathan](visual/asset-guides/refs/Jonathan.jpg), [Ophelia](visual/asset-guides/refs/Ophelia.jpg), [Preman](visual/asset-guides/refs/Preman.jpg), [Mr. Deddy](visual/asset-guides/refs/Mr.Deddy.jpg), [Mr. Gas Oke](<visual/asset-guides/refs/Mr. Gas Oke.jpg>), [Ibu](visual/asset-guides/refs/Ibu.jpg), [Ayah](visual/asset-guides/refs/Ayah.jpg), [warga perempuan](visual/asset-guides/refs/Warga-Female.jpg), dan [warga laki-laki](visual/asset-guides/refs/Warga-Male.jpg).
- [Panduan visual yang sudah ada](visual/asset-guides/). Beberapa gambar lebih lama; gunakan sebagai bahan eksplorasi, bukan mengunci arah 3D anime baru.
- [Sumber MC Meshy](../ArtSource/Characters/MC_Meshy/JejakAsa_MC_Original.glb) dan [FBX preview](../Unity/JejakAsa/Assets/_Project/Art/Characters/MC/JejakAsa_MC_Unity_Test.fbx). Preview sudah dicoba sementara, tetapi **belum rig/animasi final**; topology dan performanya masih harus diperiksa. Jangan menandainya sebagai aset siap pakai hanya karena file ada.
- Musik sementara [Loop Town](https://opengameart.org/content/loop-town) — Fupi dan [Urban Battle](https://opengameart.org/content/urban-battle) — MintoDog, keduanya tercantum CC0. Keduanya sudah dipakai pada graybox, belum dinyatakan audio final.

## 2. Paket delivery dan urutan prioritas

`[ ]` = belum diterima atau belum ada bukti. Urutan berikut adalah **urutan handoff**, bukan janji tanggal; A/B dapat membangun logika graybox secara paralel. Output paket boleh diserahkan bertahap, tetapi paket selesai hanya ketika semua item wajibnya lolos penerimaan.

| Paket | Fokus | Dipakai untuk | Ketergantungan utama |
| --- | --- | --- | --- |
| **C-D01** | Arah visual dan spesifikasi | Acuan seluruh aset | Review tim dan ukuran graybox |
| **C-D02** | Karakter + animasi vertikal pertama | Satu karakter bergerak di game | Uji rig/skin dan integrasi MC A/B |
| **C-D03** | Lingkungan modular dan props inti | Kampung, posko, rumah, arena | Layout, rute, collider, skala disepakati |
| **C-D04** | Karakter cerita dan musuh | Prolog sampai level 5 | Konsep tokoh dan rig kompatibel |
| **C-D05** | UI/UX dan ikon | Menu, dialog, misi, battle, save | Kontrak state/input A/B |
| **C-D06** | Animasi adegan, battle, bantuan warga | Cutscene sederhana dan gameplay | Model/rig, titik gerak dan trigger A/B |
| **C-D07** | Audio, pencahayaan, VFX minimum | Eksplorasi, cerita, battle | Sumber/lisensi dan scene terkait |
| **C-D08** | Integrasi, uji visual, dan handoff final | Build bersama A/B | Seluruh paket yang wajib sudah siap |

## 3. C-D01 — Arah visual dan layout

**Progres:** pengguna menyetujui arah visual dan memilih model Meshy Vicky yang sudah ada, meskipun tidak rapi. Referensi tokoh lain tersedia; gambar berisi depan, tiga-perempat dan belakang, bukan tampak samping ortografis. Uji collider pintu/gang sementara dan 5/5 tes gerak/rute graybox lama lulus; denah revisi belum menjadi scene yang dapat diuji. Penerimaan C-D01 tetap terbuka.

- [x] **Arah visual:** lembar referensi dan palet telah ditinjau dan disetujui pengguna; penerapannya dalam material/pencahayaan Unity tetap diuji pada tahap integrasi.
- [x] **Vicky:** pengguna memilih model Meshy yang sudah ada dan menerima bentuknya; rig, animasi, dan integrasi tetap pekerjaan C-D02.
- [ ] **Karakter lain:** referensi Jonathan, Ophelia, preman, Deddy, dan boss terakhir tersedia dari pengguna; pastikan kebutuhan tampak samping tepat dan konsistensi sebelum modeling. Gambar bukan model 3D.
- [ ] **Lokasi:** sketsa satu map posko dengan lingkungan sendiri, satu map interior rumah tanpa tampilan luar, lima map level terpisah, serta arena battle. Tandai spawn, titik interaksi, transisi, rute, encounter, dan area kamera yang dibutuhkan.
- [ ] **Lima level:** bersama A/B rancang map dan rute masing-masing level. Penggunaan ulang modul bangunan atau props diperbolehkan tanpa menjadikannya satu map level yang sama.
- [ ] **Tes skala dan jalur:**
  - [x] Ukur model Vicky dan uji collider player terhadap bukaan 1,5 × 2,2 m dan gang 2 m pada objek uji sederhana; kontrol negatif bukaan 0,8 m menahan player.
  - [x] Uji rute/gerak graybox Kampung lama: 5/5 Play Mode lulus, termasuk waypoint utama dan memutar.
  - [ ] Uji ukuran paket, animasi/clipping Vicky, serta jalur dan interaksi pada map posko, rumah, dan lima level revisi setelah scene tersedia.
- [ ] **Catatan keputusan:** warna, referensi, versi, perubahan yang diminta, dan keputusan tim tersedia agar aset berikutnya tidak dibuat dari asumsi yang berbeda.

**Handoff:** gambar PNG/JPG dan denah yang dapat dibaca, plus catatan ukuran dalam meter dan keputusan tim. Konsep belum menjadi bukti model/scene final.

## 4. C-D02 — Karakter pertama dan integrasi vertikal

### Karakter Vicky

- [ ] **Model Vicky:** kepala/wajah, rambut, tubuh, polo navy, celana, sepatu, headphone, dan sling bag terbaca dari jarak kamera game. Gunakan model Meshy yang ada bila setelah diperiksa memang cocok; tidak wajib menghasilkan model baru.
- [ ] **Material/tekstur:** warna, rambut, pakaian, mata, dan aksesori terbaca pada pencahayaan URP; tekstur terhubung pada material yang dipakai, bukan sekadar tersedia di folder.
- [ ] **Rig dan skin:** karakter dapat bergerak tanpa bagian tubuh/aksesori terlepas atau deformasi mencolok; proporsi dan arah model disepakati bersama A/B.
- [ ] **Animasi minimum pertama:** idle dan jalan, diuji dengan gerak aktual dari controller; model berhenti saat pemain berhenti.
- [ ] **Prefab visual Vicky:** sebagai visual child dari player, tidak mengganti `CharacterController`, input, interaksi, atau collision yang ada.
- [ ] **Uji vertikal:** bersama A/B jalankan, berjalan, mendekati satu warga, menyerahkan satu paket, dan kembali ke posko/scene uji; periksa skala, bayangan, collider, clipping, serta performa.

**Handoff:** sumber yang dapat diedit bila tersedia; model `.fbx`/`.glb`, tekstur, rig dan klip yang benar-benar diperlukan, material URP dan prefab visual; screenshot/rekaman Play Mode serta catatan masalah. Model yang berasal dari generator tetap memerlukan pemeriksaan rig, tekstur, dan hak penggunaan.

## 5. C-D03 — Lingkungan modular dan props

### Lingkungan lima map level

- [ ] **Jalan/gang:** segmen jalan lurus, belokan, persimpangan, dan jalan memutar yang dapat disusun ulang; lebar jalan dan jarak bebas sesuai controller.
- [ ] **Rumah modular:** dinding, atap, pintu, jendela, pagar, dan varian warna secukupnya agar rumah tidak terlihat identik; gunakan kembali bagian-bagian yang sama.
- [ ] **Batas dan penghalang:** tembok/pagar, pembatas gang, serta visual penghalang yang dapat dibuka setelah encounter terselesaikan.
- [ ] **Props kampung:** tiang/kabel, papan nama, pot/tanaman, tong atau barang lingkungan lain secukupnya untuk membangun suasana tanpa menghalangi rute.
- [ ] **Penanda dan tujuan:** penanda rumah penerima, titik interaksi, serta arah yang tetap terbaca tanpa bergantung pada warna saja.
- [ ] **Lima map level:** masing-masing memiliki layout, titik datang dari posko, jalur, warga, encounter dan titik boss/tujuan. Modul dan props boleh dipakai ulang antar-map; koordinasikan desain dengan A/B dan jangan menimpa scene integrasi.

### Posko

- [ ] Map posko mandiri: bangunan/area posko beserta lingkungan di sekelilingnya, dengan **satu gerbang** yang sama untuk datang dari dan pergi ke rumah atau level.
- [ ] Gerbang: visual jelas dan area interaksi yang membuka pilihan **Pulang ke Rumah / Berangkat ke Level / Batal**; kondisi pilihan dan pencegahan dialog terbuka ulang saat baru tiba dihubungkan oleh A/B.
- [ ] Area posko: meja Jonathan, kursi, papan jadwal/daftar penerima, area Ophelia, rak dan tumpukan bantuan. Papan tidak menjadi akses level terpisah.
- [ ] Titik interaksi Jonathan, Ophelia, dan gerbang jelas serta tidak saling menghalangi.
- [ ] Variasi keadaan posko awal/sudah tertata untuk prolog bila disetujui; boleh menggunakan props yang sama tanpa dua map terpisah.

### Rumah Vicky

- [ ] Satu map interior rumah sederhana: titik datang dari posko, meja, kursi, tempat tidur untuk save/istirahat, serta Ibu dan Ayah. Tidak perlu membuat tampilan luar rumah.
- [ ] Area tidur dan titik keluar menuju posko terbaca tanpa mengorbankan navigasi atau menutupi karakter dari kamera; tidak ada akses langsung rumah → level.

### Arena dan props misi

- [ ] Area battle umum dan penempatan karakter; suasana dapat memakai material/props yang sama dengan kampung.
- [ ] Penanda/variasi visual untuk boss Deddy dan boss terakhir; tidak wajib membangun dua arena baru jika cukup dengan dressing ulang.
- [ ] Paket bantuan yang mudah dikenali saat diam, dibawa, dan diserahkan; tentukan bersama A/B apakah visual paket melekat pada karakter atau hanya muncul pada interaksi.
- [ ] Dokumen, map, papan daftar, dan peta sederhana untuk Jonathan/prolog/adegan pertemuan bila dibutuhkan oleh storyboard.
- [ ] Visual item/weapon **setelah** jenis dan fungsinya diputuskan A/B; siapkan ikon dan model 3D hanya jika memang terlihat di game.

**Handoff:** modul beserta material/tekstur dan prefab yang bisa disusun ulang, scene uji terpisah atau daftar penempatan, dimensi/spawn/pivot, daftar collider, serta catatan variasi per level. A/B memeriksa integrasi scene dan rute; model visual tidak otomatis menggantikan collider gameplay.

## 6. C-D04 — Karakter cerita dan musuh

| Kebutuhan | Jumlah minimum usulan | Penggunaan dan batas |
| --- | --- | --- |
| Vicky | 1 | Sudah menjadi prioritas C-D02; model preview Meshy belum final. |
| Pak Jonathan | 1 | NPC mentor dan penghubung sponsor di posko. |
| Ophelia | 1 | NPC teman masa kecil; berdialog setiap level. |
| Ibu dan Ayah | 2 karakter sederhana | Boleh memakai basis model warga yang diubah pakaian/rambutnya. |
| Warga kampung | Basis laki-laki dan perempuan, dengan variasi secukupnya | Naskah memiliki banyak nama; bukan berarti harus membuat satu model unik untuk setiap nama. Variasikan usia, pakaian, warna, dan aksesori dengan hormat. |
| Preman/anak buah | 1 basis dengan variasi secukupnya | Dipakai di encounter dan cuplikan prolog. |
| Mr. Deddy | 1 | Identitas visual boss level 3. |
| Mr. Gas Oke | 1 | Identitas visual boss level 5; nama final masih terbuka. |
| Sponsor | 0 model wajib | Diwakili Jonathan; logo/dokumen visual hanya bila disetujui. |

- [ ] Jonathan: model, material, skala, rig/pose bicara sesuai kebutuhan dan prefab visual.
- [ ] Ophelia: model, material, skala, rig/pose bicara sesuai kebutuhan dan prefab visual.
- [ ] Ibu dan Ayah: dua variasi karakter sederhana untuk rumah, dengan material dan pose bicara.
- [ ] Warga: basis/variasi memadai untuk penerima dan warga yang membantu; bentuk/pakaian tidak boleh membuatnya tampak seperti satu karakter yang disalin tanpa perubahan.
- [ ] Preman dan anak buah: basis/variasi yang terbaca sebagai lawan, tanpa perlu model unik setiap pertemuan.
- [ ] Deddy dan boss terakhir: dua karakter visual berbeda, dikenali dalam prolog dan battle; animasi battle dapat berbagi rig jika kompatibel.
- [ ] Semua karakter yang dianimasikan telah diperiksa deformasi, aksesori, orientasi, material URP, dan skala. NPC yang hanya berbicara boleh menggunakan pose sederhana bila tidak memerlukan rig penuh.

**Handoff:** prefab visual + model/tekstur/material + daftar karakter/variasi dan sumber atau lisensinya. Rig/animasi disertakan untuk karakter yang benar-benar bergerak, bukan diasumsikan tersedia dari gambar.

## 7. C-D05 — UI/UX dan ikon

Semua layar harus memakai state/input game yang nyata dari A/B, bukan tombol visual tanpa fungsi. Uji kontras, ukuran huruf, focus keyboard, mouse, teks panjang, dan keterbacaan pada **1280×720 dan 1920×1080**.

- [ ] **Main menu:** Mulai, Lanjutkan, Load, Setting; state tidak tersedia ketika save belum ada dan konfirmasi overwrite bila diperlukan.
- [ ] **Load/save:** daftar slot sesuai keputusan desain, slot kosong/terisi, konfirmasi, sukses, dan gagal menyimpan/memuat.
- [ ] **Setting:** kontrol untuk pengaturan yang benar-benar disediakan A/B; volume musik/SFX dan resolusi masih rancangan, bukan asumsi implementasi.
- [ ] **Dialog/prolog:** panel nama dan dialog panjang, indikator lanjut, serta tata letak portrait bila portrait dipilih.
- [ ] **Posko dan progres:** briefing Jonathan, laporan, pilihan level terbuka/terkunci, hari, dan tampilan reward.
- [ ] **HUD eksplorasi:** paket, penerima, trust, waktu, tujuan aktif, dan prompt interaksi.
- [ ] **Encounter:** pilihan menghindar dengan peluang 50/50 yang terbaca, bertengkar, bantuan warga, alasan bantuan terkunci, serta feedback hasil.
- [ ] **Battle:** HP pemain/lawan, giliran, peringatan serangan kuat, Serang, Bertahan, Item, keadaan aksi terkunci, dan feedback menang/kalah.
- [ ] **Item/weapon:** daftar, jumlah, pilihan pakai/pasang, kosong/tidak tersedia, konfirmasi dan reward; jenisnya menunggu keputusan A/B.
- [ ] **Pause, hasil dan navigasi:** lanjut, kembali ke menu, konfirmasi kehilangan progres, menang/gagal/parsial, retry level, tidur/save, serta layar penutup.
- [ ] **Ikon minimum:** paket, timer, trust, interaksi, kesehatan, Serang, Bertahan, item, weapon dan status terkunci. Gunakan label teks untuk makna penting, tidak hanya warna/ikon.
- [ ] **Portrait karakter:** hanya bila arah dialog memilih portrait 2D; kebutuhan dan variasi ekspresi dipastikan terlebih dahulu, bukan wajib membuat satu untuk setiap warga.

**Handoff:** file sumber desain, PNG dengan transparansi untuk elemen yang memang dibutuhkan, aset font/ikon beserta izin, prefab atau layout Unity jika disepakati, contoh layar untuk seluruh state, serta referensi pengikatan komponen. A/B bertanggung jawab menyambungkan logika dan memverifikasi tombol.

## 8. C-D06 — Animasi dan adegan

Animasi dapat dibagi lintas karakter dengan rig kompatibel. Adegan cerita dapat berupa pose, kamera tetap, dan dialog; tidak wajib 15 cutscene sinematik berbeda.

- [ ] **Gerak dasar:** idle dan berjalan Vicky; idle/bicara NPC yang berinteraksi.
- [ ] **Pengantaran:** menerima/mengulurkan paket atau gestur yang jelas. Paket tidak menembus tangan/tubuh secara mencolok.
- [ ] **Battle pemain:** serang, bertahan, terkena serangan, menang, dan kalah; sinkronkan durasi/impact dengan A/B agar tidak terjadi damage ganda atau input terbuka terlalu cepat.
- [ ] **Battle lawan/boss:** idle, ancang-ancang/telegraph, serang biasa/kuat, terkena serangan dan kalah; animasi boleh dipakai bersama jika tetap terbaca.
- [ ] **Bantuan warga:** warga mendekat/menarik perhatian, preman merespons, penghalang terbuka, lalu kontrol kembali. Sepakati titik awal/akhir, durasi, dan apa yang terjadi jika game dijeda.
- [ ] **Prolog dan cuplikan boss:** Vicky pulang, warga berbicara, posko ditata, pertemuan Ophelia, serta Deddy/Gas Oke terlihat sekilas sesuai SB-01–SB-05.
- [ ] **Penghubung level dan ending:** laporan, percakapan Ophelia, istirahat, pertemuan terakhir, dan penutup; gunakan sistem pose/dialog yang dapat dipakai ulang.

**Handoff:** klip/animator atau daftar nama klip, rig yang cocok, titik asal/akhir, durasi dan waktu impact/trigger, serta video singkat di Unity. Tidak mencentang animasi bantuan hanya karena model warga tersedia.

## 9. C-D07 — Audio, pencahayaan, dan efek minimum

### Audio

- [ ] Musik eksplorasi/posko dan musik battle yang cocok; boleh memakai musik CC0 graybox jika disepakati tim dan hak pakainya dicatat.
- [ ] Musik atau variasi suasana untuk prolog/ending dan boss jika diperlukan setelah peninjauan; jangan membuat track baru jika track yang sama sudah cukup.
- [ ] Ambience kampung dan interior rumah/posko yang tidak menutupi dialog atau musik.
- [ ] SFX langkah, interaksi/serah paket, navigasi tombol, serang/kena/bertahan, bantuan warga, dan hasil misi.
- [ ] Level volume, loop, awal/akhir, serta pergantian saat pause, transisi battle, retry, dan kembali ke menu diperiksa bersama A/B.
- [ ] Daftar judul, pembuat, sumber, lisensi, izin penggunaan, dan perubahan untuk setiap aset suara yang dipakai.

### Pencahayaan dan efek

- [ ] Preset pencahayaan kampung, posko, rumah dan battle yang konsisten dengan gaya anime; karakter serta teks tetap terbaca.
- [ ] Efek ringan untuk interaksi/penyerahan, serangan, terkena, guard, dan peringatan boss; tidak mengaburkan informasi atau menyebabkan efek berkedip berlebihan.
- [ ] Material dan efek diuji pada URP serta perangkat target; tidak ada pink material, objek hilang, atau penurunan performa yang menghambat permainan.

**Handoff:** file audio/efek, preset atau prefab, nama penggunaan, sumber/lisensi, serta rekaman pemeriksaan di scene terintegrasi.

## 10. C-D08 — Integrasi dan penerimaan akhir

- [ ] Setiap paket yang wajib memiliki daftar aset, file sumber yang tersedia, versi ekspor, referensi, dan bukti uji; semua lisensi serta batas penggunaan terdokumentasi.
- [ ] Model/props dipasang bertahap oleh A/B/C sesuai kepemilikan file; controller, collider, trigger, data misi, dan scene rekan tidak tertimpa tanpa koordinasi.
- [ ] Uji ulang jalur wajib dan kamera setelah perubahan lingkungan; cek karakter dan paket tidak terjebak atau tertutup bangunan.
- [ ] Uji minimal satu pengantaran, tiap opsi encounter, bantuan warga, battle biasa dan kedua boss setelah visual/animasi dipasang.
- [ ] Uji transisi prolog → posko → level → laporan → Ophelia → rumah → istirahat, termasuk akhir level 5, di build terintegrasi.
- [ ] Uji seluruh state UI dan input pada 1280×720 dan 1920×1080; periksa clipping, fokus keyboard, kontras, dan teks panjang.
- [ ] Dengarkan audio pada transisi/ulang dan pastikan tidak ada musik bertumpuk; cek efek, performa dan lighting pada target Windows.
- [ ] Serahkan daftar masalah yang masih ada serta screenshot/rekaman dari build aktual; build final dicoba di komputer kedua sesuai U21/R15.
- [ ] Setelah diterima, centang item yang benar-benar lulus di dokumen ini dan perbarui status terkait di README; tidak mengubah checklist lama berdasarkan keberadaan file semata.

## 11. Format penyerahan satu paket

Untuk setiap paket C-D01–C-D08, catat di issue/PR atau pesan handoff:

1. **ID paket dan versi:** contoh `C-D02 / v1`, tanggal, dan status `draft`, `siap ditinjau`, atau `diterima`.
2. **Isi dan lokasi:** daftar file dan path dalam repository; bedakan sumber editable, ekspor, tekstur, klip, material, prefab, dan scene uji.
3. **Sumber dan hak pakai:** pembuat/generator, URL dan lisensi bila eksternal; untuk generator, catat ketentuan akun yang relevan tanpa menyimpan API key.
4. **Cara pakai:** skala, pivot/orientasi, rig/klip, ukuran tekstur, URP/material, collider bila ada, serta komponen yang boleh dan tidak boleh diubah.
5. **Bukti:** screenshot/video dari Unity, versi Editor, kondisi yang diuji, serta siapa yang memeriksa bersama A/B.
6. **Batas dan tindak lanjut:** masalah yang diketahui, asumsi yang belum disepakati, pengaruh pada performa, dan keputusan yang masih dibutuhkan.

Angka triangle, ukuran tekstur, dan target FPS sebaiknya disepakati berdasarkan perangkat target serta profil nyata; dokumen ini tidak mengklaim angka anggaran final. Jangan berikan material atau rig kosong untuk membuat suatu paket tampak selesai.
