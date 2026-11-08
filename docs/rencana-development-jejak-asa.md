# Jejak Asa — Rencana Development Tim

## 1. Tujuan, status, dan cara memakai dokumen

Dokumen operasional untuk tiga orang, dimulai ketika proyek Unity belum dibuat. Dua anggota berkolaborasi menangani Role A dan B; satu anggota menangani Role C. Tidak ada penetapan nama anggota. Untuk menghindari edit bertabrakan, setiap tugas dan file tetap memiliki satu pemilik yang disepakati sebelum dikerjakan.

Target utama: satu build Windows yang lengkap dan dapat dipresentasikan pada akhir Hari 21. Hari 22–28 hanya cadangan pengujian, stabilisasi, dan latihan presentasi. Semua fitur wajib harus sudah masuk sebelum minggu cadangan; jangan menjadikan minggu keempat sebagai syarat agar game dapat selesai.

Hari adalah hari kalender relatif terhadap hari mulai tim, bukan tanggal tertentu. Jadwal mengasumsikan ketiga role dapat bekerja secara rutin dan paralel. Jika ketersediaan anggota berbeda, sepakati kapasitas pada Hari 1 dan ubah pembagian pekerjaan, bukan diam-diam menghilangkan fitur.

Dasar desain: `docs/desain-game.md` dan presentasi `docs/Kelompok Jejak Asa.pptx`. Dokumen ini menetapkan detail implementasi yang sebelumnya belum final. Konsep utama tetap: RPG 3D low-poly, kamera isometrik tetap, pengantaran bansos, intimidasi, pilihan menghindar/bertengkar/bantuan warga, dan battle turn-based.

Urutan kerja direvisi menjadi gameplay-first: setup Kampung → gerak dan interaksi → satu pengantaran → tiga penerima, timer, hasil, dan ulang misi → intimidasi/battle/bantuan warga → main menu dan integrasi akhir. Main menu baru dikerjakan pada D11 setelah loop pengantaran lulus G1 pada D7; bukan prasyarat D1/U01. Semua fitur wajib tetap selesai dalam 21 hari.

Cara penggunaan:

1. Baca cakupan dan aturan bersama sebelum membuat script.
2. Tentukan pemilik tugas/file di antara dua anggota A/B dan satu anggota C; kepemilikan teknis tetap jelas meskipun A/B saling membantu.
3. Buat papan tugas dengan kolom Backlog, Siap, Dikerjakan, Ditinjau, dan Selesai.
4. Pecah tabel pekerjaan role menjadi kartu tugas. Setiap kartu memuat pemilik, hasil, ketergantungan, dan bukti pengujian.
5. Cocokkan hasil harian dengan jadwal dan gerbang kelulusan, bukan sekadar jumlah script atau aset.

## 2. Cakupan yang harus selesai

### 2.1 Fitur wajib pada Hari 21

| Area | Hasil minimum yang lengkap |
| --- | --- |
| Platform | Build Windows single-player, dapat dibuka tanpa Unity Editor. |
| Dunia | Satu lingkungan kampung fiktif 3D dengan gang utama, jalur memutar, pos bantuan, dan tiga tujuan warga. |
| Pemain | Gerak WASD/panah, collision, kamera isometrik tetap, dan interaksi dekat. |
| Misi | Satu misi tiga paket untuk tiga penerima, briefing, daftar tujuan, dan timer. |
| Intimidasi | Satu pengintimidasi dengan pilihan menghindar, bertengkar, atau meminta bantuan warga. |
| Battle | Satu arena 3D turn-based dengan dua aksi pemain, pola lawan terbaca, menang, dan kalah. |
| Kepercayaan | Pengantaran menambah kepercayaan; bantuan warga dapat digunakan setelah membantu satu penerima. |
| Antarmuka | Menu utama, HUD, dialog, pilihan intimidasi, battle UI, pause, dan hasil misi. |
| Presentasi visual | Model low-poly sederhana yang konsisten, pencahayaan, dan animasi/respons aksi yang terbaca. |
| Audio | Musik eksplorasi dan battle serta efek interaksi, penyerahan, aksi battle, dan hasil. |
| Penutupan sesi | Berhasil, hasil parsial, gagal, ulang misi, kembali ke menu, dan keluar aplikasi. |

### 2.2 Di luar cakupan baseline tiga minggu

Tidak membuat open world, multiplayer, akun/backend, database server, crafting, toko, skill tree, inventori kompleks, save/load lintas sesi, cutscene sinematik, atau banyak chapter. Boss terakhir, banyak map, dan banyak jenis musuh tetap ditunda sesuai batas prototipe desain awal. Satu pengintimidasi pada prototipe bukan boss cerita final.

Penyuluhan tidak menjadi minigame baru. Pesan solidaritas dan akses bantuan disampaikan melalui briefing serta dialog singkat warga. Tidak menambah jenis hambatan di luar intimidasi.

Build final tidak boleh menggunakan tombol kosong atau pilihan palsu. Ketiga respons intimidasi harus benar-benar memberi hasil yang berbeda. Aset sederhana diperbolehkan; fitur wajib yang tidak berfungsi tidak diperbolehkan.

## 3. Aturan game untuk implementasi

Angka berikut menjadi nilai awal bersama. Perubahan hanya dilakukan setelah playtest dan dicatat beserta alasannya. Perubahan angka keseimbangan tidak boleh mengubah aturan di satu script saja sehingga UI dan sistem lain berbeda.

### 3.1 Gerak, kamera, dan interaksi

- Pemain berjalan pada permukaan datar dengan kecepatan awal 3,5 meter/detik. Tidak ada lompat atau sprint pada baseline.
- Gunakan CharacterController untuk gerak dan collision pemain. Bangunan serta penghalang memiliki collider; NPC tidak berjalan memakai AI navigasi.
- Kamera berada pada sudut atas-miring tetap. Orientasi tidak diputar dengan mouse; posisi kamera mengikuti pemain dengan batas agar tidak keluar peta.
- Gerak relatif terhadap arah kamera pada bidang tanah. Menekan arah diagonal tidak menambah kecepatan.
- Jarak interaksi awal 1,8 meter. Bila beberapa target dekat, pilih satu target valid terdekat; tekan E hanya memicu satu interaksi.
- Input gerak tidak aktif saat dialog, pilihan intimidasi, battle, pause, atau hasil misi.
- Tombol UI dapat dipilih dengan mouse; tombol utama juga dapat dipilih dengan panah dan Enter. Esc membuka atau menutup pause pada eksplorasi dan battle.

### 3.2 Awal dan pengantaran misi

