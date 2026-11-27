# Review dan Handoff Kerja untuk Rafif

Tanggal: 2026-11-27. Reviewer: Budi (BUD), dibantu C-BUD (Claude Code, Claude Opus 5.5) dan tiga
reviewer independen. Cakupan: semua PR minggu ini, yaitu #3, #5, #6, #7 dan #8.

Dokumen ini memuat empat hal:
1. Apa yang sudah masuk `main` dan alasannya.
2. Apa yang ditahan dan alasannya.
3. Daftar perbaikan per PR, lengkap dengan lokasi file dan cara memperbaikinya.
4. Urutan kerja berikutnya.

Semua temuan HIGH sudah dibuktikan ulang oleh Budi, tidak diambil mentah dari reviewer.

---

## 1. Ringkasan keputusan

| PR | Isi | Keputusan | Hasil |
|---|---|---|---|
| #3 | Intake note Project Fighter | **Masuk `main`** lewat cherry-pick, isi tidak diubah | #9 → `2788be3` |
| #5 | Skeleton Unity 6.3 LTS (APP-001) | **Masuk `main`** lewat cherry-pick + 1 perbaikan | #10 → `ec2b1bb` |
| #6 | `Fix` Q48.16, `SeededRandom`, `StateHasher` (APP-003) | **Masuk `main`** lewat cherry-pick, isi tidak diubah | #11 → `a47ccb6` |
| #7 | Simulasi pertarungan + data starter | **Sebagian masuk.** Fungsi tambahan `Fix` dan 8 test-nya ikut di #11; simulasinya **ditahan** | — |
| #8 | Menu, training grey box, HUD, input, audio UI, string | **Ditahan seluruhnya.** Dikerjakan ulang per card | — |

Kenapa lewat cherry-pick, bukan merge PR asli:
- Semua commit di PR asli memakai alamat Gmail pribadimu sebagai author, padahal repo ini public.
  Kalau di-merge langsung, alamat itu masuk permanen ke riwayat `main`.
- Hasil cherry-pick tetap atas namamu (`rafif-glitch`, sebagai co-author di commit squash), tapi
  memakai alamat noreply GitHub: `318546048+rafif-glitch@users.noreply.github.com`.
- Lihat bagian 6, poin 1, untuk mengatur ini di laptopmu.

PR #3, #5, #6, #7 dan #8 **tidak ditutup**. #3, #5 dan #6 sudah tidak diperlukan lagi, jadi kamu
boleh menutupnya sendiri. #7 dan #8 adalah bahan untuk pekerjaan berikutnya (bagian 5).

---

## 2. Bukti verifikasi

Semua test dijalankan Budi dengan Unity 6000.3.29f1 headless di macOS, di worktree yang bersih per
branch. Versi ini beda satu patch dari 28f1 yang kamu pakai.

