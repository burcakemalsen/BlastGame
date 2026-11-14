# Spec 06: Özel öğeler

Durum: onaylandı (2026-11-14). Dal: `step/06-special-items`.

## Amaç
Büyük grup patlatmak ödüllendirilir: patlayan grubun yerinde bir özel öğe doğar. Özel öğeye dokununca kendi etkisini yapar; etkisi başka bir özel öğeye değerse o da çalışır (zincir). Mekanikler türün yaygın davranışına göredir; adlar, şekiller ve animasyonlar özgündür (RULES kural 1).

## Adımın bölünmesi (ürün sahibi onayı, 2026-11-14)
- **Adım 6 (bu spec):** üç özel öğe, zincirleme etki, animasyonları.
- **Adım 6b (ayrı spec):** kombolar (birbirine değen özel öğeler birleşip daha güçlü etki yapar).

## Bitti tanımı (PLAN, Adım 6)
- Büyük grup özel öğe üretiyor, özel öğe kendi etkisini yapıyor, zincir çalışıyor, etkilerin cilalı animasyonu var.
- `dotnet test` ve CI yeşil; Editor'de ve iPhone'da (WebGL) elle kontrol listesi geçiyor.

## Özel öğeler
| Grup boyutu | Öğe | Etki | Görünüş (ilkel şekiller) |
|---|---|---|---|
| 5-6 | **Rocket** | Yönü doğarken seçilir (yatay ya da dikey) ve görünür. Kendi satırını (yatay) ya da sütununu (dikey) temizler; kendi hücresinden iki yöne uçar. | Uzun eşkenar dörtgen, yönünü gösteren iki ok |
| 7-8 | **Bomb** | Kendisi + çevresindeki 8 kare (3x3) | Halkalı koyu daire |
| 9+ | **Orb** | Doğduğu grubun rengini taşır. O renkteki bütün renkli kareleri temizler. | O renkte küre, etrafında halka |

Rocket ve Bomb renksizdir (nötr açık/koyu ton; hiçbir hedef rengine benzemez).

Büyük grubun üstünde hangi öğenin doğacağı gösterilmez; doğan öğe sürprizdir (ürün sahibi kararı, 2026-11-14).

## Kurallar

### Hücre
- `CellKind`'a `Special` eklenir. Özel hücre bir `SpecialKind` taşır: `RocketHorizontal`, `RocketVertical`, `Bomb`, `Orb`. Orb ayrıca bir renk taşır.
- Özel hücre **gruba girmez**: `GroupFinder.FindGroup` özel hücreden başlatılınca boş liste döner (boş hücre gibi); renkli bir grup özel hücreyi asla içermez (zaten farklı türdür). `BlastRules.Blast` özel hücrede hiçbir şey yapmaz, boş liste döner.
- Özel hücre düşer (`GravityRules` boş olmayan her hücreyi düşürür, değişiklik yok). Doldurma yalnız renkli kare üretir (değişiklik yok).

### Dokunuş (`TurnRules.Tap`)
Önce bütün denetimler (bugünkü sırayla, değişmez). Sonra dokunulan hücreye göre:

1. **Boş** → boş sonuç; kaynak çağrılmaz.
2. **Renkli, grup < 2** → boş sonuç; kaynak çağrılmaz (bugünkü gibi).
3. **Renkli, grup 2-4** → grup temizlenir (bugünkü gibi), tek bir `Group` etkisi, `Created = Cell.Empty`.
4. **Renkli, grup ≥ 5 (birleşme)** → önce doğacak öğe belirlenir (`SpecialRules.ForGroup`): 5-6 Rocket, 7-8 Bomb, 9+ Orb (grubun rengi). Rocket'ta yön için kaynaktan **bir** çekiliş yapılır: `random.Next(2)`; `0` → `RocketHorizontal`, `1` → `RocketVertical`. Değer 0-1 dışıysa `InvalidOperationException`, grid'e dokunulmamıştır (çekiliş grid değişmeden önce yapılır). Sonra grubun **bütün** kareleri (dokunulan dahil) temizlenir ve dokunulan hücreye yeni öğe konur. Tek bir `Group` etkisi, `Created` = yeni öğe.
5. **Özel** → öğe çalışır ve zincir biter (`SpecialRules.Activate`, aşağıda). Grup gerekmez.

