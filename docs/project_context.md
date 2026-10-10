# GürBoya — Mevcut bağlam

Son güncelleme: 10 Ekim 2026. Amaç küçük aile dükkânında kutulu boya ve diğer hırdavat ürünlerinin elle stok girişi, fiyat ve satış takibidir. Türkçe arayüz zorunlu. Geliştirme M3 MacBook Air, canlı kullanım tek Windows bilgisayarında olacak.

## Güncel durum

Yerel Windows hedefi, PostgreSQL/Docker ve Razor Pages + EF Core + Npgsql onaylı. F1-A/F1-B tamamlandı. F2 için kullanıcı kararları K14–K18: boya marka/litre/renk/fiyat serbest giriş; diğer ürünlerde kutu/adet/gram; tüm stok miktarları tam sayı; negatif stok engeli; tek şifreli yönetici; TL fiyatlarda KDV ekle/dahil, başlangıç %15 ve ayrı tutar gösterimi.

F2 tamamlandı: ürün oluşturma/düzenleme/arama/pasifleştirme, stok girişi/gerekçeli düzeltme/sayım, fiyat ve stok geçmişi; ASP.NET Identity ile giriş/çıkış/şifre değiştirme. DB trigger'ları hareket ve bakiye bütünlüğünü korur; işlem kimlikleri çift gönderimi engeller. Testler geçti; mevcut F1-B veritabanı yedeklenip ayrı DB’ye dönüş doğrulandıktan sonra normal geliştirme kurulumu güncellendi. localhost:5080 üzerinde giriş yapılabilir; ilk şifre Git dışındaki .local-notes/ilk-giris.txt dosyasında. 9 Ekim teslim kontrolünde normal ürün listesi boştu; kullanıcı sonradan veri girmiş olabilir. Test verileri izole projelerde kaldı. Test sonuçları progress.md, çalıştırma komutları README.md içindedir.

F3 satış/iade uygulandı. K19–K23 ve takip yanıtları: satır/fiş TL veya yüzde indirim, müşteri lehine kuruş hesabı, renklendirilmiş boyada iade/iptal engeli, satış satırına KDV dahil elle renklendirme ücreti, dışarıda alınan ödemenin nakit/kart bilgisi. Sağlam ve hasarlı iadede para iadesi ile stok dönüşü ayrı tutulur. Cari/veresiye/parçalı ödeme veya POS entegrasyonu yoktur. Tamamlanmamış sepet yalnız formda tutulur; ödeme sonrası satış/stok/işlem kaydı tek transaction'da tamamlanır. Pigment modülü kapsam dışıdır.

Sonraki faz F4 işletme pilotu ve yedektir. Windows/AMD64, gerçek internet kesintisi, işletim sistemi açılışı ve görsel kullanıcı kabulü bekliyor. Temel rapor kapsamı, Windows donanımı ve yedek saklama/veri kaybı hedefi F4 öncesinde netleştirilecek.

## Açık sorular ve etkileri

| Konu | Ne zaman çözülmeli? |
|---|---|
| Windows sürümü, RAM ve işlemci | Windows kurulum testi öncesi; Mac'teki F1-A'yı engellemez |
| Temel stok/satış raporlarının kapsamı | F4 öncesi; satış toplamı kasa bakiyesi veya brüt kâr değildir |
| Yedek saklama, harici disk, kabul edilen veri kaybı | Canlı kullanım öncesi |

Mimari teknik taslağı [architecture.md](architecture.md), PostgreSQL modeli [database.md](database.md), faz kapıları [project_plan.md](project_plan.md) içindedir. Yeni sohbet [AGENTS.md](AGENTS.md) okuma sırasını izlemeli, gerçek dosya ve testleri doğrulamalıdır. [Devam promptu](codex_start_prompt.md) ve [oturum devri](next_session.md) F3 kapanışını ve F4 hazırlığını içerir. F1-A/F1-B/F2/F3 tekrar başlatılmaz. Gerçek sonuçları progress.md’den kontrol et.

GitHub origin https://github.com/haktangur/GurBoya.git olarak bağlandı; ilk belge commit’i main dalına gönderildi ve upstream ayarlandı. Kullanıcı sonraki doğrulanmış geliştirme commit’lerinin de GitHub’a gönderilmesini yetkilendirdi. Kişisel verisiz bağlam ve ilerleme belgeleri sürümlenir; hassas notlar .local-notes/ altında tutulur. Tüm Markdown belgeleri docs/ içindedir; başlangıç promptu ajan kurallarını açıkça okutur.

F2 çalışma dalı feat/f2-inventory, F1-B commit’i 84692fb üzerine açıldı. Kullanıcı değişiklikleri korunur; gerçek sırlar .env ve .local-notes/ altında Git dışında tutulur.

F2 kod commit’i edaf19383869094c8d8a27bd8c9db4a967547907, 10 Ekim’de origin dal kimliğiyle doğrulandı. Devir belgeleri bu commit üzerine kaydedilir. main geliştirme dallarıyla birleştirilmedi; yeni iş güncel çalışma dalının son commit’inden devam etmelidir. Aynı gün Docker DB healthy ve uygulama readiness HTTP 200 olarak yeniden doğrulandı.

F3 yerel yükseltmesi 10 Ekim'de güncel yedek ve ayrı DB restore sonrası uygulandı; mevcut 8 app tablosunun içerikleri, hesap ve .env korundu. Dört migration ve ready durumu doğrulandı. Güncel test/yedek kanıtları progress.md'dedir.

F3 kod/test/tasarım commit’i `7fe048a822de098f0d8703f8f299fb71ee9eb383`; bu devir/ilerleme belgeleri ayrı commit ile aynı çalışma dalına eklenir. Uzak dal kimliğini her yeni oturumda yeniden doğrula.

Son sohbet kapanışı: F3 ilk devir commit’i `8d82e20fcfa7fa052ddcc023fae571ab5961f39e` GitHub çalışma dalıyla yeniden eşleşti. Bu belge güncellemesi onun üzerine kaydedilir. F4 ilk sürüm için kalan tek zorunlu fazdır; F5 isteğe bağlıdır. Kullanıcı uygulamayı kontrol etmek istedi; henüz sonuç/görsel kabul bildirmedi. Yeni sohbet önce varsa bu geri bildirimi, ardından F4 açık kararlarını almalı.
