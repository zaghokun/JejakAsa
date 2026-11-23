# Alat Unity Editor

Script di folder ini berjalan di Unity Editor. Generator menyusun objek, material, collider dan referensi komponen yang kemudian tersimpan dalam scene; kode runtime berada di `../Scripts/`.

| Folder | Isi |
| --- | --- |
| `Environment` | Dekorasi rumah, lanskap, keamanan gerbang, permukaan dan visual posko |
| `Levels` | Visual Level 1 serta rute, pagar, genangan dan konfigurasi preman |
| `Story` | Integrasi set dan referensi P03–P05 di Posko |
| `UI` | Aturan impor gambar UI, sprite, tekstur dan 9-slice |

Awalan `Editor_` membedakan alat ini dari komponen gameplay. Nama menu Unity yang sudah tersedia tetap sama. Generator dijalankan secara sadar melalui menu yang bersangkutan; simpan scene sebelum membangun ulang root yang dikelolanya.

Daftar nama lama–baru dan fungsi setiap alat ada di [peta script dan progres](../../../../../docs/peta-script-dan-progres.md).
