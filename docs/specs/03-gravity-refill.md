# Spec 03: Düşme ve doldurma

Durum: onaylandı (2026-11-14). Dal: `step/03-gravity-refill`.

## Amaç
Patlamadan sonra kareler sütunlarında aşağı iner (düşme), üstte kalan boşluklar seed'li rastgele renkle dolar (doldurma). Bir dokunuşun bütün kuralı (patlat → düşür → doldur) kural katmanında tek bir çağrıda toplanır; görüntü yalnız onu çağırır ve sonucu çizer. Aynı seed ve aynı dokunuş sırası her zaman aynı grid'i verir.

Bu adımda animasyon yok (Adım 9); görüntü bir karede yeni hale geçer. Hamle sayacı ve hedef yok (Adım 4).

## Bitti tanımı (PLAN, Adım 3)
- Patlamadan sonra kareler aşağı iniyor, üstten seed'li rastgele renk geliyor; aynı seed aynı sonucu veriyor.
- `dotnet test` ve CI yeşil; Editor'de ve iPhone'da (WebGL) dokununca grup kayboluyor, üsttekiler iniyor, boşluk doluyor.

## Düşme (`GravityRules.Collapse`)
- Her sütun ayrı işlenir; kare yalnız kendi sütununda, yalnız aşağı iner. Sütunlar birbirini etkilemez.
- Sütundaki dolu hücreler **sıralarını koruyarak** en alta toplanır: alttan yukarı ilk dolu hücre `y = 0`'a, ikincisi `y = 1`'e iner, vb. Boşluklar sütunun üstünde kalır.
- Dönüş: yer değiştiren her kare için bir `CellMove(From, To)`. Yerinde kalan kare listeye girmez. Sıra: önce `x` küçükten büyüğe, aynı sütunda `To.Y` küçükten büyüğe. (Adım 9'daki düşme animasyonu bu listeyi kullanacak.)
- Hiç boşluk yoksa ya da boşluklar zaten üstteyse grid değişmez, liste boş.
- Kareyi taşımak `Cell` değerini olduğu gibi taşır (tür ve renk değişmez).
- `grid` `null` → `ArgumentNullException`.
- Not: Adım 7'de engel düşmeyi etkileyebilir; o kural o adımın spec'inde. Bu adımda hücre yalnız `Empty` ya da `Color`.

## Doldurma (`RefillRules.Fill`)
- Grid'deki her `Empty` hücreye `Cell.OfColor((TileColor)random.Next(colorCount))` yazar.
- **Sıra sabittir:** `y` küçükten büyüğe, aynı `y`'de `x` küçükten büyüğe (Adım 1'deki doldurma sırası). Rastgele kaynak yalnız boş hücre başına bir kez çağrılır; dolu hücre için çağrılmaz.
- Dönüş: doldurulan hücreler, aynı sırayla. Boş hücre yoksa boş liste ve kaynak hiç çağrılmaz.
- `colorCount` Adım 1'deki gibi `1..6` (`GridFactory.MaxColorCount`); dışı → `ArgumentOutOfRangeException`. `grid` ya da `random` `null` → `ArgumentNullException`. Kontrol sırası: `grid`, `colorCount`, `random`; hepsi kaynağa dokunmadan önce.
- Kaynak `0 .. colorCount-1` dışında değer döndürürse → `InvalidOperationException` ve **grid değişmez** (önce bütün değerler çekilir ve denetlenir, sonra yazılır).
- `Fill` boşlukların nerede olduğuna bakmaz; düşmeden önce çağrılırsa ortadaki boşluğu da doldurur. Doğru sırayı `TurnRules.Tap` kurar.

