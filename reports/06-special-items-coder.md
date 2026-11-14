# Adım 6 kodcu raporu: özel öğeler

## Aşama A

### Dosyalar (`Assets/Scripts/Core/`)
- Yeni: `SpecialKind.cs`, `EffectKind.cs`, `ClearedCell.cs`, `TurnEffect.cs`, `SpecialRules.cs` (+ Unity'nin ürettiği `.meta` dosyaları).
- Değişen: `CellKind.cs` (`Special`), `Cell.cs` (`OfSpecial`, `OfOrb`, `IsSpecial`, `Special`, `OrbColor`), `TurnResult.cs` (4 parametreli kurucu, `Effects`).
- Yeni gövdelerin hepsi `throw new NotImplementedException();`. İstisnalar: enum değerleri ve `SpecialRules` sabitleri tam; `TurnResult`'ın 3 parametreli kurucusu `Effects`'i boş liste (`Array.Empty<TurnEffect>()`) yapar ve `Effects` getter'ı çalışır. `Cell`'in mevcut üyeleri ve eşitliği, `TurnRules`, `GameSession`, `GroupFinder`, `BlastRules` değişmedi. Görüntü katmanına dokunulmadı.

### Kontroller
- `dotnet build tests/BlastGame.Tests.slnx` → `Oluşturma başarılı oldu. 0 Uyarı 0 Hata`
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı! - Başarısız: 0, Başarılı: 766, Atlanan: 0, Toplam: 766`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.` (çıkış 0)

### Spec'te belirsiz bulduğum yerler (Aşama B'de varsayımım)
1. `SpecialRules.ForGroup`: `size` 0-4 ve tanımsız `color` birlikte gelirse hata mı, `Cell.Empty` mi? Hata sırası listesi renk denetimini çekilişten önce koyuyor; varsayım: boyuttan bağımsız `ArgumentOutOfRangeException(color)`.
2. `TurnResult` 4 parametreli kurucu: null denetim sırası belirtilmemiş; varsayım: `blasted`, `moves`, `filled`, `effects`. `blasted`'ın etkilerle tutarlılığı kurucuda denetlenmez (spec istemiyor).
3. `TurnEffect` kurucusu: `kind` ile `created` tutarlılığı (Group dışında `Created` dolu olması) ya da tanımsız `kind` için hata belirtilmemiş; varsayım: yalnız spec'teki iki denetim.
4. `Activate` grid dışı hatasında parametre adı belirtilmemiş; varsayım: `x` / `y` (`GroupFinder` gibi).
5. `Cell.Special` Orb hücresinde `SpecialKind.Orb` döner (özel hücre olduğu için); spec bunu dolaylı söylüyor.
