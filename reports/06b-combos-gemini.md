# Adım 6b, kombolar: Gemini kontrolcü raporu

Model: Gemini 4 Argon Medium (`agy -p --model gemini-4-argon-medium`, salt okunur). Çalışma: 2026-11-14, 274 sn. Gemini'ye yalnız iki spec, Core dosyaları, `GridView.cs`, adımın test dosyaları ve testçi raporu verildi; Claude kontrolcünün raporunu görmedi. Çıktı orchestrator tarafından aynen aktarıldı.

### Bulgular

1. **`View/GridView.cs:516, 530`**
   - **Spec maddesi:** Görüntü katmanı — Birleşme ve `OrbRocket` / `OrbBomb` ("katılımcılar dokunulan hücreye doğru kayıp üst üste biner (~0.15 sn), kısa parlama, sonra kombo etkisi", "Orb'dan hedef karelere ışınlar...").
   - **Önem:** Orta
   - **Tür:** Kod hatası
   - **Açıklama:** `PlanConvert` içinde dokunulan hücrenin (`o`) Orb görünümüne geçişi (`_looks.Add(..., Start = start)`) ve `TileAnim.OrbSelf` adımı (`SetStep(o, TileAnim.OrbSelf, start, end - start)`) birleşme bitişi olan `fire` (`start + MergeDuration`) yerine `start` (`0`) anında başlatılıyor. Dokunulan hücre Rocket veya Bomb ise, komşu Orb henüz üzerine kaymadan `t = 0` anında aniden Orb'a dönüşüyor (birleşme sırasında ekranda iki Orb görünüyor) ve Orb nabız animasyonu (`OrbPulse`) ışınlardan hemen önce (`fire..fire + OrbPulse`) değil, birleşme kayması sırasında (`start..start + OrbPulse`) oynuyor.

2. **`tests/ComboSelectionTests.cs:13-32`**
   - **Spec maddesi:** Kurallar — Komboya girenler (güç sırası: `OrbOrb > OrbBomb > OrbRocket > BigBomb > RocketBomb > RocketCross`) ve Uç durumlar ("karışık setlerde öncelik").
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Karışık katılımcı kümelerinde `1 Orb + 2 Bomb` (`OrbBomb` > `BigBomb`) ve `2 Orb + 1 Bomb` (`OrbOrb` > `OrbBomb`) kombinasyonları test edilmemiş. Örneğin `SpecialRules.PickCombo` içindeki `if (orbs == 1 && bombs > 0)` koşulu `bombs == 1` olarak bozulsa veya `BigBomb` (`bombs >= 2`) kontrolü `OrbBomb`/`OrbRocket` üstüne taşınsa mevcut testlerin tamamı geçmeye devam eder.

3. **`tests/ComboChainTests.cs:16-54` ve `tests/ComboAreaTests.cs:57-87`**
   - **Spec maddesi:** Kombo etkisi (`RocketBomb`) ve Zincir ("özel hücre temizlenmez, zincir kuyruğuna eklenir", "kombo alanındaki öğe tekli çalışır").
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** `RocketCross` ve `BigBomb` alan içindeki özel öğeleri zincir kuyruğuna ekleme davranışı için test edilmiş, ancak `RocketBomb`'un 3 satır / 3 sütunluk bandı içinde (özellikle merkez satır/sütun dışındaki `oy ± 1` veya `ox ± 1` şeritlerinde) yer alan özel bir öğeyi zincirle tetiklediği hiçbir test yok.

4. **`tests/TurnEffectComboTests.cs:42-86`**
   - **Spec maddesi:** Arayüz sözleşmesi — `TurnEffect` 7 parametreli kurucu hata denetimleri.
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** Testçi raporunda da ("Belirsizlikler" #3) belirtildiği üzere, 7 parametreli kurucuda `cleared` ile `converted` arasındaki null önceliği test edilmiş; ancak `converted == null` ile `triggeredBy < -1` veya `triggeredBy < -1` ile `mergedCount < 0` aynı anda geçersiz verildiğinde hangi hatanın önce fırlatıldığı test edilmemiş.

5. **`tests/GameSessionComboTests.cs:49-96`**
   - **Spec maddesi:** Hamle ve hedef (`GameSession`) — `Converted` renk sayımı ve son hamle davranışı.
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** `Converted` ile kazanma ve hedef düşürme test edilmiş; ancak `Converted` karelerinin kalan hedef sayısını aşması durumunda hedefin `0`'da sabitlenmesi (negatife düşmemesi) ve son hamlede (`MovesLeft == 1`) yapılan bir kombo hedefleri tamamlayamadığında durumun `GameStatus.Lost` olması kombo senaryolarında test edilmemiş.

---

### Sorun bulunmayan alanlar
- **Katılımcı bulma ve sıralama (`Core/SpecialRules.cs`):** 4 yönlü komşulukla bağlı tüm özel hücrelerin bulunması, çapraz/boş/renkli hücrelerin bağı koparması, tek katılımcıda çekilişsiz `Activate` davranışı, `Cleared` başında önce dokunulan sonra `y`/`x` artan katılımcı sırası ve `MergedCount` tam uyumlu.
- **Alan taraması, `OrbOrb` ve dönüşüm (`Core/SpecialRules.cs`):** `RocketCross`, `RocketBomb`, `BigBomb` tarama sıraları ve sınır kırpmaları; `OrbOrb`'un diğer özel öğeleri tetiklemeden tüm tahtayı temizlemesi; `OrbRocket`/`OrbBomb`'da yalnız Orb rengindeki renkli karelerin `Converted`'a yazılıp `TriggeredBy = 0` ile kuyruğa girmesi ve `OrbRocket` yön çekilişlerinin grid değişmeden ve doldurmadan önce yapılması sözleşmeye tam uygun.
- **Kural katmanı kısıtları (`Core/`):** `UnityEngine`, `System.Random` veya C# 9 üstü dil özelliği yok; dosya başına tek tip kuralı ve tüm imza/hata sözleşmeleri korunmuş.

