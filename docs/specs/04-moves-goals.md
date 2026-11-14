# Spec 04: Hamle, hedef, kazan/kaybet

Durum: onaylandı (2026-11-14). Dal: `step/04-moves-goals`.

## Amaç
Oyuna bir level çerçevesi gelir: sınırlı hamle ve renk toplama hedefi. Patlatan her dokunuş bir hamle harcar; patlayan kareler kendi renginin hedef sayacını düşürür. Hamle bitmeden bütün hedefler sıfırlanırsa kazanılır, hamle biter ve hedef kalırsa kaybedilir. Bu kuralın tamamı kural katmanında tek bir oyun nesnesindedir (`GameSession`); görüntü yalnız onu çağırır, sayacı ve sonuç ekranını çizer.

Level ayarları bu adımda kodda sabittir; dosyadan okuma Adım 5'te.

## Bitti tanımı (PLAN, Adım 4)
- Hamle sayacı, renk toplama hedefi, kazanma ve kaybetme ekranı.
- `dotnet test` ve CI yeşil; Editor'de ve iPhone'da (WebGL) sayaçlar doğru düşüyor, iki sonuç ekranı da görülüyor, "tekrar oyna" yeni tahtayla başlatıyor.

## Kurallar (`GameSession`)
Mekanik kararlar türün yaygın davranışına göredir.

