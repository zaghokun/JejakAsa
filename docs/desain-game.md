# Jejak Asa — Desain Game Kasar

## 1. Gambaran Umum

- **Judul game:** Jejak Asa
- **Genre:** Game 3D dengan kamera isometrik tetap, eksplorasi dan pengantaran misi.
- **Platform:** PC.
- **Engine:** Unity.
- **Tema:** Solidaritas warga dan upaya membantu masyarakat menghadapi kemiskinan serta hambatan distribusi bantuan.
- **Inspirasi visual/mekanik:** eksplorasi RPG Maker, diwujudkan sebagai game 3D.

## 2. Premis Cerita

Seorang mahasiswa yang baru lulus kembali ke kampung halamannya di Jakarta Utara. Ia melihat warga di lingkungannya masih menghadapi kemiskinan dan kesulitan memperoleh bantuan. Ia mulai membantu dengan mengantarkan bantuan sosial kepada warga.

Dalam perjalanan, ia menghadapi intimidasi dari sekelompok orang yang ingin menghentikan distribusi. Warga bukan sekadar penerima bantuan: mereka memiliki peran dalam cerita dan kehidupan kampung.

## 3. Tujuan Pemain

Menyelesaikan pengantaran bantuan dengan mengatur waktu dan rute.

## 4. Siklus Permainan Utama

1. Berbicara dengan warga dan mengetahui kebutuhan serta lokasi penerima bantuan.
2. Menerima bantuan yang perlu dibagikan dan memulai misi dengan batas waktu.
3. Menjelajahi lingkungan dan memilih jalur pengantaran.
4. Mengantarkan bantuan kepada warga yang dituju.
5. Melapor di posko, berbicara dengan Ophelia, lalu pulang untuk menyimpan dan beristirahat.

## 5. Mekanik Utama

### Eksplorasi dan pengantaran

- Pemain bergerak di lingkungan 3D dengan kamera isometrik tetap.
- Setiap misi memiliki daftar penerima dan batas waktu.
- Pemain memilih urutan serta jalur pengantaran.
- Batas waktu menjadi tantangan mengatur perjalanan. Kegagalan mencapai semua tujuan tidak membuat warga menjadi pihak yang disalahkan.

### Pengejar dan genangan — graybox

Preman mengejar pemain dalam radius dan kontak mengurangi timer dengan jeda antar penalti. Genangan memperlambat jalan dan lari selama pemain berada di dalamnya. Nilai dan penempatan awal masih perlu kalibrasi saat integrasi level.

## 6. Kondisi Misi

- **Berhasil:** bantuan sampai kepada penerima sebelum batas waktu.
- **Gagal:** waktu habis sebelum pengantaran selesai. Pemain dapat mengulang misi.

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
- Musik eksplorasi bernuansa hangat.
- Efek suara sederhana untuk dialog dan interaksi.

## 10. Cakupan dan status

Alur mencakup lima level dan lima hari, rumah, posko, interaksi, dialog, jeda, penyimpanan, dan pengantaran bertimer. P01 tetap dipertahankan; pekerjaan P01 tidak dilanjutkan dalam revisi ini.

Rencana prototipe sebelumnya yang mencakup pertemuan dengan pilihan, pertarungan, dan perubahan kepercayaan adalah historis, bukan kriteria penerimaan saat ini.
