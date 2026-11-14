# Görev: Adım 6, özel öğeler (testçi)

Model: Opus 5.5 medium (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/06-special-items` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/06-special-items.md`: "Özel öğeler", "Kurallar", "Örnekler", "Uç durumlar", "Arayüz sözleşmesi" (netleştirmeler dahil). Görüntü katmanı, animasyon ve elle kontrol test edilmez.
3. Stub imzaları: `Assets/Scripts/Core/SpecialKind.cs`, `EffectKind.cs`, `ClearedCell.cs`, `TurnEffect.cs`, `SpecialRules.cs`; değişen `Cell.cs`, `CellKind.cs`, `TurnResult.cs`. Mevcut tiplerden (`Grid`, `CellPos`, `CellMove`, `TileColor`, `IRandomSource`, `SeededRandom`, `TurnRules`, `GameSession`, `ColorGoal`, `GroupFinder`, `BlastRules`) yalnız public imzalar. Uygulama gövdelerini okuma.
4. Mevcut test yardımcıları: `tests/Core.Tests/TestGrid.cs`, `tests/Core.Tests/TurnTestHelpers.cs` ve önceki adımların sahte kaynakları. Varsa onları kullan; gerekirse grid'e özel hücre koyabilmek için yardımcıyı **yeni dosyada** genişlet.

## Rolün
Amacın kodu kırmak, geçirmek değil. Spec'in söylemediği davranışı tahmin etme: test yazma, belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. Mevcut testleri değiştirme ve silme; tek istisna spec'in sözleşme değişikliğinin zorunlu kıldığı güncellemeler (RULES, Onay); öyle bir güncelleme yaptıysan raporda tek tek yaz. Paket ekleme. Commit atmazsın, push yok, dal değiştirme yok.

## Ölçü
Zor adım: yeni test için kaba üst sınır **~150** (`Theory` satırları dahil). Bir test bir davranış sınıfını sınar; aynı sınıfı tekrar etme.

## İşler
1. **Yeni testler**, konu başına sınıf: `CellSpecialTests`, `ClearedCellTests`, `TurnEffectTests`, `SpecialRulesForGroupTests`, `SpecialRulesActivateTests`, `TurnRulesSpecialTests`, `GameSessionSpecialTests` (adlar önerinin; düzenli olsun). "Uç durumlar" listesi en az kapsamdır.
2. **Örnekler birebir:** spec'teki Örnek 1-4 ve 6 (grid'ler, etkiler, `Cleared` sıraları, `TriggeredBy`, düşme ve doldurma, kaynak çağrı sayıları). Örnek 5'i küçük bir grid'le somutlaştır.
3. **Sıra sözleşmesi:** `Cleared` sırası (öğe önce, sonra alan tarama sırası), `Effects` sırası (FIFO), `Blasted` = `Cleared` konumlarının sırayla birleşimi, kaynak çekiliş sırası (yön çekilişi doldurmadan önce, tam bir kez). Sıra sözleşmedir; liste eşitliğiyle sına, küme eşitliğiyle değil.
4. **Hata türleri tam:** `Assert.Throws<T>` tam türle; `ParamName` spec'te yazıyorsa kontrol et. Geçersiz yön çekilişinde grid'in hiç değişmediğini hücre hücre sına.
5. **Tekrarlanabilirlik:** aynı seed'le aynı dokunuş dizisi → aynı grid ve aynı etkiler.
6. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Yeni testler **kırmızı**, mevcut 766 test **yeşil**. Geçen yeni test varsa incele: bir şey ölçmüyor olabilir, düzelt ya da nedenini raporla (ör. 3 parametreli `TurnResult` kurucusunun `Effects`'i boş vermesi stub'da da geçer, bu normal).
7. **Düzen:** test adları `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9. Grid'ler test içinde okunaklı dursun (`y` alttan üste, spec'teki gibi).

## Rapor
`reports/06-special-items-tester.md` (RULES kural 9, kısa): test dosyaları ve yeni test sayısı; **spec maddesi → test eşlemesi**; `dotnet test` özeti (toplam / geçen / kalan) aynen; mevcut testlerde yaptığın zorunlu güncellemeler (varsa); spec'te belirsiz bulduğun yerler. Son mesajında kısa özet.
