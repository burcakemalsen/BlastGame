# Spec 02: Grup bulma ve patlatma

Durum: onaylandı (2026-11-14). Dal: `step/02-flood-fill`.

## Amaç
Dokunulan karenin aynı renkli komşu grubunu bulmak (flood fill), grup 2 ve üstüyse patlatmak (hücreleri boşaltmak). Görüntüde dokunulan grup kaybolur. Bu adımda düşme ve doldurma yok (Adım 3); patlayan yer boş kalır.

Aynı adımda **hücre modeli değişir:** grid hücresi `TileColor?` yerine `Cell` değer tipini taşır. Bu bir arayüz sözleşmesi değişikliğidir (aşağıda "Sözleşme değişikliği").

## Bitti tanımı (PLAN, Adım 2)
- Dokunulan karenin aynı renkli komşu grubu bulunuyor; 2 ve üstü patlıyor, tek kare patlamıyor.
- Testler köşe, kenar ve tüm grid tek renk durumlarını kapsıyor; `dotnet test` ve CI yeşil.
- Editor'de ve iPhone'da (WebGL) dokununca grup kayboluyor.

## Sözleşme değişikliği: `Cell`

### Neden
Faz 2'de hücrede renkli kare dışında şeyler de olacak (özel öğe Adım 6, engel Adım 7). `TileColor?` yalnız "renk ya da boş" diyebiliyor. `Cell`, "hücrede ne var" sorusunun tek cevabı olur; yeni tür eklenince `Grid`'in imzası bir daha değişmez. Değişikliği şimdi yapmak ucuz: `Grid`'i kullanan kod ve test az.

### Davranış
- `Cell` bir **değer tipi**dir (`readonly struct`): kopyalanınca bağımsız kopya olur, iki `Cell` içerikleri aynıysa eşittir.
- `CellKind`: bu adımda yalnız `Empty` ve `Color`. `Special` ve `Obstacle` tasarımlarıyla birlikte Adım 6 ve 7'de eklenir; `Cell`'in yapısı bunun için `Grid` imzasını değiştirmeyi gerektirmez.
- `Cell.Empty` ve `default(Cell)` aynıdır: `Kind == Empty`.
- `Cell.OfColor(color)`: `Kind == Color`, `Color == color`. Tanımsız renk (ör. `(TileColor)6`, `99`, `-1`) → `ArgumentOutOfRangeException`. (Adım 1'de bu kontrol `Grid.Set`'teydi; şimdi buraya taşınır.)
- `Color` özelliği yalnız `Kind == Color` iken okunur; başka türde → `InvalidOperationException`.
- Eşitlik: aynı tür ve (renkli ise) aynı renk. `Equals(Cell)`, `Equals(object)`, `GetHashCode`, `==`, `!=` tutarlıdır. Dikkat: `Cell.Empty` hiçbir renkli hücreye eşit değildir, `OfColor(Red)` dahil (renk alanının varsayılan değeri `Red` olsa bile).
- `ToString()`: boş için `"Empty"`, renkli için `"Color(Red)"` biçimi (test hata mesajları okunur olsun diye).

### `Grid` ve `GridFactory`'deki etkisi
- `Grid.Get(x, y)` artık `Cell` döndürür; yeni grid'in bütün hücreleri `Cell.Empty`.
- `Grid.Set(x, y, cell)` artık `Cell` alır. Grid dışı yine `ArgumentOutOfRangeException`. `Cell` yalnız geçerli yollarla kurulabildiği için `Set`'te ayrıca renk kontrolü yok.
- `GridFactory.CreateRandom` davranışı aynen kalır, yalnız hücreye `Cell.OfColor((TileColor)value)` yazar.
- Adım 1'in geri kalan bütün kuralları (boyut, koordinat, `IsInside`, seed'li kaynak, doldurma sırası, hata türleri) değişmez.

