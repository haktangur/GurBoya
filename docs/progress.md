# GürBoya — İlerleme

Son güncelleme: 10 Ekim 2026.

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

## F2 — ürün, stok, KDV ve tek kullanıcı (9 Ekim 2026)

K14–K18 uygulandı: elle boya marka/litre/renk/fiyat; diğer ürünlerde kutu/adet/gram; 1 gram ve tam kutu/adet; eksi stok engeli; tek şifreli yönetici; KDV ekle/dahil, kullanıcı seçimi başlangıç %15, değiştirilebilir oran ve ayrı net/KDV/toplam.

Ürün arama, kart oluşturma/düzenleme, pasif/aktif durumu, stok girişi/gerekçeli düzeltme/sayım ve geçmiş sayfaları eklendi. Birim fiyatları decimal/numeric, dört basamaklı hesaplanır. İlk fiyat ve değişiklikler saklanır; hareketin alış fiyatı sonraki kart düzenlemesinden etkilenmez. Hareket/bakiye/istek kaydı tek transaction; UUID+hash/advisory lock tekrarları, ürün satırı kilidi eşzamanlı stok çıkışlarını korur. DB trigger'ları doğrudan bakiye değişimini, negatif stoğu, geçmiş güncelleme/silmeyi ve geçmişli ürünün birim değişimini engeller.

ASP.NET Identity 10.0.9 eklendi; özel şifreleme yazılmadı. Tek yonetici hesabı, giriş/çıkış/şifre değiştirme, 5 hatada 5 dakika kilit, CSRF korumalı formlar, HttpOnly/SameSite=Strict çerez ve kalıcı Data Protection volume'u var. İlk parola yalnızca admin komutuna verilir; admin tekrar çalışırsa mevcut şifreyi değiştirmez. Anahtar dizini 0700; XML anahtarlar ayrıca şifreli değildir, yerel OS/Docker erişimine dayanır. Windows disk güvenliği pilot öncesi doğrulanacak.

### F2 doğrulama kanıtları

| Kontrol | Sonuç |
|---|---|
| Kilitli restore / Release derleme | Geçti; 0 hata, 0 uyarı |
| dotnet format --verify-no-changes / EF pending-model | Geçti; model ve migration uyumlu |
| Açık migration ve tekrarı | Geçti; 3 migration kaydı; web başlangıcı DDL yapmaz |
| Giriş/CSRF/yanlış şifre | Yetkisiz ürün erişimi girişe yönlenir; CSRF'siz POST ve yanlış şifre reddedilir |
| Şifre değişimi/çıkış/kilit | Eski şifre reddi, yeni şifre kabulü ve 5 hata sonrası geçici kilit geçti |
| Ürün/KDV | Serbest marka/renk, 2,5 L; 100 + %15 = 115; dahil 115 → 100 net + 15 KDV; %20 değişimi geçmişi korudu |
| Birimler/geçersiz veri | Boyada zorunlu litre/marka/renk/kutu; diğerlerinde kutu/adet/gram; 0,5 ve 0.5 miktar reddi; NaN ve geçersiz oran reddi |
| Aynı form tekrarları | Ürün/fiyat/stok tekrarında çift kayıt yok; aynı UUID farklı içerikle reddedilir |
| Stok bitti/yenilendi | Fazla azaltma reddedildi; sıfır stok uyarısı; giriş sonrası tekrar kullanılabilir |
| Eşzamanlı son miktar | İki azaltma isteğinden yalnızca biri geçti; bakiye 0 |
| Sayım | Mutlak toplam, sıfır sayım ve fark 0 kaydı geçti; eski sayımın tekrarı sonraki girişi ezmedi |
| Eski form/pasif kart/geçmiş | Version çakışması ve geçmişli kartta birim değişimi reddi; pasif kartta yazma reddi ve yeniden aktifleştirme geçti |
| DB rollback/mutabakat | Zorlanan işlem hatasında hareket/bakiye geri alındı; her kartta bakiye = hareket toplamı |
| DB korumaları | Doğrudan bakiye UPDATE ve geçmiş DELETE engellendi; app superuser/DDL/TEMP yetkisiz |
| F2 custom dump/restore | Ayrı f2_restore DB'ye kullanıcı/ürün/geçmiş/trigger şeması geri yüklendi; stok toplamı eşleşti |
| Oturum kalıcılığı | Web konteyneri yeniden oluşturulunca mevcut oturumla ürün listesi 200 |
| DB kesintisi/toparlanma | Readiness/ana durum sayfası 503; DB geri gelince 200 |
| Sır/biçim/belge | Parolalar loglarda/teslim dosyalarında yok; diff/shell/Python sözdizimi ve yerel belge bağlantıları geçti |
| F1-A regresyon | Kalıcılık, 3 satır/150.00 ve Türkçe metinli ayrı DB restore tekrar geçti |

