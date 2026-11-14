# Görev: Adım 3, düşme ve doldurma (kodcu)

Model: Opus 5.5. Proje klasörü: `C:\Dev\BlastGame`, dal `step/03-gravity-refill` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/PLAN.md`: yalnız bölüm 2 (mimari).
3. `docs/specs/03-gravity-refill.md` (tamamı; sözleşme burada).
4. Mevcut kod: `Assets/Scripts/Core/*.cs` (özellikle `CellPos.cs`, `BlastRules.cs`, `RefillRules` için `GridFactory.cs`), Aşama B'de `Assets/Scripts/View/*.cs`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okumazsın da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse Terminal panelinden çalıştır ve raporla.

## Aşama A: stub
1. Yeni tipler, dosya başına bir tip, `Assets/Scripts/Core/` altında: `CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs`. İmzalar spec'teki gibi tam.
2. Her metot, kurucu, özellik ve operatörün gövdesi yalnız `throw new System.NotImplementedException();`. İstisna: `CellMove` struct'ı alan tutabilir ama hiçbir üye onları okuyup mantık çalıştırmaz.
3. Mevcut tiplere ve görüntü katmanına dokunma.
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız (mevcut testler derlenmeli) ve `scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/03-gravity-refill-coder.md`, "Aşama A" başlığıyla: yazdığın dosyalar, iki kontrolün komutu ve sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9 (kısa rapor). Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Spec'teki davranışı uygula: `CellMove`, `GravityRules.Collapse`, `RefillRules.Fill` (önce çek ve denetle, sonra yaz), `TurnResult`, `TurnRules.Tap` (hata sırası; patlama yoksa kaynağa dokunma).
2. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: tek `SeededRandom(42)` hem üretimde hem doldurmada; dokunuşta `TurnRules.Tap` + `Refresh`; sprite ve doku bir kez üretilip saklanır. Sahne dosyasına dokunma.
3. `tests/`'e dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla.
4. Testi geçirmek için özel durum kodu yazma.
5. `scripts/build-webgl.ps1` ile WebGL derle (Terminal panelinde). Yayına alma.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (toplam/geçen/kalan), `unity-check` ve WebGL sonuç satırları, yapamadıkların, emin olmadıkların, öneriler.
