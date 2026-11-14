# BlastGame planı

Durum: onaylandı (2026-11-13); bölüm 6'daki açık kararlar hariç. Kurallar: `docs/RULES.md`.

## 1. Amaç ve sınırlar
- **Amaç:** Telefonda çalışan, oynanabilir bir blast oyunu. Repo ilk günden public; README'de oyunun AI desteğiyle geliştirildiği açıkça yazılır. İş iki faza bölünmüştür; Faz 1 tek başına gösterilebilir bir bütündür.
- **Ürün sahibi rolü:** Kodu AI ajanları yazar. İnsan ürün sahibidir: kapsamı belirler, spec'leri onaylar, oynayıp kabul eder. Her adımın sonunda kod ona anlatılır; hedef, her dosyayı açıklayabilmesidir.
- **Sınırlar:** Üçüncü tarafa ait metin, görsel, level ya da ad repoya girmez. Mekanikler türün ortak çekirdeğidir (grid, grup bulma, düşme, doldurma); engel ve özel öğe özgün tasarlanır. Görseller önce renkli kare ve dairedir.
- **Ortam:** Unity 6000.6.3f1, git, .NET 10 SDK, `gh` 2.104.0. Android Build Support ve WebGL modülü kurulu. **Test cihazı iPhone:** iOS derlemesi Mac ve Apple imzası gerektirdiği için telefonda deneme WebGL derlemesiyle, tarayıcıdan yapılır (karar, 2026-11-14). Proje yolu yalnız ASCII karakter içerir (`C:\Dev\BlastGame`), çünkü Android derlemesinde kullanılan Gradle, Windows'ta ASCII dışı karakter içeren yollarda bozulabilir.

## 2. Mimari
İki katman:
- **Kural katmanı** (`Assets/Scripts/Core/`): saf C#. Grid, grup bulma, düşme, doldurma, hamle/hedef, level okuma. `UnityEngine` kullanmaz. Kendi assembly tanım dosyası vardır (asmdef: "bu klasör ayrı bir paket" diyen dosya) ve Unity motoruna başvurusu kapalıdır.
- **Görüntü katmanı** (`Assets/Scripts/View/`): Unity. Çizim, animasyon, dokunma. Kural katmanına sorar, kendi kural koymaz.

Sabit mimari kuralları:
- **Tek kaynak:** Kural kodu yalnız `Assets/Scripts/Core/` altında durur. Test projesi aynı dosyalara dışarıdan bakar; kopya yok.
- **Rastgelelik dışarıdan:** Doldurma rengi seed'li bir kaynaktan gelir (seed: aynı sayı verilince hep aynı sırayı üreten başlangıç değeri). Böylece testler tekrarlanabilir olur.
- **Dil sürümü:** Test projesi Unity'nin desteklediği C# sürümüne sabitlenir: C# 9.0, `netstandard2.1` (Adım 0'da doğrulandı; eksik dil özellikleri `reports/00-setup.md`'de).
- **Sahne kodla kurulur (karar, 2026-11-14):** Sahnede tek bir başlatıcı script olur; grid'i ve nesneleri oyun başlayınca o oluşturur. Gerekçe: ajanlar Editor'ü fareyle kullanamaz, sahne dosyasını elle yazınca bozar; kodla kurulunca her şey okunabilir C# olur. Bedeli: grid Editor'de yalnız Play'e basınca görünür.

## 3. Test yolu
- `tests/Core/Core.csproj`: `Assets/Scripts/Core/**/*.cs` dosyalarını içine alan kütüphane, Unity uyumlu dil sürümüyle.
- `tests/Core.Tests/`: xUnit testleri, `dotnet test` ile saniyeler içinde çalışır, Unity Editor açıkken de çalışır.
- `tests/` klasörü `Assets` dışında olduğu için Unity onu görmez.
- Görüntü katmanı otomatik test edilmez. Onun kapısı: spec'teki elle kontrol listesi + ürün sahibinin Editor'de oynaması + ekran görüntüsü.
- Unity derlemesi `scripts/unity-check.ps1` ile Editor penceresi açılmadan (batchmode) kontrol edilir; bu sırada Editor'ün kapalı olması gerekir.

## 4. Roller ve adım kapısı

Orchestrator yalnız beyindir: iş üretmez, yaptırır ve denetler. Her adım yeni bir orchestrator oturumuyla başlar; önceki oturum adımı kapatıp durum dosyasını günceller (uzun konuşma her mesajda yeniden okunduğu için en büyük token harcaması budur).

