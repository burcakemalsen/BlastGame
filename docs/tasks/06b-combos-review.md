# Görev: Adım 6b, kombolar (Claude kontrolcü)

Model: Opus 5.5 medium (orchestrator kodu yeniden okumaz, bu rapora güvenir). Kendi git worktree'nde çalışırsın (`step/06b-combos`'tan açılmış kopya). Bilerek bozmayı yalnız kendi worktree'nde yapar, sonra geri alırsın. Gemini kontrolcü aynı anda, senden bağımsız çalışır.

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için" ve "Kontrolcüler".
2. `docs/specs/06b-combos.md` (tamamı, netleştirmeler dahil); gerekirse `docs/specs/06-special-items.md` "Kurallar".
3. Kod: `Assets/Scripts/Core/` altında `EffectKind.cs`, `TurnEffect.cs`, `SpecialRules.cs`, `TurnRules.cs`, `GameSession.cs`. Görüntü: `Assets/Scripts/View/GridView.cs`.
4. Testler: `test(core): add combo tests` commit'inin yeni dosyaları ve güncellediği üç mevcut dosya.
5. Raporlar: `reports/06b-combos-tester.md` ("Belirsizlikler"), `reports/06b-combos-coder.md`.

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx`: özet satırı.
2. **Spec karşılaştırması:** katılımcı bulma, kombo seçimi (güç sırası), `Cleared`/`MergedCount`, alan taramaları, `OrbOrb`, dönüşüm ve `Converted`, yön çekilişleri (grid değişmeden önce, doldurmadan önce), zincir, `GameSession` sayımı, sözleşme ve netleştirmeler.
3. **Kaçan uç durum** arama.
4. **Kural 4:** Core'da `UnityEngine`, `System.Random`, C# 9 üstü.
5. **Test dokunulmazlığı:** `git log --oneline main..HEAD -- tests/` yalnız `test(core)` commit'i; `feat(core)`/`feat(view)` commit'lerinde `tests/` boş. Testçinin 5 mevcut test güncellemesi sözleşmenin zorunlu kıldığı kadar mı, testlerin ölçtüğü şey korunmuş mu?
6. **Hile arama.**
7. **Bilerek bozma:** Core'u en az beş yerde boz (ör. güç sırasında iki satırı yer değiştir; katılımcı bulmaya çaprazı ekle ya da yalnız doğrudan komşuları al; `BigBomb` yarıçapını 2 yap; `RocketBomb`'u tek satır/sütun yap; `Converted`'ı sayımdan çıkar; yön çekilişini dönüşümden sonraya al; `OrbOrb`'da özel öğeleri tetikle; dönüşen öğelerin `TriggeredBy`'ını -1 yap). Her bozmadan sonra testleri çalıştır, yakalayan testi ya da "yakalanmadı" yaz, `git checkout -- Assets/` ile geri al.
8. **Görüntü:** çalıştıramazsın; okuyarak spec'in "Görüntü katmanı" bölümüyle karşılaştır (birleşme, altı kombo animasyonu, dönüşüm sonrası patlama, `TriggeredBy` zamanlaması, havuz, sızıntı).

## Rapor
`reports/06b-combos-claude.md`, **kendi worktree'nde** (commit atma; orchestrator kopyalar). En çok 400 kelime (bozma tablosu hariç): test özeti, bulgular (dosya:satır, spec maddesi, önem, yönlendirme), bozma tablosu, "sorun bulamadım" dediğin alanlarda ne denediğin. Kişi adı ya da kullanıcı yolu yazma. Son mesajında raporun tam yolu ve bulguların kısa listesi.