## Bir dokunuş (`TurnRules.Tap`)
- Sıra: `BlastRules.Blast(grid, x, y)` → patlama olduysa `GravityRules.Collapse(grid)` → `RefillRules.Fill(grid, colorCount, random)`.
- Patlama olmadıysa (tek kare ya da boş hücre) grid değişmez ve **kaynak çağrılmaz**; sonuçtaki üç liste boş. Böylece boşa dokunuşlar sonraki renkleri kaydırmaz.
- Dönüş `TurnResult`: `Blasted` (Blast'ın listesi), `Moves` (Collapse'ın), `Filled` (Fill'in). `HasBlasted` = `Blasted.Count > 0`.
- Hatalar, hiçbir şeyi değiştirmeden ve kaynağa dokunmadan önce: `grid` `null` → `ArgumentNullException`; `random` `null` → `ArgumentNullException`; `colorCount` dışı → `ArgumentOutOfRangeException`; `(x, y)` grid dışı → `ArgumentOutOfRangeException`. Sıra: `grid`, `random`, `colorCount`, koordinat.
- Kaynak geçersiz değer döndürürse `InvalidOperationException` (Fill'den gelir); bu durumda patlama ve düşme olmuş, doldurma olmamış hal kalır. (Gerçek oyunda kaynak `SeededRandom`'dır ve bu olmaz; testte sahte kaynakla görülür.)
- **Tekrarlanabilirlik:** aynı başlangıç grid'i, aynı seed'li kaynak ve aynı dokunuş dizisi → hücre hücre aynı grid ve aynı `TurnResult` listeleri.

## Örnekler

Grid'ler üst satır yukarıda (`y` alttan üste). `R` Red, `G` Green, `B` Blue, `Y` Yellow, `.` boş.

**Örnek 1, düşme** (3x4):
```
y=3  G . B
y=2  . R .
y=1  R . B
y=0  . . G
     x=0 1 2
```
`Collapse` → `(0,1)→(0,0), (0,3)→(0,1), (1,2)→(1,0), (2,3)→(2,2)`. `(2,0)` ve `(2,1)` yerinde kaldığı için listede yok. Sonra:
```
y=3  . . .
y=2  . . B
y=1  G . B
y=0  R R G
```

**Örnek 2, doldurma:** Örnek 1'in sonucuna `Fill(grid, 3, kaynak)`, sahte kaynak sırayla `0, 1, 2, 0, 1, 2` döndürür. Boş hücreler sırayla `(1,1), (0,2), (1,2), (0,3), (1,3), (2,3)` → `R, G, B, R, G, B`. Liste bu altı hücre.

**Örnek 3, bir dokunuş** (3x3, `colorCount = 3`, sahte kaynak `2, 2, 2, ...`):
```
y=2  B G G
y=1  R R G
y=0  B R B
```
`Tap(1, 0)` → `Blasted`: `(1,0), (0,1), (1,1)`. `Collapse`: sütun 0'da `B(0,2)` → `(0,1)`; sütun 1'de `G(1,2)` → `(1,0)`. `Moves`: `(0,2)→(0,1), (1,2)→(1,0)`. Boşlar Fill sırasıyla `(1,1), (0,2), (1,2)` → hepsi `B`; `Filled` bu liste. Sonuç:
```
y=2  B B G
y=1  B B G
y=0  B G B
```
`Tap(0, 0)` ilk grid'de tek kare (`B`, komşuları `R`) → grid aynı, kaynak çağrılmaz, üç liste boş.

**Örnek 4, tekrarlanabilirlik:** iki ayrı `SeededRandom(42)` nesnesi; her biri önce kendi grid'ini `CreateRandom(8, 10, 5, kaynak)` ile üretir, sonra aynı nesne o grid'in dokunuşlarında kullanılır. İki grid'e aynı dokunuş dizisi uygulanınca her dokunuştan sonra grid'ler hücre hücre aynıdır.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Düşme: sütun tamamen boş; sütun tamamen dolu; boşluk en altta, ortada, birden çok parça halinde; tek dolu hücre en üstte (en uzun düşüş); 1x1, 1xN ve Nx1 grid; 16x16. Sıra korunuyor (renkler karışmıyor). `Collapse` ikinci kez çağrılınca boş liste, grid aynı. Sütunlar birbirini etkilemiyor. `null`.
- Doldurma: boş hücre yok → kaynak hiç çağrılmıyor (sayan sahte kaynak); yalnız bir boş hücre; bütün grid boş (16x16 → 256 çağrı, sıra doğru); dolu hücreler değişmiyor. `colorCount` 0, 1, 6, 7, negatif; `null` grid ve kaynak; kontrol sırası. Kaynak `colorCount`, `-1` döndürünce hata ve grid değişmemiş (ilk boş hücrede de son boş hücrede de).
- Dokunuş: tek kareye ve boş hücreye dokunuş → hiçbir şey ve kaynak çağrılmıyor; geçerli patlamadan sonra grid'de boş hücre yok; patlamaya katılmayan sütunlar değişmiyor; tüm grid tek renk (patlayıp tamamen yeniden doluyor); hata girdileri ve sıraları, hata sonrası grid ve kaynak dokunulmamış; tekrarlanabilirlik (aynı seed iki kez); farklı seed farklı sonuç (en az bir hücre farklı).
- `CellMove`: eşitlik, `GetHashCode` tutarlılığı, `ToString`.

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)
Mevcut tipler değişmez.

