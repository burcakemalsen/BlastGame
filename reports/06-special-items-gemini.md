# Adım 6, özel öğeler: Gemini kontrolcü raporu

Model: Gemini 4 Argon Medium (`agy -p --model gemini-4-argon-medium`, salt okunur). Çalışma: 2026-11-14, 379 sn. Gemini'ye yalnız spec, adımın Core/View dosyaları, adımın test dosyaları ve testçi raporu verildi; Claude kontrolcünün raporunu görmedi. Çıktı orchestrator tarafından aynen aktarıldı.

### Bulgular

1. **`tests/SpecialRulesActivateTests.cs:63-117, 199-257`**
   - **Spec maddesi:** Etkiler ve alanları (Rocket tarama sırası) / Zincir (FIFO kuyruğu)
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Rocket'ın uzaklık (`d = 1, 2, ...`) ve yön önceliği (yatayda sol-sağ, dikeyde alt-üst) yalnız renkli karelerin `Cleared` sırası için test edilmiş. Birden çok özel öğeyi tetikleyen zincir testlerinin (`Activate_ChainOfThreeLevels...`, `Activate_ItemInTwoAreas...`) tamamında tetikleyici olarak Bomb kullanılmış. Bir Rocket'ın iki özel öğeyi (farklı uzaklıkta ya da eşit uzaklıkta iki yönde) bulup doğru sırayla zincir kuyruğuna eklediği test edilmemiş.

2. **`tests/SpecialRulesActivateTests.cs:174-217`**
   - **Spec maddesi:** Zincir ("Başka öğeyle tetiklenen Orb kendi rengini temizler. Etki alanında temizleyecek renkli kare olmasa da öğe kendi hücresini temizler", "Her hücre en fazla bir etkinin `Cleared` listesinde yer alır")
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Zincirde yalnız Rocket → Orb (`Activate_Example5...`) test edilmiş. İki uç durum eksik: (a) Zincirde Orb'un kendisinden sonra çalışacak bir Rocket/Bomb'dan önce çalışıp onun yolundaki renkli kareleri önceden temizlemesi (sonraki etkinin bu boşalan kareleri atlaması), (b) Tetikleyen etkinin Orb rengindeki tüm kareleri zaten temizlemiş olması durumunda zincirle tetiklenen Orb'un (`TriggeredBy >= 0`) yalnız kendi hücresini temizlemesi.

3. **`tests/GameSessionSpecialTests.cs:88-96`**
   - **Spec maddesi:** Hamle ve hedef (`GameSession`) / Uç durumlar
   - **Önem:** Orta
   - **Tür:** Eksik test
   - **Açıklama:** Son hamlede özel öğe/zincir ile kazanma (`GameStatus.Won`) test edilmiş ancak son hamlede özel öğe veya birleşme sonrasında hedefler tamamlanmadığında oyunun `GameStatus.Lost` durumuna geçtiği test edilmemiş. Ayrıca `GameSession.Tap` üzerinden 5-6'lık birleşmede `random.Next(2)` geçersiz değer döndürdüğünde hem grid'in hem oturum sayaçlarının (`MovesLeft`, `Remaining`, `Status`) korunduğu doğrulanmamış.

4. **`tests/TurnEffectTests.cs:30-45` ve `Core/TurnEffect.cs:14-17`**
   - **Spec maddesi:** Arayüz sözleşmesi (`TurnEffect` kurucusu)
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** `TurnEffect` kurucusuna aynı anda `cleared: null` ve `triggeredBy: -2` verildiğinde önce `ArgumentNullException("cleared")` fırlatıldığı test edilmemiş (`TurnResult` ve `ForGroup` için hata öncelik sırası testleri mevcut, `TurnEffect` için eksik).

5. **`tests/CellSpecialTests.cs:118-159` ve `Core/Cell.cs:114-125`**
   - **Spec maddesi:** Arayüz sözleşmesi ve Uç durumlar (`Cell` eşitlik ve `GetHashCode`)
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** `Equals_OrbDifferentColors_NotEqual` yalnız `==` ve `Equals(Cell)` denetliyor; farklı renkteki iki Orb için `!=`, `Equals(object)` ve `GetHashCode` değerlerinin farklılığı test edilmemiş (`GetHashCode` içindeki `IsOrb` renk katkısı silinse testler yakalamaz).

6. **`tests/TurnRulesSpecialTests.cs:173-185`**
   - **Spec maddesi:** Kurallar — Hücre ("Özel hücre düşer")
   - **Önem:** Düşük
   - **Tür:** Eksik test
   - **Açıklama:** Birleşmeyle yeni doğan özel öğenin düşmesi test edilmiş, ancak özel öğe aktivasyonu sonrasında etki alanı dışında kalan başka bir özel öğenin tetiklenmeden yerçekimiyle (`GravityRules`) düştüğü bir `TurnRules.Tap` senaryosu bulunmuyor.

### Sorun bulunmayan alanlar

- **Kurallar ve Arayüz Sözleşmesi (`Core/`):** `CellKind`, `SpecialKind`, `Cell`, `ClearedCell`, `TurnEffect`, `TurnResult`, `SpecialRules`, `TurnRules`, `GroupFinder`, `BlastRules` ve `GameSession` spec kurallarıyla, örneklerle ve netleştirmelerle birebir uyumlu. Dokunuşun 5 durumu, kaynak çekiliş sırası (önce tek `Next(2)` yön çekilişi, sonra `y` ve `x` artan doldurma), Rocket/Bomb/Orb tarama sıraları, FIFO zincir kuyruğu, `TriggeredBy` indeksleri, her hücrenin tek `Cleared` listesinde yer alması ve Orb'un özel hücreleri hedeflememesi doğru uygulanmış.
- **Kural Katmanı Kısıtları (`Core/`):** `Core/` altındaki hiçbir dosyada `UnityEngine`, `System.Random` veya C# 9 üstü dil özelliği (`record`, `init` vb.) kullanılmamış; dosya başına tek açık tip kuralına uyulmuş.
- **Görüntü Katmanı (`View/`):** `GameBootstrap.EndlessMode` ile hamle/hedef/sonuç paneli kodları silinmeden gizlenmiş ve dokunuşlar doğrudan `TurnRules.Tap` ile işlenmiş. "Next board" butonu ve tahta dokunuşları `_view.IsAnimating` iken kapalı. `GridView` zincir zamanlamasını `TriggeredBy` ve `ReachTime` ile doğru hesaplıyor, düşme ve doldurma tüm etkiler bittikten sonra çalışıyor. Geçici efekt nesneleri `_pool` üzerinden yeniden kullanılıyor, dokular `ShapeSprites` içinde bir kez üretiliyor ve yeni tahtaya geçişte eski `GridView` ile hedef satırları temizlenerek sızıntı önleniyor.