Pada D1–D10, build development langsung membuka Kampung. Briefing, tombol Mulai Pengantaran, dan Ulang Misi dibangun untuk menguji loop tanpa main menu; bukan alasan menunda seluruh UI. Pada D11, main menu ditambahkan dengan Mulai dan Keluar, lalu menjadi scene pertama build. Tombol Mulai membuka Kampung dan memakai alur misi yang sama, tanpa menyalin logika inisialisasi.

1. Kampung menempatkan pemain di pos bantuan dan menampilkan briefing. Timer belum berjalan.
2. Tombol Mulai Pengantaran menyiapkan sesi: tiga paket, tiga penerima, kepercayaan 0, dan waktu 180 detik.
3. Menutup briefing melalui tombol tersebut mengaktifkan state Eksplorasi dan timer. Tidak ada ketergantungan pada MainMenu untuk menjalankan langkah ini.
4. Pemain menentukan urutan penerima. Tidak ada urutan wajib.
5. Dekati penerima dan tekan E. Dialog menampilkan kebutuhan singkat dan tombol Serahkan Bantuan.
6. Penyerahan valid mengurangi paket satu, menandai penerima selesai, menambah jumlah terkirim satu, dan menambah kepercayaan satu. Semua perubahan terjadi sekali sebagai satu transaksi logis.
7. Menekan tombol berulang atau mengunjungi penerima yang sudah selesai tidak mengurangi paket atau menambah progres lagi.
8. Setelah tiga penerima selesai, misi langsung berakhir berhasil. Tidak perlu kembali ke pos.

Paket tidak dibuang, tidak dicuri saat menghindar, dan tidak membutuhkan UI inventori. Daftar penerima dan penghitung paket cukup untuk prototipe.

### 3.3 Timer dan prioritas proses

| Keadaan | Timer pengantaran | Input yang tersedia |
| --- | --- | --- |
| Briefing sebelum mulai | Belum aktif | Tombol Mulai Pengantaran. |
| Eksplorasi | Berkurang hingga minimum 0 | Gerak dan interaksi. |
| Dialog warga/penerima | Berhenti | Pilihan dialog/penyerahan dan Tutup. |
| Pilihan intimidasi | Berhenti | Menghindar, Bertengkar, Bantuan Warga. |
| Battle | Berhenti | Aksi pemain pada gilirannya dan pause. |
| Pause | Berhenti | Lanjut; Kembali ke Menu ditambahkan saat main menu berfungsi pada D11. |
| Hasil misi | Berhenti permanen untuk sesi itu | Ulang Misi; Menu Utama ditambahkan saat main menu berfungsi pada D11. |

Waktu 180 detik adalah waktu eksplorasi aktif, bukan durasi seluruh sesi. Tampilan awalnya 03:00. Sesi dengan membaca dialog dan battle dapat lebih lama.

Sebelum memproses penyerahan, sistem memeriksa apakah sisa waktu masih lebih besar dari 0. Jika sudah 0, penyerahan ditolak dan hasil misi diproses. Penyerahan terakhir yang telah valid menyelesaikan misi sekali; event timer tidak boleh menimpa hasil berhasil sesudahnya.

Pause harus mengingat state sebelumnya: menutup pause dari battle kembali ke battle, bukan eksplorasi. Pause juga menghentikan jeda animasi/logika giliran agar lawan tidak bertindak di belakang menu. Tidak hanya menghentikan tampilan angka.

### 3.4 Peta dan intimidasi

- Satu penerima harus dapat didatangi sebelum pemain perlu melewati pengintimidasi. Ini memastikan syarat bantuan warga dapat dicapai tanpa battle.
- Pengintimidasi menjaga gang utama menuju bagian peta berikutnya. Jalur memutar tetap bisa mencapai semua penerima dan tidak memerlukan menang battle.
- Ketika pemain pertama kali memasuki radius 1,8 meter pengintimidasi, gerak berhenti dan pilihan muncul. Pengintimidasi tidak mengejar pemain.
- Menghindar menutup pilihan dan mengembalikan kendali di posisi yang sama. Musuh tetap menjaga gang; pemain mengambil jalur memutar. Pertemuan baru boleh aktif lagi setelah pemain keluar dari radius 2,5 meter lalu mendekat kembali, agar menu tidak langsung terbuka berulang.
- Bertengkar membuka arena battle. Posisi pemain, waktu, paket, penerima selesai, dan kepercayaan disimpan dalam sesi yang sama.
- Menang battle menonaktifkan penghalang musuh dan trigger pertemuan untuk sisa misi. Pemain kembali ke posisi aman yang ditandai di dekat lokasi pertemuan, tidak bertumpuk dengan collider.
- Tidak ada damage dunia, pencurian paket, atau pertarungan real-time.

### 3.5 Kepercayaan dan bantuan warga

- Kepercayaan adalah bilangan 0–3, bertambah satu per penerima yang berhasil dibantu. Tidak ada level, XP, atau rumus tambahan.
- Bantuan Warga tersedia jika kepercayaan minimal 1 dan pengintimidasi belum diselesaikan.
- Saat belum tersedia, tombol nonaktif disertai teks Bantu minimal satu warga terlebih dahulu. Jangan hanya menampilkan tombol abu-abu tanpa alasan.
- Saat dipilih, tampilkan dialog bahwa warga membantu mengalihkan perhatian. Satu gerak/pose singkat atau perubahan posisi NPC cukup; tidak membutuhkan pathfinding.
- Penghalang dan trigger musuh dinonaktifkan untuk sisa misi, lalu kendali kembali ke eksplorasi. Tidak masuk battle dan kepercayaan tidak dikurangi.
- Karena hanya ada satu misi, dampak kepercayaan diperlihatkan pada misi yang sama. Ini merealisasikan dukungan warga tanpa menambah misi kedua yang berada di luar cakupan prototipe. Pengembangan lintas misi tetap bukan fitur baseline.

### 3.6 Battle turn-based: aturan lengkap

Battle dimulai dari kondisi baru setiap sesi pertemuan. Tidak ada peluang acak, critical hit, item, skill tree, atau sistem level.

