# Step 1 (Grid): tester report

Okunanlar: `CLAUDE.md`, `docs/RULES.md`, `docs/specs/01-grid.md`, `Assets/Scripts/Core/*.cs` (yalnız imzalar), `tests/Core.Tests/Core.Tests.csproj`, `tests/Core.Tests/SmokeTests.cs`. Uygulama kodu yok, okunmadı.

## Dosyalar ve test sayısı

| Dosya | Test metodu | Test durumu (Theory satırları dahil) |
|---|---|---|
| `tests/Core.Tests/GridTests.cs` | 36 | 114 |
| `tests/Core.Tests/GridFactoryTests.cs` | 17 | 62 |
| `tests/Core.Tests/SeededRandomTests.cs` | 14 | 40 |
| `tests/Core.Tests/TileColorTests.cs` | 2 | 7 |
| **Toplam** | **69** | **223** |

- `tests/Core.Tests/SmokeTests.cs` silindi (spec, "Temizlik").
- Sahte kaynak: `GridFactoryTests` içinde `SequenceRandomSource` (private nested). Verilen diziyi sırayla döndürür, bitince başa döner; her çağrının `maxExclusive` değerini `Calls` listesine yazar.
- Namespace `BlastGame.Core.Tests`, adlar `Metot_Durum_Beklenen`, C# 9. Hata bekleyen testlerin hepsi `Assert.Throws<ArgumentOutOfRangeException>` ya da `Assert.Throws<ArgumentNullException>` (tam tür). "Hata vermez" testleri `Record.Exception` + `Assert.Null`.
- `TileColorTests` ayrı sınıf: tarif üç sınıf sayıyor, ama "tip başına" kuralına göre enum'un kendi sınıfı olması daha temiz. İstenirse `GridTests`'e taşınabilir.

## Spec maddesi → test eşlemesi

### Koordinat sistemi
| Madde | Test(ler) |
|---|---|
| `x` sütun `0..Width-1`, `y` satır `0..Height-1` | `GridTests.IsInside_EveryCellOfNonSquareGrid_ReturnsTrue`, `IsInside_WideGrid_UsesWidthForXAndHeightForY`, `IsInside_TallGrid_UsesWidthForXAndHeightForY`, `Get_WideGrid_YBeyondHeightThrowsEvenIfBelowWidth`, `Get_TallGrid_XBeyondWidthThrowsEvenIfBelowHeight` |
| `x`/`y` yer değiştirmesin | `GridTests.Set_WideGrid_XAndYAreNotSwapped`, `Set_TallGrid_XAndYAreNotSwapped`, `Set_EveryCellWithPositionPattern_ReadsBackSamePattern` (7x3, 3x7, 16x2, 2x16, 16x16), `IsInside_Outside3x2_...` ve `Get_Outside3x2_...` içindeki `(1, 2)` satırı |
| `(0,0)` sol alt, `y` yukarı | `GridFactoryTests.CreateRandom_3x2WithSequence012012_BottomRowThenTopRow`, `CreateRandom_NonPeriodicSequence_FillsRowsFromBottomAndXLeftToRight` (doldurma sırası `y = 0` satırından başlıyor; Grid tek başına "alt"ı gözlemlenebilir kılmıyor, bkz. Belirsizlikler 1) |