Patlatma ya da etki olduysa (3, 4, 5) **düşme bir kez, sonra doldurma bir kez** çalışır (Adım 3'teki gibi), bütün etkiler bittikten sonra.

**Kaynak çekiliş sırası (tekrarlanabilirlik için sözleşme):** bir dokunuşta önce (varsa) Rocket yön çekilişi, sonra doldurma çekilişleri (Adım 3'teki sırayla: `y` artan, sonra `x` artan). Bir dokunuşta en fazla bir birleşme olur, yani en fazla bir yön çekilişi. Özel öğeye dokunuşta yön çekilişi yoktur. Bomb ve Orb doğuşu çekiliş yapmaz.

### Etkiler ve alanları
Her özel öğe etkisi **önce kendi hücresini** temizler (`Cleared[0]` daima öğenin kendisidir), sonra alanını şu sırayla tarar:

- **Rocket** (kaynak `(sx, sy)`): uzaklık `d = 1, 2, ...` tahtanın ucuna kadar; her `d` için yatayda önce sol `(sx-d, sy)` sonra sağ `(sx+d, sy)`; dikeyde önce alt `(sx, sy-d)` sonra üst `(sx, sy+d)`. Tahta dışı atlanır.
- **Bomb** (kaynak `(bx, by)`): 3x3'ün kendisi dışındaki 8 hücresi, `y` artan sonra `x` artan; tahta dışı atlanır.
- **Orb** (renk `c`): tahtadaki `c` renkli bütün **renkli** kareler, `y` artan sonra `x` artan. Orb başka özel hücreleri (başka renkte ya da aynı renkte Orb dahil) hedeflemez; bu yüzden Orb zincir başlatmaz.

Taranan her hücre için:
- **Boş** (daha önce temizlenmiş) → atlanır.
- **Renkli** → temizlenir, bu etkinin `Cleared` listesine eklenir.
- **Özel ve henüz kuyrukta değil** → **temizlenmez**, zincir kuyruğunun sonuna eklenir (kendi hücresini kendi etkisinde temizleyecek).

### Zincir
- İlk öğe (dokunulan) kuyruğa konur. Kuyruktan sırayla (ilk giren ilk çıkar) öğe alınır, etkisi uygulanır; tararken bulduğu özel öğeler kuyruğun sonuna eklenir. Kuyruk boşalınca biter.
- Her etki kendisini tetikleyen etkinin `Effects` içindeki sırasını (`TriggeredBy`) taşır; dokunulan öğenin ve `Group` etkisinin değeri `-1`'dir. Görüntü bunu zamanlama için kullanır (aşağıda, Animasyon).
- Her özel öğe bir dokunuşta en fazla bir kez çalışır. Her hücre en fazla bir etkinin `Cleared` listesinde yer alır.
- Başka öğeyle tetiklenen Orb kendi rengini temizler. Etki alanında temizleyecek renkli kare olmasa da öğe kendi hücresini temizler.
- Adım 6'da kombo yok: yan yana iki özel öğe de olsa dokunulan tek başına çalışır, ötekini ancak alanı değerse zincirle çalıştırır.

### Hamle ve hedef (`GameSession`)
Bu adımda oyunda görünmez (aşağıda, Sonsuz oyun); kural katmanında tutarlı kalsın diye en basit kuralla güncellenir. Hedef türü ileride (engel hedefleriyle) yeniden ele alınır.
- Özel öğeye dokunuş **bir hamle harcar** (`HasBlasted` true olur, çünkü öğe kendi hücresini temizler).
- **Hedef sayımı değişir:** artık bir dokunuşta birden çok renk temizlenebilir. Bütün etkilerin `Cleared` listelerindeki **renkli** kareler renklerine göre sayılır; o renk hedefse kalanı o kadar düşer, 0'ın altına inmez. Özel öğenin kendisi (Orb dahil) hiçbir renge sayılmaz. Birleşmede dokunulan kare de sayılır (5'lik kırmızı grup → 5 kırmızı). Düşen ve doldurmayla gelen kareler sayılmaz (bugünkü gibi).
- Kazan/kaybet sırası ve oyun bittikten sonraki davranış değişmez (Adım 4).

## Örnekler
Grid'ler önceki spec'lerdeki gibi (`y` alttan üste). `R` Red, `G` Green, `B` Blue; `H` RocketHorizontal, `V` RocketVertical, `X` Bomb, `O(R)` kırmızı Orb. Sahte kaynak verilen değerleri sırayla döndürür.

**Örnek 1, birleşme → Rocket** (5x3, `colorCount = 3`, kaynak `1, 0, 0, 0, 0`):
```
y=2  B B G G G
y=1  R R R R R
y=0  G B G B G
```
`Tap(2, 1)` → 5'lik kırmızı grup. İlk çekiliş `Next(2) = 1` → `RocketVertical`. Grup temizlenir, `(2,1)`'e `V` konur. Etki: `Group`, `Origin (2,1)`, `Cleared` 5 kırmızı (`(0,1)..(4,1)`, `x` sırasıyla), `Created = V`. Düşme: `(0,2)->(0,1)`, `(1,2)->(1,1)`, `(3,2)->(3,1)`, `(4,2)->(4,1)` (`V` yerinde, altı dolu). Doldurma: `(0,2), (1,2), (3,2), (4,2)` hepsi `0` → Red.
```
y=2  R R G R R
y=1  B B V G G
y=0  G B G B G
```
`Blasted` 5 hücre; Red hedefi 5 düşer. Kaynak bir kez `Next(2)`, dört kez `Next(3)` çağrılır.

**Örnek 2, Rocket'a dokunuş** (Örnek 1'in sonu, kaynak hep `0`): `Tap(2, 1)` → etki `Rocket`, `Origin (2,1)`, `Cleared`: `(2,1) V`, `(2,0) G`, `(2,2) G`. Green hedefi 2 düşer, hamle 1 düşer. Sütun 2 tamamen boşalır; düşme yok; doldurma `(2,0), (2,1), (2,2)`.

