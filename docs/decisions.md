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

Kullanıcı arayüzü ve sunucu stack seçimi K13 ile kesinleşti. F3 iş kuralları aşağıdaki K19–K23 ve takip yanıtlarıyla netleşti. Kalanlar: Windows sürümü/donanımı, F4 rapor kapsamı, yedek saklama ve kayıp toleransı. PostgreSQL tekrar seçim beklemiyor.

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

F2 teknik ayrıntıları: marka/renk/kategori ürün üzerinde serbest metindir, önceden tanımlı seçenek listesi zorunlu değildir. Ambalaj/birim farkları ayrı ürün kartında izlenir; hareket oluşmuş kartın türü/birimi/litresi değişmez. Fiyatlar birim başına decimal/numeric dört ondalık basamakta, orta değer sıfırdan uzağa yuvarlanır; KDV hariç + KDV = dahil toplam korunur. Aynı KDV seçimi kartın alış ve satış fiyatına uygulanır. Fiş/satır indirimi ve satış kuruşları aşağıdaki F3 kararlarında netleştirildi.

## F3 kullanıcı kararları — 10 Ekim 2026

| Kimlik | Onaylanan kural |
|---|---|
| K19 | İndirim yüzde veya TL tutarı girilerek yapılabilmeli. Hem ürün satırında hem fiş toplamında kullanılacak. |
| K20 | Gerekli yuvarlama en az düzeyde, yalnız kuruşlarda ve müşteri yararına yapılmalı. Satışta kuruş altı toplam aşağı, kısmi iadede yukarı yuvarlanır; toplam iade özgün bedeli aşamaz. |
| K21 | Renklendirilmiş boya iadesi kabul edilmez. |
| K22 | Kayıtlı boya türleri için “Renklendirme ücreti ekle” düğmesi ve elle ücret girişi isteniyor. Kullanıcı KDV eklenmiş ücreti elle girer; yeniden KDV eklenmez. Satış satırının tamamına ek tutar olarak uygulanır. |
| K23 | Ödeme uygulama dışında yapılır; uygulama satışın tamamlanma durumunu ve nakit/kart bilgisini tutar. POS entegrasyonu istenmiyor. Satış yalnız ödeme alındığında nakit veya kart seçilerek tamamlanır; öncesinde stok değişmez. |

Takip yanıtları: indirim her iki düzeyde; renklendirme ücreti KDV dahil; sağlam iade stoğa döner, hasarlı iade para iadesi oluşturur ama satılabilir stoğu artırmaz. Ödeme sonrası tamamlama ve müşteri lehine kuruş kuralları onaylandı.

F3 uygulama ayrıntıları: satır indirimi boya/ürün ve varsa renklendirme ücreti toplamına, fiş indirimi satır indirimlerinden sonra oransal uygulanır. Fiş toplamı yalnız kuruş altı kadar aşağı yuvarlanır; kuruşlar en büyük kalan yöntemiyle satırlara dağıtılır. Böylece her satırda ayrı ayrı aşağı yuvarlamanın biriken kaybı önlenir. Net iki basamakta en yakına, KDV toplam−net olarak hesaplanır. İadede toplam iade miktarının özgün satır tutarındaki payı yukarı yuvarlanır; önceki iadeler düşülür, net/KDV kalan bileşenleri aşmaz. Renklendirme tutarı sıfır olsa da renklendirildi işareti iade ve iptali engeller. İptal kalan satırların tamamını sağlam olarak stoğa döndüren değişmez ters belgedir; renklendirilmiş satır varsa bütün iptal reddedilir. Ücret kartın fiyatını kalıcı değiştirmez. Cari, veresiye, parçalı tahsilat ve ödeme cihazı entegrasyonu eklenmez.

## F4 kullanıcı kararları — 10 Ekim 2026

- K24: Raporlarda Excel aktarımı ve yazdırma istenmiyor. Önerilen mevcut/biten stok, tarih aralıklı satış/iade/fark, nakit/kart ve ürün miktarları temel kapsam olarak uygulandı; kapsamın görsel kabulü bekliyor.
- K25: Hedef Windows bilgisayarına şu an erişim yok; kurulum kullanıcı isteğiyle ertelendi. Sürüm/donanım henüz bilinmiyor.
- K26: Yedekler hedef bilgisayarda yerel tutulacak. Kullanıcı saatlik yedek, yaklaşık bir saatlik işlem kaybı hedefi ve 30 gün saklamayı seçti. Aynı disk arızası yerel yedekleri de kaybettirebilir; harici kopya seçilmedi. Hedef, servis/DB açık ve son yedek başarılıyken geçerlidir; kesin kayıp garantisi değildir.
- F3 ekranları kullanıcı tarafından henüz kontrol edilmedi. Görsel kabul ve F4 pilot kapanışı bekliyor.
