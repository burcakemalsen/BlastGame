# Rapor: Adım 4, hamle, hedef, kazan/kaybet (testçi)

## Dosyalar ve sayı
- `tests/Core.Tests/ColorGoalTests.cs`: 15 test (Theory satırları dahil).
- `tests/Core.Tests/GameSessionTests.cs`: 67 test (Theory satırları dahil).
- Toplam 82 yeni test. Üst sınır ~80'in 2 üstünde: kurucu hata sırası (5 test) ve oyun bitince grid dışı koordinat (4 satır) ayrı davranış sınıfları. Mevcut testlere, `TestGrid` ve `CountingRandom`'a dokunulmadı (yeniden kullanıldı).

## `dotnet test tests/BlastGame.Tests.slnx`
```
Başarısız! - Başarısız:    82, Başarılı:   598, Atlanan:     0, Toplam:   680, Süre: 88 ms - Core.Tests.dll (net10.0)
```
Kalan 82 hata = yeni testlerin hepsi; 598 mevcut test yeşil. Geçen yeni test yok.

Not: `ColorGoal` stub'ı kurucuda `NotImplementedException` atıyor ve `GameSession` testleri hedefi `new ColorGoal(...)` ile kuruyor. Yani GameSession testleri `ColorGoal` uygulanana kadar o yüzden de kırmızı; hata türü bekleyen testler stub hatasıyla yanlışlıkla geçmiyor (hepsi tam tür istiyor).

## Spec maddesi → test
- **Kurulum, başlangıç durumu:** `Ctor_ValidArguments_SetsInitialState`, `Goals_GivenOrder_KeptWithInitialValues`, `Goals_AfterTaps_StillHoldInitialValues`
- **Goals kopyalanır:** `Ctor_GoalsListChangedAfterwards_SessionUnaffected`
- **Kurucu uç değerler:** `Ctor_MovesOneAndSingleGoal_Accepted`, `Ctor_OneGoalPerColor_AllRemainingSet`, `Ctor_GoalColorIsColorCountMinusOne_Accepted`, `Ctor_ColorCountOne_Accepted`
- **Kurucu hataları 1-6:** `Ctor_GridNull_…`, `Ctor_ColorCountOutOfRange_…`, `Ctor_MovesBelowOne_…`, `Ctor_GoalsNull_…`, `Ctor_GoalsEmpty_…`, `Ctor_GoalColorNotInPlay_…`, `Ctor_SameColorTwice_…`, `Ctor_RandomNull_…`
- **Kurucu hata sırası:** `Ctor_EverythingInvalid_ReportsGridFirst`, `…ReportsColorCountBeforeMoves`, `…ReportsMovesBeforeGoals`, `Ctor_GoalsNullAndRandomNull_…`, `Ctor_GoalsEmptyAndRandomNull_…`, `Ctor_BadGoalColorAndRandomNull_…`
- **Hiçbir şeye dokunmadan:** `Ctor_Failure_LeavesGridAndSourceUntouched`
- **Örnek 1-4:** `Tap_Example1_…` (grid, kaynak çağrısı 3, `MaxArgs` {3,3,3}), `Tap_Example2_…`, `Tap_Example3_…` (sonra `Tap(2,2)`: boş sonuç, grid ve çağrı sayısı aynı), `Tap_Example4_…`
- **Hamle:** `Tap_BlastingTap_SpendsExactlyOneMove`, `Tap_EmptyCell_SpendsNoMoveAndSkipsSource`, `Tap_NoBlastOnLastMove_StaysPlayingWithMoveKept`, `Tap_MovesRunOutAfterSeveralTaps_…`, `MovesLeft` negatif olmaz: `Tap_AfterGameOver_…`, `Tap_SeededPlay_MovesNeverNegative…`
- **Hedef sayımı:** `Tap_BlastEqualsRemaining_…`, `Tap_BlastLargerThanRemaining_StopsAtZero`, `Tap_BlastedColorIsNotAGoal_…`, `Tap_BlastedColorIsSecondGoal_…`, `Tap_RefilledTilesOfGoalColor_AreNotCounted`, `Tap_FallenTilesOfGoalColor_AreNotCounted`, `Tap_SeveralBlasts_AccumulateAcrossTaps`
- **Sonuç sırası:** `Tap_GoalsDoneWithMovesLeft_Wins` (MovesLeft > 0 ve Won), `Tap_OneOfTwoGoalsDone_…`, Örnek 2 (son hamlede kazanma önce), Örnek 3 (son hamlede kaybetme)
- **Oyun bitince:** `Tap_AfterGameOver_ChangesNothingAndSkipsSource` (Won ve Lost), `Tap_AfterGameOverOutsideGrid_StillThrowsArgumentOutOfRange`
- **`Tap` koordinat denetimi, x önce:** `Tap_OutsideGrid_ThrowsAndChangesNothing`, `Tap_BothCoordinatesOutside_ReportsXFirst`
- **`Remaining` hata:** `Remaining_ColorNotAGoal_ThrowsArgumentExceptionExactType`
- **Kaynak hatası:** `Tap_SourceReturnsInvalidValue_ThrowsAndCountersStay`
- **Tekrarlanabilirlik:** `Tap_SameSeedSameTaps_GivesSameStates`
- **`ColorGoal`:** ctor `count` 0/negatif/`int.MinValue`, eşitlik (renk, sayı, `object`, null, başka tür, operatörler), `GetHashCode` ve `HashSet`, `ToString`

