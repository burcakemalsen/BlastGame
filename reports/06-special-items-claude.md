# Adım 6 Claude kontrolcü raporu: özel öğeler

HEAD: `46acef1 feat(view): draw and animate special items`.

## Test özeti
```
Başarılı!  - Başarısız:     0, Başarılı:   913, Atlanan:     0, Toplam:   913, Süre: 395 ms - Core.Tests.dll (net10.0)
```

## Bulgular
1. **Orta, eksik test → testçi.** `SpecialRules.cs:79,87`, spec "Etkiler ve alanları / Rocket: tahtanın ucuna kadar". Döngü sınırı `d < grid.Width || d < grid.Height`; kod doğru. Ama iki döngüyü de `d < grid.Width` yapınca 913 test yeşil kalıyor (bozma 9). Yüksekliği genişliğinden büyük bir tahtada dikey Rocket'ın en uzak üst/alt hücreye kadar temizlediğini sınayan test yok (ör. 3x6 tahtada `(1,1)`'de `V`, `(1,5)` temizlenmeli; 1x4 tahtada `V` bütün sütunu temizlemeli). Yalnız `d < grid.Height` bozması yakalanıyor, çünkü test tahtaları geniş.
2. **Düşük, not.** `BlastRules.Blast` artık `TurnRules` tarafından çağrılmıyor (kodcu da yazmış). Spec'e aykırı değil; testleri yeşil. Backlog adayı.

Başka kod hatası bulamadım.

## Bilerek bozma
| # | Bozma | Sonuç |
|---|---|---|
| 1 | `ForGroup`: `size < 5` → `size <= 5` | Yakalandı (11; `ForGroup_FiveOrSix_...`, `Tap_GroupSizeThreshold_...`) |
| 2 | Yatay Rocket: sağ önce | Yakalandı (5; `Activate_HorizontalRocketMiddle_...LeftBeforeRight`) |
| 2b | Dikey Rocket: üst önce | Yakalandı (2; `Activate_VerticalRocketMiddle_...DownBeforeUp`, `Tap_Example2...`) |
| 3 | Kuyruk → yığın (LIFO) | Yakalandı (2; `Activate_ChainOfThreeLevels_FifoOrder...`, `Activate_ItemInTwoAreas_...`) |
| 4 | Bomb kendi hücresini grid'de temizlemiyor | Yakalandı (11; Bomb köşe/kenar/orta, Örnek 3) |
| 5 | Yön çekilişi grup temizlendikten sonra | Yakalandı (2; `Tap_InvalidDirectionDraw_ThrowsGridUnchangedCellByCell`) |
| 6 | Orb başka özel hücreleri de temizliyor | Yakalandı (1; `Activate_OrbAmongOtherSpecials_...`) |
| 7 | Bütün renkli temizlik ilk renge sayılıyor | Yakalandı (3; `GameSessionSpecialTests.Tap_Example3Chain_EachColorCounted...`) |
| 7b | Orb kendi rengine sayılıyor | Yakalandı (1; `Tap_Example4Orb_OrbItselfNotCounted`) |
| 8 | Zincirde `TriggeredBy` hep 0 | Yakalandı (1; `Activate_ChainOfThreeLevels_...TriggeredByIndices`) |
| 9 | Rocket döngüsü yalnız `d < Width` | **Yakalanmadı** (bulgu 1) |
| 9b | Rocket döngüsü yalnız `d < Height` | Yakalandı (4) |
| 10 | `Blasted` ters sırada | Yakalandı (8+) |
| 11 | `GroupFinder` özel hücreyi boş saymıyor | Yakalandı (3; `FindGroupAndBlast_OnSpecial_...`) |
| 12 | Bomb/Orb birleşmesinde fazladan `Next(2)` | Yakalandı (8+; `..._WithoutDraw`) |
| 13 | Orb taraması `x` önce | Yakalandı (3; Örnek 4, 5) |

Her bozmadan sonra `git checkout -- Assets/`; son durumda `git status` temiz, 913/913 yeşil. (İlk denediğim "dokunulan renge sayım" bozması eşdeğer çıktı: grupta tek renk var, özel dokunuşta ilk hücre özel; 7 ile değiştirdim.)

## "Sorun bulamadım" dediğim alanlar ve ne denediğim
- **Spec karşılaştırması:** hücre (`OfSpecial`/`OfOrb` hataları, erişim hataları, eşitlik, hash, `ToString`), dokunuşun beş durumu, çekiliş sırası (yön çekilişi grid'den ve doldurmadan önce, Bomb/Orb/özel dokunuşta yok), kendi hücresi `Cleared[0]`, Rocket/Bomb/Orb tarama sırası, kuyrukta olanın atlanması, FIFO, `TriggeredBy`, her hücre tek `Cleared`'da (`queued` dizisi; zincir sırasında düşme yok), `Blasted` birleşimi, renk başına sayım, oyun bitince boş `Effects`, `ForGroup`/`Activate` hata sırası ve `ParamName`, netleştirmeler: hepsi kodla uyumlu.
- **Kural 4:** Core'da `UnityEngine`, `System.Random`, `record`, `init`, `with`, desen eşleme (`is not`, `and`/`or`), switch ifadesi, hedefli `new()` aradım: yok (yalnız metin içinde "is not" geçiyor).
- **Test dokunulmazlığı:** `main..HEAD -- tests/` yalnız `84d166a test(core)`; `288d361`, `5c04ddc`, `46acef1`'de `tests/` boş. Mevcut testlerdeki değişiklik: 16'lık grup testi (artık Orb doğuyor, zorunlu) ve üç yerde `TestGrid.Dump` → `SpecialGrid.Dump` (rastgele tahtada özel hücre `Color` erişiminde patlar, zorunlu). Fazlası yok.
- **Hile:** testin beklediği değeri döndüren özel durum aramak için `SpecialRules`, `TurnRules`, `GameSession`'ı satır satır okudum; yok.
- **Görüntü (okuyarak, çalıştırmadan):** `EndlessMode` tek ayar, doğrudan `TurnRules.Tap`, hamle/hedef `SetActive(false)` ile gizli, kodları duruyor, sonuç paneli koşulu `!EndlessMode`; "Next board" sonuncudan sonra ilk level ve yeni seed, animasyon sürerken `IsAnimating` ile kapalı; dokunuş da kapalı. Zincir başlangıcı `TriggeredBy` + Rocket'ta `uzaklık × 0.025`, Bomb'da 0; düşme bütün etkilerden sonra. Dokular `ShapeSprites`'ta çalışma başına bir kez; geçici nesneler havuzdan, havuz `GridView`'in çocuğu, tahta değişince onunla yok oluyor (sızıntı yok). Orb toplamı en çok 0.1 + 0.28 + 0.12 = 0.5 sn. Elle kontrol (akıcılık, görünüş) doğrulanmadı.
