# Adım 3, düşme ve doldurma: Gemini kontrolcü raporu

Model: Gemini 4 Argon (`agy -p`, salt okunur, varsayılan model). Çalışma: 2026-11-14, 88 sn. Gemini'ye yalnız spec, adımın Core/View dosyaları ve adımın test dosyaları verildi; Claude kontrolcünün raporunu görmedi. Çıktı orchestrator tarafından aynen aktarıldı; yalnız yerel dosya bağlantıları düz dosya adına çevrildi.

### Bulgular

Kodda spec ile çelişen bir **kod hatası (bug) bulunmamıştır**. Testlerde hata kontrol sırasına ilişkin şu eksik senaryolar tespit edilmiştir:

1. **`TurnRulesTests.cs:162-211`**
   - **Spec kuralı:** *Bir dokunuş (`TurnRules.Tap`)* — "Sıra: `grid`, `random`, `colorCount`, koordinat." (`03-gravity-refill.md:35`)
   - **Önem:** Orta | **Tür:** Eksik test
   - **Açıklama:** Hem `colorCount` hem de `(x, y)` koordinatı aralık dışı olduğunda ikisi de `ArgumentOutOfRangeException` fırlatır. İkisinin aynı anda geçersiz verildiği ve `ex.ParamName == "colorCount"` olduğunun doğrulandığı bir test yoktur; kodda koordinat kontrolü `colorCount` kontrolünden önceye alınsa mevcut tüm testler yine geçer.

2. **`TurnRulesTests.cs:199-203`**
   - **Spec kuralı:** *Bir dokunuş (`TurnRules.Tap`)* — "Sıra: `grid`, `random`, `colorCount`, koordinat." (`03-gravity-refill.md:35`)
   - **Önem:** Orta | **Tür:** Eksik test
   - **Açıklama:** `Tap_NullGridAndEverythingElseBad_ThrowsArgumentNull` testinde hem `grid` hem `random` `null` geçildiği halde fırlatılan `ArgumentNullException` üzerinde `Assert.Equal("grid", ex.ParamName)` kontrolü yapılmamıştır. `random` kontrolü `grid`'den önce yapılsa bu test hatayı yakalayamaz.

3. **`RefillRulesTests.cs:146-150`**
   - **Spec kuralı:** *Doldurma (`RefillRules.Fill`)* — "Kontrol sırası: `grid`, `colorCount`, `random`; hepsi kaynağa dokunmadan önce." (`03-gravity-refill.md:27`)
   - **Önem:** Düşük | **Tür:** Eksik test
   - **Açıklama:** `Fill_NullGridBadCountNullRandom_GridCheckedFirst` testinde `grid` ve `random` aynı anda `null` iken `ex.ParamName == "grid"` doğrulanmamıştır.

---

### Kontrol Edilen ve Sorun Bulunmayan Noktalar

- **Düşme (`GravityRules.cs`):** Sütunların bağımsızlığı, renk/tür korunumu, yerinde kalan hücrelerin listeye girmemesi, `(x, To.Y)` artan sıralaması ve `null` kontrolü spec ile birebir uyumlu.
- **Doldurma (`RefillRules.cs`):** `(y, x)` tarama sırası, yalnız boş hücreler için rastgele kaynak çağrısı, `grid -> colorCount -> random` doğrulama sırası ve geçersiz rastgele değerde (`< 0` veya `>= colorCount`) grid'i değiştirmeden `InvalidOperationException` fırlatılması doğru.
- **Bir dokunuş (`TurnRules.cs`):** `Blast -> Collapse -> Fill` sırası, patlama olmadığında kaynağa dokunulmaması ve boş listeler dönmesi, `grid -> random -> colorCount -> (x, y)` hata sırası doğru.
- **Arayüz sözleşmesi (`CellMove.cs`, `TurnResult.cs`):** İmzalar, `CellMove.ToString()` formatı (`"(x, y)->(x, y)"`), `TurnResult` referans saklama davranışı ve `ParamName` sıralaması tam uyumlu.
- **Görüntü katmanı (`GameBootstrap.cs`, `GridView.cs`):** Tek `SeededRandom(42)` kullanımı, `TurnRules.Tap` + `Refresh()` akışı ve statik `_squareSprite` önbelleği spec'e uygun.
- **Kısıtlar ve temizlik:** `Core` katmanında `UnityEngine` veya `System.Random` kullanımı yok, C# 9 üstü dil özelliği (`record`, `init` vb.) yok, testlere özel dallanma (test-only special-casing) yok.
