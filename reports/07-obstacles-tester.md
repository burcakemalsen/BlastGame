# Adım 7, engeller: testçi raporu

## Dosyalar (hepsi yeni, `tests/Core.Tests/`)
- `ObstacleGrid.cs`: yardımcı. Boşlukla ayrılmış okunaklı grid (`R C2 P A r *R H`), dump, `Hits` biçimi (`"1,1 C2>C1"`), `ThenZeros` kaynağı.
- `ObstacleCellTests.cs`, `ObstacleValueTypeTests.cs` (ObstacleHit/Goal/Placement, TurnEffect/TurnResult/GravityResult eklentileri), `GravitySettleTests.cs` (içinde `RefillCrateTests`), `TurnRulesObstacleTests.cs`, `ObstacleHitSpecialTests.cs`, `GameSessionObstacleTests.cs`, `LevelDataObstacleTests.cs`, `LevelParserObstacleTests.cs`.

Yeni test: **169** (`Theory` satırları dahil). Bu, ~150 sınırının üstünde. Fazlası `LevelParser`'dan geliyor (40 satır): spec her yeni hata yolunu ve denetim sırasını ayrı istiyor. Mevcut testlerde değişiklik yok. Sözleşme notu gereği güncellenmesi gereken test çıkmadı.

## `dotnet test`
```
Başarısız! - Başarısız:   161, Başarılı:  1036, Atlanan:     0, Toplam:  1197
```
Mevcut 1028 testin hepsi yeşil. Yeni testlerden 161'i kırmızı (97 `NotImplementedException`, gerisi stub'ın yanlış hata türü ya da parser'ın `Unknown field 'obstacles'` hatası). Stub'la geçen 8 yeni test var:
- Doğru oldukları için geçenler: eski kurucularda `Hits`/`Collected`/`Obstacles`/`ObstacleGoals` boş (4), `HasBalloon` false, `obstacles` yokken eski dosya.
- **Zayıf, raporluyorum:** `Parse_BadObstacle_MessageHasFieldPath("5", "'obstacles'")` ve denetim sırası satırı `obstacles=5, goals=[]`. Stub `Unknown field 'obstacles'.` dediği için bu ikisi yanlış nedenle geçiyor. Mesaj metni sözleşme olmadığı için ayırt edemiyorum. Diğer `obstacles` testleri bu açığı kapatıyor.