| Parameter | Nilai/aturan |
| --- | --- |
| HP pemain | 32. |
| HP musuh | 50. |
| Aksi Serang | Mengurangi HP musuh 10. |
| Aksi Bertahan | Tidak melukai musuh; mengurangi damage lawan pada giliran lawan berikutnya. |
| Serangan biasa musuh | Damage 8, atau 2 jika pemain Bertahan. |
| Persiapan musuh | Tidak memberi damage; UI menampilkan peringatan serangan kuat berikutnya. |
| Serangan kuat musuh | Damage 18, atau 4 jika pemain Bertahan. |
| Pola lawan | Biasa → Biasa → Persiapan → Kuat, kemudian mengulang. |
| Urutan giliran | Pemain bertindak, cek HP musuh, lawan bertindak bila masih hidup, cek HP pemain. |
| Perlindungan Bertahan | Hanya berlaku untuk satu aksi lawan sesudah pilihan tersebut, lalu dihapus, termasuk bila lawan sedang Persiapan. |
| Akhir battle | HP musuh 0: menang. HP pemain 0: kalah. HP selalu dibatasi minimum 0. |

Pola saat ini dan aksi lawan yang akan datang harus terlihat. Setelah lawan selesai Persiapan, tampilkan peringatan jelas Serangan kuat berikutnya — gunakan Bertahan. Petunjuk ini tidak hanya dibedakan dengan warna.

Contoh pembuktian keseimbangan awal: Serang, Serang, Serang, Bertahan, Serang, Serang memenangkan battle dengan sisa HP pemain 4. Menekan Serang empat kali berturut-turut membuat pemain kalah pada serangan kuat pertama. Dengan demikian, kedua aksi memiliki fungsi yang nyata.

Setiap klik aksi langsung mengunci tombol sampai giliran pemain berikutnya. Animasi sederhana berupa maju-mundur, perubahan pose, atau kedipan saat kena serang diperbolehkan; perhitungan HP tetap satu kali per aksi. Tidak ada darah/gore. Tidak ada lari di dalam battle; opsi menghindar tersedia sebelum battle.

Kalah battle menghentikan misi, tetapi pengantaran yang sudah tercatat tetap muncul pada ringkasan hasil. Tidak menghapus file atau data pengguna karena progres hanya berada di memori sesi.

### 3.7 Hasil, ulang, dan keluar

| Kondisi akhir | Status yang ditampilkan |
| --- | --- |
| Tiga bantuan terkirim saat waktu masih valid | Berhasil. |
| Waktu habis atau kalah battle, dengan 1–2 bantuan sudah terkirim | Berhasil sebagian; tampilkan penyebab misi berhenti. |
| Waktu habis atau kalah battle, tanpa bantuan terkirim | Gagal; tampilkan penyebab misi berhenti. |

Ringkasan menampilkan terkirim/3, nama penerima yang selesai, kepercayaan, sisa waktu, dan penyebab akhir. Jangan menyebut warga sebagai pihak yang gagal.

Ulang Misi memuat sesi bersih langsung di Kampung: posisi di pos, briefing, waktu 180, tiga paket, penerima belum selesai, kepercayaan 0, dan musuh aktif. Pengulangan tidak melewati main menu dan tidak menyimpan state sesi sebelumnya. Pada build pengantaran awal, panel hasil cukup menyediakan Ulang Misi dan pause menyediakan Lanjut; jangan menampilkan tombol Kembali ke Menu yang belum bekerja. Mulai D11, Menu Utama keluar dari sesi dan Kembali ke Menu pada Pause meminta konfirmasi karena progres sesi akan hilang. Menutup aplikasi tidak menyimpan progres; beri informasi ini pada briefing dan kemudian pada menu.

## 4. Rancangan teknis minimum

### 4.1 Proyek dan package

- Gunakan Unity 6.3 LTS versi tepat 6000.3.27f1, sesuai arahan dosen, dengan template Universal 3D/URP. Semua anggota memakai versi tersebut; commit ProjectSettings/ProjectVersion.txt dan jangan mengeditnya untuk memaksa versi berbeda.
- Target awal Windows, keyboard dan mouse, rasio UI 16:9. Uji juga ukuran jendela lebih kecil agar tombol tidak keluar layar.
- Gunakan Input System resmi untuk kontrol; gunakan Unity UI (uGUI) dan TextMeshPro untuk antarmuka. Periksa package bawaan template sebelum menambahnya; commit manifest dan lock file.
- Gunakan CharacterController bawaan untuk pemain. Tidak memakai NavMesh karena NPC dan musuh tidak memerlukan navigasi dinamis.
- Kamera tetap dibuat sederhana. Cinemachine tidak diperlukan untuk baseline ini. Tidak memakai networking, database, atau dependency injection framework.
- Buat Git repository dengan .gitignore Unity, Visible Meta Files, dan Force Text. Commit Assets beserta .meta, Packages, dan ProjectSettings. Jangan commit Library, Temp, Logs, dan folder build sementara.
- Gunakan skala awal 1 unit = 1 meter. Player dan NPC memiliki collider yang konsisten. Aset berbeda sumber harus diperiksa skala, material URP, dan lisensinya.

### 4.2 Scene dan kepemilikan file

| Scene/kelompok aset | Pemilik edit | Fungsi |
| --- | --- | --- |
| Kampung.unity | Role A | Scene awal D1–D10; integrasi gerak, misi, penerima, timer, hasil, retry, trigger musuh, dan HUD. |
| MainMenu.unity | Role A | Dibuat dan dipasang D11 setelah loop pengantaran teruji; membuka Kampung dan keluar aplikasi memakai prefab UI Role C. |
| Battle.unity | Role B | Arena, logika giliran, karakter battle, dan battle UI. |
| VisualSandbox.unity | Role C | Mengerjakan komposisi lingkungan dan tampilan; tidak ikut build akhir. |
| Prefab lingkungan, karakter visual, dan UI | Role C | Diserahkan sebagai prefab untuk dipasang pemilik scene gameplay. |

Battle dimuat secara additive di atas Kampung. Saat battle aktif, kamera, input eksplorasi, dan visual dunia Kampung dinonaktifkan melalui root terpisah; pengelola sesi tidak ikut dinonaktifkan. Saat menang, Battle di-unload dan Kampung diaktifkan kembali pada titik aman. Saat kalah, Battle di-unload lalu overlay hasil muncul di Kampung. Tidak memuat ulang seluruh misi untuk kembali dari battle.

Dengan satu sesi yang tetap hidup di Kampung, tidak perlu singleton lintas scene, save file, atau menyalin seluruh progres ke scene battle. Uji bahwa kamera/listener audio tidak aktif ganda.

### 4.3 Data, komponen, dan kontrak antar-role

Gunakan data C# sederhana dan referensi Inspector. Tidak perlu membuat ScriptableObject untuk setiap objek; pakai hanya bila memang memudahkan data aset yang dipakai ulang. Satu sumber data untuk tiap nilai, bukan salinan di UI dan script berbeda.