**Örnek 3, zincir: Bomb → Rocket** (5x3):
```
y=2  R G B G R
y=1  G X H B G
y=0  B R G R B
```
`Tap(1, 1)`:
- Etki 1 `Bomb`, `Origin (1,1)`: `Cleared` = `(1,1) X`, `(0,0) B`, `(1,0) R`, `(2,0) G`, `(0,1) G`, `(0,2) R`, `(1,2) G`, `(2,2) B`. `(2,1) H` temizlenmez, kuyruğa girer.
- Etki 2 `Rocket`, `Origin (2,1)`, `TriggeredBy = 0`: `Cleared` = `(2,1) H`, `(3,1) B`, `(4,1) G` (`(1,1)` ve `(0,1)` zaten boş, atlanır). Etki 1'in `TriggeredBy` değeri `-1`.
- Sayım: Red 2, Green 4, Blue 3. İki özel öğe sayılmaz. `Blasted` 11 hücre, etkilerin sırasıyla.

**Örnek 4, Orb** (4x2):
```
y=1  R O(R) B R
y=0  B R    R G
```
`Tap(1, 1)` → etki `Orb`, `Cleared` = `(1,1) O(R)`, `(1,0) R`, `(2,0) R`, `(0,1) R`, `(3,1) R`. Red 4.

**Örnek 5, Rocket → Orb:** yatay bir Rocket'ın satırında mavi bir Orb varsa Orb kuyruğa girer, Rocket bitince çalışır ve tahtadaki bütün mavileri temizler (Rocket'ın zaten temizlediği hücreler boş olduğu için atlanır).