## Spec maddesi → test
- Hücre, fabrika/erişimci/balon/eşitlik: `ObstacleCellTests` (hepsi).
- Değer tipleri, kurucu denetimleri, liste kopyalanmaz: `ObstacleValueTypeTests`.
- Engel dokunulmaz, grup bağını koparır: `Tap_ObstacleCell_DoesNothing` (3 tür), `Tap_ObstacleBetweenSameColors_BreaksGroup`. Balonlu ve balonsuz aynı grup: `FindGroup_BalloonAndPlainSameColor_AreOneGroup`.
- Örnek 1: `Tap_Example1_...`, `GameSession...Tap_Example1_...`. Tek patlamada tek vuruş, daha güçlü sınama: `Tap_CrateNextToTwoGroupCells_LosesOnlyOneLayer`.
- Örnek 2: `Tap_Example2_CrateLayerAndSegments`. Örnek 3: `Tap_ExampleThree_*` (2 test). Örnek 4: `Tap_Example4_*` (TurnRules ve GameSession). Örnek 5: `Tap_Example5_*` (2). Örnek 6: `Tap_Example6_RocketPassesObstaclesToRowEnd`.
- Köşe, çapraz, `Hits` sırası (`y` sonra `x`): `Tap_CornerCrate_*`, `Tap_DiagonalObstacle_*`, `Tap_GroupHitsTwoObstacles_HitsOrderedByYThenX`.
- Sandık 3→2→1→yok, sonra bölümler birleşir: `Tap_CrateThreeTimes_*`. Roketle 3 kat → 2: `Tap_VerticalRocketThroughThreeLayerCrate_*`.
- Balon ve Çapa komşu patlamadan etkilenmez: `Tap_NeighborBalloonAndAnchor_AreNotAffected`.
- Bomb alanı, Çapa alanda kalır: `Tap_Bomb_*`. Roket yalnız kendi hattını vurur: `Tap_Rocket_DoesNotHitObstaclesBesideItsLine`. Orb komşu vuruşu ve balon: `Tap_Orb_*`.
- Kombolar: `Tap_RocketCross_*`, `Tap_BigBomb_*`, `Tap_OrbOrb_*` (Çapa dibe iner ve toplanır, Sandık altı boş kalır). `OrbRocket` ve `OrbBomb` üst üste binen alanlarda vuruş tek, ilk etkide: `Tap_OrbRocketOverlapping...`, `Tap_OrbBombOverlapping...`.
- Zincirde ikinci vuruş, kırılan engelin yeri boş: `Tap_BombThenChainedRocket_*` (2 satır). Engel kombo bağını koparır: `Tap_ObstacleBetweenSpecials_*`.
- Balon: roket alanında `Tap_RocketOverBalloon_*`; dönüşümde `Tap_OrbRocketConvertsBalloonSquare_*`; özel öğe doğan hücrede `Tap_BalloonOnTappedCellOfBigGroup_*`.
- Settle/Collapse: `GravitySettleTests` (Sandık üstü, iki Sandık, Pinyata ve balonlu kare düşer, Çapa toplanır, Çapa zaten `y=0`'da, araya boşluk girmiş ardışık Çapalar, Çapa karenin üstünde, Çapa Sandığın üstünde, iki sütunda `Collected`/`Moves` sırası, engelsiz grid'de Collapse ile aynı sonuç, Collapse Sandığa uyar ve Çapa toplamaz).
- Fill: `RefillCrateTests` (Sandık altı doldurulmaz, iki Sandık, çekiliş sırası ve sayısı, Pinyata/Çapa doldurmayı durdurmaz).
- GameSession: kurucu sırası (Netleştirme), yalnız engel hedefi, `Remaining` hataları, dört türün sayımı, dönüşen balonlu karenin bir kez sayılması, 0'ın altına inmeme, engele dokunuş hamle harcamaz, kazanma ve kaybetme: `GameSessionObstacleTests`.
- LevelData: `LevelDataObstacleTests` (hatalar, denetim sırası, kopya, `CreateSession` yerleşimi). Tekrarlanabilirlik: `CreateSession_SameSeedSameTaps_*`.
- LevelParser: geçerli örnek, varsayılan `layers`, `obstacles` yok ya da boş, yalnız engel hedefi, her hata yolu, 10 denetim sırası satırı: `LevelParserObstacleTests`.

## Belirsizlikler (tahmin etmedim, test dışı bıraktım)
1. Orb'un **kendi hücresinin** komşusundaki engel vurulur mu ("temizlenen kareler" Orb'u da kapsıyor mu)? Testlerde Orb'un yanına engel koymadım.
2. `OrbRocket` dönüşümünde, dönüşen karenin rokete düşmeyen komşusu (yatay roketin üstü ya da altı) vurulur mu? Kaçındım.
3. `SpecialRules.Activate`/`Tap` ve `BlastRules.Blast` doğrudan çağrılınca engel vurup `Hits` dolduruyor mu? Spec bunu yalnız `TurnRules.Tap` için söylüyor. Testler `TurnRules` düzeyinde.
4. `GroupFinder.FindGroup` engel hücresinden başlatılırsa ne döner? Yazılmamış. Yalnız `TurnRules` ile sınadım.
5. Hata mesajındaki yol: `Anchor` `y = 0` ve `goals` elemanında "ikisi birden ya da hiçbiri" için yol tanımlı değil. Testler tırnaksız `obstacles[0]` / `goals[0]` geçtiğini sınıyor.
6. `GameSession` 6 parametreli kurucuda `obstacleGoals` "tanımsız tür": `ObstacleGoal` kurucusu tanımsız türü reddettiği için bu durum kurulamıyor. Test yok.
7. `RocketBomb` alanında vuruş için ayrı test yok (`RocketCross` ve `BigBomb` var). Kontrolcü bozma denemesinde bakabilir.

