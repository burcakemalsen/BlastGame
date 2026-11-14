# Spec 05: JSON level

Durum: onaylandı (2026-11-14). Dal: `step/05-json-level`.

## Amaç
Level ayarları koddan dosyaya taşınır. Her level bir JSON dosyasıdır: boyut, renk sayısı, hamle, hedefler. Dosyayı okuyup doğrulayan kod kural katmanındadır (`LevelParser`), böylece bozuk dosya davranışı otomatik testlenir. Oyun level'ları sırayla oynatır.

JSON kütüphanesi: **Newtonsoft.Json** (ürün sahibi kararı, 2026-11-14; PLAN bölüm 5 notu ve bölüm 6 kapanır). Unity tarafında resmi paket `com.unity.nuget.newtonsoft-json`, test tarafında aynı sürümün NuGet paketi `Newtonsoft.Json`. Dosya, otomatik nesneye çevirme (reflection) yerine `JObject`/`JToken` ile alan alan okunur. Gerekçe: WebGL derlemesinde Unity kullanılmayan kodu budar (stripping); reflection ile çevirme bu budamaya takılabilir, alan alan okuma takılmaz. Hata mesajı da her alan için bizim elimizde olur.

## Bitti tanımı (PLAN, Adım 5)
- Level boyutu, renk sayısı, hamle ve hedef dosyadan okunuyor; bozuk dosya anlaşılır hata veriyor; en az 3 level.
- `dotnet test` ve CI yeşil; Editor'de ve iPhone'da (WebGL) üç level sırayla oynanıyor.

## Dosya biçimi
```json
{
  "width": 9,
  "height": 9,
  "colors": 4,
  "moves": 20,
  "goals": [
    { "color": "Red", "count": 20 },
    { "color": "Blue", "count": 20 }
  ]
}
```
- Kök bir nesnedir. Beş alanın beşi de zorunludur; başka alan olamaz (yazım hatası sessizce geçmesin diye).
- `width`, `height`: tam sayı, `Grid.MinSize..Grid.MaxSize` (1..16).
- `colors`: tam sayı, `1..GridFactory.MaxColorCount` (1..6). Oyunda ilk `colors` renk çıkar (`TileColor` sırasıyla).
- `moves`: tam sayı, en az 1.
- `goals`: dizi, en az bir eleman. Her eleman yalnız `color` ve `count` alanlı bir nesnedir.
  - `color`: `TileColor` adlarından biri, büyük/küçük harf duyarlı (`"Red"` geçerli, `"red"` değil). Sayı olarak renk (`0`) geçersizdir.
  - `count`: tam sayı, en az 1.
  - Renk `colors` içinde olmalı (indeksi `< colors`); aynı renk iki kez olamaz.
- "Tam sayı" JSON'da ondalıksız sayıdır: `9.0`, `9.5`, `"9"`, `true`, `null` geçersizdir; `int` aralığını aşan sayı da geçersizdir.
- Aynı anahtar bir nesnede iki kez geçerse dosya geçersizdir.
- Yorum satırı ve sondaki virgül kütüphanenin varsayılan davranışına bırakılır; test edilmez.

