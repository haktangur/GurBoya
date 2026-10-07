# GürBoya

Türkçe boya ve hırdavat dükkânı yönetim uygulaması. Tek bilgisayarda tarayıcıdan açılır; uygulama ve PostgreSQL Docker Compose ile yerel çalışır. Bulut gerekmez. Kurulum tamamlandıktan sonra günlük işlemler internet gerektirmez.

## Mevcut durum — 8 Ekim 2026

PostgreSQL'e geçiş kullanıcı tarafından onaylandı. Bu bir çalışan uygulama teslimi değil, VS Code içindeki Codex için geliştirmeye devir paketidir. Uygulama, Compose dosyası ve veritabanı henüz oluşturulmadı. Frontend/backend seçimi ve bazı iş kuralları karar bekliyor. İlk uygulanabilir görev F1-A: Docker + PostgreSQL altyapısı.

## Okuma sırası

1. [Ajan kuralları](AGENTS.md)
2. [Bağlam](project_context.md)
3. [Kararlar](decisions.md)
4. [Mimari](architecture.md)
5. [Veritabanı taslağı](database.md)
6. [Faz planı](project_plan.md)
7. [Geliştirme kuralları](development_rules.md)
8. [İlerleme](progress.md)
9. [Codex'e verilecek ilk geliştirme promptu](codex_start_prompt.md)

[ChatGPT çalışma kuralları](chatgpt.md) aynı karar ve standartlara yönlendirir. Tüm Markdown dosyaları kullanıcının istediği gibi docs/ altında tutulur. Kök AGENTS.md olmadığı için başlangıç promptu docs/AGENTS.md dosyasının açıkça okunmasını ister; kökten başlayan oturumda otomatik yüklenmesine güvenilmez.

## Dosya ve Git düzeni

Şu an yalnızca docs/ belgeleri ve kökte .gitignore bulunur; yerel Git deposu hazırlanmış ve [GitHub reposuna](https://github.com/haktangur/GurBoya) bağlanmıştır. Başlangıç belgeleri main dalına gönderildi. Compose, örnek ortam dosyası, uygulama ve test klasörleri ihtiyaç oldukça ilk geliştirme görevlerinde eklenecek. Boş frontend/backend klasörleri oluşturulmadı. Gerçek sırlar, veriler ve yedekler Git'e eklenmez. Henüz çalıştırma komutu yok; F1-A sonunda doğrulanmış komutlar buraya yazılacak.
