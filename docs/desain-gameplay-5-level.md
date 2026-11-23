# Desain Gameplay & Diferensiasi 5 Level — Jejak Asa
**Dokumen Desain Mekanik & Alur Level (Game Design Document / GDD)**

---

## 1. Latar Belakang & Masalah Desain

Pada tahap awal prototipe, alur permainan di lima level (`Kampung.unity` s.d. `Kampung_L5.unity`) terasa monoton (*repetitif / "gitu-gitu doang"*) karena hanya mengulang formula dasar yang identik:
- Mengambil 3 paket bansos di Posko.
- Berjalan menyusuri gang yang layout-nya sama.
- Menyerahkan bantuan ke 3 warga dengan urutan dan lokasi yang mirip.
- Menghadapi preman statis di titik yang sama dengan batas waktu yang sama (180 detik).

Dokumen ini mendefinisikan **diferensiasi dan eskalasi gameplay** di setiap level (Hari 1 sampai Hari 5) agar pemain merasakan tantangan baru, variasi strategi navigasi, pengenalan mekanik bertahap, dan kepuasan naratif yang memuncak pada pertarungan final.

---

## 2. Matriks Komparasi 5 Level

| Level (Hari) | Judul & Tema Naratif | Batas Waktu | Layout & Navigasi Gang | Perilaku Rintangan / Preman | Mekanik Khas / Twist Gameplay | Reward Akhir Hari |
|:---:|---|:---:|---|---|---|:---:|
| **Level 1** | **Hari 1: Langkah Pertama**<br>*(Adaptasi & Orientasi)* | 180 detik | Jalur utama lurus, bersih, dan terbuka luas. | 1 Preman statis di tengah; peluang kabur tinggi (tutorial). | **Orientasi & Dasar**: Gerak WASD, lari (`Shift`), interaksi (`E`), pengantaran 3 warga, lapor posko. | Evaluasi awal posko & briefing hari berikutnya |
| **Level 2** | **Hari 2: Genangan Banjir & Rute Tikus**<br>*(Puzzle Lingkungan)* | 160 detik | Jalan utama tergenang air & terblokir tumpukan papan. | Preman berjaga di persimpangan gang alternatif. | **Environmental Hazard (Lumpur/Genangan)**: Kecepatan lari berkurang 50% jika menerobos becek; pemain dipaksa mencari jalan tikus memutar. | **`basic_weapon`** (Tongkat kayu, +5 Damage battle) |
| **Level 3** | **Hari 3: Pemerasan & Boss Mr. Deddy**<br>*(Combat & Sinergi Warga)* | 170 detik | Akses gang belakang diblokir ganda oleh preman. | Anak buah preman kroco + **Mid-Boss Mr. Deddy**. | **Sinergi Warga (Trust $\ge 1$) & Boss Fight**: Warga pertama membantu alihkan preman kroco; Mr. Deddy wajib dikalahkan turn-based. | 1x **`healing_item`** (+12 HP) |
| **Level 4** | **Hari 4: Waktu Kritis & Preman Patroli**<br>*(Stealth & Emergency Delivery)* | 145 detik | Persimpangan sempit dengan titik intip (*blind spot*). | Preman **patroli bergerak bolak-balik** (*moving hazard*). | **Patroli Dinamis & Pacing Cepat**: Pemain harus membaca ritme langkah patroli preman atau sembunyi di celah gang; waktu sangat ketat. | 1x **`healing_item`** (+12 HP) |
| **Level 5** | **Hari 5: Puncak Krisis & Final Boss Mr. Gas Oke**<br>*(Climax Finale)* | 150 detik | Labirin barikade darurat (seng, drum, pembatas kayu). | Barikade penuh + **Final Boss Mr. Gas Oke**. | **Grand Climax**: Manuver labirin gang sempit, pertarungan puncak melawan mafia sembako (Mr. Gas Oke), epilog damai & tamat. | **Tamat Permainan** (*Victory Screen*) |

---

## 3. Rincian Desain & Spesifikasi Tiap Level

### Level 1 (Hari 1) — "Langkah Pertama: Pengenalan & Orientasi"
* **Nuansa Cerita**: Hari pertama Vicky turun ke lapangan sebagai relawan posko bansos di kampung kelahirannya.
* **Tujuan Desain**: Memberikan pengalaman awal yang ramah (onboarding), mengenalkan kontrol gerak, mekanik lari (*sprint* dengan tombol Shift), pengambilan bantuan di posko, dan cara berinteraksi dengan warga.
* **Parameter Level**:
  - `initialTimeSeconds`: **180 detik**.
  - `initialPackages`: **3 paket**.
