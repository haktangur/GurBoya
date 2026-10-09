# GürBoya — Mevcut bağlam

Son güncelleme: 9 Ekim 2026. Amaç küçük aile dükkânında kutulu boya ve diğer hırdavat ürünlerinin elle stok girişi, fiyat ve satış takibidir. Türkçe arayüz zorunlu. Geliştirme M3 MacBook Air, canlı kullanım tek Windows bilgisayarında olacak.

## Güncel durum

PostgreSQL + Docker Compose + yerel web onaylandı. Kutu başına fiyat, elle marka/litre bilgisi ve pigment yerine boya/renk kodu takibi kesinleşti. ASP.NET Core Razor Pages + EF Core + Npgsql seçimi kullanıcı tarafından onaylandı (K13); sürümler sabitlendi. F1-B durum ekranı ve migration altyapısı hazır; işletme modülü yok. F1-A altyapısı ve tekrarlanabilir kabul testi oluşturuldu; ayrıntılı iş kuralları henüz bütünüyle onaylanmadı.

F1-A Mac ARM64 kabul koşulları tamamlandı: PostgreSQL 18.6-bookworm sabit digest, kalıcı volume, healthcheck, dışa kapalı ağ ve sırsız .env.example hazır. İzole test projesinde bağlantı, yanlış parola reddi, konteyner yeniden oluşturma sonrası kalıcılık ve ayrı DB'ye yedekten dönüş doğrulandı. F1-A aşamasında yalnızca altyapı oluşturuldu; test konteynerleri kaldırıldı, sentetik veri volume’ları ve .tmp/ test dosyaları korundu. Windows/AMD64, gerçek internet kesintisi ve işletim sistemi açılış testleri bekliyor.

F1-B tamamlandı: Türkçe başlangıç/hata ekranı, DB durumu, ayrı uygulama/migration hesapları ve açık EF migration komutu test edildi. Normal geliştirme kurulumu localhost:5080 üzerinde çalışıyor; .env yerelde 0600 izinli ve Git dışında, gurboya_postgres_data yalnızca altyapı şemaları ve migration geçmişi içeriyor. Sonraki faz F2 öncesi ürün varyantı/baz, gram hassasiyeti, negatif stok ve mağaza verisine erişim için kullanıcı hesabı yaklaşımı netleştirilmeli. Çalıştırma komutları [README](README.md), gerçek test kanıtları [progress.md](progress.md) içindedir.

## Açık sorular ve etkileri

| Konu | Ne zaman çözülmeli? |
|---|---|
| Windows sürümü, RAM ve işlemci | Windows kurulum testi öncesi; Mac'teki F1-A'yı engellemez |
| Ürün varyantı/baz tipi, gram hassasiyeti, stok engeli | F2 stok modülünden önce |
| KDV dahil/hariç, para birimi, indirim ve yuvarlama | Fiyat/satış hesapları uygulanmadan önce; TRY öneri |
| Renklendirme ücreti, boya iadesi, hasarlı ürün | F3 satış/iade modülünden önce |
| Veresiye, cari ve tahsilat ihtiyacı | Kullanılabilir MVP kapsamını onaylamadan önce |
| Kullanıcı hesapları ve roller | Kimlik/yetkilendirme geliştirilmeden önce |
| Yedek saklama, harici disk, kabul edilen veri kaybı | Canlı kullanım öncesi |

Mimari teknik taslağı [architecture.md](architecture.md), PostgreSQL modeli [database.md](database.md), faz kapıları [project_plan.md](project_plan.md) içindedir. Yeni sohbet [AGENTS.md](AGENTS.md) okuma sırasını izlemeli, gerçek dosya ve testleri doğrulamalıdır. [Başlangıç promptu](codex_start_prompt.md) tamamlanan F1-A görevinin tarihsel kapsamını korur; F1-B de tamamlandı; sıradaki faz F2’dir.

GitHub origin https://github.com/haktangur/GurBoya.git olarak bağlandı; ilk belge commit’i main dalına gönderildi ve upstream ayarlandı. Kullanıcı sonraki doğrulanmış geliştirme commit’lerinin de GitHub’a gönderilmesini yetkilendirdi. Kişisel verisiz bağlam ve ilerleme belgeleri sürümlenir; hassas notlar .local-notes/ altında tutulur. Tüm Markdown belgeleri docs/ içindedir; başlangıç promptu ajan kurallarını açıkça okutur.

F1-B teslim dalı feat/f1-b-app; F1-A commit’i 2649482 üzerine açıldı. İş tabloları, stok/satış/finans kuralları ve son kullanıcı giriş sistemi bu görevde eklenmedi. Ayrıntılı testler ve sınırlamalar progress.md içindedir.
