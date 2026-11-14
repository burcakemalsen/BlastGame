# Görev: Adım 7, engeller (testçi)

Model: Opus 5.5 medium (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/07-obstacles` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/07-obstacles.md`: "Engeller", "Kurallar", "Örnekler", "Uç durumlar", "Arayüz sözleşmesi" (sözleşme değişikliği notu dahil). Görüntü katmanı test edilmez. Gerekirse `docs/specs/06b-combos.md` ve `docs/specs/05-json-level.md`'nin kural bölümleri.
3. Stub imzaları: `Assets/Scripts/Core/` altındaki yeni ve değişen tiplerin public imzaları. Uygulama gövdelerini okuma.
4. Mevcut yardımcılar: `tests/Core.Tests/SpecialGrid.cs`, `TestGrid.cs`, `TurnTestHelpers.cs`. Kullan; gerekirse yeni dosyada genişlet (engel ve balon için okunaklı grid kurucu).

## Rolün
Amacın kodu kırmak. Spec'in söylemediği davranışı tahmin etme: belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. Mevcut testleri değiştirme ve silme; tek istisna spec'in sözleşme değişikliği notunun zorunlu kıldığı güncellemeler (her birini raporda tek tek yaz). Paket ekleme, commit yok, push yok.

## Ölçü
Yeni test için kaba üst sınır **~150** (`Theory` satırları dahil). Bir test bir davranış sınıfı.

## İşler
1. **Yeni testler**, konu başına sınıf (ör. `ObstacleCellTests`, `ObstacleHitGroupTests`, `ObstacleHitSpecialTests`, `WaterBalloonTests`, `GravitySettleTests`, `RefillCrateTests`, `TurnRulesObstacleTests`, `GameSessionObstacleGoalTests`, `LevelParserObstacleTests`, `LevelDataObstacleTests`, değer tipleri için eşitlik testleri). "Uç durumlar" listesi en az kapsamdır.
2. **Örnekler birebir:** Örnek 1-6 (`Hits`, `Cleared`, `Moves`, `Collected`, `Filled`, hedef sayımı).
3. **Sıra sözleşmesi liste eşitliğiyle:** `Hits` (`y` sonra `x`), `Moves`, `Collected`, `Filled` ve doldurma çekiliş sayısı.
4. **Vuruş sayısı:** tek patlamada aynı engele 1 vuruş; `OrbRocket`/`OrbBomb` alan birleşiminde 1 vuruş; zincirde ayrı patlamadan ikinci vuruş; roket engelden geçip satır sonuna gider ve 3 katlı Sandık 2'ye düşer.
5. **Hata türleri tam;** `ParamName` yazılıysa kontrol et. `LevelParser` hatalarında mesajın alan yolunu içerdiğini ve denetim sırasını sına.
6. **Tekrarlanabilirlik:** aynı seed ve aynı dokunuşlar aynı grid, etki ve toplanma sonucunu verir (engelli bir level'la).
7. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Yeni testler kırmızı, mevcut testler yeşil olmalı. Stub'la geçen yeni test varsa incele ve raporla.
8. **Düzen:** `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9; grid'ler okunaklı (`y` alttan üste).

## Rapor
`reports/07-obstacles-tester.md` (kısa): dosyalar, yeni test sayısı, **spec maddesi → test eşlemesi**, `dotnet test` özeti aynen, mevcut testlerde yapılan güncellemeler, belirsizlikler. Son mesajında kısa özet.
