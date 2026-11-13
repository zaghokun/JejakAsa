# Jejak Asa — Storyboard dan Naskah Dialog

**Status: draft naratif untuk ditinjau tim.** Dokumen ini adalah rancangan, bukan pernyataan bahwa cerita atau sistem di dalamnya sudah tersedia di Unity. Progres implementasi tetap dicatat di [README](../README.md).

## 1. Premis dan batas rancangan

Vicky baru lulus kuliah dan kembali ke kampung fiktif di Jakarta Utara. Ia melihat kebutuhan warga di sekitarnya dan ingin membantu penyaluran bantuan secara lebih tertib. Bersama warga, dukungan sponsor, dan bimbingan Pak Jonathan, Vicky membangun posko. Setiap hari ia menyalurkan bantuan, menghadapi intimidasi, melapor, berbincang dengan Ophelia, lalu pulang beristirahat sebelum misi berikutnya.

Cerita mendukung SDG 1, 11, dan 16. Bantuan tidak digambarkan sebagai penyelesaian kemiskinan secara instan: warga ikut menentukan kebutuhan, membantu tetangga, mencatat persoalan, dan menyusun langkah selanjutnya.

### Catatan rancangan untuk ditinjau

- **Struktur map:** posko adalah map tersendiri dengan lingkungan, rumah langsung berupa map interior, dan setiap level memiliki map sendiri. Saat gameplay, keluar rumah hanya menuju posko; dari posko pemain berangkat ke map level dan kembali ke posko setelah selesai.
- **Nama boss terakhir:** Mr. Gas Oke, dikonfirmasi pengguna.
- **Penerima:** naskah mengusulkan tiga penyerahan per level agar sejalan dengan graybox; jumlah dan susunan final belum disepakati. Nama warga adalah nama tokoh cerita, bukan kewajiban membuat 15 model unik.
- **Syarat level:** untuk draft ini, antar tiga paket dan, pada level 3/5, menangkan battle boss. Timer misi berhenti saat penyerahan selesai; level boss belum dianggap selesai sebelum boss menang. Perubahan ini belum ada di graybox.
- **Urutan setelah menang:** lapor kepada Jonathan → bicara dengan Ophelia → terima reward → pulang dan istirahat. Reward setelah kedua percakapan mengikuti ide awal; jenis, angka efek, dan apakah percakapan nanti wajib masih perlu disepakati.
- **Akhir level 5:** laporan dan percakapan Ophelia tetap terjadi; penutup berlangsung di rumah pada malam yang sama. Tidak ada level 6 atau hari berikutnya yang wajib dimainkan.
- **Save:** draft hanya memastikan ada pilihan simpan/istirahat di rumah. Jumlah slot dan apakah ada autosave di luar rumah belum diputuskan; teks di bawah tidak mengklaim autosave ada.
- **Menghindar:** 50% berhasil mundur untuk mencari jalur lain; 50% berujung battle. Draft mengusulkan timer berhenti selama panel pilihan dan battle, serta dilanjutkan saat eksplorasi. Angka, posisi aman, dan interaksi timer perlu dikonfirmasi tim.
- **Bantuan warga:** tersedia setelah trust memenuhi syarat level; draft memakai minimal satu penyerahan, mengikuti graybox. Warga melakukan animasi bantuan sebelum jalan dibuka. Warga yang membantu dapat memakai model warga yang sudah ada.
- **Pertarungan:** dramatis tetapi non-grafis. Senjata dan efeknya belum ditentukan, sehingga naskah tidak menjanjikan item tertentu atau kemampuan yang belum disepakati.
- Kalimat dalam **[kurung siku]** adalah arahan visual, kondisi, atau aksi sistem; bukan dialog yang dibacakan. Nama sebelum titik dua adalah pembicara. ID adegan dapat dipakai saat handoff ke tim implementasi.

## 2. Karakter dan arahan dialog

| Tokoh | Peran dan suara | Kebutuhan visual dasar |
| --- | --- | --- |
| Vicky | Lulusan baru, peduli, terkadang terburu-buru, belajar mendengarkan | MC; idle, jalan, interaksi, respons battle |
| Pak Jonathan | Penghubung sponsor dan mentor; tenang, teliti, tidak mengambil keputusan sendirian | NPC posko; idle dan bicara |
| Ophelia | Teman masa kecil dan crush Vicky; hangat, usil seperlunya, punya pendapat sendiri | NPC posko; idle dan bicara |
| Ibu dan Ayah | Orang tua Vicky; mendukung sekaligus mengingatkan batas kemampuan | Model sederhana boleh digunakan kembali |
| Warga | Individu dengan kebutuhan dan inisiatif berbeda; bukan sekadar penerima pasif | Model modular dapat digunakan ulang |
| Preman | Mengintimidasi dan menghalangi jalur; dapat dihindari, dialihkan, atau dilawan | Satu model dasar dengan variasi seperlunya |
| Mr. Deddy | Mengendalikan akses gang dan menuntut imbalan; boss level 3 | Model boss pertama; pose bicara dan respons battle |
| Mr. Gas Oke | Ingin memonopoli distribusi bantuan lewat tekanan dan perjanjian sepihak; boss level 5 | Model boss terakhir; pose bicara dan respons battle |
| Sponsor | Organisasi pendukung yang diwakili Jonathan; membantu sumber daya, tidak muncul sebagai tokoh baru | Logo/nama pada dokumen posko opsional |

## 3. Storyboard ringkas

