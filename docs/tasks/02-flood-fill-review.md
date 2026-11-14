# Görev: Adım 2, grup bulma ve patlatma (Claude kontrolcü)

Model: Sonnet 5.5 medium. Kendi git worktree'nde çalışırsın (adımın dalı `step/02-flood-fill`'den açılmış kopya). Proje dosyalarını değiştirmezsin; bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın.

## Önce oku
1. `CLAUDE.md`, `docs/RULES.md` ("Herkes için", "Onay", "Kontrolcüler")
2. `docs/specs/02-flood-fill.md` (tamamı)
3. Kod: `Assets/Scripts/Core/*.cs`, `Assets/Scripts/View/*.cs`
4. Testler: `tests/Core.Tests/*.cs`

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırını aktar.
2. **Spec karşılaştırması:** spec'in "Sözleşme değişikliği", "Grup bulma", "Patlatma" ve "Görüntü katmanı" maddelerini kodla tek tek karşılaştır; uymayanı yaz.
3. **Kaçan uç durum:** testlerin kapsamadığı ama spec'in gerektirdiği durumları ara.
4. **Kural 4:** `Assets/Scripts/Core/` altında `UnityEngine` ya da `System.Random` kullanımı, C# 9 üstü dil özelliği var mı.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` testlerin yalnız `test(core)` commit'inde değiştiğini göstermeli; `feat(core)`/`feat(view)` commit'lerinde `git show --stat <commit> -- tests/` boş olmalı.
6. **Sözleşme geçişi:** `test(core)` commit'inde Adım 1 testlerinin değişikliği (`git show <test-commit> -- tests/Core.Tests/GridTests.cs tests/Core.Tests/GridFactoryTests.cs`) yalnız `TileColor?` → `Cell` geçişi mi, silinen ya da anlamı zayıflayan test var mı.
7. **Bilerek bozma:** Core kodunu en az üç ayrı yerde boz (ör. komşuluğa çapraz ekle ya da bir yönü çıkar, `MinGroupSize` karşılaştırmasını `>` yap, sonuç sırasını değiştir, `Cell.Equals`'ta renk karşılaştırmasını kaldır, boş başlangıç kontrolünü kaldır). Her bozmadan sonra testleri çalıştır, yakalayan testin adını ya da "yakalanmadı" yaz, sonra `git checkout -- Assets/` ile geri al. Yakalanmayan bozma = zayıf test.
8. Görüntü katmanını çalıştıramazsın; yalnız okuyarak spec'e uyumunu değerlendir (girdi: dokunma önce, ekran → hücre yuvarlama, `Refresh` yeni nesne/doku üretmiyor).

## Rapor
Rapor yolu oturum mesajında verilir (worktree'den asıl klasöre yazılamıyor; orchestrator `reports/02-flood-fill-claude.md`'ye kopyalar). En çok 400 kelime: test özeti, bulgular (her biri: dosya:satır, spec maddesi, önem: yüksek/orta/düşük, öneri: kod hatası → kodcu, eksik test → testçi), bilerek bozma tablosu (bozma, yakalayan test ya da "yakalanmadı"), "sorun bulamadım" dediğin her alan için ne denediğin. Commit atma; worktree'de değişiklik bırakma.
