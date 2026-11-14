# Adım 4, kodcu raporu

## Aşama A (stub)

**Yazılan dosyalar** (`Assets/Scripts/Core/`):
- `GameStatus.cs`: enum, tam (`Playing`, `Won`, `Lost`).
- `ColorGoal.cs`: `readonly struct`, `IEquatable<ColorGoal>`; iki özel alan (`_color`, `_count`) var, hiçbir üye okumuyor; bütün gövdeler `NotImplementedException`.
- `GameSession.cs`: spec imzaları tam; bütün gövdeler `NotImplementedException`.

Mevcut tiplere, görüntü katmanına ve `tests/`'e dokunulmadı. Commit, dal değişikliği yok.

**Kontroller**
- `dotnet build tests/BlastGame.Tests.slnx` → `Oluşturma başarılı oldu.`, `2 Uyarı`, `0 Hata`. Uyarılar: `CS0169` (`ColorGoal._color`, `ColorGoal._count` hiç kullanılmıyor); stub'ın doğal sonucu, Aşama B'de kalkar.
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.` (çıkış satırını `tail` ile boru içinde okudum; metin temiz derlemeyi gösteriyor, ayrı `$?` doğrulaması yapmadım).

**Spec'te belirsiz bulduklarım**
- `ColorGoal` kurucu hatası: `count < 1` için `ParamName` `count` net. `color` için bir denetim yok (geçersiz enum değeri); `GameSession` kurucusu 5. maddede `colorCount` sınırını zaten denetliyor. Sorun değil, not düştüm.
- `GameSession` kurucusu `goals` içinde `ColorGoal` default değeri (`Count = 0`) ile gelebilir (`default(ColorGoal)`); spec bunu ayrıca ele almıyor. Aşama B'de özel durum yazmayacağım; testçi/orchestrator isterse spec'e eklenir.
- "Doldurmayla gelen kareler sayılmaz; yalnız `Blasted`": `TurnResult` alan adı `Blasted` mi, kodda kontrol edeceğim (Aşama B'de `TurnResult.cs`'i okuyacağım).

## Aşama B (uygulama)

**Değişen dosyalar** (`Assets/`): `Core/ColorGoal.cs`, `Core/GameSession.cs` (uygulandı; spec'teki `Count < 1` ek maddesi dahil), `View/GameBootstrap.cs` (yeniden yazıldı), `View/HudView.cs` (yeni), `View/CameraFitter.cs` (en/boy oranı artık kameranın piksel boyutundan), `View/View.asmdef` (`UnityEngine.UI` referansı).

**`dotnet test tests/BlastGame.Tests.slnx`:** `Başarısız: 1, Başarılı: 680, Atlanan: 0, Toplam: 681`. **Hedef (681 yeşil) tutmadı; bir test yanlış görünüyor, kodu ona göre bükmedim.**
- Düşen: `GameSessionTests.Tap_AfterGameOver_ChangesNothingAndSkipsSource(won: False)`, satır 597: `session.Remaining(TileColor.Red)`.
- Neden: `Finished(false, ...)` yalnız `Blue x1` hedefiyle kurulur (satır 52); Red hedef değil, spec'e göre `Remaining(Red)` `ArgumentException` atar ve atıyor. `won: True` varyantı (hedef Red) geçiyor. Düzeltme testçinin işi: `Remaining` çağrısı oturumun gerçek hedef rengini (`won` ? Red : Blue) kullanmalı.

**`unity-check`:** `Unity compile check: clean.` (çıkış kodu 0).

**Hedefin gösterimi:** her hedef için renkli kare + kalan sayı; 0 olunca kare ve yazı soluklaşır ve sayı yerine "Done" yazar. Hamle sayacı barın solunda. Dokunuştan hemen sonra `Refresh`.

**Görüntü:** 9x9, 4 renk, 20 hamle, `Red x20, Blue x20` adlandırılmış sabit. Seed saatten (`DateTime.UtcNow.Ticks`), sol alt köşede ve Console'da. Üst bar ekran yüksekliğinin %12'si; kamera `rect` ile kalan alana sığdırılır. Sonuç paneli animasyon bitince açılır; açıkken `HandleTap` geri döner. "Play again": yeni seed, eski `GridView` `Destroy`, yeni grid/oturum, hedef öğeleri yeniden kurulur.

**Yapamadıklarım / doğrulamadıklarım:** Editor'de Play çalıştırmadım; elle kontrol listesi (2-5) yapılmadı, yalnız derleme doğrulandı. Düzen (bar, panel, buton boyutu, dikey ekrana sığma) gözle görülmedi. `Text` için `LegacyRuntime.ttf` yerleşik fontu varsayıldı; çalışmazsa yazılar görünmez. `EventSystem` için `StandaloneInputModule` (eski Input) kullanıldı, WebGL'de buton tıklaması elle denenmeli.

**Öneri:** `ColorGoal` kurucusu `color` değerini `Enum.IsDefined` ile denetleyebilir (spec dışı, eklemedim).

## Düzeltme
`GameSession.Goals` artık kurucuda bir kez `Array.AsReadOnly` ile sarılan salt okunur görünümü döndürüyor (`(ColorGoal[])session.Goals` cast'i başarısız olur); yalnız `GameSession.cs` değişti.
`dotnet test`: `Başarısız: 0, Başarılı: 681, Toplam: 681`; `unity-check`: `Unity compile check: clean.` (çıkış 0).
