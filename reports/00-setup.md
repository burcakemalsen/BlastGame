# Rapor: Adım 0, iskelet (kurulumcu)

Kurulumcu: Sonnet 5.5. Tarih: 2026-11-14. Tarif: `docs/tasks/00-setup.md`.

## Özet
Bitti tanımındaki 6 madde de sağlandı. Tek sorun APK adımında çıktı ve çözüldü (iş 8, ayrıca "Orchestrator'ın karar vermesi gerekenler"e bak).

## İşler

### 1. Unity projesi
- Komut: `Unity.exe -batchmode -quit -createProject C:\Dev\BlastGame -logFile ...` → çıkış 0. Klasör doluyken Unity kabul etti, `_unity_tmp` gerekmedi. Üretilen: `Assets/`, `Packages/`, `ProjectSettings/`, `BlastGame.slnx`, `Library/`.
- Ayarlar (geçici bir Editor scriptiyle batchmode'da, script sonra silindi): `EditorSettings.defaultBehaviorMode = Mode2D`, `PlayerSettings.productName = BlastGame`, `companyName = burcakemalsen`, Android paket adı `com.burcakemalsen.blastgame`, `defaultInterfaceOrientation = Portrait`, `Assets/Scenes/Main.unity` (boş sahne, build ayarlarında), etkin platform Android.
- Doğrulama (dosyalardan):
  - `ProjectSettings/EditorSettings.asset`: `m_DefaultBehaviorMode: 1` (1 = 2D)
  - `ProjectSettings/ProjectSettings.asset`: `productName: BlastGame`, `defaultScreenOrientation: 0` (0 = Portrait), `applicationIdentifier:` → `Android: com.burcakemalsen.blastgame`
  - `ProjectSettings/EditorBuildSettings.asset`: `path: Assets/Scenes/Main.unity`, `enabled: 1`
  - Etkin platform Android: `Library/EditorUserBuildSettings.asset` içinde `Android` var (ikili dosya; zayıf kanıt). Asıl kanıt, `-buildTarget Android` ile APK'nın üretilmesi (iş 8). Etkin platform `Library/` içinde durur, repoya girmez; temiz klonda Unity varsayılan platformla açılır, `scripts/build-android.ps1` `-buildTarget Android` verdiği için sorun olmaz.
- Not: `defaultBehaviorMode` yalnız yeni sahne/varlık varsayılanlarını etkiler. Oluşturulan `Main.unity` Unity'nin varsayılan `DefaultGameObjects` şablonundan (Main Camera + Directional Light) geldi; 2D kamera ayarı yapılmadı (oyun kodu/sahne işi sonraki adımın).

### 2. Klasörler ve asmdef
- `Assets/Scripts/Core/Core.asmdef`: `noEngineReferences: true`, `autoReferenced: true`.
- `Assets/Scripts/View/View.asmdef`: `references: ["Core"]`.
- `tests/`, `reports/`, `docs/specs/` var; `reports/.gitkeep`, `docs/specs/.gitkeep`, `Assets/Scripts/View/.gitkeep` eklendi.
- Unity log'unda uyarı: `Assembly for Assembly Definition File 'Assets/Scripts/View/View.asmdef' will not be compiled, because it has no scripts associated with it.` View'e ilk script gelince kaybolur; hata değil.
- Gerekçeli örnek dosya: `Assets/Scripts/Core/AssemblyMarker.cs` (`public static class AssemblyMarker { public const string Name = "BlastGame.Core"; }`, namespace `BlastGame.Core`). Neden: Core boşken `Core.csproj` derlenecek dosya bulamaz ve örnek test erişeceği bir tip bulamaz. Oyun kodu değil. İlk gerçek Core tipi gelince silinebilir (testi `tests/Core.Tests/SmokeTests.cs` buna bağlı, onunla birlikte silinir).

### 3. `.gitignore`
`docs/PLAN.md` bölüm 8'deki listenin tamamı yazıldı. `*.csproj`, `*.sln` yalnız köke uygulandı: `/*.csproj`, `/*.sln`, ayrıca `/*.slnx` (Unity 6 `.sln` yerine `.slnx` üretiyor; tarifte yoktu, aynı gerekçeyle eklendi). Ek sır kalıpları: `.env.*`, `*.pem`, `*.key`, `*.p12`, `*secret*`, `*token*`, `*credential*`. `.meta` dışlanmaz.
- Uyarı: `*token*` ve `*secret*` kalıpları adında "token" geçen meşru bir kaynak dosyayı (ör. `Tokenizer.cs`) de dışlar. Böyle bir dosya gerekirse kalıp daraltılır.

### 4. Test projeleri
- `tests/Core/Core.csproj`: `netstandard2.1`, `LangVersion 9.0`, `<Compile Include="../../Assets/Scripts/Core/**/*.cs" />` (kopya yok), `EnableDefaultCompileItems=false`.
- `tests/Core.Tests/Core.Tests.csproj`: `net10.0`, `LangVersion 9.0`, xUnit 2.*, Microsoft.NET.Test.Sdk 17.*, `Core`'a ProjectReference.
- `tests/BlastGame.Tests.slnx` (`dotnet new sln` .NET 10'da `.slnx` üretti).
- Komut: `dotnet test` (tests/ içinden) →
```
Core -> ...\tests\Core\bin\Debug\netstandard2.1\Core.dll
Core.Tests -> ...\tests\Core.Tests\bin\Debug\net10.0\Core.Tests.dll
Başarılı!  - Başarısız:     0, Başarılı:     1, Atlanan:     0, Toplam:     1, Süre: 363 ms - Core.Tests.dll (net10.0)
```
- Kökteki Unity'nin ürettiği `Core.csproj` ile `tests/Core/Core.csproj` ayrı dosyalar; `/Core.csproj` ignore'da, `tests/Core/Core.csproj` izleniyor (iş 11'de doğrulandı).

### 5. C# sürümü
Kanıt: Unity'nin ürettiği `C:\Dev\BlastGame\Core.csproj` (Assembly-CSharp-Editor.csproj'da da aynı):
```
Core.csproj:18:    <LangVersion>9.0</LangVersion>
Core.csproj:22:    <TargetFramework>netstandard2.1</TargetFramework>
```
`ProjectSettings/ProjectSettings.asset:720: apiCompatibilityLevel: 6` (6 = .NET Standard 2.1). `Core.csproj:27` `DefineConstants` `UNITY_6000_6_3;...`.
Sonuç: **C# 9.0, netstandard2.1**; test projesi buna sabitlendi (`tests/Core/Core.csproj`, `Core.Tests` de `LangVersion 9.0`).

Eksik dil özellikleri (geçici proje `netstandard2.1` + `LangVersion 9.0` ile denendi, `a.cs`):
```
public record R(int A);                       → error CS0518: ... 'System.Runtime.CompilerServices.IsExternalInit' ...
public class C { public int P { get; init; } } → aynı hata (CS0518)
```
- `record` ve `init` erişimcisi: `IsExternalInit` tipi netstandard2.1'de yok, derlenmez. Çözüm (karar ürün sahibi/orchestrator'ın): Core içine `namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} }` eklemek (Unity bunu kabul eder), ya da record/init kullanmamak.
- `System.Index`/`Range` (`a[^1]`, `a[1..]`) denendi, **derlendi** (netstandard2.1'de var).
- Denemediğim ama bilinen: C# 9 "covariant returns" çalışma zamanı desteği netstandard2.1'de yok; C# 10+ özellikleri (file-scoped namespace, global using, record struct) LangVersion 9'da kapalı. Bunları ayrıca derleyerek doğrulamadım.

### 6. `scripts/unity-check.ps1`
Not: makinede PowerShell script çalıştırma kapalı (`UnauthorizedAccess`); komutlar `-ExecutionPolicy Bypass` ile (yalnız o süreç için, sistem ayarı değişmedi).
- Temiz proje: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` →
```
Unity compile check: clean.
exit=0
```
- Bilerek hatalı `Assets/Scripts/Core/BrokenTemp.cs` (`return "not an int";`) →
```
COMPILE ERRORS (1):
  Assets\Scripts\Core\BrokenTemp.cs(1,92): error CS0029: Cannot implicitly convert type 'string' to 'int'
exit=1
```
Dosya silindi (`.meta` hiç oluşmamış, `ls` ile doğrulandı).

### 7. `scripts/prepush-scan.ps1`
Üç madde: (1) `.gitignore` `.env`, `release.keystore`, `my.jks`, `secrets.json`, `api_token.txt` yollarını `git check-ignore` ile kapsıyor mu; (2) staged diff'in eklenen satırlarında anahtar benzeri kalıp (AWS/GitHub/Google anahtarı, özel anahtar başlığı, `api_key/secret/token/password = <12+ karakter>`); (3) staged dosya adı ve eklenen satırlarda yerel terim listesindeki ifadeler (tam kelime, büyük/küçük harfe duyarlı) ve `.pdf/.zip/.docx` uzantısı. Dosya yoksa ya da liste boşsa çıkış 2 ve hata. Bulgu çıkış 1, dosya:satır yazar. Terimin kendisini çıktıya yazmaz (halka açık log'a sızmasın diye).
- Deneme: bloğun ilk teriminden 2. satıra yerleştirilmiş `scantest.txt` stage edildi →
```
[3] scan term found: scantest.txt:2
PRE-PUSH SCAN: FINDINGS. Do not push.
exit=1
```
`git reset` + dosya silindi. Tüm proje stage'liyken (112 dosya):
```
PRE-PUSH SCAN: clean.
exit=0
```
- Sınır: yalnız staged diff'in **eklenen** satırlarına bakar (silinenlere değil); ikili dosyaların içine bakmaz (yalnız adlarına). Anahtar kalıpları sezgiseldir, her sırrı yakalamaz.
- Not: scan-terms listesini yalnız script okur; ben de ilk terimi test için kabuk değişkenine alıp ekrana yazmadım.

### 8. APK
- `Assets/Editor/BuildScript.cs`: `BuildScript.BuildAndroid()`, `EditorBuildSettings.scenes` → `Builds/Android/BlastGame.apk`, sonuca göre `EditorApplication.Exit(0|1)`.
- `scripts/build-android.ps1`: `-buildTarget Android -executeMethod BuildScript.BuildAndroid`, hata/APK yoksa sıfır dışı çıkış.
- **1. deneme** (Bash aracı, bu oturumun sandbox'ı): `java.io.IOException: Unable to establish loopback connection` (Gradle daemon, Unity log satır ~2555). Aynı komut Terminal panelinde (sandbox dışı) bu hatayı vermedi → sandbox kaynaklı.
- **2. deneme** (Terminal paneli): `[CXX1429] ... C++ build system [prefab] failed ... Error: Could not find or load main class com.google.prefab.cli.AppKt` ve prefab yolu `C:\Users\<profil>\.gradle\caches\9.3.1\transforms\...`. Kök neden: kullanıcı profil klasöründe ASCII dışı harf var (log'da bozuk görünüyor), Gradle kullanıcı klasörü o yolda. PLAN'daki "Gradle ASCII dışı yol" riskinin proje yolu değil profil yolu hali.
- **Çözüm denemesi (tarif "çözüm uydurma" diyor; teşhisi doğrulamak için tek seferlik, proje içinde yaptım):** `GRADLE_USER_HOME=C:\Dev\BlastGame\Library\GradleHome` (ASCII, gitignore'lu `Library/` içinde) ile aynı script → `APK: C:\Dev\BlastGame\Builds\Android\BlastGame.apk  (13.82 MB)`.
- Sonra scripte şu koruma eklendi: `GRADLE_USER_HOME` tanımsızsa ve `USERPROFILE` ASCII dışıysa `Library\GradleHome` kullan. Env olmadan Terminal'de yeniden çalıştırıldı →
```
Non-ASCII user profile detected; GRADLE_USER_HOME = C:\Dev\BlastGame\Library\GradleHome
APK: C:\Dev\BlastGame\Builds\Android\BlastGame.apk  (13.82 MB)
```
- Bu koruma bir karardır: kabul etmezsen çıkar (aşağıda).
- Telefona yükleme ürün sahibinin işi. `Builds/` ve `*.apk` gitignore'da.

### 9. Dokümanlar
`README.md` (İngilizce; AI desteği, roller, kapı, komutlar; üçüncü taraf ad yok), `Backlog.md`, `CLAUDE.md` yalnız "Komutlar" bölümü (test, unity-check, APK, push öncesi tarama). README'deki test komutu `dotnet test tests/BlastGame.Tests.slnx`; bu ayrı ayrıca çalıştırılıp doğrulanmadı, ama `dotnet test` tests/ içinde yeşil çalıştı (aynı çözümü kullanıyor).

### 10. Gemini modelleri
Yol: `agy --help` → alt komut `models` ("List available models"); `agy models --help` flag göstermez. Komut: `agy models` (yalnız listeler, model çağırmaz). Çıktı (aynen):
```
gemini-4-argon-medium	Gemini 4 Argon (Medium)
gemini-4-argon-high	Gemini 4 Argon (High)
gemini-4-argon-low	Gemini 4 Argon (Low)
gemini-4-hydrogen-high	Gemini 4 Hydrogen (High)
gemini-4-hydrogen-xhigh	Gemini 4 Hydrogen (XHigh)
gemini-4-hydrogen-medium	Gemini 4 Hydrogen (Medium)
gemini-4-hydrogen-low	Gemini 4 Hydrogen (Low)
gemini-3.8-flash-high	Gemini 3.8 Flash (High)
gemini-3.8-flash-medium	Gemini 3.8 Flash (Medium)
gemini-3.8-flash-low	Gemini 3.8 Flash (Low)
claude-opus-5-5-low|medium|high	Claude Opus 5.5
claude-sonnet-5-5-low|medium|high	Claude Sonnet 5.5
gpt-oss-120b-medium	GPT-OSS 120B (Medium)
```
(Son üç satır kısaltıldı: Claude satırları tek satırda birleştirildi. Listede "Gemini Pro" adlı model yok; PLAN'daki "Pro" artık yok gibi, seçim ürün sahibiyle.) Model seçimi `--model <id>` bayrağıyla (`agy --help`).

### 11. Git
`git init -b main`. Git ayarına dokunulmadı (commit, mevcut genel kimlikle atıldı).
`git status --ignored --short` (ilgili kısım, aynen):
```
!! .vscode/
!! Assembly-CSharp-Editor.csproj
!! BlastGame.slnx
!! Builds/
!! Core.csproj
!! Library/
!! Logs/
!! UserSettings/
!! build/
!! tests/Core.Tests/bin/
!! tests/Core.Tests/obj/
!! tests/Core/bin/
!! tests/Core/obj/
```
`git ls-files tests` →
```
tests/BlastGame.Tests.slnx
tests/Core.Tests/Core.Tests.csproj
tests/Core.Tests/SmokeTests.cs
tests/Core/Core.csproj
```
`.meta` dosyaları izleniyor (10 adet; `git ls-files | grep -c '\.meta$'`). `git ls-files` içinde yerel dosyalar, `Library`, `.apk` yok. `git check-ignore -v` ile yerel dosyalar ve `Library/` ignore kurallarına bağlı, `tests/Core/Core.csproj` ve `.meta` bağlı değil.
Commit öncesi `scripts/prepush-scan.ps1` staged duruma karşı çalıştırıldı (rapor yazıldıktan sonra, son hâliyle): sonuç aşağıdaki "Commit" bölümünde.

## Yapamadıklarım
- Yok. (Telefonda açmak ürün sahibinin işi, yapılmadı.)

## Emin olmadıklarım
- Etkin platformun Android olduğunun doğrudan kanıtı yok (ikili dosya); APK üretimi dolaylı kanıt.
- `.meta` GUID'leri bu makinede üretildi; sorun beklenmez.
- `Main.unity` içeriği Unity şablonunun varsayılanı (3D ışık/kamera); 2D kamera ayarlaması yapmadım.
- `build/outputs` kökte neden oluştu bilmiyorum (muhtemelen Gradle/Unity; ignore'da).
- `.vscode/` klasörünü Unity (ide paketi) oluşturdu, ignore'da.
- Çıktıda Unity "Library/Bee" yolu ve Gradle sürümü (9.3.1) Unity'ye bağlı; Unity sürümü değişirse build ortamı yeniden doğrulanmalı.
- Proje yolu dışına yazdığım tek şey: `C:\Dev\create1.log` (Unity `-logFile` yolunu proje dışında verdim, kural ihlali). Silindi, içine bakılmış başka etkisi yok. Ayrıca NuGet paket önbelleği aracın kendi varsayılan yerine (kullanıcı klasörü) yazdı; Gradle önbelleği ilk denemelerde kullanıcı klasöründeydi, sonra proje içine alındı.

## Orchestrator'ın karar vermesi gerekenler
1. `build-android.ps1`'e eklenen `GRADLE_USER_HOME` koruması kalsın mı? Alternatif: kullanıcı profilini ASCII hale getirmek ya da env'i her seferinde elle vermek.
2. **Ajan oturumları sandbox içinde çalışıyor ve APK derlemesi orada Gradle loopback hatası veriyor.** APK adımı için ajan oturumundan Terminal paneli (sandbox dışı) kullanmak gerekiyor; `unity-check` ise sandbox'ta sorunsuz çalıştı. Bu, "ayrı oturum düzeni" için not.
3. `IsExternalInit` shim'i eklensin mi (record/init isteniyorsa).
4. `*token*`/`*secret*` .gitignore kalıpları çok geniş mi (bkz. iş 3).
5. Gemini modeli seçimi (iş 10 listesi; listede "Pro" yok).
6. `AssemblyMarker.cs` ve `SmokeTests.cs` Adım 1'de ne olacak (silinsin mi, kalsın mı).
7. `Main.unity`'nin 2D kameraya çevrilmesi hangi adımda.

## Commit
Commit öncesi `scripts/prepush-scan.ps1` staged duruma (113 dosya) karşı: `PRE-PUSH SCAN: clean.` çıkış 0. Staged listede `.zip/.pdf/.docx/.apk` ve yerel dosyalar yok (`git diff --staged --name-only | grep` boş). Sır dosyası yok. Push yapılmadı.

## Ek iş (orchestrator geri bildirimi)

### 1. `.utmp/` temizliği ve amend
`.gitignore`'a `/.utmp/` eklendi, `git rm -r --cached .utmp`, orchestrator'ın `docs/PLAN.md` ve `docs/RULES.md` güncel halleri (içeriklerine dokunmadan) ile birlikte `git commit --amend`. Mesaj: `chore: add Unity project skeleton, test setup and build scripts` (gövde ve Co-Authored-By kaldı). Amend, ürün sahibinin orchestrator üzerinden verdiği onayla yapıldı (henüz push edilmemişti).
```
d27a8bf chore: add Unity project skeleton, test setup and build scripts
utmp=0            (git ls-files | grep -c utmp)
```
Kökte izlenen dosyalar (`Assets/`, `ProjectSettings/`, `Packages/` hariç) listelendi: `.gitignore, Backlog.md, CLAUDE.md, README.md, docs/*, reports/*, scripts/*, tests/{BlastGame.Tests.slnx, Core.Tests/*, Core/Core.csproj}`. Üretilmiş dosya yok. Not: `.utmp/`'in kimin ürettiğini bilmiyorum (Android/CMake ara çıktısı, orchestrator'ın tespiti); ilk commit'te `git add -A` ile girmişti, benim kontrolümden kaçtı (`git status --ignored` çıktısında görmedim çünkü o an ignore'da değildi ve listeyi `grep` ile süzmüştüm).

### 2. `prepush-scan.ps1`: `-All` ve `-Range`
Üç mod: varsayılan staged, `-All` (HEAD'deki bütün izlenen dosyalar: ad + metin dosyalarının tüm satırları; içinde NUL baytı olan ikili dosyalarda yalnız ad), `-Range <rev-range>` (aralığın `git diff -U0` eklenen satırları + dosya adları). Üç madde aynı; bulunan terim çıktıya yazılmaz. `-All` ile `-Range` birlikte verilirse çıkış 2. Çıktı artık modu yazıyor.

Temiz durum (commit `caa9a68` üzerinde):
```
PRE-PUSH SCAN (staged changes): clean.                exit=0
PRE-PUSH SCAN (all tracked files in HEAD): clean.     exit=0
PRE-PUSH SCAN (range HEAD~1..HEAD): clean.            exit=0
```
Deneme dosyası `scantest.txt` (2. satırda listedeki ilk terim, 3. satırda `api_key` adlı değişkene atanmış 22 karakterlik sahte bir değer):
```
--- staged
[3] scan term found: scantest.txt:2
[2] key-like text: scantest.txt:3
PRE-PUSH SCAN (staged changes): FINDINGS. Do not push.     exit=1
--- range HEAD~1..HEAD   (geçici commit "tmp: scan test")
[3] scan term found: scantest.txt:2
[2] key-like text: scantest.txt:3
PRE-PUSH SCAN (range HEAD~1..HEAD): FINDINGS. Do not push.     exit=1
--- all
[3] scan term found: scantest.txt:2
[2] key-like text: scantest.txt:3
PRE-PUSH SCAN (all tracked files in HEAD): FINDINGS. Do not push.     exit=1
```
Geçici commit `git reset -q --hard HEAD~1` ile geri alındı (çalışma ağacı öncesinde temizdi; `git status --short` 0 satır). Sapma: `-All` testi de aynı geçici commit üzerinde yapıldı (izin yalnız `-Range` için verilmişti, ama `-All` yalnız HEAD'e baktığından başka yolu yoktu; tek bir geçici commit kullanıldı).
Uzantı kontrolü: `.zip` zaten `.gitignore`'da olduğundan normal `git add` ile stage'e girmiyor; `git add -f scantest.zip` ile denendi:
```
[3] forbidden extension: scantest.zip
PRE-PUSH SCAN (staged changes): FINDINGS. Do not push.     exit=1
```
Reset sonrası `-All`: `PRE-PUSH SCAN (all tracked files in HEAD): clean.` exit=0. (Amend edilmiş commit `d27a8bf` + script commit'i `caa9a68` üzerinde; `d27a8bf`'in kendisine ayrı bir `-All` çalıştırmadım, HEAD `caa9a68` idi. Son commit'ten sonra da `-All` çalıştırıldı, aşağıya bak.)

### 3. CI: `.github/workflows/tests.yml`
`push` + `pull_request`, `ubuntu-latest`, `actions/setup-dotnet` ile `10.0.x`, `dotnet test tests/BlastGame.Tests.slnx --configuration Release`, `permissions: contents: read`.
Action sürümleri: `actions/checkout@v7`, `actions/setup-dotnet@v6`. Seçim: `git ls-remote --tags` ile en yüksek majörler v7 ve v6 çıktı; GitHub API'deki yayın tarihlerine göre checkout v7.0.1 (2026-07-20) ve setup-dotnet v6.0.0 (2026-07-16) 3 aydan eski, yani yeni çıkmış değil. Emin olmadıklarım: bu majörlerin giriş adlarını (özellikle `dotnet-version`) sürüm notlarından okumadım; v4/v5'te aynıydı. Sürümler çalışmazsa bir önceki majöre (checkout v6, setup-dotnet v5) düşülür.
Doğrulama: YAML `python yaml.safe_load` ile ayrıştı (`on` anahtarı Python'da `True` görünür, bu YAML 1.1 özelliği, GitHub kabul eder). Aynı komut yerelde çalıştırıldı: `Başarılı! - Başarısız: 0, Başarılı: 1 ... Core.Tests.dll (net10.0)`. Linux büyük/küçük harf duyarlılığı denenmedi (Core.csproj'daki `../../Assets/Scripts/Core/**/*.cs` yolu klasör adlarıyla aynı yazılmış). Çalışma ancak ilk push'ta doğrulanır.

### 4. `.github/pull_request_template.md`
İngilizce; başlıklar: Goal, Spec, Changes, Tests, Review reports & how findings were resolved, Manual check / screenshots; her birinde kısa yorum satırı yer tutucu.

### 5. README ve CLAUDE.md
- README: en üste CI rozeti (`.../actions/workflows/tests.yml/badge.svg`, link actions sayfasına); kapı satırındaki "→ commit" yerine "her adım kendi dalında, aşama başına commit, PR + CI yeşil → merge commit" ve `docs/PLAN.md#9-git-akışı` linki. (Anchor biçimi GitHub'da `ı` harfiyle çalışır mı denemedim; çalışmazsa link bölüme değil sayfaya gider.)
- `CLAUDE.md` "Komutlar": prepush-scan'in üç modu eklendi (yalnız o bölüm).

### Commit'ler (main)
```
0335eb5 docs: add CI badge, git flow and scan modes
8b8df58 docs: add pull request template
5c66ba8 ci: add test workflow
caa9a68 feat(scripts): add -All and -Range modes to prepush scan
d27a8bf chore: add Unity project skeleton, test setup and build scripts
```
Her commit öncesi staged tarama temiz çıktı. Rapor değişikliği 6. commit'te. Push yapılmadı, git ayarına dokunulmadı.

### Yapamadıklarım
- CI'ı çalıştırmak (GitHub'a push yok); README rozeti ve anchor linki henüz doğrulanamaz.

### Emin olmadıklarım
- Action majör sürümlerinin girişleri (yukarıda); `.utmp/` kaynağı; `-All` modunun `d27a8bf`'in kendisine karşı ayrıca çalıştırılmaması.
