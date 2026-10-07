# Codex'e Verilecek İlk Geliştirme Promptu

Aşağıdaki metin VS Code içindeki Codex sohbetine yapıştırılabilir. Önce VS Code'da GürBoya proje klasörünü aç.

---

GürBoya projesinde çalışıyoruz. Önce docs/AGENTS.md dosyasını açıkça oku ve proje genelinde uygula; kökte AGENTS.md bulunmasına veya bu dosyanın otomatik yüklenmesine güvenme. Ardından docs/README.md, docs/project_context.md, docs/decisions.md, docs/architecture.md, docs/database.md, docs/project_plan.md, docs/development_rules.md, docs/progress.md ve docs/chatgpt.md dosyalarını oku. Mevcut dosya ve Git durumunu kontrol et; kullanıcı değişikliklerini koru.

Onaylı hedef: Tek Windows bilgisayarında internetsiz kullanılabilen, tarayıcıdan açılan Türkçe uygulama. Geliştirme cihazı M3 MacBook Air. PostgreSQL + Docker Compose kullanılacak; MSSQL kararı kaldırıldı. Boyada marka ve ambalaj litresi elle girilir, stok kutu adedi ve fiyat kutu başınadır. Diğer ürünlerde adet/gram; renklendirmede boya ve renk kodu, pigment takibi yok.

Bu tur docs/project_plan.md içindeki F1-A'yı uygula: Docker/Compose ortamını kontrol et; ARM64 ve AMD64 destekli sabit PostgreSQL imajıyla compose.yaml, kalıcı volume, healthcheck ve gerçek sır içermeyen .env.example hazırla. Sırları Git dışında tut, DB'yi dış ağa açma. PostgreSQL imajının volume yolunu seçilen sürümün resmî belgesinden doğrula. İş tabloları, frontend/backend iskeleti, satış veya cari modülü oluşturma; framework/ORM henüz onaylanmadı.

Yalnızca plan sunup durma; mevcut izinler ve ortam elverdiği sürece F1-A'yı tamamla. Compose yapılandırmasını, DB bağlantısını, volume silmeden konteyner yeniden oluşturma sonrası veri kalıcılığını ve ayrı test veritabanına yedekten geri yüklemeyi doğrula. Gerçek verileri silme; docker compose down -v kullanma. Docker veya gerekli izin yoksa engeli açıkça bildir, yapılabilen dosya hazırlığını tamamla; yapılmayan testi başarılı sayma. Windows'ta test edemiyorsan bunu ayrıca belirt.

Çalıştırma ve yedek komutlarını docs/README.md'ye yaz; docs/progress.md ve docs/project_context.md dosyalarını gerçek sonuçlarla güncelle. Değişiklikleri ve sırları inceleyip yalnızca bu görevin dosyalarını mantıksal bir Conventional Commit ile kaydet; kimlik/izin eksikse uydurma ve açıkça bildir. Kullanıcı GitHub'a gönderimi yetkilendirdi: origin https://github.com/haktangur/GurBoya.git. Doğrulanmış commit'leri çalıştığın dala normal push ile gönder; yeni dalda upstream ayarla. Önce uzak güncellemeleri kontrol et, force push yapma. Çakışma, kimlik veya erişim engeli varsa açıkça bildir; başarılı push'u uzak commit kimliğiyle doğrula.

Sonunda tamamlanan işi, test kanıtlarını ve kalan sınırlamaları kısa anlat. F1-B için frontend/backend/ORM önerini gerekçesiyle sun ve uygulama iskeletine geçmeden teknoloji seçimini benimle netleştir. Gereksiz bağımlılık veya mimari değişikliği yapma.
