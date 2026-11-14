# Adım 7, engeller: kodcu raporu

## Aşama A

### Dosyalar
Yeni (`Assets/Scripts/Core/`, `.meta` dosyalarını Unity üretti): `ObstacleKind.cs`, `ObstacleHit.cs`, `ObstacleGoal.cs`, `ObstaclePlacement.cs`, `GravityResult.cs`.

Değişen: `CellKind.cs` (sona `Obstacle`), `Cell.cs`, `TurnEffect.cs`, `TurnResult.cs`, `GravityRules.cs`, `GameSession.cs`, `LevelData.cs`.

Yeni kurucu, özellik ve metotlar `throw new NotImplementedException();` atıyor. İstisnalar, görev tarifindeki gibi: `Cell.HasBalloon` false döner; `TurnEffect.Hits`, `TurnResult.Collected`, `GameSession.ObstacleGoals`, `LevelData.Obstacles`/`ObstacleGoals` mevcut kurucularla boş liste verir. Mevcut metotların davranışı değişmedi. Görüntü katmanına ve `tests/`'e dokunulmadı.

### Kontroller
- `dotnet build tests/BlastGame.Tests.slnx`: `0 Uyarı`, `0 Hata`
- `dotnet test tests/BlastGame.Tests.slnx`: `Başarılı!  - Başarısız:     0, Başarılı:  1028, Atlanan:     0, Toplam:  1028`
- `scripts/unity-check.ps1`: `Unity compile check: clean.` (çıkış 0). Logda `warning CS`/`error CS` yok.

Not: çalıştırmadan önce `tasklist`'te küçük bir `unity.exe` süreci (5 MB, Editor değil gibi) görünüyordu; kontrol yine temiz geçti.

### Spec'te belirsiz bulduğum yerler
1. **`ToString` biçimleri yazılmamış.** `Cell` için engel ve balon biçimi (örnek: `Obstacle(Crate, 2)`, `Color(Red, Balloon)`?), `ObstacleHit` ("ClearedCell gibi", ama iki hücre var: `(x, y) Before -> After`?), `ObstaclePlacement` için biçim yok. `ObstacleGoal` için `Crate x1` (ColorGoal gibi) varsayıyorum. Testçi biçimi sınayacaksa spec'te sabitlenmeli.
2. **`GameSession` 6 parametreli kurucuda hata sırası.** "Parametre sırasıyla" deniyor, ama "ikisi birden boş → `ArgumentException(goals)`" hangi noktada denetlenir: `goals` öğe denetiminden önce mi, `obstacleGoals` null denetiminden sonra mı? Benim okumam: `goals` null, `obstacleGoals` null, ikisi boş mu, sonra `goals` öğeleri, sonra `obstacleGoals` öğeleri, sonra `random`. Aynı soru `LevelData` 7 parametreli kurucu için: spec sırası `goals` öğeleri, `obstacles`, `obstacleGoals` null, ikisi boş, `obstacleGoals` öğeleri; burada "ikisi boş" `obstacles` denetiminden sonra geliyor. İkisinin farklı olması bilerek mi?
3. **`Cell.WithBalloon(false)` renkli olmayan hücrede.** Spec "renkli değilse `InvalidOperationException`" diyor; `false` için de hata mı (öyle uygulayacağım), yoksa no-op mu?
4. **`ObstacleGoal` tanımsız tür + `default(ObstacleGoal)`.** `default` Count 0 verir; `GameSession` bunu "Count < 1" ile yakalar. Sorun yok, yalnız not.
5. **`Remaining(ObstacleKind)` tanımsız türde** (`(ObstacleKind)99`): hedef olmadığı için `ArgumentException(kind)` atacağım; `ArgumentOutOfRangeException` beklenmiyorsa yeterli.
