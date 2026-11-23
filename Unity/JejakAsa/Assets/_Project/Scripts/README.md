# Script runtime Jejak Asa

Folder ini berisi kode yang digunakan saat game berjalan. Nama file komponen sama dengan nama class-nya, sehingga mudah dicari lewat Project, Inspector atau pencarian kode.

| Folder | Fungsi dan titik masuk |
| --- | --- |
| `Player` | `Player_Gerak`: gerak WASD, lari, gravitasi dan perlambatan |
| `Characters` | `Character_AnimasiGerak`: idle, jalan, lari dan bicara |
| `Camera` | Kamera isometrik dan visibilitas karakter di balik atap |
| `Interaction` | Interaksi rumah, pintu kampung, distribusi paket dan gerbang posko |
| `Mission` | Pengantaran bantuan, data penerima, timer, hasil dan retry |
| `Obstacles` | Patroli/pengejaran preman dan perlambatan genangan |
| `Story` | Urutan P01–P05, fase campaign, data progres dan interaksi NPC cerita |
| `UI` | Menu, HUD, dialog/potret, tema visual, transisi dan penutup |

## Subfolder UI

- `Dialogue`: penyajian dialog dan potret.
- `HUD`: tujuan/progres, pergantian hari dan peringatan preman.
- `Menus`: menu utama dan navigasi sesi/pause/save/load.
- `Theme`: gaya visual bersama.
- `Transitions`: fade saat masuk scene.
- `Ending`: tampilan akhir cerita.

Alur cerita dimulai dari [Story/README.md](Story/README.md). Peta nama lama–baru beserta tanggung jawab tiap file tersedia di [peta script dan progres](../../../../../docs/peta-script-dan-progres.md).

Alat pembuat map dan pengimpor ada di `../Editor/`. Tes ada di `../Tests/PlayMode/`. Assembly runtime tetap `JejakAsa.Gameplay`; nama field Inspector, data save dan callback tombol tetap dipertahankan.
