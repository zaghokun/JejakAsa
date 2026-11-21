# P02 — Mendengarkan warga dan lingkungan posko

Tanggal: 19 November 2026. Cakupan: P02 / SB-02 dari storyboard, lingkungan scene Posko, dan visual karakter. Kelanjutan P03–P05 sudah terintegrasi pada 21 November 2026; lihat [catatan P03–P05](p03-p05-posko.md).

## Cara mencoba

1. Mulai permainan baru dari menu, selesaikan P01 di rumah, lalu keluar ke Posko.
2. Ikuti tujuan “Dengarkan kebutuhan warga bersama Bu Rina”. Bu Rina berdiri dekat jalur masuk.
3. Dekati Bu Rina dan tekan E. Selesaikan lima baris dialog menggunakan Enter/Spasi atau tombol lanjut.
4. Setelah selesai, temui Jonathan untuk P03, lanjutkan pertemuan Ophelia P04, lalu saksikan P05 sebelum briefing harian.

Game baru lewat `StartCampaignFromHome` mengaktifkan persyaratan P02. Save lama dan entry graybox/tes tanpa flag baru mempertahankan perilaku sebelumnya; tidak dipaksa mengulang prolog. Flag `isP02ListeningDone` adalah flag campaign, tidak dihapus oleh reset harian. Belum ada autosave posko; aturan simpan manual di rumah tetap berlaku.

## Story dan interaksi

Naskah lima baris mengikuti P02 di `storyboard-dialog-jejak-asa.md`. `P02_MendengarkanWarga` memakai `StoryDialog_InteraksiNPC.OpenSequence`; progres baru ditandai setelah seluruh dialog selesai. Perpindahan scene sebelum selesai menjaga adegan tetap pending. Gerak dikunci selama dialog dan dipulihkan setelah penutupan. Jonathan dan Ophelia belum dapat memicu briefing/laporan ketika P02 masih pending; persediaan visual ditahan selama tahap tersebut. Portrét Bu Rina memakai aset warga perempuan yang sudah tersedia.

## Lingkungan dan karakter

- Scene: `Assets/_Project/Scenes/Posko.unity`.
- Root lingkungan: `P02_Posko_Environment`, berisi visual, stok bertahap dan `Navigation_Colliders`.
- Lingkungan: 97 objek visual dalam FBX; paving dibatch menjadi satu mesh. Rumah kampung, gerbang, naungan, bangku, planter dan pohon dibuat di proyek ini; meja/kursi/papan/pot/peti memakai katalog lokal. Ada 22 BoxCollider tambahan; floor, perimeter dan trigger graybox dipertahankan.
- Vicky, Bu Rina, Jonathan, Ophelia dan Pak Arif memakai visual model karakter lokal, material URP, avatar humanoid dan controller animasi bersama. Bu Rina memakai model warga perempuan sebagai identitas visual sementara yang konsisten dengan portrétnya.
- Source: `ArtSource/P02/P02_Posko.blend`, scene `P02_Posko`; script pembangunan `ArtSource/P02/build_posko.py`.
- Staging: `Blender/AssetLibrary_Staging/P02_Posko/JejakAsa_Posko_Library.blend` dan `ASSETS.md`.
- Tidak ada unduhan baru; kebutuhan dipenuhi aset lokal dan geometri baru. Lisensi model katalog tetap mengikuti lisensi paket sumbernya. Folder Blender diabaikan Git, sehingga registri staging lokal tidak otomatis ikut clone; ringkasan sumber ada di dokumen ini.

## Verifikasi

18 tes unik Play Mode lulus: `P02ListeningTests`, `PoskoFlowIntegrationTests`, `R01FlowContractTests`, dan `ObjectiveHudTests`. Meliputi percakapan lengkap/tidak lengkap, reload scene, serialisasi/reset flag, avatar dan material karakter, navigasi posko, serta siklus misi lama. Dua tes P02 diulang setelah penambahan capture visual dan kembali lulus.

Pratinjau: [lingkungan Blender](../Previews/P02/posko-blender.png), [percakapan Unity](../Previews/P02/p02-dialogue-unity.png). Layout sekitar posko dan angka collider masih boleh dikalibrasi lewat playtest artistik; build Windows baru belum dibuat dalam pengerjaan ini.