| Rol | Model | Ne yapar |
|---|---|---|
| Ürün sahibi | İnsan | Spec'i onaylar, oynar, karar verir |
| Orchestrator | Opus 5.5 (ana oturum) | Spec ve görev tarifi yazar, ajanları başlatır, rapor ve diff okur, geri yollar, ürün sahibine anlatır, push öncesi taramayı ve push'u yapar |
| Kurulumcu | Sonnet 5.5 medium | Adım 0'ı yapar |
| Kodcu | Adıma göre Sonnet 5.5 medium ya da Opus 5.5 | Stub'ları ve uygulamayı yazar |
| Testçi | Basit adım (1, 2, 4, 5, 9) ve küçük ek: Sonnet 5.5 medium; zor adım (3, 6, 7, 8): Opus 5.5 medium | Spec ve stub imzalarından, kodu kırmaya çalışan testler yazar |
| Kontrolcü 1 | Sonnet 5.5 | Açık arar, bilerek bozma dener, rapor yazar |
| Kontrolcü 2 | Gemini 4 Argon (`agy`), yalnız zor adımlarda (3, 6, 7, 8) | Kodu spec'le karşılaştırır, kaçan uç durumu arar; farklı model ailesi. Kotası biterse ya da `agy` çalışmazsa atlanır ve rapora not düşülür |

Kodcu modeli:

| Adım | Kodcu |
|---|---|
| 1 Grid, 2 Grup bulma, 4 Hamle/hedef, 5 JSON level, 9 Cila | Sonnet 5.5 medium |
| 3 Düşme/doldurma, 6 Özel öğe, 7 Engel, 8 Çıkmaz durum | Opus 5.5 |

Sonnet bir adımda iki tur üst üste testleri geçiremezse o adım Opus'a geçer.

### Ajanlar görünür çalışır
Her ajanın ne yaptığı izlenebilir olmalı. Bu yüzden ajanlar arka planda gizli alt ajan olarak değil, Claude masaüstü uygulamasında **ayrı oturum** olarak başlatılır; her biri kenar çubuğunda görünür ve yan yana (split view) açılabilir. Gemini kontrolcüyü orchestrator Terminal panelinde `agy` ile çalıştırır. Her oturum temiz bağlamla başlar; bildiği her şey görev tarifi ve okuduğu dosyalardır. Görev tarifleri `docs/tasks/` altında durur. Orchestrator sonuçları oturumun kısa bildiriminden değil, yazılan dosyalardan ve `git diff`'ten okur. İstisna: adım kapısına girmeyen bakım işi alt ajanla yapılabilir (`docs/RULES.md`, Orchestrator). (Adım 0'da denendi, çalıştı.)

Aynı anda yalnız bir ajan dosya yazar. Tek paralel iş iki kontrolcüdür; Claude kontrolcü bilerek bozma için kendi çalışma kopyasında (git worktree: aynı reponun ayrı bir klasördeki kopyası) çalışır.

### Adım kapısı
Biri geçmeden sonrakine geçilmez:
1. **Spec** (`docs/specs/NN-ad.md`): amaç, davranış, örnekler, uç durumlar, **arayüz sözleşmesi** (sınıf ve metot adları, aldıkları, döndürdükleri), elle kontrol listesi. Orchestrator yazar, ürün sahibi onaylar.
2. **Stub:** Kodcu sözleşmedeki sınıfları içi boş yazar (stub: kapı numarası asılmış ama içi boş oda; çağrılınca "henüz yazılmadı" hatası verir). Orchestrator sözleşmeyle karşılaştırır.
3. **Testler:** Testçi yazar. Çalıştırılır, **kırmızı** olmalı. Derleniyor ama hepsi geçiyorsa test bir şey ölçmüyordur, durulur.
4. **Kod:** Kodcu yazar, tüm testler **yeşil**. `tests/` klasörüne dokunmaz. Orchestrator diff'i okur.
5. **Bağımsız kontrol:** Claude kontrolcü her adımda; Gemini yalnız zor adımlarda (3, 6, 7, 8), Claude'la paralel. Raporlar `reports/NN-ad-claude.md` ve `reports/NN-ad-gemini.md`. Bulgu kod hatasıysa kodcuya, eksik testse testçiye gider; sonra tekrar kontrol.
6. **Ürün sahibi:** Editor'de ve telefonda oynar, orchestrator kodu anlatır. Telefon denemesi için orchestrator, kontrol kapısı geçince adımın dalından WebGL derler ve taramadan sonra `gh-pages`'e yayımlar (önizleme; `main`'e merge'den önce).
7. **Push, PR, merge:** push öncesi taramadan sonra; ayrıntı bölüm 9. Kapı 6'dan sonra kod değiştiyse WebGL yeniden yayımlanır.

