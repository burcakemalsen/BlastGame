# Rapor: Adım 5 JSON level, testçi

## Dosyalar (yalnız `tests/Core.Tests/`, 77 yeni test; mevcut testlere dokunulmadı)
- `LevelParserTests.cs`: 59 (Theory satırları dahil)
- `LevelDataTests.cs`: 16
- `LevelFilesTests.cs`: 2

## Spec maddesi → test
- Örnek 1 → `Parse_ValidFile_ReturnsAllValuesInFileOrder`
- Örnek 2 + her zorunlu alanın yokluğu → `Parse_MissingField_MessageContainsFieldPath` (5 alan)
- Örnek 3, aralık sınırları → `Parse_NumberOutOfRange_*` (width/height/colors/moves geçersiz uçlar), `Parse_BoundaryValues_AreAccepted`; `count` 0 → `Parse_BadGoals_*`; `count` 1 geçerli → Örnek 1 ve diğer geçerli testler
- Tür hataları, `int` taşması → `Parse_WidthWrongTypeOrOverflow_*` (ondalık, metin, true, null, nesne, taşma), `Parse_OtherNumericFieldWithDecimal_*`; `count` ondalık → `Parse_BadGoals_*`
- Örnek 4 → `Parse_GoalColorAtLastIndex_IsAccepted`, `Parse_GoalColorBeyondColors_*`
- Örnek 5, bilinmeyen alan (hedef) → `Parse_BadGoals_*` (`'goals[0].extra'`); kök → `Parse_UnknownRootField_*`
- Örnek 6, kök sayı/metin → `Parse_RootIsNotObject_*`
- Örnek 7, boş, boşluk, fazladan metin, tekrarlanan anahtar (kökte ve hedefte) → `Parse_SyntaxError_*` (InnerException dolu)
- `null` → `Parse_NullJson_*` (ParamName `json`)
- goals dizi değil / boş / eleman nesne değil / color yok-sayı-küçük harf-bilinmeyen / tekrar / `[1]` yolu → `Parse_BadGoals_*`
- Denetim sırası (4 örnek) → `Parse_WidthAndMovesBothInvalid_*`, `Parse_UnknownRootFieldAndBadWidth_*`, `Parse_MissingMovesAndEmptyGoals_*`, `Parse_BadColorAndBadCountInSameGoal_*`
- `LevelData` kurucusu: aralık + ParamName + sıra → `Constructor_NumberOutOfRange_*`; null → `Constructor_NullGoals_*`; boş/dışı renk/tekrar/Count<1 → `Constructor_InvalidGoals_*`; sınırlar → `Constructor_BoundaryValues_*`, `Constructor_GoalColorAtLastIndex_*`
- Kopya → `Goals_SourceListChangedLater_LevelIsUnaffected`
- `CreateSession` → `CreateSession_UsesLevelValues` (boyut, renk aralığı, MovesLeft, ColorCount, Goals), `_SameSeedTwice_*`, `_NullRandom_*`
- Gerçek dosyalar → `LevelFilesTests` (en az 3 dosya; her biri hatasız)

## `dotnet test tests/BlastGame.Tests.slnx`
```
Başarısız! - Başarısız:    77, Başarılı:   681, Atlanan:     0, Toplam:   758, Süre: 86 ms - Core.Tests.dll (net10.0)
```
681 mevcut test yeşil, 77 yeni testin hepsi kırmızı (stub `NotImplementedException`). Geçen yeni test yok. Gerçek dosya testleri de kırmızı (dosyalar yok), beklenen.

## Spec'te belirsiz bulunanlar (test yazılmadı)
- Sözdizimi mesajının "satır/konum bilgisi" içerdiği: biçimi tanımsız, yalnız `InnerException` dolu mu denendi.
- Kökte JSON `null` değeri (`null` metni): "kök nesne mi" kuralına girer mi, sözdizimi mi; spec'te listede yok.
- Hedef elemanı nesne değilse yol `'goals[0]'` varsayıldı (spec yol örneklerinde yok; "eleman nesne mi" kuralından çıkarıldı).
- `width`/`moves` gibi alanda `null` değeri "eksik" mi "tür hatası" mı sayılır: yol aynı (`'width'`), yalnız yol sınandı.
- `LevelData` kurucusunda `goals` içinde `colorCount` dışı renk için `ParamName` `goals` varsayıldı (spec "parametrenin adı" diyor).
- `Goals` "salt okunur kopya": yalnız kaynak liste değişince etkilenmeme sınandı; dönen listenin türü (örn. `List<T>`'e atılabilmesi) sınanmadı.
- Boyut: ~80 sınırı içinde (77).
