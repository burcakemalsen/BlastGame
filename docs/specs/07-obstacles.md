# Spec 07: Engeller

Durum: onaylandı (2026-11-14). Dal: `step/07-obstacles`. Tasarım kararları ürün sahibinin (2026-11-14). Mekanikler türün yaygın davranışına ve referansa göre; referansta bulunamayan yerlerde ürün sahibi karar verdi. Leveller Adım 7b'de.

## Amaç
Tahtaya dört özgün engel gelir. Engeller level dosyasından yerleşir, kendi kurallarına göre kırılır ya da toplanır ve level hedefi olabilir. Adlar ve görünüşler özgündür (RULES kural 1). Level modu (hamle, hedef, kazan/kaybet) geri gelir. Adım 6 ve 6b'deki özel öğeler ve kombolar aynen kalır.

## Bitti tanımı
- Dört engel kurallarıyla çalışıyor, level dosyasından geliyor, hedef olabiliyor. Vuruş, kırılma ve toplanma animasyonları var.
- Görseller onaylı (aşağıda "Görseller").
- Her engel için bir deneme level'ı var (Adım 7b'de yeniden tasarlanır).
- `dotnet test` ve CI yeşil. Editor'de ve iPhone'da (WebGL) elle kontrol listesi geçiyor.

## Engeller
| Engel (`ObstacleKind`) | Konum | Kat | Düşer mi | Ne vurur | Hedef sayımı |
|---|---|---|---|---|---|
| Pinyata (`Pinata`) | Hücreyi kaplar | 1 | Evet, kare gibi düşer | Komşu grup patlaması, özel öğe ve kombo alanı | Kırılınca 1 |
| Sandık (`Crate`) | Hücreyi kaplar | 1-3 | Hayır, sabit | Komşu grup patlaması, özel öğe ve kombo alanı; her vuruş 1 kat | Son katı kırılınca 1 |
| Su balonu (`WaterBalloon`) | Renkli karenin üstünde, hücreyi kareyle paylaşır | 1 | Altındaki kareyle birlikte | Altındaki kare temizlenince (grupla, özel öğe ya da kombo alanında, Orb dönüşümünde) | Patlayınca 1 |
| Çapa (`Anchor`) | Hücreyi kaplar | Kırılmaz | Evet, kare gibi düşer | Hiçbir şey vurmaz | Dibe (`y = 0`) inip toplanınca 1 |

## Kurallar

### Hücre
- Pinyata, Sandık ve Çapa engel hücresidir (`CellKind.Obstacle`). Dokunulmaz: dokunuş hiçbir şey yapmaz (`HasBlasted` false, grid değişmez, hamle harcanmaz). Grup bağını koparır: grup yalnız aynı renkli renkli hücrelerden oluşur.
- Su balonu bir engel hücresi değildir, renkli hücrenin bayrağıdır (`HasBalloon`). Balonlu kare her bakımdan kendi rengindeki renkli kare gibi davranır: gruba girer, dokunulabilir, düşer. Grup bulmada balonlu ve balonsuz aynı renkli kareler birbirine bağlıdır. Balon yalnız renkli hücrede olur.
- Doldurma yalnız renkli kare üretir, balon ya da engel üretmez.

### Vuruş (Pinyata ve Sandık)
- **Grup patlaması:** patlayan grubun hücrelerinin yan komşusu (4 komşu, çapraz değil) olan her Pinyata ve Sandık 1 vuruş alır. **Bir patlama aynı engele yalnız 1 vuruş sayar**, kaç komşu kare olursa olsun (ürün sahibi kararı).
- **Tekli özel öğe:** Rocket ve Bomb alanındaki her Pinyata ve Sandık 1 vuruş alır. Roket engelde durmaz, satırın ya da sütunun sonuna kadar gider; geçtiği her engelden 1 kat götürür (ürün sahibi kararı; 3 katlı Sandık 2 kata düşer). Orb renkli kareleri temizler; temizlenen karelerin yan komşusu olan her Pinyata ve Sandık 1 vuruş alır (referansa göre).
- **Kombolar:** bir kombo her engele 1 vuruş sayar (ürün sahibi kararı). `RocketCross`, `RocketBomb`, `BigBomb`: alandaki her Pinyata ve Sandık 1 vuruş. `OrbOrb`: tahtadaki her Pinyata ve Sandık 1 vuruş. `OrbRocket` ve `OrbBomb`: kombo ve ondan dönüşen öğeler **tek patlama** sayılır; dönüşen roket ve bombaların alanlarının birleşimindeki her engel en fazla 1 vuruş alır (ürün sahibi kararı). Vuruş, alanına o engeli ilk alan etkinin `Hits` listesine yazılır.
- **Zincir:** alana girip tetiklenen ve tahtada önceden duran her özel öğe ayrı bir patlamadır. Aynı engel bir dokunuşta farklı etkilerden birden çok vuruş alabilir. Etki sırasına göre kırılmış engelin yeri boştur ve sonraki etki onu atlar.
- Vuruş: Pinyata yok olur (hücre boş). Sandık bir kat kaybeder; kat 0'a inince yok olur (hücre boş). Her vuruş `ObstacleHit` olarak kaydedilir (önceki ve sonraki hücre).
- Çapa hiçbir şeyden etkilenmez: alanda kalsa da yerinde durur ve `Cleared` ya da `Hits`'e girmez.

