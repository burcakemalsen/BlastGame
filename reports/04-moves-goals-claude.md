# Rapor: Adım 4, hamle, hedef, kazan/kaybet (Claude kontrolcü)

Dal `step/04-moves-goals`, `e7b05f0`.

## Test
```
Başarılı!  - Başarısız:     0, Başarılı:   681, Atlanan:     0, Toplam:   681, Süre: 226 ms - Core.Tests.dll (net10.0)
```

## Spec karşılaştırması
Kurallar, kurucu hataları (sıra, `ParamName`), `ColorGoal`, arayüz sözleşmesi, görüntü katmanı madde madde okundu; sapma yok.

## Bulgular
1. **`GameSession.cs:136`, "Goals ... değişmez", düşük.** `Goals` iç diziyi döndürüyor; `(ColorGoal[])session.Goals` ile öğe değiştirilebilir ve `Remaining`/sayım bozulur. Yönlendirme: kodcu (`Array.AsReadOnly`); test isteğe bağlı, spec bunu açıkça istemiyor (testçinin belirsizlik 4'ü). Karar orchestrator'ın.

Başka bulgu yok.

## Kural 4 ve test dokunulmazlığı
- `grep` Core'da `UnityEngine|System.Random|DateTime|record |init;`: sonuç yok. Seed saatten yalnız `GameBootstrap.cs:62`.
- `git log main..HEAD -- tests/`: yalnız `ad91206`, `5140f1b` (`test(core)`). `d636b8a`, `ce41c98`, `e7b05f0` için `git show --stat -- tests/` boş. `git diff --stat main..HEAD -- tests/`: yalnız iki yeni dosya eklenmiş; Adım 1-3 testleri değişmemiş.

## Bilerek bozma (her birinden sonra `git checkout -- Assets/`)
| # | Bozma | Yakalayan test |
|---|---|---|
| 1 | Boşa dokunuş da hamle düşürür | `Tap_EmptyCell_SpendsNoMoveAndSkipsSource` (+5) |
| 2 | Hedef 0'da durmaz | `Tap_BlastLargerThanRemaining_StopsAtZero`, `Tap_Example1_…` |
| 3 | Kaybetme kazanmadan önce | `Tap_Example2_CompletingGoalOnLastMoveWins` |
| 4 | Bitince `Tap` `TurnRules`'a gider | `Tap_AfterGameOver_…` (Won, Lost), `Tap_Example3_…` |
| 5 | `Equals` `Count`'u yok sayar | `Equals_DifferentCount_IsFalse` |
| 6 | `random` kontrolü ilk sırada | `Ctor_…Reports…First` (6 test) |
| 7 | `Count < 1` hedef kabul | `Ctor_DefaultGoalWithCountZero_…` |
| 8 | Bitince koordinat denetlenmez | `Tap_AfterGameOverOutsideGrid_…` (4 satır) |
| 9 | `goals` kopyalanmaz | `Ctor_GoalsListChangedAfterwards_SessionUnaffected` |
| 10 | `colorCount` üst sınırı `>=` | `Ctor_OneGoalPerColor_AllRemainingSet` |
| 11 | Hedef rengi sınırı `>` | `Ctor_GoalColorNotInPlay_…` (3 satır) |
| 12 | Renk dokunuştan sonra okunur | `Tap_Example1/2`, `Tap_Blast…` (6+) |

12/12 yakalandı. Sonda `git status` temiz.

## Görüntü katmanı (okuyarak, çalıştırılmadı)
- Panel `Update`'te yalnız `!_view.IsAnimating` iken açılıyor; `HandleTap` panel açıkken dönüyor, panel `raycastTarget` ile UI tıklamasını da yutuyor. Uygun.
- "Play again": yeni saat seed'i, yeni `SeededRandom`, grid, `GameSession`; eski `GridView` `Destroy`, hedef öğeleri yeniden kuruluyor. Kare sprite'ı `static` ve tek (`GridView.cs:18`), yeniden başlatmada doku sızmıyor. Buton `onClick` bırakışta tetiklenir, `GetMouseButtonDown` o karede false; tıklama yeni tahtaya geçmez.
- Dokunuş başına yalnız etiket string'leri oluşuyor; nesne yok.
- Seed sol altta ve `Debug.Log`'da.
- WebGL: `LegacyRuntime.ttf` yerleşik font; `activeInputHandler: 0` (eski Input) olduğu için `StandaloneInputModule` uyumlu. Risk görmedim; düzen (bar %12, seed etiketinin tahta kenarıyla çakışması, buton boyutu) yalnız elle kontrolde görülür.
