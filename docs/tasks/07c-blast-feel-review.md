# Görev: Adım 7c, patlama hissi (Claude kontrolcü)

Model: Opus 5.5 medium. Yalnız okursun; hiçbir proje dosyasını değiştirmezsin (rapor hariç). Görüntü adımı: bilerek bozma yok, Gemini yok.

## Önce oku
1. `docs/RULES.md`: "Herkes için" ve "Kontrolcüler".
2. `docs/specs/07c-blast-feel.md`.
3. `git diff main..HEAD -- Assets/` ve `reports/07c-blast-feel-coder.md`.

## İşler
1. `dotnet test tests/BlastGame.Tests.slnx` özet satırı.
2. `git diff main..HEAD --stat -- Assets/Scripts/Core tests` boş mu.
3. Spec'in beş maddesini kodla karşılaştır: halka sırası ve süre sınırı, engel vuruşunun ilk komşu patlamasında görünmesi, hedefe uçuş (yalnız hedefe sayılanlar, küçülme, ease-in, sayaç varışta azalır ve sonunda gerçek değere eşitlenir, sonsuz modda yok), dökülme kademesi ve maske, kazan/kaybet zamanlaması.
4. Havuz, sızıntı, level değişiminde/“Levels” ve “Next level” sırasında yarım kalan uçuşlar, HUD sayacının yanlış kalabileceği durumlar.

## Rapor
`reports/07c-blast-feel-claude.md`, kendi worktree'nde (commit yok). En çok 300 kelime: test özeti, bulgular (dosya:satır, madde, önem), ne denediğin. Kişi adı ya da kullanıcı yolu yazma. Son mesajında raporun tam yolu ve bulguların kısa listesi.