Son F2 test komutu: bash scripts/test-app.sh; 0 çıkış kodu; izole proje gurboya-f1b-6ae122cd127a; kanıt/dump/çerez dizini .tmp/f1-b.AgF1tk. F1-A: gurboya-f1a-4bd593cc576b, .tmp/f1-a.XSdTAX; çıkış 0. Test projeleri normal DB'den ayrıdır; konteyner/ağ kaldırıldı, DB ve anahtar volume'ları silinmedi.

İlk denemelerde test yardımcı kodunda CookieJar kopyalama ve Netscape çerez dosyasında boş süre alanı sorunları görüldü; düzeltildi ve paket tekrar tamamen geçti. Bu başarısız denemeler başarılı test sayılmadı. Son Türkçe model doğrulama değişikliğinden sonra tüm F2 paketi yeniden çalıştırıldı.

### Yerel yükseltme

Güncellemeden önce web durduruldu, backups/before-f2-b2cb25677a80.dump alındı ve before_f2_b2cb25677a80 adlı ayrı DB'ye restore edildi; kaynak/geri yüklenen migration sayısı 1 ile eşleşti. Ardından InventoryAndOwner ve StockCount uygulandı, yönetici hesabı oluşturuldu ve web yeniden başlatıldı. Mevcut veri/volume silinmedi.

localhost:5080 üzerinde gerçek yerel giriş, boş ürün listesi ve ready yanıtı doğrulandı. Sentetik ürünler normal DB'ye taşınmadı. Başlangıç şifresi .env ve .local-notes/ilk-giris.txt içinde 0600 izinli, Git dışında tutuluyor; çıktıya yazılmadı. Kullanıcı şifresini arayüzden değiştirebilir. İşlem sonrası web ve DB çalışır bırakıldı.

Windows/AMD64 çalıştırma, OS açılışı, tam internet kesintisi ve görsel tarayıcı kontrolü yapılmadı. Önceki oturumda tarayıcı sağlayıcısı yoktu/Safari erişimi verilmedi; bu görevde HTTP form akışları doğrulandı, görsel kontrol yapılmış sayılmadı. Satış fişi, iade, cari/tahsilat ve pigment modülleri eklenmedi.

## Aktif ve sonraki görev

F3 satış/iade tamamlandı; ayrıntılı 10 Ekim kanıtları aşağıdadır. Çalışma dalı feat/f2-inventory korunur; main geridedir. Sıradaki iş F4 rapor/pilot/yedek kapsamını netleştirmektir. Önceki F2 kod commit’i edaf19383869094c8d8a27bd8c9db4a967547907 ve F2 devir commit’i d5ad5b57e714f92f8cf9e3b3c974c18273ff58b6 geçmişte korunur.

## Kalan kararlar

10 Ekim F3 hazırlığında yerel HEAD ve GitHub feat/f2-inventory kimliği d5ad5b57e714f92f8cf9e3b3c974c18273ff58b6 olarak eşleşti; başlangıç çalışma ağacı temizdi. Uzak main 0057e440fa5b93b4cd1489e60224b1cf48e8d69d ile geridedir. Docker DB healthy, web çalışır ve readiness ready olarak doğrulandı. İlk sandbox ağ/Docker denemeleri erişemedi; izinli salt okunur kontroller başarılı oldu. Veri, .env, hesap ve volume değiştirilmedi; kurulum/migration tekrarlanmadı.

Oturumun ilk hazırlık adımında kullanıcı yanıtları K19–K23 olarak kaydedildi ve takip soruları soruldu. Aynı oturumda gelen takip yanıtlarıyla kapsam netleşti; aşağıdaki F3 uygulaması ve doğrulaması tamamlandı.

[project_context.md](project_context.md) açık soruların kaynağıdır. Windows gereksinimleri, işletim sistemi açılışı ve çevrimdışı pilot F4 öncesinde doğrulanmalı. Hassas notlar ortak belgelere yazılmaz.

## Yeni sohbet devri — 10 Ekim 2026

