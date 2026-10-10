# F4 işletim, yedek ve kabul

Windows kurulumu kullanıcı isteğiyle ertelendi. Aşağıdaki hedef kurulum adımları henüz Windows üzerinde çalıştırılmadı. Mevcut .env, hesap, DB ve volume korunur; ilk kurulum adımları mevcut kurulumda tekrarlanmaz.

## Raporlar

Raporlar menüsünde varsayılan bugün; iki tarih dahil, Türkiye saati kullanılır. İade ve iptal kendi işlem gününde görünür. Önceki gün satışının bugünkü iadesi bugünün farkını eksi yapabilir. Nakit/kart ayrımı her belgenin kendi yöntemidir. Satış − iade kasa bakiyesi veya kâr değildir. Tutarlar KDV dahil ve indirim sonrasıdır; ürün miktarları kutu/adet/gram ayrı tutulur. Pasif ürünler geçmiş satış raporunda görünür. Ürün adı güncel karttan gösterilir; fişlerdeki tarihsel adlar değişmez.

Stok raporu açıldığı andaki bakiyedir. Yalnız biten stok ve pasifleri dahil et filtreleri vardır. Geçmiş stok değeri, maliyet/kâr, Excel aktarımı veya yazdırma eklenmedi.

## Saatlik yerel yedek

Hedef: çalışan DB ve yedek servisi için saatte bir custom pg_dump; 30 gün saklama. Gerçek aralık kontrol/işlem süresinden dolayı bir saati biraz aşabilir. Bilgisayar kapalıysa yedek alınamaz; yeniden açılışta geciken tek güncel yedek alınır. Başarısızlıkta 60 saniyede tekrar denenir. Disk dolması veya servis kesintisi kayıp süresini artırır; bir saat kesin garanti değildir.

Yedek dizini `backups/automatic/`; tarihli dump ve `.sha256` dosyaları Git dışındadır. İlk etkinleştirme mevcut imajlar hazırken depo kökünde:

```sh
docker compose -f compose.yaml -f compose.app.yaml -f compose.backup.yaml config --quiet
docker compose -f compose.yaml -f compose.app.yaml -f compose.backup.yaml up -d --pull never backup
docker compose -f compose.yaml -f compose.app.yaml -f compose.backup.yaml ps
docker compose -f compose.yaml -f compose.app.yaml -f compose.backup.yaml logs --tail 20 backup
```

Bu servis Mac normal kurulumunda etkinleştirilmedi; izole test projesinde çalıştırıldı. Hedef Windows bilgisayarında bu komutlar, klasör ACL'leri ve Docker açılışı ayrıca doğrulanacak. Saatlik servis tek kopya çalıştırılır; aynı dizine paralel manuel yedek işçisi başlatılmaz. Durdurmak volume silmez:

```sh
docker compose -f compose.yaml -f compose.app.yaml -f compose.backup.yaml stop backup
```

`healthy` son başarı zamanının 65 dakikadan eski olmadığını gösterir. `unhealthy` veya başarısızlık günlüğünde disk alanı, Docker/DB durumu ve dizin izinlerini kontrol et. Docker sağlık durumu tek başına bildirim göndermez; günlük durum kontrolü gerekir. Yedek arşiv listesi okunup checksum üretilmeden başarı zamanı yenilenmez. Bu kontrol tüm verinin kurtarılabildiğinin kanıtı değildir; ayrı DB provası gerekir.

30 günden eski yalnız `gurboya-auto-*.dump` ve eş checksum dosyaları, başarılı yeni yedekten sonra silinir. Manuel/yükseltme yedekleri temizlenmez. Saatlik 30 gün yaklaşık 720 tam yedektir; gereken alan DB boyutuna bağlıdır ve Windows disk bilgisi geldiğinde ölçülecek. Aynı bilgisayar/disk arızası yedekleri de kaybettirebilir; kullanıcı harici kopya seçmedi. Yedek kullanıcı parola özetlerini ve işletme verisini içerir; klasöre yalnız yetkili OS hesabı erişmelidir. Windows ACL kontrolü bekliyor.

## Kurtarma provası ve arıza planı

1. Son başarılı dump ile checksum dosyasını seç. SHA-256 değerini Windows `Get-FileHash` veya Mac `shasum -a 256` çıktısıyla karşılaştır. Eşleşmiyorsa arşivi kullanma; önceki sağlam yedeği değerlendir.
2. [README](README.md) ayrı veritabanına dönüş adımlarını benzersiz yeni DB adı ve yeni konteyner geçici dosyası ile uygula. Canlı DB üzerine restore, `--clean` ve volume silme yoktur. Her komutun başarılı olmasını bekle.
3. Kaynak/yedek anına ait ürün/stok, satış/iade sayıları ve tutarları, migration geçmişi ve stok hareketi=bakiye mutabakatını doğrula. Prova DB'si inceleme için korunur; silinmesi ayrı işlemdir.
4. Dump sunucu rollerini, .env sırlarını, uygulama imajını ve Data Protection anahtarlarını içermez. Sırları güvenli yerel konumda ayrıca koru; ortak belgelere/Git'e yazma. Yeni makine kurtarmasında eşleşen uygulama sürümü ve rol/yetki düzeni kurulmalı; geçmiş tablolarda UPDATE/DELETE ve web DDL yasakları yeniden doğrulanmalı. Anahtar kaybı eski oturumları geçersiz kılar.
5. Gerçek arızada mevcut volume/dosyaları koru, yazmayı durdur, yeni ayrı kurulumda kurtar ve doğrula. Canlı bağlantıyı kurtarılan DB'ye yönlendirme ayrı, açıkça incelenmiş bakım adımıdır; bu rehber onu otomatik çalıştırmaz. Kullanıcı girişini sıfırlama; yedekteki hesabı kullan.

İlk Windows kurulumunda, ardından düzenli olarak ve büyük güncellemeler öncesi ayrı DB kurtarma provası yapılmalı. Kurtarma süresi hedefi Windows pilotunda ölçülecek.

## Bekleyen gerçek kabul

- F3 satış/indirim/renklendirme/iade/iptal ve F4 raporlarının kullanıcı ekran kabulü.
- Hedef Windows sürümü, işlemci/mimari, RAM, boş disk ve Docker/WSL uygunluğu.
- Windows üzerinde uygulama açılışı, hesap/veri erişimi ve yedek klasörü izinleri.
- Gerçek internet bağlantısı kesikken açılış, ürün/satış/rapor ve yedek denemesi.
- Windows yeniden başlatma ve kullanıcı oturumu sonrası Docker, uygulama ve geciken yedeğin otomatik gelmesi.
- Windows üzerinde seçilen yedeğin ayrı DB'ye dönüşü ve içerik mutabakatı.

Bu adımlar gerçekten yapılmadan F4 veya canlı kullanım tamamlandı sayılmaz. Mac HTTP testleri görsel kabul değildir.
