# Adım 2 kontrol raporu (Claude kontrolcü), dal `d1620a9`

## Test
`dotnet test tests/BlastGame.Tests.slnx`:
`Başarılı! - Başarısız: 0, Başarılı: 506, Atlanan: 0, Toplam: 506`

## Bulgular
Yüksek ya da orta önemde bulgu yok.

1. **Düşük, eksik test (testçi):** `GetHashCode` yalnız "eşitse aynı hash" yönünü deniyor (`CellTests.cs:190`, `:213`). `GetHashCode`'u sabit 0 yapmak yakalanmadı (M13). Spec yalnız tutarlılık ister, yani davranış olarak hata değil. Öneri: "farklı değerler çoğunlukla farklı hash verir" testi isteğe bağlı.
2. **Düşük, kod (kodcu):** `BlastRules.Blast` `FindGroup`'un döndürdüğü `List<CellPos>`'i aynen geri veriyor (`BlastRules.cs:220`). Çağıran `IReadOnlyList`'i `List`'e cast edip değiştirebilir. Spec buna bir şey demiyor, bu yüzden zorunlu değil.
3. **Düşük, yapı (testçi):** `OfColor_UndefinedColor_ThrowsArgumentOutOfRange` hem `CellTests`'te hem `GridTests`'te var (M11'de ikisi de kırıldı). Sorun değil, yalnız çift kapsama.
4. **Düşük, görüntü (`GameBootstrap.cs:131`):** `Mathf.RoundToInt` .5'te en yakın çifte yuvarlar (banker's rounding), yani tam -0.5 ya da 7.5 ham değerinde komşu hücre seçilebilir. Pratikte float ile bu noktaya tam denk gelinmez. Düzeltme gerekmez.
5. **Bilgi (`GameBootstrap.cs:114-118`):** Aynı karede birden fazla `Began` dokunuşu hepsi ayrı patlatma yapar. Spec "her karede yalnız `Began` olanlar" diyor, uyumlu.

## Spec karşılaştırması
- **Sözleşme değişikliği:** `Cell` `readonly struct`. `Empty` ve `default` aynı, `OfColor` `Enum.IsDefined` ile doğruluyor, `Color` `InvalidOperationException` atıyor, eşitlik ve `ToString` spec'teki gibi. `Equals` önce tür karşılaştırıyor, bu yüzden `Empty` ile `OfColor(Red)` eşit değil. `Grid`'de `Cell[,]`, `GridFactory`'de `Cell.OfColor`. Uymayan yok.
- **Grup bulma:** 4 yön, başlangıç dahil, çıktı y sonra x sırasında (`visited` dizisini taramayla topluyor), `Empty` başlangıç boş liste, `null` önce sonra koordinat kontrolü, grid'e dokunmuyor. Yığınlı DFS kullanıyor, 16x16'da sorun yok. Uymayan yok.
- **Patlatma:** `MinGroupSize = 2`, `Count < 2` ise grid'e dokunmadan boş liste, yoksa hepsini `Empty` yapıp `FindGroup` sırasını döndürüyor. Hata durumunda `FindGroup` fırlatıyor, `Set`'e gelinmiyor. Uyumlu.
- **Görüntü (yalnız okuma):** `Input.touchCount > 0` ise yalnız `TouchPhase.Began`, değilse `GetMouseButtonDown(0)`, çift işleme yok. Ekran → dünya `ScreenToWorldPoint`, `z = -cam.z`, kamera ortografik ve -10'da (`CameraFitter`), `RoundToInt`, `IsInside` değilse çıkış. `Blast` çağrılıyor, görüntüde grup boyu ya da komşuluk kontrolü yok. `Refresh` yeni nesne ya da doku üretmiyor (tek sprite `Build`'de, sonra yalnız `enabled` ve `color`), grid kopyası tutulmuyor. Uymayan yok. Çalıştırılmadı, yalnız okundu.

## Kaçan uç durum
Spec'teki uç durum listesini `GroupFinderTests` (53 test) ve `BlastRulesTests` ile eşledim. Köşe ve kenar, 1x1, 1xN ve Nx1, tüm grid tek renk, çapraz, U, spiral ve halka, boş başlangıç, boş hücre bölmesi, ikinci `Blast`, `null` ve grid dışı, `FindGroup` sonrası değişmeme, tekrar yok ve sıra, hepsi karşılanıyor. Spec'in istediği bir şey eksik değil.

