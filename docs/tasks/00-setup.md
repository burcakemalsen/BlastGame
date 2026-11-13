# Görev: Adım 0, iskelet (kurulumcu)

Model: Sonnet 5.5 medium. Proje klasörü: `C:\Dev\BlastGame`.

## Önce oku
1. `CLAUDE.md`
2. `docs/RULES.md` (özellikle "Herkes için", "Onay", "Push öncesi tarama", "Kurulumcu")
3. `docs/PLAN.md`: bölüm 2 (mimari), 3 (test yolu), 5/Adım 0, 8 (repo düzeni)
4. Yerel terim listesinin biçimi (orchestrator yolunu verir; script bunu okuyacak)

## Rolün
Kurulumcusun. Oyun kodu yazmazsın. Yazabileceğin yerler `docs/RULES.md` "Kurulumcu" bölümünde. `docs/` altındaki dosyalara dokunmazsın.

**İzinli:** test projesi için NuGet indirmesi (xUnit, test SDK'sı), `git init`, yerel commit.
**Yasak, gerekirse dur ve sor:** Unity modülü, global araç ya da başka yazılım kurmak; push; git ayarı değiştirmek (global de yerel de); proje klasörü dışına yazmak; yerel arşiv dosyalarını açmak.

Bir adım planlandığı gibi gitmezse kendi çözümünü uydurma: ne denediğini ve çıktıyı rapora yaz, sonraki bağımsız adıma geç.

## Ortam
- Unity: `C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe`
- .NET SDK 10, git 2.56. `gh` yok.
- Android Build Support Hub'dan kurulmuş olmalı. Kontrol: `...\Editor\Data\PlaybackEngines\AndroidPlayer` klasörü var mı. Yoksa APK adımını atla ve raporla.
- Unity batchmode komutlarını Editor kapalıyken çalıştır (aynı proje iki kez açılamaz).

## İşler
1. **Unity projesi.** `Unity.exe -batchmode -quit -createProject C:\Dev\BlastGame -logFile -` ile proje oluştur. Unity dolu klasörü kabul etmezse `C:\Dev\BlastGame\_unity_tmp` içinde oluştur, `Assets/`, `Packages/`, `ProjectSettings/` klasörlerini köke taşı, `_unity_tmp`'yi sil. Varsayılan davranış modunu 2D yap. Etkin platformu Android yap. Paket adı `com.burcakemalsen.blastgame`, ürün adı `BlastGame`, yön dikey (portrait).
2. **Klasörler ve asmdef.** `Assets/Scripts/Core/Core.asmdef` (`noEngineReferences: true`, `autoReferenced: true`), `Assets/Scripts/View/View.asmdef` (`Core`'a başvurur). `tests/`, `reports/`, `docs/specs/` klasörleri (boş klasör git'e girmez; içine `.gitkeep` koy).
3. **`.gitignore`.** `docs/PLAN.md` bölüm 8'deki listenin tamamı. Dikkat: hazır Unity şablonlarındaki `*.csproj` ve `*.sln` kuralları her klasöre uygulanır ve test projesini dışlar; bunları yalnız köke uygula (`/*.csproj`, `/*.sln`). `.meta` dosyaları dışlanmaz.
4. **Test projeleri.** `tests/Core/Core.csproj`: `Assets/Scripts/Core/**/*.cs` dosyalarını `Compile Include` ile içine alır (kopya yok), Unity'nin dil sürümüne ve API seviyesine sabitlenir (madde 5). `tests/Core.Tests/Core.Tests.csproj`: xUnit, `Core`'a başvurur. `Assets/Scripts/Core/` altına oyun kodu değil, yalnız testin dokunacağı en küçük örnek (örneğin `internal static class AssemblyMarker`) koymak gerekiyorsa bunu raporda gerekçesiyle belirt. Tek örnek test yaz; `dotnet test` yeşil olsun. Kökte bir `.sln` gerekiyorsa `tests/` içine koy.
5. **C# sürümü.** Unity 6000.6.3f1'in desteklediği C# dil sürümünü ve API seviyesini (`netstandard2.1` mi) doğrula. Kanıt olarak dosya yolu ve ilgili satırı aynen aktar (örneğin Unity'nin ürettiği bir csproj'daki `LangVersion`, ya da Editor'ün derleyici log'u). Test projesini buna sabitle. Bu sürümde eksik kalan dil özelliklerini (örneğin `record`/`init` için `IsExternalInit`) listele.
6. **`scripts/unity-check.ps1`.** Projeyi batchmode'da açıp derlesin, log'da derleme hatası varsa hataları yazsın ve sıfır olmayan kodla çıksın. Çalıştır, çıktıyı aktar. Ayrıca bilerek bir derleme hatası içeren geçici bir dosyayla scriptin hatayı yakaladığını göster, sonra dosyayı sil.
7. **`scripts/prepush-scan.ps1`.** `docs/RULES.md` "Push öncesi tarama"daki 3 maddeyi kontrol etsin. Terim listesini yerel listeden okusun (tam kelime, büyük/küçük harfe duyarlı); dosya yoksa ya da liste boşsa hata versin. Bulgu varsa dosya ve satırı yazıp sıfır olmayan kodla çıksın. Test: listedeki bir terimi içeren deneme dosyasını stage et, scriptin yakaladığını göster; sonra deneme dosyasını unstage edip sil, temiz durumda geçtiğini göster.
8. **APK.** `Assets/Editor/BuildScript.cs`: varsayılan sahneyi Android için `Builds/Android/BlastGame.apk`'ya derleyen statik bir metot (oyun kodu değil). `scripts/build-android.ps1`: bunu batchmode'da `-executeMethod` ile çağırsın, başarısızsa sıfır olmayan kodla çıksın. Çalıştır, APK'nın yolunu ve boyutunu aktar. Telefona yükleme ürün sahibinin işi.
9. **Dokümanlar.** `README.md` İngilizce: proje ne, durum (geliştirme aşamasında), AI desteğiyle geliştirildiği ve nasıl (ajan rolleri, test kapısı; `docs/PLAN.md` ve `docs/RULES.md`'ye link), nasıl çalıştırılır. Üçüncü tarafa ait ad geçmez. `Backlog.md`: başlık ve kısa açıklama. `CLAUDE.md` "Komutlar" bölümü: test, Unity kontrolü, APK, push öncesi tarama komutları (yalnız bu bölümü değiştir).
10. **Gemini modelleri.** `agy`'nin yardım çıktısından model listeleme yolunu bul ve listeyi aktar. Model çağıran bir istem gönderme.
11. **Git.** `git init`, varsayılan dal `main`. `git status --ignored` çıktısında yerel dosyalar ve `Library/` dışlanmış, `tests/Core/Core.csproj` ve `.meta` dosyaları dahil olmalı; çıktıyı aktar. İlk commit: İngilizce, kısa mesaj, sonuna `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`. Commit'ten önce `scripts/prepush-scan.ps1`'i staged duruma karşı çalıştır. Push yapma.

## Bitti tanımı
- `dotnet test` yeşil.
- `scripts/unity-check.ps1` temiz projede 0, hatalı dosyada sıfır olmayan kodla çıkıyor.
- `scripts/prepush-scan.ps1` deneme dosyasını yakalıyor, temiz durumda geçiyor.
- C# sürümü kanıtıyla doğrulandı, test projesi sabitlendi.
- APK üretildi (ya da Android modülü yoksa bu açıkça raporlandı).
- `git status --ignored` beklenen gibi; ilk commit atıldı, push yok.

## Rapor
`reports/00-setup.md`. Her iş için: ne yaptın, çalıştırdığın komut, çıktının ilgili kısmı aynen (kısaltırsan "kısaltıldı" yaz). Sonra üç ayrı liste: yapamadıkların, emin olmadıkların, orchestrator'ın karar vermesi gerekenler. Raporu commit'e dahil et.