### Su balonu
- Balonlu kare temizlendiğinde balon da patlar. `Cleared`'daki `ClearedCell.Cell`'in `HasBalloon`'u true olur. Balon için ayrı vuruş kaydı yoktur.
- Komşu patlama balonu vurmaz.
- `OrbRocket` ve `OrbBomb` balonlu kareyi dönüştürünce balon patlar: `Converted`'daki eski hücrenin `HasBalloon`'u true, yeni özel öğe balonsuzdur.
- Grup özel öğe doğurursa, dokunulan hücre önce temizlenir (balonu varsa patlar), sonra özel öğe oraya konur (Adım 6).

### Düşme ve toplama (`GravityRules.Settle`)
- Sandık sabittir. Her sütun Sandıklarla bölümlere ayrılır. Her bölümde düşebilen hücreler (renkli, özel, Pinyata, Çapa) sıralarını koruyarak bölümün dibine iner (en alt bölümde `y = 0`'a, diğerlerinde altındaki Sandığın hemen üstüne). Sandığın üstündeki kareler sandığın üstünde durur. Düşme sandığın altına geçmez.
- **Çapa toplama:** her sütunun en alt bölümünde, aşağıdan yukarı düşme sırasıyla en baştaki ardışık Çapalar (yani dibe inecek olan Çapa ve hemen üstüne gelecek Çapalar) toplanır. Grid'den kalkarlar; üstlerindeki hücreler onların yerine `y = 0`'dan başlayarak iner. Her toplanan Çapa `Collected`'a `CellMove(from, (x, 0))` olarak girer (`from == to` olabilir). Sonuç tek seferlik bir yerleşmedir ve toplamadan sonra yeni bir Çapa dibe gelmez, çünkü ardışık Çapaların hepsi aynı adımda toplanır.
- `Moves` yalnız yeri değişen ve toplanmayan hücreleri içerir (sütun `x` artan, sonra yeni `y` artan; Adım 3 sırası).
- Mevcut `GravityRules.Collapse` Sandık bölümlerine uyar ama Çapa toplamaz. `TurnRules.Tap` `Settle` kullanır.

### Doldurma (`RefillRules.Fill`)
- Her sütunda yalnız **en üstteki Sandığın üstündeki** boş hücreler doldurulur (Sandık yoksa bütün boşluklar, Adım 3'teki gibi). Sandığın altında kalan boşluk, Sandık kırılana kadar boş kalır (ürün sahibi kararı). Çekiliş sırası Adım 3'teki gibidir; doldurulmayan hücre için çekiliş yapılmaz.

### Dokunuş sırası (`TurnRules.Tap`)
1. Dokunulan hücre engel ya da boşsa: hiçbir şey olmaz.
2. Renkli hücrede grup ya da özel öğede kombo/tekli etki ve zincir (Adım 2, 6, 6b). Vuruşlar her etkinin `Hits`'ine yazılır.
3. Bütün etkiler bittikten sonra bir kez `Settle` (düşme ve Çapa toplama), sonra `Fill`.
4. Kaynak sırası değişmez: önce (varsa) `OrbRocket` yön çekilişleri, sonra doldurma.

### Hamle ve hedef (`GameSession`)
- Hedef türleri: renk (Adım 4) ve engel (`ObstacleGoal`). Bir level'da ikisi birlikte olabilir. En az bir hedef gerekir (renk ya da engel).
- Engel sayımı: `Pinata` ve `Crate` için `Hits`'te sonraki hücresi boş olan vuruşlar; `WaterBalloon` için `Cleared` ve `Converted`'daki `HasBalloon` hücreler; `Anchor` için `Collected`. Kalan sayı 0'ın altına inmez.
- Renk sayımı değişmez (balonlu kare kendi rengi olarak sayılır).
- Kazanma: bütün renk ve engel hedefleri 0. Kaybetme ve diğer kurallar Adım 4'teki gibi.

### Level dosyası (`LevelParser`)
Kök alanına isteğe bağlı `obstacles` eklenir. `goals` elemanı renk ya da engel hedefi olabilir.
```json
{
  "width": 9, "height": 9, "colors": 3, "moves": 30,
  "obstacles": [
    { "type": "Crate", "x": 0, "y": 4, "layers": 2 },
    { "type": "Pinata", "x": 4, "y": 4 },
    { "type": "WaterBalloon", "x": 2, "y": 2 },
    { "type": "Anchor", "x": 6, "y": 8 }
  ],
  "goals": [
    { "obstacle": "Crate", "count": 1 },
    { "color": "Red", "count": 10 }
  ]
}
```
- `obstacles`: yoksa engel yok. Varsa dizi (boş olabilir). Her eleman nesnedir. Alanları `type` (zorunlu, `ObstacleKind` adı, büyük/küçük harf duyarlı), `x`, `y` (zorunlu, tam sayı, grid içinde) ve `layers`. `layers` yalnız `Crate`'te olur, isteğe bağlıdır, varsayılan 1, aralık 1-3. Başka türde `layers` alanı geçersizdir.
- Bir hücreye en fazla bir yerleşim olabilir (Su balonu dahil). `Anchor` `y = 0`'da olamaz.
- `goals` elemanında `color` ya da `obstacle` alanından tam olarak biri olur, ikisi birden ya da hiçbiri olamaz. `obstacle`: `ObstacleKind` adı, tekrar edemez. `count` en az 1 olmalı ve o türün dosyadaki yerleşim sayısını aşamaz (her `Crate` 1 sayılır).
- Yeni hatalar `LevelFormatException`. Mesajda alan yolu tek tırnak içinde geçer: `'obstacles[1].x'`, `'obstacles[0].layers'`, `'goals[0].obstacle'`, `'goals[0].count'`. Hücre çakışmasında ikinci yerleşimin yolu (`'obstacles[3]'`) geçer.
- **Denetim sırası:** Adım 5 sırası korunur. `obstacles`, kök alan denetiminde `moves`'tan sonra, `goals`'tan önce gelir: var mı (isteğe bağlı), dizi mi, sonra elemanlar sırayla. Her elemanda sıra şöyle: nesne mi, bilinmeyen alan, `type` (var mı, metin mi, bilinen ad mı), `x`, `y` (var mı, tam sayı mı, aralık), `layers` (tür izin veriyor mu, tam sayı mı, aralık), `Anchor` `y = 0` mı, hücre çakışması. `goals` elemanlarında: nesne mi, bilinmeyen alan, `color`/`obstacle` (ikisi birden ya da hiçbiri), sonra seçilen alanın denetimi (renkte Adım 5 sırası; engelde var mı, metin mi, bilinen ad mı, tekrar mı), `count` (var mı, tam sayı mı, en az 1 mi, yerleşim sayısını aşıyor mu).
- `LevelData.CreateSession(random)`: önce `GridFactory.CreateRandom` (çekiliş sırası değişmez), sonra yerleşimler dosya sırasıyla konur. Pinyata, Sandık ve Çapa hücreyi değiştirir; Su balonu oradaki renkli kareye balon ekler. Aynı seed aynı tahtayı verir.

## Örnekler
`y` alttan üste. `R` Red, `B` Blue, `G` Green, `r` balonlu Red, `P` Pinyata, `C2` 2 katlı Sandık, `A` Çapa, `.` boş.

**Örnek 1, komşu vuruş tek sayılır** (4x2):
```
y=1  R  P  B  B
y=0  R  R  B  G
```
`Tap(0,0)` → grup `(0,0)`, `(1,0)`, `(0,1)`. `P` `(1,1)` hem `(1,0)`'ın hem `(0,1)`'in komşusu. Sonuç tek vuruş, Pinyata kırılır. `Hits` = `(1,1)`: `P` → boş.

**Örnek 2, Sandık katı ve düşme** (3x3, `C2` `(1,1)`'de):
```
y=2  B  R  B
y=1  R  C2 B
y=0  R  G  B
```
`Tap(0,0)` → grup `(0,0)`, `(0,1)`. `C2` `(1,1)` komşudur, `C1` olur. `(1,2)` `R` Sandığın üstünde kalır. Sütun 0'da `(0,2)` `B` `y = 0`'a iner. Doldurma sütun 0'ın üst iki hücresini doldurur.

**Örnek 3, Sandık altında boşluk** (1x3 sütun; aşağıdan yukarı `R`, `C1`, `G`): alttaki `R` bir Rocket alanında temizlenir. `C1` aynı roket alanındaysa kırılır, `G` dibe iner ve üst iki hücre doldurulur. `C1` alanda değilse `(0,0)` boş kalır ve doldurulmaz.

**Örnek 4, Çapa toplama** (2x3):
```
y=2  A  B
y=1  R  B
y=0  R  G
```
`Tap(0,0)` → `(0,0)`, `(0,1)` patlar. Çapa `(0,2)`'den dibe iner ve toplanır: `Collected` = `(0,2)→(0,0)`. Sütun 0 bomboştur ve üç hücre doldurulur. Çapa hedefi 1 azalır.

**Örnek 5, Su balonu** (3x1: `r R B`): `Tap(1,0)` → grup `(0,0)`, `(1,0)`. `Cleared`'da `(0,0)`'ın hücresi balonludur, balon hedefi 1 azalır, Red hedefi 2 azalır.

**Örnek 6, roket engelden geçer:** satırda `R C1 P A B` ve satırın başında yatay Rocket. Rocket satırı temizler: `C1` ve `P` kırılır (`Hits`), `A` yerinde kalır, `B` temizlenir.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- `Cell`: yeni fabrika ve erişimciler, yanlış türde erişim, balon yalnız renkli hücrede, eşitlik balonu ve katı içerir.
- Grup: engel bağı koparır, balonlu ve balonsuz kare aynı grup, engele ve boşa dokunuş hiçbir şey yapmaz.
- Vuruş: tek patlamada çoklu komşu tek vuruş, köşe ve kenarda komşuluk, çapraz komşu vurulmaz, Sandık 3→2→1→yok, Pinyata tek vuruşta yok, Çapa hiçbir alanda etkilenmez.
- Özel öğe ve kombo: Rocket'ın engelden geçmesi, Bomb alanı, Orb temizlediği karelerin komşusunu vurması, `OrbOrb` bütün engeller, `OrbRocket`/`OrbBomb` üst üste binen alanlarda tek vuruş, zincirde ayrı patlamalardan ikinci vuruş, kırılan engelin yerinin sonraki etkide boş olması.
- Su balonu: grupla, roket alanında ve Orb dönüşümünde patlama, komşu patlamanın vurmaması, özel öğe doğan hücrede balon.
- Düşme: Sandık üstünde duran kareler, birden çok Sandıklı sütun, Pinyatanın ve Çapanın düşmesi, Sandık kırılınca bölümlerin birleşmesi, `Moves` sırası.
- Çapa: ardışık iki Çapanın birlikte toplanması, zaten `y = 0`'da olan Çapa, Sandık üstündeki Çapanın toplanmaması, en alt bölüm dışındaki Çapa.
- Doldurma: Sandık altı boşluğun doldurulmaması, çekiliş sayısı, Sandıksız sütunun Adım 3'teki gibi kalması.
- `GameSession`: engel hedefi sayımı (dört tür), renk ve engel hedefi birlikte, yalnız engel hedefi, hedefsiz kurulum hatası, tekrarlanan engel hedefi, `Remaining(ObstacleKind)` hedef değilse hata, engele dokunuşun hamle harcamaması, kazanma koşulu.
- `LevelParser`: her yeni hata yolu, denetim sırası, varsayılan `layers`, yerleşim sayısını aşan hedef, `obstacles` yokken eski dosyaların aynen okunması.
- Tekrarlanabilirlik: aynı seed ve aynı dokunuşlar aynı sonucu verir.

## Arayüz sözleşmesi (kural katmanı)
Belirtilmeyen mevcut tipler ve imzalar değişmez. C# 9 sınırı. Dosya başına bir tip.

```csharp
public enum CellKind { Empty, Color, Special, Obstacle }   // sona eklenir

public enum ObstacleKind { Pinata, Crate, WaterBalloon, Anchor }

public readonly struct Cell                                  // eklenenler
{
    public static Cell OfObstacle(ObstacleKind kind, int layers);
        // tanımsız kind → ArgumentOutOfRangeException(kind); WaterBalloon → ArgumentException(kind);
        // Crate'te layers 1..3, Pinata/Anchor'da yalnız 1; değilse ArgumentOutOfRangeException(layers).
    public bool IsObstacle { get; }
    public ObstacleKind Obstacle { get; }   // engel hücresi değilse InvalidOperationException
    public int Layers { get; }              // engel hücresi değilse InvalidOperationException
    public bool HasBalloon { get; }         // her hücrede okunur; yalnız renkli hücrede true olabilir
    public Cell WithBalloon(bool hasBalloon); // renkli değilse InvalidOperationException
    // Eşitlik, GetHashCode ve ToString tür, renk, özel tür, engel türü, kat ve balonu içerir.
}

public readonly struct ObstacleHit : IEquatable<ObstacleHit>
{
    public ObstacleHit(CellPos pos, Cell before, Cell after);
    public CellPos Pos { get; }
    public Cell Before { get; }   // Pinata ya da Crate
    public Cell After { get; }    // Cell.Empty (kırıldı) ya da bir kat az Crate
    // Eşitlik, ==, !=, GetHashCode, ToString (ClearedCell gibi)
}

public sealed class TurnEffect                                // eklenenler
{
    public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created,
                      int triggeredBy, int mergedCount, IReadOnlyList<ClearedCell> converted,
                      IReadOnlyList<ObstacleHit> hits);
        // 7 parametreli kurucunun denetimleri, sonra hits null → ArgumentNullException(hits). Liste kopyalanmaz.
    // Mevcut kurucular kalır: Hits boş.
    public IReadOnlyList<ObstacleHit> Hits { get; }   // y artan, sonra x artan
}

public sealed class TurnResult                                // eklenenler
{
    public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled,
                      IReadOnlyList<TurnEffect> effects, IReadOnlyList<CellMove> collected);
        // mevcut 4 parametreli kurucunun denetimleri, sonra collected null → ArgumentNullException(collected).
    // Mevcut kurucular kalır: Collected boş.
    public IReadOnlyList<CellMove> Collected { get; }  // toplanan Çapalar: from → (x, 0); x artan, sonra from.y artan
}

public sealed class GravityResult
{
    public GravityResult(IReadOnlyList<CellMove> moves, IReadOnlyList<CellMove> collected); // null → ArgumentNullException (moves önce)
    public IReadOnlyList<CellMove> Moves { get; }
    public IReadOnlyList<CellMove> Collected { get; }
}

public static class GravityRules                              // eklenen
{
    public static GravityResult Settle(Grid grid);   // grid null → ArgumentNullException(grid)
    // Collapse(grid): imza aynı; Sandık bölümlerine uyar, Çapa toplamaz.
}
// RefillRules.Fill: imza aynı; yalnız en üst Sandığın üstündeki boşlukları doldurur.

public readonly struct ObstacleGoal : IEquatable<ObstacleGoal>
{
    public ObstacleGoal(ObstacleKind kind, int count);   // tanımsız kind → ArgumentOutOfRangeException(kind); count doğrulanmaz (ColorGoal gibi)
    public ObstacleKind Kind { get; }
    public int Count { get; }
    // Eşitlik, ==, !=, GetHashCode, ToString (ColorGoal gibi)
}

public readonly struct ObstaclePlacement : IEquatable<ObstaclePlacement>
{
    public ObstaclePlacement(ObstacleKind kind, CellPos pos, int layers);
        // tanımsız kind → ArgumentOutOfRangeException(kind); layers Crate'te 1..3, diğerlerinde 1 → değilse ArgumentOutOfRangeException(layers)
    public ObstacleKind Kind { get; }
    public CellPos Pos { get; }
    public int Layers { get; }
    // Eşitlik, ==, !=, GetHashCode, ToString
}

public sealed class GameSession                               // eklenenler
{
    public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                       IReadOnlyList<ObstacleGoal> obstacleGoals, IRandomSource random);
        // Hatalar parametre sırasıyla. goals ve obstacleGoals null → ArgumentNullException.
        // İkisi birden boş → ArgumentException(goals). Renk hedefi kuralları Adım 4'teki gibi (boş liste artık tek başına hata değil).
        // obstacleGoals: tanımsız tür, tekrar ya da Count < 1 → ArgumentException(obstacleGoals).
    // Mevcut 5 parametreli kurucu: obstacleGoals boş; goals boşsa hata eskisi gibi.
    public IReadOnlyList<ObstacleGoal> ObstacleGoals { get; }   // başlangıç değerleri, verilen sırayla
    public int Remaining(ObstacleKind kind);                  // hedef değilse ArgumentException(kind)
}

public sealed class LevelData                                 // eklenenler
{
    public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals,
                     IReadOnlyList<ObstaclePlacement> obstacles, IReadOnlyList<ObstacleGoal> obstacleGoals);
        // Önceki denetimler (renk hedefi boşluğu hariç), sonra obstacles null → ArgumentNullException;
        // grid dışı, çakışma, y = 0'da Anchor → ArgumentException(obstacles);
        // obstacleGoals null → ArgumentNullException; ikisi boş → ArgumentException(goals);
        // tekrar, Count < 1, yerleşim sayısını aşan → ArgumentException(obstacleGoals). Listeler kopyalanır.
    // Mevcut 5 parametreli kurucu: engel yok, goals boşsa hata eskisi gibi.
    public IReadOnlyList<ObstaclePlacement> Obstacles { get; }
    public IReadOnlyList<ObstacleGoal> ObstacleGoals { get; }
}
```

**Netleştirmeler (stub aşamasında, orchestrator, 2026-11-14):**
- Yeni tiplerin ve `Cell`'in engel/balon halinin `ToString` biçimi sözleşme değildir (boş olmayan metin döner).
- `GameSession` 6 parametreli kurucu denetim sırası: `grid`, `colorCount`, `moves`, `goals` (null, sonra renk kuralları), `obstacleGoals` null, ikisi birden boş → `ArgumentException(goals)`, `obstacleGoals` kuralları, `random`.
- `LevelData` 7 parametreli kurucu denetim sırası: `width`, `height`, `colorCount`, `moves`, `goals` (null, sonra renk kuralları), `obstacles` (null, sonra kurallar), `obstacleGoals` null, ikisi birden boş → `ArgumentException(goals)`, `obstacleGoals` kuralları.
- `WithBalloon` renkli olmayan hücrede `false` ile de `InvalidOperationException` atar.
- `Remaining(ObstacleKind)` tanımsız tür ya da hedef olmayan tür → `ArgumentException(kind)`.
- Tekli Orb'un kendi hücresi de temizlenen kare sayılır; onun yan komşusu olan engel de vurulur.
- `OrbRocket`/`OrbBomb`'da dönüşüm vuruş yapmaz: dönüşen karenin komşusu vurulmaz; yalnız dönüşen öğelerin alanları vurur.
- Vuruşlar etkiyi üreten yerde uygulanır: `SpecialRules.Activate` ve `SpecialRules.Tap` kendi etkilerinin `Hits`'ini doldurur ve grid'e uygular; grup patlamasının vuruşlarını `TurnRules.Tap` uygular. `BlastRules.Blast` değişmez (vuruş yapmaz).
- `GroupFinder.FindGroup` engel hücresinden başlatılınca boş liste döner (boş ve özel hücredeki gibi).
- `LevelParser` mesaj yolları: `y = 0`'daki Çapa → `'obstacles[i].y'`; hedefte `color` ve `obstacle` ikisi birden ya da hiçbiri → `'goals[i]'`.

**Sözleşme değişikliği notu:** `CellKind`'a değer eklenir. `GravityRules.Collapse` ve `RefillRules.Fill` Sandık varken yeni davranışa uyar; mevcut testlerde Sandık olmadığı için sonuç değişmez. Engelsiz grid'de bütün eski davranışlar aynen kalır. Mevcut testlerden sözleşme gereği güncellenmesi gereken olursa testçi günceller (RULES, Onay).

## Görüntü katmanı (`Assets/Scripts/View/`)
Otomatik test edilmez, kapısı elle kontrol listesidir.
- **Level modu geri:** varsayılan level oyunu (hamle, hedefler, kazan/kaybet paneli). Sonsuz oyun kodu kalır, bayrakla kapalıdır. HUD'da renk hedeflerinin yanında engel hedefleri kendi simgeleriyle görünür.
- **Çizimler:** önce yer tutucu çizimler (kodla, mevcut tarz). Görseller onaylanınca aynı yerlere sprite olarak takılır. Sandığın her katı ayrı görünür. Su balonu karenin üstünde şeffaf su katmanıdır, altındaki karenin rengi görünür.
- **Animasyonlar (hızlı, vurucu):** Pinyata kırılınca parçalanıp şekerleme/konfeti saçar. Sandık her vuruşta sarsılır, bir kat kırık tahta olarak düşer. Su balonu patlayınca su sıçrar. Çapa dibe inince aşağı kayıp hedef simgesine uçar. Hedefe sayılan engeller HUD'daki simgeye doğru uçar (renk hedefindeki gibi, varsa).
- **Deneme levelleri:** her engel için bir level dosyası `Assets/Resources/Levels/`'e eklenir (mevcut üç level kalır). Bunlar yalnız denemedir, Adım 7b'de yeniden yapılır.
- **Basit level seçici (ürün sahibi kararı, 2026-11-15):** başlangıçta level numaraları listelenir, dokunulan level doğrudan açılır; oyun içinde seçiciye dönmek için bir düğme olur. Kilit yok (her level açık). Yalnız görüntü katmanı; kural katmanına dokunmaz.
- Geçici nesneler havuzdan alınır, sızıntı olmaz.

## Görseller
- Dört engel (Sandığın 3 kat hali dahil) ve hedef simgeleri Gemini'nin görsel üretim aracıyla üretilir. Kodla ya da geometrik değil, özgün ve hoş görseller olur, mevcut kare ve özel öğe tarzıyla uyumlu ve şeffaf arka planlı. Su balonu şeffaf sudur.
- Orchestrator görselleri kontrol eder (özgünlük, boyut, şeffaflık, tutarlılık), ürün sahibi onaylar. Onaydan sonra kodcu `Assets/Art/Obstacles/` altına alır ve bağlar. Üretim aracı ve lisans notu kodcu raporuna ve README'ye (Adım 9) yazılır.

## Kapsam dışı
Level eğrisi ve şekilli tahta (Adım 7b), renkli ve metal engel çeşitleri, çıkmaz durum (Adım 8), ses, patlamanın sırayla izlenen animasyonu (Adım 9 adayı).

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 döner.
2. Level seçiciden her level açılıyor ve seçiciye dönülüyor. Level modu: hamle sayacı, renk ve engel hedefleri, kazan/kaybet paneli çalışıyor.
3. Her deneme level'ında: Pinyata komşu patlamayla kırılıyor ve düşüyor. Sandık kat kat kırılıyor, üstündeki kareler duruyor. Su balonu altındaki kareyle patlıyor. Çapa dibe inince toplanıyor.
4. Roket, bomba ve kombo engellere 1 vuruş yapıyor, Çapayı kırmıyor.
5. Animasyonlar hızlı ve okunaklı, Console temiz, sızıntı yok.
6. WebGL `gh-pages`'e yayımlanıyor, ürün sahibi iPhone'da 2-5'i deniyor.

## Akış (zor adım)
1. Spec onayı → `docs(spec): add obstacles spec`.
2. Görseller: Gemini üretir (paralel başlar), orchestrator kontrol eder, ürün sahibi onaylar.
3. Kodcu (Opus 5.5): stub → `feat(core): add obstacle stubs`.
4. Testçi (Opus 5.5 medium): yeni testler (~150) ve sözleşme gereği güncellemeler → `test(core): add obstacle tests`.
5. Kodcu: uygulama, level modu, yer tutucu çizim ve animasyon, deneme levelleri → `feat(core): implement obstacles`, `feat(view): show obstacles`. Görseller onaylıysa bağlar → `feat(view): add obstacle art`.
6. Claude (Opus medium, bilerek bozma) ve Gemini (`gemini-4-argon-medium`) paralel çalışır, bulgular kapanır.
7. WebGL önizleme, ürün sahibi dener.
8. Tarama, push, PR, CI, merge.