**Ara commit'ler:** Adım içinde her aşama adımın dalında yerelde ayrı commit'lenir: spec (1), stub (2), kırmızı testler (3), yeşil kod (4), kontrolcü düzeltmelerinin her biri (5). Commit'i orchestrator, aşamayı kontrol ettikten sonra atar. Böylece kontrolcü `git diff <test-commit>..<kod-commit> -- tests/` ile kodcunun testlere dokunup dokunmadığını görür; bozulursa önceki aşamaya dönülür. **Push yalnız 7. kapıda**, adımın bütün kapıları geçtikten sonra yapılır; kırmızı testli hal hiçbir zaman tek başına push'lanmaz.

Her görev tarifi şunu içerir: hangi dosyaları okuyacağı (`docs/RULES.md` ve ilgili spec her zaman), nereye yazabileceği, "bitti"nin tanımı, raporun biçimi.

## 5. Adımlar

### Adım 0: iskelet (tamamlandı, 2026-11-14)
Kurulumcu yaptı, oyun kodu yok: Unity projesi ve asmdef'ler, test projeleri, derleme/build/tarama scriptleri, CI, README, boş APK ve Pages'te boş WebGL. Yapılacaklar listesi `docs/tasks/00-setup.md` ve `docs/tasks/00b-webgl.md`'de, sonuçlar `reports/00-setup.md` ve `reports/00b-webgl.md`'de.

Bitti tanımı: Unity projesi hatasız derleniyor, `dotnet test` yeşil, C# sürümü doğrulandı, boş APK üretildi, boş WebGL sayfası iPhone Safari'de açıldı, tarama deneme dosyasını yakalıyor, `CLAUDE.md`'ye komutlar yazıldı, ilk push taramadan geçti.

### Faz 1: oynanabilir çekirdek
| # | Adım | Bitti tanımı |
|---|---|---|
| 1 | Grid | Kural katmanında grid modeli testli; Play'de renkli karelerden grid görünüyor; grid'li WebGL sayfası telefonda açıldı |
| 2 | Grup bulma ve patlatma | Dokunulan karenin aynı renkli komşu grubu bulunuyor (flood fill), 2 ve üstü patlıyor, tek kare patlamıyor; testler köşe, kenar, tüm grid tek renk durumlarını kapsıyor |
| 3 | Düşme ve doldurma | Patlamadan sonra kareler aşağı iniyor, üstten seed'li rastgele renk geliyor; aynı seed aynı sonucu veriyor |
| 4 | Hamle, hedef, kazan/kaybet | Hamle sayacı, renk toplama hedefi, kazanma ve kaybetme ekranı |
| 5 | JSON level | Level boyutu, renk sayısı, hamle, hedef dosyadan okunuyor; bozuk dosya anlaşılır hata veriyor; en az 3 level |

Adım 5 notu: Unity'nin `JsonUtility`'si `UnityEngine` içinde olduğu için kural katmanında kullanılamaz. Önerilen yol, Unity'den bağımsız bir JSON kütüphanesidir (Newtonsoft: Unity'de resmi paketi, testlerde NuGet paketi); böylece bozuk dosya davranışı da otomatik testlenir. Kesin karar Adım 5 spec'inde.

### Faz 2: ayırt edici kısım
| # | Adım | Bitti tanımı |
|---|---|---|
| 6 | Özel öğe (özgün tasarım) | Tasarım ürün sahibiyle konuşulup spec'e yazıldı; büyük grup özel öğe üretiyor, özel öğe kendi etkisini yapıyor |
| 7 | Engel (özgün tasarım) | Tasarım spec'te; engel level dosyasından geliyor, kuralına göre kırılıyor |
| 8 | Çıkmaz durum | Hiç hamle kalmadığı algılanıyor ve grid oynanabilir hale getiriliyor |
| 9 | Cila ve sunum | Basit animasyon, güzel görsel, README'de ekran görüntüsü, kısa video, tarayıcıda oynama linki (GitHub Pages) ve GitHub Releases'te APK |

## 6. Açık kararlar (ürün sahibinin)
- **Ürün katmanı:** (a) kısa tasarım dokümanı (neden bu engel, level'lar neden bu zorlukta), (b) bir level'ı binlerce kez kendi kendine oynayıp kazanma oranını ölçen küçük program. Kabul edilirse Faz 2'ye adım olarak eklenir.
- **Oyun adı, özel öğe ve engel tasarımı:** Adım 6 ve 7'den önce.
- **JSON kütüphanesi:** Adım 5 spec'inde (bölüm 5'teki not).