| Pemilik data/logika | Tanggung jawab dan kontrak |
| --- | --- |
| PlayerController — A | Gerak dan collision; dapat diaktifkan/nonaktifkan oleh state misi. Tidak menghitung paket atau battle. |
| MissionController — A | Sumber data waktu, state, paket, penerima selesai, kepercayaan, dan hasil. Mengontrol kapan interaksi/battle dapat dimulai. |
| Recipient/interaksi warga — A | Memiliki ID unik; mengirim permintaan penyerahan ke MissionController. Tidak menambah progres sendiri. |
| Encounter — A | Membuka tiga pilihan, mengecek syarat bantuan, dan menyimpan titik kembali aman. Memanggil masuk battle melalui kontrak. |
| BattleController — B | Mengelola HP, giliran, pola musuh, dan satu hasil menang/kalah. Tidak menambah kepercayaan atau mengurangi timer misi. |
| HUD dan panel — C | Menampilkan data dari controller; mengirim klik tindakan ke pemilik logika. Tidak memutuskan hasil misi. |

Nama komponen di atas adalah rancangan tanggung jawab, bukan kewajiban membuat satu class besar atau satu file untuk setiap baris.

Kontrak yang dibekukan bersama pada Hari 2:

- Data misi: ID penerima, status delivered per ID, packagesRemaining, deliveredCount, trust, timeRemaining, missionState, dan endReason.
- Permintaan penyerahan harus mengembalikan valid/tidak valid; UI hanya berubah berdasarkan hasil valid.
- Masuk battle hanya jika state Encounter dan encounter belum selesai. Pemanggilan berulang harus ditolak.
- Hasil battle hanya menang atau kalah, dikirim sekali ke MissionController.
- HUD menerima data, bukan menyimpan perhitungan timer/kepercayaan sendiri.
- UI punya satu panel aktif sesuai state. Pause menyimpan state sebelumnya dan tidak boleh memajukan giliran.

Tidak perlu ERD karena tidak ada database. Artefak teknis yang dipakai adalah tabel data, alur state, daftar scene, dan kontrak di atas.

### 4.4 Urutan scene dan loop yang dibuktikan lebih dulu

1. D1: masukkan Kampung.unity sebagai scene pertama yang aktif pada Build Profiles. Scene cukup berisi lantai, objek sementara, kamera, dan cahaya; build ini membuktikan setup, belum mengklaim gameplay selesai.
2. D2–D4: buktikan gerak, collision, kamera, interaksi dekat, dan satu penyerahan yang tidak dapat dihitung dua kali.
3. D5–D7: perluas menjadi tiga penerima. Uji mulai misi → eksplorasi → pengantaran → hasil → ulang misi langsung di Kampung, termasuk timer habis dan hasil parsial.
4. Uji minimal tiga pengulangan sesi beruntun. Setiap sesi harus memulai data baru; tidak ada paket/trust/event timer milik sesi sebelumnya yang tertinggal.
5. D8–D10: sambungkan intimidasi, battle additive, dan bantuan warga. Battle kembali ke sesi berjalan; Ulang Misi membuat sesi baru. Kedua jalur ini tidak boleh diperlakukan sebagai reset yang sama.
6. D11: buat MainMenu, tambahkan ke Scene List sebelum Kampung, dan pastikan Mulai, Keluar, serta kembali ke menu berfungsi. Menu hanya menjadi pintu masuk/keluar, bukan pemilik data misi.

Editor dapat langsung membuka Kampung untuk pengujian sepanjang pengembangan. Konfigurasi build rilis mulai D11 tetap membuka MainMenu. Battle tidak pernah menjadi scene awal rilis, dan VisualSandbox tidak disertakan pada build final.

Loop gameplay di atas adalah perpindahan state dan respons terhadap aksi, bukan while(true) yang mengunci thread Unity. Update dipakai seperlunya untuk input/timer ketika state mengizinkan; penyerahan, hasil, dan retry dipicu oleh aksi atau perubahan state.

## 5. Pembagian tiga role tanpa nama anggota

Semua role ikut menguji game. Pembagian berikut menetapkan pemilik utama; bukan berarti anggota dilarang saling membantu. Pemilik file tetap mengintegrasikan hasil bantuannya.

### 5.1 Role A — Gameplay, dunia interaktif, dan integrasi

| ID | Pekerjaan yang harus dilakukan | Hasil dan kriteria selesai |
| --- | --- | --- |
| A01 | Membuat proyek 6000.3.27f1, repository, Kampung awal, dan build Windows pertama. | Anggota lain dapat clone dan menjalankan build yang langsung membuka Kampung; tidak membutuhkan MainMenu. |
| A02 | Membuat graybox Kampung terlebih dahulu. | Scene punya titik awal, collider, jalur utama, jalur alternatif, dan tiga lokasi penerima; loop dapat diuji langsung. |
| A03 | Gerak CharacterController dan kamera tetap. | Gerak diagonal tidak lebih cepat; dinding tidak ditembus; kamera tidak berputar. |
| A04 | Interaksi dekat, briefing, dan penyerahan. | Paket hanya diberikan pada ID valid, sekali per penerima; hasil langsung terbaca pada UI. |
| A05 | MissionController, state, timer, dan hasil. | Semua aturan pada bagian 3 berlaku; satu sumber data dan satu akhir misi. |
| A06 | Pertemuan intimidasi dan tiga respons. | Hindar bisa dipakai, battle bisa dimulai, bantuan terkunci/terbuka dengan benar. |
| A07 | Integrasi additive Battle bersama B. | Kembali dari menang tidak mereset misi; kalah menampilkan hasil; tidak ada kamera/input ganda. |
| A08 | Trust, bantuan warga, dan menonaktifkan penghalang. | Nilai naik hanya dari penyerahan valid; bantuan menyelesaikan hambatan tanpa battle. |
| A09 | Integrasi prefab visual/UI; buat MainMenu dan navigasinya pada D11 setelah G1 lulus. | Loop tidak bergantung pada menu; Mulai/Keluar/kembali ke menu bekerja pada build rilis tanpa referensi putus. |
| A10 | Build harian, integrasi akhir, dan packaging. | Folder build lengkap berjalan di komputer lain, bukan hanya executable yang disalin sendirian. |

A menerima prefab dari C dan hasil battle dari B. A tidak menunggu aset final untuk membuat gameplay. A bukan satu-satunya penguji atau satu-satunya anggota yang boleh menggabungkan pekerjaan; perubahan lintas batas tetap ditinjau pemilik sistem terkait.

### 5.2 Role B — Battle, transisi arena, dan verifikasi aturan

