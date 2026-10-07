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

## F1-A — tamamlanan altyapı (8 Ekim 2026)

- compose.yaml: PostgreSQL 18.6-bookworm + sabit çok mimarili index digest, named volume, healthcheck, internal ağ, host portu yok.
- .env.example: gerçek sır yok; boş parola Compose tarafından reddedilir. Yönetim hesabı uygulama hesabı olarak kullanılmayacak.
- scripts/test-postgres.sh: normal kurulumdan ayrı, her seferinde benzersiz proje/volume; sentetik test verisi. Konteyner yeniden oluşturma, dump/copy/restore ve içerik karşılaştırması.
- README: Mac/PowerShell kurulum, başlatma/durdurma, volume korunması, yedek ve ayrı DB'ye geri yükleme komutları; mimari/bağlam/faz planı güncellendi.
- İş tabloları, frontend/backend/ORM veya kapsam dışı bağımlılık eklenmedi.

## Doğrulama kanıtları

Ortam: macOS ARM64; Docker istemci/motor 29.4.2, motor aarch64; Docker Compose v5.1.3. Docker Desktop başlangıçta kapalıydı, açıldı. Sandbox içindeki ilk Docker erişimi izin nedeniyle başarısız oldu; izinli tekrar çalıştırmada testler geçti. Bu başarısız deneme DB testi başarısı sayılmadı.

| Kontrol | Sonuç |
|---|---|
| Resmî sürüm ve volume yolu | PostgreSQL 18.6-bookworm; 18+ volume /var/lib/postgresql, PGDATA /var/lib/postgresql/18/docker |
| Registry manifest | linux/amd64 ve linux/arm64/v8 mevcut; index sha256:afc7e2d441324c0388fa80c3d24f733b4194a4eb7f47dd8ee2b08eb1a24a647c |
| bash -n scripts/test-postgres.sh | Geçti |
| Compose config --quiet / boş parola | Geçerli ayar kabul edildi; boş parola reddedildi; çözülmüş sırlar loglanmadı |
| up --wait / healthcheck | Geçti |
| TCP üzerinden SQL ve yanlış parola | Kayıt yazma/okuma geçti; yanlış parola reddedildi |
| Ağ | Host port bağlaması 0; Compose ağı internal=true |
| --force-recreate --pull never | Konteyner kimliği değişti; 3 kayıt, 150.00 toplam ve Türkçe metinler korundu |
| pg_dump custom → hosta cp → konteynere cp → pg_restore | Ayrı f1a_restore DB'sinde 3 kayıt, 150.00 toplam ve Türkçe metinler birebir eşleşti |
| git diff --check, Markdown bağlantıları ve sır kontrolü | Geçti; üretilen parolalar teslim dosyalarında yok; .env/.tmp/yedekler ignore kapsamında |
| Ana gurboya DB | public şemasında 0 tablo; iş tablosu oluşturulmadı |

Kabul testi komutu: bash scripts/test-postgres.sh. Son sürüm README ile aynı compose cp yöntemini kullanarak 0 çıkış koduyla geçti. Son test projesi gurboya-f1a-ab6d5876f497; yerel çıktı dizini .tmp/f1-a.qNwnz8. Test sonunda konteyner/ağ kaldırılır, volume silinmez. Yerel test sırları ve dump .tmp/ altındadır, Git dışındadır; gerçek müşteri verisi kullanılmadı. Normal gurboya projesi başlatılmadı. Windows/AMD64 üzerinde çalışma, tam internet kesintisi ve Windows açılışı test edilmedi; manifest desteği çalışma testi yerine geçmez.

## Aktif ve sonraki görev

Teslim dalı chore/f1-a-postgres. Git gönderim sonucu ve doğrulanan uzak commit kimliği görev tesliminde ayrıca bildirilir. F1-B öncesinde framework/ORM ve hesap yaklaşımı kullanıcı onayı bekler. Asgari yetkili uygulama/migration hesapları bu fazda tasarlanacak.

## Kalan kararlar

[project_context.md](project_context.md) açık soruların kaynağıdır. Windows gereksinimleri, işletim sistemi açılışı ve çevrimdışı pilot F4 öncesinde doğrulanmalı. Hassas notlar ortak belgelere yazılmaz.
