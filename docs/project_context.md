# GürBoya — Mevcut bağlam

Son güncelleme: 8 Ekim 2026. Amaç küçük aile dükkânında kutulu boya ve diğer hırdavat ürünlerinin elle stok girişi, fiyat ve satış takibidir. Türkçe arayüz zorunlu. Geliştirme M3 MacBook Air, canlı kullanım tek Windows bilgisayarında olacak.

## Güncel durum

PostgreSQL + Docker Compose + yerel web onaylandı. Kutu başına fiyat, elle marka/litre bilgisi ve pigment yerine boya/renk kodu takibi kesinleşti. Frontend/backend, ORM ve uygulama sürümleri seçilmedi. Tamamlanmış uygulama modülü yok. F1-A altyapısı ve tekrarlanabilir kabul testi oluşturuldu; ayrıntılı iş kuralları henüz bütünüyle onaylanmadı.

F1-A Mac ARM64 kabul koşulları tamamlandı: PostgreSQL 18.6-bookworm sabit digest, kalıcı volume, healthcheck, dışa kapalı ağ ve sırsız .env.example hazır. İzole test projesinde bağlantı, yanlış parola reddi, konteyner yeniden oluşturma sonrası kalıcılık ve ayrı DB'ye yedekten dönüş doğrulandı. İş tabloları/uygulama iskeleti oluşturulmadı. Normal gurboya kurulumu başlatılmadı; test konteynerleri durdurulup kaldırıldı, sentetik veri volume'ları ve .tmp/ test dosyaları yerelde korundu. Windows/AMD64, gerçek internet kesintisi ve işletim sistemi açılış testleri bekliyor.

Sonraki görev F1-B: frontend/backend/ORM ve hesap yaklaşımını kullanıcıyla netleştir. Teknoloji seçimi onaylanmadan iskelet oluşturma. Çalıştırma komutları [README](README.md), gerçek test kanıtları [progress.md](progress.md) içindedir.

## Açık sorular ve etkileri

| Konu | Ne zaman çözülmeli? |
|---|---|
| Windows sürümü, RAM ve işlemci | Windows kurulum testi öncesi; Mac'teki F1-A'yı engellemez |
| Frontend/backend/ORM seçimi | F1-B uygulama iskeletinden önce |
| Ürün varyantı/baz tipi, gram hassasiyeti, stok engeli | F2 stok modülünden önce |
| KDV dahil/hariç, para birimi, indirim ve yuvarlama | Fiyat/satış hesapları uygulanmadan önce; TRY öneri |
| Renklendirme ücreti, boya iadesi, hasarlı ürün | F3 satış/iade modülünden önce |
| Veresiye, cari ve tahsilat ihtiyacı | Kullanılabilir MVP kapsamını onaylamadan önce |
| Kullanıcı hesapları ve roller | Kimlik/yetkilendirme geliştirilmeden önce |
| Yedek saklama, harici disk, kabul edilen veri kaybı | Canlı kullanım öncesi |

Mimari teknik taslağı [architecture.md](architecture.md), PostgreSQL modeli [database.md](database.md), faz kapıları [project_plan.md](project_plan.md) içindedir. Yeni sohbet [AGENTS.md](AGENTS.md) okuma sırasını izlemeli, gerçek dosya ve testleri doğrulamalıdır. [Başlangıç promptu](codex_start_prompt.md) tamamlanan F1-A görevinin tarihsel kapsamını korur; sıradaki faz F1-B’dir.

GitHub origin https://github.com/haktangur/GurBoya.git olarak bağlandı; ilk belge commit’i main dalına gönderildi ve upstream ayarlandı. Kullanıcı sonraki doğrulanmış geliştirme commit’lerinin de GitHub’a gönderilmesini yetkilendirdi. Kişisel verisiz bağlam ve ilerleme belgeleri sürümlenir; hassas notlar .local-notes/ altında tutulur. Tüm Markdown belgeleri docs/ içindedir; başlangıç promptu ajan kurallarını açıkça okutur.