* **Layout Gang**:
  - Jalan utama gang bersih tanpa halangan berat.
  - Tiga penerima bantuan (Pak RT, Bu Siti, Bu Dewi) berada di posisi yang mudah dilihat dan dijangkau dari jalan utama.
* **Tantangan & Rintangan**:
  - 1 Preman kampung berdiri statis di persimpangan tengah.
  - Memberikan dialog peringatan awal. Pemain dapat memilih opsi *Menghindar* dengan tingkat keberhasilan tinggi atau mencoba pertarungan ringan.
* **Kondisi Selesai**:
  - Mengantar 3/3 paket $\rightarrow$ kembali ke Posko $\rightarrow$ lapor ke Pak Jonathan & dialog Ophelia $\rightarrow$ pulang ke Rumah $\rightarrow$ tidur di kasur.

---

### Level 2 (Hari 2) — "Genangan Banjir & Rute Tikus"
* **Nuansa Cerita**: Semalam hujan lebat mengguyur wilayah pesisir Jakarta Utara. Saluran air mampet mengakibatkan gang utama tergenang air bercampur lumpur, serta beberapa titik tertutup sampah kayu.
* **Tujuan Desain**: Menguji kemampuan navigasi pemain melalui rute alternatif (tidak bisa hanya berjalan lurus seperti Level 1) serta memperkenalkan konsep rintangan lingkungan (*environmental hazard*).
* **Parameter Level**:
  - `initialTimeSeconds`: **160 detik** (waktu terasa lebih mendesak karena rute lebih panjang).
  - `initialPackages`: **3 paket**.
* **Mekanik Khusus — Environmental Hazard (Area Becek / Lumpur)**:
  - Kotak trigger transparan (`EnvironmentalHazard.cs`) diletakkan di gang utama.
  - Saat pemain melangkah ke dalam genangan, kecepatan jalan berkurang sebesar 50% (dari 3.5 m/s menjadi 1.75 m/s).
  - Terdapat tumpukan kayu/seng pembatas yang menutup akses langsung ke rumah Bu Siti, memaksa Vicky mencari jalan tikus memutar di gang utara.
* **Reward Logistik**:
  - Setelah menyelesaikan laporan dan berbicara dengan Ophelia di Posko pada sore hari, Vicky mendapatkan hadiah **`basic_weapon`** (tongkat kayu / pipa penyangga posko yang memberikan bonus serangan +5 damage di pertempuran turn-based).

---

### Level 3 (Hari 3) — "Intimidasi Preman & Boss Mr. Deddy"
* **Nuansa Cerita**: Bantuan yang mulai dirasakan manfaatnya oleh warga membuat kelompok preman lokal gerah. Mereka memblokir akses gang secara terencana untuk memeras paket sembako.
* **Tujuan Desain**: Mengintegrasikan sistem *Kepercayaan Warga (Trust)* sebagai kunci pembuka jalan, serta menguji pemain dalam pertarungan bos pertama (*Mid-Boss Battle*).
* **Parameter Level**:
  - `initialTimeSeconds`: **170 detik**.
  - `initialPackages`: **3 paket**.
  - `isBoss`: **True** (Encounter Mr. Deddy).
* **Mekanik Khusus 1 — Pemanfaatan Bantuan Warga (Citizen Help)**:
  - Jalan menuju warga kedua diblokir oleh anak buah preman yang menolak bergeser.
  - Pemain harus menyerahkan paket pertama kepada warga terdekat terlebih dahulu untuk memperoleh **`Trust >= 1`**.
  - Begitu nilai Trust tercapai, opsi **[3] Minta Bantuan Warga** aktif: warga akan berteriak dari jendela atau melempar perhatian anak buah preman, membuka blokade jalan secara damai tanpa battle.
* **Mekanik Khusus 2 — Pertarungan Boss (Mr. Deddy)**:
  - Di ujung gang menuju penerima ketiga, **Mr. Deddy** menghadang langsung.
  - Boss ini tidak dapat dihindari (pilihan kabur ditolak).
  - Menggunakan pola battle khusus (*Biasa $\rightarrow$ Persiapan $\rightarrow$ Serangan Kuat*).
  - Pemain disarankan telah memasang `basic_weapon` di menu RPG (`Esc` $\rightarrow$ Senjata) untuk mengalahkannya secara efisien.

---

