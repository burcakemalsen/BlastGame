# Görev: Adım 3, düşme ve doldurma (testçi)

Model: Opus 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/03-gravity-refill` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/03-gravity-refill.md` (tamamı; "Görüntü katmanı" ve "Elle kontrol listesi" test edilmez).
3. Stub imzaları: `Assets/Scripts/Core/CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs`. Mevcut tiplerden (`Grid`, `Cell`, `CellPos`, `IRandomSource`, `SeededRandom`, `GridFactory`, `BlastRules`) yalnız public imzaları. Uygulama gövdelerini okuma.
4. Mevcut test yardımcıları: `tests/Core.Tests/TestGrid.cs` (satır metninden grid kurar). Diğer test dosyalarını yalnız düzeni görmek için göz gezdir; okuyup tekrar etme.

## Rolün
Amacın kodu kırmak, geçirmek değil. Spec'in söylemediği davranışı tahmin etme: test yazma, belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. **Mevcut testleri değiştirme ve silme** (bu adımda sözleşme değişikliği yok). Commit atmazsın, push yok, dal değiştirme yok.

## Ölçü
Zor adım: yeni test için kaba üst sınır **~150** (`Theory` satırları dahil; RULES, Testçi). Bir test bir davranış sınıfını sınar; aynı sınıfı tekrar etme. Aşarsan raporda nedenini yaz.

## İşler
1. **Yeni testler**, tip başına sınıf: `CellMoveTests`, `GravityRulesTests`, `RefillRulesTests`, `TurnResultTests`, `TurnRulesTests`. Spec'in "Düşme", "Doldurma", "Bir dokunuş", "Örnekler" ve "Uç durumlar" bölümlerinin her maddesi için normal durum, sınır ve geçersiz girdi. Uç durum listesi en az kapsamdır.
2. **Örnekler birebir:** spec'teki Örnek 1-4'ü tam liste ve tam grid olarak test et.
3. **Sahte kaynaklar:** testlerde sıralı değer döndüren ve çağrı sayısını tutan bir `IRandomSource` yaz (gerekirse mevcut yardımcıyı kullan). "Kaynak çağrılmaz" maddelerini çağrı sayısıyla kanıtla.
4. **Sıra:** `Collapse`, `Fill` ve `TurnResult` listelerini spec'teki sıraya göre tam liste olarak karşılaştır (küme olarak değil).
5. **Hata türleri tam:** `Assert.Throws<ArgumentNullException>` / `<ArgumentOutOfRangeException>` / `<InvalidOperationException>`. Genel `Exception` ya da `ThrowsAny` yok. Kontrol sırasını birden çok geçersiz girdiyle sına. Hata sonrası grid'in ve kaynağın (çağrı sayısı) dokunulmadığını kontrol et.
6. **Tekrarlanabilirlik:** aynı seed'le iki bağımsız oyunda aynı dokunuş dizisi aynı grid'leri vermeli; farklı seed en az bir hücrede farklı.
7. **Düzen:** test adları `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9.
8. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Proje derlenmeli; yeni testler **kırmızı**, mevcut testler **yeşil** olmalı. Geçen yeni test varsa incele: bir şey ölçmüyor olabilir, düzelt ya da nedenini raporla.

## Rapor
`reports/03-gravity-refill-tester.md` (RULES kural 9, kısa):
- Test dosyaları ve yeni test sayısı.
- **Spec maddesi → test eşlemesi.**
- `dotnet test` özeti (toplam / geçen / kalan) aynen; geçen yeni test varsa gerekçesi.
- Spec'te belirsiz bulduğun yerler; yazmadığın testler ve nedeni.

Son mesajında kısa özet.
