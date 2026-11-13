# Spec 01: Grid

Durum: onaylandı (2026-11-14). Dal: `step/01-grid`.

## Amaç
Oyun alanının kural katmanındaki modelini (grid) ve seed'li rastgele doldurmayı kurmak; Unity'de bu grid'i renkli karelerle göstermek. Bu adımda dokunma, patlatma, düşme yok.

## Bitti tanımı (PLAN, Adım 1)
- Kural katmanında grid modeli testli (`dotnet test` yeşil, CI yeşil).
- Play'de renkli karelerden grid görünüyor.
- Grid'li WebGL sayfası telefonda açıldı.

## Davranış

### Koordinat sistemi
- Hücre `(x, y)`: `x` sütun (soldan sağa, `0 .. Width-1`), `y` satır (**alttan üste**, `0 .. Height-1`). `(0, 0)` sol alt köşedir.
- Gerekçe: Adım 3'te kareler aşağı düşecek; "aşağı" = `y` azalır. Unity'nin dünya koordinatı da y yukarı.

### Grid
- Boyut: `1 ≤ Width ≤ 16`, `1 ≤ Height ≤ 16`. Dışı `ArgumentOutOfRangeException`.
- Yeni grid'in bütün hücreleri boştur.
- Bir hücre ya boştur ya da bir renk taşır. Bu adımda başka içerik (engel, özel öğe) yok.
- Grid dışındaki bir hücreyi okumak ya da yazmak `ArgumentOutOfRangeException`.
- `IsInside(x, y)` hiçbir zaman hata vermez; içerideyse `true`.

### Renkler
- Altı renk: `Red`, `Green`, `Blue`, `Yellow`, `Purple`, `Orange` (bu sırayla, değerleri `0..5`).
- Bir level kaç renk kullanacağını `colorCount` ile seçer: ilk `colorCount` renk kullanılır (`colorCount = 3` → `Red`, `Green`, `Blue`).

### Rastgele kaynak
- Kural katmanı rastgeleliği yalnız dışarıdan verilen `IRandomSource` üzerinden alır.
- `Next(maxExclusive)`: `0 ≤ sonuç < maxExclusive`. `maxExclusive ≤ 0` → `ArgumentOutOfRangeException`.
- `SeededRandom(seed)`: aynı seed her zaman aynı diziyi üretir; her `int` seed geçerlidir (negatif ve 0 dahil).
- Algoritma **SplitMix64** (standart sabitleriyle); başlangıç durumu `(ulong)(long)seed`; `Next(max)` = `(int)(üretilen64 % (ulong)max)`. `System.Random` kullanılmaz. Gerekçe: `System.Random`'ın seed'li dizisi .NET ile Unity'nin çalışma ortamları arasında aynı olmayabilir; kendi algoritmamız testte ve oyunda birebir aynı sonucu verir.

### Rastgele doldurma
- `GridFactory.CreateRandom(width, height, colorCount, random)`: her hücreye `(TileColor)random.Next(colorCount)` yazar.
- **Doldurma sırası sabittir:** `y = 0`'dan `Height-1`'e, her satırda `x = 0`'dan `Width-1`'e. Böylece aynı kaynak aynı grid'i verir ve testler sahte bir kaynakla tam beklenen grid'i kontrol edebilir.
- `colorCount` dışı (`< 1` ya da `> 6`) → `ArgumentOutOfRangeException`. `random` `null` → `ArgumentNullException`. Boyut kuralları Grid'deki gibi.
- Sonuçta boş hücre kalmaz.

## Örnekler
- `new Grid(3, 2)` → `Width = 3`, `Height = 2`, `Get(0,0)` ... `Get(2,1)` hepsi `null`.
- `grid.Set(2, 1, TileColor.Blue)` → `Get(2, 1) == Blue`; diğer hücreler değişmez. `Set(2, 1, null)` → hücre yeniden boş.
- `Get(3, 0)`, `Get(-1, 0)`, `Get(0, 2)` → `ArgumentOutOfRangeException`.
- Sahte kaynak `0, 1, 2, 0, 1, 2` sırasıyla döndürürse, `CreateRandom(3, 2, 3, kaynak)` → alt satır `(0,0)=Red, (1,0)=Green, (2,0)=Blue`, üst satır `(0,1)=Red, (1,1)=Green, (2,1)=Blue`.
- `CreateRandom(8, 10, 5, new SeededRandom(42))` iki kez çağrılırsa iki grid hücre hücre aynıdır.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Boyut sınırları: 0, 1, 16, 17, negatif; genişlik ve yükseklik ayrı ayrı.
- Köşe hücreler `(0,0)`, `(W-1,0)`, `(0,H-1)`, `(W-1,H-1)`; hemen dışları.
- 1x1 grid; 16x16 grid.
- `colorCount` 0, 1, 6, 7; `colorCount = 1` → bütün grid tek renk (`Red`).
- `colorCount = 6` ve yeterince büyük grid'de yalnız `0..5` değerleri görülür.
- `SeededRandom`: aynı seed → aynı ilk 100 değer; seed 1 ile 2'nin ilk 10 değeri aynı değil; `Next(1)` hep 0; `Next(0)` ve `Next(-1)` hata; 6000 çekilişte `Next(6)`'nın her değeri en az 800 kez gelir (kaba eşitlik).
- `IsInside` sınır dışı koordinatlarda `false`, hata yok.

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)

