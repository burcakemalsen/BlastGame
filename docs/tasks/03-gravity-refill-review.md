# Görev: Adım 3, düşme ve doldurma (Claude kontrolcü)

Model: Opus 5.5 medium (ürün sahibi kararı, 2026-11-14: orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın (`step/03-gravity-refill`'den açılmış kopya). Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/03-gravity-refill.md` (tamamı).
3. Kod: `Assets/Scripts/Core/` altında `CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs`; gerekirse çağırdıkları mevcut tipler. Görüntü: `Assets/Scripts/View/GameBootstrap.cs`, `GridView.cs`.
4. Testler: `tests/Core.Tests/` altında `CellMoveTests.cs`, `GravityRulesTests.cs`, `RefillRulesTests.cs`, `TurnResultTests.cs`, `TurnRulesTests.cs`, `TurnTestHelpers.cs`.
5. Testçi raporu `reports/03-gravity-refill-tester.md` ("Belirsizlikler" bölümü).

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** "Düşme", "Doldurma", "Bir dokunuş", "Arayüz sözleşmesi" ve "Görüntü katmanı" maddelerini kodla tek tek karşılaştır.
3. **Kaçan uç durum:** spec'in gerektirip testlerin kapsamadığı durumlar. Testçinin açık bıraktığı: `Fill`/`Tap`'te aynı türdeki hataların sırası (`grid`/`random`, `colorCount`/koordinat) `ParamName` ile sınanabilir mi.
4. **Kural 4:** Core'da `UnityEngine`, `System.Random`, C# 9 üstü özellik var mı.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'ini göstermeli; `feat(core)`/`feat(view)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı. Testçi mevcut testlere dokunmamış olmalı (`git show --stat <test-commit>` yalnız yeni dosyalar).
6. **Bilerek bozma:** Core'u en az dört ayrı yerde boz (ör. `Collapse`'ta sırayı ters çevir ya da yerinde kalanı listeye ekle; `Fill`'de sırayı x-önce yap ya da değeri yazdıktan sonra denetle; `Tap`'te patlama olmasa da `Fill` çağır; hata kontrol sırasını değiştir; `CellMove.Equals`'ta `To`'yu yok say). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
7. Görüntü katmanını çalıştıramazsın; okuyarak spec'e uyumunu değerlendir (tek `SeededRandom`, `TurnRules.Tap` + `Refresh`, sprite bir kez üretiliyor).

## Rapor
`reports/03-gravity-refill-claude.md`, **kendi worktree'nde** (asıl klasöre yazamazsın; commit atma, orchestrator kopyalar). Rapor dışında worktree'de değişiklik bırakma. En çok 400 kelime: test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem yüksek/orta/düşük, yönlendirme: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu, "sorun bulamadım" dediğin her alan için ne denediğin. Son mesajında raporun tam yolunu yaz.