## Ek: Netleştirmeler 1-5 ve RocketBomb (spec commit 909b2a8)
Yeni dosya `tests/Core.Tests/ObstacleClarificationTests.cs`, 9 test:
- Orb'un kendi hücresinin komşusu vurulur: `Tap_SingleOrb_HitsObstacleNextToItsOwnCell`.
- Dönüşüm vuruş yapmaz: `Tap_OrbRocketConversion_DoesNotHitNeighborOfConvertedSquare`.
- Vuruş etkiyi üreten yerde uygulanır: `Activate_RocketThroughCrate_FillsHitsAndAppliesToGrid`, `SpecialRulesTap_ComboThroughCrate_FillsHitsAndAppliesToGrid`, `Blast_GroupNextToObstacles_DoesNotHitThem`. Sonuncusu, `Cell` engel fabrikası yazılınca geçmesi beklenen bir gerileme korumasıdır; şimdi yalnız grid kurulumundaki stub hatası yüzünden kırmızı.
- `FindGroup` engelden başlayınca boş liste: `FindGroup_FromObstacleCell_ReturnsEmpty` (3 satır: Pinyata, Sandık, Çapa).
- RocketBomb alanı: `Tap_RocketBomb_HitsObstaclesInThreeRowsAndColumnsOnly`.

Sıkılaştırılan iki test (bu adımda yazdıklarım): `Parse_AnchorOnBottomRow_MessageHasYPath` artık `'obstacles[0].y'` arıyor; `Parse_GoalWithBothOrNeitherKindField_ThrowsWithGoalPath` artık `'goals[0]'` arıyor (tırnaklı). Belirsizlik 1-5 ve 7 kapandı.

Toplam yeni test 178. Mevcut 1028 testin hepsi yeşil, mevcut testlere dokunulmadı.
```
Başarısız! - Başarısız:   170, Başarılı:  1036, Atlanan:     0, Toplam:  1206
```
Ek'teki 9 test kırmızı. Stub'la geçen yeni testler yine aynı 8 test.

## Ek 2: kontrol bulguları
Uygulama kodunu okumadım. Testleri yalnız spec'e ve iki kontrol raporundaki bulgu tanımlarına göre yazdım. 11 yeni test:
- **Denetim sırası** (`Parse_TwoErrors_FirstInCheckOrderIsReported`, 3 yeni satır):
  - aynı hücreye Pinyata, sonra `y = 0`'da Çapa → yol `'obstacles[1].y'`, mesajda `'obstacles[1]'` yok (Claude S8);
  - engel hedefi tekrarı ve `count 0` birlikte → `'goals[1].obstacle'`;
  - `color` ve `obstacle` birlikte, `count 0` → `'goals[0]'`.
- **Negatif koordinat:** parser'da `x = -1` ve `y = -1` (2 satır), `LevelData`'da `x = -1` ve `y = -1` (2 satır, `ArgumentException(obstacles)`).
- **Uçtan uca** (`ObstacleReviewFollowUpTests.cs`): Çapa Sandığın üstünde, Sandık aynı dokunuşta kırılıyor, Çapa aynı turda dibe inip toplanıyor. `Hits`, `Collected`, `Moves`, iki hedefin sayımı ve kazanma kontrol ediliyor.
- **Balon sayımı:** bir grupta iki balon ikisi de sayılıyor; `OrbOrb`'da tahtadaki bütün balonlar sayılıyor.
- **Zincirde ikinci vuruş:** `OrbBomb`'dan dönüşen bomba, alanda önceden duran roketi tetikliyor; roket aynı Sandığa ikinci vuruşu ayrı patlama olarak yapıyor.

**Sözleşme gereği güncellenen mevcut test** (tek değişiklik): `LevelParserTests.Parse_BadGoals_MessageContainsGoalPath`, satır `[ { "count": 5 } ]`. Beklenen yol `'goals[0].color'` yerine `'goals[0]'` oldu (netleştirme: `color` ve `obstacle` ikisi de yoksa yol `'goals[i]'`). Testin ölçtüğü şey aynı kaldı: mesajda alan yolunun geçmesi.

**Test yazmadığım belirsizlik:** `LevelData`'ya `default(ObstaclePlacement)` verilirse (Pinyata, `(0,0)`, `Layers 0`) ne olmalı, spec söylemiyor. `LevelData` hataları yalnız "grid dışı, çakışma, y = 0'da Anchor" diye listeleniyor; kat denetimi `ObstaclePlacement` kurucusunda. Tahmin etmedim, karar ürün sahibinin.

```
Başarılı!  - Başarısız:     0, Başarılı:  1226, Atlanan:     0, Toplam:  1226
```
Yeni testlerin hepsi geçiyor, kod hatası adayı çıkmadı.
