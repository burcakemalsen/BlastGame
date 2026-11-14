# Görev: Adım 5, JSON level (Claude kontrolcü)

Model: Opus 5.5 medium (ürün sahibi kararı: orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın. **İlk iş:** `git reset --hard step/05-json-level` (worktree başka bir daldan açılmış olabilir); sonra `git log --oneline -1` ile doğru commit'te olduğunu yaz. Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/05-json-level.md` (tamamı).
3. Kod: `Assets/Scripts/Core/` altında `LevelFormatException.cs`, `LevelData.cs`, `LevelParser.cs`, `Core.asmdef`; gerekirse çağırdıkları mevcut tipler (`GameSession`, `GridFactory`, `ColorGoal`). Görüntü: `Assets/Scripts/View/GameBootstrap.cs`, `HudView.cs`. Level dosyaları: `Assets/Resources/Levels/*.json`.
4. Testler: `tests/Core.Tests/LevelParserTests.cs`, `LevelDataTests.cs`, `LevelFilesTests.cs`.
5. Raporlar: `reports/05-json-level-tester.md` (belirsizlikler, düzeltme) ve `reports/05-json-level-coder.md` (Aşama B, doğrulanmayanlar).

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** "Dosya biçimi", "Kurallar" (denetim sırası dahil), "`LevelData`", "Arayüz sözleşmesi", "Görüntü katmanı" maddelerini kodla tek tek karşılaştır. Level dosyalarının değerleri spec tablosuyla aynı mı.
3. **Kaçan uç durum:** spec'in gerektirip testlerin kapsamadığı durumlar (ör. `int` taşması, `9.0`, tekrarlanan anahtar, kökten sonra fazladan metin, iç içe bilinmeyen alan).
4. **Kural 4 ve WebGL:** Core'da `UnityEngine`, `System.Random`, saat, C# 9 üstü özellik var mı. Newtonsoft yalnız `LevelParser` içinde ve public imzada görünmüyor mu; reflection ile otomatik çevirme (`JsonConvert.DeserializeObject<T>`, `ToObject<T>`) kullanılmamış mı (WebGL budaması riski).
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'lerini göstermeli; `feat(...)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı. Önceki adımların testlerine dokunulmamış olmalı. Tek istisna: ürün sahibinin onayladığı `LevelDataTests` `default(ColorGoal)` düzeltmesi (`test(core)` commit'i); diff'inin yalnız o satır olduğunu doğrula.
6. **Bilerek bozma:** Core'u en az dört ayrı yerde boz (ör. aralık sınırını `<=` yerine `<` yap; bilinmeyen alan denetimini kaldır; tekrar eden hedef rengi denetimini kaldır; denetim sırasını değiştir; `goals` listesini kopyalama; ondalık sayıyı kabul et; hata yolunda indeksi yanlış yaz). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
7. **Görüntü katmanı:** çalıştıramazsın; okuyarak spec'e uyumunu değerlendir: dosyalar ordinal sırayla mı yükleniyor; bozuk ya da eksik dosyada çökme yerine hata paneli ve `Debug.LogError` var mı; "Next level" / "Try again" / "All levels done!" + "Play again" akışı doğru mu; level geçişinde nesne sızıntısı var mı; üç hedef barda sığacak gibi mi.

## Rapor
`reports/05-json-level-claude.md`, **kendi worktree'nde** (asıl klasöre yazamazsın; commit atma, orchestrator kopyalar). Rapor dışında worktree'de değişiklik bırakma. En çok 400 kelime: test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem yüksek/orta/düşük, yönlendirme: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu, "sorun bulamadım" dediğin her alan için ne denediğin. Son mesajında raporun tam yolunu yaz.
