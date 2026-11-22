# File story P01–P05

| Adegan | File | Yang diatur |
| --- | --- | --- |
| P01 / SB-01 | `P01_VickyPulang.cs` | Pembukaan rumah, dialog Vicky–Ibu, kamera dan akhir prolog rumah |
| P02 / SB-02 | `P02_MendengarkanWarga.cs` | Dialog Bu Rina dan kebutuhan warga |
| P03 / SB-03 | `P03_SponsorDanPosko.cs` | Dialog sponsor, montage penyiapan posko, urutan interaksi P02–P05 |
| P04 / SB-04 | `P04_PertemuanOphelia.cs` | Dialog pertemuan kembali dan penanda P04 selesai; dipanggil pengatur posko |
| P05 / SB-05 | `P05_BayanganAncaman.cs` | Dialog villain, kamera sinematik, lampu, audio, fade dan pemulihan kontrol |

P01 dipasang di scene `Rumah`; P02–P05 memakai scene `Posko`. Naskah acuan ada di `docs/storyboard-dialog-jejak-asa.md`, sedangkan teks yang dimainkan tersimpan di script masing-masing di atas.

Di Hierarchy Posko, buka `P03-P05_PrologPosko`:

- `P03_DaftarWarga`: properti daftar penerima.
- `P05_DialogVillain`: komponen dialog villain.
- `P05_BayanganAncaman_Set`: set sinematik yang aktif saat P05, berisi `P05A_GangDeddy` dan `P05B_RuangGasOke`.

Utility pembangun set ada di `Assets/_Project/Editor/P03P05_PoskoStoryIntegration.cs`.

## Script pengatur story bersama

Folder `Story/System/` berisi sistem yang dipakai bersama oleh P01–P05 dan alur harian berikutnya.

| Nama lama | Nama sekarang | Yang diatur |
| --- | --- | --- |
| `GameFlowManager.cs` | [StoryFlow_PengaturAlur.cs](System/StoryFlow_PengaturAlur.cs) | Fase campaign, hari, syarat transisi, briefing/laporan, save/load |
| `GameFlowState.cs` | [StoryState_DataProgres.cs](System/StoryState_DataProgres.cs) | Data progres, flag story, fase harian dan metadata save |
| `StoryNpcController.cs` | [StoryDialog_InteraksiNPC.cs](System/StoryDialog_InteraksiNPC.cs) | Interaksi NPC, menjalankan urutan dialog dan menandai percakapan selesai |
| `DialoguePresentation.cs` | [StoryDialog_Tampilan.cs](System/StoryDialog_Tampilan.cs) | Potret dan penyajian dialog bersama |
| `ObjectiveHud.cs` | [StoryObjective_TujuanAktif.cs](System/StoryObjective_TujuanAktif.cs) | Tujuan aktif serta HUD hari/fase sesuai progres |

Nama class komponen mengikuti nama file baru. GUID `.meta`, nama field save, `GameProgressData`, `DayPhase`, dan nilai enum dipertahankan agar data campaign serta referensi scene tetap terhubung.