## 7. Riskler
| Risk | Önlem |
|---|---|
| Üçüncü taraf malzeme ya da yayımlanmaması gereken bilgi repoya girer | `.gitignore` `.zip`, `.pdf`, `.docx` dosyalarını dışlar; her push'ta tarama (terim listesi yerelde) |
| Model kotası adım ortasında biter | Durum her kapıdan sonra kaydedilir; iş kaldığı kapıdan devam eder |
| Orchestrator "küçük bir şey" diye kodu kendi düzeltir | `RULES.md` orchestrator bölümü; kontrolcü commit yazarına ve diff'e bakar |
| Uzun oturumda bağlam özetlenir, kurallar unutulur | Kurallar ve durum dosyada; her adım temiz oturumla başlayabilir |
| Test spec'i değil kodu doğrular | Testçi uygulamayı görmez; testler koddan önce yazılır ve kırmızı görülür; bilerek bozma denemesi |
| Kodcu testi geçirmek için hile yapar | Kontrolcü özel durum kodunu arar; `tests/` diff'i boş olmalı |
| Stub hatası testi yanlışlıkla yeşil yapar | Testçi beklenen hata türünü tam belirtir (`Exception` değil, örneğin `ArgumentException`) |
| `dotnet test` yeşil, Unity derlemiyor | Dil sürümü sabit; her adımda `scripts/unity-check.ps1` çalışır |
| Android build ortam sorunu | Proje ASCII yolda; Gradle önbelleği ASCII yola alınır (`build-android.ps1`); boş APK Adım 0'da denendi |
| WebGL iPhone Safari'de kötü çalışır (performans, dokunma, ses) | Boş WebGL Adım 0'da, grid'li hali Adım 1'de telefonda denenir; sorun çıkarsa gerçek iOS derlemesi (Backlog) yeniden değerlendirilir |
| Hazır Unity `.gitignore` şablonu test projesini dışlar | `*.csproj` ve `*.sln` kuralı yalnız köke uygulanır; Adım 0'da `git status` ile kontrol edilir |

## 8. Repo düzeni
Public repoda değerlendiren kişinin görmek isteyeceği her şey durur: README, kod, testler, `CLAUDE.md`, `docs/RULES.md`, `docs/PLAN.md`, `docs/specs/`, `docs/tasks/`, `reports/`, `Backlog.md`, commit geçmişi.

`.gitignore` şunları dışlar (kesin liste dosyanın kendisinde): yerel ayarlar, Unity'nin ürettikleri, build çıktıları (APK GitHub Releases'te yayımlanır), sırlar (`.env`, imza anahtarı, anahtar ve token dosyaları), test çıktıları, editör ayarları ve ek güvence olarak `*.zip`, `*.pdf`, `*.docx`.

`.meta` dosyaları repoda kalır (Unity onlarsız dosya bağlantılarını kaybeder).

## 9. Git akışı
Hedef: repoya bakan biri, her adımın nasıl yapıldığını (spec, test önce, kod sonra, bağımsız kontrol) geçmişten okuyabilsin; `main` her zaman yeşil olsun.

- **Dal (branch):** Her adım kendi dalında yapılır: `step/01-grid`, `step/02-flood-fill` gibi. `main`'e doğrudan commit yalnız Adım 0'da olur.
- **Commit mesajı:** İngilizce, emir kipinde, Conventional Commits biçiminde: `docs(spec): add grid spec`, `feat(core): add grid stubs`, `test(core): add grid tests`, `feat(core): implement grid model`, `fix(core): handle empty grid`, `chore: ...`, `ci: ...`. Her commit tek iş; gövdede gerekirse "neden". Bir aşamanın görev tarifi ve raporu o aşamanın commit'ine girer (ör. `test(core)` commit'i testçi tarifini, testleri ve testçi raporunu birlikte taşır); tarif ve rapor için ayrı commit atılmaz. Kural, plan ve spec değişiklikleri kendi `docs` commit'inde kalır.
- **PR (pull request):** 7. kapıda dal push'lanır ve `main`'e PR açılır. PR açıklaması: adımın amacı, spec linki, değişenler, test sonucu (kaç test, hepsi yeşil), kontrolcü raporlarının linki ve bulguların nasıl kapandığı, ürün sahibinin elle kontrolü (ekran görüntüsüyle). Şablon `.github/pull_request_template.md`'de durur.
- **CI:** GitHub Actions her push'ta ve PR'da `dotnet test` çalıştırır; README'de durum rozeti (badge) görünür. PR yalnız CI yeşilken birleştirilir. Unity derlemesi CI'da çalışmaz (Unity lisansı gerektirir); yerelde `scripts/unity-check.ps1` ile kontrol edilir.
- **Merge:** Merge commit ile (squash yok), böylece dalın TDD sırası (kırmızı test commit'i, sonra yeşil kod) geçmişte kalır; `main`'in kendi çizgisi yalnız yeşil hallerden oluşur. Merge'den sonra dal silinir.
- **Sürüm etiketi:** Faz 1 sonunda `v0.1.0`, Faz 2 sonunda `v1.0.0`; her etikette GitHub Releases'e APK ve kısa değişiklik notu; WebGL sürümü GitHub Pages'te güncellenir.
- **PR'ı kim açar:** orchestrator, `gh` ile; açıklama metnini de o yazar.
