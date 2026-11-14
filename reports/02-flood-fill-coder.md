# Adım 2, kodcu raporu

## Aşama A

**Yeni (stub, gövdeler `NotImplementedException`):** `Assets/Scripts/Core/CellKind.cs` (enum, `Empty = 0`), `Cell.cs`, `CellPos.cs`, `GroupFinder.cs`, `BlastRules.cs` (`MinGroupSize = 2`). `Cell` ve `CellPos` yalnız özel alan tutar (`_kind`/`_color`, `_x`/`_y`); hiçbir üye onları okumaz. `Cell.Empty` `default` kaldı.

**Değişen:**
- `Grid.cs`: iç dizi `Cell[,]`, `Get` `Cell` döndürür, `Set` `Cell` alır, renk kontrolü kalktı, sınır kontrolü aynı.
- `GridFactory.cs`: `Cell.OfColor((TileColor)value)`.
- `GridView.cs` (`Build`): `Cell` okur, `IsEmpty` ise atlar, renkli ise `cell.Color`.

**Kontroller:**
- `dotnet build tests/Core/Core.csproj` → `Oluşturma başarılı oldu.`, `4 Uyarı`, `0 Hata`. Uyarılar CS0169 (stub struct alanları kullanılmıyor); Aşama B'de kalkar.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.`, çıkış kodu 0.

Test projesi çalıştırılmadı (beklenen).

**Spec'te belirsizlik:** Bulmadım. Küçük not: `Cell`/`CellPos` stub'larında alan tutmak için özel alanlar kullandım; spec'te alan adı yok, sözleşmeye public üye eklenmedi.