### Mevcut testler
Bu spec'in onayı, sözleşmenin gerektirdiği test güncellemelerinin iznidir (RULES, Onay). Testçi yapar, kendi commit'ine girer:
- `TileColor?` ile yazılmış `Get`/`Set` testleri `Cell` ile yeniden yazılır; test sayısı ve kapsamı azalmaz.
- "`Set` tanımsız rengi reddeder" testleri "`Cell.OfColor` tanımsız rengi reddeder" olur.
- `null` = boş hücre yerine `Cell.Empty` kullanılır.
Bunun dışında mevcut bir test silinmez, anlamı değişmez.

## Grup bulma

- **Komşuluk 4 yönlüdür:** yukarı, aşağı, sol, sağ. Çapraz komşu sayılmaz.
- `GroupFinder.FindGroup(grid, x, y)`: `(x, y)` hücresiyle aynı renkte olan ve ona aynı renkli hücreler üzerinden 4 yönlü komşulukla bağlanan bütün hücreler, başlangıç hücresi dahil.
- **Sıra sabittir:** önce `y` küçükten büyüğe, aynı `y`'de `x` küçükten büyüğe (doldurma sırasıyla aynı). Her hücre bir kez geçer.
- Başlangıç hücresi `Empty` ise sonuç boş liste (hata değil: boş yere dokunmak "hiçbir şey olmaz" demektir).
- Tek başına kalan renkli hücre için sonuç yalnız o hücreyi içeren 1 elemanlı liste.
- `FindGroup` grid'i değiştirmez.
- `grid` `null` → `ArgumentNullException`. `(x, y)` grid dışı → `ArgumentOutOfRangeException`. (Kontrol sırası: önce `null`, sonra koordinat.)
- Algoritma serbest; 16x16 tek renk grid'de (256 hücre) de doğru çalışmalı.

## Patlatma

- `BlastRules.MinGroupSize = 2`.
- `BlastRules.Blast(grid, x, y)`: `FindGroup` ile grubu bulur.
  - Grup boyu `≥ MinGroupSize` ise gruptaki her hücreyi `Cell.Empty` yapar ve boşaltılan hücreleri `FindGroup`'un sırasıyla döndürür.
  - Değilse (tek kare ya da boş hücre) grid'e dokunmaz, boş liste döndürür.
- Gruba girmeyen hücreler hiçbir durumda değişmez.
- Hatalar `FindGroup`'unkilerle aynı; hata durumunda grid değişmez.
- Patlayan grubun rengini isteyen kod (Adım 4'te hedef sayacı gibi) `Blast`'tan önce `grid.Get(x, y)` ile okur.

## Örnekler

Grid'ler üst satır yukarıda olacak şekilde yazıldı (`y` alttan üste). `R` = Red, `G` = Green, `B` = Blue, `.` = boş.

**Örnek 1** (3x3):
```
y=2  R G G
y=1  R R G
y=0  B R G
     x=0 1 2
```
- `FindGroup(1, 0)` → `(1,0), (0,1), (1,1), (0,2)`. `(1,0)`'dan `(1,1)`'e, oradan `(0,1)`'e, oradan `(0,2)`'ye gidilir.
- `FindGroup(2, 0)` → `(2,0), (2,1), (1,2), (2,2)`.
- `FindGroup(0, 0)` → `(0,0)` (tek).
- `Blast(0, 0)` → boş liste, grid aynı.
- `Blast(1, 1)` → `(1,0), (0,1), (1,1), (0,2)`; sonra grid:
```
y=2  . G G
y=1  . . G
y=0  B . G
```

**Örnek 2, çapraz bağlamaz** (2x2):
```
y=1  G R
y=0  R G
```
- `FindGroup(0, 0)` → yalnız `(0,0)`; `Blast(0, 0)` → boş liste.

**Örnek 3:** 16x16, `colorCount = 1` (hepsi Red). `FindGroup(7, 7)` → 256 hücre, sırası `(0,0), (1,0), ..., (15,0), (0,1), ..., (15,15)`. `Blast(0, 0)` → 256 hücre, grid tamamen boş.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Başlangıç hücresi dört köşede ve dört kenarın ortasında (komşu ararken grid dışına taşmamalı).
- 1x1 grid: `FindGroup` 1 eleman, `Blast` boş liste. 1xN ve Nx1 şeritler (tek sütun / tek satır boyunca grup).
- Tüm grid tek renk (en küçük ve 16x16): grup bütün grid.
- Aynı renkte ama birbirine bağlı olmayan iki bölge: yalnız dokunulanın bölgesi döner.
- Başka renkle çevrili tek kare; çaprazda aynı renk olan kare.
- Kıvrımlı yol: U ya da spiral biçimli grup (yalnız düz çizgi değil); içinde başka renk olan halka.
- Boş hücre başlangıç: `FindGroup` boş liste, `Blast` boş liste.
- Boş hücreler grubu böler: aynı renkli iki kare arasında boş hücre varsa bağlı değildir.
- Tam 2'lik grup patlar; 1'lik patlamaz.
- `Blast` sonrası aynı hücreye ikinci `Blast`: boş liste, grid değişmez.
- `null` grid; grid dışı koordinatlar (`-1`, `Width`, `Height`); hata sonrası grid değişmemiş.
- `FindGroup` çağrısından sonra grid hücre hücre aynı.
- Sonuçta tekrar eden hücre yok; sıra spec'teki gibi.
- `Cell`: `Empty == default`; `OfColor(Red) != Empty`; aynı renk eşit, farklı renk eşit değil; `==`/`!=`/`Equals`/`GetHashCode` tutarlı; `Empty.Color` → `InvalidOperationException`; tanımsız renk (6, 99, -1) → `ArgumentOutOfRangeException`; altı tanımlı rengin hepsi kabul.

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)