## Kurallar (`LevelParser.Parse`)
- `json` `null` → `ArgumentNullException` (`ParamName` `json`).
- Diğer her bozukluk → `LevelFormatException`. Mesaj İngilizce ve tek cümlelik, sorunu söyler:
  - JSON sözdizimi hatası (boş metin, yalnız boşluk, kapanmamış parantez, kökten sonra fazladan metin, tekrarlanan anahtar): mesaj kütüphanenin satır/konum bilgisini içerir; kütüphanenin hatası `InnerException` olarak saklanır.
  - Alan hatası: mesaj alanın yolunu tek tırnak içinde içerir. Yol örnekleri: `'moves'`, `'goals'`, `'goals[1].color'`, `'goals[0].count'` (dizi indeksi 0'dan). Örnek mesajlar: `Missing field 'moves'.`, `Field 'width' must be an integer.`, `Field 'width' must be between 1 and 16, got 17.`, `Unknown field 'move'.`, `Field 'goals[1].color' has unknown color 'red'.`, `Goal color 'Purple' at 'goals[0].color' is not among the first 4 colors.`, `Goal color 'Red' at 'goals[1].color' is given twice.`
  - Testler mesajın tamamını değil, alan yolunu (`'goals[1].color'` gibi) içerdiğini sınar.
- **Denetim sırası** (birden fazla hata varsa ilki bildirilir): sözdizimi → kök nesne mi → bilinmeyen kök alanı (dosyadaki sırayla ilki) → `width`, `height`, `colors`, `moves`, `goals` sırasıyla her biri için önce var mı, sonra tür, sonra aralık → `goals` elemanları sırayla; her elemanda nesne mi, bilinmeyen alan, `color` (var mı, metin mi, bilinen ad mı, `colors` içinde mi, tekrar mı), `count` (var mı, tam sayı mı, en az 1 mi).
- Başarılı okuma `LevelData` döndürür; `Goals` dosyadaki sırayla.

## `LevelData`
- Değişmez nesne: `Width`, `Height`, `ColorCount`, `Moves`, `Goals`. Kurucu aynı kuralları denetler (dosyasız da kullanılabilsin): aralık dışı sayı → `ArgumentOutOfRangeException`, `goals` `null` → `ArgumentNullException`, boş / `colors` dışı renk / tekrarlanan renk / `Count < 1` → `ArgumentException` (`ParamName` parametrenin adı; sıra parametre sırası). Dışarıdaki liste sonradan değişirse `LevelData` etkilenmez.
- `CreateSession(random)`: `GridFactory.CreateRandom(Width, Height, ColorCount, random)` ile grid kurar, aynı `random` ile `new GameSession(grid, ColorCount, Moves, Goals, random)` döndürür. `random` `null` → `ArgumentNullException`. Aynı seed iki kez → aynı grid.

## Örnekler
1. Yukarıdaki dosya → `Width 9, Height 9, ColorCount 4, Moves 20, Goals [Red x20, Blue x20]`.
2. `moves` yok → `LevelFormatException`, mesajda `'moves'`.
3. `"width": 17` → mesajda `'width'`. `"width": 0` → aynı.
4. `"colors": 3` ve hedef `Blue` → geçerli (Blue indeksi 2). Hedef `Yellow` → mesajda `'goals[0].color'`.
5. `{ "color": "Red", "count": 5, "extra": 1 }` ilk hedefte → mesajda `'goals[0].extra'`.
6. `[1, 2]` (kök dizi) → `LevelFormatException`.
7. `{ "width": 9, ` (yarım) → `LevelFormatException`, `InnerException` dolu.

## Uç durumlar (testçinin en az bunları kapsaması beklenir)
- `null`, boş metin, yalnız boşluk, kök dizi / sayı / metin, kökten sonra fazladan metin, tekrarlanan anahtar.
- Her zorunlu alanın yokluğu; her sayısal alanda tür hataları (ondalık, metin, `true`, `null`, nesne) en az bir alanda, `int` taşması.
- Aralık sınırları: `width`/`height` 1 ve 16 geçerli, 0 ve 17 geçersiz; `colors` 1 ve 6 geçerli, 0 ve 7 geçersiz; `moves` 1 geçerli, 0 ve negatif geçersiz; `count` 1 geçerli, 0 geçersiz.
- `goals`: dizi değil, boş dizi, eleman nesne değil, `color` yok / sayı / küçük harf / bilinmeyen ad, `colors - 1` indeksli renk (geçerli sınır) ve `colors` indeksli renk (geçersiz), tekrar, ikinci elemandaki hatanın yolunda `[1]` geçmesi.
- Bilinmeyen alan kökte ve hedefte.
- Denetim sırası: iki hata birlikte varken ilkinin bildirilmesi (en az iki örnek).
- `LevelData` kurucusu: her hata türü; liste kopyalanıyor.
- `CreateSession`: grid boyutu ve renk aralığı level'a uyuyor, oturumun `MovesLeft`, `Goals`, `ColorCount` değerleri level'dan; aynı seed → aynı grid; `null` kaynak.
- **Gerçek level dosyaları:** `Assets/Resources/Levels/` altındaki her `.json` hatasız okunuyor ve en az 3 dosya var (testçi dosyaları repo kökünü bularak okur; kopya yok).

## Arayüz sözleşmesi (kural katmanı, `Assets/Scripts/Core/`, namespace `BlastGame.Core`)
Mevcut tipler değişmez.

```csharp
public sealed class LevelFormatException : Exception
{
    public LevelFormatException(string message);
    public LevelFormatException(string message, Exception innerException);
}

public sealed class LevelData
{
    public LevelData(int width, int height, int colorCount, int moves, IReadOnlyList<ColorGoal> goals);
    public int Width { get; }
    public int Height { get; }
    public int ColorCount { get; }
    public int Moves { get; }
    public IReadOnlyList<ColorGoal> Goals { get; }   // salt okunur kopya
    public GameSession CreateSession(IRandomSource random);
}

public static class LevelParser
{
    public static LevelData Parse(string json);
}
```

Dosya başına bir tip: `LevelFormatException.cs`, `LevelData.cs`, `LevelParser.cs`. C# 9 sınırı. Newtonsoft yalnız `LevelParser` içinde kullanılır; sözleşmede Newtonsoft tipi görünmez.

## Bakım (kodcudan önce, bakım alt ajanı)
- Unity: `Packages/manifest.json`'a `com.unity.nuget.newtonsoft-json` eklenir.
- `Core.asmdef`: Unity motoruna başvuru kapalı kalır (`noEngineReferences: true`); `overrideReferences: true` ve `precompiledReferences: ["Newtonsoft.Json.dll"]`.
- Test: `tests/Core/Core.csproj`'a Unity paketinin içerdiği Newtonsoft sürümüyle aynı `Newtonsoft.Json` NuGet paketi; lock dosyası güncellenir (CI `--locked-mode`).
- Doğrulama: `dotnet test` yeşil (681), `unity-check` 0.

## Görüntü katmanı (`Assets/Scripts/View/`, namespace `BlastGame.View`)
Otomatik test edilmez; kapısı elle kontrol listesi. Sahne dosyasına dokunulmaz.

- **Dosyalar:** `Assets/Resources/Levels/level01.json`, `level02.json`, `level03.json` (kodcu yazar). Oyun `Resources.LoadAll<TextAsset>("Levels")` ile yükler, ada göre sıralar (ordinal). Resources WebGL'de ek indirme istemez.
- **Level değerleri** (ürün sahibi kapı 6'da ayarlayabilir):

| Dosya | Tahta | Renk | Hamle | Hedef |
|---|---|---|---|---|
| `level01.json` | 9x9 | 3 | 15 | Red x15 |
| `level02.json` | 9x9 | 4 | 20 | Red x20, Blue x20 |
| `level03.json` | 9x9 | 5 | 25 | Green x15, Yellow x15, Purple x10 |

- `GameBootstrap`'taki sabit level değerleri kaldırılır; oturum `LevelData.CreateSession` ile kurulur. Seed Adım 4'teki gibi saatten, köşede görünür.
- **Üst bar:** Adım 4'tekine ek olarak level numarası (`Level 2`). 3 hedef de sığar.
- **Akış:** kazanınca panelde "You win!" ve "Next level" (sonraki level, yeni seed). Kaybedince "Out of moves" ve "Try again" (aynı level, yeni seed). Son level kazanılınca "All levels done!" ve "Play again" (level 1'e döner). İlerleme kaydedilmez (Backlog).
- **Bozuk dosya:** oyun çökmez. Ekranda okunaklı bir hata paneli: `level02.json: <mesaj>`; aynı metin Console'a `Debug.LogError` ile yazılır. Bütün dosyalar oyun başında okunur; biri bozuksa hata paneli çıkar, oyun başlamaz. Hiç level yoksa da aynı panel (`No level files found.`).
- Adım 4'teki gibi dokunuş başına yeni nesne yok; level geçişinde eski tahta nesneleri yeniden kullanılır ya da temizlenir.

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0 ile çıkar.
2. Editor'de Play: `Level 1`, 15 hamle, tek hedef; tahtada 3 renk.
3. Level 1 kazanılınca "Next level" → `Level 2`, 4 renk, iki hedef; kaybedince "Try again" aynı level'ı yeni tahtayla açar.
4. Level 3: 5 renk, üç hedef barda sığıyor; kazanınca "All levels done!", "Play again" level 1'e döner.
5. Bir dosyaya geçici olarak bozukluk koyunca (ör. `moves` silinir) hata paneli dosya adı ve alanla görünür, Console'da aynı mesaj; dosya geri alınır (commit'e girmez).
6. WebGL derlenir, `gh-pages`'e yayımlanır; ürün sahibi iPhone'da 2-4'ü dener.

## Adımın akışı (PLAN bölüm 4 ve 9; basit adım)
1. Spec onayı → `docs(spec): add JSON level spec`.
2. Bakım: Newtonsoft paketleri ve asmdef, `dotnet test` yeşil, `unity-check` 0 → `chore: add Newtonsoft.Json`.
3. Kodcu (Sonnet 5.5 medium): stub'lar → `feat(core): add level loading stubs`.
4. Testçi (Sonnet 5.5 medium): yeni testler, ~80 sınırı; mevcut testlere dokunulmaz. Yeni testler kırmızı, eskiler yeşil. Gerçek level dosyası testi bu aşamada kırmızıdır (dosyalar henüz yok) → `test(core): add level loading tests`.
5. Kodcu: uygulama, level dosyaları, görüntü katmanı; bütün testler yeşil → `feat(core): implement level loading`, `feat(view): load levels from JSON files`.
6. Kontrolcü: Claude (Opus 5.5 medium, bilerek bozma). Gemini yok. Bulgular kapanır.
7. WebGL önizlemesi `gh-pages`'e; ürün sahibi Editor'de ve iPhone'da dener.
8. Tarama, push, PR, CI yeşil, merge.
