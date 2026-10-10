# Yeni sohbete devam promptu

Proje klasörünü açıp aşağıdaki metni yeni Codex sohbetine yapıştır. Bu prompt tamamlanan F1-A/F1-B/F2 çalışmalarını yeniden başlatmaz.

```text
GürBoya projesine kaldığımız yerden devam ediyoruz. Önce docs/AGENTS.md dosyasını oku ve proje genelinde uygula. Ardından docs/next_session.md, docs/README.md, docs/project_context.md, docs/decisions.md, docs/architecture.md, docs/database.md, docs/project_plan.md, docs/development_rules.md, docs/progress.md ve docs/chatgpt.md dosyalarını oku.

F1-A, F1-B ve F2 Mac ARM64 üzerinde tamamlandı ve test edildi. Çalışma dalı feat/f2-inventory; doğrulanmış F2 kod commit'i edaf19383869094c8d8a27bd8c9db4a967547907, devir belgeleri bunun üzerindedir. Git durumunu, origin güncellemelerini ve çalışan Docker servislerini kontrol et. main dalını güncel sanma; tamamlanan kodun bulunduğu son commit üzerinden devam et. Kullanıcı değişikliklerini, .env dosyasını, yönetici hesabını, verileri ve volume'ları koru. İlk kurulumu veya migration'ları körlemesine tekrarlama; veri silme, volume kaldırma ve force push yapma.

Sıradaki görev F3 satış/iade hazırlığıdır. Önce indirim, satış/fiş yuvarlaması, boya ve hasarlı ürün iadesi, renklendirme ücreti ve veresiye/tahsilat ihtiyacını bana sade sorularla sor. Onaylı F2 kararlarını yeniden sorma. Yanıtlarımı almadan belirsiz satış iş kurallarını veya yeni iş tablolarını uygulama. Yanıtları karar belgelerine işle, ardından F3'ü uygula. Mevcut KDV oranı kullanıcı seçimi %15'tir; mevzuat varsayımı değildir. Pigment modülü ekleme.

Uygun testleri çalıştır; sonuçları ve yapılmayan Windows/AMD64, görsel tarayıcı, tam internet kesintisi ve OS açılışı kontrollerini açıkça belgele. Şema değişikliğinden önce yedek al ve ayrı DB'ye dönüşünü doğrula; uygulanmış migration'ları değiştirme. Ortak Markdown dosyaları docs/ altında kalsın; sırları Git'e veya çıktıya yazma. Doğrulanmış değişiklikleri mantıksal commit'lerle kaydet ve origin https://github.com/haktangur/GurBoya.git üzerindeki çalışma dalına normal push ile gönder. Push yetkilidir; uzak commit kimliğini doğrula.
```
