# Görev: Adım 0 eki, WebGL derlemesi ve GitHub Pages (kurulumcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`. Repo: `burcakemalsen/BlastGame` (public, `main` push edildi, CI yeşil).

## Neden
Test cihazı iPhone. iOS derlemesi Mac ve Apple imzası gerektirdiği için telefonda deneme WebGL derlemesiyle, Safari'den yapılır. Aynı sayfa README'de "tarayıcıda oyna" linki olur. APK Android için yan ürün olarak kalır (`docs/PLAN.md` bölüm 1 ve Adım 0).

## Önce oku
`CLAUDE.md`, `docs/RULES.md` ("Herkes için", "Onay", "Push öncesi tarama", "Kurulumcu"), `docs/PLAN.md` bölüm 1, Adım 0, bölüm 8 ve 9, `reports/00-setup.md` (özellikle iş 8: sandbox ve Gradle notları).

## Rolün ve sınırlar
`docs/tasks/00-setup.md`'deki kurulumcu sınırları aynen geçerli. Push yapmazsın; GitHub repo ayarına dokunmazsın (Pages'i orchestrator açar). WebGL modülü kurulu: `C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Data\PlaybackEngines\WebGLSupport`. Derleme ajan oturumunun sandbox'ında ortam hatası verirse, APK'da yaptığın gibi Terminal panelinde çalıştır ve raporla.

## İşler
1. **WebGL derlemesi.** `Assets/Editor/BuildScript.cs`'e `BuildWebGL()` ekle: varsayılan sahneyi `Builds/WebGL/`'e derler. `scripts/build-webgl.ps1`: batchmode'da `-buildTarget WebGL -executeMethod BuildScript.BuildWebGL`, başarısızsa sıfır olmayan kodla çıkar, sonunda klasörün toplam boyutunu yazar.
2. **GitHub Pages uyumu.** GitHub Pages sıkıştırılmış Unity dosyaları için gereken `Content-Encoding` başlığını göndermez. Sıkıştırma ayarını buna göre seç (ör. sıkıştırma kapalı ya da Gzip + Decompression Fallback); seçimini ve gerekçesini, boyut farkıyla birlikte raporla. Ayarı kalıcı yap (`ProjectSettings`), yalnız build sırasında değil.
3. **Mobil uyum.** Unity'nin WebGL şablonu telefonda tam ekran ve dikey çalışmalı; mobil tarayıcıda "desteklenmiyor" uyarısı çıkıyorsa kaynağını bul ve raporla. Gerekirse hangi şablon ya da ayarın kullanıldığını yaz. Oyun kodu yazma.
4. **Yerel deneme.** Derlemeyi yerelde basit bir statik sunucuyla aç (ör. `python -m http.server`; Unity WebGL `file://` ile açılmaz), sayfanın yüklendiğini HTTP 200 ve tarayıcı konsolu ya da sunucu log'u ile göster. Sunucuyu kapat.
5. **Yayın dalı.** `scripts/deploy-webgl.ps1`: `Builds/WebGL/` içeriğini yerel `gh-pages` dalına commit'ler (`git worktree` ile ayrı klasörde; `main`'in çalışma ağacına dokunmaz; ilk seferde öksüz dal, sonra normal commit, force yok). Kökte `.nojekyll` olsun. Commit mesajı `deploy: webgl build <main'in kısa hash'i>`. Push yapmaz; sonunda "push için: `git push origin gh-pages`" yazar. Çalıştır, `git log gh-pages --oneline -1` ve dosya listesini aktar.
6. **Dokümanlar.** README: "Play in the browser" bölümü, link `https://burcakemalsen.github.io/BlastGame/` (henüz çalışmıyor olabilir, orchestrator yayına alınca doğrular); Running bölümüne WebGL komutları; "Requires" satırına WebGL modülü. `CLAUDE.md` "Komutlar"a WebGL derleme ve yayın komutları (yalnız o bölüm). `Backlog.md`'ye bir madde: "Native iOS build (needs macOS/Xcode or a macOS CI runner and Apple signing)".

## Commit'ler
`main`'e, Conventional Commits, her biri öncesinde staged tarama: ör. `feat(build): add WebGL build script`, `chore(build): configure WebGL compression for GitHub Pages`, `feat(scripts): add gh-pages deploy script`, `docs: add browser play link and WebGL commands`. Orchestrator'ın bu görevle birlikte yaptığı `docs/PLAN.md` değişikliği ve bu dosya ayrı bir commit olsun: `docs: switch device test to WebGL`. Push yok, git ayarına dokunma.

## Bitti tanımı
- `scripts/build-webgl.ps1` 0 ile çıkıyor, `Builds/WebGL/index.html` var.
- Sıkıştırma ayarı Pages'e uygun ve kalıcı; gerekçesi raporda.
- Yerel sunucuda sayfa yükleniyor (kanıtıyla).
- Yerel `gh-pages` dalında derleme commit'i var, `main` temiz.
- README, `CLAUDE.md`, `Backlog.md` güncel.

## Rapor
`reports/00b-webgl.md`: her iş için ne yaptın, komut, çıktının ilgili kısmı aynen; sonra yapamadıkların, emin olmadıkların, orchestrator'ın karar vermesi gerekenler. Rapor son commit'e girsin. Son mesajında kısa özet ve `git log --oneline -8` ile `git log gh-pages --oneline -1`.
