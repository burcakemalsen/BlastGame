# Görev: Adım 7, engeller (kodcu)

Model: Opus 5.5 (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/07-obstacles` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/specs/07-obstacles.md` (tamamı; sözleşme burada). Gerekirse `docs/specs/06b-combos.md` ve `docs/specs/05-json-level.md`'nin ilgili bölümleri.
3. Mevcut kod: `Assets/Scripts/Core/` (gereken dosyalar); Aşama B'de `Assets/Scripts/View/*.cs` ve `Assets/Resources/Levels/`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okuman da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma, paket ekleme.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse durup raporla.

## Aşama A: stub
1. Sözleşmedeki eklemeler: `CellKind.Obstacle`, `ObstacleKind`, `Cell` üyeleri, `ObstacleHit`, `TurnEffect` 8 parametreli kurucu ve `Hits`, `TurnResult` 5 parametreli kurucu ve `Collected`, `GravityResult`, `GravityRules.Settle`, `ObstacleGoal`, `ObstaclePlacement`, `GameSession` 6 parametreli kurucu, `ObstacleGoals`, `Remaining(ObstacleKind)`, `LevelData` 7 parametreli kurucu, `Obstacles`, `ObstacleGoals`.
2. Yeni kurucu, özellik ve metotların gövdesi yalnız `throw new System.NotImplementedException();`. İstisnalar (mevcut testler bozulmasın diye): mevcut kurucular çalışmaya devam eder; `TurnEffect.Hits` ve `TurnResult.Collected` mevcut kurucularla boş liste verir; `Cell.HasBalloon` mevcut hücrelerde false döner; `GameSession.ObstacleGoals` ve `LevelData.Obstacles`/`ObstacleGoals` mevcut kurucularla boş liste verir. Mevcut metotların davranışını bu aşamada değiştirme.
3. Görüntü katmanına dokunma.
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız, `dotnet test tests/BlastGame.Tests.slnx` mevcut testler yeşil, `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/07-obstacles-coder.md`, "Aşama A" başlığıyla: dosyalar, üç kontrolün sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9. Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Kural katmanı, spec'in "Kurallar" bölümü: engel hücresi ve balon, grup bağı, vuruşlar (grup komşusu tek vuruş, özel öğe ve kombo alanları, roket engelden geçer, Orb komşu vuruşu, `OrbRocket`/`OrbBomb` tek patlama, zincirde ayrı patlamalar), `GravityRules.Settle` (Sandık bölümleri, Çapa toplama), `Collapse` Sandık bölümlerine uyar, `RefillRules.Fill` en üst Sandığın üstü, `TurnRules.Tap` (engele/boşa dokunuş hiçbir şey, `Settle` + `Collected`), `GameSession` engel hedefleri, `LevelParser` ve `LevelData` (yeni alanlar, denetim sırası, `CreateSession` yerleşim).
2. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: level modu geri (sonsuz oyun bayrakla kapalı, kod kalır), HUD'da engel hedefleri, yer tutucu çizimler (Sandık katları, şeffaf su balonu), animasyonlar, her engel için bir deneme level dosyası (`level04.json`..`level07.json`). Görseller sonra ayrı mesajla gelecek; çizimleri sonradan sprite ile değiştirilebilecek şekilde tek yerde topla. Sahne dosyasına dokunma.
3. `tests/`'e dokunmadan `dotnet test` yeşil, `unity-check` 0. Bir test yanlış görünüyorsa kodu bükme: dur ve raporla. Testi geçirmek için özel durum kodu yazma.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti, `unity-check` sonucu, animasyonda spec'ten sapmalar, doğrulanmayanlar, öneriler.