## Kural 4
`Assets/Scripts/Core/` altında `UnityEngine`, `System.Random`, `record`, `init`, `is not`, `with {`, `??=`, `new()`, `required` taraması boş döndü. `Core.asmdef` `noEngineReferences: true`, csproj'lar `LangVersion 9.0`. İhlal yok.

## Test dokunulmazlığı
`git log --oneline main..HEAD -- tests/` yalnız `28cf7e9 test(core)` gösterdi. `b06c5f0`, `5850ef2`, `b9fe813`, `d1620a9` için `git show --stat -- tests/` boş. Temiz.

## Sözleşme geçişi
`git show 28cf7e9 -- tests/Core.Tests/GridTests.cs tests/Core.Tests/GridFactoryTests.cs` diff'i okundu. Değişiklikler yalnız `TileColor?` → `Cell`, `null` → `Cell.Empty`, `Assert.Null` → `Assert.Equal(Cell.Empty, ...)` ve `Set_UndefinedColor*` → `OfColor_UndefinedColor*` yeniden adlandırması. `[Fact]`/`[Theory]` sayısı eski ile aynı (GridTests 39→39, GridFactoryTests 21→21). Silinen ya da zayıflayan test yok. `Assert.NotNull(...)` → `Assert.False(IsEmpty)` ve `Assert.Equal(Kind == Color)` denk ya da daha sıkı.

## Bilerek bozma (her biri sonrası `git checkout -- Assets/`)
| # | Bozma | Sonuç |
|---|---|---|
| M1 | Yukarı komşuyu çıkar | 53 kırıldı |
| M2 | Çapraz komşu ekle (x+1, y+1) | 13 kırıldı |
| M3 | `Count < Min` → `<=` | 11 kırıldı, ör. `Blast_EachColorPair_BlastsAndLeavesOtherCell` |
| M4 | `Cell.Equals` renk karşılaştırmasını kaldır (`return true`) | 87 kırıldı |
| M5 | Boş başlangıç kontrolünü kaldır | 6 kırıldı, ör. `FindGroup_EmptyOneByOne_ReturnsEmptyList` |
| M6 | Sonuç sırasını y azalan yap | 72 kırıldı |
| M7 | `null` grid kontrolünü kaldır | 4 kırıldı, ör. `Blast_NullGrid_ThrowsArgumentNull` |
| M8 | `FindGroup` koordinat kontrolünü kaldır | **yakalanmadı**; eşdeğer mutant: `Grid.Get` aynı `ArgumentOutOfRangeException`'ı atıyor, yalnız parametre adı ve mesaj değişir |
| M9 | `Blast` son hücreyi boşaltmıyor | 35 kırıldı, ör. `Blast_Uniform2x2_BlastsAllFour` |
| M10 | `Equals` tür farkını yok say (`Empty == OfColor(Red)`) | 8 kırıldı, ör. `EqualityMembers_AllPairsOfSevenValues_AreConsistent` |
| M11 | `OfColor` doğrulamasını kaldır | 13 kırıldı, ör. `OfColor_UndefinedColor_ThrowsArgumentOutOfRange` |
| M12 | `Color` boş hücrede fırlatmıyor | 2 kırıldı, ör. `Empty_Color_ThrowsInvalidOperation` |
| M13 | `GetHashCode` sabit 0 | **yakalanmadı** (bulgu 1) |
| M14 | `CellPos.Equals` yalnız X | 2 kırıldı, ör. `Equals_DifferentY_ReturnsFalse` |
| M15 | `Grid.Set` (0,0)'ı (1,0)'a da yazıyor | 5 kırıldı, ör. `Set_FourCorners_...` |
| M16 | `Blast` Empty yerine Red yazıyor | 37 kırıldı |
| M17 | `Blast` sonuca yinelenen hücre ekliyor | 38 kırıldı |

17 bozmanın 15'i yakalandı, 2'si yakalanmadı: M8 eşdeğer (sorun değil), M13 bulgu 1.

## "Sorun bulamadım" alanları
- **Core mantığı:** spec maddeleri tek tek kodla karşılaştırıldı, 17 bozma ve uç durum listesi eşlemesi denendi.
- **Görüntü katmanı:** `GameBootstrap`, `GridView`, `CameraFitter` okundu. Çalıştırılamadığı için girdi akışı ve dokunma-hücre dönüşümü yalnız kod okumasıyla değerlendirildi; elle kontrol listesi madde 3-6 (Editor ve iPhone) hâlâ ürün sahibine kalıyor.

Worktree temiz (`git status --short` boş), commit atılmadı.
