# Rapor: Adım 0 eki, WebGL derlemesi ve GitHub Pages (kurulumcu)

Kurulumcu: Sonnet 5.5. Tarih: 2026-11-14. Tarif: `docs/tasks/00b-webgl.md`.

## Özet
Bitti tanımının 5 maddesi sağlandı. Push yapılmadı, repo ayarına dokunulmadı. Tek not: `main` çalışma ağacı tam temiz değil (aşağıda, "Orchestrator'ın karar vermesi gerekenler" 1).

## İşler

### Önce: orchestrator değişikliği
`docs/PLAN.md` ve `docs/tasks/00b-webgl.md` ayrı commit'e girdi: `0089327 docs: switch device test to WebGL` (staged tarama temiz).

### 1. WebGL derlemesi
- `Assets/Editor/BuildScript.cs`: `BuildWebGL()` eklendi (`Builds/WebGL`); Android ile ortak `Build(outputPath, target)` metoduna taşındı, `BuildAndroid()` davranışı aynı.
- `scripts/build-webgl.ps1`: `-buildTarget WebGL -executeMethod BuildScript.BuildWebGL`, hata ya da `index.html` yoksa sıfır dışı çıkış, sonda klasör boyutu.
- Derleme ajan sandbox'ında denenmedi; APK deneyimi (rapor 00, iş 8) nedeniyle doğrudan Terminal panelinde çalıştırıldı. Çıktı:
```
WebGL build: C:\Dev\BlastGame\Builds\WebGL  (16.41 MB total)     (sıkıştırma kapalı, son hâl)
```
`Builds/WebGL/index.html` var. Gradle/ASCII sorunu WebGL'de çıkmadı (Gradle kullanmıyor). Commit: `1a1113e feat(build): add WebGL build script`.