### Grid
| Madde | Test(ler) |
|---|---|
| `MinSize = 1`, `MaxSize = 16` | `GridTests.SizeConstants_MatchSpec_MinOneMaxSixteen` |
| `1 ≤ Width, Height ≤ 16`, geçerli boyut | `GridTests.Constructor_ValidSize_SetsWidthAndHeight` (1x1, 16x16, 1x16, 16x1, 3x2, 2x3, 8x10) |
| Boyut dışı → `ArgumentOutOfRangeException` (genişlik ve yükseklik ayrı) | `Constructor_WidthOutOfRange_ThrowsArgumentOutOfRange` (0, 17, -1, `int.MinValue`, `int.MaxValue`), `Constructor_HeightOutOfRange_ThrowsArgumentOutOfRange` (aynı değerler), `Constructor_OneOrBothDimensionsJustOutside_ThrowsArgumentOutOfRange` (17x16, 16x17, 0x1, 1x0, 0x0, 17x17) |
| Yeni grid'in bütün hücreleri boş | `Constructor_3x2_EveryCellIsEmpty`, `Constructor_AnyValidSize_EveryCellIsEmpty` (1x1, 16x16, 1x16, 16x1, 5x9) |
| Hücre ya boş ya bir renk | `Set_EachColor_GetReturnsSameColor` (6 renk), `Set_Null_ClearsCell`, `Set_NullOnEmptyCell_StaysEmpty` |
| Grid dışını okumak → `ArgumentOutOfRangeException` | `Get_Outside3x2_ThrowsArgumentOutOfRange` (11 koordinat), `Get_1x1_NeighboursOfOriginThrow`, `Get_16x16_JustOutsideCornersThrow`, `Get_WideGrid_...`, `Get_TallGrid_...` |
| Grid dışına yazmak → `ArgumentOutOfRangeException` | `Set_ColorOutside3x2_ThrowsArgumentOutOfRange` (11 koordinat), `Set_NullOutside3x2_ThrowsArgumentOutOfRange` (4 koordinat; `null` yazmak da sınır kontrolünden geçmeli), `Get_WideGrid_...`, `Get_TallGrid_...` |
| `IsInside` hata vermez, içerideyse `true` | `IsInside_EveryCellOfNonSquareGrid_ReturnsTrue`, `IsInside_Corners3x2_ReturnsTrue`, `IsInside_Outside3x2_ReturnsFalseWithoutThrowing` (17 koordinat, `int.MinValue`/`int.MaxValue` dahil), `IsInside_1x1_OnlyOriginInside`, `IsInside_16x16_CornersInsideAndJustOutsideFalse` |

### Renkler
| Madde | Test(ler) |
|---|---|
| Altı renk, bu sırayla, `0..5` | `TileColorTests.TileColor_Value_MatchesSpecOrder` (6 satır), `TileColor_HasExactlySixValues` |
| İlk `colorCount` renk kullanılır | `GridFactoryTests.CreateRandom_SeededSource16x16_OnlyFirstColorCountColorsAppear` (1, 2, 3, 5, 6), `CreateRandom_SourceValue_MapsToColorWithSameNumber` (0..5 → Red..Orange) |

### Rastgele kaynak
| Madde | Test(ler) |
|---|---|
| `SeededRandom` bir `IRandomSource` | `SeededRandomTests.SeededRandom_ImplementsIRandomSource` |
| `0 ≤ Next(max) < max` | `Next_ManyDraws_StayWithinZeroAndMaxExclusive` (2, 3, 6, 7, 100, `int.MaxValue`; 2000 çekiliş), `Next_Two_ProducesBothValues` |
| `maxExclusive ≤ 0` → `ArgumentOutOfRangeException` | `Next_MaxZeroOrNegative_ThrowsArgumentOutOfRange` (0, -1, `int.MinValue`) |
| Aynı seed → aynı dizi | `Next_SameSeed_SameFirst100Values` (42, 0, -1, `int.MinValue`, `int.MaxValue`), `Next_SameSeedWithSmallMax_SameFirst100Values` |
| Her `int` seed geçerli (negatif, 0 dahil) | `Constructor_AnyIntSeed_DoesNotThrowAndCanDraw` (0, 1, -1, 42, `int.MinValue`, `int.MaxValue`) |
| SplitMix64, standart sabitler | `Next_Seed0_MatchesSplitMix64ReferenceOutputs`, `Next_Seed0_FirstValueIsReferenceOutputModuloMax` (2, 6, 1000, 65536, 1000000007) |
| Başlangıç durumu `(ulong)(long)seed` | `Next_SeedMinusOne_UsesSignExtendedInitialState` (`-1` → `0xFFFFFFFFFFFFFFFF`; `(ulong)(uint)seed` gibi yanlış dönüşümü yakalar) |
| `Next(max) = (int)(çıktı % (ulong)max)`, çağrı başına bir çıktı | `Next_Seed0_FirstValueIsReferenceOutputModuloMax`, `Next_DifferentMaxPerCall_EachCallConsumesOneOutput` |