```csharp
public readonly struct CellMove : IEquatable<CellMove>
{
    public CellMove(CellPos from, CellPos to);
    public CellPos From { get; }
    public CellPos To { get; }

    public bool Equals(CellMove other);
    public override bool Equals(object obj);
    public override int GetHashCode();
    public override string ToString();               // "(x, y)->(x, y)"
    public static bool operator ==(CellMove a, CellMove b);
    public static bool operator !=(CellMove a, CellMove b);
}

public static class GravityRules
{
    public static IReadOnlyList<CellMove> Collapse(Grid grid);
}

public static class RefillRules
{
    public static IReadOnlyList<CellPos> Fill(Grid grid, int colorCount, IRandomSource random);
}

public sealed class TurnResult
{
    public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves, IReadOnlyList<CellPos> filled);
    public IReadOnlyList<CellPos> Blasted { get; }
    public IReadOnlyList<CellMove> Moves { get; }
    public IReadOnlyList<CellPos> Filled { get; }
    public bool HasBlasted { get; }                  // Blasted.Count > 0
}

public static class TurnRules
{
    public static TurnResult Tap(Grid grid, int x, int y, int colorCount, IRandomSource random);
}
```

`TurnResult` kurucusu `null` liste alırsa `ArgumentNullException`; kontrol parametre sırasıyla (`blasted`, `moves`, `filled`), hata ilk `null` parametrenin adını taşır. Listeler kopyalanmadan saklanır. Dosya başına bir tip: `CellMove.cs`, `GravityRules.cs`, `RefillRules.cs`, `TurnResult.cs`, `TurnRules.cs`. C# 9 sınırı: `record` ve `init` yok.

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi. Sahne dosyasına dokunulmaz; ayarlar (8x10, 5 renk, seed 42) aynen.

- **Tek kaynak:** `GameBootstrap` bir `SeededRandom(42)` oluşturur; grid'i onunla üretir ve aynı nesneyi doldurma için kullanmaya devam eder. Böylece ilk dizilim Adım 1-2'dekiyle aynı kalır ve oyun baştan sona tek seed'e bağlıdır.
- Dokunuşta `BlastRules.Blast` yerine `TurnRules.Tap(_grid, x, y, ColorCount, _random)` çağrılır, sonra `_view.Refresh()`. Girdi ve ekran→hücre çevirisi Adım 2'deki gibi.
- **Sprite önbelleği (Backlog maddesi):** kare sprite'ı ve dokusu bir kez üretilir ve saklanır; `Build` tekrar çağrılsa da yeni doku oluşturmaz. `Refresh` yeni doku ya da nesne oluşturmaz (Adım 2'deki gibi). Madde Backlog'dan silinir.
- Animasyon yok: dokunuştan sonraki karede grid yeni haliyle görünür.

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play: ilk dizilim Adım 2'dekiyle aynı.
3. 2+ gruba tıklayınca grup kaybolur, üstündeki kareler iner, sütunların tepesi yeni renklerle dolar; grid'de boşluk kalmaz.
4. Tek kareye tıklamak hiçbir şey yapmaz; sonra yapılan patlamanın renkleri, tek kareye hiç tıklanmamış gibi aynıdır.
5. Play'i durdurup başlatıp aynı tıklama dizisini tekrarlayınca aynı grid'ler oluşur (en az 3 tıklama, ekran görüntüsüyle karşılaştırma).
6. Uzun süre (20+ patlatma) tıklamak hata vermez, Console temiz.
7. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone Safari'de 3, 4 ve 6'yı dener.

## Adımın akışı (PLAN bölüm 4 ve 9; zor adım)
1. Spec onayı → `docs(spec): add gravity and refill spec`.
2. Kodcu (Opus 5.5): yeni tiplerin stub'ları → `feat(core): add gravity and refill stubs`. Orchestrator sözleşmeyle karşılaştırır, `dotnet build` ve `unity-check`.
3. Testçi (Opus 5.5 medium): yeni testler, ~150 sınırı (RULES, Testçi); mevcut testlere dokunulmaz. Yeni testler kırmızı, eskiler yeşil → `test(core): add gravity and refill tests`.
4. Kodcu: uygulama + görüntü katmanı, bütün testler yeşil → `feat(core): implement gravity and refill`, `feat(view): collapse and refill after blast`.
5. Kontrolcüler paralel: Claude (Sonnet 5.5, bilerek bozma) ve Gemini (`agy`; önce hata çıktısı görünür şekilde denenir, çalışmazsa atlanır ve rapora yazılır). Bulgular kapanır.
6. WebGL önizlemesi `gh-pages`'e; ürün sahibi Editor'de ve iPhone'da dener.
7. Tarama, push, PR, CI yeşil, merge.