Docker yeniden başladıktan sonra db healthy ve web çalışır; /health/ready HTTP 200 / ready doğrulandı. İşletme verisi değiştirilmedi. Eski F1-A başlangıç promptu güncel [devam promptuyla](codex_start_prompt.md) değiştirildi; [devir notu](next_session.md), bağlam ve okuma bağlantıları güncellendi. Tamamlanan aşamalar tekrar başlatılmamalı; F3 iş kuralları kullanıcı yanıtlarını bekler.

Bu kapanış yalnızca Markdown değişikliğidir; yukarıdaki kod testleri geçerli olup tam test paketi tekrar çalıştırılmadı. Yerel Markdown bağlantıları, diff biçimi ve teslim belgelerinde yerel parolaların bulunmaması kontrolleri geçti. Push öncesi git fetch origin sonrası dal ayrışması 0/0 olarak doğrulandı. Devir commit'i aynı çalışma dalına normal push ile gönderilir; son commit kimliği ve uzak eşleşmesi teslim yanıtında bildirilir.


## F3 — satış, indirim, renklendirme ve iade (10 Ekim 2026)

K19–K23 ve takip yanıtları uygulandı. Satış listesi/yeni satış/detay ekranları; hem satır hem fişte TL veya yüzde indirim; boya satırının tamamına elle KDV dahil renklendirme ücreti; renk snapshot'ı; nakit/kart ile ödeme sonrası tamamlama. Tamamlanmamış form stok değiştirmez ve kalıcı taslak değildir. Renklendirilmiş boya, ücret 0 olsa bile iade/iptal edilemez. Sağlam iade stok dönüşlü, hasarlı iade stok dönüşsüzdür. İptal kalan miktarları geri döndüren ayrı belgedir; geçmiş silinmez. Cari/veresiye/parçalı ödeme, POS entegrasyonu veya pigment eklenmedi.

Satış toplamı yalnız kuruş altı kadar müşteri lehine aşağı yuvarlanır, kuruş dağıtımı satır/fiş toplamını korur. Birikimli kısmi iade yukarı kuruşa tamamlanır, önceki iadeler düşülür; özgün tutar/net/KDV aşılmaz. Kaynak fiyatlar, renk, ad ve oranlar geçmişte korunur. Satış/iade/stok/UUID kaydı tek transaction; ürünler kimlik sırasıyla, iadede önce kaynak satış kilitlenir. Modelde yeni dört tablo ve stok kaynakları vardır. Yeni `20261010175529_SalesAndReturns`; önceki migration dosyaları değişmedi.

### F3 doğrulama kanıtları

| Kontrol | Sonuç |
|---|---|
| Locked restore, Release build, format, EF model uyumu | Geçti; 0 hata/uyarı, yeni bağımlılık yok |
| F1-B/F2 regresyonları | Giriş/CSRF/parola/kilit, KDV, stok/sayım/geçmiş, oturum kalıcılığı ve DB kesinti/toparlanma geçti |
| Satış önizleme/ödeme | Önizlemede stok değişmedi; nakit/kart seçmeden tamamlama ve kesirli miktar reddedildi |
| İndirim/renklendirme | Satır yüzde + fiş TL, fiş yüzde, KDV dahil ek ücret, kart/nakit, renk snapshot'ı ve aşırı indirim reddi geçti |
| Müşteri lehine kuruş | 30,015 → 30,01 TL; 10,01+10,00+10,00 kısmi iade; iki 0,005 TL satır → 0,01; ücretsiz ve çok küçük iadeler geçti |
| Renklendirilmiş boya | Pozitif ve sıfır ücretli renklendirilmiş satırların iade/iptali reddedildi |
| Tekrar ve eşzamanlılık | Aynı satış/iade/iptal tek kayıt; farklı içerik aynı UUID reddi; son stok ve kalan iade yarışında yalnız biri başarılı |
| Eski form ve toplu miktar | Sürüm değişimi reddi; aynı ürünün birden fazla satırı birlikte stok kontrolü; yetersiz ikinci satırda kısmi satış yok |
| Sağlam/hasarlı/pasif iade | Hasarlı iadede stok artmadı; sağlam iade döndü; pasif kart etkinleşmeden stok iadesi aldı; fazla/yabancı satış satırı reddedildi |
| Hata enjeksiyonu | Satış ve iade stok yazımında zorlanan hatada başlık/satır/hareket/operation/bakiye tamamen rollback; aynı istek sonra başarılı |
| DB geçmiş ve bütünlük | UPDATE/DELETE, eksik başlık ve tamamlanmış belgeye ek satır reddedildi; stok defteri=bakiye |
| Ayrı DB restore | Stok toplamı, satış/iade kayıt sayıları ve tutarları, miktarlar ve trigger sayısı eşleşti |
| F1-A regresyon | 3 sentetik kayıt/150.00 toplam, kalıcılık ve ayrı DB restore geçti |