Değişen ve yeni tipler; `TileColor`, `IRandomSource`, `SeededRandom` aynen kalır.

```csharp
public enum CellKind { Empty, Color }   // Empty = 0

public readonly struct Cell : IEquatable<Cell>
{
    public static readonly Cell Empty;               // == default(Cell)
    public static Cell OfColor(TileColor color);

    public CellKind Kind { get; }
    public bool IsEmpty { get; }                     // Kind == CellKind.Empty
    public TileColor Color { get; }                  // Kind != Color ise InvalidOperationException

    public bool Equals(Cell other);
    public override bool Equals(object obj);
    public override int GetHashCode();
    public override string ToString();
    public static bool operator ==(Cell a, Cell b);
    public static bool operator !=(Cell a, Cell b);
}

public readonly struct CellPos : IEquatable<CellPos>
{
    public CellPos(int x, int y);
    public int X { get; }
    public int Y { get; }

    public bool Equals(CellPos other);
    public override bool Equals(object obj);
    public override int GetHashCode();
    public override string ToString();               // "(x, y)"
    public static bool operator ==(CellPos a, CellPos b);
    public static bool operator !=(CellPos a, CellPos b);
}

public sealed class Grid                             // değişen iki metot
{
    public Cell Get(int x, int y);
    public void Set(int x, int y, Cell cell);
    // MinSize, MaxSize, ctor, Width, Height, IsInside aynen
}

public static class GroupFinder
{
    public static IReadOnlyList<CellPos> FindGroup(Grid grid, int x, int y);
}

public static class BlastRules
{
    public const int MinGroupSize = 2;
    public static IReadOnlyList<CellPos> Blast(Grid grid, int x, int y);
}
```

Dosya başına bir tip: `CellKind.cs`, `Cell.cs`, `CellPos.cs`, `GroupFinder.cs`, `BlastRules.cs` (yeni); `Grid.cs`, `GridFactory.cs` (değişir). C# 9 sınırı: `record` ve `init` yok.

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi. Sahne dosyasına dokunulmaz; ayarlar (8x10, 5 renk, seed 42) aynen.

