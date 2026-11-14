# Görev: Adım 2, grup bulma ve patlatma (kodcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/02-flood-fill` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku
1. `CLAUDE.md`
2. `docs/RULES.md` (özellikle "Herkes için" ve "Kodcu")
3. `docs/PLAN.md` bölüm 2 (mimari) ve 4 (adım kapısı)
4. `docs/specs/02-flood-fill.md` (tamamı; sözleşme buradadır)
5. Mevcut kod: `Assets/Scripts/Core/*.cs`, `Assets/Scripts/View/*.cs`

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`.
- Spec'te olmayan özellik ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator, kontrolden sonra atar. Push yok, dal değiştirme yok, git ayarına dokunma.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse Terminal panelinden çalıştır ve raporla.

## Aşama A: stub ve imza geçişi
1. Yeni tipler, dosya başına bir tip, `Assets/Scripts/Core/` altında: `CellKind.cs`, `Cell.cs`, `CellPos.cs`, `GroupFinder.cs`, `BlastRules.cs`. İmzalar, `CellKind` enum'u, `Cell.Empty` alanı ve `MinGroupSize` sabiti spec'teki gibi tam olsun.
2. Yeni tiplerde her metot, kurucu, özellik ve operatörün gövdesi yalnız `throw new System.NotImplementedException();`. Yalnız şu istisna: struct'lar alan tutabilir (`Cell` için tür ve renk, `CellPos` için x ve y), ama hiçbir üye onları okuyup mantık çalıştırmaz. `Cell.Empty` `default` kalır.
3. `Grid`: `Get` `Cell` döndürür, `Set` `Cell` alır; iç dizi `Cell[,]`. Sınır kontrolü aynen kalır. `TileColor?` ve `Set`'teki renk kontrolü kalkar (spec, "Sözleşme değişikliği").
4. `GridFactory`: hücreye `Cell.OfColor((TileColor)value)` yazar; başka değişiklik yok.
5. `GridView`: yalnız derlenecek kadar uyarla (`Cell` oku, boşsa atla, renkliyse `cell.Color`). Girdi ve `Refresh` Aşama B'de.
6. Sözleşmede olmayan public üye ekleme.
7. Kontrol: `dotnet build tests/Core/Core.csproj` (yalnız kural katmanı) ve `scripts/unity-check.ps1` hatasız. Test projesinin bu aşamada derlenmemesi beklenir (eski testler eski imzayı kullanıyor); onu çalıştırma, düzeltmeye çalışma.

**Rapor (Aşama A):** `reports/02-flood-fill-coder.md` dosyasına "Aşama A" başlığıyla: yazdığın ve değiştirdiğin dosyalar, iki kontrolün komutu ve sonuç satırları, spec'te belirsiz bulduğun yerler. Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Spec'teki davranışı uygula: `Cell` (eşitlik, `ToString`, hatalar), `CellPos`, `GroupFinder.FindGroup` (4 yönlü, sabit sıra, hata sırası), `BlastRules.Blast`.
2. Görüntü katmanı: spec'in "Görüntü katmanı" bölümündeki gibi girdi (dokunma önce, yoksa fare), ekran → hücre, `BlastRules.Blast`, `GridView.Refresh` (yeni doku ya da nesne oluşturmaz). Sahne dosyasına dokunma.
3. `tests/` klasörüne dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla.
4. Testi geçirmek için özel durum kodu yazma.
5. `scripts/build-webgl.ps1` ile WebGL derle (Terminal panelinde). Yayına alma; o orchestrator'ın işi.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (toplam/geçen/kalan), `unity-check` ve WebGL derleme sonuç satırları, yapamadıkların, emin olmadıkların, öneriler.
