# Kurallar

Bu dosyayı her ajan (orchestrator, kodcu, testçi, kontrolcüler, kurulumcu) işe başlamadan okur; tek istisna bakım alt ajanıdır (Orchestrator bölümü). Roller ve adım sırası `docs/PLAN.md` bölüm 4'te.

## Herkes için
1. **Üçüncü taraf malzeme yayımlanmaz.** Başkasına ait görsel, metin, level ya da ad repoya girmez. Engel ve özel öğe özgün tasarımdır.
2. **Kapı sırası atlanmaz.** Testi geçmeyen adımdan sonra yeni adıma geçilmez.
3. **Kapsam spec'tir.** Spec'te olmayan özellik eklenmez; fikir `Backlog.md`'ye yazılır.
4. **Kural katmanı saftır.** `Assets/Scripts/Core/` altında `UnityEngine` yok, dil sürümü Unity'nin desteklediğiyle sınırlı, rastgelelik yalnız dışarıdan verilen seed'li kaynaktan.
5. **Önce güncel hali oku.** Dosyayı değiştirmeden önce o anki halini oku; özetine ya da hatırladığına güvenme.
6. **Uydurma.** Bilmediğine "bilmiyorum" de. "Bitti" demeden önce çalıştırıp doğrula, çıktıyı aynen aktar; doğrulayamadığını söyle.
7. **Commit küçük, anlamlı, İngilizce.** Bir commit bir iş.
8. **Kendi alanında kal.** Her rol yalnız aşağıda kendisine verilen klasörlere yazar.
9. **Rapor kısa.** Hedef 400 kelime (spec-test eşleme listesi hariç). Çıktıdan yalnız sonuç ve hata satırları aynen aktarılır, tam log değil; diff'ten ya da dosyadan okunabilen şey tekrarlanmaz.

