# Görev: Adım 1, Grid (kodcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/01-grid` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku
1. `CLAUDE.md`
2. `docs/RULES.md` (özellikle "Herkes için" ve "Kodcu")
3. `docs/PLAN.md` bölüm 2 (mimari) ve 4 (adım kapısı)
4. `docs/specs/01-grid.md` (tamamı; sözleşme buradadır)

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/` (stub ve uygulama). `tests/` klasörüne dokunmazsın, okumazsın da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9, `netstandard2.1`; rastgelelik yalnız `IRandomSource` üzerinden.
- Spec'te olmayan özellik ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator, kontrolden sonra atar. Push yok, dal değiştirme yok, git ayarına dokunma.
- Unity batchmode komutlarını (`scripts/unity-check.ps1`) Editor kapalıyken çalıştır. Sandbox ortam hatası verirse Terminal panelinden çalıştır ve raporla.

## Aşama A: stub
1. Spec'teki "Arayüz sözleşmesi"ndeki beş tipi `Assets/Scripts/Core/` altında, dosya başına bir tip olarak yaz: imzalar, sabitler (`MinSize`, `MaxSize`, `MaxColorCount`) ve `TileColor` enum'u spec'teki gibi tam olsun.
2. Her metot ve kurucunun gövdesi yalnız `throw new System.NotImplementedException();`. Özellikler (`Width`, `Height`) de get'te aynı hatayı fırlatsın. Alan, doğrulama ya da mantık yazma.
3. Sözleşmede olmayan public üye ekleme. `AssemblyMarker.cs`'e bu aşamada dokunma (testler ona bağlı; Aşama B'de silinecek).
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` ve `scripts/unity-check.ps1` hatasız. Çıktıları aynen aktar.

**Rapor (Aşama A):** `reports/01-grid-coder.md` dosyasına "Aşama A" başlığıyla: yazdığın dosyalar, iki kontrolün komutu ve çıktısı, spec'te belirsiz bulduğun yerler. Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Spec'teki davranışı uygula: `Grid`, `SeededRandom` (SplitMix64, başlangıç durumu `(ulong)(long)seed`), `GridFactory` (sabit doldurma sırası).
2. `Assets/Scripts/Core/AssemblyMarker.cs` ve `.meta`'sını sil (spec, "Temizlik").
3. Görüntü katmanı (`Assets/Scripts/View/`): spec'in "Görüntü katmanı" bölümündeki gibi `GameBootstrap` ve gereken yardımcılar. Sahne dosyasına dokunma. `Assets/Scripts/View/.gitkeep`'i sil.
4. `tests/` klasörüne dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla (karar ürün sahibinin).
5. Testi geçirmek için özel durum kodu yazma (testin beklediği değeri doğrudan döndürmek gibi).
6. `scripts/build-webgl.ps1` ile WebGL derle (Terminal panelinde). Yayına alma; o orchestrator'ın işi.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (geçen/kalan sayısı), `unity-check` ve WebGL derleme çıktısı, yapamadıkların, emin olmadıkların, öneriler.