**Örnek 6, birleşme → Orb:** 9 ya da daha büyük yeşil grup → dokunulan hücrede yeşil Orb; kaynaktan yön çekilişi yapılmaz (ilk çekiliş doldurmanındır).

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Eşikler: grup 4 (öğe yok), 5 ve 6 (Rocket), 7 ve 8 (Bomb), 9 ve daha büyük (Orb, grubun rengi). Yön çekilişi `0` ve `1`; geçersiz değer (`-1`, `2`) → `InvalidOperationException` ve grid değişmemiş.
- Çekiliş sayısı ve sırası: Rocket birleşmesinde tam bir `Next(2)` ve doldurmadan önce; Bomb/Orb birleşmesinde ve özel öğeye dokunuşta yön çekilişi yok.
- Birleşmede doğan öğe dokunulan hücrede; grubun en alt/sol hücresine değil. Doğan öğe düşmeyle aşağı iner.
- Rocket: köşede, kenarda, ortada; yatay ve dikey; 1 genişlik/yükseklik tahtada yalnız kendi hücresi; tarama sırası (eşit uzaklıkta sol/alt önce).
- Bomb: köşede (4 hücre), kenarda (6), ortada (9).
- Orb: renginde başka kare yokken yalnız kendisi; başka Orb'ları (aynı renk dahil) temizlemez, tetiklemez.
- Zincir: iki ve daha uzun zincir; kuyruk sırası (ilk bulunan önce); `TriggeredBy` değerleri (Örnek 3'te `-1`, `0`); bir öğenin iki etkinin alanında olması (bir kez çalışır); birbirini gören iki Rocket (döngü yok); daha önce temizlenmiş hücrenin atlanması; her hücrenin tek bir `Cleared`'da olması.
- `GroupFinder`/`BlastRules`: özel hücreden başlayınca boş liste, grid değişmez; renkli grup komşu özel hücreyi içine almaz.
- `GameSession`: özel öğeye dokunuş hamle harcar; çok renkli temizlikte her renk ayrı sayılır; özel öğe sayılmaz; son hamlede zincirle kazanma; oyun bitince özel öğeye dokunuş hiçbir şeyi değiştirmez ve kaynağa dokunmaz.
- `Cell`: `OfSpecial`/`OfOrb` hata durumları, `Special`/`OrbColor`/`Color` erişimi yanlış türde hata, eşitlik (yön farkı, Orb renk farkı), `GetHashCode` tutarlılığı, `ToString`.
- Tekrarlanabilirlik: aynı seed, aynı dokunuşlar → aynı grid, aynı etkiler.
- `TurnResult`: `Blasted` = etkilerin `Cleared` konumlarının sırayla birleşimi; eski kurucu `Effects`'i boş verir.

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)
Belirtilmeyen mevcut tipler ve imzalar değişmez. C# 9 sınırı: `record` ve `init` yok. Dosya başına bir tip.