- **Girdi:** Proje eski Input Manager'ı kullanıyor (`activeInputHandler: 0`); yeni Input System paketi eklenmez. Her karede: `Input.touchCount > 0` ise yalnız `TouchPhase.Began` olan dokunmalar; değilse `Input.GetMouseButtonDown(0)`. Böylece telefonda dokunma, Unity'nin ondan ürettiği fare tıklamasıyla iki kez işlenmez.
- **Ekran → hücre:** dokunulan nokta kameranın `ScreenToWorldPoint`'iyle dünya koordinatına çevrilir; hücre `(RoundToInt(wx), RoundToInt(wy))`. Kareler arasındaki ince boşluğa dokunmak en yakın kareyi seçer. `IsInside` değilse hiçbir şey olmaz.
- **Patlatma:** hücre bulununca `BlastRules.Blast` çağrılır. Görüntü kural koymaz: grup boyu ya da komşuluk kontrolü yapmaz.
- **Güncelleme:** `GridView` her hücre için bir kare nesnesi tutar ve kural katmanının grid'ini okuyarak günceller (`Refresh`): `Empty` hücrenin karesi gizlenir, renkli hücreninki o renkte gösterilir. `Refresh` yeni doku ya da nesne oluşturmaz. Görüntü grid'in kopyasını tutmaz.
- Bu adımda animasyon, ses, skor yok. Patlayan yer boş kalır; grid boşalınca yeniden başlatmak için Play'i durdurup başlatmak (telefonda sayfayı yenilemek) yeterli.

## Temizlik
- `TileColor?` imzaları kalkar; kural ve görüntü katmanında `Get`/`Set` kullanan her yer `Cell`'e geçer.
- Backlog'daki "sprite cache" maddesi bu adımda kısmen karşılanır (`Refresh` doku üretmez); madde Adım 3 için kalır.

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play: grid Adım 1'deki gibi aynı dizilimle görünür (seed 42 değişmedi).
3. Aynı renkli 2 ve üstü gruba tıklayınca grubun tamamı kaybolur, başka kare değişmez.
4. Tek kareye tıklayınca hiçbir şey olmaz. Çapraz aynı renk gruba katılmaz.
5. Boş hücreye, kareler arasındaki boşluğa ve grid dışına tıklamak hata vermez (Console temiz).
6. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone Safari'de dokunarak 3-5'i dener. Dokunma sayfayı yakınlaştırmaz ya da kaydırmaz, tek dokunuş tek patlatmadır.

## Adımın akışı (PLAN bölüm 4 ve 9)
1. Spec onayı → `docs(spec): add flood fill spec`.
2. Kodcu (Sonnet 5.5 medium): yeni tiplerin stub'ları; `Grid` ve `GridFactory` yeni imzaya geçer (içleri `Cell` ile yazılır, `Cell` stub olduğu için çalışmaz) → `feat(core): add cell model and flood fill stubs`. Bu commit'ten sonra test projesi derlenmez, çünkü eski testler eski imzayı kullanıyor; bir sonraki commit bunu kapatır. Orchestrator bu aşamada `dotnet build tests/Core/Core.csproj` (yalnız kural katmanı) ve `scripts/unity-check.ps1` ile derlemeyi kontrol eder.
3. Testçi (Sonnet 5.5 medium): mevcut testlerin `Cell`'e geçişi + yeni testler. Test projesi derlenir; `Cell`'e dokunan bütün testler (eski grid testleri dahil) kırmızı, `Cell`'e dokunmayanlar (ör. `SeededRandom`) yeşil olabilir → `test(core): add flood fill tests`.
4. Kodcu: uygulama + görüntü katmanı, bütün testler yeşil → `feat(core): implement flood fill and blast`, `feat(view): blast tapped group`.
5. Kontrolcü (Claude, Sonnet 5.5); Gemini bu adımda yok (basit adım). Bulgular kapanır.
6. WebGL önizlemesi `gh-pages`'e; ürün sahibi Editor'de ve iPhone'da dener.
7. Tarama, push, PR, CI yeşil, merge.
