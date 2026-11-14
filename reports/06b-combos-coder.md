# Adım 6b kodcu raporu

## Aşama A

### Dosyalar
- `Assets/Scripts/Core/EffectKind.cs`: sona `RocketCross`, `RocketBomb`, `BigBomb`, `OrbRocket`, `OrbBomb`, `OrbOrb`.
- `Assets/Scripts/Core/TurnEffect.cs`: 7 parametreli kurucu (gövde `NotImplementedException`), `MergedCount`, `Converted`. Mevcut 5 parametreli kurucu çalışır; `MergedCount` = `Group` 0, diğerleri 1; `Converted` boş dizi. Özellikler alan okur; 7 parametreli kurucu fırlattığı için o yoldan değer gelmez.
- `Assets/Scripts/Core/SpecialRules.cs`: `Tap(Grid, int, int, IRandomSource)` (gövde `NotImplementedException`). `Activate`, `TurnRules`, `GameSession` değişmedi. Görüntü katmanına dokunulmadı. Yeni dosya yok.

### Kontroller
- `dotnet build tests/BlastGame.Tests.slnx`: `Oluşturma başarılı oldu. 0 Uyarı 0 Hata`
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı! - Başarısız: 0, Başarılı: 927, Atlanan: 0, Toplam: 927`
- `scripts/unity-check.ps1`: `Unity compile check: clean.`

### Spec'te belirsiz bulduklarım
1. 7 parametreli kurucu, `kind` ile `mergedCount`/`converted` tutarlılığını denetler mi? (ör. `Group` + `mergedCount 2`, `Rocket` + dolu `converted`, `mergedCount > cleared.Count`). Spec yalnız null ve negatif denetimi sayıyor; ben başka denetim eklemeyeceğim.
2. 5 parametreli kurucu kombo türüyle çağrılırsa `MergedCount` 1 olur (spec kuralı gereği). Kural katmanı kombo için her zaman 7 parametreliyi kullanacak.
3. `Tap` "hücre özel değil → `ArgumentException`": boş hücre de bu kapsamda sayılacak (Adım 6 `Activate` ile aynı).
4. `Tap` katılımcı tek olduğunda tekli etki + zincir: `Activate` ile birebir aynı sonuç (Orb dahil, çekiliş yok) varsayıyorum.
5. Katılımcı bulmada bağlantı yalnız özel hücreler üzerinden; arada boş hücre bağı koparır (renkli gibi) varsayıyorum.

## Aşama B

### Değişen dosyalar
- `Assets/Scripts/Core/TurnEffect.cs`: 7 parametreli kurucu uygulandı (sıra: `cleared`, `converted` null; `triggeredBy`; `mergedCount`). 5 parametreli kurucu ona zincirlenir.
- `Assets/Scripts/Core/SpecialRules.cs`: `Tap` (katılımcı bulma, güç sırası, alan taramaları, `OrbOrb`, dönüşüm, yön çekilişleri grid değişmeden önce). Zincir döngüsü `Activate` ile ortak özel metoda (`RunChain`) taşındı; `Activate` davranışı aynı. `ForGroup`'un roket çekilişi aynı mesajla ortak yardımcıya alındı.
- `Assets/Scripts/Core/TurnRules.cs`: özel dokunuşta `SpecialRules.Tap`.
- `Assets/Scripts/Core/GameSession.cs`: renk sayımına `Converted` eklendi.
- `Assets/Scripts/View/GridView.cs`: kombo animasyonları. Yeni dosya yok; sahne dosyasına dokunulmadı.

### Kontroller
- `dotnet test`: `Başarılı! - Başarısız: 0, Başarılı: 1022, Atlanan: 0, Toplam: 1022`
- `unity-check`: çıkış 0, `Unity compile check: clean.`
- Yanlış görünen test çıkmadı; testlere dokunulmadı.

### Animasyon (spec'e göre)
- Birleşme: katılımcılar dokunulan hücreye kayar (0.15 sn); merkez şişip kaybolur, kısa parlama.
- `RocketCross`: dört yöne iki roket çifti. `RocketBomb`: bandın her satırı ve sütunu için birer çift (en fazla 6 çift), tahta dışındaki satır/sütun atlanır.
- `BigBomb`: büyük halka (7x7 ölçüsünde), halka halka yayılan temizlik, güçlü sarsıntı.
- `OrbRocket`/`OrbBomb`: dokunulan hücre Orb görünümüne geçip nabız atar; ışınlar sırayla gider, değdiği kare yeni öğeye döner; dönüşen öğeler, dönüşümler bitince 0.02 sn arayla patlar.
- `OrbOrb`: büyük parlama, tahta merkezinden dışa dalga (~0.4 sn).
- Zamanlama `TriggeredBy` üzerinden; kombo alanındaki öğenin başlama zamanı, kombonun o hücreye ulaştığı an. Geçici nesneler mevcut havuzdan geliyor; yeni liste (`_looks`) her tur temizleniyor.

### Sapmalar ve doğrulanmayanlar
- Spec'te "tahta merkezden dışa dalga": tahtanın geometrik merkezini kullandım, dokunulan hücreyi değil.
- Editor'de ve WebGL'de elle kontrol listesi yapılmadı; animasyonlar yalnız derleme düzeyinde doğrulandı. Süreler gözle ayarlanmadı.

### Öneriler
- Elle kontrol için her komboyu kuran bir deneme tahtası (level dosyası) işe yarar; spec dışı olduğu için eklemedim.

## Kontrol düzeltmesi

- `Assets/Scripts/View/GridView.cs` `PlanConvert`: dokunulan hücrenin Orb görünümüne geçişi ve `OrbSelf` nabzı artık birleşme bitince (`fire = start + MergeDuration`) başlıyor; dokunulan öğe birleşme boyunca kendi görünümünde kalıyor, iki Orb görünmüyor. Işın zamanlaması değişmedi (nabız bitince başlıyor).
- `dotnet test`: `Başarılı! - Başarısız: 0, Başarılı: 1022, Atlanan: 0, Toplam: 1022`
- `unity-check`: çıkış 0, `Unity compile check: clean.`
- Editor/WebGL elle kontrolü yapılmadı.