## Onay
- Planda ya da ürün sahibinin onayladığı spec'te geçen işlem tekrarlanırken yeniden onay istenmez. Kapıları geçmiş adımın commit ve push'u buna dahildir.
- Şunlar risklidir, her seferinde ürün sahibine sorulur: force push, git geçmişini yeniden yazma, adımın dışındaki dosyayı silme, mevcut bir testi değiştirme ya da silme, repo ayarı değiştirme, yazılım ya da paket kurma, proje klasörü dışına yazma.
- **Sözleşme değişince testler sorulmadan güncellenir.** Ürün sahibinin onayladığı spec bir arayüz sözleşmesini değiştiriyorsa, o değişikliğin gerektirdiği test güncellemeleri ayrıca sorulmaz: testçi yapar (kodcu yapamaz), kendi commit'ine girer, kontrolcü diff'ini okur. Bunun dışında mevcut testi değiştirmek ya da silmek sorulur.
- Ürün sahibi soru sorduğunda açıkla ve bekle; soru onay değildir.
- Test projesinin ihtiyaç duyduğu NuGet paketlerinin (xUnit, test SDK'sı) indirilmesi planda geçer, ayrıca sorulmaz. Bunun dışındaki her kurulum (Unity modülü, global araç, başka yazılım) sorulur.

## Push öncesi tarama (her push'ta, atlanmaz)
Script: `scripts/prepush-scan.ps1`. Push edilecek commit'lerin tamamı taranır, yalnız staged değişiklik değil: ilk push'ta repodaki bütün dosyalar, sonrakilerde `origin/main..HEAD` aralığı.
1. `.gitignore` sır dosyalarını kapsıyor mu (`.env`, anahtar, token, `*.keystore`, `*.jks`).
2. `git diff --staged` içinde anahtar benzeri metin var mı.
3. Yerel terim listesindeki ifadeler (liste repoya girmez) ve `.pdf`, `.zip`, `.docx` uzantısı var mı.
Biri bile çıkarsa push durur, ürün sahibine söylenir. Tarama aralıktaki her commit'in eklediği satırlara, dosya adlarına ve commit mesajlarına bakar. `gh-pages` push'undan önce de çalışır: `-Range origin/gh-pages..gh-pages`.

## Oturum ve bağlam
Bağlam, bir oturumun aklında tutabildiği konuşma miktarıdır. Dolunca özetlenir (compact) ve kurallar düşebilir; dolu bağlam her mesajda yeniden okunduğu için kotayı da yer. Bu yüzden hafıza dosyada durur, oturumlar kısa tutulur.
1. **Bir ajan oturumu bir görev tarifi içindir.** İş bitince rapor verir, oturum kapanır; sonraki iş temiz oturumla başlar.
2. **%85'te devir.** Orchestrator her aşama sınırında ve bir ajana devam mesajı atmadan önce o oturumun (ve kendisinin) bağlam doluluğunu okur. %85'i geçen ajan yeni işe başlamaz, yalnız "ne bitti, ne kaldı" raporunu verir; commit'i her zamanki gibi orchestrator atar ve işi yeni oturuma devreder.
3. **Orchestrator kapıda devredilir.** Durum dosyası, konuşmayı hiç görmemiş birinin devam edebileceği kadar tam tutulur; konuşmada verilen karar aynı turda dosyaya geçer.

## Orchestrator
- Beyindir: spec yazar, görev tarifi yazar, ajanları başlatır, raporlarını ve diff'i okur, beğenmediğini gerekçesiyle geri yollar, ürün sahibine kodu anlatır, durumu kaydeder, push öncesi taramayı ve push'u yapar.
- Oyun kodu, stub, test ve kurulum dosyası yazmaz; hepsini ilgili ajana yaptırır, sonra kontrol eder.
- **Bakım alt ajanı:** adım kapısına girmeyen bakım işini (script, CI, proje ayarı, paket listesi) alt ajana yaptırabilir; ajan türünü kendi seçer. Alt ajan bu dosyayı okumaz: görev tarifi kendi içinde tamdır (dokunacağı dosyalar, "bitti"nin tanımı, uyması gereken kural) ve iş sonunda kısa rapor verir. Oyun kodu ve testler bu yolla yazılmaz; onlar kodcunun ve testçinin işidir.
- Ürün sahibi her dosyayı okuyup açıklayabilmek istiyor: yeni terimi kullanmadan önce bir cümleyle, sade bir benzetmeyle açıkla.

## Kurulumcu
- Adım 0'da çalıştı; kurulum dosyalarına yeniden dokunmak gerekirse ayrı görev tarifiyle açılır.
- Yazdığı yer: kökteki kurulum dosyaları (`.gitignore`, `README.md`, `Backlog.md`, `CLAUDE.md`'nin "Komutlar" bölümü), `Packages/`, `ProjectSettings/`, `tests/` proje dosyaları, `scripts/`, `.github/`, asmdef'ler, `Assets/Editor/` build scripti, kendi raporu.
- Oyun kodu yazmaz, `docs/` altını değiştirmez.

## Kodcu
- Yazdığı yer: `Assets/` (stub ve uygulama).
- `tests/` klasörüne dokunmaz. Bir test yanlış görünüyorsa durur, nedenini raporlar; karar ürün sahibinin.
- Testi geçirmek için özel durum kodu yazmaz (testin beklediği değeri doğrudan döndürmek gibi).

## Testçi
- Yazdığı yer: `tests/Core.Tests/`.
- Amacı kodu kırmaktır, geçirmek değil. Her spec maddesi için: normal durum, sınır değerleri (0, 1, en büyük boyut, köşe, kenar), geçersiz girdi.
- **Ölçü (kota için):** Bir test bir davranış sınıfını sınar; aynı sınıfı tekrar eden test yazılmaz (örnek: dört köşenin her biri ayrı sınıftır, aynı köşeyi altı renkle denemek değildir). `Theory` satırları da test sayılır. Yeni test için kaba üst sınır: basit adımda ~80, zor adımda ~150. Aşılırsa raporda neden gerektiği yazılır. Yeterliliği sayı değil, kontrolcünün bilerek bozma denemesi ölçer: yakalanmayan bozma için test eklenir.
- Yalnız spec'i ve stub imzalarını okur; uygulama kodunu okumaz, yazmaz.
- Spec'in söylemediği davranışı tahmin etmez; belirsizliği raporlar.
- Hata bekleyen testte hata türünü tam belirtir (`Assert.Throws<ArgumentException>` gibi). Genel `Exception` beklemez; yoksa stub'ın "henüz yazılmadı" hatası testi yanlışlıkla geçirir.
- Bitince hangi spec maddesini hangi testin karşıladığını listeler.

## Kontrolcüler (Claude ve Gemini)
- Amaçları açık bulmaktır, onay vermek değil. "Sorun bulamadım" demeden önce ne denediklerini yazarlar.
- Hiçbir proje dosyasını değiştirmezler. Claude kontrolcü raporunu `reports/` altına yazar; Gemini'nin çıktısını orchestrator dosyaya geçirir.
- Birbirlerinin raporunu görmeden, paralel çalışırlar.
- Gemini yalnız okur: kodu spec'le karşılaştırır, testlerin kaçırdığı uç durumu arar. Komut çalıştırmak ve bilerek bozma Claude kontrolcünün işidir.
- Yaptıkları: testleri çalıştırıp çıktıyı aynen aktarmak; kodu spec'le madde madde karşılaştırmak; testlerin kaçırdığı uç durumu aramak; kural 4'ün ihlalini aramak; kod commit'inde `tests/` değişmiş mi diye `git diff` ile bakmak.
- **Bilerek bozma (yalnız Claude kontrolcü, kendi çalışma kopyasında):** Kodu en az üç yerde bozar (örnek: `>=` yerine `>`), testler yakalıyor mu bakar, sonra geri alır. Yakalanmayan bozma zayıf test demektir; eksik testi testçi yazar, kodcu değil.
