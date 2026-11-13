# Step 1 (Grid): coder report

## Aşama A (stub)

### Yazılan dosyalar (`Assets/Scripts/Core/`)
- `TileColor.cs`: enum, `Red..Orange` (0..5).
- `IRandomSource.cs`: arayüz, `Next(int maxExclusive)`.
- `Grid.cs`: `MinSize = 1`, `MaxSize = 16`, kurucu, `Width`, `Height`, `IsInside`, `Get`, `Set`. Hepsinin gövdesi `NotImplementedException`.
- `SeededRandom.cs`: kurucu ve `Next`, gövdeler `NotImplementedException`.
- `GridFactory.cs`: `MaxColorCount = 6`, `CreateRandom`, gövde `NotImplementedException`.
- Unity'nin ürettiği `.meta` dosyaları da var (beş tip için).
- `AssemblyMarker.cs`'e dokunulmadı.

### Kontroller
`dotnet build tests/BlastGame.Tests.slnx`:
```
  Core -> C:\Dev\BlastGame\tests\Core\bin\Debug\netstandard2.1\Core.dll
  Core.Tests -> C:\Dev\BlastGame\tests\Core.Tests\bin\Debug\net10.0\Core.Tests.dll

Oluşturma başarılı oldu.
    0 Uyarı
    0 Hata
```

`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1`:
```
Unity compile check: clean.
exit=0
```

### Spec'te belirsiz bulunanlar
- Belirsizlik yok; sözleşme imzaları birebir uygulandı.
- Not: `tests/Core/Core.csproj` Core kaynaklarını `Assets/Scripts/Core/` altından otomatik topluyor, yeni dosyalar için csproj değişikliği gerekmedi.