```csharp
public enum CellKind { Empty = 0, Color, Special }                 // Special eklenir

public enum SpecialKind { RocketHorizontal, RocketVertical, Bomb, Orb }

public readonly struct Cell : IEquatable<Cell>                     // eklenenler
{
    public static Cell OfSpecial(SpecialKind kind);  // tanımsız değer → ArgumentOutOfRangeException(kind); Orb → ArgumentException(kind), Orb için OfOrb
    public static Cell OfOrb(TileColor color);       // tanımsız renk → ArgumentOutOfRangeException(color)
    public bool IsSpecial { get; }
    public SpecialKind Special { get; }              // özel değilse InvalidOperationException
    public TileColor OrbColor { get; }               // Orb değilse InvalidOperationException
    // Color yalnız CellKind.Color'da geçerli kalır (Orb'da da InvalidOperationException).
    // Eşitlik: tür + SpecialKind + (Orb ise) renk. ToString: "Special(Bomb)", "Special(Orb, Red)".
}

public enum EffectKind { Group, Rocket, Bomb, Orb }

/// <summary>A cell emptied by an effect, with what it held before.</summary>
public readonly struct ClearedCell : IEquatable<ClearedCell>
{
    public ClearedCell(CellPos pos, Cell cell);
    public CellPos Pos { get; }
    public Cell Cell { get; }
    // Equals, GetHashCode, ==, !=, ToString: "(x, y) Color(Red)"
}

/// <summary>One step of a tap, in play order.</summary>
public sealed class TurnEffect
{
    public TurnEffect(EffectKind kind, CellPos origin, IReadOnlyList<ClearedCell> cleared, Cell created, int triggeredBy);
        // cleared null → ArgumentNullException; triggeredBy < -1 → ArgumentOutOfRangeException(triggeredBy);
        // liste kopyalanmaz (TurnResult gibi)
    public EffectKind Kind { get; }
    public int TriggeredBy { get; }                  // tetikleyen etkinin Effects sırası; ilk etki ve Group'ta -1
    public CellPos Origin { get; }                   // Group: dokunulan hücre; diğerleri: öğenin hücresi
    public IReadOnlyList<ClearedCell> Cleared { get; } // bu etkinin temizlediği hücreler, tarama sırasıyla
    public Cell Created { get; }                     // yalnız Group'ta doğan öğe olabilir; yoksa Cell.Empty
}

public sealed class TurnResult                                     // eklenenler
{
    public TurnResult(IReadOnlyList<CellPos> blasted, IReadOnlyList<CellMove> moves,
                      IReadOnlyList<CellPos> filled, IReadOnlyList<TurnEffect> effects); // effects null → ArgumentNullException
    // Mevcut 3 parametreli kurucu kalır; Effects boş liste olur.
    public IReadOnlyList<TurnEffect> Effects { get; } // çalışma (kuyruk) sırasıyla; zamanlama TriggeredBy ile
    // Blasted: etkilerin Cleared konumlarının sırayla birleşimi (birleşmede dokunulan hücre dahil,
    // orada artık yeni öğe dursa da). HasBlasted değişmez.
}

public static class SpecialRules
{
    public const int RocketMinGroup = 5;
    public const int BombMinGroup = 7;
    public const int OrbMinGroup = 9;

    /// Doğacak öğe; size < 5 → Cell.Empty ve kaynak çağrılmaz. 5-6'da tek Next(2) çekilişi.
    /// Hatalar, bu sırayla: random null → ArgumentNullException; size < 0 → ArgumentOutOfRangeException(size);
    /// tanımsız color → ArgumentOutOfRangeException(color); geçersiz çekiliş → InvalidOperationException.
    public static Cell ForGroup(int size, TileColor color, IRandomSource random);

    /// (x, y)'deki özel öğeyi çalıştırır, zinciri bitirir, etkileri çalışma (kuyruk) sırasıyla döndürür.
    /// Düşme ve doldurma yapmaz. Hatalar, bu sırayla: grid null → ArgumentNullException;
    /// grid dışı → ArgumentOutOfRangeException (önce x sonra y); hücre özel değil → ArgumentException.
    public static IReadOnlyList<TurnEffect> Activate(Grid grid, int x, int y);
}
```

`TurnRules.Tap` ve `GameSession.Tap` imzaları değişmez; davranışları yukarıdaki kurallara göre genişler. Etkisiz dokunuşta (`HasBlasted` false) `Effects` boştur. `GameSession` oyun bittikten sonra döndürdüğü boş sonuçta da `Effects` boştur.

