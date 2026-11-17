# Spec 07c: Patlama hissi

Durum: onaylandı (2026-11-15). Dal: `step/07c-blast-feel`. Yalnız görüntü katmanı (`Assets/Scripts/View/`); kural katmanı ve testler değişmez. Ürün sahibi kararı: "bir deneyelim, olmazsa başka bir şey deneriz". Süreler hedeftir; ürün sahibi telefonda deneyip ayarlatır.

## Amaç
Şu an grup tek seferde yok oluyor ("güm"). Patlama göz tarafından izlenebilsin, hedefe sayılan her şey hedefe gittiğini göstersin, yeni kareler üstten dökülsün.

## Davranış
1. **İçten dışa patlama (grup):** dokunulan kare önce patlar. Ardından gruptaki kareler, dokunulan kareye grup içindeki adım uzaklığına göre (4 komşu üzerinden halka halka) sırayla patlar. Halka başına gecikme ~0.03 sn. Toplam yayılma ~0.25 sn'yi geçmez; büyük grupta halka gecikmesi kısalır. Her kare kısa bir büyüyüp kaybolma yapar (mevcut `BlastDuration` ölçüsünde). Komşu engele vuruş, vuran ilk komşu kare patladığı anda görünür. Grup özel öğe doğuruyorsa mevcut toplanma animasyonu kalır (bu madde normal grup patlaması içindir).
2. **Hedefe uçuş:** level modunda hedefe sayılan her öğe (hedef rengindeki renkli kare, kırılan Pinyata, son katı kırılan Sandık, patlayan Su balonu, toplanan Çapa) patladığı/kırıldığı yerden kendi görünüşünün bir kopyasıyla HUD'daki hedef simgesine uçar. Uçarken küçülür (~1'den ~0.4'e) ve hızlanır (yavaş başlar, ivmelenerek varır; ease-in), süre ~0.45 sn. Kopyalar art arda çok kısa aralıklarla kalkar. Hedef sayacı her kopya vardığında 1 azalır. Simge varışta kısa bir "pıt" büyümesi yapar. Hedefe sayılmayan öğeler uçmaz. Sonsuz oyunda (bayrak açıkken) uçuş yoktur. Uçuşlar HUD tarafında çizilir, kamera sınırında kesilmez (Adım 7 Çapa uçuşu gibi). Nesneler havuzdan alınır.
3. **Üstten dökülme:** yeni kareler tahtanın üstünden düşerek yerine gelir. Düşen kareler (mevcut ve yeni) aynı anda değil, sütun içinde alttan üste çok kısa gecikmeyle (~0.02 sn) dökülür, böylece akış gibi görünür. Düşme eğrisi mevcut kübik ease-in kalır. Yeni karelerin tahta dışındaki kısmı görünmez (maske).
4. **Kazan/kaybet zamanlaması:** kazan/kaybet paneli, son hamlenin uçuşları ve dökülmesi bittikten sonra açılır.
5. Dokunuş, animasyon bitene kadar kapalı kalır (mevcut davranış).

## Kapsam dışı
Özel öğe ve kombo animasyonları (değişmez), ses, kural değişikliği, düşerken dokunma (Backlog).

## Elle kontrol listesi
1. `scripts/unity-check.ps1` 0, `dotnet test` yeşil (değişmemiş olmalı).
2. Büyük bir grupta patlama dokunulan yerden halka halka yayılıyor, toplam kısa.
3. Hedef rengindeki kareler ve hedef engeller patladığı yerden küçülüp hızlanarak hedef simgesine uçuyor; sayaç vardıkça azalıyor; hedef olmayanlar uçmuyor.
4. Yeni kareler üstten akarak dökülüyor; tahtanın üstünde görünmüyor.
5. Kazan/kaybet paneli uçuşlar bittikten sonra açılıyor. Console temiz, sızıntı yok, akıcı (iPhone).
6. WebGL `gh-pages`'e; ürün sahibi iPhone'da dener.

## Akış (küçük görüntü adımı)
1. Spec → `docs(spec): add blast feel spec`.
2. Kodcu (Opus 5.5 medium): uygulama → `feat(view): ...`. Test aşaması yok (kural katmanı değişmez).
3. Claude kontrolcü (Opus medium, okuyarak; `tests/` ve `Core/` diff'i boş mu, spec maddeleri, sızıntı/havuz). Gemini yok (görüntü adımı).
4. WebGL önizleme, ürün sahibi dener, gerekirse süreler ayarlanır.
5. Tarama, push, PR, CI, merge.