| ID | Tempat / waktu | Gambar dan aksi utama | Dialog | Hasil / handoff |
| --- | --- | --- | --- | --- |
| SB-01 | Interior rumah, pagi | Vicky tiba dengan tas dan berbicara dengan Ibu | P01 | Prolog dimulai tanpa map luar rumah |
| SB-02 | Lingkungan posko | Vicky berbicara dengan warga, menyadari mereka sudah saling membantu | P02 | Tujuan program jelas |
| SB-03 | Posko kosong | Jonathan menyampaikan dukungan sponsor; Vicky dan warga menyiapkan posko | P03 | Posko dan mentor diperkenalkan |
| SB-04 | Posko | Ophelia bertemu lagi dengan Vicky | P04 | Hubungan diperkenalkan |
| SB-05 | Gang / ruang pertemuan | Sekilas Deddy dan Gas Oke memantau rencana posko | P05 | Bayangan dua boss, tanpa pertarungan |
| SB-06 | Posko → gang | Briefing dan pengantaran pertama | L1-01–L1-06 | Level 1; belajar distribusi |
| SB-07 | Posko → rumah | Laporan, Ophelia, reward, istirahat | L1-07–L1-10 | Level 2 terbuka setelah istirahat |
| SB-08 | Posko → gang | Wilayah lebih jauh; rintangan preman dan bantuan warga diperkenalkan | L2-01–L2-08 | Level 2 selesai; Deddy mulai terasa |
| SB-09 | Posko → rumah | Laporan, percakapan, reward, istirahat | L2-09–L2-12 | Level 3 terbuka |
| SB-10 | Posko → gang utama | Deddy menutup jalur; warga membantu mengantar dan memberi kesaksian | L3-01–L3-07 | Konfrontasi boss pertama |
| SB-11 | Arena → posko → rumah | Battle Deddy; laporan dan respons Ophelia | L3-08–L3-13 | Level 4 terbuka |
| SB-12 | Posko → gang | Warga memeriksa data distribusi; ancaman yang lebih besar muncul | L4-01–L4-09 | Level 4 selesai |
| SB-13 | Posko → rumah | Rencana menghadapi tekanan pada program | L4-10–L4-13 | Level 5 terbuka |
| SB-14 | Posko → gang / arena | Pengantaran terakhir; Gas Oke mencoba mengambil alih program | L5-01–L5-09 | Konfrontasi boss terakhir |
| SB-15 | Posko → rumah | Warga dan sponsor menindaklanjuti, Vicky dan Ophelia berbicara, keluarga menutup cerita | L5-10–L5-14 | Ending; tidak mengklaim kemiskinan selesai |

**Produksi hemat:** adegan dapat menggunakan kamera tetap, ekspresi/pose sederhana, panel dialog, dan perpindahan scene. Storyboard ini tidak mengharuskan semua cutscene menggunakan animasi sinematik penuh.

## 4. Prolog — Pulang dan membangun posko

### P01 / SB-01 — Vicky pulang

[Interior rumah. Pagi. Vicky baru tiba membawa tas dan bertemu Ibu. Tidak ada map luar rumah.]

**Vicky:** Akhirnya pulang juga. Rasanya gang ini lebih sempit daripada yang kuingat.

**Ibu:** Gang-nya tidak mengecil. Kamu saja yang sudah lama tidak pulang.

**Vicky:** Bisa jadi, Bu. Bagaimana keadaan warga sekarang?

**Ibu:** Banyak yang sedang berusaha mencukupi kebutuhan sehari-hari. Tetangga juga saling membantu, tapi kadang bantuan yang datang belum sampai ke semua orang yang membutuhkan.

**Vicky:** Aku ingin cari tahu apa yang benar-benar mereka perlukan. Aku tidak mau datang lalu asal membagikan sesuatu.

**Ibu:** Bagus. Mulai dengan mendengarkan mereka, ya.

### P02 / SB-02 — Mendengarkan warga

[Setelah berpindah dari rumah ke lingkungan posko yang masih kosong, Vicky menemui seorang warga yang membawa daftar kebutuhan.]

**Vicky:** Bu, saya Vicky. Saya baru pulang ke sini. Apa ada yang bisa saya bantu?

**Bu Rina:** Kami sedang mendata kebutuhan beberapa rumah. Ada yang sulit keluar, ada juga yang lebih butuh bahan pokok daripada barang lain.

**Vicky:** Boleh saya ikut mendata dan mengantar?

**Bu Rina:** Boleh. Tapi mari kita cocokkan dulu dengan warga lain, supaya pembagiannya jelas dan tidak ada rumah yang terlewat.

**Vicky:** Setuju, Bu. Kita buat catatan yang bisa diperiksa bersama.

### P03 / SB-03 — Sponsor dan posko

[Posko kosong. Jonathan datang membawa map; beberapa warga membantu merapikan meja.]

**Pak Jonathan:** Vicky? Saya Jonathan. Saya mendapat ceritamu dari warga. Saya menjadi penghubung dengan sponsor yang bersedia menyediakan bantuan untuk program ini.

**Vicky:** Terima kasih, Pak. Saya ingin bantuan sampai ke rumah yang sudah didata warga. Tapi saya belum pernah mengelola program seperti ini.

**Pak Jonathan:** Kita kerjakan bersama. Warga membantu menyusun kebutuhan, sponsor membantu persediaan, dan kita mencatat apa yang masuk serta keluar. Jangan mengambil keputusan untuk warga tanpa berbicara dengan mereka.

**Vicky:** Baik, Pak. Tempat ini bisa kita jadikan posko. Saya akan bertanggung jawab atas pengantarannya.

**Bu Rina:** Kami bisa membantu memastikan alamat dan menyampaikan kalau ada kebutuhan yang berubah.

**Pak Jonathan:** Saya juga akan mendampingimu. Kalau ada masalah di lapangan, laporkan. Kita cari jalan keluar bersama.

[Meja, papan daftar, dan beberapa paket ditata. Posko siap digunakan.]

### P04 / SB-04 — Pertemuan kembali

**Ophelia:** Jadi benar kamu pulang. Aku kira setelah lulus kamu bakal lupa jalan ke kampung.

**Vicky:** Jalan ke kampung masih ingat. Jalan ke rumahmu saja yang hampir salah belok.

**Ophelia:** Padahal dulu kamu yang paling sering mengajak main di gang ini.

**Vicky:** Hai, Ophelia. Senang bertemu lagi.

**Ophelia:** Aku juga senang. Aku sering membantu warga menyampaikan kabar ke posko. Kalau kamu mau bekerja di sini, bersiaplah mendengar banyak cerita.

**Vicky:** Justru itu yang kubutuhkan. Semoga aku tidak merepotkanmu.

**Ophelia:** Belum tentu. Kita lihat setelah hari pertamamu.

### P05 / SB-05 — Bayangan ancaman

[Di ujung gang, Deddy melihat orang-orang menata paket. Transisi ke ruang sederhana; Gas Oke memeriksa peta wilayah. Mereka tidak bertemu Vicky dalam adegan ini.]

**Mr. Deddy:** Posko baru? Kalau mereka mulai melewati gang itu, orang-orang akan lupa siapa yang selama ini mengatur jalan di sini.

**Anak buah:** Mau kami hentikan sekarang, Pak?

**Mr. Deddy:** Awasi dulu. Aku ingin tahu seberapa jauh mereka berani melangkah.

[Potong ke Gas Oke.]

**Mr. Gas Oke:** Program bantuan tanpa kendaliku? Menarik. Biarkan mereka mulai. Nanti kita bicarakan siapa yang berhak menentukan jalurnya.

[Fade ke posko. Kendali diberikan kepada pemain.]

## 5. Level 1 — Langkah pertama

**Tujuan cerita:** memperkenalkan pengantaran dan menunjukkan bahwa warga ikut mengatur bantuan. Tidak ada kewajiban encounter preman pada level ini.

### L1-01 — Briefing Jonathan

