# Adım 1, Grid: Claude kontrolcü raporu

## Test özeti
`dotnet test tests/BlastGame.Tests.slnx`:
`Başarılı!  - Başarısız:     0, Başarılı:   246, Atlanan:     0, Toplam:   246`

## Bulgular
Spec'e aykırı kod hatası **bulamadım**. Küçük gözlemler (hepsi düşük):

1. **Düşük, eksik test (opsiyonel).** `GridFactory.cs:70-73`'teki boyut kontrolleri `new Grid` kurucusunun kontrolleriyle yinelenir; yükseklik kontrolünü kaldırınca (M23) hiçbir test kırılmadı. Davranış aynı (aynı istisna, kaynak tüketilmeden), yani eşdeğer mutasyon. İsteyen testçi "geçersiz boyutta random kaynağı hiç çağrılmaz" testi ekler; spec açıkça istemiyor.
2. **Düşük, görüntü.** `GameBootstrap.cs:228`: sahnede kamera varsa ortografik yapılır, clip değerlerine dokunulmaz; kamera z=-10, kareler z=0, varsayılanla sorun yok. Yalnız not.
3. **Düşük, görüntü.** `GridView.Build` her çağrıda yeni `Texture2D` üretir; Adım 1'de bir kez çağrılıyor, sorun yok. Adım 3'te yeniden çizim gelirse sprite önbelleğe alınmalı (Backlog notu).

## Spec karşılaştırması
- Koordinat, boyut sınırları, boş başlangıç, dışarı Get/Set → `ArgumentOutOfRangeException`, `IsInside` hata vermez, `Set`'te tanımsız renk reddi: `Grid.cs` uyuyor.
- `SeededRandom`: SplitMix64 sabitleri, başlangıç `(ulong)(long)seed`, `% (ulong)max`, `max <= 0` hatası: uyuyor.
- `GridFactory`: doğrulama sırası, y→x doldurma, `InvalidOperationException`, `null` kontrolü: uyuyor.
- Görüntü (yalnız okuyarak, çalıştıramadım): 8x10, 5 renk, seed 42, kare 0.9, aralık 1, renk kodları ve arka plan doğru; kamera `max(halfH, halfW/aspect)` ile en/boy sığdırır, yarım hücre boşluk var, boyut değişince `Update`'te yeniden sığdırır, merkez doğru; sahne dosyasına dokunulmuyor.

## Kural 4
`Core/` altında `UnityEngine`, `System.Random` yok (grep); `Core.asmdef` `noEngineReferences: true`; `Core.csproj` `LangVersion 9.0`; C# 10+ özelliği yok. `AssemblyMarker`/`SmokeTests` silinmiş.

## Test dokunulmazlığı
`git log -- tests/`: yalnız `test(core)` (b80944a, e2973ac) ile kurulum commit'leri (89d6136, d776057). `700cc45`, `56a4dde`, `49ee873`, `14d7520`, `80b6089` için `git show --stat -- tests/` boş.

## Bilerek bozma (hepsi geri alındı, `git status` temiz)
| Bozma | Sonuç |
|---|---|
| IsInside `x < Width` → `<=` | yakalandı: `IsInside_Outside3x2_ReturnsFalseWithoutThrowing` (6 test) |
| IsInside `x >= 0` → `x > 0` | yakalandı: `IsInside_Corners3x2_ReturnsTrue` |
| Doldurma x/y yer değiştirdi | yakalandı: `..._FillsRowsFromBottomAndXLeftToRight` (14) |
| Doldurma y ters sıra | yakalandı: aynı test (8) |
| SplitMix64 çarpan sabiti 1 bit | yakalandı: `Next_Seed0_FirstValueIsReferenceOutputModuloMax` |
| Gamma sabiti 1 bit | yakalandı: aynı test |
| Kaydırma 30→29 | yakalandı: aynı test |
| Seed `(long)` → `(uint)` | yakalandı: `Next_SeedMinusOne_UsesSignExtendedInitialState` |
| `ThrowIfOutside` y kontrolü kalktı | yakalandı: `Set_ColorOutside3x2_...` (12) |
| x kontrolü kalktı | yakalandı: `Set_ColorOutside3x2_...` (8) |
| `Get` kontrolsüz | yakalandı: `Get_Outside3x2_...` (15) |
| Kaynak `>= colorCount` → `>` | yakalandı: `CreateRandom_SourceValueOutOfRange_...` |
| Kaynak negatif kontrolü kalktı | yakalandı: `...badValue: -1` |
| `Enum.IsDefined` kalktı | yakalandı: `Set_UndefinedColor_ThrowsArgumentOutOfRange` |
| `colorCount > 6` → `> 7` | yakalandı: `(colorCount: 7)` |
| `colorCount < 1` → `< 0` | yakalandı: `(colorCount: 0)` |
| `random == null` kalktı | yakalandı: `CreateRandom_NullRandom_ThrowsArgumentNull` |
| `maxExclusive <= 0` → `< 0` | yakalandı: `Next_MaxZeroOrNegative...(0)` |
| Boyut alt/üst sınır kaydırma (genişlik min, yükseklik min, yükseklik max) | yakalandı: `Constructor_*OutOfRange` |
| Factory yükseklik kontrolü kalktı (M23) | yakalanmadı, eşdeğer (bulgu 1) |
| `Next`'e maske eklendi (M14) | yakalanmadı, eşdeğer: davranış değişmiyor |

Yakalanmayan gerçek zayıflık yok.
