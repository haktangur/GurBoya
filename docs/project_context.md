# GürBoya — Mevcut bağlam

Son güncelleme: 9 Ekim 2026. Amaç küçük aile dükkânında kutulu boya ve diğer hırdavat ürünlerinin elle stok girişi, fiyat ve satış takibidir. Türkçe arayüz zorunlu. Geliştirme M3 MacBook Air, canlı kullanım tek Windows bilgisayarında olacak.

## Güncel durum

Yerel Windows hedefi, PostgreSQL/Docker ve Razor Pages + EF Core + Npgsql onaylı. F1-A/F1-B tamamlandı. F2 için kullanıcı kararları K14–K18: boya marka/litre/renk/fiyat serbest giriş; diğer ürünlerde kutu/adet/gram; tüm stok miktarları tam sayı; negatif stok engeli; tek şifreli yönetici; TL fiyatlarda KDV ekle/dahil, başlangıç %15 ve ayrı tutar gösterimi.

F2 tamamlandı: ürün oluşturma/düzenleme/arama/pasifleştirme, stok girişi/gerekçeli düzeltme/sayım, fiyat ve stok geçmişi; ASP.NET Identity ile giriş/çıkış/şifre değiştirme. DB trigger'ları hareket ve bakiye bütünlüğünü korur; işlem kimlikleri çift gönderimi engeller. Testler geçti; mevcut F1-B veritabanı yedeklenip ayrı DB’ye dönüş doğrulandıktan sonra normal geliştirme kurulumu güncellendi. localhost:5080 üzerinde giriş yapılabilir; ilk şifre Git dışındaki .local-notes/ilk-giris.txt dosyasında. Normal ürün listesi boş, test verileri izole projelerde kaldı. Test sonuçları progress.md, çalıştırma komutları README.md içindedir.

Sonraki faz F3 satış/iade. Bu tur satış fişi, cari, tahsilat veya pigment modülü oluşturulmaz. Windows/AMD64, gerçek internet kesintisi ve işletim sistemi açılışı doğrulaması bekliyor.

## Açık sorular ve etkileri

| Konu | Ne zaman çözülmeli? |
|---|---|
| Windows sürümü, RAM ve işlemci | Windows kurulum testi öncesi; Mac'teki F1-A'yı engellemez |
| Satış indirimi ve fiş/satır yuvarlaması | F3 öncesi; ürün birim fiyatının KDV kuralları onaylı |
| Renklendirme ücreti, boya iadesi, hasarlı ürün | F3 satış/iade modülünden önce |
| Veresiye, cari ve tahsilat ihtiyacı | Kullanılabilir MVP kapsamını onaylamadan önce |
| Yedek saklama, harici disk, kabul edilen veri kaybı | Canlı kullanım öncesi |

Mimari teknik taslağı [architecture.md](architecture.md), PostgreSQL modeli [database.md](database.md), faz kapıları [project_plan.md](project_plan.md) içindedir. Yeni sohbet [AGENTS.md](AGENTS.md) okuma sırasını izlemeli, gerçek dosya ve testleri doğrulamalıdır. [Başlangıç promptu](codex_start_prompt.md) tamamlanan F1-A görevinin tarihsel kapsamını korur; F2 tamamlandı; sıradaki kapsam F3’tür. Gerçek sonuçları progress.md’den kontrol et.

GitHub origin https://github.com/haktangur/GurBoya.git olarak bağlandı; ilk belge commit’i main dalına gönderildi ve upstream ayarlandı. Kullanıcı sonraki doğrulanmış geliştirme commit’lerinin de GitHub’a gönderilmesini yetkilendirdi. Kişisel verisiz bağlam ve ilerleme belgeleri sürümlenir; hassas notlar .local-notes/ altında tutulur. Tüm Markdown belgeleri docs/ içindedir; başlangıç promptu ajan kurallarını açıkça okutur.

F2 çalışma dalı feat/f2-inventory, F1-B commit’i 84692fb üzerine açıldı. Kullanıcı değişiklikleri korunur; gerçek sırlar .env ve .local-notes/ altında Git dışında tutulur.
