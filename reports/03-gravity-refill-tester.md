# Adım 3, düşme ve doldurma: testçi raporu

## Dosyalar ve sayı
`tests/Core.Tests/` altında, **85 yeni test** (`Theory` satırları dahil; ~150 sınırının altında):
- `CellMoveTests.cs` 13, `TurnResultTests.cs` 9, `GravityRulesTests.cs` 16, `RefillRulesTests.cs` 21, `TurnRulesTests.cs` 26.
- `TurnTestHelpers.cs`: `CountingRandom` (sıralı/sabit/döngülü değer, çağrı sayısı, `maxExclusive` kaydı; değer biterse testi düşürür), `Clone`, `AssertMoves` (tam sıra, yalnız koordinat karşılaştırır; `CellMove` eşitliğine dayanmaz).
Mevcut testlere dokunulmadı.

## Spec maddesi → test
**Düşme**
- Sütunda sırayı koruyarak en alta toplama → `Collapse_SeveralGapPieces_OrderOfColorsPreserved`, `Collapse_GapInMiddle_*`, `Collapse_SingleGapAtBottom_*`
- Liste sırası (x, sonra To.Y), yerinde kalan girmez → `Collapse_Example1_*`, `Collapse_16x16BottomRowEmpty_*`, `Collapse_ColumnsIndependent_*`
- Boşluk yok / zaten üstte → `Collapse_NoEmptyCells_*`, `Collapse_GapsAlreadyAtTop_*`
- `Cell` değeri aynen taşınır → `Collapse_AllSixColorsWithGaps_KindAndColorCarried`
- Sütunlar bağımsız → `Collapse_ColumnsIndependent_*`, `Collapse_SingleRowWithGaps_*`
- Uç: tamamen boş sütun, en uzun düşüş (16), 1x1 boş/dolu, 1xN, Nx1, 16x16, ikinci çağrı → ilgili `Collapse_*` testleri
- `null` → `Collapse_NullGrid_ThrowsArgumentNull`

**Doldurma**
- Örnek 2 birebir (liste, grid, 6 çağrı) → `Fill_Example2_*`
- Sıra y sonra x, 256 çağrı → `Fill_Whole16x16Empty_*`
- Boş yoksa kaynak çağrılmaz → `Fill_NoEmptyCell_*`; tek boş → `Fill_SingleEmptyCell_*`
- Dolu hücre değişmez → `Fill_MixedGrid_*`; ortadaki boşluğu da doldurur → `Fill_GapBelowFilledCell_*`
- `Next(colorCount)` argümanı → `Fill_AnyColorCount_PassesColorCountAsMaxExclusive`, `Fill_ColorCountOne_AllRed`
- `colorCount` 1 ve 6 sınırı → `Fill_ColorCountOne_*`, `Fill_ColorCountSixValueFive_Orange`; 0, -1, 7, MinValue → `Fill_ColorCountOutOfRange_*` (kaynak 0 çağrı, grid aynı)
- `null` grid/kaynak ve kontrol sırası → `Fill_NullGrid_*`, `Fill_NullRandom_*`, `Fill_NullGridBadCountNullRandom_*`, `Fill_BadCountAndNullRandom_*`
- Kaynak `colorCount` / `-1` (ilk ve son boşta) → IOE, grid aynı → `Fill_SourceReturnsOutOfRange_*`

**Bir dokunuş**
- Örnek 3 birebir → `Tap_Example3Group_*`, `Tap_Example3SingleCell_*`
- Boşa dokunuş: boş hücre, 1x1 → `Tap_EmptyCell_*`, `Tap_1x1Grid_*`; sonraki renkler kaymaz → `Tap_MissBeforeBlast_*`
- Patlamadan sonra boş yok → `Tap_ValidBlast_NoEmptyCellLeft`; katılmayan sütunlar aynı → `Tap_ValidBlast_UninvolvedColumnsUnchanged`
- `Blasted` = Blast'ın listesi → `Tap_BlastedList_MatchesBlastRulesOrder`, `Tap_TopRightCornerGroup_*` (köşe koordinatı)
- Tek renk grid → `Tap_WholeGridOneColor_*`
- Hatalar, sıra, hata sonrası grid ve kaynak → `Tap_NullGrid_*`, `Tap_NullRandom_*`, `Tap_ColorCountOutOfRange*`, `Tap_CoordinateOutside_*`, `Tap_NullGridAndEverythingElseBad_*`, `Tap_NullRandomBadCountBadCoordinate_*`
- Kaynak geçersiz → patlama+düşme kalır → `Tap_SourceReturnsOutOfRange_*`
- Örnek 4 ve farklı seed → `Tap_Example4SameSeedSameTaps_*`, `Tap_DifferentSeedsSameGrid_*`