**Pak Jonathan:** Hari pertama, kita mulai dari tiga rumah yang sudah didata warga. Periksa alamat sebelum menyerahkan paket.

**Vicky:** Jadi saya mengambil paket dari posko, lalu mengantar sesuai daftar?

**Pak Jonathan:** Betul. Waktu pengantaran terbatas. Jika selesai, kembali dan laporkan hasilnya kepadaku.

**Vicky:** Siap, Pak. Saya akan mendengarkan kalau ada kebutuhan yang berubah.

[Tujuan: antar tiga paket.]

### L1-02 — Bu Rina

[Sebelum penyerahan.]

**Bu Rina:** Daftarnya sudah kami cocokkan. Rumah Pak Rahmat ada di ujung gang, dan Bu Sari di dekat tikungan.

**Vicky:** Terima kasih, Bu. Ini paket untuk rumah Ibu. Tolong beri tahu posko kalau ada yang perlu diperbarui.

[Setelah penyerahan.]

**Bu Rina:** Akan saya sampaikan. Tetangga-tetangga juga siap membantu mengecek alamat.

### L1-03 — Pak Rahmat

**Pak Rahmat:** Nak, kamu dari posko baru itu?

**Vicky:** Ya, Pak. Nama Bapak ada di daftar yang disusun warga. Ini paket untuk keluarga Bapak.

**Pak Rahmat:** Terima kasih. Saya biasanya membantu memperbaiki pagar tetangga. Kalau posko butuh meja diperbaiki, kabari saya.

[Setelah penyerahan.]

**Vicky:** Baik, Pak. Saya sampaikan kepada Pak Jonathan.

### L1-04 — Bu Sari

**Bu Sari:** Jalanmu tidak tersesat? Gang ini mirip semua kalau baru pulang.

**Vicky:** Hampir, Bu. Untung Bu Rina memberi petunjuk. Ini bantuan untuk Ibu.

**Bu Sari:** Terima kasih. Saya akan memberi tahu tetangga kalau jadwal posko berikutnya sudah ada.

### L1-05 — Misi selesai

**Vicky:** Tiga paket sudah diterima. Aku kembali ke posko untuk melapor.

### L1-06 — Ulang interaksi warga

[Dipakai untuk penerima level 1 yang sudah menerima paket.]

**Warga:** Paketnya sudah kami terima. Terima kasih, Vicky. Hati-hati kembali ke posko.

### L1-07 — Laporan

**Vicky:** Pak, ketiga paket sudah saya serahkan. Warga juga membantu saya menemukan alamat yang benar.

**Pak Jonathan:** Bagus. Saya cocokkan dengan daftar. Catatan seperti ini penting supaya warga dan sponsor tahu bantuan disalurkan ke mana.

**Vicky:** Pak Rahmat menawarkan bantuan memperbaiki meja posko.

**Pak Jonathan:** Sampaikan terima kasih. Program ini memang kerja bersama, bukan pekerjaan satu orang.

### L1-08 — Ophelia dan reward

**Ophelia:** Bagaimana hari pertamamu? Masih hafal semua tikungan?

**Vicky:** Hampir. Aku lebih banyak dibantu warga daripada yang kuduga.

**Ophelia:** Itu hal baik. Kamu tidak perlu menjadi pahlawan sendirian.

**Vicky:** Baik, aku catat. Besok aku akan lebih siap.

**Ophelia:** Setelah bertemu Jonathan dan bercerita denganku, ambil perlengkapan yang sudah disiapkan di posko. Lalu pulang dan istirahat.

[Sistem: berikan reward level 1 satu kali. Jenis dan efek reward belum ditetapkan.]

### L1-09 — Di rumah

**Ayah:** Hari pertama selesai?

**Vicky:** Selesai, Yah. Aku baru sadar, membantu itu juga berarti mau menerima bantuan orang lain.

**Ayah:** Pelajaran yang bagus. Makan dulu, lalu istirahat.

### L1-10 — Istirahat

[Interaksi tempat tidur.]

**Vicky:** Aku perlu istirahat sebelum kembali ke posko besok.

**Pilihan:** `Simpan dan istirahat` / `Belum`.

[Jika simpan berhasil: hari berikutnya dimulai dan level 2 terbuka. Teks sistem lengkap ada pada bagian 11.]

## 6. Level 2 — Jalan memutar

### L2-01 — Briefing

**Pak Jonathan:** Hari ini ada tiga rumah di sisi lain kampung. Jalur utama lebih cepat, tetapi warga melaporkan ada orang yang mengintimidasi pengantar bantuan.

**Vicky:** Kalau bertemu mereka, saya harus bagaimana?

**Pak Jonathan:** Kamu bisa mencoba menghindar, tetapi ada kemungkinan mereka memaksamu bertarung. Kalau warga sudah percaya dan bersedia membantu, dengarkan mereka. Jangan menganggap perkelahian satu-satunya jalan.

**Vicky:** Saya mengerti. Saya akan mulai dari rumah terdekat.

### L2-02 — Pak Bima

**Pak Bima:** Ada jalan memutar di sisi kiri. Lebih jauh, tapi warga biasanya lewat sana kalau gang utama dijaga.

**Vicky:** Terima kasih, Pak. Ini paket untuk rumah Bapak.

**Pak Bima:** Saya bisa memberitahu tetangga tentang jalur itu juga.

### L2-03 — Bu Lina

**Bu Lina:** Terima kasih sudah mengantar. Saya sedang menjaga anak, jadi sulit meninggalkan rumah lama-lama.

**Vicky:** Ini paket untuk Ibu. Kalau ada perubahan kebutuhan, Ibu bisa menyampaikannya ke posko lewat warga pendata.

**Bu Lina:** Baik. Saya juga bisa membantu menyampaikan kabar kepada keluarga di sebelah.

### L2-04 — Mas Damar

**Mas Damar:** Saya mendengar posko mulai menerima laporan pengantaran. Bagus kalau catatannya bisa diperiksa bersama.

**Vicky:** Kami masih belajar, Mas. Ini paket untuk rumah Mas Damar.

**Mas Damar:** Terima kasih. Nanti saya bantu memberi tahu warga kapan posko buka.

### L2-05 — Encounter pertama

[Preman menghalangi gang.]

**Preman:** Mau lewat sini? Banyak sekali paket yang kamu bawa.

**Vicky:** Paket ini untuk warga yang sudah didata. Saya hanya ingin mengantar.

**Preman:** Jalan ini kami jaga. Kalau mau lewat, urus dulu dengan kami.

[Gunakan pilihan dan cabang bersama di bagian 10.]

### L2-06 — Setelah jalan dibuka dengan bantuan warga

**Pak Bima:** Kami kenal jalan ini dan akan bicara dengan mereka. Kamu lanjutkan pengantaran.

