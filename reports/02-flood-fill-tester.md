# Adım 2 testçi raporu: grup bulma ve patlatma

Dal `step/02-flood-fill`. Yalnız `tests/Core.Tests/` ve bu rapor yazıldı; uygulama kodu okunmadı (yalnız imzalar). Commit yok.

## Dosyalar ve sayılar
Sayı = xUnit test durumu (Theory satırları ayrı sayılır).

| Dosya | Durum | Test | Geçen |
|---|---|---|---|
| `CellTests.cs` | yeni | 42 | 1 |
| `CellPosTests.cs` | yeni | 30 | 0 |
| `GroupFinderTests.cs` | yeni | 102 | 0 |
| `BlastRulesTests.cs` | yeni | 77 | 1 |
| `GridBuilderTests.cs` | yeni (yardımcının testi) | 9 | 1 |
| `TestGrid.cs` | yeni yardımcı (test değil) | - | - |
| `GridTests.cs` | geçirildi | 122 | 67 |
| `GridFactoryTests.cs` | geçirildi | 77 | 29 |
| `SeededRandomTests.cs`, `TileColorTests.cs` | dokunulmadı | 40 + 7 | 47 |

Yeni 260, geçirilen 199 (Adım 1'de 199: Grid 122 + GridFactory 77), dokunulmayan 47; toplam 506. Eski toplam 246 idi; azalma yok.

## `dotnet test tests/BlastGame.Tests.slnx` (aynen)
```
Başarısız! - Başarısız:   360, Başarılı:   146, Atlanan:     0, Toplam:   506
```
Test projesi derleniyor. Kırmızıların nedeni `NotImplementedException` (stub).

### Geçen 146'nın incelemesi
Hiçbiri `Cell`/`CellPos` davranışını ölçmüyor, geçmeleri beklenen:
- `SeededRandomTests` (40), `TileColorTests` (7): `Cell`'e dokunmuyor.
- `GridTests` (67): yalnız boyut, `IsInside`, ctor aralığı, `Get`/`Set` grid dışı `ArgumentOutOfRangeException` (sınır kontrolü hücre değerine bakmadan atıyor), sabitler.
- `GridFactoryTests` (29): `CreateRandom`'ın girdi doğrulama hataları (hücre yazılmadan önce atılır), `MaxColorCount`.
- `CellKind_EmptyIsZero`, `MinGroupSize_MatchesSpec_Two`, `GridBuilderTests.Parse_NoRows_...`: sabit/enum/yardımcı doğrulaması.

## Mevcut testlerin geçişi
Dosyalar `GridTests.cs`, `GridFactoryTests.cs`; test yöntemi sayısı değişmedi (Grid 39, GridFactory 21), hiçbiri silinmedi.
- `TileColor?` → `Cell`: `null` → `Cell.Empty`, `Assert.Null(grid.Get)` → `Assert.Equal(Cell.Empty, ...)`, renk → `Cell.OfColor(...)`. `Set_FourCorners` beklenen dizisi `Cell[,]`, `PatternColor` `Cell` döndürüyor. `GridFactoryTests`'te `NotNull` → `IsEmpty == false`, "yalnız ilk renkler" testinde `Kind == Color` eklendi.
- Spec'in istediği yeniden yazım: `Set_UndefinedColor_ThrowsArgumentOutOfRange` → `OfColor_UndefinedColor_ThrowsArgumentOutOfRange` (aynı 6 değer; `Cell.OfColor` içeride, `grid.Set` lambda içinde), `Set_UndefinedColor_LeavesCellUnchanged` → `OfColor_UndefinedColor_LeavesGridCellUnchanged`.
- Yalnız ad değişti (`Null` → `Empty`): `Set_Empty_ClearsCell`, `Set_Empty_OtherCellsKeepTheirColors`, `Set_EmptyOnEmptyCell_StaysEmpty`, `Set_EmptyOutside3x2_...`, `Set_ColorThenEmptyThenColor_...`, `Set_EachDefinedColorAndEmpty_StillAccepted`.

## Spec maddesi → test
- **Cell: Empty == default, Kind/IsEmpty**: `Empty_Kind_IsEmpty`, `Empty_IsEmpty_ReturnsTrue`, `Default_KindIsEmptyAndIsEmptyTrue`, `Empty_EqualsDefault`, `CellKind_EmptyIsZero`.
- **OfColor, altı renk**: `OfColor_DefinedColor_KindColorAndColorMatch`. **Tanımsız (6, 7, 99, -1, min, max)**: `OfColor_UndefinedColor_ThrowsArgumentOutOfRange` (+ GridTests'teki geçirilmiş ikizi).
- **Color okuma**: `Empty_Color_ThrowsInvalidOperation`, `Default_Color_...`.
- **Eşitlik (`Equals`×2, `GetHashCode`, `==`, `!=`)**: `Equals_RedAndEmpty_...`, `Equals_RedAndDefault_...`, `Equals_SameColorBuiltTwice_...`, `Equals_EveryPairOfDifferentColors_...`, `EqualityMembers_AllPairsOfSevenValues_AreConsistent`, `EqualsObject_Null/OtherTypes`, `GetHashCode_*`, `HashSet_...`, `Copy_OfCell_...`.
- **ToString**: `ToString_Empty_IsEmpty`, `ToString_Colored_IsColorWithName`.
- **CellPos** (ctor, X/Y, eşitlik, `==`, hash, `ToString "(x, y)"`): `CellPosTests` tamamı.
- **Grid Get/Set/GridFactory Cell'e geçiş**: `GridTests`, `GridFactoryTests` (yukarıdaki geçiş).
- **FindGroup: 4 yön, çaprazsız**: `Example2Diagonal`, `Checkerboard3x3`, `SingleSurroundedByOtherColor...`.
- **Sıra ve tekrarsızlık**: her `AssertGroup` çağrısı tam sıra + tekrar kontrolü yapar (`TestGrid.AssertGroup`); `Example1RedGroup/GreenGroup` her üyeden başlatır.
- **Boş başlangıç**: `StartOnEmptyCell`, `EmptyOneByOne`, `AllEmptyGrid`. **Boş hücre böler**: `EmptyCellBetweenSameColor`, `EmptyColumnSplitsVertically`, `EmptyCellsInsideRegion`.
- **Tek kare**: `Example1LoneBlue`, `OneByOne`. **Hata**: `NullGrid`, `NullGridAndBadCoordinates` (kontrol sırası), `OutsideCoordinates` (-1, W, H, swap, int uçları), `OneByOne_NeighbourCoordinatesThrow`, `SixteenBySixteen_JustOutsideCorners`, `OutsideCoordinates_LeavesGridUnchanged`.
- **Grid değişmez**: `Call_LeavesEveryCellUnchanged`, `OnEmptyCell_LeavesGridUnchanged`. **Önbellek/sonuç bağımsızlığı**: `SameCallTwice`, `AfterGridChanges`, `EarlierResult_IsNotAffected...`.
- **Köşe/kenar**: `Uniform3x3_AnyStartCell` (9 hücre), `UniformWide4x2`, `UniformTall2x4`, `RowEndNextToNextRowStart_DoesNotWrapAround`.
- **1x1, 1xN, Nx1**: `OneByOne`, `UniformSingleColumn/Row`, `ColumnBroken/RowBroken`, `UniformSixteenWide/TallStrip`. **Tüm grid tek renk**: `Uniform2x2`, `Uniform3x3`, `Example3UniformSixteenBySixteen`.
- **İki bağlantısız bölge**: `SameColorTwoRegions`, `CrossShape`. **Kıvrımlı yol**: `UShape`, `Snake5x5` (iki uçtan), `SerpentineSixteenBySixteen` (136 hücre), `ColumnStripes`. **Halka**: `RingAroundOtherColor`, `OuterRingAndInnerSameColor`.
- **x/y karışması**: `Wide4x2`, `Tall2x4`, `RectangularGridWithDistinctCells`, ayrıca ölçüt olarak 3x2'de swap'lı grid dışı koordinatlar.
- **Blast: örnek 1, 2, 3**: `Example1RedGroup`, `Example1GreenGroup`, `Example1LoneBlue`, `Example2Diagonal`, `Example3UniformSixteenBySixteen`.
- **Tam 2 patlar, 1 patlamaz**: `GroupOfExactlyTwoHorizontal/Vertical`, `OneByOne`, `SingleSurrounded...`, `EachColorPair`, `MinGroupSize_MatchesSpec_Two`.
- **Gruba girmeyen hücre değişmez, düşme yok**: `TwoRegionsSameColor`, `CrossShape`, `UShape`, `RingAround...`, `EmptyCellsAroundGroup_StayEmptyAndNothingFalls`, `Checkerboard3x3`.
- **Boş başlangıç / ikinci Blast**: `StartOnEmptyCell`, `AllEmptyGrid`, `SameCellTwice`, `EveryCellOfBlastedGroup_NowGivesEmptyList`, `TwoGroupsOneAfterAnother`, `GroupBetweenTwoOthers`.
- **Dönen liste**: `Result_MatchesFindGroupBeforeBlast`, `EveryReturnedCellHadTheTouchedColor...`, `Result_IsNotAffectedByLaterBlastsOrSets`.
- **Blast hataları ve grid değişmezliği**: `NullGrid`, `NullGridAndBadCoordinates`, `OutsideCoordinates_...AndLeavesGridUnchanged` (büyük gruplu grid, yanlış kısmi patlatma yakalanır), `OneByOne_NeighbourCoordinatesThrow`, `SixteenBySixteen_JustOutsideCorners...`.
- **Yardımcı doğruluğu (`TestGrid`)**: `GridBuilderTests` (üst satır önce, `y` alttan üste, `.` boş, 6 harf, `Dump` gidiş-dönüş, hatalı girdi).

## Belirsizlikler ve yazılmayan testler
- **Sonuç listesinin "yeni örnek" olması:** Görev tarifi `FindGroup` sonucunun her çağrıda yeni olmasını önerdi; spec bunu söylemiyor (boş sonuçta paylaşılan boş liste de geçerli olabilir). `NotSame` testi yazmadım; yerine içerik aynılığı, eski sonucun grid değişince bozulmaması ve önbellek olmaması test edildi.
- **Hata mesajı / `ParamName`:** spec yalnız tür söylüyor; mesaj ve parametre adı test edilmedi.
- **`Cell.OfColor` hata sonrası `Grid.Set`:** `Cell` yalnız geçerli yolla kurulduğu için "Set tanımsız renk" yolu yok; Adım 1'deki "hata sonrası hücre değişmez" testi bu yüzden `OfColor` hatasının grid'e hiç ulaşmadığını gösteriyor.
- **`default(CellPos)`:** (0, 0) kabul edildi; spec açıkça söylemiyor ama `readonly struct` varsayılanı.
- **Hash değeri:** yalnız eşit nesnelerin eşit hash'i test edildi; farklı nesnelerin farklı hash'i spec'te yok.
- **`Cell` ile `CellKind` değerleri:** `Special`/`Obstacle` Adım 6 ve 7'de; test yok.
- **`GridFactory.CreateRandom` ve `Cell.OfColor` tanımsız rengi:** `GridFactory` zaten kaynağın aralık dışı değerini `InvalidOperationException` ile reddediyor (Adım 1, değişmedi); yeni test eklenmedi.
- **Satır sonu:** `GridFactoryTests.cs` git uyarısı verdi (CRLF → LF); içeriği etkilemez, orchestrator isterse normalize eder.
