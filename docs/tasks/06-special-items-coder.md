# Görev: Adım 6, özel öğeler (kodcu)

Model: Opus 5.5 (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/06-special-items` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/PLAN.md`: yalnız bölüm 2 (mimari).
3. `docs/specs/06-special-items.md` (tamamı; sözleşme burada).
4. Mevcut kod: `Assets/Scripts/Core/*.cs` (özellikle `Cell.cs`, `CellKind.cs`, `TurnResult.cs`, `TurnRules.cs`, `GameSession.cs`, `GroupFinder.cs`); Aşama B'de `Assets/Scripts/View/*.cs`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okuman da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma, paket ekleme.
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse durup raporla.

## Aşama A: stub
1. Yeni tipler, dosya başına bir tip, `Assets/Scripts/Core/` altında: `SpecialKind.cs`, `EffectKind.cs`, `ClearedCell.cs`, `TurnEffect.cs`, `SpecialRules.cs`. İmzalar spec'in "Arayüz sözleşmesi" bölümündeki gibi tam.
2. Mevcut tiplere eklenecek üyeler: `CellKind.Special`; `Cell.OfSpecial`, `Cell.OfOrb`, `IsSpecial`, `Special`, `OrbColor`; `TurnResult`'ın 4 parametreli kurucusu ve `Effects`. Enum değerleri ve sabitler (`SpecialRules.RocketMinGroup` vb.) tam yazılır.
3. Yeni metot, kurucu ve özelliklerin gövdesi yalnız `throw new System.NotImplementedException();`. İstisna: mevcut testlerin bozulmaması için mevcut davranış korunur: `TurnResult`'ın 3 parametreli kurucusu çalışmaya devam eder (`Effects` boş liste), `Cell`'in mevcut üyeleri ve eşitliği bugünkü testleri geçirmeye devam eder. Mevcut metotların (`TurnRules.Tap`, `GameSession.Tap`, `GroupFinder`, `BlastRules`) davranışını bu aşamada değiştirme.
4. Görüntü katmanına dokunma.
5. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız, `dotnet test tests/BlastGame.Tests.slnx` mevcut testler yeşil, `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/06-special-items-coder.md`, "Aşama A" başlığıyla: yazdığın/değiştirdiğin dosyalar, üç kontrolün komutu ve sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9 (kısa rapor). Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Kural katmanı, spec'in "Kurallar" bölümü: `Cell` eklemeleri (eşitlik, `GetHashCode`, `ToString`), `GroupFinder`/`BlastRules`'ta özel hücre, `SpecialRules.ForGroup` (yön çekilişi grid değişmeden önce, geçersiz değer `InvalidOperationException`), `SpecialRules.Activate` (her etki önce kendi hücresi, alan tarama sırası, FIFO zincir kuyruğu, `TriggeredBy`, her hücre tek `Cleared`'da), `TurnRules.Tap`'in beş durumu ve kaynak çekiliş sırası, `TurnResult.Blasted` = `Cleared` konumlarının sırayla birleşimi, `GameSession`'da renk başına sayım.
2. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: sonsuz oyun (`EndlessMode` ayarı, doğrudan `TurnRules.Tap`, hamle/hedef/panel gizli ama kodu silinmez, "Next board" butonu), özel öğelerin ilkel şekillerle çizimi ve hafif canlılık hareketi, cilalı ve hızlı animasyonlar (birleşme toplanması, Rocket uçuşu, Bomb dalgası, Orb ışınları), zincirde tetiklenen öğenin etki ona ulaştığı anda başlaması (`TriggeredBy` + konumdan zamanlama), düşme/doldurma hepsinden sonra. Geçici nesneler havuzdan; dokunuş başına doku ya da kalıcı nesne yok; "Next board"da sızıntı yok. Sahne dosyasına dokunma.
3. `tests/`'e dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla.
4. Testi geçirmek için özel durum kodu yazma.
5. WebGL derleme gerekmez; orchestrator commit'ten sonra temiz ağaçtan derler.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (toplam/geçen/kalan), `unity-check` sonuç satırı, animasyon sürelerinde spec'ten saptığın değerler, yapamadıkların, emin olmadıkların, öneriler.