```csharp
public enum TileColor { Red, Green, Blue, Yellow, Purple, Orange }

public sealed class Grid
{
    public const int MinSize = 1;
    public const int MaxSize = 16;

    public Grid(int width, int height);

    public int Width { get; }
    public int Height { get; }

    public bool IsInside(int x, int y);
    public TileColor? Get(int x, int y);           // null = boş hücre
    public void Set(int x, int y, TileColor? color);
}

public interface IRandomSource
{
    int Next(int maxExclusive);
}

public sealed class SeededRandom : IRandomSource
{
    public SeededRandom(int seed);
    public int Next(int maxExclusive);
}

public static class GridFactory
{
    public const int MaxColorCount = 6;
    public static Grid CreateRandom(int width, int height, int colorCount, IRandomSource random);
}
```

Dosya başına bir tip: `TileColor.cs`, `Grid.cs`, `IRandomSource.cs`, `SeededRandom.cs`, `GridFactory.cs`.

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi.

- **Başlatma kodla:** `GameBootstrap` statik bir `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` metoduyla oyun açılınca kendini oluşturur. Sahne dosyasına (`Main.unity`) dokunulmaz.
- **Bu adımın ayarları** (`GameBootstrap` içinde sabit): genişlik 8, yükseklik 10, 5 renk, seed 42. Level dosyası Adım 5'te gelir.
- **Kareler:** her hücre bir `SpriteRenderer`; sprite kodda üretilen 1x1 beyaz dokudan yapılır ve renkle boyanır (görsel dosya yok). Hücre aralığı 1 birim, kare boyu 0.9 (aralarında ince boşluk).
- **Renkler:** `Red #E74C3C`, `Green #2ECC71`, `Blue #3498DB`, `Yellow #F1C40F`, `Purple #9B59B6`, `Orange #E67E22`; arka plan `#1E1E2E`.
- **Kamera:** sahnedeki kamera (yoksa kodla oluşturulur) ortografik yapılır; grid ekranın ortasına, hem genişlikte hem yükseklikte sığacak şekilde ayarlanır (kenarlarda en az yarım hücre boşluk). Dikey de yatay da ekranda grid tamamen görünür; WebGL'de ekran yönü kilitlenemediği için yön kilidi yerine bu kullanılır. Ekran boyutu değişince (telefonu çevirme) yeniden sığdırılır.
- Görüntü katmanı kural koymaz: grid'i `GridFactory`'den alır, yalnız çizer.

## Temizlik
- `Assets/Scripts/Core/AssemblyMarker.cs` (+ `.meta`) kodcu tarafından, `tests/Core.Tests/SmokeTests.cs` testçi tarafından silinir. Bu, mevcut bir testin silinmesidir; bu spec'in onayı o iznin kendisidir.
- `IsExternalInit` eklenmez (bu adımda `record`/`init` gerekmiyor).

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play: 8 sütun x 10 satır renkli kare, 5 renk, ekranın ortasında, arada ince boşluk, koyu arka plan.
3. Play'i durdurup yeniden başlatınca aynı dizilim (seed 42).
4. Game penceresini yatay ve dikey oranlara getirince grid tamamen görünür kalır.
5. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone Safari'de dikey ve yatay açar, grid tamamen görünür.

## Adımın akışı (PLAN bölüm 4 ve 9)
1. Spec onayı → `docs(spec): add grid spec` commit'i.
2. Kodcu (Sonnet 5.5 medium): stub'lar → orchestrator sözleşmeyle karşılaştırır → `feat(core): add grid stubs`.
3. Testçi (Opus 5.5): testler, kırmızı → `test(core): add grid tests`.
4. Kodcu: uygulama + görüntü katmanı, testler yeşil → `feat(core): implement grid model`, `feat(view): render grid from code`.
5. Kontrolcüler (Claude + Gemini) paralel; bulgular kapanır.
6. WebGL önizlemesi `gh-pages`'e yayımlanır; ürün sahibi Editor'de ve telefonda dener.
7. Tarama, push, PR, CI yeşil, merge.