**Vicky:** Terima kasih, Pak. Saya akan mengabari posko setelah selesai.

### L2-07 — Misi selesai

**Vicky:** Semua paket sudah sampai. Gang ini perlu kita bicarakan bersama di posko.

### L2-08 — Ulang interaksi warga

**Warga:** Paketnya sudah diterima. Kalau melihat pengantar lain, beri tahu mereka ada jalan memutar.

### L2-09 — Laporan

**Vicky:** Semua sampai, Pak. Tapi ada orang yang menghalangi gang. Warga membantu menunjukkan pilihan jalan.

**Pak Jonathan:** Terima kasih sudah melaporkan. Saya akan mencatat lokasinya dan berbicara dengan warga setempat.

**Vicky:** Mereka menyebut orang bernama Deddy. Saya belum tahu apa perannya.

**Pak Jonathan:** Jangan membuat kesimpulan sebelum kita punya informasi. Yang penting, warga bisa menyampaikan kejadian dengan aman.

### L2-10 — Ophelia dan reward

**Ophelia:** Aku dengar perjalananmu lebih ramai hari ini.

**Vicky:** Ya. Aku sempat berpikir harus mengurus semuanya sendiri. Untung warga tahu jalan lain.

**Ophelia:** Dulu kamu juga begitu. Kalau layang-layang tersangkut, kamu memanjat dulu baru bertanya.

**Vicky:** Dan kamu yang mencari tangga.

**Ophelia:** Setidaknya sekarang kamu bertanya lebih cepat.

[Sistem: setelah laporan dan percakapan, berikan reward level 2 satu kali.]

### L2-11 — Di rumah

**Ibu:** Kamu kelihatan lelah. Misi hari ini sulit?

**Vicky:** Ada gang yang dijaga orang-orang. Warga membantu kami mencari jalan lain.

**Ibu:** Dengarkan mereka dan jaga dirimu. Besok tetap ada waktu untuk melanjutkan.

### L2-12 — Istirahat

**Vicky:** Besok aku harus kembali ke posko. Aku istirahat dulu.

**Pilihan:** `Simpan dan istirahat` / `Belum`.

[Jika berhasil: level 3 terbuka.]

## 7. Level 3 — Penjaga gang

### L3-01 — Briefing

**Pak Jonathan:** Tiga rumah di gang utama sudah masuk daftar. Warga juga melaporkan bahwa Deddy meminta imbalan agar pengantar bisa lewat.

**Vicky:** Apakah kita punya jalan alternatif?

**Pak Jonathan:** Ada, tapi tidak semua rumah bisa dijangkau dengan mudah. Antar paket seperti biasa. Kalau bertemu Deddy, utamakan keselamatan dan jangan membuat janji atas nama warga.

**Vicky:** Saya akan memastikan paket sampai dan kembali dengan laporan yang jelas.

### L3-02 — Bu Wati

**Bu Wati:** Terima kasih, Nak. Belakangan kami khawatir pengantaran berhenti di ujung gang.

**Vicky:** Paket untuk Ibu sudah saya bawa. Kami akan mencatat setiap rumah yang sudah menerima.

**Bu Wati:** Saya dan tetangga bisa membantu memastikan tidak ada yang terlewat.

### L3-03 — Pak Jaya

**Pak Jaya:** Saya melihat orang-orang Deddy meminta pengantar membayar untuk lewat.

**Vicky:** Kami tidak meminta warga membayar untuk bantuan ini. Apakah Bapak bersedia menyampaikan yang Bapak lihat kepada posko?

**Pak Jaya:** Bersedia. Saya juga akan mengajak tetangga yang menyaksikan, supaya ceritanya tidak hanya dari saya.

### L3-04 — Nisa

**Nisa:** Saya menyimpan catatan jadwal kedatangan bantuan. Bisa saya tunjukkan kepada Pak Jonathan?

**Vicky:** Tentu. Ini paket untuk rumahmu. Catatan itu bisa membantu warga dan posko mencocokkan laporan.

**Nisa:** Baik, saya akan datang ke posko bersama tetangga.

### L3-05 — Pertemuan Deddy

[Setelah tiga paket diterima, Deddy menunggu di ujung jalur. Timer pengantaran dihentikan; level belum selesai.]

**Mr. Deddy:** Kamu Vicky, ya? Banyak yang membicarakan poskomu.

**Vicky:** Saya baru selesai mengantar bantuan untuk warga.

**Mr. Deddy:** Pengantaran di gang ini harus seizin saya. Beri saya bagian dari setiap kiriman, dan jalanmu akan mudah.

**Vicky:** Paket itu dicatat untuk keluarga yang membutuhkan. Saya tidak berhak memberikannya sebagai imbalan.

**Mr. Deddy:** Kalau begitu, jangan harap kamu bisa keluar membawa poskomu tetap berdiri.

**Vicky:** Saya tidak ingin bertengkar, tapi saya juga tidak akan menyerahkan hak warga.

[Mulai battle boss Deddy.]

### L3-06 — Ulang interaksi warga

**Warga:** Paketnya sudah sampai. Kami akan membantu mencatat apa yang terjadi di gang.

### L3-07 — Peringatan sebelum boss

**Sistem:** `Mr. Deddy menghadang jalan. Menangkan battle untuk menyelesaikan level.`

### L3-08 — Battle Deddy

[Awal battle.]

**Mr. Deddy:** Kita lihat apakah keberanianmu cukup untuk menjaga posko itu.

**Vicky:** Aku akan bertahan. Warga tidak harus menyerahkan bagian mereka.

[Menjelang serangan kuat, jika pola battle menampilkan peringatan.]

**Mr. Deddy:** Jangan kira aku akan menahan diri!

**Sistem:** `Deddy bersiap menyerang kuat. Pertimbangkan Bertahan.`

[Jika Vicky menang.]

**Mr. Deddy:** Cukup! Aku mundur hari ini. Tapi bukan aku satu-satunya yang memperhatikan bantuan itu.

**Vicky:** Gang ini milik semua warga. Aku akan melaporkan kejadian ini kepada posko.

[Jika Vicky kalah.]

**Mr. Deddy:** Pulanglah. Gang ini belum bisa kamu lewati.

**Vicky:** Aku harus kembali dan mencoba lagi dengan lebih siap.

[Gunakan layar gagal dan retry pada bagian 10. Kekalahan tidak membuka level 4.]

### L3-09 — Setelah menang

[Warga mendekat setelah battle.]

**Pak Jaya:** Kami melihat apa yang terjadi. Kami akan menyampaikan laporan bersama ke posko.

**Vicky:** Terima kasih, Pak. Saya tidak ingin siapa pun mengambil risiko sendirian.

### L3-10 — Laporan