### Level 4 (Hari 4) — "Waktu Kritis & Preman Patroli"
* **Nuansa Cerita**: Kondisi ekonomi semakin terjepit. Seorang lansia di ujung perkampungan sakit dan membutuhkan bantuan logistik darurat secepatnya, sementara anak buah preman mulai berpatroli aktif di persimpangan jalan.
* **Tujuan Desain**: Memperkenalkan dinamika *timing & stealth pacing* melalui rintangan bergerak (*moving hazard*), serta menuntut efisiensi pergerakan dengan timer yang ketat.
* **Parameter Level**:
  - `initialTimeSeconds`: **145 detik** (level paling cepat dan menegangkan).
  - `initialPackages`: **3 paket**.
* **Mekanik Khusus — Preman Patroli Bergerak (`MovingPatrol.cs`)**:
  - Preman tidak lagi berdiam diri di satu titik. Karakter preman berjalan bolak-balik di antara dua koordinat gang utama secara terus-menerus.
  - Radius deteksi preman ikut bergerak seiring posisinya.
  - Pemain harus memanfaatkan celah gang, dinding pembatas, atau menunggu sejenak hingga preman berbalik arah sebelum berlari menyeberang.
* **Pacing Pengantaran**:
  - Warga yang sakit diprioritaskan di lokasi terjauh, menuntut pemain pintar memanfaatkan sisa stamina lari (`Shift`).

---

### Level 5 (Hari 5) — "Puncak Krisis & Final Boss Mr. Gas Oke"
* **Nuansa Cerita**: Dalang di balik penimbunan dan kelangkaan sembako di perkampungan terungkap: **Mr. Gas Oke**, bos mafia distribusi elpiji dan pangan ilegal. Ia mendirikan barikade darurat di seluruh penjuru kampung untuk menggagalkan hari terakhir posko relawan.
* **Tujuan Desain**: Menghadirkan klimaks permainan dengan rute navigasi paling kompleks (mini-labirin) dan pertarungan turn-based paling sengit sebelum menyelesaikan seluruh rangkaian cerita.
* **Parameter Level**:
  - `initialTimeSeconds`: **150 detik**.
  - `initialPackages`: **3 paket**.
  - `isBoss`: **True** (Encounter Mr. Gas Oke).
* **Mekanik Khusus 1 — Tata Letak Labirin Barikade**:
  - Gang utama ditutup drum minyak dan seng gelombang, menciptakan rute bercabang yang membutuhkan ingatan navigasi pemain.
* **Mekanik Khusus 2 — Pertarungan Puncak Final Boss (Mr. Gas Oke)**:
  - Memiliki stat HP lebih tebal dan pola serangan menekan (*Persiapan $\rightarrow$ Serangan Kuat Semburan Gas $\rightarrow$ Serangan Biasa*).
  - Pemain harus membaca peringatan giliran: gunakan **Bertahan** saat tanda *Serangan Kuat* muncul, dan manfaatkan **`healing_item`** saat HP menipis.
* **Kondisi Kemenangan & Tamat**:
  - Mengalahkan Mr. Gas Oke dan menuntaskan 3 paket bansos akan membuka status **`DayPhase.Tamat`**.
  - Warga keluar ke gang untuk merayakan pulihnya pasokan pangan dan berterima kasih kepada Vicky dan tim posko.

---

## 4. Panduan Implementasi Teknis (Role A/B)

Pengembangan diferensiasi gameplay ini dirancang khusus agar dapat dikerjakan secara independen oleh **Gameplay Programmer (Role A/B)** tanpa perlu menunggu modeling atau tekstur baru dari Role C:

1. **Komponen `EnvironmentalHazard.cs`**:
   - Dibuat sebagai komponen MonoBehaviour dengan `BoxCollider` (`isTrigger = true`).
   - Menyimpan nilai `speedMultiplier = 0.5f`.
   - Pada `OnTriggerEnter(Collider other)`, jika objek bertag `"Player"`, set pengali kecepatan di `PlayerController`. Pada `OnTriggerExit`, kembalikan ke kecepatan normal.
2. **Komponen `MovingPatrol.cs`**:
   - Dipasang pada GameObject preman patroli (misal di Level 4).
   - Menggerakkan posisi lokal/dunia preman bolak-balik antara `pointA` dan `pointB` dengan `Vector3.MoveTowards` secara berkala.
3. **Inspector Scene (`Kampung_L*.unity`)**:
   - `initialTimeSeconds` diatur langsung pada komponen `PlayerInteraction` di setiap scene:
     - `Kampung.unity`: 180s
     - `Kampung_L2.unity`: 160s
     - `Kampung_L3.unity`: 170s
     - `Kampung_L4.unity`: 145s
     - `Kampung_L5.unity`: 150s
4. **Graybox Obstacles**:
   - Manfaatkan Unity primitive (Cube/Cylinder) dengan material abu-abu/kayu yang sudah ada untuk memblokir atau membuka jalur alternatif di masing-masing scene level.

