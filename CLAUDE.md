# BlastGame

Mobil "blast" (collapse/match-2) oyunu. Unity 6000.6.3f1, C#. Public portföy projesi; son karar ürün sahibinin. Ad geçici.

## Önce oku
- `docs/RULES.md`: herkesin ve her rolün kuralları. Her ajan işe başlamadan okur.
- `docs/PLAN.md`: mimari, roller ve modeller, adım kapısı, adımlar, riskler, repo düzeni.
- `docs/specs/`: adım başına spec. Yalnız üzerinde çalıştığın adımınkini oku.
- `docs/tasks/`: ajanlara verilen görev tarifleri.
- `reports/`: kontrolcü raporları.
- `Backlog.md`: sonraya bırakılan fikirler.

## Asla unutma
- Ana oturum orchestrator'dır: oyun kodu, stub, test, kurulum dosyası yazmaz; yaptırır ve denetler.
- Üçüncü taraf malzeme yayımlanmaz (`docs/RULES.md`, kural 1).
- Ürün sahibi soru sorduğunda açıkla ve bekle. Planda ya da onaylı spec'te geçen işlem için yeniden onay isteme; riskli işlemler (`docs/RULES.md`, Onay) hariç.
- Yeni terimi kullanmadan önce bir cümleyle, sade bir benzetmeyle açıkla.

## Komutlar
Repo kökünden çalıştır. Unity komutlarında Editor kapalı olmalı. PowerShell scriptleri için `-ExecutionPolicy Bypass` gerekir (makinede script çalıştırma kapalı).
- Test: `dotnet test tests/BlastGame.Tests.slnx`
- Unity derleme kontrolü: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/unity-check.ps1` (0 = temiz, 1 = derleme hatası; log `Logs/unity-check.log`)
- APK: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-android.ps1` (çıktı `Builds/Android/BlastGame.apk`; log `Logs/build-android.log`; ortam notu için `reports/00-setup.md`)
- WebGL derleme: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-webgl.ps1` (çıktı `Builds/WebGL/index.html`; log `Logs/build-webgl.log`; sıkıştırma kapalı, Pages için). Ajan sandbox'ında Android derlemesi Gradle loopback hatası verir; APK ve WebGL derlemesi için Terminal panelini kullan.
- WebGL yayın (yerel `gh-pages` dalına commit, push yapmaz): `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/deploy-webgl.ps1`; sonra `git push origin gh-pages` (orchestrator, tarama sonrası). Yerel deneme: `python -m http.server 8765 --directory Builds/WebGL` (`file://` ile açılmaz).
- Push öncesi tarama (üç mod; `-ExecutionPolicy Bypass` ile): `scripts/prepush-scan.ps1` (varsayılan: staged, commit öncesi), `scripts/prepush-scan.ps1 -All` (HEAD'deki bütün izlenen dosyalar, ilk push), `scripts/prepush-scan.ps1 -Range origin/main..HEAD` (sonraki push'lar). Tam komut: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/prepush-scan.ps1 [-All | -Range <aralık>]` (0 = temiz, 1 = bulgu, 2 = tarama çalışmadı)
