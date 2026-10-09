# GürBoya — Faz planı

Durum: F1-A Mac ARM64 kabul testleri tamamlandı; Windows/AMD64 doğrulaması bekliyor. F1-B stack seçimi onaylandı; Razor Pages/EF Core/Npgsql iskeleti ve Mac ARM64 kabul testleri tamamlandı. Son kullanıcı hesapları mağaza verisi girişinden önce netleştirilecek. Diğer fazların ayrıntıları açık iş kuralları ve stack onayına bağlı taslak plandır. Tarih/süre taahhüdü verilmedi.

| Faz | Amaç ve kapsam | Bağımlılık | Teknik görev ve bitiş/test koşulu |
|---|---|---|---|
| F0 | Gereksinim ve yerel mimari hazırlığı | Kullanıcı yanıtları | Yerel web/PostgreSQL/Docker onaylandı; kalan kararlar kaydedildi. Tüm tasarım onaylanmış değildir. |
| F1-A | Taşınabilir veritabanı geliştirme altyapısı | Docker erişimi | Compose, sabit PostgreSQL imajı, volume, healthcheck, sırsız .env.example; bağlantı ve yeniden oluşturma sonrası kalıcılık testi, yedekten ayrı DB'ye dönüş testi. |
| F1-B | Uygulama iskeleti | Stack/ORM ve hesap yaklaşımı onayı, F1-A | Arayüz/API sınırı, Türkçe durum/hata, DB erişimi, migration altyapısı; açılış, bağlantı hatası ve migration testleri. |
| F2 | Kullanılabilir ürün ve stok | Ürün/birim/negatif stok kuralları | Ürün/marka/kategori, kutu hacmi, adet/gram, fiyatlar, stok giriş/sayım/düzeltme; kesirli kutu reddi, geçmiş koruması ve stok mutabakatı testleri. |
| F3 | Satış ve düzeltme | F2; vergi/indirim/iade kararları | Satış, renk kodu, stok çıkışı, tekrar güvenliği, iptal/iade; eşzamanlılık, rollback, geçmiş fiyat, fazla iade engeli testleri. |
| F4 | İşletme pilotu ve yedek | F3; cari/tahsilat ihtiyacı kararı | Temel stok/satış raporu, Windows dağıtımı, kullanıcı yetkileri, günlük yedek, kurtarma; internet kapalı açılış ve gerçek kullanıcı kabulü. |
| F5 | İhtiyaca göre genişleme | Pilot geri bildirimi | Cari, tedarikçi, alış belgeleri, Excel aktarımı, etiket/teklif; her modül için ayrı onay, tasarım ve test. |

## MVP önerisi

Elle ürün/fiyat girişi, kutulu boya ve adet/gram stokları, satış/renk kodu, iade/düzeltme, temel listeler, erişim kontrolü ve yedekten kurtarma. Pigment/reçete yoktur. Cari/veresiye dükkân için zorunluysa F5'e ertelenemez; pilot öncesi MVP'ye alınır. Tahsilat kapsamı netleşmeden satış toplamı kasa bakiyesi olarak gösterilmez. Maliyet yöntemi netleşmeden brüt kâr raporu üretilmez.

## İlk görev F1-A

Yalnızca PostgreSQL altyapısı oluştur. Mevcut belgeleri oku; cihaz/Docker/Compose durumunu denetle. Desteklenen sabit PostgreSQL sürümü ve iki mimaride imajı doğrula; seçim gerekçesini kaydet. compose.yaml, sırsız .env.example ve gereken küçük yardımcı dosyaları oluştur. Veritabanını dış ağa açma. Geliştirme portu gerekiyorsa 127.0.0.1 kullan. Bu fazda uygulama framework'ü veya iş tabloları oluşturma.

Test DB'sinde veri yaz, konteyneri volume silmeden yeniden oluştur ve veriyi doğrula. pg_dump/pg_restore ile ayrı DB'ye prova yap; gerçek veriye dokunma. Compose doğrulamasında sır içeren çözülmüş yapılandırmayı loga dökme. Windows testi bu cihazda yapılamıyorsa bekliyor olarak kaydet. ARM64 başarısını iki platform testi diye sunma. Kurulum/çalıştırma/yedek komutlarını docs/README.md'ye ekle.

F1-A bittiğinde değişiklikleri/testleri kaydet ve commit oluştur; F1-B için kısa stack önerisini kullanıcıya sun. Stack kararı verilmeden bir framework seçip uygulama geliştirmeye geçme.

## F1-B uygulama kapsamı

Onaylanan Razor Pages + EF Core + Npgsql ile tek web projesi. Türkçe durum/hata ekranları, yerel CSS, canlılık/hazırlık HTTP uçları, ayrı uygulama/migration DB hesapları ve açık migration komutu. İlk migration iş tablosu oluşturmaz. Açılışta DDL yapılmaması, eksik migration, DB kesintisi/toparlanma, migration tekrar güvenliği ve uygulama yetki sınırları gerçek PostgreSQL üzerinde test edilir. Son kullanıcı girişi ve iş modülleri bu iskelete dahil değildir.
