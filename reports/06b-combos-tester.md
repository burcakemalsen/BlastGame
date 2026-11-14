# Adım 6b, kombolar: testçi raporu

## Dosyalar
Yeni (`tests/Core.Tests/`): `ComboTestHelpers.cs` (yardımcı: MergedCount/Converted'li etki metni, desenli tahta, alan beklentisi), `SpecialRulesTapTests.cs`, `ComboParticipantsTests.cs`, `ComboSelectionTests.cs`, `ComboAreaTests.cs`, `ComboConversionTests.cs`, `ComboChainTests.cs`, `TurnEffectComboTests.cs`, `TurnRulesComboTests.cs`, `GameSessionComboTests.cs`, `ComboReproducibilityTests.cs`.

Yeni test: **95** (`Theory` satırları dahil).

## `dotnet test` özeti
```
Başarısız! - Başarısız:    90, Başarılı:   932, Atlanan:     0, Toplam:  1022, Süre: 92 ms - Core.Tests.dll (net10.0)
```
Mevcut 927 test yeşil (güncellenen 5'i dahil). Yeni testlerden 90'ı kırmızı: `SpecialRules.Tap` ve 7 parametreli kurucu `NotImplementedException`; `TurnRules`/`GameSession` testleri ise henüz `Activate`'e gittiği için assertion ile düşüyor.

Stub'la geçen 5 yeni test (bilerek): `EffectKind` sırası, 5 parametreli kurucunun `MergedCount`/`Converted` değerleri (3 satır), `Activate`'in yan yana öğede kombo yapmaması. Değişmemesi gereken davranışı koruyorlar.

## Mevcut testlerde yapılan güncellemeler (sözleşme değişikliği notu)
Hepsinde ölçülen şey korunup özel öğeler ayrıldı: yan yana duran öğe çapraza alındı. Böylece zincir sürüyor, kombo oluşmuyor. Adım 6 Örnek 3'ün kombo hali `TurnRulesComboTests.Tap_Step6Example3BombNextToRocket_NowRocketBombWholeBoard`'da.
1. `TurnRulesSpecialTests.Tap_Example3Chain_...` → `Tap_ChainOfSeparatedItems_GravityAndRefillOnceAfterAllEffects`: grid `RGBGR/GXBBG/BRHRB`, roket `(2,0)`'da. Etkiler, Blasted, Moves, Filled, grid ve çekilişler yeniden hesaplandı.
2. `GameSessionSpecialTests`: `Example3` alanı yerine `Chain` (aynı grid) geldi.
   - `Tap_Example3Chain_...` → `Tap_Chain_EachColorCountedSeparatelyItemsNotCounted`, kalanlar 2/3/1.
   - `Tap_ChainClearsMoreThanGoal_...`: hedef Green 2 yerine Blue 2 (temizlenen 4), Red 10 → 7.
   - `Tap_ChainOnLastMove_...`: hedefler R3/G2/B4.
3. `SpecialReviewFollowUpTests.Tap_RocketWithBombAboveItsRow_...`: grid `XGG/BHR` (bomba çaprazda), beklentiler aynı, son grid `BBB/XGG`.

`TurnRulesSpecialTests`'teki `Example3`'lü iki hata testi (null random, kötü colorCount) ve `SpecialRules.Activate` testleri değişmedi.

## Spec maddesi → test
- Örnek 1: `ComboParticipantsTests.Tap_Example1RocketPair_*`, `Tap_Example1TapOtherRocket_*`; `TurnRulesComboTests.Tap_Example1RocketCross_*`; `GameSessionComboTests.Tap_Example1RocketCross_*`
- Örnek 2: `SpecialRulesTapTests.Tap_Example2DiagonalItem_*`
- Örnek 3: `ComboAreaTests.Tap_Example3RocketBomb_*`
- Örnek 4: `ComboAreaTests.Tap_Example4BigBombCorner_*`
- Örnek 5: `ComboConversionTests.Tap_Example5OrbRocket_*`; `TurnRulesComboTests.Tap_Example5OrbRocket_DirectionDrawsBeforeRefill`; `GameSessionComboTests.Tap_Example5*`
- Örnek 6: `ComboConversionTests.Tap_Example6*`; `TurnRulesComboTests.Tap_Example6*`; `GameSessionComboTests.Tap_Example6*`
- Örnek 7: `ComboChainTests.Tap_Example7OrbOrb_*`
- Örnek 8: `SpecialRulesTapTests.Tap_Example8*`
- Katılımcılar: tek, dört komşu (y/x sırası), bağlı zincir, çapraz, renkli/boş hücre bağı kopar → `ComboParticipantsTests`. Tek katılımcı = `Activate` → `SpecialRulesTapTests.Tap_LoneItem_*`
- Seçim (6 kombo, karışık setler, dokunulanın türünden bağımsız) → `ComboSelectionTests` (18 satır)
- Alanlar: kenar ve köşe kırpma, RocketBomb kenarda, BigBomb 7x7 sınırı/kenar/küçük tahta, boş hücre atlanır → `ComboAreaTests`; BigBomb 3 uzaklık var, 4 yok → `ComboChainTests.Tap_BigBombAreaEdge_*`
- Dönüşüm: renk yok (Rocket/Bomb), `Converted` sırası, çekiliş sayısı/sırası/yönü, çekilişte grid değişmez (3 satır), OrbBomb'da çekiliş yok, dönüşen kare renkli olarak temizlenmez, Orb rengi dokunulan roket olsa da Orb'dan gelir, başka Orb dönüşmez, dönüşen bombanın bulduğu öğe sonra kuyruğa girer → `ComboConversionTests`
- OrbOrb: tahta boşalır, öğeler tetiklenmez, boş hücre → `ComboChainTests`
- Zincir: alandaki öğe tekli çalışır, komşusuyla birleşmez, y/x kuyruk sırası → `ComboChainTests`
- Hatalar ve sıraları, ParamName, grid ve kaynak dokunulmaz → `SpecialRulesTapTests`
- `TurnEffect` 7 parametreli kurucu, enum → `TurnEffectComboTests`
- `TurnRules`: Blasted = Cleared birleşimi, düşme ve doldurma bir kez, kaynak sırası, geçersiz çekiliş → `TurnRulesComboTests`
- `GameSession`: hamle, `Converted` sayımı, katılımcı sayılmaz, son hamlede kazanma, geçersiz çekilişte durum değişmez → `GameSessionComboTests`
- Tekrarlanabilirlik (en az bir kombo doğrulanıyor) → `ComboReproducibilityTests`

## Belirsizlikler (spec'te yazmıyor; varsayım ya da test edilmedi)
1. `SpecialRules.Tap` null hatalarında ParamName `grid`/`random`, aralık hatasında `x`/`y` varsayıldı (mevcut `TurnRules` geleneği). Grid ile random null sırası ancak ParamName'le ayırt edilebiliyor.
2. Geçersiz yön çekilişinde kaynağın ilk kötü değerde mi durduğu, yoksa bütün çekilişleri mi yaptığı belli değil. Çağrı sayısı denetlenmedi.
3. 7 parametreli kurucuda null denetimi ile aralık denetimi arasındaki sıra yazmıyor. Test edilmedi.
4. Kombo etkisinde `Created = Cell.Empty` varsayıldı (`TurnEffect` belgesi: "otherwise Cell.Empty").
5. Zincirdeki tekli etkilerde `MergedCount = 1`, `Converted` boş varsayıldı.
6. Geçersiz çekilişte `GameSession` durumunun (hamle, hedef, grid) değişmediği varsayıldı (Adım 6'daki karşılığı gibi).
7. Beklenen değerler elle hesaplandı. Uygulama olmadığı için doğrulanamadı; kodcu uygulayınca kırmızı kalan test olursa önce hesap kontrol edilmeli.