**TurnResult / CellMove**: null parametre adları ve sırası, kopyasız saklama, `HasBlasted`; eşitlik (üç biçim), takas, null/başka tür, `GetHashCode`, `ToString`.

## `dotnet test` özeti
```
Başarısız! - Başarısız:    85, Başarılı:   506, Atlanan:     0, Toplam:   591, Süre: 73 ms - Core.Tests.dll (net10.0)
```
Kırmızıların hepsi yeni testler; nedenleri 53 `NotImplementedException`, 32 `Assert.Throws() Failure` (stub beklenen tür yerine `NotImplementedException` atıyor). Geçen yeni test yok.

## Belirsizlikler, yazılmayan testler
- **Örnek 3 `Filled` sırası:** spec boşları `(0,2), (1,1), (1,2)` diye sayıyor; ama `Filled` Fill'in listesi ve Fill sırası y sonra x. Test `(1,1), (0,2), (1,2)` bekliyor. Spec metni düzeltilebilir.
- **Blast listesi sırası** spec'te tanımlı değil (Adım 2'nin işi); Örnek 3 dışındaki testler `BlastRules.Blast` ile karşılaştırıyor.
- **Fill'de geçersiz değerde kaç çağrı yapılır** ("önce bütün değerler çekilir"): ilk geçersizde durmak mı, hepsini çekmek mi belirsiz; çağrı sayısı test edilmedi, yalnız grid'in değişmediği.
- **`ParamName`** yalnız `TurnResult` için tanımlı. Fill/Tap'ta `grid` ve `random` ikisi de `ArgumentNullException`; ikisinin sırası tür ile ayırt edilemiyor, `colorCount` ve koordinat da ikisi de `ArgumentOutOfRangeException`. Bu iki çiftin sırası test edilmedi (kaynak/grid dokunulmazlığı test edildi).
- `default(CellMove)` için yalnız hash, `(0,0)->(0,0)` ile eşitlik dolaylı; ayrıca tanımlı değil.

## Ek (kontrol kapısından sonra)
Spec `abe54a5` ile `Fill`/`Tap` hatalarına `ParamName` tanımlandı. Yeni dosya `tests/Core.Tests/TurnParamNameTests.cs`, **7 yeni test**. Mevcut testler değişmedi.

Hangi bozma hangi testi kırar:
- `Fill_NullGridAndNullRandom_ParamNameGrid`: `random` kontrolü `grid`'den önce gelirse `"random"` döner, test düşer.
- `Tap_NullGridAndNullRandom_ParamNameGrid`: aynı şey `Tap` için.
- `Tap_BadColorCountAndCoordinateOutside_ParamNameColorCount`: koordinat `colorCount`'tan önce denetlenirse `"x"` döner. Kaynak çağrı sayısının 0 olduğunu ve grid'in değişmediğini de sınar.
- `Tap_XAndYBothOutside_ParamNameX` (`(-1,-1)` ve `(3,3)`, 2 satır): `y` önce denetlenirse ya da tek ortak ad kullanılırsa `"y"`/başka ad döner. Alt ve üst sınırın ikisi de var.
- `Tap_OnlyYOutside_ParamNameY`: `y` hatasına `"x"` ya da sabit ad verilirse düşer. Bu olmadan yukarıdaki test, her koordinat hatasına `"x"` yazan kodla da geçerdi.
- `Tap_SingleCellOnGridWithHoles_HolesStaySourceNotCalled`: patlama olmasa da `Fill` çağrılırsa boşluklar dolar ve kaynak çağrılır, test düşer. Önceki boşa dokunuş testlerinde grid'de doldurulacak boşluk yoktu ya da tek boşluk dokunulan hücreydi; bu kuralı doğrudan sınamıyorlardı.

`dotnet test` (uygulama kodu zaten yazılı, yeni testler beklendiği gibi yeşil):
```
Başarılı!  - Başarısız:     0, Başarılı:   598, Atlanan:     0, Toplam:   598, Süre: 59 ms - Core.Tests.dll (net10.0)
```
Testleri bozma yaparak çalıştırıp kırmızıyı görmedim; yukarıdaki eşleme testlerin iddialarından çıkarıldı.
