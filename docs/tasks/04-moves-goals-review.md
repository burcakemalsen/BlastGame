# Görev: Adım 4, hamle, hedef, kazan/kaybet (Claude kontrolcü)

Model: Opus 5.5 medium (ürün sahibi kararı: orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın (`step/04-moves-goals`'tan açılmış kopya). Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/04-moves-goals.md` (tamamı).
3. Kod: `Assets/Scripts/Core/` altında `GameStatus.cs`, `ColorGoal.cs`, `GameSession.cs`; gerekirse çağırdıkları mevcut tipler (`TurnRules`, `TurnResult`). Görüntü: `Assets/Scripts/View/GameBootstrap.cs`, `HudView.cs`, `CameraFitter.cs`, `View.asmdef`.
4. Testler: `tests/Core.Tests/ColorGoalTests.cs`, `GameSessionTests.cs`.
5. Raporlar: `reports/04-moves-goals-tester.md` (belirsizlikler) ve `reports/04-moves-goals-coder.md` (Aşama B, doğrulanmayanlar).

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** "Kurallar", "Kurucu hataları", "`ColorGoal`", "Arayüz sözleşmesi" ve "Görüntü katmanı" maddelerini kodla tek tek karşılaştır.
3. **Kaçan uç durum:** spec'in gerektirip testlerin kapsamadığı durumlar.
4. **Kural 4:** Core'da `UnityEngine`, `System.Random`, saat (`DateTime`), C# 9 üstü özellik var mı. Seed saatten yalnız görüntü katmanında seçilmeli.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'lerini göstermeli; `feat(core)`/`feat(view)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı. Testçi Adım 1-3 testlerine dokunmamış olmalı.
6. **Bilerek bozma:** Core'u en az dört ayrı yerde boz (ör. boşa dokunuşta da hamle düşür; hedefi 0'da durdurma; kazanmayı kaybetmeden sonra kontrol et; oyun bitince `Tap`'i `TurnRules`'a geçir; `goals`'u kopyalama; kurucu hata sırasını değiştir; `ColorGoal.Equals`'ta `Count`'u yok say). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
7. **Görüntü katmanı:** çalıştıramazsın; okuyarak spec'e uyumunu değerlendir: panel animasyon bitince açılıyor ve açıkken tahtaya dokunuş geçmiyor mu; "Play again" yeni seed ve yeni oturum kuruyor, eski nesneler sızmıyor mu; dokunuş başına yeni doku/nesne yok mu; seed köşede ve Console'da mı; WebGL'de çalışmayabilecek bir şey (font, `EventSystem`/girdi modülü) var mı.

## Rapor
`reports/04-moves-goals-claude.md`, **kendi worktree'nde** (asıl klasöre yazamazsın; commit atma, orchestrator kopyalar). Rapor dışında worktree'de değişiklik bırakma. En çok 400 kelime: test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem yüksek/orta/düşük, yönlendirme: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu, "sorun bulamadım" dediğin her alan için ne denediğin. Son mesajında raporun tam yolunu yaz.
