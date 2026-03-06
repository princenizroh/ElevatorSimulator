# K Project — Dokumentasi Teknis


---

## Daftar Isi

1. [Gambaran Proyek](#1-gambaran-proyek)
2. [Arsitektur Proyek](#2-arsitektur-proyek)
3. [Sistem Akun Pengguna](#3-sistem-akun-pengguna)
4. [Sistem Simpan & Muat](#4-sistem-simpan--muat)
5. [Sistem Koin & Inventori](#5-sistem-koin--inventori)
6. [Sistem Lokalisasi](#6-sistem-lokalisasi)
7. [Panduan Setup Scene](#7-panduan-setup-scene)
8. [Penyimpanan Data & Path File](#8-penyimpanan-data--path-file)
9. [Cara Build](#9-cara-build)

---

## 1. Gambaran Proyek

Proyek game 2D platformer berbasis Unity yang mengimplementasikan tiga sistem gameplay mandiri sesuai kebutuhan technical test:

| Sistem | Deskripsi |
|---|---|
| **Sistem Akun Pengguna** | Manajemen multi-akun: buat akun, login, logout, dan hapus akun |
| **Sistem Simpan & Muat** | Slot simpan per-pengguna (maks 3), simpan/muat manual, auto-save saat keluar & ganti scene |
| **Sistem Lokalisasi** | Pergantian bahasa saat runtime untuk 5 bahasa (EN, RU, ZH-S, ZH-T, JA) |

---

## 2. Arsitektur Proyek

### Struktur Folder

```
Assets/Scripts/
├── _Core/
│   ├── Constants/      GameConstants.cs          — centralized string/int constants
│   ├── Interfaces/     ISaveable.cs               — contract for saveable objects
│   └── Patterns/       Singleton.cs               — generic DontDestroyOnLoad singleton
│
├── UserAccountSystem/
│   ├── Core/           UserAccountManager.cs
│   ├── Data/           UserAccountData.cs
│   └── UI/             UserAccountUI.cs
│
├── SaveSystem/
│   ├── Core/           SaveManager.cs, SaveSerializer.cs
│   ├── Data/           SaveData.cs, SaveSlotInfo.cs, SaveResult.cs
│   └── UI/             SaveSlotUI.cs
│
├── CoinSystem/
│   ├── Core/           CoinManager.cs, Coin.cs
│   └── UI/             CoinUI.cs
│
├── InventorySystem/
│   ├── Core/           InventoryManager.cs, Item.cs
│   └── UI/             InventoryUI.cs
│
└── LocalizationSystem/
    ├── Core/           LocalizationManager.cs
    ├── Data/           LanguageCode.cs, LocalizationFontConfig.cs (ScriptableObject)
    ├── UI/             LocalizedText.cs, LanguageSelectorUI.cs
    └── Editor/         LocalizedTextEditor.cs
```

### Prinsip Desain Utama

- **Singleton pattern** — semua manager menggunakan `Singleton<T>` dengan `DontDestroyOnLoad`. Instance duplikat langsung dihancurkan.
- **Observer pattern** — komunikasi antar sistem dilakukan via `static event Action<T>`. Tidak ada panggilan langsung antar manager.
- **Pemisahan tanggung jawab** — script UI hanya bereaksi terhadap event; manager tidak pernah menyentuh UI.
- **Tidak ada `FindObjectOfType`** — semua referensi menggunakan `Instance` (singleton) atau dihubungkan via `[SerializeField]`.

### Diagram Alur Event

```
UserAccountManager.OnUserChanged
    ├── SaveManager        → reset slot, muat ulang info slot
    ├── SaveSlotUI         → perbarui label slot
    └── UserAccountUI      → tampilkan/sembunyikan tombol Play & Logout

CoinManager.OnCoinsChanged
    └── CoinUI             → perbarui teks jumlah koin

InventoryManager.OnItemsChanged
    └── InventoryUI        → perbarui grid slot item

LocalizationManager.OnLanguageChanged
    └── LocalizedText (×N) → perbarui teks + font tiap komponen
```

---

## 3. Sistem Akun Pengguna

### Cara Kerja

Semua data akun disimpan dalam satu file binary (`users.dat`) di `Application.persistentDataPath`.  
Saat game dimulai, `UserAccountManager` memuat file ini ke dalam `List<UserAccountData>` di memori.

#### Field UserAccountData

| Field | Tipe | Keterangan |
|---|---|---|
| `username` | `string` | Identitas unik akun |
| `password` | `string` | Kata sandi (plain-text) |
| `createdAt` | `DateTime` | Waktu pembuatan akun |

#### UserAccountManager — API Publik

| Method | Return | Keterangan |
|---|---|---|
| `CreateAccount(username, password)` | `bool` | Buat akun jika username belum ada |
| `SwitchAccount(username, password)` | `bool` | Login; memicu `OnUserChanged` |
| `DeleteAccount(username)` | `bool` | Hapus akun + semua file save miliknya |
| `Logout()` | `void` | Set `ActiveUser = null`; memicu `OnUserChanged(null)` |
| `GetAllAccounts()` | `List<UserAccountData>` | Mengembalikan salinan semua akun |
| `GetActiveUserSavePath()` | `string` | Mengembalikan path folder save pengguna aktif |

#### Alur UI (UserAccountUI.cs)

```
Main Menu
│
├── Tombol [Daftar Akun]
│     └── Tampilkan panel daftar akun yang sudah ada
│           └── Klik baris akun → buka panel Login
│
├── Tombol [Buat Akun]
│     └── Buka panel pembuatan akun
│           └── Username + Password (min 8 karakter) → konfirmasi → kembali ke daftar
│
├── Tombol [Login] (shortcut langsung)
│     └── Buka panel login langsung
│
└── Setelah login berhasil:
      ├── Tombol [Play] muncul → muat scene Gameplay, buka SaveSlotUI
      └── Tombol [Logout] muncul → memanggil Logout(), sembunyikan kedua tombol
```

Field password menggunakan `TMP_InputField.ContentType.Password` (input bertopeng).  
Semua teks feedback sudah dilokalisasi via `LocalizationManager.Instance.GetText("key")`.

---

## 4. Sistem Simpan & Muat

### Struktur File Save

Setiap pengguna memiliki folder tersendiri. File save disimpan dalam format binary (BinaryFormatter).

```
Application.persistentDataPath/
└── users/
    └── {username}/
        └── saves/
            ├── slot_0.sav
            ├── slot_1.sav
            └── slot_2.sav
```

#### Field SaveData

| Field | Tipe | Keterangan |
|---|---|---|
| `coins` | `int` | Jumlah koin saat disimpan |
| `inventoryItemNames` | `List<string>` | Nama resource item yang dipegang |
| `collectedCoinIds` | `List<string>` | ID semua koin yang pernah diambil |
| `collectedItemIds` | `List<string>` | ID semua item yang pernah diambil |
| `savedAt` | `DateTime` | Waktu simpan, ditampilkan di UI slot |

ID koin/item dibuat dengan format `"{sceneName}_{x:F1}_{y:F1}"` — posisi dunia menjadikannya unik. Ini mencegah koin dan item muncul kembali setelah memuat save.

#### Enum SaveResult

| Nilai | Arti |
|---|---|
| `Success` | Operasi berhasil |
| `InvalidSlot` | Index slot di luar jangkauan |
| `NoActiveUser` | Tidak ada pengguna yang login |
| `SlotEmpty` | Mencoba muat/hapus slot yang kosong |
| `FileCorrupted` | Deserialisasi gagal (file rusak) |
| `WriteFailed` | Gagal menulis ke disk |

#### SaveManager — API Publik

| Method | Return | Keterangan |
|---|---|---|
| `SetActiveSlot(int)` | `void` | Menandai slot mana yang sedang aktif |
| `SaveToSlot(int)` | `SaveResult` | Simpan manual ke slot tertentu |
| `LoadFromSlot(int)` | `SaveResult` | Muat slot dan terapkan data ke game |
| `DeleteSlot(int)` | `SaveResult` | Hapus data slot |
| `AutoSave()` | `SaveResult` | Simpan ke `ActiveSlotIndex` |
| `HasAnySave()` | `bool` | True jika minimal satu slot berisi data |
| `RefreshSlotInfo()` | `void` | Muat ulang metadata slot dari disk |

#### Pemicu Auto-Save

| Pemicu | Perilaku |
|---|---|
| `OnApplicationQuit` | Memanggil `AutoSave()` jika `ActiveSlotIndex >= 0` |
| Pindah scene dari Gameplay | Memanggil `AutoSave()` via `SceneManager.activeSceneChanged` |

#### Muat Tertunda (`_pendingData`)

Ketika memuat slot dari Main Menu sebelum scene Gameplay dimuat, `CoinManager` belum ada. `SaveManager` menyimpan `SaveData` ke `_pendingData`. Saat `HandleSceneChanged` mendeteksi scene Gameplay telah dimasuki, data diterapkan secara otomatis via `ApplyData(_pendingData)`.

#### Alur UI (SaveSlotUI.cs)

```
SaveSlotPanel (di Main Menu, aktif setelah menekan Play)
│
├── Tombol slot (Kosong)   → SetActiveSlot(i) → muat scene Gameplay (game baru)
│
└── Tombol slot (Ada data)
      └── Buka popup
            ├── [Muat]   → LoadFromSlot(i) → muat scene Gameplay
            └── [Hapus]  → DeleteSlot(i) → perbarui UI, tampilkan feedback
```

Label slot menampilkan `"Slot N"` + teks `"Kosong"` (dilokalisasi) atau tanggal `dd/MM/yyyy HH:mm`.

---

## 5. Sistem Koin & Inventori

### CoinManager

- Melacak total koin dan `HashSet<string> _collectedCoinIds`.
- `AddCoins(int amount, string coinId)` — menambah koin dan mencatat ID-nya.
- `IsCollected(string id)` — mengembalikan true jika koin sudah pernah diambil.
- Memicu `static event Action<int> OnCoinsChanged` setiap ada perubahan.

### Coin.cs (objek pickup)

1. Di `Awake`: membuat `_id = "{sceneName}_{x:F1}_{y:F1}"`.
2. Di `Start`: jika `CoinManager.IsCollected(_id)` → `Destroy(gameObject)` langsung (tidak muncul lagi).
3. Saat trigger masuk dengan Player: memanggil `AddCoins(coinValue, _id)` → hancurkan diri sendiri.

### InventoryManager

- Melacak `List<ItemData>` dan `HashSet<string> _collectedItemIds`.
- `AddItem(ItemData, string itemId)` — menambah item dan mencatat ID-nya.
- `IsCollected(string id)` — mengembalikan true jika item sudah pernah diambil.
- Memicu `static event Action<List<ItemData>> OnItemsChanged` setiap ada perubahan.

### Item.cs (objek pickup)

Pola sama seperti Coin.cs — pencegahan respawn berbasis ID, memicu `AddItem` saat diambil.

### CoinUI.cs

Subscribe ke `CoinManager.OnCoinsChanged` → memperbarui `TextMeshProUGUI` dengan jumlah koin terkini.

### InventoryUI.cs

- Subscribe ke `InventoryManager.OnItemsChanged` → membangun ulang grid slot item.
- Membaca `PlayerInputActions` untuk toggle panel inventori dengan tombol **E**.
- Panel tersembunyi secara default.

### ItemData (ScriptableObject)

Berada di `Resources/Game Data/Item/`. Dimuat saat runtime via `Resources.Load<ItemData>("Game Data/Item/{name}")` ketika memulihkan data save.

---

## 6. Sistem Lokalisasi

### Bahasa yang Didukung

| Kode | Bahasa | File JSON |
|---|---|---|
| `EN` | Inggris | `en.json` |
| `RU` | Rusia | `ru.json` |
| `ZH_S` | Mandarin Sederhana | `zh-s.json` |
| `ZH_T` | Mandarin Tradisional | `zh-t.json` |
| `JA` | Jepang | `ja.json` |

### Format JSON

Semua file berada di `Assets/StreamingAssets/Localization/`. Formatnya:

```json
{
  "key_name": "Translated text here",
  "err_username_empty": "Username cannot be empty.",
  "slot_empty": "Empty"
}
```

### LocalizationManager

- Memuat JSON via `UnityWebRequest` (diperlukan untuk `StreamingAssets` di semua platform).
- Menyimpan semua pasangan key-value di `Dictionary<string, string> _entries`.
- Preferensi bahasa disimpan sebagai file binary di `persistentDataPath/language.dat`.
- `GetText(string key)` — mengembalikan string terjemahan, atau `[key]` jika kunci tidak ditemukan.
- `GetCurrentFont()` — mengembalikan `TMP_FontAsset` untuk bahasa aktif via `LocalizationFontConfig`.
- `SetLanguage(LanguageCode)` — mengganti bahasa saat runtime, muat ulang JSON, memicu `OnLanguageChanged`.

### LocalizedText.cs (komponen)

Pasang pada objek `TextMeshProUGUI` manapun.

1. Set field `key` di Inspector (dropdown menampilkan semua kunci dari `en.json`).
2. Di `OnEnable`: subscribe ke `OnLanguageChanged`, langsung memanggil `UpdateText()`.
3. `UpdateText()` mengatur `.text = GetText(key)` dan `.font = GetCurrentFont()`.

### LocalizationFontConfig (ScriptableObject)

Memetakan setiap `LanguageCode` ke `TMP_FontAsset`. Ditetapkan di field Inspector milik `LocalizationManager`.

Buat via: **Klik kanan di Project → Create → KProject → Localization Font Config**

> **Font CJK:** Bahasa Mandarin dan Jepang memerlukan Font Asset yang dibuat dengan Atlas Resolution 4096×4096 dan rentang karakter `3040-30FF, 3400-4DBF, 4E00-9FFF, F900-FAFF, FF00-FFEF` via **Window → TextMeshPro → Font Asset Creator**.

### LanguageSelectorUI.cs

Tetapkan 5 tombol secara berurutan (EN, RU, ZH-S, ZH-T, JA). Setiap tombol memanggil `LocalizationManager.Instance.SetLanguage(_order[i])`.

### LocalizedTextEditor.cs (Custom Editor)

Di Inspector komponen `LocalizedText`, field `key` menampilkan dropdown semua kunci yang diparsing dari `en.json`. Tombol **Refresh Keys** membaca ulang file. Ini mencegah typo pada nama kunci.

### Daftar Lengkap Kunci Lokalisasi

| Kunci | Penggunaan |
|---|---|
| `select_save` | Judul panel slot simpan |
| `slot_empty` | Label slot kosong |
| `back` | Tombol kembali |
| `load` | Tombol muat |
| `delete` | Tombol hapus |
| `cancel` | Tombol batal |
| `list_account` | Tombol buka daftar akun |
| `create_account` | Judul panel buat akun |
| `login_account` | Judul panel login |
| `username` | Label field username |
| `password` | Label field password |
| `confirm` | Tombol konfirmasi |
| `logout` | Tombol logout |
| `play` | Tombol bermain |
| `open_inventory` | Judul UI inventori |
| `press_e` | Petunjuk "Tekan E" |
| `coin_label` | Label penghitung koin |
| `err_username_empty` | Validasi: username kosong |
| `err_password_empty` | Validasi: password kosong |
| `err_password_short` | Validasi: password < 8 karakter |
| `err_username_exists` | Validasi: username sudah digunakan |
| `err_wrong_credentials` | Login gagal (username/password salah) |
| `err_save_corrupted` | File save rusak |
| `err_save_no_user` | Tidak ada pengguna aktif saat muat |
| `err_save_failed` | Gagal simpan/hapus (umum) |
| `err_load_failed` | Gagal muat (umum) |
| `slot_deleted` | Feedback hapus slot berhasil |

---

## 7. Panduan Setup Scene

### Scene Main Menu

GameObject yang diperlukan:

| GameObject | Komponen |
|---|---|
| `UserAccountManager` | `UserAccountManager.cs` |
| `SaveManager` | `SaveManager.cs` |
| `LocalizationManager` | `LocalizationManager.cs` + assign `LocalizationFontConfig` |
| `UserAccountUI` | `UserAccountUI.cs` + semua referensi UI dihubungkan |
| `SaveSlotUI` | `SaveSlotUI.cs` + tombol slot + panel popup |
| `LanguageSelectorUI` | `LanguageSelectorUI.cs` + 5 tombol bahasa |

Ketiga prefab manager menggunakan `DontDestroyOnLoad` — otomatis ikut berpindah ke scene Gameplay (jangan tambahkan kembali ke scene Gameplay).

### Scene Gameplay

GameObject yang diperlukan:

| GameObject | Komponen |
|---|---|
| `CoinManager` | `CoinManager.cs` |
| `InventoryManager` | `InventoryManager.cs` |
| `CoinUI` | `CoinUI.cs` + referensi `TextMeshProUGUI` |
| `InventoryUI` | `InventoryUI.cs` + panel, container, item prefab, InputActions |
| Koin di level | `Coin.cs` pada setiap objek koin |
| Item di level | `Item.cs` + referensi `ItemData` pada setiap objek item |

---

## 8. Penyimpanan Data & Path File

Semua data disimpan di bawah `Application.persistentDataPath` (tidak ada Registry, tidak ada PlayerPrefs).

| File | Path | Format | Isi |
|---|---|---|---|
| Semua akun | `persistentDataPath/users.dat` | Binary | `List<UserAccountData>` |
| Slot simpan | `persistentDataPath/users/{username}/saves/slot_{n}.sav` | Binary | `SaveData` |
| Preferensi bahasa | `persistentDataPath/language.dat` | Binary | `int` (nilai enum LanguageCode) |
| JSON lokalisasi | `StreamingAssets/Localization/*.json` | JSON | Pasangan key-value string |

---

## 9. Cara Build

1. Buka **File → Build Settings**.
2. Tambahkan scene secara berurutan: `Scenes/MainMenu`, `Scenes/Gameplay`.
3. Atur **Target Platform** ke `Windows`.
4. Klik **Build** — pilih folder output.
5. Folder `StreamingAssets/Localization/` otomatis disalin ke dalam build oleh Unity.

> Setelah build, file `language.dat` dan `users.dat` dibuat saat pertama kali game dijalankan di:  
> `C:\Users\{user}\AppData\LocalLow\{NamaPerusahaan}\{NamaProyek}\`
