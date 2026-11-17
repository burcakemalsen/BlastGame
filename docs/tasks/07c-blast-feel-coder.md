# Görev: Adım 7c, patlama hissi (kodcu)

Model: Opus 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/07c-blast-feel` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Kodcu" bölümleri.
2. `docs/specs/07c-blast-feel.md` (tamamı).
3. Mevcut kod: `Assets/Scripts/View/GridView.cs`, `HudView.cs`, `GameBootstrap.cs`, `ObstacleSprites.cs`. Kural katmanından yalnız gereken public imzalar (`TurnResult`, `TurnEffect`, `GameSession`).

## Rolün ve sınırlar
- Yazdığın yer yalnız `Assets/Scripts/View/`. Kural katmanına, `tests/`'e ve sahne dosyasına dokunma.
- Spec'te olmayan özellik ekleme; fikrin varsa raporda "öneri" olarak yaz.
- Commit atma; push yok, dal değiştirme yok, paket ekleme yok.
- Unity batchmode komutlarını Editor kapalıyken çalıştır.

## İş
Spec'in "Davranış" bölümündeki beş madde. Süreleri `GridView`/`HudView` başında adlı sabitler olarak topla ki ürün sahibi denedikten sonra kolayca ayarlansın. Hedef sayacının uçuş varışında azalması için HUD'un mevcut güncelleme yolunu bük ama kural katmanının sayımını değiştirme (gösterilen sayı animasyonla geride kalır, sonunda gerçek değere eşitlenir).

## Bitti
`dotnet test tests/BlastGame.Tests.slnx` yeşil (1226, değişmemiş), `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` 0.

## Rapor
`reports/07c-blast-feel-coder.md` (RULES kural 9): değişen dosyalar, iki kontrolün sonuç satırları, eklenen sabitler ve değerleri, spec'ten sapmalar, doğrulanmayanlar. Son mesajında kısa özet.
