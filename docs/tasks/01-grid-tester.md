# Görev: Adım 1, Grid (testçi)

Model: Opus 5.5. Proje klasörü: `C:\Dev\BlastGame`, dal `step/01-grid` (zaten açık; dal değiştirme).

## Önce oku
1. `CLAUDE.md`
2. `docs/RULES.md` (özellikle "Herkes için" ve "Testçi")
3. `docs/specs/01-grid.md` (tamamı)
4. Stub imzaları: `Assets/Scripts/Core/*.cs` (şu an gövdeleri yalnız `NotImplementedException`; imzadan başka bir şey okumana gerek yok)
5. Mevcut test düzeni: `tests/Core.Tests/Core.Tests.csproj`, `tests/Core.Tests/SmokeTests.cs`

## Rolün
Amacın kodu kırmak, geçirmek değil. Uygulama henüz yok; testlerin spec'e göre doğru davranışı tarif etmeli. Spec'in söylemediği bir davranışı tahmin etme: test yazma, belirsizliği raporla.

Yazdığın yer yalnız `tests/Core.Tests/`. `Assets/`'e yazmazsın. Commit atmazsın (orchestrator atar). Push yok, dal değiştirme yok.

## İşler
1. **Testler.** Spec'in "Davranış", "Örnekler" ve "Uç durumlar" bölümlerinin her maddesi için test: normal durum, sınır değerleri, geçersiz girdi. Spec'in uç durum listesi en az kapsamdır; daha fazlasını ara (ör. `Set` sonrası diğer hücrelerin değişmemesi, `Set(..., null)`, aynı hücreye üst üste yazma, geniş/dar grid'lerde koordinat karışıklığı: `x` ile `y`'nin yer değiştirmesi, `Width`/`Height`'ın karışması).
2. **Hata türleri tam:** `Assert.Throws<ArgumentOutOfRangeException>`, `Assert.Throws<ArgumentNullException>`. Genel `Exception` ya da `ThrowsAny` kullanma; yoksa stub'ın `NotImplementedException`'ı testi yanlışlıkla geçirir.
3. **Sahte kaynak:** `GridFactory`'nin doldurma sırasını ve `colorCount` kullanımını sınamak için testlerin içinde verilen diziyi sırayla döndüren bir `IRandomSource` yaz. Kaç kez ve hangi `maxExclusive` ile çağrıldığını da kaydetsin; `CreateRandom`'ın her hücre için tam bir kez `Next(colorCount)` çağırdığını kontrol et.
4. **SeededRandom:** spec'teki determinizm, aralık, kaba eşitlik testleri. Spec algoritmayı (SplitMix64, başlangıç durumu `(ulong)(long)seed`, `Next(max) = (int)(çıktı % (ulong)max)`) tanımlıyor. SplitMix64'ün bilinen referans çıktılarından emin olduğun bir değer varsa (ör. belirli bir seed'in ilk 64 bitlik çıktısı) bunu `Next` üzerinden doğrulayan bir test ekleyebilirsin. Emin değilsen ekleme, raporda söyle; uydurma değer yazma.
5. **Temizlik:** `tests/Core.Tests/SmokeTests.cs`'i sil (spec, "Temizlik"; ürün sahibi onayladı).
6. **Düzen:** test sınıfları tip başına (`GridTests`, `SeededRandomTests`, `GridFactoryTests`), test adları `Metot_Durum_Beklenen` biçiminde, namespace `BlastGame.Core.Tests`. Dil sürümü C# 9 (test projesi `LangVersion 9.0`).
7. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Testler **derlenmeli ve kırmızı olmalı** (stub `NotImplementedException` atıyor). Geçen test varsa tek tek incele: hiçbir şey ölçmüyorsa düzelt; gerçekten stub'dan bağımsız olarak geçmesi gerekiyorsa (ör. yalnız sabit değeri ya da enum değerini kontrol eden test) nedenini raporla.

## Rapor
`reports/01-grid-tester.md`:
- Test dosyaları ve test sayısı.
- **Spec maddesi → test eşlemesi** (her madde ve her uç durum için hangi test(ler)).
- `dotnet test` özeti (toplam / geçen / kalan) aynen; geçen testlerin listesi ve gerekçesi.
- Spec'te belirsiz bulduğun yerler; yazmadığın testler ve nedeni.

Son mesajında kısa özet.
