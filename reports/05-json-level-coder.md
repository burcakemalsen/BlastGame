# Adım 5 kodcu raporu

## Aşama A

**Yazılan dosyalar** (`Assets/Scripts/Core/`, Unity `.meta` dosyalarıyla): `LevelFormatException.cs` (iki kurucu tam, yalnız `base(...)`), `LevelData.cs` (kurucu, 5 özellik ve `CreateSession` gövdeleri `NotImplementedException`), `LevelParser.cs` (`Parse` stub). İmzalar spec "Arayüz sözleşmesi" ile birebir. Mevcut dosyalara ve `tests/`'e dokunulmadı.

**Kontroller**
- `dotnet build tests/BlastGame.Tests.slnx` → `0 Uyarı`, `0 Hata`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.` (çıkış 0)

**Spec'te belirsiz bulduklarım**
- `LevelData` kurucusunda birden fazla hata (ör. hem `width` aralık dışı hem `goals` null) için denetim sırası "parametre sırası" diye geçiyor; Aşama B'de soldan sağa denetleyeceğim.
- `colors` dışı renk hatasının `ArgumentException` mi `ArgumentOutOfRangeException` mı olduğu: spec `ArgumentException` diyor, onu uygulayacağım.
- `int` aralığını aşan sayıda mesajın "must be an integer" mı "between" mi dediği belirtilmemiş; "must be an integer" seçeceğim.

## Aşama B

**Değişen/yeni dosyalar:** `Assets/Scripts/Core/LevelParser.cs`, `LevelData.cs` (uygulama); `Assets/Resources/Levels/level01..03.json` (spec tablosu); `Assets/Scripts/View/GameBootstrap.cs` (sabitler kalktı; `Resources.LoadAll<TextAsset>("Levels")`, ordinal sıralama, hepsi başta okunur, `StartLevel`, panel akışı, hata paneli + `Debug.LogError`), `HudView.cs` (`SetLevel`, `ShowResult(title, buttonText)`, `ShowError`; üst barda `Level N` + hamle iki satır). Sahneye ve `tests/`'e dokunulmadı. `.meta`'lar Unity üretti.

**`dotnet test tests/BlastGame.Tests.slnx`:** `Başarısız! - Başarısız: 1, Başarılı: 757, Atlanan: 0, Toplam: 758`

**`unity-check.ps1`:** `Unity compile check: clean.` (çıkış 0)

**Kalan 1 test: testin hatası, kodu bükmedim.**
`LevelDataTests.Constructor_InvalidGoals_ThrowsArgumentExceptionWithGoalsParamName`, `BadGoalLists()` içinde `new ColorGoal(TileColor.Red, 0)` (tests/Core.Tests/LevelDataTests.cs:66) veri üretilirken `ColorGoal` kurucusunda `ArgumentOutOfRangeException` ('count') atıyor; test gövdesine hiç girmiyor. Mevcut `ColorGoal` kurucusu `count < 1`'i reddeder (Adım 4, değişmez). Spec'in "`Count < 1` → `ArgumentException`" maddesi yalnız `default(ColorGoal)` ile ulaşılabilir; `LevelData` bunu zaten denetliyor. Testçi bu satırı `default(ColorGoal)` ile değiştirmeli; benim yapabileceğim bir şey yok.

**Seçimler:** kurucuda `colors` dışı/tekrar renk `ParamName` `goals`; `goals[i]` nesne değilse yol `'goals[0]'`; JSON `null` alan değeri ve `int` taşması "must be an integer"; kök `null`/nesne olmayan kök `LevelFormatException`; tarih benzeri metin tarih olarak çevrilmesin diye `DateParseHandling.None`.

**Doğrulayamadıklarım:** görüntü katmanı Editor'de çalıştırılmadı (yalnız derleme kontrolü); elle kontrol listesi 2-6 ürün sahibinde. Hata panelinin 44 punto sarmalı uzun mesajda gerçek ekranda kontrol edilmeli.

**Öneri:** bozuk dosya için hata panelinde "Retry" yok; Backlog'a yazılabilir.

## Kontrol sonrası

- **Bulgu 1 (`HudView.cs`):** hedef öğesi 190→164 birim, simge 64→56, yazı 52→44, öğe içi kenar boşluğu 80→64, aralık 10→6. Üç hedef 3×164+2×6 = 504 birim; iPhone oranında kalan ~538'e sığar. İki ve tek hedefte sağa hizalı kalır, `Moves` alanına (%42'ye kadar) girmez. Gerçek ekranda görmedim; ürün sahibi kontrol listesinde bakmalı.
- **Bulgu 2 (`LevelParser.cs`):** kökten sonra fazladan değer hatası artık `JsonReaderException(message, path, LineNumber, LinePosition, null)` ile kuruluyor; Newtonsoft mesaja satır/konumu ekler ve `InnerException` olarak saklanır. Mesajı çalıştırıp görmedim, kütüphanenin bu kurucu davranışına dayanıyorum.
- `dotnet test`: `Başarılı! - Başarısız: 0, Başarılı: 766, Toplam: 766`. `unity-check`: `Unity compile check: clean.` (0).
