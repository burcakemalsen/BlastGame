# Görev: Adım 2, grup bulma ve patlatma (testçi)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/02-flood-fill` (zaten açık; dal değiştirme).

## Önce oku
1. `CLAUDE.md`
2. `docs/RULES.md` (özellikle "Herkes için", "Onay" ve "Testçi")
3. `docs/specs/02-flood-fill.md` (tamamı)
4. Stub imzaları: `Assets/Scripts/Core/Cell.cs`, `CellKind.cs`, `CellPos.cs`, `GroupFinder.cs`, `BlastRules.cs` ve `Grid.cs`'in public imzaları. Yalnız imzaları oku; uygulama kodunu (`Grid`, `GridFactory`, `SeededRandom` gövdeleri) okuma.
5. Mevcut testler: `tests/Core.Tests/*.cs` (Adım 1'den; şu an yeni imzayla derlenmiyor).

## Rolün
Amacın kodu kırmak, geçirmek değil. Spec'in söylemediği davranışı tahmin etme: test yazma, belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. Commit atmazsın, push yok, dal değiştirme yok.

## İşler
1. **Mevcut testlerin geçişi (sözleşme değişikliği).** Spec, "Mevcut testler" bölümü bunun iznidir. `TileColor?` ile yazılmış `Get`/`Set` testlerini `Cell` ile yeniden yaz: `null` yerine `Cell.Empty`, renk yerine `Cell.OfColor(...)`. "`Set` tanımsız rengi reddeder" testleri "`Cell.OfColor` tanımsız rengi reddeder" olur. Başka hiçbir mevcut testi silme ve anlamını değiştirme; test sayısı ve kapsamı azalmasın. Raporda bu geçişin özetini ver (hangi dosyalar, eski ve yeni test sayısı).
2. **Yeni testler.** Tip başına sınıf: `CellTests`, `CellPosTests`, `GroupFinderTests`, `BlastRulesTests`. Spec'in "Sözleşme değişikliği", "Grup bulma", "Patlatma", "Örnekler" ve "Uç durumlar" bölümlerinin her maddesi için normal durum, sınır ve geçersiz girdi. Uç durum listesi en az kapsamdır; daha fazlasını ara (ör. dikdörtgen grid'lerde `x`/`y` karışması, `Blast` sonrası gruba girmeyen hücrelerin hiç değişmemesi, `FindGroup` sonucunun her çağrıda yeni ve aynı olması).
3. **Grid kurma yardımcısı.** Spec örneklerini okunur yazmak için testlerin içinde, satır metinlerinden grid kuran bir yardımcı yazabilirsin (ör. `"RGG", "RRG", "BRG"`, üst satır önce; `.` = boş). Yardımcının kendisi de satır sırasını doğru çevirdiğini gösteren bir testle sınansın; `y` alttan üste.
4. **Sıra:** `FindGroup` ve `Blast` sonucunu spec'teki sıraya göre tam liste olarak karşılaştır (yalnız küme olarak değil); ayrıca tekrar eden eleman olmadığını kontrol et.
5. **Hata türleri tam:** `Assert.Throws<ArgumentNullException>`, `Assert.Throws<ArgumentOutOfRangeException>`, `Assert.Throws<InvalidOperationException>`. Genel `Exception` ya da `ThrowsAny` yok; yoksa stub'ın `NotImplementedException`'ı testi yanlışlıkla geçirir. Hata sonrası grid'in değişmediğini de kontrol et.
6. **Düzen:** test adları `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9 (`LangVersion 9.0`).
7. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Test projesi **derlenmeli**; `Cell`'e ya da yeni tiplere dokunan testler **kırmızı** olmalı. Geçen testleri tek tek incele: `Cell`'e dokunmayanlar (ör. `SeededRandom`, `TileColor`, yalnız boyut ve `IsInside` testleri) geçebilir; bunun dışında geçen test bir şey ölçmüyor olabilir, düzelt ya da nedenini raporla.

## Rapor
`reports/02-flood-fill-tester.md`:
- Test dosyaları ve test sayısı (yeni / geçirilen / toplam).
- **Spec maddesi → test eşlemesi** (her madde ve her uç durum için hangi test(ler)).
- `dotnet test` özeti (toplam / geçen / kalan) aynen; geçen testlerin listesi ya da grubu ve gerekçesi.
- Mevcut testlerin geçişi: neyi nasıl değiştirdin.
- Spec'te belirsiz bulduğun yerler; yazmadığın testler ve nedeni.

Son mesajında kısa özet.