Nihai `bash scripts/test-app.sh` çıkış 0; proje `gurboya-f1b-2ffb7317e066`, kanıt dizini `.tmp/f1-b.cUaZe3`, çalışma günlüğü `.tmp/f3-app-final.log`. `bash scripts/test-postgres.sh` çıkış 0; proje `gurboya-f1a-64220046992e`, dizin `.tmp/f1-a.V6cpVE`, günlük `.tmp/f3-postgres-run.log`. Test konteyner/ağları kaldırıldı, volume'ları korundu. Test verisi normal kurulumda oluşturulmadı.

İlk testte deferred SQL toplam karşılaştırması çok sütunlu alt sorgu hatası verdi; ROW ifadesiyle düzeltildi ve paket tekrar geçti. Son incelemede belgeye ek satır korumasının migration'a eklenişi düzeltildi; test artık özgül ret mesajını da denetler, nihai paket yeniden geçti. İlk başarısız deneme başarılı sayılmadı. Son ek kontrolde yeni SDK konteynerinde --no-restore derlemesi NuGet önbelleği olmadığı için başarısız oldu; locked restore + format doğrulaması + Release build yeniden çalıştırıldı ve 0 hata/uyarıyla geçti. Bir Python sözdizimi kontrolü sandbox dışındaki varsayılan cache yoluna yazamadı; yerel `.tmp/pycache` ile yeniden geçti.

### F3 yerel yükseltme

Web durduruldu; `backups/before-f3-37d7cf1481f2.dump` adlı yeni özel izinli yedek alındı. `before_f3_37d7cf1481f2` adlı yeni ayrı DB'ye --no-owner/--no-privileges ile restore edildi. Mevcut 8 app tablosunun tüm satır içerik özetleri ve 3 migration kaydı eşleşti. F3 migration'ı uygulandıktan sonra aynı mevcut tablo içerikleri tekrar eşleşti; mevcut kullanıcı/şifre özeti dahil veriler korundu, satış/iade tablosu boş kaldı. `.env` dosyasının içerik hash'i değişmedi. Admin/db-setup tekrarlanmadı, volume silinmedi, canlı DB'ye restore yapılmadı.

Migration geçmişi 4 kayıt; web yeniden başlatıldı ve localhost:5080/health/ready HTTP 200 / ready doğrulandı. Sonuç manifesti `.tmp/f3-upgrade-result.json` Git dışındadır. Yeni yedek F3 öncesi veriyi içerir; ilerideki işlemler için güncel yedek alınmalıdır.

Windows/AMD64, işletim sistemi açılışı, tam internet kesintisi ve görsel tarayıcı kontrolü yapılmadı. Bilgisayar kullanım envanterinde bağlı browser sağlayıcısı bulunmadı; native tarayıcı erişimi açılmadı. HTTP/form testleri görsel kabul değildir. Mac F3 tamamlanması Windows canlı kullanım kabulü değildir.

Ortak belgeler, F4 devam promptu ve devir notu güncellendi. Commit/push kimliği aşağıdaki Git kapanışında ve teslim yanıtında doğrulanır. Sonraki görev F4 öncesi rapor kapsamı, Windows hedefi, yedek sıklığı/saklama/harici ortam ve veri kaybı hedefini kullanıcıyla netleştirmektir.

### Git kapanışı

F3 kod/test/tasarım commit'i `7fe048a822de098f0d8703f8f299fb71ee9eb383`. Bu ilerleme ve F4 devir belgeleri onun üzerine ayrı docs commit'i olarak kaydedilir. Push öncesi origin çalışma dalı `d5ad5b57e714f92f8cf9e3b3c974c18273ff58b6` olarak yeniden kontrol edildi; başka uzak değişiklik görülmedi. Her iki commit çalışma dalına normal push ile gönderilecek; son devir kimliği ve uzak eşleşmesi teslim yanıtında bildirilecek. Commit öncesi sır/dışlama, belge bağlantıları ve staged diff kontrolleri geçti.
