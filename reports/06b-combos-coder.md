# Adım 6b kodcu raporu

## Aşama A

### Dosyalar
- `Assets/Scripts/Core/EffectKind.cs`: sona `RocketCross`, `RocketBomb`, `BigBomb`, `OrbRocket`, `OrbBomb`, `OrbOrb`.
- `Assets/Scripts/Core/TurnEffect.cs`: 7 parametreli kurucu (gövde `NotImplementedException`), `MergedCount`, `Converted`. Mevcut 5 parametreli kurucu çalışır; `MergedCount` = `Group` 0, diğerleri 1; `Converted` boş dizi. Özellikler alan okur; 7 parametreli kurucu fırlattığı için o yoldan değer gelmez.
- `Assets/Scripts/Core/SpecialRules.cs`: `Tap(Grid, int, int, IRandomSource)` (gövde `NotImplementedException`). `Activate`, `TurnRules`, `GameSession` değişmedi. Görüntü katmanına dokunulmadı. Yeni dosya yok.

### Kontroller
- `dotnet build tests/BlastGame.Tests.slnx`: `Oluşturma başarılı oldu. 0 Uyarı 0 Hata`
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı! - Başarısız: 0, Başarılı: 927, Atlanan: 0, Toplam: 927`
- `scripts/unity-check.ps1`: `Unity compile check: clean.`

### Spec'te belirsiz bulduklarım
1. 7 parametreli kurucu, `kind` ile `mergedCount`/`converted` tutarlılığını denetler mi? (ör. `Group` + `mergedCount 2`, `Rocket` + dolu `converted`, `mergedCount > cleared.Count`). Spec yalnız null ve negatif denetimi sayıyor; ben başka denetim eklemeyeceğim.
2. 5 parametreli kurucu kombo türüyle çağrılırsa `MergedCount` 1 olur (spec kuralı gereği). Kural katmanı kombo için her zaman 7 parametreliyi kullanacak.
3. `Tap` "hücre özel değil → `ArgumentException`": boş hücre de bu kapsamda sayılacak (Adım 6 `Activate` ile aynı).
4. `Tap` katılımcı tek olduğunda tekli etki + zincir: `Activate` ile birebir aynı sonuç (Orb dahil, çekiliş yok) varsayıyorum.
5. Katılımcı bulmada bağlantı yalnız özel hücreler üzerinden; arada boş hücre bağı koparır (renkli gibi) varsayıyorum.