### Rastgele doldurma
| Madde | Test(ler) |
|---|---|
| Her hücreye `(TileColor)random.Next(colorCount)` | `CreateRandom_SourceValue_MapsToColorWithSameNumber`, `CreateRandom_CallsNextExactlyOncePerCellWithColorCount` (3x2/3, 1x1/1, 1x1/6, 16x16/6, 8x10/5, 16x1/4, 1x16/2: çağrı sayısı = `W*H`, her çağrıda `maxExclusive = colorCount`) |
| Doldurma sırası `y` alttan, `x` soldan | `CreateRandom_3x2WithSequence012012_BottomRowThenTopRow`, `CreateRandom_NonPeriodicSequence_FillsRowsFromBottomAndXLeftToRight` (4x3, 3x4, 5x2, 2x5, 16x16, 16x1, 1x16; dizi sütun-öncelikli, yukarıdan-aşağı ya da sağdan-sola doldurmada farklı grid verecek şekilde seçildi ve bu, yazmadan önce ayrı bir betikle kontrol edildi), `CreateRandom_1x1_UsesFirstValue`, `CreateRandom_SeededSource_MatchesDirectDrawsInFillOrder` |
| Boyut, istenen genişlik/yükseklik | `CreateRandom_ValidSize_ReturnsGridWithThatWidthAndHeight` (7 boyut, genişlik/yükseklik karışmasını yakalar) |
| `colorCount < 1` ya da `> 6` → `ArgumentOutOfRangeException` | `CreateRandom_ColorCountOutOfRange_ThrowsArgumentOutOfRange` (0, 7, -1, `int.MinValue`, `int.MaxValue`), sınırlar geçerli: `CreateRandom_ColorCountAtBounds_DoesNotThrow` (1, 6) |
| `random == null` → `ArgumentNullException` | `CreateRandom_NullRandom_ThrowsArgumentNull` |
| Boyut kuralları Grid'deki gibi | `CreateRandom_SizeOutOfRange_ThrowsArgumentOutOfRange` (0, 17, -1, `int.MinValue`, `int.MaxValue`; genişlik ve yükseklik ayrı; 17x16, 16x17) |
| Boş hücre kalmaz | `CreateRandom_SeededSource_NoEmptyCells` (1x1, 8x10, 16x16, 1x16, 16x1), `CreateRandom_SeededSource16x16_OnlyFirstColorCountColorsAppear` (`NotNull`) |
| Her çağrı ayrı grid | `CreateRandom_TwoCalls_ReturnIndependentGrids` |
| `MaxColorCount = 6` | `GridFactoryTests.MaxColorCount_MatchesSpec_Six` |

### Örnekler
| Örnek | Test |
|---|---|
| `new Grid(3, 2)` → boyutlar, hepsi `null` | `Constructor_ValidSize_SetsWidthAndHeight(3, 2)`, `Constructor_3x2_EveryCellIsEmpty` |
| `Set(2, 1, Blue)` → `Get == Blue`, diğerleri değişmez | `Set_Blue_At2And1_GetReturnsBlue`, `Set_OneCell_OtherCellsStayEmpty` |
| `Set(2, 1, null)` → yeniden boş | `Set_Null_ClearsCell` |
| `Get(3,0)`, `Get(-1,0)`, `Get(0,2)` hata | `Get_Outside3x2_ThrowsArgumentOutOfRange` (ilk üç satır) |
| Sahte kaynak `0,1,2,0,1,2` → 3x2 grid | `CreateRandom_3x2WithSequence012012_BottomRowThenTopRow` |
| `CreateRandom(8, 10, 5, new SeededRandom(42))` iki kez aynı | `CreateRandom_SameSeedTwice_GridsAreCellByCellEqual` |

