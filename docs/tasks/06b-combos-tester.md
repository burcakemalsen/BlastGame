# Görev: Adım 6b, kombolar (testçi)

Model: Opus 5.5 medium (zor adım). Proje klasörü: `C:\Dev\BlastGame`, dal `step/06b-combos` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/06b-combos.md`: "Kombolar", "Kurallar", "Örnekler", "Uç durumlar", "Arayüz sözleşmesi" (netleştirmeler ve sözleşme değişikliği notu dahil). Görüntü katmanı test edilmez. Gerekirse `docs/specs/06-special-items.md`'nin "Kurallar" bölümü (tekli öğeler, zincir).
3. Stub imzaları: `Assets/Scripts/Core/EffectKind.cs`, `TurnEffect.cs`, `SpecialRules.cs`; diğer tiplerden yalnız public imzalar. Uygulama gövdelerini okuma.
4. Mevcut yardımcılar: `tests/Core.Tests/SpecialGrid.cs`, `TestGrid.cs`, `TurnTestHelpers.cs`. Kullan; gerekirse yeni dosyada genişlet.

## Rolün
Amacın kodu kırmak. Spec'in söylemediği davranışı tahmin etme: belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. Mevcut testleri değiştirme ve silme; tek istisna spec'in sözleşme değişikliği notunun zorunlu kıldığı güncellemeler (yan yana özel öğeye `TurnRules.Tap`/`GameSession.Tap` ile dokunan testler artık kombo görür). Her güncellemeyi raporda tek tek yaz; güncellenen testin hâlâ ölçtüğü şeyi koru (gerekirse özel öğeleri ayırarak). `SpecialRules.Activate` testleri değişmez. Paket ekleme, commit yok, push yok.

## Ölçü
Yeni test için kaba üst sınır **~150** (`Theory` satırları dahil). Bir test bir davranış sınıfı.

## İşler
1. **Yeni testler**, konu başına sınıf (ör. `ComboParticipantsTests`, `ComboSelectionTests`, `ComboAreaTests`, `ComboConversionTests`, `ComboChainTests`, `TurnEffectComboTests`, `TurnRulesComboTests`, `GameSessionComboTests`). "Uç durumlar" listesi en az kapsamdır.
2. **Örnekler birebir:** Örnek 1-8 (katılımcılar, `MergedCount`, `Cleared` sırası, `Converted`, `TriggeredBy`, kaynak çağrıları).
3. **Sıra sözleşmesi liste eşitliğiyle:** `Cleared` (katılımcılar önce: dokunulan, sonra `y`/`x`), alan sırası, `Converted` sırası, `Effects` sırası, yön çekilişlerinin sırası ve sayısı, doldurmadan önce olmaları.
4. **Hata türleri tam;** `ParamName` yazılıysa kontrol et. Geçersiz yön çekilişinde grid hücre hücre değişmemiş.
5. **Tekrarlanabilirlik:** aynı seed, aynı dokunuşlar → aynı grid ve etkiler (en az bir kombonun gerçekten oluştuğunu da doğrula).
6. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Yeni testler kırmızı, mevcut 927 test yeşil (güncellediğin testler yeni davranışı beklediği için kırmızı olabilir; raporda say). Stub'la geçen yeni test varsa incele ve raporla.
7. **Düzen:** `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9; grid'ler okunaklı (`y` alttan üste).

## Rapor
`reports/06b-combos-tester.md` (kısa): dosyalar, yeni test sayısı, **spec maddesi → test eşlemesi**, `dotnet test` özeti aynen, mevcut testlerde yapılan güncellemeler, belirsizlikler. Son mesajında kısa özet.
