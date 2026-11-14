# Rapor 07c: patlama hissi (Claude kontrolcü)

Dal ucu `ba944f9`, karşılaştırma `origin/main..HEAD`. Commit yok.

## Test
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı!  - Başarısız:     0, Başarılı:  1226, Atlanan:     0, Toplam:  1226`
- `git diff --stat origin/main..HEAD -- Assets/Scripts/Core tests`: boş.

## Spec maddeleri
1. Halka: BFS 4 komşu, gecikme `min(0.03, 0.25/maxRing)`, toplam yayılma <= 0.25. Uyuyor.
2. Uçuş: sayılan öğe listesi `GameSession.Tap` sayımıyla birebir (Cleared, Converted, kırılan Hits, Collected). Küçülme 1->0.4, ease-in k², 0.45 sn, sayaç varışta düşer, sonunda `Refresh` ile eşitlenir, sonsuz modda `_goalsRoot` kapalı olduğu için uçuş yok. Uyuyor.
3. Dökülme: sütun içi alttan üste 0.02 kademe, kübik ease-in, maske var. Uyuyor.
4. Panel: `IsAnimating` son uçuş inene kadar true. Uyuyor.

## Bulgular
1. **Orta (olası, doğrulanmadı):** `GameBootstrap.cs:142-146`. Tur ortasında "Levels" ile seviye seçilince eski `GridView` `Destroy` ile silinir ama coroutine durdurulmaz. Unity silmeyi kare sonuna bıraktığı için coroutine aynı karede bir kez daha dönebilir. Dönerse `ShowFlyer` kopyayı `HideFlyers`'tan sonra yeniden açar (kopya bir sonraki turun sonuna kadar ekranda kalır), `GoalArrived` da yeni seviyede aynı türden hedefin sayısını 1 düşürür. Çözüm önerisi: silmeden önce `StopAllCoroutines` çağırmak ya da `SetActive(false)` yapmak.
2. **Düşük:** `GameBootstrap.cs:110-115`. Tur ortasında "Levels" açılırsa uçuşlar seviye panelinin üstünde sürer. Kodcu raporu da bunu yazıyor.
3. **Düşük (his):** `GridView.cs:781-792`. Hedef rengindeki karenin kopyası patlama anında tam boyla karenin üstünde belirir. Bu yüzden o karenin büyüyüp kaybolma animasyonu görünmez. Ürün sahibinin denemesinde bakılmalı.
4. **Bilgi:** `GridView.cs:469`. Engel vuruşu ilk komşunun başlangıcından `BlastDuration/2` sonra görünür; spec "patladığı anda" diyor. Kodcu raporu bunu yazıyor.

## Ne denedim
Testleri çalıştırdım, Core/tests diff'ine baktım. View diff'ini satır satır okudum. Sayaç akışını izledim: `Shown`/`Claimed`, `GoalArrived` alt sınırı ve uçuş adayları ile oturum sayımının eşleşmesi. Havuzu kontrol ettim (HUD kopyaları ve Fx yeniden kullanılıyor; tur başına yalnız `Sort` karşılaştırıcısı ayırıyor). "Next level" yalnız tur bittikten sonra çıktığı için yarım uçuş bırakmıyor. Unity, WebGL ve telefon denemesi yapmadım; bulgu 1 çalıştırılarak doğrulanmadı.
