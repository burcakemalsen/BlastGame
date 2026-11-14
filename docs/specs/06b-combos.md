# Spec 06b: Kombolar

Durum: onaylandı (2026-11-14). Dal: `step/06b-combos`. Tasarım kararları ürün sahibinin (2026-11-14); mekanikler referansa göre, belirsiz kalanlar aşağıda "orchestrator önerisi" diye işaretli.

## Amaç
Birbirine değen özel öğelerden birine dokununca birleşip tek başlarına yaptıklarından daha güçlü bir etki yaparlar. Mekanikler türün yaygın davranışına göredir; adlar, şekiller ve animasyonlar özgündür (RULES kural 1). Adım 6'daki tekli öğeler, zincir ve sonsuz oyun aynen kalır.

## Bitti tanımı
- Altı kombo çalışıyor, birleşme ve kombo etkilerinin hızlı, cilalı animasyonu var.
- `dotnet test` ve CI yeşil; Editor'de ve iPhone'da (WebGL) elle kontrol listesi geçiyor.

## Kombolar
| Kombo (`EffectKind`) | Etki (merkez: dokunulan hücre `(ox, oy)`) |
|---|---|
| Rocket + Rocket (`RocketCross`) | Satır `oy` ve sütun `ox` temizlenir (artı) |
| Rocket + Bomb (`RocketBomb`) | Satırlar `oy-1..oy+1` ve sütunlar `ox-1..ox+1` temizlenir (her yönde 3 yan yana roket) |
| Bomb + Bomb (`BigBomb`) | Merkezden her yöne 3 kare: 7x7 (ürün sahibi kararı) |
| Orb + Rocket (`OrbRocket`) | Orb'un rengindeki bütün renkli kareler rastgele yönlü rokete döner, sonra hepsi patlar |
| Orb + Bomb (`OrbBomb`) | Orb'un rengindeki bütün renkli kareler bombaya döner, sonra hepsi patlar |
| Orb + Orb (`OrbOrb`) | Bütün tahta temizlenir |

## Kurallar

### Komboya girenler (ürün sahibi kararı)
- Kombo yalnız **dokunuşla** olur. Dokunulan özel öğeden başlayarak, yanlardan (4 komşu; çapraz değil) birbirine bağlı **bütün özel hücreler** komboya girer (katılımcılar). Katılımcı tek ise (dokunulanın yanında özel öğe yoksa) kombo yoktur, Adım 6'daki tekli etki çalışır.
- Bütün katılımcılar tükenir; hiçbiri tekli etkisini yapmaz.
- Uygulanan etki, katılımcıların türlerinden kurulabilen **en güçlü kombodur**. Bu sırayla ilk uyan seçilir (güç sırası, orchestrator önerisi):
  1. 2 ya da daha çok Orb → `OrbOrb`
  2. Orb ve Bomb → `OrbBomb`
  3. Orb ve Rocket → `OrbRocket`
  4. 2 ya da daha çok Bomb → `BigBomb`
  5. Rocket ve Bomb → `RocketBomb`
  6. 2 ya da daha çok Rocket → `RocketCross`
  
  (Yatay ve dikey roket aynı türdür: Rocket.) Orb'lu komboda (`OrbBomb`, `OrbRocket`) tek bir Orb vardır; renk onun rengidir.
