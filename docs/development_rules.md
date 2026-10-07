# GürBoya — Geliştirme standartları

## Kod ve bağımlılıklar

İngilizce tanımlayıcılar, Türkçe kullanıcı metinleri. Modülleri iş sorumluluğuna göre ayır; sade fonksiyonlar, bağımlılık enjeksiyonu ve test edilebilir sınırlar kullan. SOLID, gereksiz katman/arayüz üretme gerekçesi değildir. Framework onayından sonra biçimlendirme ve lint araçları seçilir. Sürümleri sabitle; paket lock dosyalarını Git'e ekle. ARM64/AMD64 desteğini ve desteklenen sürümleri doğrula.

Sunucu tarafında giriş doğrulama, parametreli sorgu, belirlenmiş hata yanıtları gerekir. Hassas detaylar kullanıcıya/loğa sızdırılmaz. Para hesabı binary float ile yapılmaz; API decimal aktarımı ve Türkçe sayı biçimi uçtan uca test edilir. Tekrarlanan istekler aynı işleme bağlı kalmalı. UI düğmesini devre dışı bırakmak veri bütünlüğü korumasının yerine geçmez.

## Veritabanı ve testler

Her şema değişikliği sürümlü migration ile; ORM/migration aracı stack seçimi sonrası kararlaştırılır. Uygulama hesabı superuser değildir. Ayrı migration hesabı, test veritabanı ve canlı veritabanı kullan. Başlangıç scriptlerinin yalnızca boş volume üzerinde çalışabildiğini dikkate al; bunları kalıcı migration yerine kullanma.

Stok/finans işlemleri gerçek PostgreSQL üzerinde entegrasyon testleri gerektirir. Eşzamanlı son stok satışı, tekrar gönderim, kısmi hata/rollback, iade sınırı ve fiyat geçmişi öncelikli testlerdir. Kritik kullanıcı akışlarına uçtan uca test ekle. Her fazda test yapılır, sona bırakılmaz. İşletme verisi test fixture'ı değildir. Başarısız veya çalıştırılamayan kontrolü açık yaz.

## Git ve GitHub

Yerel main dalı temel; yeni iş için kısa feature/chore/docs dalı tercih edilir. Küçük mantıksal commit'ler: docs:, chore:, feat:, fix:, test:, refactor:. Commit öncesi diff'i incele, sırları ve gereksiz dosyaları kontrol et, uygun testleri çalıştır, belgeleri güncelle. Kullanıcıya ait değişiklikleri sessizce commit'e katma. Kimlik ayarlı değilse kullanıcı adına kimlik uydurma.

GitHub origin: https://github.com/haktangur/GurBoya.git. Kullanıcı doğrulanan proje değişikliklerinin GitHub'a gönderilmesini yetkilendirdi; her görev sonunda ilgili commit'leri çalışılan dala normal push ile gönder, yeni dal için upstream ayarla. Push öncesi uzak değişiklikleri kontrol et; başkasının değişikliklerini ezme. main başlangıç belgeleriyle yayımlandı. PR/CI yapılandırması henüz kurulmadı. Başarılı push'u uzak commit kimliğiyle doğrula; erişim/kimlik veya korumalı dal engelini açıkça bildir. Force push ve geçmiş silme açık yetki gerektirir. Kod incelemesi/check'ler sağlanmadan canlı dağıtım tamamlandı sayılmaz.

Ortak Markdown belgelerinin tamamı, project_context.md ve progress.md dahil sürümlenir. Kişisel notlar .local-notes/, sırlar .env veya güvenli yerel depoda tutulur; örnek ortam dosyası gerçek sır içermez. Ignore dosyası her isimdeki sırrı otomatik bulamaz; diff incelemesi zorunludur.

## Devir ve güvenlik

Her görev sonunda yapılanı, testleri, sınırlamaları ve sonraki görevi progress.md'ye işle; karar/bağlam değişirse diğer belgeleri de güncelle. Ürün pasifleştirme geçmişi silmez. Harici kaynak/cihaz bağımlılıklarını çevrimdışı kullanım hedefine göre değerlendir. Disk yedeği, geri yükleme provası ve Windows açılış testi canlıya geçiş koşuludur.
