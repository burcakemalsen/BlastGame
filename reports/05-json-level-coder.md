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
