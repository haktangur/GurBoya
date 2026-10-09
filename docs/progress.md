# GürBoya — İlerleme

Son güncelleme: 9 Ekim 2026.

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

## F1-B — uygulama iskeleti (8 Ekim 2026)

Kullanıcı Razor Pages + EF Core + Npgsql önerisini onayladı. src/GurBoya.Web, sabit SDK/runtime Dockerfile, NuGet lock dosyası ve EF araç manifesti eklendi. Türkçe başlangıç, 404/hata ekranları ve yerel CSS; /health/live ile /health/ready uçları hazır. Başlangıç ekranı yalnızca altyapı durumunu gösterir; iş modülleri yoktur.

compose.app.yaml web'i sadece 127.0.0.1:5080'e açar ve root olmayan kullanıcıyla çalıştırır. gurboya_app ve gurboya_migrator hesapları ayrıldı; yönetim parolası web'e verilmez. app ve infrastructure şemaları hazırlandı. İlk migration boş iş modelini kaydeder; web açılışı şema değiştirmez. EF migration açık migrate bakım hizmetiyle uygulanır.

### F1-B doğrulama sonuçları

Mac ARM64 üzerinde Docker ile çalıştırıldı. SDK 10.0.401, runtime/EF Core/araç 10.0.9, Npgsql EF sağlayıcısı 10.0.3. .NET SDK/runtime registry manifestlerinde linux/amd64 ve linux/arm64 doğrulandı; çalıştırma yalnızca ARM64'te yapıldı. Hostta bulunan SDK 10.0.103 değiştirilmedi.

| Kontrol | Sonuç |
|---|---|
| Kilitli NuGet restore ve Release derleme | Geçti; 0 hata, 0 uyarı |
| dotnet format --verify-no-changes | Geçti |
| EF has-pending-model-changes | Geçti; model ile migration uyumlu |
| Compose config --quiet, shell sözdizimi | Geçti |
| Açılış ve eksik migration | Web açılışında şema tablosu oluşmadı; readiness/ana sayfa 503, Türkçe yeniden deneme |
| db-setup tekrar çalıştırma | İki çalıştırma başarılı; migration/veri silinmedi |
| Migration ve tekrar | İki çalıştırmada EF geçmişi 1 kayıt; iş tablosu 0 |
| Hazır uygulama | Ana sayfa ve readiness 200; yerel CSS 200, Türkçe 404 |
| Yetki sınırları | Uygulama/migrator superuser değil; app CREATE/TEMP yok; gerçek DDL ve geçmiş DELETE denemeleri permission denied |
| Runtime ve port | Web root değil; yayımlanan port 127.0.0.1'e bağlı |
| DB kesintisi/toparlanma | Liveness 200; readiness/ana sayfa 503; DB geri gelince 200 |
| Sır/log kontrolü | Test parolaları konteyner loglarında yok; GSS kütüphane uyarısı yok |
| F1-A regresyon | scripts/test-postgres.sh geçti; 3 satır/150.00 toplam ve Türkçe metinler kalıcılık/restore sonrası korundu |

Son F1-B testi: bash scripts/test-app.sh, çıkış 0; proje gurboya-f1b-0ad08acecc85, yerel kanıt dizini .tmp/f1-b.1i4zjY. F1-A regresyonu: proje gurboya-f1a-2832e8bd1c77, .tmp/f1-a.CgKAoA, çıkış 0. Her iki testte konteyner/ağ kaldırıldı, volume silinmedi. İşletme verisi kullanılmadı.

İlk derlemedeki HTTP header API hatası düzeltildi. İlk başarılı kabul testinde görülen Npgsql Kerberos kütüphane araması, yerel SCRAM kurulumunda GssEncryptionMode.Disable ile kaldırılıp yeniden test edildi. Sonuçlar son kod içindir.

Normal gurboya geliştirme kurulumu da başlatıldı: localhost:5080; DB'de yalnızca altyapı şemaları/EF geçmişi var. Üç farklı rastgele parola içeren .env Git dışında 0600 izinli oluşturuldu. Uygulama çalışır bırakıldı. Browser sağlayıcısı bulunmadı ve Safari bilgisayar erişimi onaylanmadı; görsel UI kontrolü yapılamadı. HTTP testleri görsel test yerine sunulmaz. Windows/AMD64, tam internet kesintisi ve OS açılışı doğrulanmadı. ASP.NET Data Protection anahtarlarının kalıcılığı/korunması ve kullanıcı oturumu, veri giriş ekranlarından önce tamamlanmalı.

9 Ekim 2026 ek kontrolü: Kullanıcı Docker Desktop’ı kapatıp açtıktan sonra gurboya-db-1 healthy ve gurboya-web-1 çalışır bulundu; localhost:5080/health/ready yeniden ready yanıtı verdi. Bu, Mac üzerinde Docker yeniden başlatma doğrulamasıdır; Windows/OS açılış testi değildir.

## Aktif ve sonraki görev

F1-B teslim dalı feat/f1-b-app; Git gönderim sonucu ve uzak commit kimliği teslim mesajında bildirilir. F2 öncesinde ürün varyantı/baz, gram hassasiyeti, negatif stok kuralı ve kullanıcı hesabı yaklaşımını netleştir. F2 iş modülüne bu tur geçilmedi.

## Kalan kararlar

[project_context.md](project_context.md) açık soruların kaynağıdır. Windows gereksinimleri, işletim sistemi açılışı ve çevrimdışı pilot F4 öncesinde doğrulanmalı. Hassas notlar ortak belgelere yazılmaz.
