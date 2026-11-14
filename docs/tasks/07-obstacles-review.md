# Görev: Adım 7, engeller (Claude kontrolcü)

Model: Opus 5.5 medium (orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın (`step/07-obstacles`'tan açılmış kopya). Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın. Gemini kontrolcü aynı anda, senden bağımsız çalışır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/07-obstacles.md` (tamamı, netleştirmeler dahil); gerekirse `docs/specs/06b-combos.md` "Kurallar".
3. Kod: `git diff main..HEAD --stat -- Assets/` ile değişen Core dosyaları; görüntüde `Assets/Scripts/View/GridView.cs`, `HudView.cs`, `GameBootstrap.cs`, `ObstacleSprites.cs`, `Assets/Editor/ObstacleArtImporter.cs`, `Assets/Resources/Levels/level04..07.json`.
4. Testler: `test(core): add obstacle tests` commit'inin dosyaları.
5. Raporlar: `reports/07-obstacles-tester.md` (belirsizlikler, "Ek"), `reports/07-obstacles-coder.md`.

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırı.
2. **Spec karşılaştırması:** engel hücresi ve balon, grup bağı, vuruşlar (grup komşusu tek vuruş, özel öğe alanı, roket engelden geçer, Orb komşuları ve kendi hücresi, kombolar ve `OrbRocket`/`OrbBomb` tek patlama, zincirde ayrı patlama), Su balonu, `Settle` (Sandık bölümleri, Çapa toplama), `Collapse`, `Fill` (en üst Sandığın üstü, çekiliş sayısı), `TurnRules.Tap` (engele dokunuş), `GameSession` sayım ve kazanma, `LevelParser`/`LevelData` (alanlar, denetim sırası, yollar), sözleşme ve netleştirmeler.
3. **Kaçan uç durum** arama (ör. Sandık üstünde Çapa sonra Sandık kırılınca, aynı turda kırılan Sandık ve doldurma, balonlu karenin özel öğe doğurması, Orb rengi balonlu kareler).
4. **Kural 4:** Core'da `UnityEngine`, `System.Random`, C# 9 üstü.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'i; `feat(core)`/`feat(view)` commit'lerinde `tests/` boş. Kodcunun notu: Adım 5 testi `Parse_BadGoals_MessageContainsGoalPath` `'goals[0].color'` bekliyor, netleştirme `'goals[0]'` diyor; kodcu mesaja ikisini de koydu. Bu bir hile mi, kabul edilebilir mi, değerlendir.
6. **Hile arama.**
7. **Bilerek bozma:** Core'u en az beş yerde boz (ör. grup komşu vuruşunu çoklu say; roketi engelde durdur; Sandık katını 1 yerine 2 düşür; `Fill`'i Sandık altına da uygula; Çapa toplamayı yalnız ilk Çapayla sınırla; `OrbRocket` alanlarında vuruşu her etkiye ayrı say; Orb kendi hücresinin komşusunu atla; parser denetim sırasında iki adımı yer değiştir). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
8. **Görüntü:** çalıştıramazsın; okuyarak spec'in "Görüntü katmanı" bölümüyle karşılaştır (level modu, HUD engel hedefleri, çizimler/sprite, animasyonlar, havuz, sızıntı, deneme levelleri geçerli mi).

## Rapor
`reports/07-obstacles-claude.md`, **kendi worktree'nde** (commit atma; orchestrator kopyalar). En çok 400 kelime (bozma tablosu hariç): test özeti, bulgular (dosya:satır, spec maddesi, önem, yönlendirme: kodcu ya da testçi), bozma tablosu, "sorun bulamadım" dediğin alanlarda ne denediğin. Kişi adı ya da kullanıcı yolu yazma. Son mesajında raporun tam yolu ve bulguların kısa listesi.
