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
