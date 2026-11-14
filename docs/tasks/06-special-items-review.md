# Görev: Adım 6, özel öğeler (Claude kontrolcü)

Model: Opus 5.5 medium (orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın (`step/06-special-items`'den açılmış kopya). Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın. Gemini kontrolcü aynı anda, senden bağımsız çalışır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/06-special-items.md` (tamamı, netleştirmeler dahil).
3. Kod: `Assets/Scripts/Core/` altında `Cell.cs`, `CellKind.cs`, `SpecialKind.cs`, `EffectKind.cs`, `ClearedCell.cs`, `TurnEffect.cs`, `TurnResult.cs`, `SpecialRules.cs`, `TurnRules.cs`, `GroupFinder.cs`, `GameSession.cs`. Görüntü: `Assets/Scripts/View/GameBootstrap.cs`, `GridView.cs`, `HudView.cs`, `ShapeSprites.cs`.
4. Testler: `tests/Core.Tests/` altında adımın yeni dosyaları (`git show --stat` ile `test(core): add special item tests` commit'inden bul) ve güncellenen `TurnRulesTests.cs`, `GameSessionTests.cs`.
5. Raporlar: `reports/06-special-items-tester.md` ("Belirsizlikler"), `reports/06-special-items-coder.md`.

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** "Kurallar" (hücre, dokunuşun beş durumu, kaynak çekiliş sırası, etkiler ve alanları, zincir, hamle ve hedef), "Arayüz sözleşmesi" ve netleştirmeler kodla madde madde.
3. **Kaçan uç durum:** spec'in gerektirip testlerin kapsamadığı durumlar (ör. uzun zincir, aynı öğenin iki alanda kalması, Orb'un özel hücreleri atlaması, geçersiz yön çekilişinde grid'in değişmemesi, `TriggeredBy` doğruluğu, `Blasted` birleşim sırası).
4. **Kural 4:** Core'da `UnityEngine`, `System.Random`, C# 9 üstü özellik var mı.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'ini göstermeli; `feat(core)`/`feat(view)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı. Testçinin mevcut testlerdeki güncellemeleri (`TurnRulesTests`, `GameSessionTests`) gerçekten sözleşme değişikliğinin zorunlu kıldığı kadar mı; fazlası var mı?
6. **Hile arama:** testin beklediği değeri doğrudan döndüren özel durum kodu.
7. **Bilerek bozma:** Core'u en az beş ayrı yerde boz (ör. eşiği `>=` yerine `>` yap; Rocket taramasında sol/sağ sırasını değiştir; kuyruğu yığına (LIFO) çevir; Bomb'u kendi hücresini temizlemeden çalıştır; yön çekilişini doldurmadan sonra yap; Orb'un özel hücreleri de temizlemesine izin ver; renk başına sayımı dokunulan renge geri çevir). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
8. **Görüntü katmanı:** çalıştıramazsın; okuyarak spec'e uyumunu değerlendir: sonsuz oyun (`EndlessMode`, doğrudan `TurnRules.Tap`, hamle/hedef/panel gizli ama kod duruyor, "Next board"), zincir zamanlaması `TriggeredBy` ile, düşme etkilerden sonra, dokunuş başına doku/kalıcı nesne yok (havuz), "Next board"da sızıntı yok, animasyon sürerken dokunuş ve buton kapalı.

## Rapor
`reports/06-special-items-claude.md`, **kendi worktree'nde** (commit atma; orchestrator kopyalar). Rapor dışında worktree'de değişiklik bırakma. En çok 400 kelime (bozma tablosu hariç): test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem yüksek/orta/düşük, yönlendirme: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu, "sorun bulamadım" dediğin her alan için ne denediğin. Kişi adı ya da kullanıcı yolu yazma. Son mesajında raporun tam yolunu ve bulguların kısa listesini yaz.
