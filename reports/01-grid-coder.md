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

## Aşama B (uygulama)

### Değişen dosyalar
- `Assets/Scripts/Core/`: `Grid.cs`, `SeededRandom.cs` (SplitMix64, başlangıç `(ulong)(long)seed`), `GridFactory.cs` uygulandı. `AssemblyMarker.cs` ve `.meta` silindi.
- `Assets/Scripts/View/`: yeni `GameBootstrap.cs`, `GridView.cs`, `TileColors.cs`, `CameraFitter.cs`; `.gitkeep` silindi. Sahne dosyasına dokunulmadı. `tests/`'e dokunulmadı.
- Uygulama notları: `CreateRandom` bütün doğrulamayı kaynağı çağırmadan önce yapar; `Grid.Set` grid dışında hata verirse grid değişmez; hata, dışarıdaki eksene göre `nameof(x)` ya da `nameof(y)` ile atılır.
- Orchestrator incelemesinden: `[RuntimeInitializeOnLoadMethod]` metodu `Start` yerine `CreateOnLoad` adlandırıldı.
- View dosyalarında `using Grid = BlastGame.Core.Grid;` takma adı var: `UnityEngine.Grid` ile ad çakışması (CS0104) çıkmıştı.

### Kontroller (son kod hâliyle)
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı! - Başarısız: 0, Başarılı: 223, Atlanan: 0, Toplam: 223`
- `scripts/unity-check.ps1`: `Unity compile check: clean.` (exit 0)
- `scripts/build-webgl.ps1`: `WebGL build: C:\Dev\BlastGame\Builds\WebGL  (16.57 MB total)` (exit 0)
- `Logs/build-webgl.log` ve `Logs/unity-check.log` içinde `GameBootstrap`, `warning CS` ya da `error CS` satırı yok.

### Doğrulanmayanlar
- Editor'de Play yapılmadı; görüntü (renkler, kamera sığdırma, yeniden boyutlandırma) elle kontrol listesinde, ürün sahibi deneyecek.
- WebGL çıktısı tarayıcıda açılmadı.

### Öneriler
- Yok.
