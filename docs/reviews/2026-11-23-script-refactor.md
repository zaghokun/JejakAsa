# Verifikasi penataan script — 23 November 2026

## Perubahan

- 21 script runtime dan 10 alat Editor diberi nama sesuai tanggung jawabnya, lalu dipindahkan ke folder fungsi.
- 29 file tes Play Mode dikelompokkan; nama class dan ID tes tetap sama.
- Struct `DialogueLine` dipisahkan ke `Story/Data/StoryData_BarisDialog.cs`. Nama tipe dan field data dipertahankan.
- Referensi C#, identifier class scene, target callback dan tautan dokumentasi diperbarui. Aturan gameplay, format save, field Inspector dan nama method callback tidak diubah.

[Peta lengkap nama lama–baru dan fungsi file](../peta-script-dan-progres.md).

## Pemeriksaan file dan scene

- Hash SHA-256 metadata sebelum dan sesudah perpindahan sama untuk seluruh 60 file yang dipindahkan.
- Tidak ada GUID duplikat atau referensi tipe lama dalam kode/identifier scene setelah penataan.
- Delapan scene diperiksa: Kampung, Kampung_L2–L5, MainMenu, Posko dan Rumah. Semuanya memiliki **0 Missing Script**.
- 38 file yang mendeklarasikan class berhasil dikenali Unity dengan nama class sesuai nama file. Dua file model/struct data diperiksa melalui kompilasi.
- Lima callback persistent menu utama masih menunjuk target dan method `KlikTombol` yang tersedia.
- Unity kembali ke Edit Mode, scene Kampung terbuka tanpa perubahan yang belum disimpan. Pemeriksaan Console terakhir mengembalikan 0 error.

## Play Mode

Run seluruh assembly `JejakAsa.PlayModeTests` menemukan 92 kasus: 90 lulus dan 2 gagal karena asumsi tes lama.

1. `U02KampungRouteTests` masih melewati titik jalur graybox yang sekarang tertutup pagar. Waypoint diperbarui untuk memverifikasi rute kering memutar dan rute utama berlumpur menuju ketiga warga. Tes navigasi memberi anggaran 300 detik untuk mengisolasi keterjangkauan; kasus campaign terpisah tetap memverifikasi misi dengan anggaran Level 1 sebesar 50 detik. Titik pendekatan berhenti sebelum collider warga.
2. `U03KampungInteractionTests` mengharapkan potret laki-laki generik untuk penerima pertama. Tes kini memastikan identitas Ibu Nur dan potret Ibu, sekaligus tetap memverifikasi buka dialog dengan E dan tutup lewat mouse.

Pengujian ulang kelas interaksi: **2/2 lulus**. Pengujian ulang akhir rute: **1/1 lulus**. Dengan 89 kasus lain yang lulus pada run penuh, hasil akhirnya **92 kasus unik terverifikasi lulus**; ini merupakan gabungan run penuh dan pengujian ulang, bukan klaim satu run penuh terakhir 92/92.

Job run penuh: `541a78e03f634b5491a4271187dff52e`. Run kelas interaksi dan rute: `d8bc819943d84bcaa58d24b544655e22`. Run akhir rute: `5651739c6f28446a9ca39047971c5788`.

Tidak dibuat build Windows baru untuk penataan ini. Penilaian visual dan playtest kesulitan tetap dilakukan terpisah dari pemeriksaan struktur kode.
