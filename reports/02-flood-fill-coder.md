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

## Aşama B

**Değişen:** `Core/Cell.cs`, `Core/CellPos.cs`, `Core/GroupFinder.cs`, `Core/BlastRules.cs` (uygulandı); `View/GridView.cs` (`Build` her hücre için bir `SpriteRenderer` kurar, `Refresh` yeni nesne/doku üretmeden `enabled` ve `color` günceller); `View/GameBootstrap.cs` (`Update`: dokunma varsa yalnız `TouchPhase.Began`, yoksa `GetMouseButtonDown(0)`; `ScreenToWorldPoint` + `RoundToInt` → `IsInside` → `BlastRules.Blast` → `Refresh`). Sahne ve `tests/` değişmedi.

**Kontroller:**
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı!  - Başarısız: 0, Başarılı: 506, Atlanan: 0, Toplam: 506`
- `scripts/unity-check.ps1` → `Unity compile check: clean.`, çıkış 0 (batchmode yaklaşık 10 dk sürdü; log'da `error CS` yok)
- `scripts/build-webgl.ps1` (Terminal paneli) → `WebGL build: C:\Dev\BlastGame\Builds\WebGL  (13.14 MB total)`. Script `working tree is not clean; this build cannot be deployed` uyarısı verdi (commit edilmemiş değişiklik var); commit sonrası yeniden derlemek gerekir, yoksa `deploy-webgl.ps1` reddeder.

**Doğrulanmadı:** Editor'de/iPhone'da elle kontrol listesi (3-5) yapılmadı. Girdi kodu otomatik test edilmedi.

**Yanlış görünen test:** yok.

**Öneri:** `GameBootstrap` artık girdi de taşıyor; Adım 3'te animasyon gelince girdiyi ayrı bir bileşene ayırmak düşünülebilir.
