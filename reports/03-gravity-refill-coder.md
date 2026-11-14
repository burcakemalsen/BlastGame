# Adım 3, kodcu raporu

## Aşama A

**Yeni (stub, bütün gövdeler `throw new System.NotImplementedException();`):** `Assets/Scripts/Core/CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs` (+ Unity'nin ürettiği `.meta` dosyaları). İmzalar spec'teki sözleşmeyle aynı. `CellMove` yalnız özel alan tutar (`_from`, `_to`); kurucu dahil hiçbir üye onları okumaz. Mevcut tipler ve görüntü katmanı değişmedi; `tests/`'e dokunulmadı.

**Kontroller:**
- `dotnet build tests/BlastGame.Tests.slnx` → `Oluşturma başarılı oldu.`, `2 Uyarı`, `0 Hata`. Uyarılar CS0169 (`CellMove._from`/`_to` kullanılmıyor); Aşama B'de kalkar.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.`, çıkış kodu 0; log'da `error CS` yok.

**Spec'te belirsizlikler (karar ürün sahibinin / testçinin; Aşama B'de varsayılanım parantez içinde):**
1. `TurnResult` kurucusunda birden çok `null` liste gelirse hangi parametre adının `ArgumentNullException`'da raporlanacağı yazmıyor (parametre sırası: `blasted`, `moves`, `filled`).
2. `TurnResult` verilen listeleri kopyalamalı mı, olduğu gibi mi saklamalı belirtilmemiş (olduğu gibi saklarım; dışarıdan değiştirilebilirlik testlenmemeli ya da spec'e eklenmeli).
3. Patlama olmayan `Tap`'te dönen boş listelerin her çağrıda yeni nesne mi, paylaşılan boş liste mi olduğu yazmıyor (yeni boş liste, `BlastRules` gibi).

Sonuç: 1 ve 2 spec'te netleşti (`9e60302`), 3 test edilmiyor; yeni liste kaldı.

## Aşama B

**Değişen:**
- `Core/CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs`: uygulandı. `Fill` önce boş hücreleri toplar, bütün değerleri çekip denetler, sonra yazar. `Tap` hatalarını spec sırasıyla (`grid`, `random`, `colorCount`, `x`, `y`) kaynağa ve grid'e dokunmadan atar; patlama yoksa `Collapse`/`Fill` çağrılmaz.
- `View/GameBootstrap.cs`: tek `SeededRandom(42)` alanı (`_random`), hem `CreateRandom`'da hem `TurnRules.Tap`'te; dokunuşta `Tap` + `Refresh`.
- `View/GridView.cs`: kare sprite'ı statik alanda bir kez üretilir; `Build` tekrar çağrılsa da yeni doku oluşturmaz (Unity nesnesi yok edilmişse `== null` ile yeniden üretir). `Refresh` değişmedi.
- Sahne ve `tests/` değişmedi.

**Kontroller:**
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı!  - Başarısız: 0, Başarılı: 591, Atlanan: 0, Toplam: 591`
- `scripts/unity-check.ps1` → `Unity compile check: clean.`, çıkış 0; log'da `error CS` yok.
- `scripts/build-webgl.ps1` (Terminal paneli) → `WebGL build: C:\Dev\BlastGame\Builds\WebGL  (13.17 MB total)`. Uyarı: `working tree is not clean; this build cannot be deployed` (`Build info: 9a7b78a (clean=False)`); yayın için commit sonrası yeniden derlenmeli.

**Yapılmadı / doğrulanmadı:**
- Elle kontrol listesi (Editor'de Play, iPhone) yapılmadı.
- Spec'teki "Madde Backlog'dan silinir" (sprite önbelleği): `Backlog.md` kodcunun alanı (`Assets/`) dışında olduğu için silmedim; orchestrator'a bırakıldı.

**Öneri:** yok.

## Aşama C

**Değişen:**
- `View/GridView.cs`: `AnimateTurn(TurnResult)` + `IsAnimating`. Korutin: önce `Blasted` kareleri doğrusal küçülür; sonra `Refresh` ile renkler son hale geçer, her kare başlangıç konumuna alınır (`Moves` için `From`, `Filled` için hücresinin sütundaki yeni kare sayısı kadar üstü) ve hepsi aynı anda `t³` ile hücresine iner, sonda tam konuma oturtulur. Grid'in kopyası tutulmaz. Başlangıç konumları ve sütun sayaçları `Build`'de bir kez ayrılan dizilerde tekrar kullanılır. Maske: grid alanını kaplayan bir `SpriteMask` (aynı önbellekteki sprite, `Build`'de bir kez); kareler `VisibleInsideMask`.
- `View/GameBootstrap.cs`: animasyon sürerken dokunuş yok sayılır; dokunuşta `TurnRules.Tap` sonucu `AnimateTurn`'e verilir (`Refresh` artık animasyonun içinde). Patlama yoksa görüntü değişmez.
- Kural katmanı, `tests/`, sahne değişmedi; paket eklenmedi (`SpriteMask` Unity'nin yerleşik modülünden, `manifest.json` değişmedi).

**Sabitler (`GridView`):** `BlastDuration = 0.12f` sn, `FallDuration = 0.35f` sn (`TileSize = 0.9f` değişmedi).

**Kontroller:**
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı!  - Başarısız: 0, Başarılı: 598, Atlanan: 0, Toplam: 598`
- `scripts/unity-check.ps1` → `Unity compile check: clean.`, çıkış 0; log'da `error CS` yok.

**Emin olmadıklarım / doğrulanmadı:**
- Editor'de ve iPhone'da görsel olarak denenmedi (maskenin kenarı, akıcılık, Ek A elle kontrol 1-5).
- Dokunuş başına bir korutin nesnesi oluşur (geçici çöp, kalıcı nesne değil); spec'in "kalıcı nesne yok" şartını karşıladığını düşünüyorum.
- Küçülme doğrusal; spec yalnız "küçülerek" diyor, ease belirtmiyor.

## Aşama C ek: 9x9

- `View/GameBootstrap.cs`: `Width` 8 → 9, `Height` 10 → 9; kamera, maske ve animasyon boyutu grid'den okuduğu için başka değişiklik gerekmedi.
- `dotnet test` → `Başarısız: 0, Başarılı: 598, Toplam: 598`; `unity-check` → `Unity compile check: clean.`, çıkış 0.
