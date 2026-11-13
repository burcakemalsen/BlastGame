# Görev: Adım 1 eki, iki yeni davranışın testleri (testçi)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/01-grid` (dal değiştirme).

## Önce oku
1. `CLAUDE.md`, `docs/RULES.md` ("Herkes için", "Onay", "Testçi")
2. `docs/specs/01-grid.md`: "(ek, 2026-11-14)" ile işaretli iki madde ve "Uç durumlar"daki iki yeni satır
3. Stub imzaları için `Assets/Scripts/Core/Grid.cs` ve `GridFactory.cs`'in yalnız public imzaları. Uygulama gövdelerini okuma.
4. Mevcut testler: `tests/Core.Tests/GridTests.cs`, `GridFactoryTests.cs` (sahte kaynak `SequenceRandomSource` orada)

## İş
- Yalnız iki yeni davranış için test ekle: `Grid.Set` tanımsız `TileColor` → `ArgumentOutOfRangeException` (tanımlı altı renk ve `null` hâlâ kabul); `GridFactory.CreateRandom` kaynak aralık dışı değer döndürürse → `InvalidOperationException`.
- Hata türleri tam (`Assert.Throws<...>`). Mevcut testleri değiştirme ya da silme. Yazdığın yer yalnız `tests/Core.Tests/`.
- `dotnet test tests/BlastGame.Tests.slnx`: yeni testler **kırmızı** olmalı (uygulama henüz bu kontrolleri yapmıyor), eski 223 yeşil kalmalı.
- Commit atma.

## Rapor
`reports/01-grid-tester.md` sonuna "Ek (2026-11-14)" bölümü, en çok 200 kelime: eklenen testler, spec maddesi → test eşlemesi, `dotnet test` özet satırı.