**Netleştirmeler (stub aşamasında çıkan sorular, orchestrator, 2026-11-14):**
- `ForGroup`: denetimler boyuttan bağımsız, yazılan sırayla; 0-4 boyutta tanımsız renk de `ArgumentOutOfRangeException(color)`.
- 4 parametreli `TurnResult` kurucusu: null denetimi `blasted`, `moves`, `filled`, `effects` sırasıyla; `Blasted` ile etkilerin tutarlılığı kurucuda denetlenmez.
- `TurnEffect` kurucusu yalnız yazılan iki denetimi yapar (`kind`/`created` uyumu ve tanımsız `kind` denetlenmez).
- `Activate` tahta dışı hatasında `ParamName` `x` ya da `y` (`GroupFinder` gibi).
- Orb hücresinde `Cell.Special` `SpecialKind.Orb` döner.
- `Group` etkisinin `Cleared` sırası `GroupFinder` sırasıdır (`y` artan, sonra `x` artan). Düşme listesi Adım 3'teki gibi (`x` artan, sonra `To.Y` artan). Rocket `ToString`: `"Special(RocketHorizontal)"`, `"Special(RocketVertical)"`.

**Sözleşme değişikliği notu:** `GroupFinder.FindGroup`/`BlastRules.Blast` özel hücre için yeni bir durum kazanır (önceden özel hücre yoktu); `GameSession`'ın hedef sayımı renk başına olur (tek renkli grupta sonuç aynı). Mevcut testlerin bunlar yüzünden değişmesi gerekirse testçi yapar (RULES, Onay).

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi. Sahne dosyasına dokunulmaz.

