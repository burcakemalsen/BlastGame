# Adım 5 kontrol raporu (Claude kontrolcü)

Commit: `072b82a`. Worktree `step/05-json-level`'a fast-forward ile getirildi (`reset --hard` izni verilmedi; çalışma kopyası temizdi, sonuç aynı).

## Test özeti
`Başarılı!  - Başarısız: 0, Başarılı: 761, Atlanan: 0, Toplam: 761`

## Bulgular
1. **Orta, görüntü → kodcu / elle kontrol 4.** `Assets/Scripts/View/HudView.cs:99`, spec "Üst bar: 3 hedef de sığar". Üç hedef 3×190 + 2×10 = 590 birim istiyor; hedef alanı canvas genişliğinin %55'i. 1080×1920'de 594 (ucu ucuna), iPhone oranında (1170×2532, match 0.5) yaklaşık 538. Hedefler alanın solundan ~50 birim taşıyor (`childControlWidth=false`). Moves yazısıyla çakışıyor mu, ancak cihazda görülür.
2. **Düşük, kod → kodcu.** `Assets/Scripts/Core/LevelParser.cs:68`, spec "sözdizimi hatası: mesaj satır/konum bilgisini içerir". Kökten sonra *geçerli* bir değer gelirse (`{...} {}`) elle kurulan `JsonReaderException` satır/konum içermiyor. `extra` gibi geçersiz metinde kütüphane mesajı geliyor, sorun yok.
3. **Düşük, eksik test → testçi.** Denetim sırası: hedefte bilinmeyen alanın `color`'dan önce bildirildiği test edilmiyor (M4); `width`→`height`→`colors` sırası test edilmiyor (M10).
4. **Düşük, eksik test → testçi.** `long` sınırını aşan sayı (`99999999999999999999`, BigInteger yolu) ve negatif taşma test edilmiyor. Kod okuyarak doğru (`value.Value is long` reddediyor).
5. **Düşük, görüntü.** `HudView.cs:92` `Destroy` kare sonunda çalışıyor; level geçişindeki tek karede eski hedefler de layout'ta. Sızıntı yok.

## Bilerek bozma
| # | Bozma | Sonuç |
|---|---|---|
| M1 | `width` üst sınırı 15 | `Parse_BoundaryValues_AreAccepted` |
| M2 | Kökte bilinmeyen alan denetimi kapalı | `Parse_UnknownRootField…`, `…ReportsUnknownFieldFirst` |
| M3 | Tekrarlanan hedef rengi denetimi kapalı | `Parse_BadGoals…('goals[1].color')` |
| M4 | Hedefte bilinmeyen alan denetimi `count`'tan sonra | **yakalanmadı** |
| M5 | `LevelData` listeyi kopyalamıyor | `Goals_SourceListChangedLater…` |
| M6 | Ondalık sayı kabul | 5 test (`9.0`, `9.5`, `2.5`…) |
| M7 | Hata yolunda indeks +1 | 8 test |
| M8 | Kökten sonraki metin denetimi silindi | `Parse_SyntaxError…("… extra")` |
| M9 | `LevelData` `Count < 1` denetimi silindi | `Constructor_InvalidGoals…[Red x0]` |
| M10 | `height`, `width`'ten önce | **yakalanmadı** |
| M11 | `colors` dışı renk denetimi kapalı | `Parse_GoalColorBeyondColors…` |
| M12 | `color` tür denetimi gevşetildi | yakalanmadı; davranış eşdeğer (`0` "bilinmeyen renk" olarak yine aynı yolla reddediliyor) |

Her bozmadan sonra `git checkout -- Assets/`; son durumda worktree temiz.

## Sorun bulamadığım alanlar
- **Spec karşılaştırması:** beş alan, aralıklar, denetim sırası, mesaj yolları, `ArgumentNullException(json)`, `LevelData` parametre sırası ve kopya, `CreateSession` tek `random` ile. Level dosyaları tabloyla aynı; `TileColor` sırasıyla Blue=2, Purple=4, yani `colors` içinde.
- **Kural 4 / WebGL:** Core'da `UnityEngine`, `System.Random`, `DateTime`, C# 9 üstü sözdizimi aranıp bulunmadı. Newtonsoft yalnız `LevelParser.cs` ve asmdef'te; public imzada yok. `DeserializeObject`/`ToObject` yok. Tekrarlanan anahtar iç içe nesnede de reddediliyor (test var).
- **Test dokunulmazlığı:** `main..HEAD -- tests/` yalnız `test(core)` ve planlı `chore` (csproj + lock). Üç `feat` commit'inde `tests/` boş. `12636b5` diff'i tek satır (`default(ColorGoal)`); `05131fc` yalnız yeni dosya ekliyor.
- **Görüntü (okuyarak):** ordinal sıralama, bozuk/eksik dosyada `Debug.LogError` + panel ve oyun başlamıyor, Next/Try again/All levels done + Play again akışı, eski `GridView` ve hedef nesneleri yok ediliyor.