### Uç durumlar
| Uç durum | Test(ler) |
|---|---|
| Boyut 0, 1, 16, 17, negatif; genişlik, yükseklik ayrı | `Constructor_ValidSize_...`, `Constructor_WidthOutOfRange_...`, `Constructor_HeightOutOfRange_...`, `Constructor_OneOrBothDimensionsJustOutside_...`, `CreateRandom_SizeOutOfRange_...` |
| Köşeler ve hemen dışları | `IsInside_Corners3x2_ReturnsTrue`, `IsInside_Outside3x2_...`, `IsInside_16x16_...`, `Get_16x16_JustOutsideCornersThrow`, `Set_FourCorners_EachKeepsItsOwnColorAndRestStaysEmpty` (4x3, 3x4, 16x16, 16x1, 1x16) |
| 1x1 grid | `IsInside_1x1_OnlyOriginInside`, `Get_1x1_NeighboursOfOriginThrow`, `Set_1x1_RoundTrips`, `Constructor_AnyValidSize_EveryCellIsEmpty(1, 1)`, `CreateRandom_1x1_UsesFirstValue` |
| 16x16 grid | `IsInside_16x16_...`, `Get_16x16_...`, `Set_FourCorners_...(16, 16)`, `Set_EveryCellWithPositionPattern_...(16, 16)`, `CreateRandom_...` (16x16 satırları) |
| `colorCount` 0, 1, 6, 7 | `CreateRandom_ColorCountOutOfRange_...` (0, 7), `CreateRandom_ColorCountAtBounds_DoesNotThrow` (1, 6) |
| `colorCount = 1` → hepsi `Red` | `CreateRandom_ColorCountOne_WholeGridIsRed`, `CreateRandom_SeededSource16x16_OnlyFirstColorCountColorsAppear(1)` |
| `colorCount = 6`, büyük grid'de yalnız `0..5` | `CreateRandom_SeededSource16x16_OnlyFirstColorCountColorsAppear(6)` |
| `SeededRandom` aynı seed → aynı ilk 100 | `Next_SameSeed_SameFirst100Values`, `Next_SameSeedWithSmallMax_SameFirst100Values` |
| Seed 1 ile 2'nin ilk 10'u aynı değil | `Next_Seeds1And2_First10ValuesDiffer` (`Next(int.MaxValue)` ile; dizi olarak eşit değil) |
| `Next(1)` hep 0 | `Next_One_AlwaysZero` (4 seed, 1000 çekiliş) |
| `Next(0)`, `Next(-1)` hata | `Next_MaxZeroOrNegative_ThrowsArgumentOutOfRange` |
| 6000 çekilişte `Next(6)` her değer ≥ 800 | `Next_Six_6000Draws_EachValueAtLeast800Times` (42, 0, 1, -1) |
| `IsInside` sınır dışında `false`, hata yok | `IsInside_Outside3x2_ReturnsFalseWithoutThrowing`, `IsInside_1x1_...`, `IsInside_16x16_...` |

### Spec listesinin dışında eklenenler
- Aynı hücreye üst üste yazma: `Set_SameCellTwice_LastValueWins`, `Set_ColorThenNullThenColor_ReturnsLastColor`.
- `null` yazınca diğer hücreler korunur: `Set_Null_OtherCellsKeepTheirColors`.
- Bütün hücreleri desenle yazıp geri okuma (indeks karışması, yanlış satır adımı): `Set_EveryCellWithPositionPattern_ReadsBackSamePattern`.
- `Set` boyutu değiştirmez: `Set_DoesNotChangeWidthOrHeight`.
- İki grid hücre paylaşmaz (statik dizi gibi hatalar): `Constructor_TwoGrids_DoNotShareCells`, `CreateRandom_TwoCalls_ReturnIndependentGrids`.

## SplitMix64 referans değerleri
- Seed 0 (durum 0) için ilk üç 64 bitlik çıktı `0xE220A8397B1DCDAF`, `0x6E789E6AA1B965F4`, `0x06C45D188009454F`. Bu, SplitMix64'ün yaygın bilinen referans çıktısı; ayrıca standart algoritmayı (artış `0x9E3779B97F4A7C15`, çarpanlar `0xBF58476D1CE4E5B9`, `0x94D049BB133111EB`, kaydırmalar 30/27/31) test dışında bağımsız bir betikle hesaplayıp aynı değerleri aldım. Testler bunu `Next(int.MaxValue)` ve birkaç başka `max` üzerinden `çıktı % max` olarak kontrol ediyor.
- Seed -1 değerleri (`0xE4D971771B652C20`, `0xE99FF867DBF682C9`, `0x382FF84CB27281E9`) yayımlanmış bir referanstan değil, aynı betikle `0xFFFFFFFFFFFFFFFF` durumundan hesaplandı. Betik seed 0'da referansla birebir tuttuğu için güveniyorum, ama bu değerin dış kaynağı yok.