| ID | Pekerjaan yang harus dilakukan | Hasil dan kriteria selesai |
| --- | --- | --- |
| B01 | Memahami kontrak data dan menyiapkan arena percobaan. | Battle dapat dicoba mandiri dengan bentuk sementara, tanpa menunggu Kampung lengkap. |
| B02 | Membuat state battle dan giliran terkunci. | Satu klik menghasilkan satu aksi; giliran lawan tidak berjalan ganda. |
| B03 | Mengimplementasikan HP dan Serang/Bertahan. | Damage tepat, HP minimum 0, guard berlaku satu aksi lawan lalu habis. |
| B04 | Pola musuh dan telegraph. | Urutan Biasa/Biasa/Persiapan/Kuat konsisten dan serangan berikutnya terbaca. |
| B05 | Menang, kalah, dan penghentian aksi. | Musuh mati tidak membalas; setelah hasil, tidak ada aksi/coroutine tersisa yang mengubah HP. |
| B06 | Memasang battle UI dari C. | Angka HP, giliran, peringatan, dan tombol mencerminkan data aktual. |
| B07 | Membuat pemuatan/pembongkaran Battle bersama A. | Arena hanya aktif sekali; pause/resume dan keluar tidak meninggalkan scene atau audio aktif. |
| B08 | Menambahkan respons animasi sederhana dan SFX battle. | Aksi terbaca tanpa membuat sistem combo atau animasi kompleks. |
| B09 | Menulis regression test logika battle yang rawan. | Urutan menang, urutan kalah, dan guard yang kedaluwarsa diuji deterministik. |
| B10 | Menjalankan matriks uji bersama tim dan membantu bug integrasi. | Catatan reproduksi, hasil actual, dan perbaikan dapat ditinjau; bukan hanya laporan sudah dicoba. |

B boleh menggunakan panel sementara yang benar-benar bekerja sampai prefab C tersedia. Panel sementara harus diganti atau dirapikan sebelum rilis, bukan dibawa sebagai UI ganda.

### 5.3 Role C — Level visual, aset low-poly, antarmuka, dan audio

| ID | Pekerjaan yang harus dilakukan | Hasil dan kriteria selesai |
| --- | --- | --- |
| C01 | Menetapkan skala, palet, daftar aset, dan asal/lisensi aset. | Daftar kebutuhan minimum jelas; setiap aset pihak ketiga punya sumber dan izin pakai. |
| C02 | Membuat graybox layout bersama A. | Satu penerima bisa dicapai lebih dulu; jalur memutar benar-benar terhubung; tidak ada rumah tujuan buntu. |
| C03 | Membuat atau memperoleh model pemain, warga, dan musuh. | Satu gaya low-poly, material URP, skala masuk akal, dan orientasi konsisten. |
| C04 | Menyiapkan rumah, warung/pos, jalan, pot, dan dekor minimum. | Prefab modular dapat dipakai ulang tanpa mengubah collider gameplay secara diam-diam. |
| C05 | Menyiapkan visual paket, penanda penerima, dan musuh. | Pemain, tujuan, paket, dan ancaman mudah dibedakan tanpa bergantung pada warna saja. |
| C06 | Dahulukan briefing, HUD, dialog, pause, hasil/retry, encounter, dan battle UI. Buat prefab Main Menu pada D11. | Panel awal cukup untuk menguji loop; tombol menu baru dipasang setelah navigasinya berfungsi. Semua panel rilis punya teks, anchor, fokus, dan status aktif/nonaktif lengkap. |
| C07 | Idle/jalan pemain dan respons battle sederhana. | Tidak perlu banyak rig/animasi; gerak dan hasil aksi tetap terbaca. |
| C08 | Audio dan pencahayaan. | Musik tidak bertumpuk setelah transisi; volume masuk akal; lingkungan tidak terlalu gelap. |
| C09 | Memasang bantuan warga secara visual bersama A. | Dialog/pose/perpindahan singkat benar-benar terlihat dan musuh tidak lagi menghalangi. |
| C10 | Menguji keterbacaan, navigasi, performa visual, serta materi demo. | Petunjuk jelas; UI tidak terpotong; tangkapan layar berasal dari build aktual dan aset memiliki kredit. |

Gambar konsep yang ada di docs/visual adalah PNG, bukan file model 3D siap impor ke Unity. Model perlu dibuat atau diperoleh secara terpisah. Jangan menganggap ilustrasi PPT sudah menyelesaikan pekerjaan aset.

## 6. Dependensi, handoff, dan kerja bersama

### 6.1 Urutan dependensi

1. A membuat proyek/repository dengan Editor 6000.3.27f1 dan scene awal Kampung; B serta C memastikan hasil clone dan build berjalan pada Hari 1.
2. Semua role menyepakati skala, ID penerima, state, kontrak battle, dan tombol pada Hari 2.
3. A mengerjakan loop pengantaran tanpa main menu; B mengerjakan battle mandiri; C menyiapkan layout dan UI pengujian. Bentuk sementara diperbolehkan dan ketiganya berjalan paralel.
4. Battle memakai hasil menang/kalah yang sudah nyata sebelum dipasang ke alur misi. Jangan menghubungkan tombol battle kosong ke transisi final.
5. Prefab visual C dipasang oleh pemilik scene gameplay. Integrasi visual tidak boleh menunggu Hari 20.
6. Loop pengantaran lulus G1 pada D7; main menu dan navigasi dipasang D11. Seluruh fitur inti terhubung paling lambat D12, lalu fokus pada pembuktian, aset final, dan stabilisasi.

### 6.2 Aturan handoff

Setiap penyerahan mencantumkan file/prefab, referensi Inspector yang diperlukan, cara menjalankan, output yang diharapkan, dan masalah yang sudah diketahui. Sertakan build atau rekaman singkat jika perubahan perlu diamati.

Tombol UI yang diserahkan C harus memiliki nama/peran yang jelas. A atau B memasang callback sesuai pemilik logika; tidak ada aksi bisnis disisipkan di script visual tanpa kesepakatan.

Satu orang mengedit satu scene pada satu waktu. Perubahan ProjectSettings, Packages, tag/layer, dan nama scene diumumkan sebelum digabungkan. Jangan mengganti nama/memindahkan aset langsung lewat file explorer sehingga .meta tertinggal.

### 6.3 Ritme kerja

