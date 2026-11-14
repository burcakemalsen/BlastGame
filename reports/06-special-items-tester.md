# Adım 6 testçi raporu: özel öğeler

## Dosyalar (hepsi `tests/Core.Tests/`)
- Yeni yardımcı: `SpecialGrid.cs` (özel hücreli grid metni: `H` `V` `X`, küçük harf = Orb; etki satırı biçimi `Kind x,y t=TriggeredBy c=Created | x,yL ...`; hücre hücre karşılaştırma; `Cleared` tekillik denetimi).
- Yeni testler: `CellSpecialTests.cs` (33), `ClearedCellTests.cs` (7), `TurnEffectTests.cs` (`TurnEffectTests` 8 + `TurnResultEffectsTests` 7), `SpecialRulesForGroupTests.cs` (26), `SpecialRulesActivateTests.cs` (31), `TurnRulesSpecialTests.cs` (28), `GameSessionSpecialTests.cs` (7).
- **Yeni test: 147** (`Theory` satırları dahil; sınır ~150).

## `dotnet test` özeti
```
Başarısız! - Başarısız:   144, Başarılı:   769, Atlanan:     0, Toplam:   913, Süre: 135 ms - Core.Tests.dll (net10.0)
```
- 144 kalan = 143 yeni + sözleşme yüzünden güncellenen 1 mevcut test (aşağıda). Kırmızıların hepsi stub'dan (`NotImplementedException` ya da eski `Tap` davranışı); test hatası değil.
- Mevcut testlerden 765'i yeşil. Stub'da da geçen 4 yeni test: `Thresholds_AreFiveSevenNine` (sabitler stub'da var), `Ctor3_Effects_EmptyNotNull` (eski kurucu), `Tap_NoBlast_EffectsEmptyAndSourceNotCalled` (2 satır; eski `Tap` 3 parametreli kurucuyla boş `Effects` veriyor). Normal.

## Mevcut testlerde zorunlu güncellemeler (RULES, Onay)
1. `TurnRulesTests.Tap_WholeGridOneColor_BlastsAllNoMovesRefillsAll` → `..._BlastsAllOrbFallsRefillsRest`: 16'lık grup artık yeşil Orb bırakır; beklenen hareket `(2,1)->(2,0)`, 15 çekiliş, 15 dolum, yeni grid. (Şu an kırmızı; uygulama gelince yeşillenmeli.)
2. `TurnRulesTests.Tap_Example4SameSeedSameTaps_...` ve `Tap_DifferentSeedsSameGrid_...`: `TestGrid.Dump` → `SpecialGrid.Dump` (rastgele tahtada özel hücre doğunca eski `Dump` `Color` erişiminde patlar).
3. `GameSessionTests.Snapshot`: aynı nedenle `SpecialGrid.Dump`.

## Spec maddesi → test
- Hücre (`OfSpecial`/`OfOrb` hataları, erişim hataları, eşitlik, hash, `ToString`, Orb'da `Special == Orb`): `CellSpecialTests`.
- `ClearedCell`: `ClearedCellTests`. `TurnEffect` kurucusu (liste kopyalanmaz, iki denetim, uyum denetlenmez): `TurnEffectTests`.
- `TurnResult` 4 parametreli kurucu (null sırası, `effects` adı, tutarlılık denetlenmez), eski kurucu boş `Effects`: `TurnResultEffectsTests`.
- `ForGroup` eşikleri 0/1/4, 5/6 (çekiliş 0/1, tek `Next(2)`), 7/8, 9/10/256; geçersiz çekiliş; hata sırası ve `ParamName`: `SpecialRulesForGroupTests`.
- `Activate` hataları (x önce y); Rocket orta/kenar/köşe, yatay/dikey, 1 genişlik/yükseklik, eşit uzaklıkta sol/alt önce, boşları atlama; Bomb orta (9), dört köşe (4), kenar (6), 1x1; Orb (Örnek 4, renk yokken yalnız kendisi, başka Orb/özel öğeye dokunmaz); zincir (Örnek 3, Örnek 5 somut grid, üç seviye FIFO ve `TriggeredBy` 1, iki alandaki öğe bir kez, birbirini gören iki Rocket, kombo yok): `SpecialRulesActivateTests`.
- `Tap`: Örnek 1, 2, 3, 6 birebir (etkiler, `Blasted`, `Moves`, `Filled`, grid, `MaxArgs`); eşikler 4-10 ve doğan öğe dokunulan hücrede; geçersiz yön çekilişinde hücre hücre grid aynı; doğan öğe düşer; 2-4 grupta tek `Group` etkisi; etkisiz dokunuşta boş `Effects`; özel hücre gruba girmez; `Blasted` = `Cleared` birleşimi; denetimler önce; tekrarlanabilirlik; `GroupFinder`/`BlastRules` özel hücrede boş: `TurnRulesSpecialTests`.
- `GameSession` (hamle harcar, renk başına sayım, öğe sayılmaz, birleşmede dokunulan sayılır, 0 altına inmez, son hamlede zincirle kazanma, oyun bitince özel öğeye dokunuş etkisiz ve kaynağa dokunmaz): `GameSessionSpecialTests`.

## Belirsizlikler
1. `Group` etkisinin `Cleared` sırası spec'te yalnız Örnek 1'de "x sırasıyla" geçiyor. Çok satırlı grupta `GroupFinder` sırasını (y sonra x) varsaydım (`Tap_Example6...`, `Tap_MergeAtTopOfColumn...`).
2. Birden çok sütunda düşme sırası Adım 3'teki gibi (x artan) varsayıldı (Örnek 3'ün `Tap` testi).
3. Rocket `ToString`: spec yalnız Bomb ve Orb örneği veriyor; `"Special(RocketHorizontal)"` kalıptan çıkarıldı.
4. `TurnEffect` kurucusunda `cleared` null ve `triggeredBy < -1` aynı anda olunca hangisinin önce denetleneceği yazılmamış; test yazılmadı.
5. `Tap_SameSeedSameTapsWithSpecials_...` seed 2026'da en az bir özel öğenin çalıştığını varsayar; uygulama gelmeden doğrulanamadı. Çalışmazsa test "proves nothing" mesajıyla düşer, seed değişmeli.

## Kontrol sonrası eklenenler
Dosya: `tests/Core.Tests/SpecialReviewFollowUpTests.cs`. **Yeni test: 14**; mevcut testlere dokunulmadı. Uygulamaya (`46acef1`) karşı hepsi ilk çalıştırmada yeşil, test bükülmedi.

Bulgu → test:
1. Rocket uzunluğu (`d < grid.Width` sınırı yakalanmamıştı): `Activate_VerticalRocketOnTallBoard_ReachesTopCell` (3x6), `Activate_VerticalRocketOnOneByFour_ClearsWholeColumn`, `Activate_HorizontalRocketOnWideBoard_ReachesRightmostCell` (6x3), `Activate_HorizontalRocketOnFourByOne_ClearsWholeRow`.
2. Rocket'ın kuyruğa ekleme sırası: `Activate_HorizontalRocketFindsSeveralItems_QueuedInScanOrder`, `Activate_VerticalRocketFindsSeveralItems_DownBeforeUpAtEqualDistance` (tahtada rengi olmayan Orb'lar yalnız kendini temizler, `Effects` sırası kuyruk sırasını gösterir).
3. Zincirde Orb: (a) `Activate_OrbBeforeRocketInQueue_RocketSkipsCellsOrbCleared`, (b) `Activate_TriggeredOrbAfterItsColorWasCleared_ClearsOnlyItself`.
4. `GameSession`: `SessionTap_SpecialOnLastMoveGoalNotMet_Lost`, `SessionTap_RocketMergeInvalidDirection_ThrowsAndStateUnchanged` (2 satır: `-1`, `2`).
5. `Cell`: `Orb_DifferentColors_NotEqualAsObjectAndHashesNotAllSame` (altı rengin Orb hash'leri tek değerde toplanmıyor).
6. Alan dışındaki özel öğe yalnız düşer: `Tap_RocketWithBombAboveItsRow_BombFallsNotTriggered`, `Tap_GroupUnderBomb_BombFallsNotTriggered`.

`TurnEffect` kurucusunda null ile `-2` aynı anda gelince hangisinin önce denetleneceği spec'te yazılı değil; test yazılmadı.

```
Başarılı!  - Başarısız:     0, Başarılı:   927, Atlanan:     0, Toplam:   927, Süre: 83 ms - Core.Tests.dll (net10.0)
```
