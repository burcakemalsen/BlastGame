# Görev: Adım 6b, kombolar (kodcu)

Model: Opus 5.5 (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/06b-combos` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/specs/06b-combos.md` (tamamı; sözleşme burada). Gerekirse `docs/specs/06-special-items.md`'nin "Kurallar" bölümü (tekli öğeler ve zincir).
3. Mevcut kod: `Assets/Scripts/Core/` altında `EffectKind.cs`, `TurnEffect.cs`, `SpecialRules.cs`, `TurnRules.cs`, `GameSession.cs`, `Cell.cs`; Aşama B'de `Assets/Scripts/View/*.cs`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okuman da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma, paket ekleme.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse durup raporla.

## Aşama A: stub
1. Sözleşmedeki eklemeler: `EffectKind`'a altı değer (sona); `TurnEffect`'e 7 parametreli kurucu, `MergedCount`, `Converted`; `SpecialRules.Tap(grid, x, y, random)`.
2. Yeni kurucu, özellik ve metotların gövdesi yalnız `throw new System.NotImplementedException();`. İstisna: mevcut testler bozulmasın diye mevcut 5 parametreli `TurnEffect` kurucusu çalışmaya devam eder ve `MergedCount`/`Converted` bu kurucuyla spec'teki değerleri verir (`Group` 0, diğerleri 1; `Converted` boş). Mevcut metotların (`Activate`, `TurnRules.Tap`, `GameSession`) davranışını bu aşamada değiştirme.
3. Görüntü katmanına dokunma.
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız, `dotnet test tests/BlastGame.Tests.slnx` mevcut testler yeşil, `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/06b-combos-coder.md`, "Aşama A" başlığıyla: dosyalar, üç kontrolün sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9. Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Kural katmanı, spec'in "Kurallar" bölümü: katılımcı bulma (4 komşu, bağlı bütün özel hücreler), güç sırasına göre kombo seçimi, `Cleared` başında katılımcılar + `MergedCount`, alan tarama sıraları, `OrbOrb` (özel öğeler tetiklenmeden temizlenir), dönüşüm (`Converted`, yeni öğeler kuyruğa `TriggeredBy = 0`), `OrbRocket` yön çekilişleri grid değişmeden önce, zincir (Adım 6), `TurnRules.Tap` özel dokunuşta `SpecialRules.Tap`, `GameSession` renk sayımına `Converted`.
2. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: birleşme toplanması, altı kombonun animasyonu, dönüşüm ışınları ve dönüşen öğelerin patlaması, `TriggeredBy` ile zamanlama, havuz, sızıntı yok. Sahne dosyasına dokunma.
3. `tests/`'e dokunmadan `dotnet test` yeşil, `unity-check` 0. Bir test yanlış görünüyorsa kodu bükme: dur ve raporla. Testi geçirmek için özel durum kodu yazma.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti, `unity-check` sonucu, animasyonda spec'ten sapmalar, doğrulanmayanlar, öneriler.
