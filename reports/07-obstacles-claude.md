# Adım 7, engeller: Claude kontrolcü raporu

## Test
```
Başarılı!  - Başarısız:     0, Başarılı:  1215, Atlanan:     0, Toplam:  1215
```

## Bulgular
1. **Denetim sırası açığı, orta, testçi.** `LevelParser.cs` `ReadObstacle`: `Anchor y = 0` denetimi ile hücre çakışması yer değiştirince (S8) hiçbir test kırılmıyor. `Parse_TwoErrors_FirstInCheckOrderIsReported`'a satır önerisi: obstacles `[ {Pinata x2 y0}, {Anchor x2 y0} ]` → beklenen `'obstacles[1].y'`, beklenmeyen `'obstacles[1]'`. Aynı tabloda `obstacle` tekrarı ile `count` aşımı sırası da yok.
2. **Eksik uç durum, düşük, testçi.** `TurnRules.Tap` düzeyinde "Sandık üstünde Çapa, Sandık aynı dokunuşta kırılır, Çapa aynı turda dibe iner ve toplanır" testi yok (yalnız `Settle` düzeyinde Sandık üstü Çapa ve `OrbOrb` örneği var). Kod doğru davranıyor (`Settle` bütün vuruşlardan sonra bir kez çalışıyor); gerileme koruması olarak önerilir.
3. **Eski test ve iki yollu mesaj, düşük, testçi (karar ürün sahibinin).** `LevelParser.cs` `ReadGoal`: mesaj `Goal 'goals[0]' needs exactly one of 'goals[0].color' and 'goals[0].obstacle'.` Bunu hile saymıyorum: özel durum dalı yok, metin her girdide aynı ve doğal, iki alanı adıyla anması kullanıcı için de doğru. Ama Adım 5 testi `Parse_BadGoals_MessageContainsGoalPath` artık eski sözleşmeyi (`'goals[0].color'`) sınıyor; sözleşme değişikliği kuralıyla testçi `'goals[0]'`'a çekmeli, mesaj ileride sadeleşirse yanlış kırmızı vermesin.
4. **Görüntü sapması, bilgi.** Hedefe uçuş yalnız Çapada (`GridView`), Pinyata/Sandık/balon uçmuyor; kodcu raporunda yazılı, spec "varsa" diyor. Elle kontrolde ürün sahibi bakmalı.

## Sorun bulamadığım alanlar (ne denedim)
- **Spec karşılaştırması:** grup komşu vuruşu tek `HashSet` ile; roket `Visit`'te engelde durmuyor; Orb komşuları `cleared` (kendi hücresi dahil) üzerinden; `OrbRocket`/`OrbBomb` dönüşen öğeler kombonun `hitSet`'ini paylaşıyor, zincirde tetiklenen eski öğe yeni küme alıyor; dönüşüm vuruş yapmıyor; `OrbOrb` Çapayı bırakıyor. `Settle` bölümleri, yalnız alt bölümde ardışık Çapa toplama, `Collected` sırası; `Fill` en üst Sandığın üstü ve çekiliş sayısı; engele dokunuş hamle harcamıyor; `GameSession` sayımı (balon `Cleared`+`Converted`, kırılan vuruş, `Collected`) ve 6/7 parametreli kurucu sırası netleştirmeye uygun; parser sırası ve yollar uygun.
- **Kural 4:** Core'da `UnityEngine`, `System.Random` yok; C# 7 üstü yapı görmedim.
- **Test dokunulmazlığı:** yerel `main` eski (Adım 6/6b birleşmelerini içermiyor), bu yüzden `ce9f4f8..HEAD` ile baktım: `tests/`'e yalnız `b7c9ac1 test(core)` dokunuyor, yalnız ekleme. `feat(core)`/`feat(view)` commit'lerinde `tests/` boş.
- **Hile:** test değerine göre dallanma, sabit konum ya da sonuç yok.
- **Görüntü:** level modu açık (`EndlessMode = false`), HUD engel simgeleri ve `Remaining(kind)`, Sandık katı başına sprite, balon şeffaf katman, Çapa uçucuları yeniden kullanılıyor, parçacıklar havuzdan. Dört deneme level'ı okuyarak geçerli (alan, sınır, `y = 0` Çapa yok, hedef ≤ yerleşim). Çalıştırmadım; Editor/WebGL doğrulanmadı.

## Bilerek bozma
| # | Bozma | Sonuç |
|---|---|---|
| S1 | Grup komşu vuruşu her kare için ayrı | Yakalandı: `Tap_CrateNextToTwoGroupCells_LosesOnlyOneLayer`, `Tap_CornerCrate_IsHitByGroup` |
| S2 | Roket ilk engelden sonra durur | Yakalandı: `Tap_Example6_*`, `Activate_RocketThroughCrate_*` (+2) |
| S3 | Sandık vuruşta 2 kat kaybeder | Yakalandı: 17 test |
| S4 | `Fill` Sandık altını da doldurur | Yakalandı: `Fill_GapBelowCrate_IsNotFilled` (+5) |
| S5 | Yalnız ilk Çapa toplanır | Yakalandı: `Settle_LeadingAnchorsWithGap_*` (+1) |
| S6 | `OrbRocket`/`OrbBomb` öğeleri ayrı patlama | Yakalandı: `Tap_OrbRocketOverlapping*`, `Tap_OrbBombOverlapping*` |
| S7 | Orb kendi hücresinin komşusunu atlar | Yakalandı: `Tap_SingleOrb_HitsObstacleNextToItsOwnCell` |
| S8 | Parser: çakışma denetimi Çapa `y = 0`'dan önce | **Yakalanmadı** (bulgu 1) |
| S9 | Dönüşümde balon düşmez | Yakalandı: `Tap_OrbRocketConvertsBalloonSquare_*` (+1) |
| S10 | Kırılmayan Sandık vuruşu hedefe sayılır | Yakalandı: `Tap_CrateGoal_CountsOnlyLastLayerThenWins` |
| S11 | `Collapse` Çapa toplar | Yakalandı: `Collapse_AnchorAtBottom_IsNotCollected` |
| S12 | Balonlu ve balonsuz kare ayrı grup | Yakalandı: `FindGroup_BalloonAndPlainSameColor_*` (+3) |
| S13 | `Hits` sıralanmaz | Yakalandı: `Tap_GroupHitsTwoObstacles_HitsOrderedByYThenX` |
| S14 | `OrbOrb` engelleri vurmaz | Yakalandı: `Tap_OrbOrb_HitsEveryObstacleOnce*` |
| S15 | Çapa her bölümde toplanır | Yakalandı: `Settle_AnchorAboveCrate_*` |
| S16 | `layers` her türde serbest | Yakalandı: `Parse_BadObstacle_MessageHasFieldPath` |

Her bozmadan sonra `git checkout -- Assets/` ile geri alındı; çalışma ağacı temiz.
