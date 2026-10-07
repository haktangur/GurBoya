# GürBoya — İlerleme

Son güncelleme: 8 Ekim 2026.

## Tamamlanan hazırlık

- Başlangıç gereksinimleri ve kullanıcı yanıtları analiz edildi.
- Yerel web, kutu fiyatlandırması, marka/litre girişi, pigment takibinin yapılmaması belgelendi.
- PostgreSQL geçişi onaylandı ve aktif mimari/bağlam güncellendi. MSSQL aktif plan olmaktan çıkarıldı.
- PostgreSQL veri modeli taslağı, fazlar, geliştirme/ChatGPT/Codex kuralları ve başlangıç promptu yazıldı.
- Bütün Markdown belgeleri docs/ altında toplandı; .gitignore hazırlandı.
- Yerel Git deposu main dalıyla oluşturuldu. origin https://github.com/haktangur/GurBoya.git olarak bağlandı; başlangıç commit’i 3799495 main dalına başarıyla gönderildi ve upstream ayarlandı.

- Kullanıcının GitHub gönderim yetkisi çalışma kurallarına ve başlangıç promptuna işlendi; sonraki görevler doğrulanmış commit’leri çalışma dalına normal push ile gönderecek.

## Aktif ve sonraki görev

VS Code Codex'e devir: F1-A Docker + PostgreSQL altyapısı. Henüz Compose veya uygulama kodu, veritabanı/tablolar, Docker kurulumu yok. Bu hazırlık tam ürün mimarisinin onaylandığı anlamına gelmez. F1-B için framework/ORM ve diğer fazlar için iş kuralları karar bekliyor.

## Doğrulama

11 Markdown belgesinin yerel bağlantıları kontrol edildi; kırık bağlantı ve kökte Markdown dosyası yok. Karar tutarlılığı incelendi. .env, .env.local, kişisel notlar ve yedeklerin Git dışında kaldığı doğrulandı. Uygulama testleri çalıştırılmadı; çalıştırılabilir uygulama henüz yok. Windows/Docker uyumluluğu test edilmedi. İmajlar indirilmedi. Yerel veritabanı veya işletme verisi oluşturulmadı.

## Kalan kararlar

[project_context.md](project_context.md) açık soruların kaynağıdır. Öncelik F1-A'yı tamamlamak, sonra framework/ORM seçimini onaylatmaktır. Hassas notlar ortak belgelere yazılmaz.
