# Jejak Asa — Desain Game Kasar

## 1. Gambaran Umum

- **Judul game:** Jejak Asa
- **Genre:** RPG 3D dengan kamera isometrik tetap, eksplorasi, pengantaran misi, dan pertarungan turn-based.
- **Platform:** PC.
- **Engine:** Unity.
- **Tema:** Solidaritas warga dan upaya membantu masyarakat menghadapi kemiskinan serta hambatan distribusi bantuan.
- **Inspirasi visual/mekanik:** struktur eksplorasi dan pertarungan RPG Maker, diwujudkan sebagai game 3D.

## 2. Premis Cerita

Seorang mahasiswa yang baru lulus kembali ke kampung halamannya di Jakarta Utara. Ia melihat warga di lingkungannya masih menghadapi kemiskinan dan kesulitan memperoleh bantuan. Ia mulai membantu dengan mengantarkan bantuan sosial kepada warga.

Dalam perjalanan, ia menghadapi intimidasi dari sekelompok orang yang ingin menghentikan distribusi. Pemain menentukan cara melewati mereka: menghindar, bertengkar, atau meminta bantuan warga.

Warga bukan sekadar penerima bantuan: mereka dapat membantu tokoh utama dan ikut menentukan keberhasilan misi.

## 3. Tujuan Pemain

Menyelesaikan pengantaran bantuan dengan mengatur waktu dan rute, sambil menjaga kepercayaan warga serta memilih cara menghadapi intimidasi.

## 4. Siklus Permainan Utama

1. Berbicara dengan warga dan mengetahui kebutuhan serta lokasi penerima bantuan.
2. Menerima bantuan yang perlu dibagikan dan memulai misi dengan batas waktu.
3. Menjelajahi lingkungan, memilih jalur, dan menghindari atau menghadapi musuh.
4. Mengantarkan bantuan kepada warga yang dituju.
5. Menerima hasil misi dan melihat perubahan kepercayaan warga atau akses bantuan pada misi berikutnya.

## 5. Mekanik Utama

### Eksplorasi dan pengantaran

- Pemain bergerak di lingkungan 3D dengan kamera isometrik tetap.
- Setiap misi memiliki daftar penerima dan batas waktu.
- Pemain memilih urutan serta jalur pengantaran.
- Batas waktu menjadi tantangan mengatur perjalanan. Kegagalan mencapai semua tujuan tidak membuat warga menjadi pihak yang disalahkan.

### Intimidasi dan pilihan pemain

Saat menghadapi musuh yang mengintimidasi, pemain memilih salah satu tindakan:

- **Menghindar:** melewati ancaman tanpa pertarungan, dengan kemungkinan harus mengambil jalur lain.
- **Bertengkar:** masuk ke arena pertarungan turn-based.
- **Meminta bantuan warga:** warga membantu mengalihkan perhatian atau membuka jalan. Pilihan ini tersedia bila kondisi misi atau kepercayaan warga memungkinkan.

Timer pengantaran berhenti selama pertarungan agar pemain tidak kehilangan waktu karena berada di layar battle.

### Pertarungan turn-based

- Pemain dan musuh bergiliran melakukan aksi.
- Pertarungan berfokus pada mengatasi intimidasi, bukan kekerasan berat.
- Hasil pertarungan membuka kembali jalur pengantaran.

Detail aksi, statistik, dan item belum ditetapkan; tentukan setelah prototipe alur eksplorasi terasa menyenangkan.

### Kepercayaan warga

- Kepercayaan meningkat ketika pemain membantu warga dan menyelesaikan pengantaran.
- Kepercayaan dapat membuka bantuan warga pada misi berikutnya.
- Kepercayaan memengaruhi dukungan yang tersedia, bukan nilai atau martabat warga.

## 6. Kondisi Misi

- **Berhasil:** bantuan sampai kepada penerima sebelum batas waktu.
- **Berhasil sebagian:** sebagian bantuan terkirim; cerita menunjukkan dampaknya tanpa menyalahkan warga yang belum menerima bantuan.
- **Gagal:** waktu habis sebelum pengantaran selesai. Pemain dapat mencoba misi lagi atau melanjutkan cerita sesuai rancangan level.

## 7. Arah Cerita dan Karakter

- Tokoh utama ingin membantu, tetapi belajar bahwa perubahan membutuhkan kerja sama warga.
- Warga memiliki kepribadian, kebutuhan, dan pendapat yang berbeda.
- Musuh dan tokoh antagonis adalah karakter fiktif; jangan menggambarkan semua pejabat atau warga sebagai kelompok yang seragam.
- Konflik utama berpusat pada intimidasi yang menghambat pengantaran, bukan simulasi lengkap kebijakan bansos.

## 8. Hubungan dengan SDGs

- **SDG 1 — Tanpa Kemiskinan:** tema bantuan dan kondisi ekonomi warga.
- **SDG 11 — Kota dan Permukiman Berkelanjutan:** latar kehidupan di lingkungan perkotaan.
- **SDG 16 — Perdamaian, Keadilan, dan Kelembagaan yang Tangguh:** konflik intimidasi dan distribusi yang adil.

## 9. Gaya Visual dan Audio

- Visual 3D low-poly dengan kamera isometrik tetap; model dan lingkungan dibuat sederhana agar sesuai dengan cakupan proyek.
- Lingkungan kampung perkotaan dibuat fiktif dan menghormati warga yang menjadi inspirasi.
- Musik eksplorasi bernuansa hangat; musik pertarungan lebih tegang.
- Efek suara sederhana untuk dialog, interaksi, dan pertarungan.

## 10. Batas Prototipe Pertama

Buat satu bagian kecil permainan yang mencakup:

- Satu area lingkungan 3D.
- Tokoh utama dan beberapa warga.
- Satu misi dengan beberapa pengantaran dan timer.
- Satu pertemuan musuh dengan pilihan menghindar, bertengkar, atau meminta bantuan warga.
- Satu pertarungan turn-based.
- Hasil misi yang memperlihatkan perubahan kepercayaan warga.

Tunda boss terakhir, banyak peta, banyak jenis musuh, percabangan cerita besar, dan sistem ekonomi sampai siklus inti ini terbukti berjalan.

## 11. Hal yang Belum Diputuskan

- Judul final dan nama karakter.
- Durasi timer serta jumlah penerima per misi.
- Detail aturan pertarungan dan konsekuensi jika kalah.
- Apakah misi gagal dapat diulang atau cerita berlanjut dengan hasil berbeda.
