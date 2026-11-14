# Görev: Adım 1 eki, iki yeni davranışın uygulaması (kodcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/01-grid` (dal değiştirme).

## Önce oku
1. `CLAUDE.md`, `docs/RULES.md` ("Herkes için", "Kodcu")
2. `docs/specs/01-grid.md`: "(ek, 2026-11-14)" ile işaretli iki madde
3. `Assets/Scripts/Core/Grid.cs`, `GridFactory.cs`
4. Yeni testler: `tests/Core.Tests/GridTests.cs` ve `GridFactoryTests.cs` içinde `UndefinedColor` ve `SourceValue...OutOfRange`/`BadValue` geçen testler

## İş
- `Grid.Set`: tanımsız `TileColor` → `ArgumentOutOfRangeException`; hata sonrası hücre değişmez. `null` ve tanımlı altı renk kabul.
- `GridFactory.CreateRandom`: kaynak `0 .. colorCount-1` dışı değer verirse `InvalidOperationException`.
- Yalnız `Assets/Scripts/Core/` altına yaz; `tests/`'e dokunma. Testi geçirmek için özel durum kodu yazma.
- Kontrol: `dotnet test tests/BlastGame.Tests.slnx` 246/246 yeşil. `scripts/unity-check.ps1`'i **çalıştırma** (aynı anda başka bir ajan Unity derlemesi yapıyor; Unity kontrolünü orchestrator yapacak).
- `Packages/`, `Builds/` ve diğer dosyalara dokunma; `git reset`/`checkout`/`stash`/`clean` çalıştırma. Commit atma.

## Rapor
`reports/01-grid-coder.md` sonuna "Ek (2026-11-14)" bölümü, en çok 200 kelime: değişen dosyalar, `dotnet test` özet satırı.