- Awal sesi: laporkan selesai apa, dikerjakan apa, dan hambatannya. Pilih pekerjaan berdasarkan dependensi yang sudah siap.
- Sebelum mulai edit: pull perubahan terbaru. Gunakan branch pendek per tugas; jangan menahan pekerjaan selama satu minggu tanpa integrasi.
- Sebelum merge: jalankan skenario terkait, periksa Console, dan minta peninjauan satu anggota lain untuk perubahan lintas sistem.
- Akhir sesi: catat bukti hasil, commit perubahan yang bisa dijalankan, dan buat build bersama pada hari integrasi.
- Bug dicatat dengan langkah reproduksi, harapan, hasil aktual, tingkat keparahan, dan pemilik. Menyembunyikan error Console bukan perbaikan.

## 7. Jadwal utama tiga minggu — 21 hari kalender

D = Hari. Output pada kolom terakhir adalah hasil tim yang harus dapat diamati. Pekerjaan tidak dianggap selesai hanya karena script sudah dibuat.

### 7.1 Minggu 1 — fondasi dan pengantaran berjalan

| Hari | Role A | Role B | Role C | Output tim |
| --- | --- | --- | --- | --- |
| D1 | Buat proyek URP 6000.3.27f1, Git, .gitignore, Kampung awal, dan build Windows langsung ke Kampung. | Clone proyek; cocokkan Editor/package, jalankan build, dan tinjau struktur Battle. | Clone proyek; daftar aset, lisensi, palet, dan skala. | Tiga anggota dapat membuka proyek yang sama; U01 tahap awal lulus tanpa MainMenu. |
| D2 | Graybox Kampung, titik awal, dan kontrak MissionController tanpa ketergantungan menu. | Sepakati kontrak hasil battle; buat Battle mandiri. | Layout gang dan tiga tujuan; sketsa briefing/HUD/hasil serta titik kamera. | Kontrak state/data, pemilik scene, dan jalur uji langsung Kampung disepakati. |
| D3 | Gerak, collision, kamera, dan input lock. | HP, urutan giliran, dan aksi Serang. | Model/prefab bentuk dasar; rumah modular dan paket. | Pemain bisa berjalan tanpa menembus bangunan; battle dasar dapat dijalankan sendiri. |
| D4 | Satu penerima, prompt E, briefing, dan penyerahan satu kali. | Bertahan, pola musuh, dan peringatan serangan kuat. | Prefab HUD, dialog, dan tombol dengan teks sementara. | Satu paket bisa diserahkan; pola lawan berfungsi. |
| D5 | Tiga penerima, paket tersisa, dan trust. | Menang/kalah serta penguncian tombol. | Prioritaskan pause, hasil/retry, encounter UI, dan battle UI; belum membuat main menu. | Pengantaran lengkap dapat dihitung; battle punya dua hasil nyata. |
| D6 | Timer per state, hasil, ulang bersih langsung Kampung, dan pause. | Test battle deterministik; persiapan additive load bersama A. | Perbaiki keterbacaan; serahkan panel awal untuk integrasi loop. | Misi bisa berhasil, parsial, atau gagal, lalu diulang tanpa menu dan tanpa progres dobel. |
| D7 | Gabungkan pekerjaan; uji tiga sesi beruntun dan buat build milestone langsung Kampung. | Uji battle mandiri dan transisi percobaan. | Uji navigasi/UI loop pada build; perbaiki rute dan collider bersama A. | G1: mulai → antar → hasil → ulang bekerja; battle mandiri siap. MainMenu belum menjadi syarat. |

### 7.2 Minggu 2 — seluruh fitur terhubung

| Hari | Role A | Role B | Role C | Output tim |
| --- | --- | --- | --- | --- |
| D8 | Encounter dan menghindar; cek jalur alternatif. | Battle additive, laporan hasil sekali, dan kontrol kamera/audio. | Lingkungan low-poly utama dan visual NPC. | Pemain bisa menghindar dan mencapai tujuan tanpa battle. |
| D9 | Pasang transisi battle dan titik kembali aman. | Kembali menang/kalah; pause selama giliran. | Pasang model/animasi sederhana ke prefab battle dan karakter. | Pergi-pulang battle tidak mereset timer, paket, dan penerima. |
| D10 | Bantuan warga, syarat trust, dan musuh selesai permanen. | Uji semua transisi; tangani klik cepat dan hasil ganda. | Dialog bantuan warga, penanda, dan status tombol terkunci. | Ketiga pilihan intimidasi bekerja; bantuan tidak hanya teks tanpa dampak. |
| D11 | Buat MainMenu dari prefab C, hubungkan Mulai/Keluar/kembali ke menu, pasang sebagai scene pertama build; integrasikan konfirmasi keluar sesi. | Respons aksi dan SFX battle; cek bahwa masuk sesi melalui menu tidak merusak transisi battle. | Buat prefab Main Menu; finalkan musik, SFX, pencahayaan, dan konsistensi material. | Main menu kini berfungsi dan memakai loop yang sudah teruji; tidak ada tombol menu kosong. |
| D12 | Tutup alur menu → misi → hasil → ulang/menu dan buat build terintegrasi. | Uji urutan menang/kalah dalam misi sebenarnya. | Uji jalur, menu, dan keterbacaan pada build; ambil screenshot aktual. | G2: vertical slice lengkap, termasuk main menu dan semua fitur wajib fungsional. |
| D13 | Perbaiki bug progres, timer, state, atau collision. | Regression test dan perbaikan battle/transisi. | Rapikan animasi, audio, posisi teks, dan prefab visual. | Masalah penghalang demo ditutup; tidak menambah fitur baru. |
| D14 | Buat build alpha dan tes di komputer lain. | Jalankan matriks uji penuh bersama A. | Tinjau visual/UI/audio; catat lisensi dan kredit. | Gerbang G3: alpha, daftar bug tersisa, serta pembekuan fitur. |

### 7.3 Minggu 3 — pembuktian, perbaikan, dan rilis