### Sonsuz oyun (ürün sahibi kararı, 2026-11-14)
Bu adımda odak özel öğeler; hamle, hedef ve kazan/kaybet oyunda görünmez, tahta sonsuz oynanır.
- Dokunuş `GameSession.Tap` yerine doğrudan `TurnRules.Tap(grid, x, y, colorCount, random)` ile işlenir (oturum hamle bitince dokunuşu keseceği için). Grid boyutu ve renk sayısı yine level dosyasından gelir; hamle ve hedef alanları okunur ama kullanılmaz.
- Üst bardaki hamle sayacı ve hedefler, kazan/kaybet paneli **gizlenir; kodları silinmez** (`GameSession`, `HudView`'deki ilgili parçalar, level dosyaları ve testleri yerinde kalır, ileride yeniden açılır). Bunu tek bir adlandırılmış ayar (ör. `EndlessMode = true`) yönetir.
- Görünür kalanlar: level numarası, seed ve küçük bir **"Next board"** butonu. Butona basınca sıradaki level dosyası (sonuncudan sonra ilki) yeni seed'le açılır; böylece 3, 4 ve 5 renkli tahtalar denenebilir. Animasyon sürerken buton çalışmaz. Yükleme hatası paneli bugünkü gibi kalır.

### Özel öğelerin çizimi
- Tablodaki görünüşler, kodla üretilen ilkel şekillerle (Adım 1'deki gibi doku başlangıçta bir kez üretilir, dokunuş başına yeni doku yok). Rocket'ın yönü bir bakışta anlaşılır; Orb'un rengi hedef karelerin rengiyle aynıdır.
- Özel öğeler hafif bir "canlılık" hareketi yapar (ör. Orb halkasının yavaş dönmesi ya da hafif nabız). Seçim kodcunun; dikkat dağıtmayacak kadar hafif.

### Animasyon (cilalı olmalı)
Süreler hedeftir, kodcu ayarlayabilir; ürün sahibi elle kontrolde bakar. Hareketlerde doğrusal yerine yumuşak eğri (ease) kullanılır. **Etkiler hızlı ve vurucudur** ("göz açıp kapayıncaya kadar"; türün yaygın davranışı): uzun bir zincir de bir iki saniyede biter, oyuncu beklemez.
- **Normal patlama:** kare küçülerek kaybolur (bugünkü 0.12 sn; hafif büyüyüp küçülme olabilir).
- **Birleşme:** grup kareleri dokunulan hücreye doğru kayıp küçülerek toplanır (~0.2 sn), sonra yeni öğe "pat" diye belirir (0'dan biraz büyüğe, sonra normale, ~0.15 sn).
- **Rocket:** öğe iki parçaya ayrılır, iki parça zıt yönlere tahtanın ucuna kadar sabit hızla uçar (~0.025 sn/hücre); geçtiği her kare parça üstünden geçerken kaybolur. Arkasında kısa, sönen bir iz bırakır.
- **Bomb:** anında patlar: merkezde kısa parlama, dışa doğru büyüyüp sönen bir halka dalgası (~0.15 sn); 3x3 kareler dalgayla birlikte kaybolur. İsteğe bağlı: tahtada çok kısa bir sarsıntı.
- **Orb:** Orb kısa nabız atar, sonra hedef karelere çok küçük aralıklarla ışın gider; ışın değen kare kısa beyaz parlamayla kaybolur, ışınlar söner. Toplam ~0.5 sn'yi geçmez (hedef sayısı çoksa aralık kısalır).
- **Zincir (üst üste, beklemeden):** tetiklenen öğe, tetikleyen etki **ona ulaştığı anda** patlar; tetikleyenin bitmesi beklenmez. Rocket'ta an, parçanın o hücreye vardığı andır (uzaklık × hücre başı süre); Bomb'da patlama anıdır. Görüntü bu anı `TriggeredBy` ve hücre konumlarından hesaplar. Böylece etkiler aynı anda sürebilir ("bam bam").
- **Düşme ve doldurma:** bütün etkiler bittikten sonra, Adım 3 Ek A'daki gibi.
- Animasyon sürerken dokunuş geçmez (bugünkü gibi). Sayaçlar dokunuştan hemen sonra güncellenir (bugünkü gibi).
- Işın, iz, halka gibi geçici nesneler havuzdan (pool: bir kez üretilip tekrar kullanılan nesne kümesi) gelir; dokunuş başına kalıcı nesne üretilmez, "Play again"de sızıntı olmaz.
- Ses yok.

## Kapsam dışı
Kombolar (Adım 6b), engeller (Adım 7), ses, çıkmaz durum (Adım 8), level dosyasında başlangıç özel öğesi, AI ile üretilen görseller (Adım 9'da değerlendirilir).

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play (3 renkli Level 1, büyük grup sık çıkar): 5-6'lık grup Rocket, 7-8'lik Bomb, 9+'lık Orb üretir; öğe dokunulan hücrede doğar; Rocket'ın yönü görünür; Orb grubun rengindedir.
3. Her öğeye dokununca etkisi doğru: Rocket satır/sütun, Bomb 3x3, Orb kendi rengi. Hamle/hedef barı ve sonuç paneli görünmez; tahta istenildiği kadar oynanır. "Next board" sıradaki level'ı yeni seed'le açar.
4. Zincir: bir öğenin alanındaki öğe de çalışır; etki ona ulaştığı anda patlar (beklemez), düşme hepsinden sonra başlar. Uzun zincir de kısa sürer.
5. Animasyonlar akıcı ve okunaklı (ürün sahibinin gözü); Console temiz; birkaç "Play again"den sonra sızıntı belirtisi yok.
6. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone Safari'de 2-5'i dener (akıcılık telefonda da yeterli).

## Adımın akışı (PLAN bölüm 4 ve 9; zor adım)
1. Spec onayı → `docs(spec): add special items spec`.
2. Kodcu (Opus 5.5): stub'lar → `feat(core): add special item stubs`. Orchestrator `dotnet build` ve `unity-check`'e bakar.
3. Testçi (Opus 5.5 medium): yeni testler (~150 sınırı) ve sözleşmenin gerektirdiği güncellemeler. Yeni testler kırmızı, eskiler yeşil → `test(core): add special item tests`.
4. Kodcu: uygulama, bütün testler yeşil → `feat(core): implement special items`; görüntü + animasyon → `feat(view): draw and animate special items`.
5. Kontrolcüler paralel: Claude (Opus 5.5 medium, bilerek bozma) + Gemini (`--model gemini-4-argon-medium`, yalnız okur). Bulgular kapanır.
6. WebGL önizlemesi `gh-pages`'e; ürün sahibi Editor'de ve iPhone'da dener.
7. Tarama, push, PR, CI yeşil, merge.
