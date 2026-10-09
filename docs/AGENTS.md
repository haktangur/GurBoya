# GürBoya — Codex çalışma kuralları

Bu belge kullanıcı isteğiyle docs/ altındadır. Kökten başlayan oturumda otomatik keşfine güvenme; başlangıç promptu bu dosyayı açıkça okutmalıdır. Bu dosyanın okunması proje genelinde bu kurallara uyulması isteğini taşır.

## Önce oku

[README](README.md), [bağlam](project_context.md), [kararlar](decisions.md), [mimari](architecture.md), [veritabanı](database.md), [plan](project_plan.md), [geliştirme kuralları](development_rules.md), [ilerleme](progress.md). Gerçek dosya/Git durumu ile karşılaştır.

## Sınırlar

- GürBoya tek Windows bilgisayarında yerel çalışan Türkçe web uygulamasıdır; geliştirme M3 Mac'te yapılır. Veritabanı PostgreSQL, çalışma ortamı Docker Compose. MSSQL kullanılmaz.
- Kod, sınıf ve değişken adları İngilizce; arayüz, hata ve bildirimler Türkçe.
- Boyada litre ambalaj hacmi, stok kutu adedi, fiyat kutu başınadır. Diğer ürünler kutu/adet/gram (tam sayı, gram hassasiyeti 1 gram); pigment modülü yoktur.
- Onaylı karar ile öneriyi ayır. Büyük mimari değişikliği onaysız yapma; kapsam dışı modül/bağımlılık ekleme.
- F1-A, F1-B ve F2 tamamlandı; Razor Pages + EF Core + Npgsql onaylıdır. Yeni görevde progress.md ve faz kapılarını esas al. database.md iş modeli taslağıdır; onaylanmamış iş tablolarını topluca oluşturma.
- İşlem geçmişi silinmez; parasal değerler decimal/numeric; satış/stok tek transaction; tekrar gönderim ve eşzamanlılık korunur.
- Gerçek sır, yerel DB, yedek veya müşteri verisini Git'e ekleme. Kullanıcı değişikliklerini geri alma. Volume silme, canlı verinin üstüne restore ve uzak Git geçmişini yeniden yazma açık yetki gerektirir.
- Anlamlı testleri çalıştır; çalışmayan testi başarılı gösterme. Mac testini Windows doğrulaması sayma.
- Markdown belgelerini docs/ altında tut. Her önemli görev sonunda ilgili tasarım, project_context.md ve progress.md dosyalarını güncelle.
- Küçük mantıksal Conventional Commits kullan. Commit öncesi diff, sır ve test kontrolü yap. Kullanıcı GitHub push işlemini yetkilendirdi; doğrulanan commit'leri origin deposundaki çalışma dalına normal push ile gönder ve uzak commit kimliğini doğrula. Başarısız gönderimi başarılı gösterme.
- Mevcut kapsamda ilerle; bir belirsizlik yalnızca sonraki fazı etkiliyorsa mevcut işi durdurma. Riskli önerileri gerekçesiyle eleştir; gereksiz karmaşıklık ekleme.

Dosya keşfi kaynağı: [OpenAI AGENTS.md rehberi](https://developers.openai.com/codex/guides/agents-md).
