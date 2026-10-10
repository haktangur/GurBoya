# GürBoya — Faz planı

Durum: F1-A Mac ARM64 kabul testleri tamamlandı; Windows/AMD64 doğrulaması bekliyor. F1-B stack seçimi onaylandı; Razor Pages/EF Core/Npgsql iskeleti ve Mac ARM64 kabul testleri tamamlandı. Tek şifreli yönetici hesabı K17 ile onaylandı; F2 Mac ARM64 kabul testleri tamamlandı. F3 iş kuralları onaylandı ve satış/iade uygulandı. F4 ayrıntıları pilot kararlarına bağlıdır. Tarih/süre taahhüdü verilmedi.

| Faz | Amaç ve kapsam | Bağımlılık | Teknik görev ve bitiş/test koşulu |
|---|---|---|---|
| F0 | Gereksinim ve yerel mimari hazırlığı | Kullanıcı yanıtları | Yerel web/PostgreSQL/Docker onaylandı; kalan kararlar kaydedildi. Tüm tasarım onaylanmış değildir. |
| F1-A | Taşınabilir veritabanı geliştirme altyapısı | Docker erişimi | Compose, sabit PostgreSQL imajı, volume, healthcheck, sırsız .env.example; bağlantı ve yeniden oluşturma sonrası kalıcılık testi, yedekten ayrı DB'ye dönüş testi. |
| F1-B | Uygulama iskeleti | Stack/ORM ve hesap yaklaşımı onayı, F1-A | Arayüz/API sınırı, Türkçe durum/hata, DB erişimi, migration altyapısı; açılış, bağlantı hatası ve migration testleri. |
| F2 | Kullanılabilir ürün ve stok | Ürün/birim/negatif stok kuralları | Ürün/marka/kategori, kutu hacmi, adet/gram, fiyatlar, stok giriş/sayım/düzeltme; kesirli kutu reddi, geçmiş koruması ve stok mutabakatı testleri. |
| F3 | Satış ve düzeltme | F2; vergi/indirim/iade kararları | Satış, renk kodu, stok çıkışı, tekrar güvenliği, iptal/iade; eşzamanlılık, rollback, geçmiş fiyat, fazla iade engeli testleri. |
| F4 | İşletme pilotu ve yedek | F3; cari/tahsilat ihtiyacı kararı | Temel stok/satış raporu, Windows dağıtımı, kullanıcı yetkileri, günlük yedek, kurtarma; internet kapalı açılış ve gerçek kullanıcı kabulü. |
| F5 | İhtiyaca göre genişleme | Pilot geri bildirimi | Cari, tedarikçi, alış belgeleri, Excel aktarımı, etiket/teklif; her modül için ayrı onay, tasarım ve test. |

## F3 sonrası kalan fazlar

F3 geliştirme ve Mac ARM64 otomatik kabul testleri tamamlandı. İlk sürümün işletmede kabulü için **bir zorunlu faz (F4)** kaldı. **F5 isteğe bağlı genişleme fazıdır**; kendiliğinden başlatılmaz.

- F4: rapor kapsamının netleştirilmesi, temel stok/satış raporları, Windows dağıtımı, yedek/kurtarma, çevrimdışı ve OS açılışı kontrolleri, görsel kullanıcı kabulü.
- F5: pilot geri bildirimine göre ayrıca onaylanan ek modüller.

Kullanıcı uygulamayı kontrol etmek için giriş bilgisini istedi; kontrolün sonucu veya görsel kabulü henüz bildirilmedi. F3 teknik tamamlanması, kullanıcının ekranları onayladığı veya Windows canlı kullanımının kabul edildiği anlamına gelmez.

## MVP önerisi

Elle ürün/fiyat girişi, kutulu boya ve adet/gram stokları, satış/renk kodu, iade/düzeltme, temel listeler, erişim kontrolü ve yedekten kurtarma. Pigment/reçete yoktur. Cari/veresiye dükkân için zorunluysa F5'e ertelenemez; pilot öncesi MVP'ye alınır. Tahsilat kapsamı netleşmeden satış toplamı kasa bakiyesi olarak gösterilmez. Maliyet yöntemi netleşmeden brüt kâr raporu üretilmez.