### 2. GitHub Pages ve sıkıştırma
- Başlangıç ayarı: `ProjectSettings/ProjectSettings.asset`: `webGLCompressionFormat: 1` (Gzip), `webGLDecompressionFallback: 0`. Bu hâlle derlendi (ölçüm için), çıktı `Build/WebGL.data.gz`, `.framework.js.gz`, `.wasm.gz`: toplam **5 406 874 bayt (5,16 MiB)**.
- Seçim: **sıkıştırma kapalı** (`webGLCompressionFormat: 2`, Disabled), kalıcı olarak `ProjectSettings.asset` içinde. Satır: `webGLCompressionFormat: 2`.
- Gerekçe: GitHub Pages `.gz` dosyalarına `Content-Encoding: gzip` başlığı eklemez (tarifteki bilgi; ben Pages'e erişmedim, doğrulamadım). Unity'nin yükleyicisi (`WebGL.loader.js`) bu durumda açıkça hata verecek şekilde yazılmış ("Content-Encoding ... missing", Safari için ayrıca bir Content-Type uyarısı); kapalı sıkıştırmada bu sınıf sorun hiç oluşmaz. Gzip + Decompression Fallback çalışır ama tarayıcıda JS ile açıldığı için başlangıç yavaşlar ve ek JS yükler; denemedim.
- Boyut farkı (bayt):

| Dosya | Gzip | Sıkıştırmasız |
|---|---|---|
| `WebGL.wasm` | 4 267 265 | 13 273 635 |
| `WebGL.data` | 1 017 660 | 3 561 015 |
| `WebGL.framework.js` | 73 553 | 326 123 |
| `WebGL.loader.js` | 27 907 | 27 914 |
| Klasör toplamı | 5,16 MiB | 16,41 MiB |

- Önemli: boyut farkı telefondaki indirme boyutuyla aynı olmayabilir. Pages'in (CDN) kapalı sıkıştırmalı `.wasm`/`.js` dosyalarına kendiliğinden gzip uygulayıp uygulamadığını bilmiyorum; `.data` (`application/octet-stream`) büyük olasılıkla ham iner (3,4 MiB). İlk yayında ağ sekmesinden bakmak gerekir.
- Commit: `059d903 chore(build): configure WebGL compression for GitHub Pages`.

### 3. Mobil uyum
- Şablon: `ProjectSettings.asset` içinde `webGLTemplate: APPLICATION:Default`. Unity 6000.6.3f1 Default şablonu (`...\WebGLTemplates\Base\Default\index.html`) `iPhone|iPad|iPod|Android` kullanıcı ajanında `unity-mobile` stilini uyguluyor (tam ekran: `style.css` `.unity-mobile #unity-fullscreen-container { width: 100%; height: 100% }`, `viewport` meta etiketi ekleniyor, alt bilgi gizleniyor). **Bu sürümde "mobilde desteklenmiyor" uyarısı yok**; şablonda öyle bir metin aramadım ama taramada çıkmadı, mobil emülasyonda da çıkmadı.
- Gereksinimler (yükleyici `WebGL.loader.js` hata metinlerinden okundu): WebGL 2 ("Upgrade to iOS 15 or later" ipucu), WebAssembly ve **WebAssembly 2023** özellikleri ("Chrome ≥ 91, Firefox ≥ 89, Safari ≥ 16.4"). Yani iPhone'da **iOS 16.4 ve üstü** gerekir; ürün sahibinin iOS sürümüne bakılmalı.
- Mobil emülasyon (built-in tarayıcı, Android Chrome kullanıcı ajanı, 375x812): `container: "unity-mobile"`, canvas 375x812 (viewport'un tamamı), viewport meta `width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes`, konsolda hata yok, uyarı bandı boş. Bu **iPhone Safari değil**; gerçek cihaz denemesi ürün sahibinin.
- Dikey yön: `defaultInterfaceOrientation: Portrait` WebGL'de tarayıcıyı kilitlemez; sayfa cihazın yönüne uyar. Yatay çevirince canvas yatay kalır. Kilitleme gerekirse Adım 1'de oyun/şablon tarafında çözülür (şimdi yapmadım).
- Oyun kodu yazılmadı; sahne Adım 0'ın boş `Main.unity`'si.

### 4. Yerel deneme
Komut: `python -m http.server 8765 --directory Builds/WebGL --bind 127.0.0.1` (Python 3.12.10). `curl` ile:
```
index 200 text/html
WebGL.loader.js 200 text/javascript 27914
WebGL.framework.js 200 text/javascript 326123
WebGL.data 200 application/octet-stream 3561015
WebGL.wasm 200 application/wasm 13273635
```
Sunucu log'u (kısaltıldı): `GET /index.html 200`, `/Build/WebGL.loader.js 200`, `.framework.js 200`, `.data 200`, `.wasm 200`.
Tarayıcı (built-in, `http://127.0.0.1:8765/index.html`) konsolu (kısaltıldı, 51 kayıttan seçilmiş): `Initialize engine version: 6000.6.3f1 (45d8eee7de74)`, `Creating WebGL 2.0 context.`, `OPENGL LOG: Creating OpenGL ES 3.0 graphics device`, `[Physics::Module] Name: PhysX`, `Input Manager initialize...`, `Audio context resumed after 0.213 seconds.`, `[UnityCache] '.../Build/WebGL.data' successfully downloaded and stored in the browser cache`. Sayfa durumu: başlık `Unity Web Player | BlastGame`, canvas var, `#unity-loading-container` gizli (yükleme bitti), uyarı bandı boş; mobil emülasyonda `onlyErrors` konsolu boş. Sunucu `Get-NetTCPConnection -State Listen` ile kapatıldığı doğrulandı (`curl` → bağlantı yok).
- Sınır: Sayfa boş sahneyi gösterdiğinden görsel olarak yalnız varsayılan sahne arka planı var; ekran görüntüsü almadım. Unity'nin "yüklendi" kanıtı konsol kayıtlarıdır.

### 5. Yayın dalı
- `scripts/deploy-webgl.ps1`: `Builds/gh-pages-worktree` (gitignore'lu `Builds/` içinde) klasöründe `git worktree` açar (`gh-pages` yoksa `--orphan -b`), içeriği build ile değiştirir, `.nojekyll` ekler, commit eder (`deploy: webgl build <main kısa hash>`), worktree'yi kaldırır. Push yok, force yok. Sonda "To publish: git push origin gh-pages".
- **İlk deneme sorunu:** Git `core.autocrlf` ayarı yüzünden `framework.js`, `style.css`, `index.html` için "LF will be replaced by CRLF" uyarısı verdi, yani dosya baytları değişebilirdi. Henüz push edilmediği için kendi oluşturduğum `gh-pages` dalını `git branch -D` ile silip, scripte `git -c core.autocrlf=false` (yalnız komut satırı bayrağı; git ayarı değişmedi) ekleyerek yeniden çalıştırdım. Commit: `d448918 fix(scripts): keep line endings unchanged in gh-pages deploy`. Bayt kontrolü (`md5sum`, `git show gh-pages:<dosya>` ile `Builds/WebGL/<dosya>`): `Build/WebGL.framework.js identical`, `index.html identical`, `Build/WebGL.wasm identical`.
- Çalıştırma:
```
git log gh-pages --oneline -1   →   8db8eb5 deploy: webgl build f58b3ee
```
Dosya listesi (`git ls-tree -r --name-only gh-pages`): `.nojekyll`, `Build/WebGL.data`, `Build/WebGL.framework.js`, `Build/WebGL.loader.js`, `Build/WebGL.wasm`, `TemplateData/{MemoryProfiler.png, favicon.ico, fullscreen-button.png, progress-bar-empty-dark.png, progress-bar-empty-light.png, progress-bar-full-dark.png, progress-bar-full-light.png, style.css, unity-logo-dark.png, unity-logo-light.png, unity-logo-title-footer.png, webmemd-icon.png}`, `index.html`.
- İkinci çalıştırma (aynı içerik): `gh-pages: nothing changed.`, dal tek commit'te kaldı (normal-commit yolu, içerik değişince çalışacak şekilde yazıldı ama gerçek değişiklikle denenmedi).
- `git worktree list` yalnız `C:/Dev/BlastGame [main]`.
- Not: `.nojekyll` ve içerik commit'i `Co-Authored-By` gövdesi taşıyor. Dal henüz push edilmedi; commit mesajındaki kısa hash `f58b3ee` (deploy script commit'i), sonradan gelen `d448918`/`b36cdf2` değil. Bu yalnız bilgi amaçlı.
- `scripts/prepush-scan.ps1 -Range`/`-All` gh-pages'i taramaz (HEAD/`main` aralığı). Elle denedim: `git grep -a -w -F -f <terimler> gh-pages` → **1 dosya eşleşti: `Build/WebGL.wasm` (3 yer)**, terim listesinin **ilk terimi** (4 harfli, genel bir sözcük). Eşleşen satırlar Unity'nin kendi bellek profili metinleri (`%s  <TERM> Block count %zu`, `... Large allocation bytes ...`, `... main deferred allocation count ...`), yani yanlış pozitif, gerçek bir eşleşme değil. Terimi bu rapora yazmadım. Karar: orchestrator.

### 6. Dokümanlar
README: "Play in the browser" bölümü (`https://burcakemalsen.github.io/BlastGame/`, henüz çalışmıyor olabilir), Running bölümüne WebGL derleme ve deploy komutları, "Requires" satırına WebGL Build Support. `CLAUDE.md` "Komutlar": WebGL derleme (Terminal paneli notu), yayın (`deploy-webgl.ps1`, `git push origin gh-pages`, yerel `python -m http.server`). `Backlog.md`: "Native iOS build (needs macOS/Xcode or a macOS CI runner and Apple signing)". Commit: `b36cdf2 docs: add browser play link and WebGL commands`.

## Commit'ler (main)
```
0089327 docs: switch device test to WebGL
1a1113e feat(build): add WebGL build script
059d903 chore(build): configure WebGL compression for GitHub Pages
f58b3ee feat(scripts): add gh-pages deploy script
d448918 fix(scripts): keep line endings unchanged in gh-pages deploy
b36cdf2 docs: add browser play link and WebGL commands
```
Her commit öncesi staged tarama: clean.

## Yapamadıklarım
- iPhone Safari'de ya da GitHub Pages'te deneme (cihaz/erişim yok). Gzip + Decompression Fallback seçeneğini derleyip denemedim, yalnız Gzip ve Disabled.
- `deploy-webgl.ps1`'in "içerik değişince ikinci normal commit" yolu gerçek değişiklikle denenmedi.

## Emin olmadıklarım
- GitHub Pages'in `.gz` için `Content-Encoding` göndermediği bilgisi tariften, doğrulanmadı; CDN'in sıkıştırmasız `.wasm`/`.js`'e otomatik gzip uygulayıp uygulamadığı bilinmiyor.
- iOS 16.4 gereksinimi yükleyici metinlerinden çıkarıldı, gerçek cihazda sınanmadı.
- Mobil emülasyon Android kullanıcı ajanıydı; iPhone Safari'ye özgü bir sorun (ör. WebGL belleği, ses kilidi, dokunma) bu denemede görünmez.
- 16,41 MiB'lık toplam, ilk sayfa yüklemesi için mobil bağlantıda uzun olabilir (özellikle 13 MB `.wasm`).

## Orchestrator'ın karar vermesi gerekenler
1. **Çalışma ağacı tam temiz değil:** başlangıçta `docs/PLAN.md`, `docs/RULES.md`, `docs/tasks/00-setup.md`, `reports/00-setup.md` değişiklik görünüyordu (ben değiştirmedim; metin temizliği gibi görünüyor). Dokunmadım, commit'lemedim. Senin işin.
2. `gh-pages` taraması: elle `-All` benzeri tarama ilk terimde yanlış pozitif veriyor (yukarıda). Push öncesi `gh-pages` için ayrı bir kural gerekir mi (ör. yalnız dosya adı ve metin dosyaları)? `prepush-scan.ps1`'e `-Ref <dal>` modu eklememi istersen yaparım.
3. Sıkıştırma: Kapalı (16,41 MiB) ya da Gzip + Decompression Fallback (5,16 MiB, yavaş açılış, denenmedi) ya da Brotli (HTTPS ve `Content-Encoding` ister, Pages'te büyük olasılıkla çalışmaz). İlk yayında ağ sekmesine bakıp karar verilebilir.
4. Yatay/dikey kilit: WebGL'de yön kilitlenmez; Adım 1'in spec'ine eklenmeli mi.
5. Deploy commit mesajındaki hash: `main`'in o anki HEAD'i; yayından önce son hâliyle yeniden deploy etmek istersen `deploy-webgl.ps1` aynı build'i yeniden commit etmez (içerik aynıysa "nothing changed").

## Ek iş: tarama terimleri yerel git klasöründen okunuyor

- `scripts/prepush-scan.ps1` terim listesini artık `$(git rev-parse --git-common-dir)/info/scan-terms` dosyasından okuyor (düz metin, satır başına bir terim, `#` ile başlayan ve boş satırlar atlanır). Script ve yorumlarında artık yerel dosya adı yok; yorum: "local term list (kept inside .git/info, never committed)". Dosya git-common-dir altında olduğundan worktree'lerde de çalışır. Commit: `d6b3567 feat(scripts): read scan terms from local git info`.
- Kural uyumu: bu rapordaki yasaklı ifadeler nötr sözcüklerle değiştirildi (commit `5e8b2be docs: neutralize wording in WebGL report`). `README.md`, `Backlog.md`, script yorumları ve bu işin commit mesajları kontrol edildi; yasak ifade yok (README'deki GitHub kullanıcı adı, orchestrator'ın verdiği repo adından geliyor).
- Davranış kontrolleri: dosya yoksa `ERROR: local term list (.git/info/scan-terms) missing; cannot scan names.` çıkış **2**; dosya boşsa `ERROR: local term list is empty.` çıkış **2** (denemek için dosyayı geçici taşıyıp geri koydum; boyutu 297 bayt, aynı).
- `-All` (şu anki HEAD, `d6b3567`): **29 bulgu**, hepsi madde [3] (terim bulundu); terimler yazılmadı. Dosya:satır: `.gitignore:2`, `.gitignore:3`, `CLAUDE.md:8`, `CLAUDE.md:9`, `CLAUDE.md:17`, `docs/PLAN.md:8`, `:55`, `:65`, `:83`, `:121`, `:122`, `:124`, `:138`, `docs/RULES.md:6`, `:25`, `:29`, `:37`, `docs/tasks/00-setup.md:9`, `:12`, `:15`, `:32`, `:34`, `:36`, `reports/00-setup.md:77`, `:107`, `:141`, `:142`, `:155`, `:180`. Dağılım: `.gitignore` 2, `CLAUDE.md` 3, `docs/PLAN.md` 8, `docs/RULES.md` 4, `docs/tasks/00-setup.md` 6, `reports/00-setup.md` 6. Bunlar henüz temizlenmemiş geçmiş/dokümanlar; çalışma ağacındaki (commit'lenmemiş) düzeltmeleri `-All` görmez, çünkü HEAD'i okur. `reports/00-setup.md` benim yazdığım rapor; temizlik orchestrator'da olduğundan dokunmadım.
- Staged mod denemesi: listedeki ilk terim bir deneme dosyasına yazıldı ve stage edildi → `[3] scan term found: scantest.txt:2`, `PRE-PUSH SCAN (staged changes): FINDINGS. Do not push.`, çıkış 1. Deneme dosyası unstage edilip silindi, `git status` yalnız orchestrator'ın dört değişikliğini gösteriyor.
- Yapamadıklarım / emin olmadıklarım: `-Range` ve `-All` bu ek işte yeniden denenmedi (yalnız `-All` ve staged); mantık değişmedi, yalnız terim kaynağı değişti. Yerel `gh-pages` dalı yeni kurallardan önce oluşturuldu; commit mesajı ve içeriği (build çıktısı) yasak ifade içermiyor, ama içeriğin tamamı yeniden taranmadı.
