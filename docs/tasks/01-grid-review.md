# Görev: Adım 1, Grid (Claude kontrolcü)

Model: Sonnet 5.5 medium. Kendi git worktree'nde çalışırsın (adımın dalı `step/01-grid`'den açılmış kopya). Proje dosyalarını değiştirmezsin; bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın.

## Önce oku
1. `CLAUDE.md`, `docs/RULES.md` ("Herkes için", "Kontrolcüler")
2. `docs/specs/01-grid.md` (tamamı)
3. Kod: `Assets/Scripts/Core/*.cs`, `Assets/Scripts/View/*.cs`
4. Testler: `tests/Core.Tests/*.cs`

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** spec'in her "Davranış" maddesini ve "Görüntü katmanı" maddesini kodla karşılaştır; uymayanı yaz.
3. **Kaçan uç durum:** testlerin kapsamadığı ama spec'in gerektirdiği durumları ara.
4. **Kural 4:** `Assets/Scripts/Core/` altında `UnityEngine` ya da `System.Random` kullanımı, C# 9 üstü dil özelliği var mı.
5. **Test dokunulmazlığı:** kod commit'lerinde `tests/` değişmiş mi: `git log --oneline -- tests/` ile testlerin yalnız `test(core)` commit'lerinde değiştiğini kontrol et; `feat(core)`/`fix(core)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı.
6. **Bilerek bozma:** Core kodunu en az üç ayrı yerde boz (ör. `>=` yerine `>`, doldurma sırasını ters çevir, SplitMix64 sabitini değiştir, sınır kontrolünü kaldır). Her bozmadan sonra testleri çalıştır, yakalayan testin adını ya da "yakalanmadı" yaz, sonra `git checkout -- Assets/` ile geri al. Yakalanmayan bozma = zayıf test.
7. Görüntü katmanını çalıştıramazsın; yalnız okuyarak spec'e uyumunu değerlendir.

## Rapor
`C:\Dev\BlastGame\reports\01-grid-claude.md` (asıl klasöre, worktree'ye değil). En çok 400 kelime: test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem: yüksek/orta/düşük, öneri: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu (bozma, yakalayan test ya da "yakalanmadı"), "sorun bulamadım" dediğin her alan için ne denediğin. Commit atma; worktree'de değişiklik bırakma.
