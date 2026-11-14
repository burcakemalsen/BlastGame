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

## Aşama C: düşme animasyonu (orchestrator başlatınca)
Spec'in "Ek A" bölümü (`fc0a210`). Yalnız `Assets/Scripts/View/` değişir; kural katmanı ve `tests/` değişmez.
1. Animasyonu yalnız `TurnRules.Tap`'in döndürdüğü `TurnResult`'tan çiz: `Blasted` küçülerek kaybolur, sonra `Moves` ve `Filled` sabit sürede kübik ease-in ile düşer; yeni kareler sütunlarında grid'in üstünden gelir. Süreler adlandırılmış sabit.
2. Grid alanının dışında kalan kareler görünmez (maske). Animasyon sürerken dokunuş yok sayılır.
3. Mevcut kare nesneleri kullanılır; dokunuş başına yeni doku ya da kalıcı nesne yok. Paket ekleme, sahne dosyasına dokunma.
4. Kontrol: `dotnet test` yeşil (598), `scripts/unity-check.ps1` 0. WebGL derleme bu aşamada gerekmez; orchestrator commit'ten sonra temiz ağaçtan derler.

**Rapor (Aşama C):** aynı dosyaya "Aşama C" başlığıyla: değişen dosyalar, iki kontrolün sonuç satırları, sabitlerin adları ve değerleri, emin olmadıkların.
