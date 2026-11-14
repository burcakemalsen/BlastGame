# Adım 6b, kombolar: Claude kontrolcü raporu

HEAD: `d9aa82c feat(view): animate combos` (worktree `step/06b-combos`'a ff ile getirildi).

## Test özeti
```
Başarılı!  - Başarısız:     0, Başarılı:  1022, Atlanan:     0, Toplam:  1022, Süre: 268 ms - Core.Tests.dll (net10.0)
```

## Bulgular
Core'da spec'e aykırı bir şey bulamadım. Bulgular görüntü katmanında ve küçük:

1. **`GridView.cs:516` (`PlanConvert`), düşük.** Dokunulan hücre Rocket/Bomb, Orb başka bir katılımcıysa dokunulan hücre `start` anında Orb görünümüne geçiyor. Gerçek Orb karosu da aynı anda oraya kayıyor. Sonuç: ~0.15 sn ekranda iki Orb var, dokunulan roket/bomba birleşmeden kayboluyor. Spec "Birleşme: katılımcılar dokunulan hücreye kayıp üst üste biner". Yönlendirme: görünümü `start + MergeDuration`'da değiştirmek yeterli (kodcu). Elle kontrolde bakılsın.
2. **`GridView.cs:302` ile `:472`, bilgi.** `BigBomb` alanındaki zincir öğesi `ReachTime`'a göre karonun temizlenmesinden 0.02 sn önce başlıyor. Gözle fark edilmez, düzeltmek gerekmez.
3. **`PlanConvert`, bilgi.** Dönüşüm boşsa (Orb renginde kare yok) Orb yine `OrbPulse + OrbBeam` kadar bekleyip nabız atıyor, ışın gitmiyor. Spec'e aykırı değil.
4. **Süre, bilgi.** Dönüşen öğe başına 0.02 sn arayla patlama var. 9x9'da ~20 dönüşen kare için toplam yaklaşık 1.3 sn ediyor. Spec'in "bir iki saniye" sınırının içinde. Çok kalabalık tahtada sınıra yaklaşır, elle kontrolde bakılsın.

## Sorun bulamadığım alanlar ve ne denedim
- **Spec karşılaştırması (`SpecialRules.cs`):** Katılımcı bulma 4 komşu üzerinden yapılıyor, yalnız özel hücrelerden geçiyor ve dokunulan başa geldikten sonra y, x sırasıyla diziliyor. Güç sırası spec'teki 6 satırla birebir aynı. `Cleared` katılımcılarla başlıyor, `MergedCount = participants.Count`. Alan taramaları y, x sırasıyla. `OrbOrb` özel öğeleri de temizliyor ve tetiklemiyor. Dönüşümde eski hücre `Converted`'a giriyor, öğeler sırayla `TriggeredBy 0` ile kuyruğa ekleniyor. Yön çekilişleri herhangi bir `grid.Set`'ten önce yapılıyor (geçersiz değerde grid değişmiyor). `OrbBomb` ve boş dönüşüm çekiliş yapmıyor. Tek katılımcıda `Activate` çağrılıyor. Hata sırası grid, random, x/y, özel değil. `TurnRules` `Tap` kullanıyor, `GameSession` `Converted`'ı sayıyor. 7 parametreli kurucunun denetim sırası da sözleşmeye uyuyor.
- **Kural 4:** Core'da `UnityEngine`, `System.Random`, `record`/`init`/`new()`/file-scoped namespace/`is not` yok.
- **Test dokunulmazlığı:** Bu adımda `tests/`'e yalnız `39a44bd test(core)` dokunmuş. `3b0fe26`, `944d167`, `d9aa82c` commit'lerinde `tests/` yok. Mevcut 3 dosyadaki 5 güncellemenin hepsi yan yana duran öğeyi çapraza almak. Ölçülen davranışlar korunmuş: zincir, düşme ve doldurmanın bir kez olması, renk sayımı, üst satırdaki bombanın düşmesi. Sözleşmenin gerektirdiğinden fazla değişiklik yok.
- **Hile:** Core'da sabit koordinat, test verisine özel dal ya da beklenen değeri doğrudan döndüren kod yok.
- **Uç durum:** Orb rengi dokunulan rokette değil Orb'da; dönüşen bomba başka dönüşmüş öğeyi buluyor; kombo alanında yan yana iki öğe var; tekli dokunuşta random null. Hepsine bakan bir test var (bozma tablosu).

## Bilerek bozma (23 deneme, hepsi yakalandı; her biri `git checkout -- Assets/` ile geri alındı)
| # | Bozma | Kırmızı | Yakalayan (örnek) |
|---|---|---|---|
| 1 | Güç sırası: OrbRocket, OrbBomb'dan önce | 5 | `ComboSelectionTests.Tap_ParticipantKinds_*`, `TurnRulesComboTests.Tap_Example6OrbBombChain_*` |
| 2 | RocketBomb, BigBomb'dan önce | 1 | `ComboSelectionTests.Tap_ParticipantKinds_*` |
| 3 | Katılımcı: yalnız doğrudan komşular | 1 | `ComboParticipantsTests.Tap_LinkedChainOfItems_*` |
| 4 | Katılımcıya çapraz eklendi | 9 | `ComboChainTests.Tap_Example7OrbOrb_*`, `SpecialReviewFollowUpTests.Tap_RocketWithBombAboveItsRow_*` |
| 5 | BigBomb yarıçapı 2 | 4 | `ComboChainTests.Tap_BigBombAreaEdge_*`, `ComboAreaTests.Tap_BigBombNearBottomEdge_*` |
| 6 | RocketBomb tek satır/sütun | 4 | `ComboAreaTests.Tap_RocketBombCorner_*` |
| 7 | `Converted` sayımdan çıkarıldı | 3 | `GameSessionComboTests.Tap_Example5OrbRocket_*` |
| 8 | Yön çekilişi dönüşüm sırasında (grid değiştikten sonra) | 5 | `ComboConversionTests.Tap_InvalidDirectionDraw_*`, `GameSessionComboTests.Tap_ComboInvalidDirectionDraw_*` |
| 9 | OrbOrb özel öğeleri tetikliyor | 2 | `ComboChainTests.Tap_Example7OrbOrb_*`, `Tap_OrbOrbWithHoles_*` |
| 10 | Dönüşen öğelerde `TriggeredBy -1` | 5 | `ComboConversionTests.Tap_Example6ThreeParticipants_*` |
| 11 | Katılımcılar sıralanmıyor | 1 | `ComboParticipantsTests.Tap_FourNeighbours_*` |
| 12 | `MergedCount = Cleared.Count` | 32 | çok sayıda |
| 13 | `TurnRules` yine `Activate`'i çağırıyor | 12 | `TurnRulesComboTests`, `GameSessionComboTests`, `ComboReproducibilityTests` |
| 14 | BigBomb taraması x öncelikli | 4 | `ComboAreaTests.Tap_BigBombOnSmallBoardWithHoles_*` |
| 15 | Kombo alanındaki öğede `TriggeredBy -1` | 2 | `ComboChainTests.Tap_ItemInComboArea_*` |
| 16 | OrbOrb özel öğeleri atlıyor | 2 | `ComboChainTests.Tap_Example7OrbOrb_*` |
| 17 | `Converted`'a yeni öğe yazılıyor | 10 | `ComboConversionTests.Tap_SameColorOrbElsewhere_*` |
| 18 | Boş dönüşümde bir çekiliş | 1 | `ComboConversionTests.Tap_NoCellOfOrbColor_*` |
| 19 | `Converted` taraması x öncelikli | 3 | `ComboConversionTests.Tap_OrbRocketFiveCells_*` |
| 20 | OrbBomb da çekiliş yapıyor | 5 | `ComboConversionTests.Tap_ConvertedCells_NeverClearedAsColor` |
| 21 | random null denetimi tekli yoldan sonra | 1 | `SpecialRulesTapTests.Tap_NullRandomAndOutside_*` |
| 22 | `Converted` iki kez sayılıyor | 2 | `GameSessionComboTests.Tap_Example5*`, `Tap_Example6*` |
| 23 | Orb dokunuşunda 2 katılımcı tekli yola gidiyor | 22 | çok sayıda |

Not: 2, 3, 11, 18 ve 21'i tek bir test yakalıyor. Bunlar yeterli ama ince; ek test zorunlu değil.

## Görüntü (yalnız okundu, çalıştırılmadı)
Spec ile karşılaştırdım. Birleşme 0.15 sn ve ardından parlama var. `RocketCross` dört yöne iki çift atıyor. `RocketBomb` tahta içinde kalan satır/sütun başına birer çift atıyor. `BigBomb` 7x7'lik halka çiziyor, sarsıntısı tekli bombadan güçlü (0.16'ya 0.07). Dönüşümde ışınlar sırayla gidiyor, değdiği kare gerçek yön ile yeni öğeye dönüyor, öğeler bütün ışınlar bitince 0.02 sn arayla patlıyor. `OrbOrb` tahta merkezinden dışa ~0.4 sn dalga yapıyor. Zamanlama `TriggeredBy`/`ReachTime` ile kuruluyor. Efekt nesneleri havuzdan geliyor ve `ReleaseFx` ile geri veriliyor. `_looks` her tur temizleniyor ve yalnız efekt evresinde uygulanıyor. Sızıntı görmedim. Elle kontrol yapılmadı.