**Vicky:** Semua paket sampai. Deddy meminta bagian dari kiriman dan menghadang saya. Pak Jaya serta warga lain bersedia memberi keterangan.

**Pak Jonathan:** Kita catat keterangan mereka tanpa memaksa siapa pun. Saya akan menyampaikan laporan ke sponsor dan mendampingi warga menentukan langkah berikutnya.

**Vicky:** Deddy bilang ada orang lain yang memperhatikan program ini.

**Pak Jonathan:** Kita perlu tetap teliti. Dugaan bukan bukti, tetapi laporan ini tidak akan kita abaikan.

### L3-11 — Ophelia dan reward

**Ophelia:** Aku mendengar kamu bertemu Deddy. Kamu baik-baik saja?

**Vicky:** Masih gemetar sedikit. Aku takut kalau posko harus berhenti.

**Ophelia:** Wajar takut. Lain kali, beri tahu kami lebih cepat. Kami ingin membantu, bukan hanya menunggu kabar.

**Vicky:** Maaf. Aku akan berusaha tidak memikul semuanya sendiri.

**Ophelia:** Bagus. Dan Vicky... aku lega kamu kembali dengan selamat.

**Vicky:** Aku juga lega ada kamu di sini.

[Sistem: setelah laporan dan percakapan, berikan reward level 3 satu kali.]

### L3-12 — Di rumah

**Ayah:** Jonathan memberi kabar kamu sudah sampai posko. Ibu dan Ayah khawatir.

**Vicky:** Maaf, Yah. Aku akan lebih berhati-hati. Warga juga membantu mencatat kejadiannya.

**Ayah:** Membela yang benar tidak berarti harus menghadapi semuanya sendirian.

### L3-13 — Istirahat

**Vicky:** Besok kami harus membahas laporan warga. Malam ini aku perlu beristirahat.

**Pilihan:** `Simpan dan istirahat` / `Belum`.

[Jika berhasil: level 4 terbuka.]

## 8. Level 4 — Catatan yang tertahan

### L4-01 — Briefing

**Pak Jonathan:** Warga sudah menyerahkan keterangan tentang gang. Sponsor juga menerima permintaan agar seluruh bantuan di wilayah ini disalurkan melalui pihak lain.

**Vicky:** Apakah itu orang yang dimaksud Deddy?

**Pak Jonathan:** Permintaannya atas nama Mr. Gas Oke. Kita belum tahu hubungan mereka. Hari ini antar tiga paket dan cocokkan daftar penerima dengan warga seperti biasa.

**Vicky:** Jadi kita tetap bekerja, sambil memastikan semua catatan jelas.

**Pak Jonathan:** Tepat. Kita tidak akan menuduh siapa pun tanpa dasar.

### L4-02 — Bu Dewi

**Bu Dewi:** Kami sempat diberi tahu bahwa posko akan tutup. Apakah benar?

**Vicky:** Tidak, Bu. Posko masih berjalan. Ini paket untuk keluarga Ibu, dan jadwal resmi bisa diperiksa langsung di posko.

**Bu Dewi:** Baik. Saya akan menyampaikan informasi itu kepada tetangga agar mereka tidak bingung.

### L4-03 — Pak Arif

**Pak Arif:** Saya mencatat tanggal bantuan yang diterima rumah-rumah di sekitar sini. Ada beberapa kabar yang tidak cocok dengan catatan kami.

**Vicky:** Terima kasih, Pak. Ini paket Bapak. Boleh catatannya dibawa ke pertemuan warga dan Jonathan?

**Pak Arif:** Boleh. Kami bisa mencocokkannya bersama, bukan hanya percaya kabar yang beredar.

### L4-04 — Tia

**Tia:** Ada orang yang meminta kami menandatangani daftar tanpa menjelaskan isinya. Kami belum menandatanganinya.

**Vicky:** Keputusan yang baik untuk memeriksa dulu. Ini bantuan untuk rumahmu; tidak ada kewajiban menandatangani dokumen yang tidak kamu pahami untuk menerimanya.

**Tia:** Saya akan membawa salinannya ke pertemuan warga.

### L4-05 — Encounter

**Preman:** Posko kalian sebentar lagi tidak dibutuhkan. Serahkan saja jalur ini ke orang yang lebih berpengalaman.

**Vicky:** Warga berhak mengetahui siapa yang menyalurkan bantuan dan bagaimana catatannya dibuat.

**Preman:** Kalau begitu, coba lewat dulu.

[Gunakan pilihan dan cabang bersama di bagian 10.]

### L4-06 — Bantuan warga

**Bu Dewi:** Kami ingin berbicara denganmu. Jangan halangi pengantaran ke rumah-rumah kami.

**Preman:** Kalian ikut campur juga?

**Bu Dewi:** Ini kampung kami. Kami berhak menyampaikan kebutuhan kami.

[Warga mengalihkan perhatian; jalan dibuka.]

### L4-07 — Misi selesai

**Vicky:** Pengantaran selesai. Aku harus menyampaikan kabar dan mengundang warga mencocokkan catatan di posko.

### L4-08 — Ulang interaksi warga

**Warga:** Paketnya sudah kami terima. Kami akan membawa catatan kami ke pertemuan di posko.

### L4-09 — Pesan untuk posko

[Di akhir pengantaran, Vicky menerima pesan; tidak ada mekanik mengumpulkan bukti sebagai objektif pemain.]

**Pak Jonathan:** Vicky, warga sudah sampai di posko untuk mencocokkan catatan. Kembali setelah kamu selesai.

**Vicky:** Baik, Pak. Semua paket sudah sampai. Saya segera kembali.

### L4-10 — Laporan dan pertemuan

**Vicky:** Tiga paket sudah diterima. Warga juga ingin memeriksa kabar tentang posko dan dokumen yang diminta pihak lain.

**Pak Jonathan:** Mari kita cocokkan catatan warga dengan daftar posko. Terima kasih sudah membawa persoalan ini ke pertemuan, bukan mengambil keputusan sendiri.

**Pak Arif:** Catatan kami menunjukkan ada pengantar yang diminta berhenti di gang. Kami siap menyampaikan keterangan.

**Pak Jonathan:** Saya akan meneruskan catatan dan keterangan yang warga setujui kepada sponsor. Mr. Gas Oke meminta pertemuan besok; warga perlu ikut menentukan tanggapan kita.

### L4-11 — Ophelia dan reward

**Ophelia:** Tadi banyak warga datang. Posko ini semakin ramai.

**Vicky:** Awalnya aku membayangkan tempat ini hanya untuk menyimpan paket. Sekarang rasanya seperti tempat semua orang bisa didengar.

**Ophelia:** Karena mereka ikut membangunnya. Kamu juga akhirnya belajar menyiapkan lebih dari satu kursi.

