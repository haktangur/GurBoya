# GürBoya — Karar kayıtları

## Onaylı kullanıcı kararları — 8 Ekim 2026

| Kimlik | Karar |
|---|---|
| K01 | Ad GürBoya; yetkili proje klasöründe çalış; Markdown belgelerini docs/ altında topla. |
| K02 | Tek dükkân ve tek bilgisayar için Türkçe web uygulaması. |
| K03 | Yerel veritabanı; günlük işlemler internetsiz yapılabilmeli; bulut gerekmiyor. |
| K04 | Önceki MSSQL tercihi K11 ile değiştirildi; tarihsel karar, artık uygulanmaz. |
| K05 | Elle ürün, marka, fiyat, miktar girişi; kutulu boyada litre bilgisi, diğer ürünlerde adet veya gram. |
| K06 | Hazırlık bu sohbette, uygulama geliştirme VS Code içindeki Codex'te yapılacak. |
| K07 | Dükkân Windows; geliştirme cihazı M3 MacBook Air. |
| K08 | Boyalar farklı marka ve hacimlerde kapalı kutu; ambalaj hacmi ayrı alan. |
| K09 | Boya ve renk kodu yeterli; pigment stoğu takip edilmeyecek. |
| K10 | Kutulu boya fiyatı kutu başına. |
| K11 | PostgreSQL'e geçiş onaylandı; Docker ile Mac ve Windows'ta yerel çalışma hedefi kabul edildi. |
| K12 | GitHub origin https://github.com/haktangur/GurBoya.git; doğrulanmış değişikliklerin normal push ile gönderilmesi yetkilendirildi. |

K11 gerekçesi: PostgreSQL resmî imajının ARM64 ve AMD64 desteği, M3 üzerinde MSSQL emülasyonu ve ayrı makine ihtiyacını ortadan kaldırır. MSSQL emülasyonu ve uzak geliştirme veritabanı alternatifleri artık aktif plan değildir.

## Uygulama hazırlığında önerilen seçimler

Bunlar kullanıcı tarafından ayrı ayrı seçilmiş kararlar değildir; ilgili faz öncesi değerlendirilir: modüler monolit, ambalaj başına ürün kartı, stok hareket defteri ve transaction içinde güncellenen stok bakiyesi, negatif stok engeli, fiyat geçmişi, günlük harici yedek. Ayrıntılar mimari ve database belgelerindedir.

Belgeleri sürümleme yaklaşımı: project_context.md ve progress.md dahil bütün ortak belgeler Git'te tutulacak. Yerel tutmak hassas bilgiyi paylaşmama avantajı verse de cihaz/sohbet değişiminde bağlam kaybı ve ekipte tutarsızlık doğurur. Bu proje için sürümleme tercih edildi; kişisel/hassas notlar .local-notes/ altında Git dışında kalır. Bu bir dokümantasyon düzenleme tercihidir, kullanıcı onaylı mimari karar olarak sunulmaz.

## Karar bekleyenler

Frontend/backend framework'leri ve ORM; tam yazılım sürümleri; kullanıcı hesabı/roller; Windows sürümü ve donanımı; negatif stok; iade ve renklendirme ücreti; KDV/indirim/yuvarlama; cari ve tahsilat ihtiyacı; yedek saklama ve kayıp toleransı. PostgreSQL tekrar seçim beklemiyor.
