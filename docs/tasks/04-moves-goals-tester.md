# Görev: Adım 4, hamle, hedef, kazan/kaybet (testçi)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`, dal `step/04-moves-goals` (zaten açık; dal değiştirme).

## Önce oku (yalnız bunlar; kota dar)
1. `docs/RULES.md`: yalnız "Herkes için", "Onay" ve "Testçi" bölümleri.
2. `docs/specs/04-moves-goals.md`: "Kurallar", "Örnekler", "Uç durumlar", "Arayüz sözleşmesi". Görüntü katmanı ve elle kontrol test edilmez.
3. Stub imzaları: `Assets/Scripts/Core/GameStatus.cs`, `ColorGoal.cs`, `GameSession.cs`. Mevcut tiplerden (`Grid`, `Cell`, `TileColor`, `IRandomSource`, `SeededRandom`, `GridFactory`, `TurnRules`, `TurnResult`) yalnız public imzalar. Uygulama gövdelerini okuma.
4. Mevcut test yardımcıları: `tests/Core.Tests/TestGrid.cs` ve Adım 3 testlerindeki sahte kaynak (sıralı değer döndüren, çağrı sayan). Varsa onu kullan, yeniden yazma.

## Rolün
Amacın kodu kırmak, geçirmek değil. Spec'in söylemediği davranışı tahmin etme: test yazma, belirsizliği raporla. Yazdığın yer yalnız `tests/Core.Tests/`. **Mevcut testleri değiştirme ve silme.** Commit atmazsın, push yok, dal değiştirme yok.

## Ölçü
Basit adım: yeni test için kaba üst sınır **~80** (`Theory` satırları dahil; RULES, Testçi). Bir test bir davranış sınıfını sınar; aynı sınıfı tekrar etme.

## İşler
1. **Yeni testler**, tip başına sınıf: `ColorGoalTests`, `GameSessionTests`. Spec'in her kural maddesi için normal durum, sınır ve geçersiz girdi; "Uç durumlar" listesi en az kapsamdır.
2. **Örnekler birebir:** spec'teki Örnek 1-4 (`MovesLeft`, her `Remaining`, `Status`, boş sonuç, grid ve kaynak çağrı sayısı).
3. **"Kaynak çağrılmaz" ve "grid değişmez"** maddelerini çağrı sayısı ve hücre hücre karşılaştırmayla kanıtla (boşa dokunuş, `Won` ve `Lost` sonrası dokunuş, kurucu hataları).
4. **Hata türleri tam** (`ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException`); `ArgumentException` beklenen yerde alt türünün (`ArgumentNullException` vb.) yanlışlıkla geçmemesi için `Assert.Throws<ArgumentException>` (tam tür) kullan. `ParamName`'i kontrol et. Kontrol sırasını birden çok geçersiz girdiyle sına.
5. **Kaynak hatası:** geçersiz değer döndüren sahte kaynakla `Tap` → `InvalidOperationException`, `MovesLeft` ve `Remaining` değişmemiş.
6. **Tekrarlanabilirlik:** aynı seed'li iki oturum, aynı dokunuşlar → aynı `MovesLeft`, kalanlar, `Status`.
7. **Düzen:** test adları `Metot_Durum_Beklenen`, namespace `BlastGame.Core.Tests`, C# 9.
8. **Çalıştır:** `dotnet test tests/BlastGame.Tests.slnx`. Yeni testler **kırmızı**, mevcut testler **yeşil**. Geçen yeni test varsa incele: bir şey ölçmüyor olabilir, düzelt ya da nedenini raporla.

## Rapor
`reports/04-moves-goals-tester.md` (RULES kural 9, kısa): test dosyaları ve yeni test sayısı; **spec maddesi → test eşlemesi**; `dotnet test` özeti (toplam / geçen / kalan) aynen; spec'te belirsiz bulduğun yerler. Son mesajında kısa özet.