**Vicky:** Aku sengaja menyiapkan satu di dekatku.

**Ophelia:** Untuk siapa?

**Vicky:** Untuk teman yang selalu mengingatkanku mendengar orang lain.

**Ophelia:** Kalau begitu, aku akan duduk di sana setelah pekerjaanku selesai.

[Sistem: setelah laporan dan percakapan, berikan reward level 4 satu kali.]

### L4-12 — Di rumah

**Ibu:** Besok akan ada pertemuan penting?

**Vicky:** Ya. Kami ingin memastikan keputusan soal bantuan dibuat bersama warga, bukan hanya oleh orang yang paling berkuasa.

**Ibu:** Kalau begitu, dengarkan mereka seperti yang kamu janjikan saat pulang.

### L4-13 — Istirahat

**Vicky:** Aku siap untuk besok. Sekarang waktunya istirahat.

**Pilihan:** `Simpan dan istirahat` / `Belum`.

[Jika berhasil: level 5 terbuka.]

## 9. Level 5 — Harapan yang dijaga bersama

### L5-01 — Briefing terakhir

**Pak Jonathan:** Sponsor sudah menerima laporan dan bersedia mendengar keterangan warga. Tugas kita pagi ini tetap sama: tiga paket harus sampai.

**Vicky:** Lalu pertemuan dengan Mr. Gas Oke?

**Pak Jonathan:** Setelah pengantaran. Ia meminta kendali tunggal atas distribusi dan ingin mencatat semua keputusan tanpa keterlibatan warga. Kita akan menolak usulan itu bersama.

**Vicky:** Saya antar paketnya dulu. Setelah itu saya menemui kalian.

### L5-02 — Bu Ratna

**Bu Ratna:** Saya mendengar ada pertemuan tentang masa depan posko.

**Vicky:** Benar, Bu. Jonathan mengundang warga untuk ikut menyampaikan pendapat. Ini paket untuk keluarga Ibu.

**Bu Ratna:** Terima kasih. Saya akan datang dan mengajak tetangga yang ingin berbicara.

### L5-03 — Pak Hendra

**Pak Hendra:** Daftar kebutuhan warga sudah kami perbarui, termasuk rumah yang sebelumnya belum tercatat.

**Vicky:** Terima kasih, Pak. Paket ini untuk Bapak. Daftar itu akan membantu posko merencanakan pengantaran selanjutnya.

**Pak Hendra:** Kami akan terus memperbaruinya bersama, supaya keputusan tidak hanya dari satu pihak.

### L5-04 — Maya

**Maya:** Waktu posko pertama dibuka, saya kira hanya akan ramai beberapa hari.

**Vicky:** Saya juga tidak tahu akan sejauh ini. Banyak warga yang ikut membuatnya berjalan. Ini paket untuk rumahmu.

**Maya:** Terima kasih. Setelah ini kami akan ikut pertemuan di posko.

### L5-05 — Pertemuan Gas Oke

[Setelah tiga paket diterima, Vicky kembali menuju titik pertemuan. Timer berhenti; level belum selesai. Gas Oke menghadang bersama anak buah.]

**Mr. Gas Oke:** Jadi kamu Vicky. Poskomu cukup terkenal sekarang.

**Vicky:** Warga yang membuatnya berjalan. Kalau ada urusan, kita bisa bicarakan bersama mereka dan Pak Jonathan.

**Mr. Gas Oke:** Tidak perlu. Serahkan distribusi kepadaku. Warga tinggal menerima apa yang kuberikan, dan poskomu tidak perlu ikut campur.

**Vicky:** Warga berhak mengetahui dan menentukan apa yang mereka butuhkan. Catatan bantuan juga harus bisa diperiksa.

**Mr. Gas Oke:** Kamu pikir catatan dan beberapa orang di posko bisa menghentikanku?

**Vicky:** Aku tidak berdiri sendiri. Aku tidak akan menyerahkan keputusan warga karena intimidasi.

[Mulai battle boss terakhir.]

### L5-06 — Ulang interaksi warga

**Warga:** Paketnya sudah diterima. Kami akan ikut pertemuan dan menyampaikan pendapat kami.

### L5-07 — Peringatan sebelum boss

**Sistem:** `Mr. Gas Oke menghadang jalan. Menangkan battle untuk menyelesaikan level.`

### L5-08 — Battle terakhir

[Awal battle.]

**Mr. Gas Oke:** Kalau kamu kalah, berhenti menghalangi rencanaku.

**Vicky:** Bantuan untuk warga bukan alat untuk menguasai mereka.

[Menjelang serangan kuat, jika pola battle menampilkan peringatan.]

**Mr. Gas Oke:** Aku akan menyelesaikan ini sekarang!

**Sistem:** `Gas Oke bersiap menyerang kuat. Pertimbangkan Bertahan.`

[Jika Vicky menang.]

**Mr. Gas Oke:** Ini belum membuktikan apa pun!

**Vicky:** Yang akan menjawab bukan hanya aku. Warga dan sponsor punya hak memeriksa laporan dan menentukan langkah selanjutnya.

[Jika Vicky kalah.]

**Mr. Gas Oke:** Sepertinya kamu belum siap mempertahankan poskomu.

**Vicky:** Aku akan kembali. Aku tidak harus menghadapi ini sendirian.

[Gunakan layar gagal dan retry pada bagian 10. Kekalahan tidak membuka ending.]

### L5-09 — Setelah battle

[Jonathan, Ophelia, dan perwakilan warga tiba. Tidak ada penangkapan atau penyelesaian hukum instan.]

**Pak Jonathan:** Warga sudah membawa catatan mereka. Sponsor bersedia meninjau laporan bersama perwakilan kampung.

**Bu Ratna:** Kami ingin keputusan tentang kampung kami dibicarakan bersama, dengan catatan yang terbuka.

**Mr. Gas Oke:** Kalian boleh mengadakan pertemuan itu. Tapi urusan ini belum selesai.

**Pak Jonathan:** Justru karena itu prosesnya harus berjalan dengan benar. Mari kita mulai dengan mendengarkan semua pihak.

### L5-10 — Laporan terakhir

[Di posko.]

**Vicky:** Tiga paket terakhir hari ini sudah diterima. Gas Oke mencoba memaksa kita menyerahkan kendali distribusi.

**Pak Jonathan:** Saya mencatat laporanmu. Sponsor akan meninjau keterangan warga dan proses distribusi bersama perwakilan kampung.

**Vicky:** Apakah itu berarti semua masalah selesai?

**Pak Jonathan:** Belum. Bantuan ini membantu kebutuhan sekarang, tetapi pekerjaan jangka panjang tetap diperlukan. Yang penting, warga punya suara dalam menentukan langkahnya.

