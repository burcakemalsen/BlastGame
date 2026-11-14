# Görev: Adım 5, JSON level (testçi)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/05-json-level` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/05-json-level.md`: "Dosya biçimi", "Kurallar", "`LevelData`", "Örnekler", "Uç durumlar", "Arayüz sözleşmesi". Bakım, görüntü katmanı ve elle kontrol test edilmez.
3. Stub imzaları: `Assets/Scripts/Core/LevelFormatException.cs`, `LevelData.cs`, `LevelParser.cs`. Mevcut tiplerden (`ColorGoal`, `GameSession`, `GridFactory`, `Grid`, `TileColor`, `IRandomSource`, `SeededRandom`) yalnız public imzalar. Uygulama gövdelerini okuma.
4. Mevcut test yardımcıları: `tests/Core.Tests/TestGrid.cs` ve önceki adımların testlerindeki sahte kaynak. Varsa onları kullan, yeniden yazma.

## Rolün
Amacın kodu kırmak, geçirmek değil. Spec'in söylemediği davranışı tahmin etme: test yazma, belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. **Mevcut testleri değiştirme ve silme.** Paket ekleme (Newtonsoft test projesine zaten geliyor). Commit atmazsın, push yok, dal değiştirme yok.

## Ölçü
Basit adım: yeni test için kaba üst sınır **~80** (`Theory` satırları dahil; RULES, Testçi). Bir test bir davranış sınıfını sınar; aynı sınıfı tekrar etme. Tür hataları için her alanı her türle denemek gerekmez; spec'teki "en az bir alanda" ölçüsü yeter.

## İşler
1. **Yeni testler**, tip başına sınıf: `LevelParserTests`, `LevelDataTests`, ayrıca `LevelFilesTests` (gerçek dosyalar). "Uç durumlar" listesi en az kapsamdır.
2. **Örnekler birebir:** spec'teki Örnek 1-7.
3. **Hata türleri tam:** `LevelFormatException`, `ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException` için `Assert.Throws<T>` (tam tür; alt türün yanlışlıkla geçmemesi için). `LevelData` kurucusunda `ParamName`'i kontrol et.
4. **Mesajlar:** `LevelFormatException` mesajının tamamını sınama; yalnız alan yolunu tek tırnakla içerdiğini sına (`Assert.Contains("'goals[1].color'", ex.Message)` gibi). Sözdizimi hatalarında `InnerException` dolu olmalı.
5. **Denetim sırası:** iki hata birlikte varken ilkinin yolunun mesajda geçtiğini en az iki örnekle sına (ör. hem `width` hem `moves` hatalı → `'width'`).
6. **Kopya:** `LevelData`'ya verilen liste sonradan değişince `Goals` değişmiyor.
7. **`CreateSession`:** grid boyutu, hücre renkleri `0..ColorCount-1`, oturumun `MovesLeft`, `ColorCount`, `Goals` değerleri level'dan; aynı seed'li iki çağrı → hücre hücre aynı grid; `null` kaynak hata.
8. **Gerçek dosyalar:** test, çalıştığı klasörden yukarı çıkarak `Assets` klasörünü içeren repo kökünü bulur ve `Assets/Resources/Levels/*.json` dosyalarını okur (kopya yok). En az 3 dosya var ve her biri `LevelParser.Parse`'tan hatasız geçiyor. Dosyalar henüz yok; bu test şimdilik kırmızı olacak, beklenen durum.
9. **Düzen:** test adları `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9. JSON metinleri test içinde okunaklı dursun (verbatim string `@"..."`).
10. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Yeni testler **kırmızı**, mevcut 681 test **yeşil**. Geçen yeni test varsa incele: bir şey ölçmüyor olabilir, düzelt ya da nedenini raporla (ör. `LevelFormatException` kurucu testleri stub'da da geçer, bu normal).

## Rapor
`reports/05-json-level-tester.md` (RULES kural 9, kısa): test dosyaları ve yeni test sayısı; **spec maddesi → test eşlemesi**; `dotnet test` özeti (toplam / geçen / kalan) aynen; spec'te belirsiz bulduğun yerler. Son mesajında kısa özet.