| Hari | Role A | Role B | Role C | Output tim |
| --- | --- | --- | --- | --- |
| D15 | Perbaiki blocker/crash dan state mission. | Perbaiki bug battle; uji regresi sesudah perubahan. | Tutup aset/UI/audio yang belum final tanpa menambah scope. | Tidak ada fitur wajib yang hilang atau tombol kosong. |
| D16 | Build untuk playtest pengguna baru. | Amati pilihan battle dan pemahaman telegraph. | Amati navigasi, prompt, serta pembacaan tujuan/waktu. | Catatan playtest nyata: kebingungan, rute, kesalahan, dan waktu eksplorasi. |
| D17 | Sesuaikan angka timer/gerak hanya berdasarkan hasil uji; catat perubahan. | Sesuaikan battle bila perlu, lalu perbarui test dan UI. | Perbaiki penanda, layout, kontras, dan suara yang terbukti bermasalah. | Angka final disepakati; tidak mengubah mekanik inti. |
| D18 | Buat release candidate dan kemasan build lengkap. | Jalankan ulang seluruh kasus kritis. | Verifikasi kredit, petunjuk, dan tampilan hasil; siapkan demo. | Gerbang G4: kandidat rilis lengkap; tidak bergantung pada minggu keempat. |
| D19 | Cadangan perbaikan bug; jangan mulai fitur baru. | Uji ulang bug yang diperbaiki. | Perbaiki masalah visual penghalang atau petunjuk yang tidak terbaca. | Tidak ada blocker; setiap perbaikan punya bukti sesudahnya. |
| D20 | Uji build bersih di komputer kedua; siapkan salinan cadangan. | Uji ulang menang, parsial, gagal, retry, dan keluar aplikasi. | Latihan demo dari build aktual; cocokkan screenshot dan penjelasan. | Game bisa dibuka tanpa Editor/internet; demonstrasi bisa diulang. |
| D21 | Rilis build dan snapshot source yang sama; bekukan perubahan. | Rekap hasil pengujian dan known issues non-blocking. | Finalkan kredit/aset dan materi presentasi. | Gerbang G5: build final, source, bukti uji, dan demo siap dikumpulkan. |

### 7.4 Minggu keempat opsional — Hari 22–28

| Hari | Fokus bersama | Batasan |
| --- | --- | --- |
| D22–23 | Playtest tambahan pada komputer dan pengguna berbeda. | Memakai build D21 sebagai baseline. |
| D24–25 | Perbaikan bug yang ditemukan; regresi kasus terkait dan alur penuh. | Tidak membuka fitur baru, map baru, atau boss. |
| D26 | Uji performa, waktu muat, audio, dan pembacaan UI. | Jangan mengganti render pipeline atau seluruh model/rig. |
| D27 | Latihan presentasi dan siapkan build cadangan yang sudah diuji. | Video demo bukan pengganti game yang bisa dimainkan. |
| D28 | Final check dan pengumpulan ulang jika memang diperlukan. | Rilis hanya build yang sudah melewati kriteria G5. |

## 8. Gerbang kelulusan dan tindakan jika terlambat

| Gerbang | Batas | Wajib terbukti |
| --- | --- | --- |
| G0 | D2 | Proyek 6000.3.27f1 terbuka di tiga komputer; build langsung Kampung; kontrak dan pemilik file jelas. |
| G1 | D7 | Gerak, tiga pengantaran, timer, hasil, pause, dan tiga retry beruntun berjalan langsung Kampung; battle mandiri menang/kalah. Main menu bukan syarat. |
| G2 | D12 | Menghindar, battle, bantuan warga, trust, dan kembali arena terhubung; main menu D11 juga berfungsi pada build. |
| G3 | D14 | Alpha dapat dimainkan di komputer lain; fitur dibekukan; bug tersisa punya prioritas dan pemilik. |
| G4 | D18 | Kandidat rilis lengkap dengan UI, audio, visual, semua jalur hasil, dan tanpa fitur palsu. |
| G5 | D21 | Seluruh pengujian kritis lulus; build bersih, source, kredit, dan demo siap. |

Jika gerbang tidak tercapai, lakukan triase pada hari yang sama. Dahulukan crash, progres hilang, state macet, dan fitur wajib yang tidak berfungsi. Gunakan bentuk/model low-poly yang lebih sederhana, kurangi dekor, pendekkan dialog, dan pakai animasi transform sederhana daripada menghilangkan pilihan pemain.

A dan B dapat membagi perbaikan logika; C dapat mengurangi beban integrasi melalui prefab yang sudah siap. Hindari memindahkan kepemilikan scene tanpa handoff. Jika kapasitas nyata tetap tidak cukup, laporkan risiko dan sepakati perubahan cakupan secara eksplisit dengan tim/dosen; jangan mengklaim fitur sudah selesai padahal disembunyikan.

## 9. Matriks pengujian penerimaan

A/B/C di kolom utama berarti pemilik pelaksanaan awal; minimal satu anggota lain memeriksa hasilnya. Semua pengujian dijalankan lagi pada kandidat rilis yang sama, bukan pada versi Editor yang berbeda.

| ID | Skenario dan hasil yang wajib terlihat | Utama |
| --- | --- | --- |
| U01 | D1: clone bersih terbuka di Unity 6000.3.27f1 dan build Windows langsung membuka Kampung. Mulai D11/rilis, konfigurasi scene awal berubah menjadi MainMenu dan Mulai membuka Kampung. | A |
| U02 | Jalan lurus/diagonal berkecepatan sama; pemain tidak menembus rumah dan tidak tersangkut jalur wajib. | A |
| U03 | Saat beberapa target berdekatan, satu tekan E hanya membuka satu interaksi. | A |
| U04 | Penyerahan ke penerima valid mengubah paket, delivered, dan trust sekali. Klik berulang tidak menggandakan hasil. | A |
| U05 | Penerima yang sudah selesai tidak menerima paket lagi; urutan ketiga penerima bebas. | A |
| U06 | Timer aktif saat eksplorasi dan benar-benar berhenti pada semua state yang ditentukan. | A |
| U07 | Timer 0 menolak penyerahan terlambat; penyerahan final yang sudah sah tidak ditimpa hasil gagal ganda. | A |
| U08 | Hasil berhasil, parsial, dan gagal benar; nama penerima serta penyebab berhenti cocok dengan sesi. | A |
| U09 | Menghindar tidak langsung membuka menu lagi; jalur memutar mencapai semua tujuan tanpa battle. | C |
| U10 | Trust 0 mengunci bantuan dengan alasan; setelah satu pengantaran, bantuan tersedia dan menyingkirkan hambatan tanpa battle. | A |
| U11 | Urutan Serang ×3, Bertahan, Serang ×2 menang dengan HP pemain 4 pada angka baseline. | B |
| U12 | Serang empat kali beruntun kalah pada serangan kuat pertama; UI menunjukkan hasil dan penyebab yang benar. | B |
| U13 | Bertahan saat Persiapan habis pada aksi Persiapan itu; perlindungan tidak terbawa ke serangan kuat berikutnya. | B |
| U14 | Klik aksi cepat tetap menghasilkan satu aksi; musuh dengan HP 0 tidak membalas; battle tidak mengirim hasil dua kali. | B |
| U15 | Menang battle mempertahankan waktu, paket, trust, dan penerima; pengintimidasi tidak muncul lagi pada sesi yang sama. | B |
| U16 | Pause dari battle menghentikan aksi yang sedang menunggu, lalu kembali ke giliran yang benar tanpa damage/aksi dobel. | B |
| U17 | Tiga retry beruntun langsung Kampung mereset seluruh sesi dan menampilkan briefing baru. D7 menguji berhasil/waktu habis; setelah integrasi, ulangi juga sesudah kalah battle. Tidak ada timer, paket, trigger, HP, atau event lama tertinggal. | A |
| U18 | Mulai D11: Mulai/Keluar bekerja, kembali ke menu dari pause meminta konfirmasi, batal mempertahankan sesi, dan Mulai lagi membuka sesi bersih. D1–D10 tidak menampilkan tombol menu yang belum bekerja. | A |
| U19 | Hanya kamera/audio listener yang benar aktif; musik tidak bertumpuk setelah beberapa transisi/ulang misi. | C |
| U20 | UI terbaca pada 1920×1080 dan 1280×720; fokus keyboard, alasan tombol terkunci, serta prompt tidak terpotong. | C |
| U21 | Build komputer kedua membuka menu, menyelesaikan misi, ulang, dan keluar tanpa Editor atau akses internet. | Semua |