## Spec'te belirsiz bulduklarım (test yazılmadı)
1. `ColorGoal` kurucusu tanımsız `TileColor` (ör. `(TileColor)99`) için ne yapar: spec yalnız `count`'u denetliyor. Oturum bu rengi "oyunda çıkmayan renk" diye reddeder, o yüzden `Remaining((TileColor)99)` için `ArgumentException` bekledim (spec: "renk hedef değilse").
2. Kurucu hata sırası 4 ile 5 arasında: boş liste ile geçersiz hedef aynı anda olamaz; sorun yok. Ama `goals` içinde `ColorGoal` yerine varsayılan (`default`, `Count = 0`) bir öğe varsa ne olur, spec söylemiyor (`default(ColorGoal)` kurucudan geçmez). Test yok.
3. Kaynak hatasından sonra gridin durumu: spec yalnız hamle ve hedefin değişmediğini söylüyor; grid kısmen değişmiş olabilir (kopya mı alınacak?). Test yazmadım.
4. `Goals` listesinin dışarıdan değiştirilemezliği (salt okunur olması) spec'te yok; yalnız "kurucuya verilen liste kopyalanır" test edildi.
5. Boş hücreye dokunuşun `TurnRules` tarafında "boş sonuç" döndüğü Adım 3 davranışına dayanıyor; `GameSession` testi bunu yeniden doğruluyor.

## Ek
`Ctor_DefaultGoalWithCountZero_ThrowsArgumentExceptionExactType` eklendi (belirsizlik 2, spec ca46110): `default(ColorGoal)` hedefi -> `ArgumentException` tam tür, `ParamName` "goals", kaynak çağrılmaz, grid değişmez. `dotnet test`: Başarısız: 83, Başarılı: 598, Toplam: 681 (yeni testler kırmızı, eskiler yeşil). Yeni test sayısı 83.

## Ek 2
`Tap_AfterGameOver_ChangesNothingAndSkipsSource` düzeltildi: `Remaining` artık oturumun gerçek hedef rengiyle çağrılıyor (`won ? Red : Blue`); önceki hali Lost oturumunda hedef olmayan Red için `ArgumentException` alıyordu. `dotnet test`: Başarılı! - Başarısız: 0, Başarılı: 681, Toplam: 681.
