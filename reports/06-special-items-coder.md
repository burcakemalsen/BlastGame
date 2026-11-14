# Adım 6 kodcu raporu: özel öğeler

## Aşama A

### Dosyalar (`Assets/Scripts/Core/`)
- Yeni: `SpecialKind.cs`, `EffectKind.cs`, `ClearedCell.cs`, `TurnEffect.cs`, `SpecialRules.cs` (+ Unity'nin ürettiği `.meta` dosyaları).
- Değişen: `CellKind.cs` (`Special`), `Cell.cs` (`OfSpecial`, `OfOrb`, `IsSpecial`, `Special`, `OrbColor`), `TurnResult.cs` (4 parametreli kurucu, `Effects`).
- Yeni gövdelerin hepsi `throw new NotImplementedException();`. İstisnalar: enum değerleri ve `SpecialRules` sabitleri tam; `TurnResult`'ın 3 parametreli kurucusu `Effects`'i boş liste (`Array.Empty<TurnEffect>()`) yapar ve `Effects` getter'ı çalışır. `Cell`'in mevcut üyeleri ve eşitliği, `TurnRules`, `GameSession`, `GroupFinder`, `BlastRules` değişmedi. Görüntü katmanına dokunulmadı.

### Kontroller
- `dotnet build tests/BlastGame.Tests.slnx` → `Oluşturma başarılı oldu. 0 Uyarı 0 Hata`
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı! - Başarısız: 0, Başarılı: 766, Atlanan: 0, Toplam: 766`
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` → `Unity compile check: clean.` (çıkış 0)

### Spec'te belirsiz bulduğum yerler (Aşama B'de varsayımım)
1. `SpecialRules.ForGroup`: `size` 0-4 ve tanımsız `color` birlikte gelirse hata mı, `Cell.Empty` mi? Hata sırası listesi renk denetimini çekilişten önce koyuyor; varsayım: boyuttan bağımsız `ArgumentOutOfRangeException(color)`.
2. `TurnResult` 4 parametreli kurucu: null denetim sırası belirtilmemiş; varsayım: `blasted`, `moves`, `filled`, `effects`. `blasted`'ın etkilerle tutarlılığı kurucuda denetlenmez (spec istemiyor).
3. `TurnEffect` kurucusu: `kind` ile `created` tutarlılığı (Group dışında `Created` dolu olması) ya da tanımsız `kind` için hata belirtilmemiş; varsayım: yalnız spec'teki iki denetim.
4. `Activate` grid dışı hatasında parametre adı belirtilmemiş; varsayım: `x` / `y` (`GroupFinder` gibi).
5. `Cell.Special` Orb hücresinde `SpecialKind.Orb` döner (özel hücre olduğu için); spec bunu dolaylı söylüyor.

## Aşama B

### Değişen dosyalar
- Kural katmanı (`Assets/Scripts/Core/`): `Cell.cs` (özel hücre alanı, eşitlik, `GetHashCode`, `ToString`), `ClearedCell.cs`, `TurnEffect.cs`, `TurnResult.cs` (4 parametreli kurucu), `SpecialRules.cs` (`ForGroup`, `Activate`: kendi hücresi önce, tarama sırası, FIFO kuyruk, `TriggeredBy`), `GroupFinder.cs` (özel başlangıç → boş liste; `BlastRules` bunu kullandığı için değişmedi), `TurnRules.cs` (beş durum; yön çekilişi grid değişmeden önce; `Blasted` = `Cleared` konumlarının birleşimi; düşme ve doldurma bir kez), `GameSession.cs` (renk başına sayım).
- Görüntü katmanı (`Assets/Scripts/View/`): `GridView.cs` (yeniden yazıldı: özel öğe çizimi, canlılık, tek zaman çizelgesinde animasyonlar, efekt havuzu), yeni `ShapeSprites.cs` (+ `.meta`; ilkel şekil dokuları çalışma başına bir kez), `TileColors.cs` (nötr tonlar), `HudView.cs` (`SetEndless`, "Next board" butonu), `GameBootstrap.cs` (`EndlessMode`, doğrudan `TurnRules.Tap`, sonuç paneli kapalı). Sahne dosyasına dokunulmadı.

### Kontroller
- `dotnet test tests/BlastGame.Tests.slnx` → `Başarılı! - Başarısız: 0, Başarılı: 913, Atlanan: 0, Toplam: 913` (seed 2026 tekrarlanabilirlik testi dahil).
- `scripts/unity-check.ps1` → `Unity compile check: clean.` (çıkış 0; View/Core'da uyarı yok).

### Animasyon (spec'ten sapmalar ve seçimler)
- Süreler spec'teki hedefler: patlama 0.12, toplanma 0.2 + pat 0.15, Rocket 0.025 sn/hücre (sabit hız), Bomb dalga 0.15, Orb toplam ≤ 0.5 (ışın aralığı en fazla 0.035, hedef çoksa kısalır), düşme 0.35.
- Rocket yarıları tahtanın ucunu iz boyu (1.6 hücre) kadar geçip maskeyle gizlenir; iz sonra 0.1 sn'de söner. Bu yüzden Rocket etkisinin bitişi uçuştan ~0.14 sn uzun; düşme bundan sonra başlar.
- Bomb: zincirdeki öğe patlama anında başlar (spec); 3x3 kareler 0.02 sn sonra 0.1 sn'de kaybolur; 0.12 sn hafif sarsıntı (isteğe bağlı olan eklendi).
- Canlılık: yalnız süs katmanı hareket eder (Rocket okları eksen boyunca hafif nefes, Bomb halkası hafif nabız, Orb'un üç parçalı halkası yavaş döner); dokunuş animasyonlarıyla çakışmaz.
- Geçici nesneler (yarı, iz, parlama, halka, ışın) `GridView` altındaki havuzdan gelir; havuz en yoğun dokunuş kadar büyür, tahta değişince `GridView` ile yok olur. Dokunuş başına doku ya da kalıcı nesne yok (dokunuş başına yalnız bir coroutine).

### Yapamadıklarım / emin olmadıklarım
- Editor'de Play ve WebGL'de elle kontrol yapamadım (ajan Editor'ü kullanamaz); görünüş, okunaklılık ve akıcılık doğrulanmadı. Elle kontrol listesi 2-5 ürün sahibinde.
- `EndlessMode` `static readonly` (const değil) ki kapalı dal derleyici uyarısı vermesin.

### Öneriler
- `BlastRules.Blast` artık `TurnRules`'ta kullanılmıyor (grup boyutu ve renk önceden gerektiği için `GroupFinder` doğrudan çağrılıyor); ileride kaldırılması ya da `TurnRules`'un ona dönmesi düşünülebilir (Backlog adayı).