Test otomatis permanen cukup untuk logika yang berisiko: penyerahan dobel, prioritas akhir misi, dan transisi/damage battle. Gunakan Unity Test Framework bila tersedia; jangan menghabiskan jadwal untuk menguji posisi teks atau keberadaan tombol. Interaksi nyata dan build tetap diuji manual karena unit test bukan bukti kamera, scene, audio, atau input sudah benar.

Catatan hasil uji minimal memuat versi build/commit, kasus, langkah, hasil aktual, lulus/gagal, dan bukti gambar/video bila diperlukan. Jika angka battle berubah pada D17, hitung ulang skenario U11–U13 serta perbarui aturan dan UI bersama-sama.

## 10. Kebutuhan aset dan standar kualitas minimum

| Kelompok | Kebutuhan baseline | Pemilik utama |
| --- | --- | --- |
| Karakter | Satu pemain, satu basis warga dengan tiga variasi sederhana, satu pengintimidasi; dapat memakai ulang model di arena. | C |
| Lingkungan | Rumah modular, pos/warung, permukaan jalan, collider penghalang, dan dekor secukupnya. | C; pemasangan collider A |
| Gameplay | Paket, penanda penerima, penanda musuh, titik spawn/kembali yang aman. | C dan A |
| Arena | Satu area battle dan dua posisi karakter; tidak perlu map eksplorasi kedua. | B dan C |
| Animasi | Idle/jalan pemain, respons serang/bertahan/kena, dan indikasi bantuan warga. Model sederhana/gerak transform boleh digunakan selama aksi terbaca. | C dan B |
| UI | Menu, briefing, HUD, dialog penerima, encounter, battle, pause/konfirmasi, dan hasil. | C; pengikatan A/B |
| Audio | Musik eksplorasi/battle dan efek untuk tindakan utama; semua punya izin pakai. | C |

Visual harus konsisten, tidak harus detail. Jangan menggunakan lokasi/nama asli untuk menuduh warga atau institusi tertentu. Jangan bergantung hanya pada warna untuk membedakan tujuan, serangan kuat, atau tombol terkunci.

Target performa awal adalah gameplay stabil minimal 30 FPS pada komputer presentasi pada 1280×720. Ini target pengujian, bukan klaim bahwa performa sudah tercapai. Catat spesifikasi mesin, pengaturan, dan hasil; kurangi dekor/bayangan sebelum mengganti sistem besar.

## 11. Definition of Done dan paket pengumpulan

Satu kartu tugas selesai hanya jika hasilnya bisa dijalankan, skenario relevan sudah dicoba, tidak menambah error Console, referensi Inspector tidak putus, dan perubahan sudah tersedia di repository. Untuk fitur lintas role, selesai berarti sudah terintegrasi, bukan hanya bekerja pada scene percobaan.

Paket final pada D21:

- Folder build Windows lengkap dengan executable beserta data pendampingnya, dikemas menjadi arsip.
- Source proyek Unity yang cocok dengan build final, tanpa Library/Temp, beserta manifest, lock file, dan ProjectSettings.
- Petunjuk singkat menjalankan game, kontrol, batasan satu sesi tanpa save, dan versi build.
- Daftar sumber/lisensi aset dan kredit.
- Rekap matriks uji serta known issues non-blocking. Crash, alur macet, fitur wajib tidak bekerja, atau progres dobel bukan known issue yang boleh dibiarkan untuk rilis.
- Presentasi desain, screenshot build aktual, dan cadangan build yang sudah diuji. Video demonstrasi boleh menjadi cadangan, bukan pengganti aplikasi.

## 12. Checklist mulai pada Hari 1

1. Dua anggota A/B menyepakati satu pembuat proyek awal dan satu pemeriksa clone/build; anggota ketiga menangani C. Pemilik tiap tugas/file dicatat tanpa penetapan nama di dokumen ini.
2. Pakai Unity 6.3 LTS 6000.3.27f1 di semua komputer. Baca aturan bagian 3 dan urutan gameplay-first pada bagian 4.4.
3. Buat satu proyek Universal 3D/URP di dalam repository bersama. Anggota lain clone proyek tersebut, bukan membuat proyek baru masing-masing.
4. Simpan Kampung.unity, tambahkan lantai/objek sementara, kamera, dan cahaya. Atur Kampung sebagai scene pertama Build Profiles; jangan membuat MainMenu dulu.
5. Aktifkan Force Text dan Visible Meta Files; gunakan .gitignore Unity di root proyek. Commit Assets beserta .meta, Packages, dan ProjectSettings; abaikan cache dan hasil build.
6. Pembuat proyek membuat build Windows; pemeriksa menjalankan build dari hasil clone. C juga membuka proyek yang sama. Selesaikan checklist U01 sebelum mulai gameplay.
7. Buat kartu D2–D7 untuk gerak → satu pengantaran → tiga penerima/timer → hasil/ulang. Setelah G1, lanjut intimidasi/battle/bantuan warga; main menu dijadwalkan D11.
8. Gunakan bentuk sementara dan UI sederhana yang berfungsi. Jangan menunggu aset final atau desain main menu untuk membuktikan loop.

Keputusan penting: targetnya game kecil yang lengkap dalam tiga minggu, bukan banyak sistem setengah tersambung. Minggu keempat menjadi ruang aman untuk meningkatkan keandalan, bukan alasan menunda integrasi.
