# Pengujian Play Mode

| Folder | Yang diverifikasi |
| --- | --- |
| `Campaign` | Lima level, kontrak alur, entry langsung level dan integrasi posko |
| `Story` | Pembukaan P01, P02, fade dan prolog posko |
| `Movement` | Gerak, jalur kampung dan navigasi rumah |
| `Interaction` | Pemilihan target, prompt dan interaksi di map |
| `Mission` | Pengantaran, urutan, timer, prioritas hasil dan retry |
| `Obstacles` | Perilaku preman dan efek genangan |
| `Environment` | Batas rumah, keamanan keluar, keterbacaan dan challenge Level 1 |
| `UI` | Presentasi GUI, hover mouse, tujuan dan integrasi menu |

Jalankan lewat **Window → General → Test Runner → PlayMode**. Assembly tetap `JejakAsa.PlayModeTests`. Nama file/class tes serta ID U02–U18 dipertahankan agar hasil lama tetap dapat dilacak.

Tes otomatis memeriksa kasus yang dituliskan di masing-masing file. Hasilnya tidak menggantikan penilaian visual, playtest kesulitan atau pengujian executable Windows.