### L5-11 — Ophelia dan reward

**Ophelia:** Ingat kursi yang kamu siapkan untuk temanmu itu?

**Vicky:** Masih ada. Aku menjaga supaya tidak dipindahkan.

**Ophelia:** Bagus. Setelah hari ini, mungkin kita bisa duduk sebentar tanpa membahas daftar paket.

**Vicky:** Aku mau. Mungkin sambil berjalan seperti dulu? Kali ini aku janji tidak salah belok.

**Ophelia:** Aku tidak percaya janji yang terakhir itu. Tapi aku mau menemanimu.

**Vicky:** Ophelia... terima kasih sudah tetap di sini.

**Ophelia:** Terima kasih juga sudah pulang, Vicky.

[Sistem: setelah laporan dan percakapan, berikan reward level 5 satu kali; jenis dan kegunaannya belum ditetapkan. Arahkan pemain pulang untuk penutup; tidak membuka level 6.]

### L5-12 — Perpisahan di posko

**Bu Rina:** Besok kami akan melanjutkan pendataan bersama. Masih ada banyak yang perlu dibicarakan.

**Vicky:** Saya akan datang membantu. Terima kasih sudah mempercayai saya dan mengingatkan kalau saya keliru.

**Pak Jonathan:** Posko ini akan tetap menjadi tempat warga menyampaikan kebutuhan. Sekarang, pulanglah dan beristirahat.

### L5-13 — Penutup di rumah

**Ibu:** Selamat pulang, Vicky. Bagaimana pertemuannya?

**Vicky:** Kami masih punya banyak pekerjaan, Bu. Tapi sekarang warga, posko, dan sponsor bisa membicarakannya bersama.

**Ayah:** Itu awal yang baik.

**Vicky:** Waktu aku pulang, kukira aku harus memperbaiki semuanya sendiri. Ternyata yang paling penting adalah mendengarkan dan berjalan bersama.

**Ibu:** Kalau begitu, besok kita lanjutkan. Malam ini, makan dulu.

**Vicky:** Siap, Bu.

### L5-14 — Layar akhir

[Visual kampung pada sore/malam, posko dan warga. Kredit dapat muncul setelah layar penutup.]

**Narasi:** Satu pengantaran tidak menyelesaikan semua persoalan. Namun, ketika warga didengar dan bekerja bersama, harapan memiliki tempat untuk tumbuh.

**Teks layar:** `JEJAK ASA`  
`Bantuan sampai, harapan tumbuh.`

**Pilihan:** `Kembali ke Menu Utama`.

## 10. Dialog cabang dan kondisi gameplay bersama

### G01 — Pilihan intimidasi

[Gunakan setelah dialog encounter level terkait.]

**Sistem:** `Pilih cara menghadapi intimidasi.`

- `1. Menghindar — 50% berhasil mundur, 50% berujung battle.`
- `2. Bertengkar — masuk battle.`
- `3. Minta bantuan warga — tersedia setelah memenuhi syarat kepercayaan.`

[Jika bantuan terkunci.]

**Sistem:** `Bantu minimal satu warga terlebih dahulu agar bantuan warga tersedia.`

### G02 — Menghindar berhasil

**Vicky:** Aku akan mundur dan mencari jalur lain.

**Preman:** Pergi sana. Jangan coba-coba lewat sini lagi.

[Sistem: pemain kembali ke posisi aman. Penghalang tetap ada; jalur memutar dapat digunakan.]

### G03 — Menghindar gagal

**Vicky:** Aku akan mencoba melewati gang sebelah.

**Preman:** Mau pergi begitu saja? Tidak semudah itu!

**Vicky:** Mereka menghadangku. Aku harus bersiap.

[Sistem: masuk battle preman; probabilitas cabang 50%.]

### G04 — Memilih bertengkar

**Vicky:** Aku tidak akan menyerahkan paket yang sudah disiapkan untuk warga.

**Preman:** Kalau begitu, hadapi kami!

[Sistem: masuk battle preman.]

### G05 — Meminta bantuan warga

[Hanya jika syarat terpenuhi. Warga bergerak mendekat dan menarik perhatian preman.]

**Warga:** Vicky, lanjutkan pengantaran. Kami akan berbicara dengan mereka.

**Preman:** Kenapa kalian semua datang ke sini?

**Warga:** Karena ini jalan kami juga. Kami ingin bantuan untuk tetangga sampai dengan aman.

**Vicky:** Terima kasih. Aku akan segera menyelesaikan pengantaran.

[Sistem: tunggu animasi bantuan selesai, buka penghalang, lalu kembalikan kendali.]

### G06 — Battle preman menang/kalah

[Menang.]

**Preman:** Baik, kami mundur. Lewat saja!

**Vicky:** Aku hanya ingin paket sampai ke warga.

**Sistem:** `Jalan terbuka. Lanjutkan pengantaran.`

[Kalah.]

**Vicky:** Aku belum berhasil. Aku harus kembali dan mencoba lagi.

**Sistem:** `Pengantaran berhenti karena kalah battle.`

[Masuk hasil gagal/parsial. Retry mengulang level aktif, bukan menghapus level sebelumnya.]

### G07 — Waktu habis

**Vicky:** Waktunya habis. Aku belum berhasil mengantar semua paket hari ini.

**Pak Jonathan:** Catat apa yang sudah terjadi. Kita akan mencoba lagi dengan rute yang lebih tepat.

**Sistem:** `Waktu habis. Ulangi level untuk mencoba kembali.`

### G08 — Hasil misi

[Berhasil: syarat penyerahan dan boss bila ada terpenuhi.]

**Sistem:** `Pengantaran selesai. Kembali ke posko dan lapor kepada Pak Jonathan.`

[Parsial: setidaknya satu paket sampai, tetapi timer habis atau kalah sebelum syarat berhasil.]

**Sistem:** `Sebagian bantuan tersalurkan, tetapi misi belum selesai. Ulangi level untuk mencoba lagi.`

[Gagal: belum ada paket sampai dan misi berakhir.]

**Sistem:** `Misi belum berhasil. Ulangi level untuk mencoba lagi.`

**Pilihan hasil gagal/parsial:** `Ulangi Level` / `Kembali ke Menu Utama`.

[Konfirmasi ke menu memakai teks G12; data yang belum tersimpan tidak boleh dinyatakan aman.]

### G09 — Laporan dan reward berulang

[Jika belum semua syarat level terpenuhi.]

**Pak Jonathan:** Selesaikan pengantaran dan hadapi hambatan level ini terlebih dahulu. Setelah itu, kembali untuk melapor.

[Jika laporan sudah diberikan tetapi Ophelia belum diajak bicara.]

**Pak Jonathan:** Laporanmu sudah saya catat. Temui Ophelia sebelum mengambil perlengkapan dan pulang.

