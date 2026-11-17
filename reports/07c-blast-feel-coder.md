# Rapor 07c: patlama hissi (kodcu)

Dal `step/07c-blast-feel`. Commit yok.

## Değişen dosyalar
- `Assets/Scripts/View/GridView.cs`: halka halka grup patlaması (BFS, 4 komşu), komşu engel vuruşu ilk komşu karenin patlama anında, hedefe uçuş planı ve oynatma, sütun içi kademeli dökülme, tek saatli tur akışı (tur, son uçuş inene kadar sürer).
- `Assets/Scripts/View/HudView.cs`: hedef başına gösterilen sayı (`Shown`) ve tur içi talep (`Claimed`); `TryClaimGoal`, `GoalArrived` (sayı 1 düşer, oturumun değerinin altına inmez, simge "pıt"), `RefreshMoves`, `HideFlyer`. `TryGetGoalScreenPoint` kalktı. Simge pivotu ortaya alındı (pıt yerinde büyüsün; konum aynı).
- `Assets/Scripts/View/GameBootstrap.cs`: dokunuşta yalnız hamle sayısı güncellenir; tur gösterilince `OnTurnShown` HUD'u oturumla eşitler. `SetGoalTarget` yerine `SetGoalFlights`. Seviye değişince açık uçuşlar gizlenir.
- `Assets/Scripts/View/GoalRef.cs` (+ Unity'nin ürettiği `.meta`): renk ya da engel hedefini adlandıran küçük struct.

## Kontroller
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı!  - Başarısız:     0, Başarılı:  1226, Atlanan:     0, Toplam:  1226`
- `scripts/unity-check.ps1`: `Unity compile check: clean.` (exit 0; logda `warning CS`/`error CS` yok)

## Eklenen sabitler (GridView)
| Sabit | Değer | Anlam |
|---|---|---|
| `RingDelay` | 0.03 | halka başına gecikme |
| `RingSpreadMax` | 0.25 | son halkanın en geç başlama zamanı; büyük grupta halka gecikmesi kısalır |
| `FallStagger` | 0.02 | sütunda alttan üste dökülme aralığı |
| `FlyDuration` | 0.45 | uçuş süresi (ease-in, k²) |
| `FlyEndScale` | 0.4 | varıştaki boy (karenin payı) |
| `FlyGap` | 0.02 | kalkışlar arası en az aralık |
| `FlyGapTotal` | 0.3 | bir turdaki bütün kalkışların en geniş yayılımı (çok öğede aralık kısalır) |

HudView: `IconPop` 0.15 sn, `IconPopAmount` 0.3. `AnchorFly` (0.35) artık yalnız hedefe sayılmayan Çapa'nın batıp sönmesinde.

## Spec'ten sapmalar / yorumlar
- Kopya, öğenin patladığı anda yerinde görünür; kalkış aralığı yüzünden gecikirse kalkışa kadar orada bekler (göz kırpması olmasın diye).
- Birleşen gruptaki (özel öğe doğuran) hedef renkli kareler, toplanma bitince birleşme hücresinden uçar.
- Hedefin gösterdiği sayıdan fazla öğe uçmaz (kural katmanı da 0'ın altına saymaz); en erken patlayanlar uçar.
- Çapa: önce eskisi gibi kayar, sonra ortak uçuşla (düz çizgi, ease-in, küçülerek) gider; eski yay ve sönme kalktı.
- Komşu engel vuruşu: ilk komşu karenin başlangıcı + `BlastDuration/2` (önceki zamanlamanın aynı oranı).
- Maske zaten vardı; değiştirilmedi.

## Doğrulanmayanlar
- Görsel his, süreler, akıcılık ve iPhone/WebGL davranışı (elle kontrol listesi 2-6) denenmedi; yalnız derleme ve testler.
- Tur ortasında "Levels" açılırsa süren uçuş kopyaları seviye panelinin üstünde tur bitene kadar görünebilir (Çapa uçuşunda da önceden böyleydi). Öneri: panel açılınca uçuşları gizlemek.

## Ek: kontrol düzeltmeleri
Kontrol raporu `reports/07c-blast-feel-claude.md` bulgu 1 ve 2; sabitler değişmedi, bulgu 3-4'e dokunulmadı.
- Bulgu 1: `GridView.StopTurn()` eklendi (coroutine'i durdurur, uçuşları temizleyip gizler, geri çağrıları boşaltır, `IsAnimating` false). `GameBootstrap.StartLevel` eski tahtayı silmeden önce `StopTurn()` + `SetActive(false)` çağırır; ardından `HideFlyers` ve yeni oturumla `BuildGoals`/`Refresh` (önceden de vardı) sayacı eşitler.
- Bulgu 2: `OnLevels` panel açılınca `HideFlyers` çağırır; `ShowFlyer` panel açıkken kopya çizmez. Tur sürer, kopyalar görünmeden iner, tur sonunda `OnTurnShown` sayacı gerçek değere eşitler.
- `dotnet test`: `Başarılı!  - Başarısız:     0, Başarılı:  1226, Atlanan:     0, Toplam:  1226`
- `unity-check.ps1`: `Unity compile check: clean.` (exit 0)
- Doğrulanmayan: tur ortasında seviye değiştirme elle denenmedi.
