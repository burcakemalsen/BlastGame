# Görev: Adım 5, JSON level (kodcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/05-json-level` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/PLAN.md`: yalnız bölüm 2 (mimari).
3. `docs/specs/05-json-level.md` (tamamı; sözleşme burada).
4. Mevcut kod: `Assets/Scripts/Core/*.cs` (özellikle `GameSession.cs`, `ColorGoal.cs`, `GridFactory.cs`); Aşama B'de `Assets/Scripts/View/*.cs`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okuman da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Newtonsoft.Json paketi zaten eklendi (Unity paketi, `Core.asmdef` başvurusu, test projesinde NuGet). Yalnız `LevelParser` içinde kullan; public imzalarda Newtonsoft tipi olmaz.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma, paket ekleme.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse durup raporla.

## Aşama A: stub
1. Yeni tipler, dosya başına bir tip, `Assets/Scripts/Core/` altında: `LevelFormatException.cs`, `LevelData.cs`, `LevelParser.cs`. İmzalar spec'teki gibi tam.
2. `LevelFormatException`'ın iki kurucusu tam yazılır (yalnız `base(...)` çağırır; stub'ı yok). Diğer her metot, kurucu ve özelliğin gövdesi yalnız `throw new System.NotImplementedException();`.
3. Level dosyalarını bu aşamada yazma; mevcut tiplere ve görüntü katmanına dokunma.
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız ve `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/05-json-level-coder.md`, "Aşama A" başlığıyla: yazdığın dosyalar, iki kontrolün komutu ve sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9 (kısa rapor). Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Spec'in "Kurallar" ve "`LevelData`" bölümlerini uygula: `JObject`/`JToken` ile alan alan okuma (reflection ile otomatik çevirme yok), tekrarlanan anahtar hata (`JsonLoadSettings.DuplicatePropertyNameHandling = Error`), sözdizimi hatasında kütüphane hatası `InnerException`, alan yolu tek tırnakla mesajda, spec'teki denetim sırası, `int` taşması ve ondalık sayı hata, `LevelData` listeyi kopyalar, `CreateSession` aynı kaynağı grid ve oturuma verir.
2. Level dosyaları: `Assets/Resources/Levels/level01.json`, `level02.json`, `level03.json`, spec'teki tablodaki değerlerle.
3. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: `GameBootstrap`'taki sabit level değerleri kalkar; `Resources.LoadAll<TextAsset>("Levels")`, ada göre ordinal sıralama, hepsi başta okunur; üst barda `Level N`; panel akışı "Next level" / "Try again" / "All levels done!" + "Play again"; bozuk ya da eksik dosyada okunaklı hata paneli (`level02.json: <mesaj>`, `No level files found.`) ve `Debug.LogError`; saatten seed Adım 4'teki gibi; level geçişinde sızıntı yok. Sahne dosyasına dokunma.
4. `tests/`'e dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla.
5. Testi geçirmek için özel durum kodu yazma.
6. WebGL derleme gerekmez; orchestrator commit'ten sonra temiz ağaçtan derler.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (toplam/geçen/kalan), `unity-check` sonuç satırı, yapamadıkların, emin olmadıkların, öneriler.