- Zincirle tetiklenen öğe komboya girmez, yanındaki öğeleri de birleştirmez: tek başına çalışır (Adım 6'daki gibi).
- Kombo **bir hamle harcar** (`HasBlasted` true).

### Kombo etkisi (`TurnEffect`)
Kombo, `Effects`'in ilk elemanıdır (`TriggeredBy = -1`, `Origin` = dokunulan hücre). `Cleared` önce **bütün katılımcıları** içerir (önce dokunulan, sonra ötekiler `y` artan sonra `x` artan); katılımcı sayısı `MergedCount`'tur. Sonra alan taranır:

- `RocketCross`, `RocketBomb`, `BigBomb`: alanın katılımcı olmayan hücreleri **`y` artan, sonra `x` artan** sırayla. Boş hücre atlanır; renkli hücre temizlenir; özel hücre temizlenmez, zincir kuyruğuna eklenir (Adım 6).
- `OrbOrb`: tahtadaki katılımcı olmayan bütün dolu hücreler (renkli ve özel), `y` artan sonra `x` artan, **temizlenir**. Tahtadaki başka özel öğeler tetiklenmez, yalnız temizlenir (sonuç aynı, tahta zaten boşalıyor; orchestrator önerisi).
- `OrbRocket`, `OrbBomb` (dönüşüm): Orb rengindeki bütün **renkli** kareler (`y` artan, sonra `x` artan) dönüşür. Bu kareler `Converted` listesine girer (konum + eski renkli hücre); grid'de yerlerine yeni özel öğe konur ve **hepsi bu sırayla zincir kuyruğuna** eklenir (`TriggeredBy = 0`). Her biri sonra kendi tekli etkisini yapar (Adım 6: önce kendi hücresi, sonra alanı; alanında bulduğu başka özel öğeler kuyruğa eklenir). Dönüşen kare `Cleared`'da renkli olarak yer almaz (kendi roket/bomba etkisinde özel olarak yer alır); "her hücre en fazla bir `Cleared`'da" kuralı korunur. Renk sayımı için `Converted` de sayılır (aşağıda).
- `OrbRocket` yön çekilişi: `Converted` sırasıyla her kare için bir `random.Next(2)` (`0` yatay, `1` dikey). Bütün çekilişler **grid değişmeden önce** yapılır; değer 0-1 dışıysa `InvalidOperationException`, grid değişmemiştir. Bu çekilişler doldurma çekilişlerinden öncedir. Başka kombo ve tekli öğe dokunuşu çekiliş yapmaz.
- Orb renginde hiç renkli kare yoksa dönüşüm boştur; kombo yalnız katılımcıları temizler.

### Zincir, düşme, doldurma
- Kombonun alanına (ya da dönüşen öğelerin alanlarına) giren özel öğeler Adım 6'daki FIFO kuyrukla çalışır; her öğe bir kez, her hücre tek `Cleared`'da.
- Düşme ve doldurma bütün etkiler bittikten sonra bir kez (Adım 3). Kaynak sırası: önce (varsa) `OrbRocket` yön çekilişleri, sonra doldurma.

### Hamle ve hedef (`GameSession`, oyunda gizli)
- Renk sayımı: bütün etkilerin `Cleared`'ındaki renkli kareler **ve** `Converted`'daki renkli kareler. Özel öğeler sayılmaz.

## Örnekler
`y` alttan üste. `.` renkli herhangi bir kare (rengi örnekte önemsiz), `R` Red, `B` Blue; `H` yatay, `V` dikey roket, `X` Bomb, `O(R)` kırmızı Orb.

**Örnek 1, Rocket + Rocket** (5x5): `(2,2)` `H`, `(3,2)` `V`, gerisi renkli. `Tap(2,2)` → tek etki `RocketCross`, `MergedCount = 2`, `Cleared` = `(2,2) H`, `(3,2) V`, sonra `(2,0)`, `(2,1)`, `(0,2)`, `(1,2)`, `(4,2)`, `(2,3)`, `(2,4)`. Kaynaktan yön çekilişi yok.

**Örnek 2, çapraz kombo değildir:** `(2,2)` `H`, `(3,3)` `X`. `Tap(2,2)` → Adım 6 tekli `Rocket`; `(3,3)` satır 2'de değil, çalışmaz.

**Örnek 3, Rocket + Bomb ve zincir** (5x5): `(2,2)` `X`, `(2,1)` `H`, `(4,4)` `V`, gerisi renkli. `Tap(2,2)` → katılımcılar `(2,2)`, `(2,1)`; `RocketBomb`: satır 1-3 ve sütun 1-3. `Cleared` = `(2,2) X`, `(2,1) H`, sonra `y` sırasıyla alanın renkli hücreleri; `(4,4)` alanda değil. Etki sayısı 1.

**Örnek 4, Bomb + Bomb köşede** (9x9): `(0,0)` `X`, `(1,0)` `X`. `Tap(0,0)` → `BigBomb`, alan `x 0..3`, `y 0..3` (16 hücre, ikisi katılımcı). `Cleared` 16 hücre.

**Örnek 5, Orb + Rocket** (4x3, kaynak `1, 0, 1`, sonra doldurma için `0`'lar):
```
y=2  R B R B
y=1  B O(R) H B
y=0  R B B B
```
`Tap(1,1)` → katılımcılar `(1,1)`, `(2,1)`; `OrbRocket`, renk Red. `Converted` = `(0,0) R`, `(0,2) R`, `(2,2) R`; yönler `V`, `H`, `V`. Etkiler: 0 `OrbRocket` (`Cleared` = `(1,1) O(R)`, `(2,1) H`, `MergedCount = 2`); 1 `Rocket` `(0,0)` (`TriggeredBy 0`): `(0,0) V`, `(0,1) B` (sütun 0'da `(0,2)` özel, kuyrukta zaten var, atlanır); 2 `Rocket` `(0,2)` (`TriggeredBy 0`): `(0,2) H`, `(1,2) B`, `(3,2) B` (`(2,2)` kuyrukta, atlanır); 3 `Rocket` `(2,2)` (`TriggeredBy 0`): `(2,2) V`, `(2,0) B` (`(2,1)` boş). Sayım: Red 3 (`Converted`), Blue 4.

**Örnek 6, üç katılımcı → en güçlü:** `(1,1)` `O(R)`, `(2,1)` `H`, `(2,2)` `X` (Bomb Orb'a değmiyor ama Rocket üzerinden bağlı). `Tap(1,1)` → katılımcı 3; `OrbBomb` (Orb + Bomb, Orb + Rocket'tan güçlü). Kırmızılar bombaya döner, kuyruktaki sırayla patlar.

**Örnek 7, Orb + Orb:** iki Orb yan yana; dokununca bütün tahta temizlenir, başka özel öğeler tetiklenmeden temizlenir; tek etki.

**Örnek 8, zincirle tetiklenen öğe kombo yapmaz:** `(0,0)` `H` tek başına (yanında özel yok), satırında `(3,0)` `X`, `X`'in yanında `(3,1)` `V`. `Tap(0,0)` → `Rocket`, `(3,0)` kuyruğa; `Bomb` tek başına çalışır (3x3 alanında `(3,1)` var → `(3,1)` de kuyruğa, tekli `Rocket`). Kombo yok.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Katılımcı bulma: tek komşu, birden çok komşu, zincir halinde bağlı (dokunulana değmeyen ama bağlı), çapraz komşu girmez, renkli kare bağı koparır.
- Seçim: altı kombonun her biri; karışık setlerde öncelik (Orb+Rocket+Bomb → `OrbBomb`; Rocket+Bomb+Bomb → `BigBomb`; Orb+Orb+Rocket → `OrbOrb`; yatay+dikey roket → `RocketCross`).
- Alanlar: tahta kenarında/köşesinde kırpma; `RocketBomb` kenarda (satır/sütun eksik); `BigBomb` 7x7 tahtadan büyükse kırpma; `Cleared` başında katılımcılar ve `MergedCount`.
- Dönüşüm: renk karesi yokken; dönüşen öğenin alanı başka dönüşmüş öğeyi bulunca tekrar kuyruğa girmez; `Converted` sırası; yön çekilişi sayısı ve sırası; geçersiz çekilişte grid değişmemiş; `OrbBomb`'da çekiliş yok.
- `OrbOrb`: bütün tahta boşalır; başka özel öğeler tetiklenmez (etki sayısı 1).
- Zincir: kombo alanındaki öğe tekli çalışır; zincirle tetiklenen öğe komşularıyla kombo yapmaz.
- `TurnRules.Tap`: tek etkide `Blasted` = `Cleared` birleşimi; düşme ve doldurma bir kez; kaynak sırası.
- `GameSession`: kombo hamle harcar; `Converted` renk sayımına girer.
- Tekrarlanabilirlik: aynı seed, aynı dokunuşlar → aynı sonuç.

## Arayüz sözleşmesi (kural katmanı)
Belirtilmeyen mevcut tipler ve imzalar değişmez. C# 9 sınırı. Dosya başına bir tip.

```csharp
public enum EffectKind { Group, Rocket, Bomb, Orb, RocketCross, RocketBomb, BigBomb, OrbRocket, OrbBomb, OrbOrb } // sona eklenir

public sealed class TurnEffect                                       // eklenenler
{
    public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created,
                      int triggeredBy, int mergedCount, IReadOnlyList<ClearedCell> converted);
        // cleared/converted null → ArgumentNullException (cleared önce); triggeredBy < -1 → ArgumentOutOfRangeException(triggeredBy);
        // mergedCount < 0 → ArgumentOutOfRangeException(mergedCount). Listeler kopyalanmaz.
    // Mevcut 5 parametreli kurucu kalır: MergedCount = (kind == Group ? 0 : 1), Converted boş.
    public int MergedCount { get; }                       // Cleared başındaki katılımcı sayısı; Group'ta 0, tekli öğede 1
    public IReadOnlyList<ClearedCell> Converted { get; }  // yalnız OrbRocket/OrbBomb'da dolu
}

public static class SpecialRules                                     // eklenen
{
    /// (x, y)'deki özel öğeye dokunuş: katılımcılar 2+ ise kombo, değilse tekli etki; sonra zincir.
    /// Düşme ve doldurma yapmaz. Hatalar, bu sırayla: grid null → ArgumentNullException; random null → ArgumentNullException;
    /// grid dışı → ArgumentOutOfRangeException (x sonra y); hücre özel değil → ArgumentException; geçersiz yön çekilişi → InvalidOperationException.
    public static IReadOnlyList<TurnEffect> Tap(Grid grid, int x, int y, IRandomSource random);
    // Mevcut Activate(grid, x, y) değişmez: kombosuz tekli etki + zincir (Adım 6).
}
```
`TurnRules.Tap` özel öğeye dokunuşta `SpecialRules.Tap`'i kullanır. İmzalar değişmez.

**Netleştirmeler (stub aşamasında, orchestrator, 2026-11-14):**
- 7 parametreli `TurnEffect` kurucusu yalnız yazılan denetimleri yapar (`kind` ile `mergedCount`/`converted` uyumu denetlenmez).
- `SpecialRules.Tap`'te boş hücre de "özel değil" sayılır → `ArgumentException`.
- Katılımcı tek ise `SpecialRules.Tap`'in sonucu `Activate` ile birebir aynıdır (çekiliş yok).
- Katılımcı bağı yalnız özel hücreler arasındadır; arada boş ya da renkli hücre bağı koparır.

**Sözleşme değişikliği notu:** Adım 6'da yan yana iki özel öğeden birine dokunmak tekli etkiydi; artık kombodur. `TurnRules`/`GameSession` düzeyinde yan yana özel öğe içeren mevcut testler (ör. Adım 6 Örnek 3: Bomb yanında Rocket) yeni davranışa göre güncellenir; testçi yapar (RULES, Onay). `SpecialRules.Activate` testleri değişmez.

## Görüntü katmanı (`Assets/Scripts/View/`)
Otomatik test edilmez; kapısı elle kontrol listesi. Adım 6'daki sonsuz oyun ve çizimler kalır. Süreler hedeftir; etkiler hızlı ve vurucu.
- **Birleşme:** katılımcılar dokunulan hücreye doğru kayıp üst üste biner (~0.15 sn), kısa parlama, sonra kombo etkisi.
- **`RocketCross`:** iki roket çifti aynı anda dört yöne.
- **`RocketBomb`:** her yönde 3 yan yana roket (geniş bant).
- **`BigBomb`:** büyük halka dalgası (7x7), tek bombadan belirgin sarsıntı.
- **`OrbRocket` / `OrbBomb`:** Orb'dan hedef karelere ışınlar sırayla (çok kısa aralıklarla), değdiği kare rokete/bombaya dönüşür; dönüşümler bitince hepsi neredeyse aynı anda patlar (zincirdeki gibi üst üste). Toplam bir iki saniyeyi geçmez.
- **`OrbOrb`:** büyük parlama, tahta merkezden dışa dalga halinde temizlenir (~0.4 sn).
- Zamanlama `TriggeredBy` ve konumlardan (Adım 6); dönüşen öğeler dönüşümleri bitince başlar. Geçici nesneler havuzdan; sızıntı yok.

## Kapsam dışı
Engeller (Adım 7), ses, çıkmaz durum (Adım 8), hamle/hedef (gizli kalır).

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0.
2. Editor'de (ve WebGL'de) her kombo bir kez: iki roket artı, roket+bomba üçlü bant, iki bomba büyük patlama, Orb+roket kırmızıları rokete çevirip patlatır, Orb+bomba bombaya çevirir, iki Orb tahtayı temizler.
3. Üç öğe yan yana: en güçlü kombo uygulanır. Çapraz duran öğe birleşmez.
4. Zincir: kombonun alanındaki öğe tekli patlar; düşme hepsinden sonra.
5. Animasyonlar hızlı ve okunaklı; Console temiz; "Next board" sonrası sızıntı yok.
6. WebGL `gh-pages`'e; ürün sahibi iPhone'da 2-5'i dener.

## Akış (zor adım)
1. Spec onayı → `docs(spec): add combos spec`.
2. Kodcu (Opus 5.5): stub → `feat(core): add combo stubs`.
3. Testçi (Opus 5.5 medium): yeni testler (~150) + sözleşme gereği güncellemeler → `test(core): add combo tests`.
4. Kodcu: uygulama + animasyon → `feat(core): implement combos`, `feat(view): animate combos`.
5. Claude (Opus medium, bilerek bozma) + Gemini (`gemini-4-argon-medium`) paralel; bulgular kapanır.
6. WebGL önizleme, ürün sahibi dener.
7. Tarama, push, PR, CI, merge.