## İlk görev F1-A

Yalnızca PostgreSQL altyapısı oluştur. Mevcut belgeleri oku; cihaz/Docker/Compose durumunu denetle. Desteklenen sabit PostgreSQL sürümü ve iki mimaride imajı doğrula; seçim gerekçesini kaydet. compose.yaml, sırsız .env.example ve gereken küçük yardımcı dosyaları oluştur. Veritabanını dış ağa açma. Geliştirme portu gerekiyorsa 127.0.0.1 kullan. Bu fazda uygulama framework'ü veya iş tabloları oluşturma.

Test DB'sinde veri yaz, konteyneri volume silmeden yeniden oluştur ve veriyi doğrula. pg_dump/pg_restore ile ayrı DB'ye prova yap; gerçek veriye dokunma. Compose doğrulamasında sır içeren çözülmüş yapılandırmayı loga dökme. Windows testi bu cihazda yapılamıyorsa bekliyor olarak kaydet. ARM64 başarısını iki platform testi diye sunma. Kurulum/çalıştırma/yedek komutlarını docs/README.md'ye ekle.

F1-A bittiğinde değişiklikleri/testleri kaydet ve commit oluştur; F1-B için kısa stack önerisini kullanıcıya sun. Stack kararı verilmeden bir framework seçip uygulama geliştirmeye geçme.

## F1-B uygulama kapsamı

Onaylanan Razor Pages + EF Core + Npgsql ile tek web projesi. Türkçe durum/hata ekranları, yerel CSS, canlılık/hazırlık HTTP uçları, ayrı uygulama/migration DB hesapları ve açık migration komutu. İlk migration iş tablosu oluşturmaz. Açılışta DDL yapılmaması, eksik migration, DB kesintisi/toparlanma, migration tekrar güvenliği ve uygulama yetki sınırları gerçek PostgreSQL üzerinde test edilir. Son kullanıcı girişi ve iş modülleri bu iskelete dahil değildir.

## F2 onaylı kapsam — 9 Ekim 2026

Boya: elle marka/litre/renk/fiyat. Diğer ürünler: kutu/adet/gram; tüm miktarlar tam sayı. Ürün kartı, arama, pasifleştirme, giriş/düzeltme/sayım, negatif stok engeli, fiyat/stok geçmişi. Fiyat: TL, KDV ekle/dahil, varsayılan %15, değiştirilebilir oran ve ayrı net/KDV/toplam. Tek yönetici girişi/şifre değişimi ve CSRF korumalı formlar. Kabul: fiyat örnekleri, kesirli miktar reddi, çift gönderim, eşzamanlı son stok, rollback, stok mutabakatı, pasif/yeniden aktif ürün, geçmiş ve yedekten dönüş. F3 satış fişi bu kapsamda yoktur.


## F3 onaylı kapsam — 10 Ekim 2026

K19–K23 ve takip yanıtlarıyla satış, satır/fiş TL-yüzde indirim, müşteri lehine fiş kuruşu, elle KDV dahil renklendirme ücreti ve renk kodu, nakit/kart tamamlama, iade/iptal uygulandı. Renklendirilmiş boya iade/iptal edilemez. Sağlam iade stoğa döner, hasarlı iade para iadesiyle sınırlı kalır. Cari, parçalı ödeme ve POS entegrasyonu yoktur. Gerçek PostgreSQL/HTTP testleri tekrar, yarış, rollback, geçmiş fiyat, iade sınırı ve yedekten dönüşü kapsar. Sonuçlar progress.md'de; sıradaki iş F4 pilot hazırlığıdır.

## F4 uygulama durumu — 10 Ekim 2026

Temel stok/satış raporları ve saatlik yerel/30 günlük yedek servisi geliştirildi. Excel/yazdırma yok. Windows kurulumu kullanıcı isteğiyle ertelendi; donanım bilgisi ve hedef bilgisayara erişim bekleniyor. Mac otomatik testlerinin geçmesi F4 kapanışı sayılmaz. F3/F4 görsel kabulü, Windows/AMD64 çalışma, OS açılışı ve gerçek internet kesintisi henüz bekliyor. Bkz. [işletim ve kabul rehberi](f4_operations.md).