- **Kurulum:** `GameSession(grid, colorCount, moves, goals, random)`. Grid hazır (dolu) verilir; oturum onu sahiplenir ve dokunuşlarda değiştirir. Başlangıçta `MovesLeft = moves`, her hedefin kalanı `Count`, `Status = Playing`.
- **Hamle:** yalnız patlatan dokunuş (`TurnResult.HasBlasted`) bir hamle harcar. Tek kareye ya da boş hücreye dokunuş hamle harcamaz, sayaçları değiştirmez, kaynağı çağırmaz (Adım 3'teki `TurnRules.Tap` kuralı).
- **Hedef sayımı:** patlayan grubun bütün kareleri aynı renktedir; o renk bir hedefse kalanı patlayan kare sayısı kadar düşer, **0'ın altına inmez**. Hedef olmayan renk patlarsa hiçbir hedef değişmez (hamle yine harcanır). Doldurmayla gelen ve düşen kareler sayılmaz; yalnız `Blasted`.
- **Sonuç, her patlatan dokunuştan sonra, bu sırayla:**
  1. Bütün hedeflerin kalanı 0 → `Won`. Son hamlede tamamlanırsa da kazanılır (kazanma kaybetmeden önce bakılır).
  2. Değilse ve `MovesLeft == 0` → `Lost`.
  3. Değilse `Playing`.
- **Oyun bitince:** `Status` `Won` ya da `Lost` iken `Tap` hiçbir şeyi değiştirmez (grid, hamle, hedef), kaynağı çağırmaz ve üç listesi boş bir `TurnResult` döndürür. Koordinat yine denetlenir (aşağıda).
- **`Tap(x, y)`:** önce koordinat denetlenir (grid dışı → `ArgumentOutOfRangeException`, önce `x` sonra `y`, `ParamName` `x`/`y`); sonra durum `Playing` değilse boş sonuç; değilse `TurnRules.Tap(Grid, x, y, colorCount, random)` çağrılır, sonucu döndürülür ve yukarıdaki sayım/sonuç uygulanır.
- **`Remaining(color)`:** o rengin hedefinde kalan sayı. Renk hedef değilse → `ArgumentException` (`ParamName` `color`).
- **Kaynak hatası:** `TurnRules.Tap` `InvalidOperationException` atarsa (yalnız sahte kaynakla olur) hata dışarı çıkar; hamle ve hedef değişmemiş kalır.

### Kurucu hataları
Hiçbir şeye dokunmadan, bu sırayla; `ParamName` parametrenin adı:
1. `grid` `null` → `ArgumentNullException`.
2. `colorCount` `1..GridFactory.MaxColorCount` dışı → `ArgumentOutOfRangeException`.
3. `moves < 1` → `ArgumentOutOfRangeException`.
4. `goals` `null` → `ArgumentNullException`; boş liste → `ArgumentException`.
5. Bir hedefin rengi `colorCount`'tan büyük ya da eşit (oyunda o renk hiç çıkmaz) → `ArgumentException` (`goals`). Aynı renk iki kez → `ArgumentException` (`goals`). `Count < 1` olan hedef (kurucudan geçmemiş `default(ColorGoal)`) → `ArgumentException` (`goals`).
6. `random` `null` → `ArgumentNullException`.

### `ColorGoal`
Değer tipi: `Color` + `Count`. Kurucu `count < 1` → `ArgumentOutOfRangeException` (`count`). Eşitlik iki alana bakar. `ToString` → `"Red x20"`.

Not: Adım 7'de engel hedefleri gelince hedef türü genişler; bu adımda yalnız renk hedefi var.

`Goals` kurucuya verilen sırayla, başlangıç değerleriyle durur (değişmez); güncel sayı `Remaining`'den okunur. Oturum `goals` listesini kopyalar; dışarıda değiştirilmesi oturumu etkilemez.

## Örnekler
Grid'ler Adım 3'teki gibi (`y` alttan üste). `R` Red, `G` Green, `B` Blue.

**Örnek 1, sayım** (3x3, `colorCount = 3`, `moves = 5`, hedef `Red x2, Blue x5`, sahte kaynak hep `1` (Green)):
```
y=2  B G G
y=1  R R G
y=0  B R B
```
`Tap(1, 0)` → 3 kırmızı patlar. `MovesLeft = 4`, `Remaining(Red) = 0` (2 - 3, 0'da durur), `Remaining(Blue) = 5`, `Playing`.

**Örnek 2, son hamlede kazanma:** `moves = 1`, hedef `Red x3`, Örnek 1'in grid'i. `Tap(1, 0)` → `MovesLeft = 0`, `Remaining(Red) = 0`, `Won` (Lost değil).

**Örnek 3, kaybetme:** `moves = 1`, hedef `Blue x1`, aynı grid. `Tap(1, 0)` (kırmızı) → `MovesLeft = 0`, `Remaining(Blue) = 1`, `Lost`. Sonra `Tap(2, 2)` → boş sonuç, grid aynı, kaynak çağrılmaz.

**Örnek 4, boşa dokunuş:** aynı grid, `Tap(0, 0)` (tek mavi) → `MovesLeft` aynı, sayaçlar aynı, `Playing`, kaynak çağrılmaz.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- Kurucu: her hata ve sırası; `moves = 1`; tek hedef; `colorCount` kadar hedef (her renk); hedef rengi `colorCount - 1` (geçerli sınır) ve `colorCount` (geçersiz); aynı renk iki kez; dışarıdaki `goals` listesi sonradan değişince oturum etkilenmiyor.
- Hamle: patlatan dokunuş 1 düşürür; boşa dokunuş düşürmez; `MovesLeft` hiç negatif olmaz.
- Hedef: tam kadar, fazlasıyla (0'da durur), hedef olmayan renk; iki hedeften yalnız biri bitince `Playing`.
- Sonuç: son hamlede kazanma; son hamlede kaybetme; hamle kalırken kazanma (`MovesLeft > 0` ve `Won`); bittikten sonra `Tap` hiçbir şeyi değiştirmiyor ve kaynağa dokunmuyor (`Won` ve `Lost` ayrı ayrı); bittikten sonra grid dışı koordinat yine hata.
- `Remaining` hedef olmayan renkte hata.
- Tekrarlanabilirlik: aynı seed, aynı dokunuşlar → aynı `MovesLeft`, kalanlar, `Status`.
- `ColorGoal`: `count` 0 ve negatif; eşitlik, `GetHashCode` tutarlılığı, `ToString`.

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)
Mevcut tipler değişmez.

```csharp
public enum GameStatus { Playing, Won, Lost }

public readonly struct ColorGoal : IEquatable<ColorGoal>
{
    public ColorGoal(TileColor color, int count);
    public TileColor Color { get; }
    public int Count { get; }

    public bool Equals(ColorGoal other);
    public override bool Equals(object obj);
    public override int GetHashCode();
    public override string ToString();               // "Red x20"
    public static bool operator ==(ColorGoal a, ColorGoal b);
    public static bool operator !=(ColorGoal a, ColorGoal b);
}

public sealed class GameSession
{
    public GameSession(Grid grid, int colorCount, int moves, IReadOnlyList<ColorGoal> goals, IRandomSource random);
    public Grid Grid { get; }
    public int ColorCount { get; }
    public int MovesLeft { get; }
    public GameStatus Status { get; }
    public IReadOnlyList<ColorGoal> Goals { get; }  // başlangıç hedefleri, verilen sırayla
    public int Remaining(TileColor color);
    public TurnResult Tap(int x, int y);
}
```

Dosya başına bir tip: `GameStatus.cs`, `ColorGoal.cs`, `GameSession.cs`. C# 9 sınırı: `record` ve `init` yok.

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi. Sahne dosyasına dokunulmaz; her şey kodla kurulur.

- **UI paketi:** Unity'nin standart UI paketi (`com.unity.ugui`) eklenir (ürün sahibi onayı, 2026-11-14). Kurulum bakım işidir; kodcudan önce yapılır, `unity-check` ile doğrulanır.
- **Level ayarı (sabit, Adım 5'te dosyaya):** 9x9, 4 renk (önceki adımlarda 5; türde ilk level'lar az renkle başlar, az renk büyük grup demektir; Adım 5'te renk sayısı level başına ayarlanır), 20 hamle, hedef `Red x20, Blue x20`. Değerler `GameBootstrap`'ta adlandırılmış sabitlerdir.
- **Seed:** her başlangıçta ve her "tekrar oyna"da seed saatten seçilir (ürün sahibi kararı, 2026-11-14; Backlog maddesi kapanır). Seed ekranın bir köşesinde küçük yazıyla görünür ve Console'a yazılır; böylece bir hata aynı tahtayla tekrar kurulabilir. Kural katmanı seed'i yine dışarıdan alır. Bir `SeededRandom` grid'i üretir ve aynı nesne o oyunun bütün doldurmalarında kullanılır (Adım 3'teki gibi).
- **Üst bar:** tahtanın üstünde kalan hamle sayısı ve her hedef için renkli kare + kalan sayı. Hedef 0 olunca tamamlandığı anlaşılır (ör. sayı yerine tik ya da soluk görünüm; seçim kodcunun, elle kontrolde ürün sahibi bakar). Tahta ve bar dikey telefon ekranına birlikte sığar.
- **Dokunuş:** `GameSession.Tap` çağrılır; sonuç Adım 3 Ek A'daki gibi canlandırılır. Sayaçlar dokunuştan hemen sonra güncellenir.
- **Sonuç ekranı:** `Status` `Won` ya da `Lost` olunca, animasyon bittikten sonra tahtanın üstüne yarı saydam bir panel gelir: "You win!" ya da "Out of moves", altında "Play again" butonu. Panel açıkken tahtaya dokunuş geçmez. Butona basınca yeni seed'le yeni grid ve yeni `GameSession` kurulur, sayaçlar sıfırlanır, panel kapanır. Metinler İngilizce (public portföy).
- Yeni doku ya da kalıcı nesne dokunuş başına oluşturulmaz (Adım 3'teki gibi); yeniden başlatmada eski tahta nesneleri yeniden kullanılır ya da temizlenir, sızıntı olmaz.

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play: üst bar 20 hamle ve iki hedef gösterir; tahta ve bar ekrana sığar; köşede seed görünür.
3. Patlatan dokunuş hamleyi 1 düşürür, patlayan kırmızı/mavi sayısı kadar hedefi düşürür; tek kareye dokunuş hiçbir sayacı değiştirmez.
4. Hedefler bitince "You win!" paneli; hamle bitince "Out of moves" paneli. Panel açıkken tahtaya dokunmak bir şey yapmaz.
5. "Play again" yeni bir tahta (farklı seed) ve dolu sayaçlarla başlatır; birkaç kez tekrarlayınca Console temiz.
6. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone Safari'de 2-5'i dener (dikey ekranda bar ve panel okunaklı, buton basılıyor).

## Adımın akışı (PLAN bölüm 4 ve 9; basit adım)
1. Spec onayı → `docs(spec): add moves and goals spec`.
2. Bakım: `com.unity.ugui` paketi eklenir, `unity-check` 0 → `chore: add Unity UI package`.
3. Kodcu (Sonnet 5.5 medium): stub'lar → `feat(core): add moves and goals stubs`. Orchestrator `dotnet build` ve `unity-check`'e bakar.
4. Testçi (Sonnet 5.5 medium): yeni testler, ~80 sınırı; mevcut testlere dokunulmaz. Yeni testler kırmızı, eskiler yeşil → `test(core): add moves and goals tests`.
5. Kodcu: uygulama + görüntü katmanı, bütün testler yeşil → `feat(core): implement moves and goals`, `feat(view): show moves, goals and result panel`.
6. Kontrolcü: Claude (Opus 5.5 medium, bilerek bozma; ürün sahibi kararı: orchestrator ajanın işini yeniden okumaz, güvenilecek kontrol güçlü modelde). Gemini yok (basit adım). Bulgular kapanır.
7. WebGL önizlemesi `gh-pages`'e; ürün sahibi Editor'de ve iPhone'da dener.
8. Tarama, push, PR, CI yeşil, merge.