| Yang dites | EditMode (test RFL) | PlayMode (test RFL) | Error / warning compiler |
|---|---|---|---|
| PR #5 asli | compile ok (belum ada test RFL) | — | 0 / 0 |
| PR #6 asli (di atas #5) | 26 lulus, 0 gagal | — | 0 / 0 |
| PR #7 asli (di atas #6) | 70 lulus, 0 gagal | — | 0 / 0 |
| PR #8 asli (seluruh tumpukan) | 84 lulus, 0 gagal | 5 lulus, 0 gagal | 0 / 0 |
| Branch cherry-pick APP-001 (#10) | compile ok | — | 0 / 0 |
| Branch cherry-pick APP-003 (#11) | 34 lulus, 0 gagal (26 + 8 test `Fix`) | — | 0 / 0 |
| **`main` setelah merge (`a47ccb6`)** | **34 lulus, 0 gagal** (FixTests 23, SeededRandomTests 7, StateHasherTests 4) | — | 0 / 0 |

Catatan bukti:
- Jumlah test yang kamu tulis di setiap PR (26 / 70 / 84 / 5) **cocok persis** dengan hasil di atas.
  Klaimmu jujur, bagus.
- Test bawaan paket (Addressables, Input System) tidak dihitung.
- `bash tools/watchdog.sh` lulus di semua branch dan di `main`.
- `csharpier check Assets/RFL` bersih.
- `Assets/RFL/Sim` tidak memakai `float`, `double`, API Unity, `System.Random`, `DateTime`,
  `Dictionary` maupun LINQ.
- CI `watchdog` hijau di #9, #10 dan #11.
- Tidak ada alamat Gmail di riwayat `main` (`git log origin/main` → 0).
- Pemeriksaan keamanan seluruh tumpukan (25.476 baris diff) tidak menemukan secret, token, path
  lokal, maupun registry paket di luar Unity. Tidak ada aset biner, jadi aturan provenance belum
  berlaku.

Yang **belum** diverifikasi:
- Test di Windows (menunggu APP-002).
- `tools/run-tests.sh`, karena file ini belum ada.
- Test di Unity 28f1 persis.

---

## 3. Apa yang berubah dari PR aslimu

### #10 (APP-001): satu perbaikan di `ProjectSettings/EditorBuildSettings.asset`

- `m_Scenes` dan `m_configObjects` sekarang kosong.
- Alasannya, file aslinya menunjuk ke `Menu.unity`, `GreyBox.unity` dan setting Addressables/
  Localization yang baru ada di #8.
- Satu GUID input-actions (`052faaac…`) **tidak ada di file mana pun** di seluruh tumpukan.
- Akibatnya #5 tidak bisa berdiri sendiri. Saat lapisan game masuk nanti, tambahkan lagi entri yang
  memang dipakai, dan jangan bawa GUID input-actions yang tidak ada filenya.

### #11 (APP-003): isi #6 tidak diubah, ditambah dari #7

- Ditambah perubahan `Fix.cs` dan `FixTests.cs` dari #7, yaitu `FromRatio`, `FromMilli`, `Abs`,
  `Clamp`, `Fix*int`, `Fix/int`, `RoundToInt` dan `ToMilli`, beserta 8 test.
- Saat nanti mengerjakan ulang #7, rebase ke `main`. Bagian `Fix` ini sudah ada di sana, jadi
  diff-mu akan mengecil.

---

## 4. Review rinci per PR

Tingkat keparahan mengikuti aturan repo:
- **HIGH / CRITICAL** memblokir merge.
- **MEDIUM** wajib diperbaiki atau diputuskan sebelum card ditutup.
- **MINOR** masuk backlog dan tidak memblokir.

### PR #3: intake note (sudah masuk)

Catatannya berguna dan klaimnya sesuai isi PR lain. Ada satu MEDIUM dan beberapa MINOR.

| # | Sev | Lokasi | Masalah | Perbaikan |
|---|---|---|---|---|
| 3.1 | MEDIUM | `project-fighter-intake.md:64-68, 75-78, 82, 109, 111` | Catatan ini mengusulkan merge #7/#8 "as built" lalu baru memperbaiki nilai dan scope. Itu berarti tuning literal masuk ke C# (melanggar SDLC rule 14) dan ada perilaku yang tidak disebut card (melanggar rule 9). SDLC.md:6-7 mensyaratkan waiver tertulis dari Rafif dan Budi untuk itu. | Jangan minta waiver. Kerjakan ulang per card (bagian 5). Kalau memang butuh waiver, ajukan sebagai keputusan eksplisit `DN-nnn` di `docs/decisions/decision-log.md`. |
| 3.2 | MINOR | `:35` | Baris APP-001 menyebut scenes dan `ProjectSetup.cs`, padahal keduanya ada di #8. | Pindahkan ke baris APP-010/011. |
| 3.3 | MINOR | `:28, 37, 92` | `SeededRandom` disebut "ported", padahal kode baru (PCG32). | Tulis "`SeededRandom` ditulis baru". |
| 3.4 | MINOR | `:82-85` | Urutan card tidak memuat APP-006, APP-008 dan STO-007. | Tambahkan, atau sebutkan card mana yang menanggungnya. |
| 3.5 | MINOR | `:49` | ART-006 adalah ring Malioboro underworld, bukan `RFL_ARN_LeagueRing`. | Sebutkan kalau itu hanya stand-in. |
| 3.6 | MINOR | penempatan | CR dan keputusan harus masuk `open-questions.md`, `changes/CR-nn.md` dan `decision-log.md`. Selain itu, `#5`–`#8` di file repo tidak otomatis menjadi link. | Tambahkan satu baris penutup tentang hal ini, dan pakai URL lengkap. |

### PR #5: skeleton Unity (sudah masuk, dengan perbaikan 5.1)

Paketnya bersih:
- Lock file cocok dengan manifest (56/40, 0 masalah) dan semua dari `packages.unity.com`.
- `RFL.Sim` memakai `noEngineReferences: true`.
- Asmdef test benar.
- Timestep 1/60, color space linear, HDR menyala.
- Bisa di-revert sendirian.

| # | Sev | Lokasi | Masalah | Perbaikan |
|---|---|---|---|---|
| 5.1 | MEDIUM | `EditorBuildSettings.asset:8-17` | Ada referensi ke aset milik #8 dan satu GUID yang tidak ada filenya. | **Sudah diperbaiki di #10.** |
| 5.2 | MEDIUM | APP-001 (`apps/TICKETS.md:15-23`) | Box yang belum terpenuhi: boot scene (Menu scene bukan boot scene), target frame rate 60 (kode #8 mengisi `-1` / `120`), dan cek Windows. | Buat `Assets/RFL/Scenes/Boot.unity` berisi Main Camera + Directional Light sebagai build scene 0. Tulis pertanyaan "60 vs 120/uncapped" di `docs/decisions/open-questions.md`. Box Windows menunggu APP-002. |
| 5.3 | MINOR | `RFL.Game.asmdef:7-9`, `RFL.Editor.asmdef:8-13`, `manifest.json:6` | Ada referensi Localization, Addressables, InputSystem dan UGUI yang belum dipakai kode. `Unity.Addressables` direferensikan langsung padahal hanya dependensi transitif. | Tambahkan `com.unity.addressables` (2.10.3) ke manifest saat dipakai. |
| 5.4 | MINOR | `QualitySettings.asset:32,86` | `vSyncCount: 0`, padahal CODING-STANDARD.md:95 meminta VSync menyala. | Ubah ke `1` di level PC. |
| 5.5 | MINOR | `.gitattributes` | Tidak ada `* text=auto` dan `*.sh text eol=lf`. Di Windows, `tools/*.sh` bisa berubah ke CRLF lalu bash gagal. Driver `unityyamlmerge` juga belum didokumentasikan. | Tambahkan aturannya, lalu dokumentasikan konfigurasi driver. |
| 5.6 | MINOR | `EditorSettings.asset:8` | `m_LineEndingsForNewScripts: 0` (OS-native) bertabrakan dengan `*.cs eol=lf`. | Ubah ke `1`. |
| 5.7 | MINOR | `EditorSettings.asset:29-30` | Domain reload dimatikan tanpa dokumentasi. | Tambahkan satu baris di CODING-STANDARD: setiap static di-reset di `SubsystemRegistration`. Ini sudah kamu lakukan dengan benar di #8. |
| 5.8 | MINOR | `ProjectSettings.asset:173-176` | Sisa template: app ID Android/iOS/tvOS, quality level Mobile, `SampleSceneProfile`. | Bersihkan di APP-002. |
| 5.9 | MINOR | `docs/STRUCTURE.md:29-30` | Kolom komentar bergeser satu spasi. | Tambahkan satu spasi. |
| 5.10 | MINOR | `.gitignore` | `Assets/InitTestScene*.unity` dari test runner tidak diabaikan. | Tambahkan `/[Aa]ssets/InitTestScene*.unity*`. |
| 5.11 | MINOR | — | Nullable belum diwajibkan di level proyek. | Opsional: file `.rsp` dengan `-nullable:enable`. |

### PR #6: `Fix`, `SeededRandom`, `StateHasher` (sudah masuk tanpa perubahan)

Pekerjaan terbaikmu minggu ini:
- PCG32 cocok dengan vektor referensi resmi (42/54 → `a15c02b7, 7b47f409, …`), dan golden value
  test-mu cocok dengan implementasi Python independen.
- `Range` memakai rejection yang unbiased.
- FNV-1a tidak bergantung pada byte order.

Semua temuannya MINOR:

| # | Sev | Lokasi | Masalah | Bukti | Perbaikan |
|---|---|---|---|---|---|
| 6.1 | MINOR | `Fix.cs:58` | Perkalian memakai raw 64-bit, jadi wrap di atas sekitar 46.341². Ini sudah terdokumentasi, tapi gagalnya diam-diam. | `50000*50000 = -1794967296.000` | Tambahkan test yang mengunci batas itu, dan opsional assert `#if DEBUG`. |
| 6.2 | MINOR | `Fix.cs:100` | `ToString` overflow untuk nilai sangat besar. | `FromRaw(long.MinValue)` → `0.000` | Pisahkan bagian bulat (`Raw >> 16`) dan pecahan (`(Raw & 0xFFFF) * 1000`). |
| 6.3 | MINOR | `Fix.cs:91` vs `:100` | `ToMilli` membulatkan half-up, sedangkan `ToString` half-away-from-zero. | `FromRaw(-4096)`: `-62` vs `-0.063` | Pilih satu aturan dan tambahkan kasus test ini. |
| 6.4 | MINOR | `Fix.cs:58` vs `:60` | Perkalian floor, pembagian truncate, sehingga `(-a)*b ≠ -(a*b)` selisih 1 raw. | `-1` vs `0` | Catat di CODING-STANDARD bahwa sim tidak boleh bergantung pada simetri tanda `Fix*Fix`. |
| 6.5 | MINOR | proses | "Done when" APP-003 meminta hijau di Mac **dan Windows**. | hanya Mac | Lampirkan hasil Windows sebelum card ditutup. |

### PR #7: simulasi pertarungan (ditahan, bagian `Fix` sudah masuk)

Mekanika determinisme sudah benar:
- Tidak ada float, Unity API, Dictionary, LINQ atau DateTime.
- `MatchState` berupa satu value type unmanaged, dan dijaga oleh test.
- Hash mencakup ke-17 field `MatchState` dan ke-42 field `RobotState`.

Tapi ada bug pertarungan yang **terbukti**, dan masalah proses.

| # | Sev | Lokasi | Masalah | Bukti | Perbaikan |
|---|---|---|---|---|---|
| 7.1 | **HIGH** | `Simulation.cs:107-113`, `Simulation.Combat.cs:49`, `:108` | **Trade selalu dimenangkan P1.** `Resolve(P1→P2)` mengisi `d.Move = MoveId.None` untuk P2. Lalu `Resolve(P2→P1)` membaca `_moves[(int)a.Move]`, yang sudah menjadi entri kosong. Hasilnya, hit P2 memberi 0 damage dan 0 hitstun, tapi tetap membatalkan move P1. | Keduanya jab di tick 0: `Hit side=1 Jab dmg=33`, `Hit side=2 move=None dmg=0`, HP akhir P1=1550, P2=1517. Dibuktikan ulang oleh Budi. | Ambil `MoveDef` kedua penyerang **sebelum** resolve apa pun, lalu teruskan ke `Resolve` sebagai parameter, jangan dibaca ulang dari `a.Move`. Tambahkan test: keduanya jab di tick yang sama, keduanya kehilangan 33 HP. |
| 7.2 | **HIGH** (proses) | seluruh PR | Empat card (APP-006/007/008/009) digabung dalam satu PR, tanpa card. Mekanik yang hilang tidak disebut: parry, sidestep, chord 3 f, imunitas groggy 3 s + pengali ×1.5, submission, throw, pool HP per part (kode memakai 30 % rata). APP-007 juga bergantung pada APP-005 yang belum ada. | `grep -rniE "parry\|sidestep\|throw\|submission\|chord\|immun" Assets/RFL/Sim` tidak menemukan mekaniknya. | Buat card "baseline sim port" yang menyebut persis apa yang masuk dan mendaftar setiap mekanik yang hilang serta nilai yang menyimpang sebagai card lanjutan. |
| 7.3 | MEDIUM | `Simulation.cs:94-98, 120-126`; `Simulation.Movement.cs:339`; `Simulation.Rounds.cs:71-74` | Dash yang diminta pada tick time-out terus berulang sepanjang RoundOver, karena `PendingDash` tidak pernah dibersihkan. Stamina yang terpakai terbawa ke ronde berikutnya. | 9 dash dari tick 22 sampai 150, stamina P1 1000 → 550. Dibuktikan ulang oleh Budi. | Bersihkan `PendingDash` dan `LinkQueued` di `Lock`, atau panggil `Recover` sebelum return di cabang timer. Tambahkan regression test dengan skrip yang sama. |
| 7.4 | MEDIUM | `Simulation.Movement.cs:104-110, 117-119, 129-131` | Stun berjalan satu frame lebih pendek dari nilai datanya, jadi frame advantage melenceng satu. Ini berpengaruh ke angka on-block di STO-002/003 dan ke panel frame data. | Jab on-hit +3 (seharusnya +4), on-block −4 (seharusnya −3). | Tetapkan konvensinya di `MoveDef.cs`, lalu perbaiki (mis. `Stun = m.Hitstun + 1`). Tambahkan test advantage untuk setiap move starter. |
| 7.5 | MEDIUM | konstanta di `Simulation*.cs`, `Data/StarterMoves.cs`, `StatusRules.cs` | Nilai tuning ada di C# dan berbeda dari card. Contoh: regen stamina 20 %/s vs 2,5 %/s; hit-stop 6 vs 4/8; block push > hit push; juggle rata 70 %; corner break 10 % vs 30 %; KO floor 25 % vs 40 %; wake-up invuln 20 f vs 6 f. | Melanggar CODING-STANDARD "Data and tuning". | Belum memblokir, karena APP-005 (importer) belum ada. Tapi card belum boleh ditutup. Catat setiap nilai sebagai baris backlog (card, file:line, nilai manual, nilai kode), lalu pindahkan ke `tuning.json` di APP-005. |
| 7.6 | MINOR | `MatchState.cs` | Belum ada `SeededRandom` di dalam `MatchState`. | — | Tambahkan `Rng`, isi seed-nya di `NewMatch`, dan masukkan ke hash, sebelum CPU dibuat. |
| 7.7 | MINOR | `SimulationTests.cs:320-376` | Test determinisme lemah: hanya dijalankan dua kali di proses yang sama, input `&3` saja, dan tanpa golden hash. | — | Pakai input `&15`, satu ronde penuh, dan sebuah konstanta golden hash. Konstanta ini juga dibutuhkan APP-004. |
| 7.8 | MINOR | `FightRulesTests.cs:518-556` | Test corner break menghitung ulang rumus produksi. | — | Ganti dengan kasus yang dihitung tangan (1550 HP di 550 → 650; KO → 387). |
| 7.9 | MINOR | `Simulation.Damage.cs:15`, `Data/StarterTypes.cs:14` | Ada array static yang bisa diubah, dan `StarterTypes.All` public. | — | Pakai `ReadOnlySpan<int>` atau `switch`, dan akses lewat `Get` saja. |
| 7.10 | MINOR | `Data/Strings.meta` | Ini meta untuk folder kosong yang tidak dilacak git. | — | Hapus. |
| 7.11 | MINOR | semua tipe Sim | Semua public, padahal standarnya hanya `MatchState`, `Fix`, tick dan input frame yang public. | — | Jadikan `internal` dan pakai `InternalsVisibleTo`. |
| 7.12 | MINOR | `MatchState.cs:44-64` | Event kelima dalam satu tick dibuang diam-diam. | — | Naikkan `MaxEvents` atau beri penghitung debug. |

### PR #8: lapisan game (ditahan seluruhnya)

Batas ke simulasi sudah benar:
- View hanya membaca `MatchState`.
- Ada accumulator 60 Hz dengan clamp 0,25 s, input di-latch per frame, dan tidak ada float yang
  masuk ke sim.

Tapi PR ini tidak bisa masuk utuh.

| # | Sev | Lokasi | Masalah | Perbaikan |
|---|---|---|---|---|
| 8.1 | **HIGH** | commit `02e5c9c` | Enam tiket digabung (APP-006, 010, 011, 012, 013, STO-007) tanpa card. Alasan "satu assembly" tidak berlaku, karena Strings, Profile dan Input bisa di-compile sendiri. | Buka card dulu, lalu ikuti rencana pemecahan di bagian 5. |
| 8.2 | **HIGH** | `MainMenu.uxml:7-8`, `MainMenuScreen.cs:18-27`, `VersusScreen.cs`, `DifficultyScreen.cs`, `RobotSelectScreen.cs` | Di luar scope MVP (SDLC rule 9): Versus jadi item pertama dan dibangun penuh (PvP, PvCPU tiga tingkat, pilih 3 robot, P2 hot-join), padahal README.md:24 dan apps/TICKETS.md:121 meminta Training pertama dan Versus berupa stub. Teks UI juga tidak jujur: `ui.difficulty.hard.desc` menulis "The CPU is fast, blocks and uses long combos", padahal CPU-nya `return default` dan diam saja (`GreyBoxController.cs:339`). Dibuktikan ulang oleh Budi. | Training jadi item pertama. Versus jadi stub nonaktif dengan teks "setelah MVP". Hapus layar Versus/Difficulty, string CPU dan P2 hot-join. Robot select dikunci ke satu robot (mirror match). |
| 8.3 | **HIGH** | `Game/DevTools/ShotRunner.cs:24-36`, `Game/Core/GameServices.cs:27` | ShotRunner ikut ke build pemain tanpa `#if`. Kalau game dijalankan dengan `-rfl-shots <dir>`, ia membuat folder, memindahkan `profile.json` keluar dari `persistentDataPath`, menulis PNG, menyuntikkan input lalu keluar. Tidak ada tiket yang menyebutnya. Dibuktikan ulang oleh Budi. | Pindahkan ke `RFL.Game.DevTools.asmdef` dengan `defineConstraints: ["DEVELOPMENT_BUILD \|\| UNITY_EDITOR"]` dan daftarkan lewat `[RuntimeInitializeOnLoadMethod]`, jangan dipanggil dari `GameServices`. Lalu buatkan card, atau buang. |
| 8.4 | MEDIUM | lihat kolom berikutnya | Tidak satu pun tiket memenuhi "Done when":<br>• APP-010: belum ada toggle ronde tak terbatas, input history, overlay hitbox, dan preset dummy. Dummy tidak pernah menyerang, jadi parry, sidestep dan counter-hit tidak bisa didemokan.<br>• APP-011: Training belum pertama, belum ada credits dan pengaturan shake/reduced-motion.<br>• APP-012: belum ada damage chip 400 ms.<br>• APP-013: belum ada variasi pitch dan whoosh.<br>• STO-007: kata prompt controller masih hard-code bahasa Inggris di `Glyphs.cs:12-80`. | Selesaikan, atau tulis card sebagai "fase 1" dengan checklist yang jujur. |
| 8.5 | MEDIUM | `GreyBoxController.cs:96-126` | Hook public `DevSet*` menulis `MatchState` langsung di kode yang ikut rilis. | Pakai `[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]`, atau `Simulation.DebugApply` yang dikeluarkan dari build rilis. |
| 8.6 | MEDIUM | `GreyBoxController.cs:514-535` | Pilihan corner break dan timer 10 s-nya ada di luar stream input. Akibatnya match tidak bisa direplay dari seed + input, dan P2 tidak pernah memilih. | Rekam pilihannya sebagai input atau field di `MatchState`. Pindahkan `CornerBreakSeconds` ke data tuning. |
| 8.7 | MEDIUM | `ProfileStore.cs:66-74` | Profil dengan skema lebih baru diturunkan versinya diam-diam lalu ditimpa. Dengan Steam Cloud, ini bisa membuat data pemain hilang. | Muat sebagai read-only dan jangan disimpan ulang. Tambahkan test-nya. |
| 8.8 | MEDIUM | `SettingsScreen.cs:86-90`, `MenuController.cs:107-112` | Kalau `Save()` melempar exception, menu macet setengah ter-pop. | Tangkap `IOException` dan `UnauthorizedAccessException`. Panggil `Exit()` sebelum stack diubah, atau pakai try/finally. |
| 8.9 | MEDIUM | `Editor/ProjectSetup.cs:74-83, 181, 241, 379-392` | `ProjectSetup.Run` bisa menghapus kerja di editor: `NewScene` tanpa menawarkan simpan, scene yang di-commit dibangun ulang, `DeleteAsset` tanpa syarat, dan `bundleVersion` di-reset ke 0.1.0. | Tawarkan simpan sebelum `NewScene`, hapus `CleanTemplate`, dan isi PlayerSettings hanya kalau masih default. |

MINOR #8, semuanya masuk backlog:
1. `ProfileStore.cs:41` hanya menangkap `IOException`.
2. Nilai profil tidak di-clamp. Ini memengaruhi `AudioListener.volume` dan resolusi.
3. `CsvTable.cs`:
   - Quote yang tidak ditutup menelan sisa file.
   - `"` di tengah field mengubah mode diam-diam.
   - Nomor baris salah kalau field berisi newline.
   - Belum ada test CSV.
4. Importer tidak mengecek bahwa placeholder `{n}` ID dan EN cocok.
5. 41 dari 146 baris punya teks ID = EN. Cek mana yang memang kata serapan.
6. `Loc.Get` dengan key yang hilang mengirim warning setiap frame. HUD juga memformat ulang label
   setiap frame, sehingga ada alokasi per frame.
7. Sekitar 300 baris kode art-prefab yang tidak terpakai di `RobotView.cs:88-420`.
8. Konstanta nama scene `"GreyBox"` dan `"Menu"` terduplikasi.
9. Ada `using` yang tidak terpakai di `NavFrame.cs` dan `DeviceKind.cs`.
10. `Until` di ShotRunner menyerah diam-diam setelah 30 s.
11. Belum ada penanganan controller yang dicabut saat fight.
12. Test:
    - Belum ada test paritas pad vs keyboard. Padahal ini "Done when" APP-006.
    - `Training_GamepadWalksInAndJabConnects` memakai 0,5 s waktu nyata, sehingga akan flaky di CI.
    - `IndonesianNumbersUseDotThousands` mengetes `CultureInfo` .NET, bukan kodemu.
    - 2 dari 5 test PlayMode mengunci alur Versus/CPU yang di luar scope.

---

## 5. Handoff: urutan kerja berikutnya

Kerjakan berurutan. Setiap langkah adalah satu card dan satu PR, dibuat dari `main` terbaru dengan
`git fetch origin` lalu `git worktree add ../wt-<branch> -b <branch> origin/main`.

**Sebelum mulai coding apa pun:**
- Lakukan bagian 6, poin 1 (email).
- Minta Budi membuat atau menyetujui card untuk setiap langkah di bawah. Aturannya: tidak ada card,
  tidak ada kerja.

| Urutan | Card / PR | Isi | Done when |
|---|---|---|---|
| 1 | APP-001 lanjutan | `Boot.unity` (kamera + lampu) sebagai build scene 0, `vSyncCount: 1`, perbaikan `.gitattributes` / line ending / `.gitignore` (5.4–5.6, 5.10). Tulis pertanyaan frame rate 60 vs 120 di `open-questions.md`. | Boot scene tampil, EditMode hijau, watchdog hijau. |
| 2 | APP-007 "baseline sim port" | Rebase #7 ke `main`. Perbaiki **7.1** (trade) dan **7.3** (dash), masing-masing dengan regression test. Putuskan konvensi stun (7.4) dan tambahkan test advantage. Card harus mendaftar mekanik yang belum ada dan nilai yang menyimpang (7.2, 7.5). | Test trade (keduanya −33 HP) dan test dash di time-out lulus, semua test lama tetap hijau, dan daftar penyimpangan masuk backlog. |
| 3 | STO-007 strings | Ambil dari `02e5c9c`: `ui_strings.csv`, `Editor/CsvTable.cs`, `StringRow.cs`, `StringTableImporter.cs`, `Game/UI/Loc.cs`, `LocLabel.cs`, `Localization/**`, `AddressableAssetsData/**` dan `StringTableTests.cs`. Inline `ProjectSetup.LocDir`, hapus baris Versus/CPU, tambahkan test CSV dan placeholder, dan pindahkan kata `Glyphs.cs` ke CSV. | Importer lulus, test CSV dan placeholder lulus. |
| 4 | Profile store (bagian APP-011) | `ProfileData.cs`, `SettingsData.cs`, `ProfileStore.cs` dan `ProfileStoreTests.cs`. Perbaiki 8.7 dan MINOR 1–2. | Ada test skema baru (read-only) dan test nilai di luar rentang. |
| 5 | APP-006 devices | `FightInput.cs`, `MenuInput.cs`, `NavFrame.cs` dan `DeviceKind.cs`. Gate atau hapus `Inject`, dan tambahkan test paritas pad vs keyboard. | Tombol logis yang sama di pad dan keyboard menghasilkan `InputFrame` yang sama. |
| 6 | APP-013 UI cues | `UiCue.cs`, `UiCues.cs` dan `AudioService.cs`. Lepaskan ketergantungan pada `GameServices`, dan tambahkan variasi pitch ±2 semitone. | Test cue unik lulus, dan pitch bervariasi. |
| 7 | APP-011 menu shell | Training pertama, Versus berupa stub, credits, pengaturan shake/reduced-motion. Hapus layar Versus/Difficulty, dan perbaiki 8.8 dan 8.9. | Box APP-011 tercentang semua. |
| 8 | APP-010 + APP-012 | Grey box dan HUD: perbaiki 8.5 dan 8.6, hapus kode art yang tidak terpakai, kunci ke satu robot, buat dummy yang bisa menyerang (agar parry dan sidestep bisa didemokan), tambahkan input history dan hitbox overlay, damage chip 400 ms, dan test walk berbasis tick, bukan waktu nyata. | "Every base mechanic demonstrable in one session." |
| — | ShotRunner, BuildScript | Tahan dulu sampai ada card. Kalau dipakai, pindahkan ke asmdef dev-only (8.3). BuildScript milik APP-015. | — |

Setiap PR wajib berisi:
- Baris card yang ditutup, dan box yang tercentang.
- Output test yang dikutip (EditMode/PlayMode beserta jumlahnya), watchdog, dan csharpier.
- Agent dan model yang dipakai.
- Daftar box yang **belum** tercentang, kalau ada.

Ukurannya kira-kira bisa dijelaskan dalam satu kalimat "apa yang diubah".

---

## 6. Kebiasaan yang perlu diperbaiki

1. **Email commit.** Di GitHub, buka *Settings → Emails* dan nyalakan *Keep my email addresses
   private*. Lalu di laptop:
   ```bash
   git config --global user.email "318546048+rafif-glitch@users.noreply.github.com"
   ```
   Repo ini public, jadi alamat pribadi di commit bisa dibaca siapa saja.
2. **Card dulu, baru kode.** Ketiga PR besar dibuka dengan tulisan "card pending". Aturan repo
   berbunyi *no card, no work*.
3. **Pecah sebelum menulis, bukan sesudahnya.** #7 berisi 4 tiket dan #8 berisi 6 tiket. Alasan
   "satu assembly / satu partial class" tidak menghalangi pemecahan. Rencanakan irisan yang bisa
   di-compile sendiri sejak awal.
4. **Port bukan alasan membawa scope.** Pekerjaan dari Project Fighter boleh dipakai ulang, tapi
   hanya bagian yang disebut card MVP. Fitur tambahan (Versus, CPU, 3 robot) ditulis sebagai usulan
   di `open-questions.md`, bukan langsung dimasukkan ke kode.
5. **Teks UI harus jujur.** Jangan menjanjikan perilaku yang belum ada ("blank beats wrong").
6. **Test perilaku, bukan implementasi.** Pakai angka yang dihitung tangan atau referensi eksternal,
   seperti yang sudah kamu lakukan untuk PCG32 dan `DamageFormulaWorkedExamples`. Jangan menghitung
   ulang rumus produksi.
7. **Kamu bertanggung jawab atas setiap baris yang ditulis agent.** Bug trade 7.1 adalah jenis bug
   yang mudah lolos kalau diff agent tidak dibaca dengan teliti. Sebelum membuka PR, coba jelaskan
   ke dirimu sendiri apa yang terjadi kalau dua hal terjadi di tick yang sama.

---

## 7. Yang sudah bagus

- Klaim di PR jujur: jumlah test cocok persis, box yang belum terpenuhi disebut terang-terangan, dan
  agent serta model yang dipakai dicantumkan.
- `Fix`, PCG32 dan FNV-1a benar sesuai buku teks, dengan golden value dari referensi independen.
- `MatchState` berupa satu value type unmanaged yang dijaga test, dan hash-nya mencakup setiap field
  yang memengaruhi masa depan.
- Simulasi patuh aturan determinisme: integer per-mille, `long` eksplisit, tidak ada float maupun
  tipe engine, dan ada test yang memindai token terlarang.
- Batas sim dan view rapi: accumulator 60 Hz, input di-latch per frame.
- Domain reload dimatikan dan setiap static di-reset di `SubsystemRegistration`. Ini jebakan halus,
  dan kamu menanganinya dengan benar.
- Profile store menulis ke file temp lalu `File.Replace`, menyimpan salinan `.bad`, dan aman dari
  pengaruh bahasa sistem.
- Localization dikerjakan dengan benar sejak hari pertama: setiap label adalah key, dan importer
  melaporkan semua masalah sekaligus.
- Tumpukan PR rapi: #6–#8 tidak pernah menyentuh file skeleton.
- Nama test terbaca seperti pernyataan desain ("JabHitsOnItsEighthFrame").
