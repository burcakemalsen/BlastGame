# Adım 3 kontrol raporu (Claude kontrolcü)

Commit `6e75493`, dal `step/03-gravity-refill`.

## Test
`Başarılı!  - Başarısız:     0, Başarılı:   591, Atlanan:     0, Toplam:   591`

## Bulgular
1. **Orta, eksik test → testçi.** `TurnRules.cs:12-21`, `RefillRules.cs:16-20`: Tap'te `colorCount` ile koordinat kontrolünün yeri değiştirilince (M7) ve `grid`/`random` null kontrolü yer değiştirince (M10) hiçbir test kırılmıyor. Fill'de `grid`/`random` saf yer değişimi de aynı durumda (M6 yalnız `colorCount`'u da geçtiği için yakalandı). Spec sırayı istiyor ("Bir dokunuş", "Doldurma"); aynı türdeki hataları yalnız `ParamName` ayırt eder. Kod zaten `nameof` kullanıyor, ama spec `ParamName`'i yalnız `TurnResult` için tanımlıyor. Öneri: spec'e Fill/Tap için `ParamName` satırı, testçi sıra testlerine `ParamName` iddiası.
2. **Düşük, eksik test → testçi.** "Patlama olmadıysa kaynak çağrılmaz" kuralını (M5: patlama olmasa da `Fill` çağır) yalnız `Tap_EmptyCell_...` yakaladı; tek kare dokunuşunda grid'de boş olmadığı için Fill zaten çağrı yapmıyor. Boşluğu olan grid'de tek kareye dokunuş testi bu kuralı daha doğrudan sınar.

Kod hatası bulamadım.

## Spec karşılaştırması (madde madde okundu, uyumlu)
- Düşme: sütun başına, sıra korunuyor, yerinde kalan listede yok, sıra x sonra To.Y, null kontrolü.
- Doldurma: kontrol sırası doğru; y sonra x; boş hücre başına bir çağrı; önce çek ve denetle, sonra yaz.
- Dokunuş: sıra ve hatalar doğru; patlama yoksa üç boş liste, kaynak çağrılmıyor.
- Arayüz: imzalar, `ToString`, `TurnResult` null sırası ve kopyasız saklama doğru; mevcut Core dosyaları değişmemiş.
- Görüntü (yalnız okudum): tek `SeededRandom(42)`; `TurnRules.Tap` + `Refresh`; sprite `static` alanda bir kez; Backlog maddesi silinmiş.

## Kural 4
Core'da `UnityEngine`, `System.Random`, `record`, `init`, `??=` grep'i boş; `LangVersion` 9.0.

## Test dokunulmazlığı
`main..HEAD -- tests/` yalnız `9a7b78a test(core)`. Stub, `feat(core)` ve `feat(view)` commit'lerinde `tests/` boş. Test commit'i yalnız yeni dosya ekliyor (A).

## Bilerek bozma
| # | Bozma | Yakalayan |
|---|---|---|
| M1 | Collapse sütunları ters | `Collapse_Example1_...` +3 |
| M2 | Yerinde kalanı listeye ekle | `Collapse_NoEmptyCells_...` +9 |
| M3 | Fill x-önce | `Fill_Example2_...` +4 |
| M4 | Fill denetlemeden yaz | `Fill_SourceReturnsOutOfRange_...` +3 |
| M5 | Tap patlamasız da Fill | `Tap_EmptyCell_...` (yalnız 1) |
| M6 | Fill'de `random` kontrolü en başa | `Fill_BadCountAndNullRandom_...` |
| M7 | Tap koordinatı `colorCount`'tan önce | **yakalanmadı** |
| M8 | `CellMove.Equals` `To`'yu yok sayar | `Equals_OneCoordinateDiffers_...` |
| M9 | Tap `random` kontrolü Blast'tan sonra | `Tap_NullRandom_ThrowsGridUntouched` +1 |
| M10 | Tap `random` kontrolü `grid`'den önce | **yakalanmadı** |
| M11 | Fill `colorCount` üst sınırı 5 | `Fill_ColorCountSixValueFive_Orange` +5 |
| M12 | Collapse eski hücreyi boşaltmıyor | 13 test |

Her bozmadan sonra `git checkout -- Assets/`; rapor dışında değişiklik yok.
