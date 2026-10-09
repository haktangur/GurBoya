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
| K13 | Kullanıcı ASP.NET Core Razor Pages + EF Core + Npgsql önerisini onayladı ve F1-B geliştirmesine devam edilmesini istedi. |
| K12 | GitHub origin https://github.com/haktangur/GurBoya.git; doğrulanmış değişikliklerin normal push ile gönderilmesi yetkilendirildi. |

K11 gerekçesi: PostgreSQL resmî imajının ARM64 ve AMD64 desteği, M3 üzerinde MSSQL emülasyonu ve ayrı makine ihtiyacını ortadan kaldırır. MSSQL emülasyonu ve uzak geliştirme veritabanı alternatifleri artık aktif plan değildir.

## Uygulama hazırlığında önerilen seçimler

Bunlar kullanıcı tarafından ayrı ayrı seçilmiş kararlar değildir; ilgili faz öncesi değerlendirilir: modüler monolit, ambalaj başına ürün kartı, stok hareket defteri ve transaction içinde güncellenen stok bakiyesi, fiyat geçmişi, günlük harici yedek. Ayrıntılar mimari ve database belgelerindedir.

Belgeleri sürümleme yaklaşımı: project_context.md ve progress.md dahil bütün ortak belgeler Git'te tutulacak. Yerel tutmak hassas bilgiyi paylaşmama avantajı verse de cihaz/sohbet değişiminde bağlam kaybı ve ekipte tutarsızlık doğurur. Bu proje için sürümleme tercih edildi; kişisel/hassas notlar .local-notes/ altında Git dışında kalır. Bu bir dokümantasyon düzenleme tercihidir, kullanıcı onaylı mimari karar olarak sunulmaz.

## Karar bekleyenler

Kullanıcı arayüzü ve sunucu stack seçimi K13 ile kesinleşti. Kalanlar: Windows sürümü ve donanımı; iade ve renklendirme ücreti; satış indirimi/fiş yuvarlaması; cari ve tahsilat ihtiyacı; yedek saklama ve kayıp toleransı. PostgreSQL tekrar seçim beklemiyor.

## F1-B teknik uygulama seçimleri

.NET SDK 10.0.401, ASP.NET Core runtime 10.0.9, EF Core/dotnet-ef 10.0.9 ve Npgsql EF sağlayıcısı 10.0.3 sabitlendi. SDK/runtime imajlarının çok mimarili digestleri Dockerfile içinde; NuGet bağımlılık grafiği packages.lock.json içinde tutulur. Bunlar onaylanan stack'in uygulama sürümleridir, ayrı kullanıcı iş kuralı kararı değildir.

DB yönetim hesabı, gurboya_migrator ve gurboya_app ayrıldı. Migration açık bakım komutuyla yapılır; web açılışı DDL çalıştırmaz. F1-B yalnızca durum ekranı içerir. F1-B’de kimlik yaklaşımı bekliyordu; K17 ile tek şifreli hesap onaylandı.

## F2 kullanıcı kararları — 9 Ekim 2026

| Kimlik | Onaylanan kural |
|---|---|
| K14 | Boyada marka, ambalaj boyutu, renk ve fiyat elle girilir. Boya boyutu litre bilgisidir; stok/fiyat kutu başınadır. |
| K15 | Gram hassasiyeti 1 gramdır. Diğer ürünlerde de kutu seçilebilir (ör. bir kutu çivi); kutu/adet/gram tam sayıdır. |
| K16 | Yetersiz stokta işlem engellenir ve “Stokta yok” uyarısı gösterilir. Yeni stok girişi sonrası ürün tekrar kullanılabilir. |
| K17 | Tek kişi için şifreli yönetici hesabı kullanılır. |
| K18 | TL fiyat girişinde “KDV ekle” / “KDV dahil” seçilir; KDV hariç fiyat, KDV tutarı ve toplam ayrı görünür. Başlangıç oranı kullanıcı isteğiyle %15; oran ürün kartından değiştirilebilir. |

F2 teknik ayrıntıları: marka/renk/kategori ürün üzerinde serbest metindir, önceden tanımlı seçenek listesi zorunlu değildir. Ambalaj/birim farkları ayrı ürün kartında izlenir; hareket oluşmuş kartın türü/birimi/litresi değişmez. Fiyatlar birim başına decimal/numeric dört ondalık basamakta, orta değer sıfırdan uzağa yuvarlanır; KDV hariç + KDV = dahil toplam korunur. Aynı KDV seçimi kartın alış ve satış fiyatına uygulanır. Fiş/satır indirimi ve satış toplamının kuruş yuvarlaması F3 öncesinde ayrıca netleştirilecek.
