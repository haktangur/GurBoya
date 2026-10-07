# GürBoya — Mevcut bağlam

Son güncelleme: 8 Ekim 2026. Amaç küçük aile dükkânında kutulu boya ve diğer hırdavat ürünlerinin elle stok girişi, fiyat ve satış takibidir. Türkçe arayüz zorunlu. Geliştirme M3 MacBook Air, canlı kullanım tek Windows bilgisayarında olacak.

## Güncel durum

PostgreSQL + Docker Compose + yerel web onaylandı. Kutu başına fiyat, elle marka/litre bilgisi ve pigment yerine boya/renk kodu takibi kesinleşti. Frontend/backend, ORM ve tam sürümler seçilmedi. Tamamlanmış uygulama modülü yok. Hazırlık belgeleri oluşturuldu; ayrıntılı iş kuralları henüz bütünüyle onaylanmadı.

Aktif görev: F1-A'ya devir — Docker/PostgreSQL altyapısını VS Code Codex'te oluştur. İlk görevde iş tabloları veya frontend/backend framework'ü kurulmayacak. Kodlamaya tümüyle yasak yok; kullanıcı bundan sonraki geliştirmeyi VS Code'a devretti. Yalnızca onaylanan kapsam uygulanır.

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

Mimari teknik taslağı [architecture.md](architecture.md), PostgreSQL modeli [database.md](database.md), faz kapıları [project_plan.md](project_plan.md) içindedir. Yeni sohbet [AGENTS.md](AGENTS.md) okuma sırasını izlemeli, gerçek dosya ve testleri doğrulamalıdır. Sonraki somut görev [başlangıç promptunda](codex_start_prompt.md) tanımlıdır.

GitHub origin https://github.com/haktangur/GurBoya.git olarak bağlandı; ilk belge commit’i main dalına gönderildi ve upstream ayarlandı. Kullanıcı sonraki doğrulanmış geliştirme commit’lerinin de GitHub’a gönderilmesini yetkilendirdi. Kişisel verisiz bağlam ve ilerleme belgeleri sürümlenir; hassas notlar .local-notes/ altında tutulur. Tüm Markdown belgeleri docs/ içindedir; başlangıç promptu ajan kurallarını açıkça okutur.
