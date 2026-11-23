# Sumber aset genangan Level 1

Ground025 dari [ambientCG](https://ambientcg.com/view?id=Ground025), oleh Lennart Demes. Lisensi [CC0-1.0](https://docs.ambientcg.com/license/). Diunduh 23 November 2026 sebagai paket `Ground025_2K-JPG.zip` dari `https://ambientcg.com/get?file=Ground025_2K-JPG.zip`.

Paket sumber lengkap beserta `.blend` disimpan lokal di `Blender/AssetLibrary_Staging/Level1_Puddles/Ground025/`. Registri di folder induknya mencatat sumber dan pemakaian. File runtime di `Unity/JejakAsa/Assets/_Project/Art/Environment/Level1Puddles/` ikut repository.

`prepare_puddle_assets.py` menyalin color dan normal OpenGL, lalu mengubah roughness menjadi kanal alpha smoothness untuk URP. Tidak memakai gambar AI. Hash file runtime ada di `puddle-source.json`.

`build_puddle_library.py` membuat `Blender/AssetLibrary_Staging/Level1_Puddles/Level1_Puddles_Library.blend` terpisah dengan tekstur packed, tiga collection varian dan dua material sebagai asset, serta katalog `JejakAsa/Level1/Puddles`.

Mesh tepi lumpur, permukaan air, riak dan pagar dibuat di proyek ini melalui `Editor_Level1_RuteDanRintangan.cs`. Model preman memakai aset dan rig lokal yang sudah digunakan sebelumnya. Papan petunjuk sudah dihapus dari scene dan generator.