## `dotnet test` özeti
Komut: `dotnet test tests/BlastGame.Tests.slnx` (derleme uyarısız):
```
Başarısız! - Başarısız:   214, Başarılı:     9, Atlanan:     0, Toplam:   223, Süre: 81 ms - Core.Tests.dll (net10.0)
```
Kalan 214 testin hata mesajları (trx dökümünden sayıldı):
- 174: `System.NotImplementedException` (stub doğrudan atıyor).
- 32: `Assert.Throws() Failure: Exception type was not an exact match` (beklenen `ArgumentOutOfRangeException`/`ArgumentNullException`, gelen `NotImplementedException`; tam tür kontrolü çalışıyor).
- 8: `Assert.Null() Failure` (`Record.Exception` ile "hata vermez" testleri: `Constructor_AnyIntSeed_...` ×6, `CreateRandom_ColorCountAtBounds_...` ×2; yakalanan istisna `NotImplementedException`).

Yani test kodundan kaynaklı başka bir hata yok; kırmızının hepsi stub'dan.

Geçen 9 test ve gerekçesi (hepsi derleme zamanı sabitini ya da enum değerini ölçüyor, stub'da da doğru oldukları için yeşil; sözleşmenin bozulmasını yakalamak için tutuldu):
- `GridTests.SizeConstants_MatchSpec_MinOneMaxSixteen` (`const`)
- `GridFactoryTests.MaxColorCount_MatchesSpec_Six` (`const`)
- `TileColorTests.TileColor_Value_MatchesSpecOrder` ×6 (enum değerleri)
- `TileColorTests.TileColor_HasExactlySixValues` (enum üye sayısı)

## Belirsizlikler ve yazılmayan testler
1. **"Alt" Grid'de gözlemlenemiyor.** `Grid` yalnız `(x, y)` ile okunuyor; `(0,0)`'ın sol alt olduğu ancak doldurma sırası ve görüntü katmanıyla anlam kazanıyor. Grid için ayrı bir "alt" testi yazılamaz; doldurma sırası testleri ve Adım 3'teki düşme testleri bunu taşıyacak.
2. **Birden fazla geçersiz argümanda hangi hata önce?** Örn. `CreateRandom(0, 5, 3, null)`: `ArgumentOutOfRangeException` mı `ArgumentNullException` mı? Spec söylemiyor; test yazılmadı. Her hata testinde diğer argümanlar geçerli tutuldu.
3. **Geçersiz argümanda kaynak çağrılır mı?** `CreateRandom` hata vermeden önce `random.Next` çağırmamalı mı (kaynağın durumu ilerler)? Spec söylemiyor; test yazılmadı. Öneri: "doğrulama önce, çekiliş sonra" spec'e eklenebilir.
4. **Kaynak aralık dışı değer verirse?** Sahte kaynak `Next(colorCount)` için `colorCount` ya da daha büyük değer dönerse `CreateRandom` ne yapar (olduğu gibi `(TileColor)7` yazmak, hata vermek)? Spec "her hücreye `(TileColor)random.Next(colorCount)` yazar" diyor, doğrulama tarif etmiyor; test yazılmadı.
5. **Hatalı `Set` sonrası durum.** Grid dışına `Set` hata verdikten sonra grid'in değişmemiş olması beklenir ama spec söylemiyor; test yazılmadı.
6. **`Set` ile tanımsız enum değeri** (`(TileColor)6` ya da `-1`): spec yalnız "boş ya da bir renk" diyor, kabul mü hata mı belirtmiyor; test yazılmadı.
7. **Seed 1 ve 2 testi `max` seçimi.** Spec "ilk 10 değer aynı değil" diyor ama hangi `max` ile çekileceğini söylemiyor. `Next(int.MaxValue)` kullandım (küçük `max`'te tesadüfi eşitlik olasılığını düşürmek için). Test, dizinin eleman eleman hepsinin farklı olduğunu değil, iki dizinin eşit olmadığını kontrol ediyor.
8. **Kaba eşitlik eşiği seed'e bağlı.** 6000 çekilişte beklenen 1000, standart sapma ~29; 800 eşiği ~7 sapma aşağıda. Doğru bir SplitMix64 ile dört seed'de de yanlışlıkla kırmızı olma olasılığı ihmal edilebilir, ama değerler uygulama gelmeden görülemedi.