[Jika reward sudah diterima.]

**Pak Jonathan:** Laporan dan perlengkapan untuk hari ini sudah selesai. Silakan pulang dan istirahat.

### G10 — Ophelia sebelum dan sesudah percakapan

[Sebelum level aktif selesai.]

**Ophelia:** Hati-hati di jalan, Vicky. Ceritakan harimu setelah pengantaran selesai.

[Setelah percakapan level selesai dan reward diberikan.]

**Ophelia:** Pekerjaan hari ini sudah selesai. Sampai bertemu lagi, Vicky.

[Jika level selesai tetapi laporan Jonathan belum diberikan.]

**Ophelia:** Selamat kembali. Laporkan pengantaran ke Pak Jonathan dulu, ya. Setelah itu kita bicara.

### G11 — Posko, pintu, dan rumah

[Mendekati satu-satunya gerbang masuk/keluar posko. Setelah baru tiba, pilihan tidak langsung muncul kembali sampai pemain keluar dari area gerbang dan mendekat lagi.]

**Sistem:** `Mau pergi ke mana?`

**Pilihan:** `Pulang ke Rumah` / `Berangkat ke Level` / `Batal`.

[Tujuan yang belum tersedia mengikuti progres dan menampilkan alasan; Batal menutup pilihan tanpa berpindah scene.]

[Memilih level yang belum terbuka.]

**Sistem:** `Level ini belum terbuka. Selesaikan tugas hari ini dan istirahat di rumah.`

[Ingin pulang sebelum laporan.]

**Vicky:** Aku harus melaporkan hasil pengantaran kepada Pak Jonathan terlebih dahulu.

[Ingin pulang setelah laporan tetapi sebelum bicara dengan Ophelia.]

**Vicky:** Aku sudah berjanji akan bercerita kepada Ophelia sebelum pulang.

[Ingin masuk level berikutnya sebelum istirahat.]

**Vicky:** Pekerjaan hari ini selesai. Aku harus pulang dan beristirahat sebelum memulai tugas berikutnya.

[Berinteraksi dengan orang tua di luar adegan khusus.]

**Ibu:** Kalau lelah, pulang dan istirahatlah. Jangan lupa makan.

**Ayah:** Semoga pengantaranmu lancar. Sampaikan salam kepada warga di posko.

### G12 — Jeda dan kembali ke menu

**Sistem:** `Permainan dijeda.`

**Pilihan:** `Lanjutkan` / `Menu Utama`.

[Jika memilih Menu Utama.]

**Sistem:** `Kembali ke menu utama? Progres yang belum disimpan dapat hilang.`

**Pilihan:** `Batal` / `Ya, ke Menu`.

[Jika Batal: kembali ke layar jeda; jika Ya: keluar ke menu.]

## 11. Teks antarmuka yang diperlukan

Teks berikut merupakan naskah usulan untuk desain menu baru; implementasi dan daftar pengaturannya belum final.

### Main Menu

- `Mulai` — memulai permainan baru.
- `Lanjutkan` — memuat save terakhir yang tersedia.
- `Load` — memilih save yang tersedia.
- `Setting` — membuka pengaturan.
- `Tidak ada data tersimpan.` — jika Lanjutkan/Load belum tersedia.
- `Memulai permainan baru dapat menimpa data tersimpan. Lanjutkan?` — tampilkan jika aksi benar-benar akan menimpa save.
- `Batal` / `Lanjutkan` — pilihan konfirmasi.

### Pengaturan dan load

- `Pengaturan` / `Volume Musik` / `Volume Efek Suara` / `Resolusi` / `Kembali`.
- `Pilih Data Tersimpan` / `Slot Kosong` / `Hari {hari} — Level {level}`.
- `Muat data ini? Progres yang belum disimpan dapat hilang.`
- `Batal` / `Muat`.
- `Data tidak dapat dimuat. Periksa kembali data tersimpan.` — jika proses load gagal; jangan menghapus data lama.

[Jumlah slot, pengaturan yang benar-benar tersedia, serta format data harus disepakati sebelum implementasi.]

### Rumah dan save

- `Simpan dan istirahat? Hari berikutnya akan dimulai.` — level 1–4 setelah syarat selesai.
- `Batal` / `Simpan dan istirahat`.
- `Permainan tersimpan.` — hanya setelah penulisan save berhasil.
- `Penyimpanan gagal. Progres belum tersimpan; coba lagi.` — jika penulisan gagal; jangan membuka hari berikutnya seolah save berhasil.
- `Istirahat belum tersedia. Selesaikan tugas hari ini terlebih dahulu.`
- `Hari {hari} — Level {level} terbuka.` — setelah kondisi hari berikutnya benar-benar terpenuhi.
- Pada akhir level 5: `Tutup hari ini? Cerita utama akan berakhir.` / `Belum` / `Lanjutkan`.

### Misi dan battle

- `Paket: {tersisa}` / `Warga terbantu: {terkirim}/{target}` / `Kepercayaan: {trust}` / `Sisa waktu: {waktu}`.
- `[E] Serahkan bantuan kepada {nama}` / `[E] Bicara dengan {nama}`.
- `Bantuan sudah diterima.` / `Paket habis.` / `Waktu habis.`
- `Serang` / `Bertahan` / `Item` / `HP {sekarang}/{maksimum}`.
- `Giliranmu` / `Giliran lawan` / `Aksi sedang berlangsung`.
- `Belum ada item yang tersedia.` / `Gunakan {nama item}?` / `Batal` / `Gunakan`.
- `Perlengkapan: {nama}` / `Belum ada weapon yang dipasang.`
- `Reward diterima: {nama reward}.` — hanya setelah reward benar-benar diberikan.
- `Reward hari ini sudah diterima.` — jangan memberikan reward ganda.

## 12. Catatan handoff A/B dan Role C

- **A/B:** tentukan state dan pemicu per ID adegan; dialog warga sebelum dan sesudah penyerahan harus mengikuti state penerima. Pastikan boss dipicu sekali setelah syarat, dan retry tidak mengulang reward yang sudah diberikan.
- **A/B:** sebelum implementasi, putuskan aturan save/slot/autosave, item dan weapon, jumlah penerima, serta timer/posisi aman saat menghindar. Jangan membuat teks menjanjikan sistem yang belum ada.
- **Role C:** storyboard memberi acuan suasana, lokasi, tokoh, dan pose minimal; bukan daftar final model, jadwal produksi, atau kewajiban membuat cutscene kompleks.
- **Review bersama:** periksa keterbacaan dialog, representasi warga Jakarta Utara tanpa karikatur, nada hubungan Vicky–Ophelia, dan konsistensi transisi setelah aturan gameplay disepakati.
