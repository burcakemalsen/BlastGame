# Görev: Adım 4, hamle, hedef, kazan/kaybet (kodcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/04-moves-goals` (zaten açık; dal değiştirme).

İki aşama var. **Şimdi yalnız Aşama A'yı yap.** Aşama B'yi orchestrator, testler yazıldıktan sonra mesajla başlatır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/PLAN.md`: yalnız bölüm 2 (mimari).
3. `docs/specs/04-moves-goals.md` (tamamı; sözleşme burada).
4. Mevcut kod: `Assets/Scripts/Core/*.cs` (özellikle `TurnRules.cs`, `TurnResult.cs`, `CellMove.cs` eşitlik örneği için); Aşama B'de `Assets/Scripts/View/*.cs`.

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/`. `tests/` klasörüne dokunmazsın, okuman da gerekmez.
- Kural 4: `Assets/Scripts/Core/` altında `UnityEngine` yok; C# 9 (`record`/`init` yok), `netstandard2.1`. Rastgelelik yalnız verilen `IRandomSource`'tan.
- Spec'te olmayan özellik ya da public üye ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; commit'i orchestrator atar. Push yok, dal değiştirme yok, git ayarına dokunma, paket ekleme (uGUI zaten eklendi).
- Unity batchmode komutlarını Editor kapalıyken çalıştır. Sandbox ortam hatası verirse Terminal panelinden çalıştır ve raporla.

## Aşama A: stub
1. Yeni tipler, dosya başına bir tip, `Assets/Scripts/Core/` altında: `GameStatus.cs`, `ColorGoal.cs`, `GameSession.cs`. İmzalar spec'teki gibi tam. `GameStatus` enum'ı tam yazılır (stub'ı yok).
2. Her metot, kurucu, özellik ve operatörün gövdesi yalnız `throw new System.NotImplementedException();`. İstisna: `ColorGoal` struct'ı alan tutabilir ama hiçbir üye onları okuyup mantık çalıştırmaz.
3. Mevcut tiplere ve görüntü katmanına dokunma.
4. Kontrol: `dotnet build tests/BlastGame.Tests.slnx` hatasız ve `scripts/unity-check.ps1` 0.

**Rapor (Aşama A):** `reports/04-moves-goals-coder.md`, "Aşama A" başlığıyla: yazdığın dosyalar, iki kontrolün komutu ve sonuç satırları, spec'te belirsiz bulduğun yerler. RULES kural 9 (kısa rapor). Son mesajında kısa özet.

## Aşama B: uygulama (orchestrator başlatınca)
1. Spec'in "Kurallar" bölümünü uygula: `ColorGoal`, `GameSession` (kurucu hata sırası, hamle yalnız patlamada, hedef 0'da durur, kazanma kaybetmeden önce, bitince `Tap` hiçbir şeye ve kaynağa dokunmaz, koordinat yine denetlenir, `goals` kopyalanır).
2. Görüntü katmanı, spec'in "Görüntü katmanı" bölümü: 9x9, 4 renk, 20 hamle, `Red x20, Blue x20` adlandırılmış sabitler; saatten seed (köşede ve Console'da); uGUI ile üst bar ve sonuç paneli ("You win!" / "Out of moves", "Play again"); dokunuşta `GameSession.Tap` + mevcut animasyon; panel animasyon bitince açılır, açıkken tahtaya dokunuş geçmez; "Play again" yeni seed, yeni grid, yeni oturum, sızıntı yok. Sahne dosyasına dokunma, her şey kodla kurulur.
3. `tests/`'e dokunmadan `dotnet test tests/BlastGame.Tests.slnx` yeşil, `scripts/unity-check.ps1` 0. Bir test yanlış görünüyorsa kodu ona göre bükme: dur ve nedenini raporla.
4. Testi geçirmek için özel durum kodu yazma.
5. WebGL derleme gerekmez; orchestrator commit'ten sonra temiz ağaçtan derler.

**Rapor (Aşama B):** aynı dosyaya "Aşama B" başlığıyla: değişen dosyalar, `dotnet test` özeti (toplam/geçen/kalan), `unity-check` sonuç satırı, tamamlanan hedefin nasıl gösterildiği, yapamadıkların, emin olmadıkların, öneriler.
