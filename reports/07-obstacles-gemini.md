# Adım 7, engeller: Gemini kontrolcü raporu

Model: Gemini 4 Argon Medium (`agy -p --model gemini-4-argon-medium`, salt okunur). Çalışma: 2026-11-15, 230 sn. Gemini'ye yalnız iki spec, Core dosyaları, dört View dosyası, adımın test dosyaları ve testçi raporu verildi; Claude kontrolcünün raporunu görmedi. Çıktı orchestrator tarafından aynen aktarıldı.

### Bulgular

1. **Dosya:** `tests/LevelParserObstacleTests.cs:148-158`, `core/LevelParser.cs:146-149`, `core/LevelData.cs:93-96`
   - **Spec maddesi:** Level dosyası (`LevelParser`) – Denetim sırası (`Anchor` `y = 0` mı, sonra hücre çakışması)
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Spec'e göre engel elemanlarında `Anchor` `y = 0` denetimi (`'obstacles[i].y'`) hücre çakışmasından (`'obstacles[i]'`) önce gelmelidir. `LevelParser.cs:146-149` sırayı doğru uyguluyor ancak `LevelData.cs:93-96` ters sırayla denetliyor ve `LevelParserObstacleTests` içinde `y = 0`'da dolu bir hücreye `Anchor` konulan çakışma testi yok. `LevelParser`'da bu iki denetimin yeri değişse tüm testler geçer.

2. **Dosya:** `tests/LevelParserObstacleTests.cs:156-157`, `core/LevelParser.cs:195-204`
   - **Spec maddesi:** Level dosyası (`LevelParser`) – `goals` elemanlarında denetim sırası
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Spec'e göre hedef elemanlarında önce `color`/`obstacle` (ikisi birden/hiçbiri), sonra seçilen alanın denetimi (engelde: var mı, metin mi, bilinen ad mı, tekrar mı), en son `count` denetlenmelidir. Testlerde tekrar eden engel hedefi (`'goals[1].obstacle'`) ile geçersiz `count` (`'goals[1].count'`) veya `color`+`obstacle` birlikte verilmesi (`'goals[0]'`) ile geçersiz `count` önceliği sınanmamış.

3. **Dosya:** `tests/LevelDataObstacleTests.cs:65-79`, `tests/LevelParserObstacleTests.cs:98-99`, `core/LevelData.cs:88-92`
   - **Spec maddesi:** `LevelData` ve `LevelParser` – Grid dışı koordinatlar ve geçersiz yerleşim
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Yerleşim testlerinde yalnızca üst sınır (`x >= width`, `y >= height`) sınanmış; negatif koordinatlar (`x < 0`, `y < 0`) ve `LevelData` kurucusunda `default(ObstaclePlacement)` (`Layers == 0`) hiç test edilmemiş. `LevelData.CopyPlacements` içinde `x < 0 || y < 0` kontrolü unutulsa `taken[x, y]` erişimi `ArgumentException` yerine `IndexOutOfRangeException` fırlatır.

4. **Dosya:** `tests/GameSessionObstacleTests.cs:153-187`, `core/GameSession.cs:166-175`
   - **Spec maddesi:** Hamle ve hedef (`GameSession`) – Su balonu sayımı
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Su balonu testlerinin tamamında tek patlamada yalnızca 1 balon patlatılıyor ve `OrbOrb` kombosunda balon hiç test edilmiyor. Bir etkide birden fazla balon patladığında hepsinin hedefe sayıldığını doğrulayan test yok.

5. **Dosya:** `tests/ObstacleHitSpecialTests.cs:153-212`, `core/SpecialRules.cs:125, 257, 322`
   - **Spec maddesi:** Vuruş – Kombolar (`OrbRocket`/`OrbBomb`) ve Zincir
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** `OrbRocket`/`OrbBomb`'dan dönüşen öğelerin ortak `hitSet` paylaşması ve tekli `Bomb`'dan zincirlenen roketin ayrı patlama sayılması ayrı ayrı test edilmiş; ancak `OrbRocket`/`OrbBomb` sırasında tahtada önceden duran bir özel öğenin zincirle tetiklenip aynı engele ikinci vuruşu yapması test edilmemiş.

6. **Dosya:** `tests/TurnRulesObstacleTests.cs:170-215`, `core/TurnRules.cs:69`
   - **Spec maddesi:** Düşme ve toplama (`GravityRules.Settle`) – Sandık kırılması ve Çapa toplama
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** Aynı dokunuşta Çapanın altındaki Sandığın kırılmasıyla bölümlerin birleşip Çapanın `y = 0`'a inerek toplanmasını sınayan uçtan uca bir `TurnRules.Tap` testi bulunmuyor.

7. **Dosya:** `view/GridView.cs:378-383, 805-815, 831-840`
   - **Spec maddesi:** Görüntü katmanı – Sandık vuruş ve kırılma animasyonu
   - **Önem:** Orta
   - **Tür:** Görüntü
   - **Açıklama:** Aynı dokunuşta zincirleme iki etki aynı Sandığa vurduğunda (`C3 -> C2 -> C1` veya `C2 -> C1 -> boş`), `_steps[x, y]` tek eleman tuttuğu için ikinci adım ilk `Shake` adımını eziyor; ayrıca `_looks` listesine aynı hücre için iki kayıt eklendiğinden `t >= look2.Start` sonrasında her karede önce eski kat (`ShowCell`), hemen ardından yeni kat uygulanıyor.

8. **Dosya:** `view/GameBootstrap.cs:51, 138-144`, `view/GridView.cs:469-470, 506-513`
   - **Spec maddesi:** Görüntü katmanı – Çapa ve hedefe sayılan engellerin HUD simgesine uçması
   - **Önem:** Düşük
   - **Tür:** Görüntü
   - **Açıklama:** `_camera.rect` üstteki HUD çubuğunu dışarıda bıraktığı için (`1f - HudView.BarFraction`), `_flyers` üzerinde `SpriteMaskInteraction.None` kullanılsa bile dünya kamerasıyla çizilen Çapa HUD bölgesine girince kamera viewport sınırında kesilir. Ayrıca yalnız `Anchor` için HUD'a uçuş var; kırılan `Pinata`, `Crate` ve `WaterBalloon` için yok.

### Ne denedim
- **Kural katmanı (`core/*.cs`):** `Cell`, `ObstacleHit`, `ObstacleGoal`, `ObstaclePlacement`, `TurnEffect`, `TurnResult`, `GravityResult`, `GroupFinder`, `BlastRules`, `SpecialRules`, `GravityRules`, `RefillRules`, `TurnRules`, `GameSession`, `LevelData` ve `LevelParser` sözleşme ve netleştirmelerle satır satır karşılaştırıldı; kural katmanında kod hatası bulunmadı.
- **Kısıtlar:** `core/*.cs` içinde `UnityEngine`, `System.Random` veya C# 9 üstü dil özelliği kullanılmadığı ve dosya başına tek açık tip kuralına uyulduğu doğrulandı.

