# GürBoya

Türkçe boya ve hırdavat dükkânı yönetim uygulaması. Tek bilgisayarda tarayıcıdan açılır; uygulama ve PostgreSQL Docker Compose ile yerel çalışır. Bulut gerekmez. Kurulum tamamlandıktan sonra günlük işlemler internet gerektirmez.

## Mevcut durum — 10 Ekim 2026

F2 ürün/stok modülü ve şifreli giriş hazırlandı: elle boya marka/litre/renk/fiyat, kutu/adet/gram, stok girişi/düzeltme/sayım ve fiyat geçmişi. KDV ekle/dahil seçimiyle net, KDV ve toplam ayrı gösterilir; başlangıç oranı %15, karttan değiştirilebilir. F3 satış/iade hazır: satır ve fiş indirimi, KDV dahil elle renklendirme ücreti, nakit/kart tamamlama, sağlam/hasarlı iade ve iptal. Güncel kabul sonuçları [ilerleme belgesinde](progress.md).

## Okuma sırası

1. [Ajan kuralları](AGENTS.md)
2. [Bağlam](project_context.md)
3. [Kararlar](decisions.md)
4. [Mimari](architecture.md)
5. [Veritabanı taslağı](database.md)
6. [Faz planı](project_plan.md)
7. [Geliştirme kuralları](development_rules.md)
8. [İlerleme](progress.md)
9. [Yeni sohbete devam promptu](codex_start_prompt.md)

[ChatGPT çalışma kuralları](chatgpt.md) aynı karar ve standartlara yönlendirir. Tüm Markdown dosyaları kullanıcının istediği gibi docs/ altında tutulur. Kök AGENTS.md olmadığı için başlangıç promptu docs/AGENTS.md dosyasının açıkça okunmasını ister; kökten başlayan oturumda otomatik yüklenmesine güvenilmez.

Yeni sohbet için [devir notunu](next_session.md) oku ve [devam promptunu](codex_start_prompt.md) kullan. Güncel geliştirme dalı `feat/f2-inventory`; main henüz bu geliştirmeleri içermez.

## Dosya ve Git düzeni

Kökte temel compose.yaml, uygulama için compose.app.yaml ve Dockerfile; src/GurBoya.Web altında uygulama, scripts/ altında kurulum/kabul testleri bulunur. Gerçek sırlar, test verileri ve yedekler Git dışında kalır. [GitHub deposu](https://github.com/haktangur/GurBoya) için normal push yetkilidir.

## İlk kurulum ve çalıştırma

Docker Desktop Linux konteyner modunda çalışıyor olmalı. Komutları depo kökünde çalıştır. Mac ARM64 üzerinde doğrulanmıştır; Windows/AMD64 çalıştırma ve açılış testleri bekliyor. Windows için [Docker Desktop gereksinimlerini](https://docs.docker.com/desktop/setup/install/windows-install/) ayrıca kontrol et.

Mac/Linux:

```sh
cp .env.example .env
chmod 600 .env
```

Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Mevcut .env dosyasını kopyalayarak ezme. .env içinde POSTGRES_PASSWORD alanına parola yöneticisiyle üretilmiş benzersiz, uzun bir parola yaz; boş örnek bilinçli olarak çalışmaz. Özel karakter/Compose yorumlama sorunlarını önlemek için en az 32 rastgele alfasayısal karakter kullanılabilir. .env dosyasını paylaşma veya Git'e ekleme; Windows dosya izinlerini yalnızca ilgili kullanıcıyla sınırla. Aşağıdaki yönetim komutları örnekteki gurboya_admin/gurboya adlarını kullanır; adları değiştirirsen komutları da uyarla.

```sh
docker version
docker compose version
docker compose config --quiet
docker compose pull db
docker compose up -d --wait --wait-timeout 120 db
docker compose ps
docker compose exec db psql -X -U gurboya_admin -d gurboya -c "SELECT current_database(), version();"
```

config --quiet yapılandırmayı denetler; düz config çıktısı parolayı açığa çıkarabileceği için paylaşılmaz. Host portu yayımlanmaz; yönetim compose exec üzerinden yapılır. Uygulama aynı database ağı üzerinden db:5432 adresine bağlanır. gurboya_admin başlangıç superuser hesabıdır; uygulama hesabı değildir. Ayrı uygulama/migration hesapları aşağıdaki F1-B kurulumuyla oluşturulur.

```sh
# Volume korunarak durdurma:
docker compose stop
# Yereldeki imaj ile yeniden başlatma (indirme yapmaz):
docker compose up -d --pull never --wait db
# Volume korunarak konteyneri yeniden oluşturma:
docker compose up -d --force-recreate --pull never --wait db
# Konteyner ve ağı kaldırır, volume kalır:
docker compose down
```

Volume gurboya_postgres_data adındadır. Compose proje adını/depo ayarını değiştirmek başka bir volume seçebilir; boş DB görmek eski verinin silindiği anlamına gelmez. Volume silme komutlarını kullanma. İlk indirme internet gerektirir; --pull never testi tam internet kesintisi testi değildir. Docker Desktop'ın işletim sistemi açılışında başlaması ayrıca ayarlanıp Windows'ta doğrulanmalıdır.

İmaj postgres:18.6-bookworm ve çok mimarili index digest ile sabitlenmiştir; AMD64 zorlaması yoktur. PostgreSQL 18 için volume /var/lib/postgresql, veri dizini /var/lib/postgresql/18/docker'dır. [Resmî imaj belgesi](https://github.com/docker-library/docs/blob/master/postgres/README.md#pgdata) bu yolu tanımlar. Sürüm/digest güncellemeleri bilinçli yapılmalı, yedek ve kabul testiyle doğrulanmalı; major yükseltme yalnızca etiketi değiştirmekle yapılmaz. .env değişikliği mevcut volume üzerindeki DB parolasını, kullanıcı veya DB adını değiştirmez.

## Yedek ve ayrı veritabanına dönüş

Örnekler varsayılan adları kullanır. Her yedek için yeni bir dosya adı seç; aşağıdaki tarih/sıra örneğini tekrar kullanıp eski yedeği ezme. backups klasörünü ilk kullanımda oluştur. Binary dump konteyner içinde üretilip compose cp ile taşınır; böylece Windows PowerShell yönlendirmesi dosyayı bozmaz.

```sh
mkdir backups
docker compose exec -T db pg_dump -U gurboya_admin -d gurboya --format=custom --file=/tmp/gurboya.dump
docker compose cp db:/tmp/gurboya.dump backups/gurboya-20261008-01.dump
```

Her komutun başarılı olduğunu kontrol et; pg_dump başarısızsa sonraki kopyalama adımına geçme. Yedeği güvenli harici ortama da kopyala. Volume ve aynı diskteki yedek disk arızasına karşı koruma sağlamaz. Günlük otomasyon, saklama süresi ve veri kaybı hedefi F4 öncesinde kararlaştırılacak. Dump sunucu rollerini/parolaları kapsamaz; yerel kurulum sırları ayrıca güvenli saklanır.

Prova için daha önce kullanılmamış bir DB adı seç. createdb mevcut DB varsa hata verir; bu durumda dur, mevcut DB'nin üzerine yükleme yapma. --clean veya --create ile canlı DB'yi hedefleme.

```sh
docker compose cp backups/gurboya-20261008-01.dump db:/tmp/gurboya-restore.dump
docker compose exec -T db createdb -U gurboya_admin gurboya_restore_check
docker compose exec -T db pg_restore -U gurboya_admin -d gurboya_restore_check --exit-on-error --single-transaction --no-owner --no-privileges /tmp/gurboya-restore.dump
docker compose exec -T db psql -X -U gurboya_admin -d gurboya_restore_check -c "SELECT current_database();"
```

Yükleme hatasız bitince kaynak/yedek içeriğine uygun satır sayısı ve toplamları karşılaştır. F1-A'da ana DB boş; aşağıdaki kabul testi yalnızca izole test DB'sine üç sentetik kayıt yazar ve geri yüklenen değerleri karşılaştırır. İşletme verisiyle prova yapılmaz.

## Tekrarlanabilir altyapı kabul testi

Mac/Linux üzerinde Bash, OpenSSL ve Docker Compose gerekir. Windows'ta bu script henüz doğrulanmadı; Git Bash/WSL ortamı ayrıca test edilmelidir.

```sh
bash scripts/test-postgres.sh
```

Script benzersiz bir gurboya-f1a-* projesi, rastgele yerel parola ve .tmp/ altında özel izinli test dosyaları oluşturur. Normal gurboya projesini değiştirmez. Compose/boş parola, healthcheck, TCP bağlantısı/yanlış parola, yayımlanmayan port/iç ağ, yeni konteynerde kalıcılık ve ayrı DB'ye custom dump geri yükleme kontrol edilir. Üç satır, 150.00 toplam ve Türkçe metinler birebir karşılaştırılır; ana DB'de iş tablosu bulunmadığı doğrulanır.

Çıkışta yalnızca test konteynerleri/ağı kaldırılır; test volume'u, .tmp/ içindeki yerel parola ve dump korunur. Her tekrar ayrı volume bırakır. Script sonunda yazılan proje/dosya adlarıyla incelenebilir; bunların silinmesi ayrıca bilinçli yapılmalıdır. Test hatası sıfır olmayan çıkış koduyla bildirilir.

## F1-B uygulama kurulumu

Bu bölüm önceki yalnızca DB komutlarına uygulama katmanını ekler. Mevcut .env dosyasını koru; .env.example'daki APP_DB_PASSWORD ve MIGRATION_DB_PASSWORD alanlarını ekleyip yönetim parolasından farklı uzun rastgele parolalar yaz. WEB_PORT varsayılanı 5080'dir. Üç parola da zorunludur. ADMIN_PASSWORD yalnızca ilk yönetici oluşturma komutunda kullanılır; web hizmetine verilmez. İlk kurulum/derleme internet ister; imajlar hazırlandıktan sonra günlük kullanım CDN veya NuGet erişimi gerektirmez.

Aşağıdaki komutlar Mac terminalinde ve PowerShell'de depo kökünden çalışır. Her adım başarılı olmadan sonrakine geçme. Güncel migration’lar gurboya DB’sine kullanıcı/ürün/stok tablolarını da ekler; mevcut işletme verisi olan kurulumda önce doğrulanmış yedek alınmalıdır.

```sh
docker compose -f compose.yaml -f compose.app.yaml config --quiet
docker compose -f compose.yaml -f compose.app.yaml build web
docker compose -f compose.yaml -f compose.app.yaml up -d --wait db
docker compose -f compose.yaml -f compose.app.yaml run --rm db-setup
docker compose -f compose.yaml -f compose.app.yaml run --rm migrate
docker compose -f compose.yaml -f compose.app.yaml up -d web
```

Tarayıcı adresi: http://localhost:5080. GET /health/live uygulama canlılığını, GET /health/ready veritabanı ve migration hazırlığını bildirir. Hazır durumda 200, bağlantı/migration eksikliğinde 503 döner. Bağlantı kesintisi ana sayfada Türkçe mesaj ve yeniden deneme bağlantısı gösterir. Status ekranı mağaza modüllerinin kullanıma hazır olduğu anlamına gelmez.

```sh
# İmaj indirmeden normal başlatma:
docker compose -f compose.yaml -f compose.app.yaml up -d --pull never db web
# Veri korunarak durdurma:
docker compose -f compose.yaml -f compose.app.yaml stop
```

Kurulumdaki db-setup, projeye ait DB/rollerin yetkilerini ayarlar; tekrar çalıştırıldığında parolaları .env değerlerine getirir. Mevcut DB yönetim parolası .env değiştirilerek değişmez. Uygulama/migration parola değişiminden sonra db-setup çalıştırıp web'i --force-recreate ile yeniden oluştur. Migration normal web açılışında uygulanmaz; incelenmiş yeni migration için yedek aldıktan sonra migrate bakım komutunu ayrıca çalıştır. Sırlar komut satırına verilmez; çözülmüş Compose yapılandırmasını ve konteyner environment çıktısını paylaşma.

Uygulama portu yalnızca loopback'e bağlıdır; DB portu yayımlanmaz. Ürün ve stok sayfaları ASP.NET Identity oturumu gerektirir; form POST işlemleri CSRF korumalıdır. Data Protection anahtarları protection_keys volume’unda /keys altında, yalnızca uygulama kullanıcısına açık 0700 dizinde tutulur. Bu yerel kurulumda anahtar dosyaları ayrıca şifrelenmez; Docker/işletim sistemi hesabı erişimi korunmalı ve canlı Windows kurulumunun disk güvenliği ayrıca doğrulanmalıdır. Npgsql bağlantısında GSS/Kerberos kapalıdır; yerel iç ağdaki SCRAM parolalı bağlantı kullanılır.

## Geliştirme, migration ve doğrulama

Sürümler: SDK 10.0.401, ASP.NET runtime/EF Core/dotnet-ef 10.0.9, Npgsql EF sağlayıcısı 10.0.3. SDK global.json ile, imajlar Dockerfile digestleriyle, NuGet grafiği packages.lock.json ile sabitlenir. Yerel SDK tam sürümü yoksa Docker SDK ile çalışılabilir; sistem SDK'sı bu görevde değiştirilmedi.

Tam SDK kurulu ortamda:

```sh
dotnet restore src/GurBoya.Web --locked-mode
dotnet tool restore
dotnet build src/GurBoya.Web -c Release --no-restore
dotnet format src/GurBoya.Web --verify-no-changes --no-restore
dotnet ef migrations has-pending-model-changes --project src/GurBoya.Web
```

Yeni şema için `dotnet ef migrations add MigrationName --project src/GurBoya.Web --output-dir Data/Migrations` kullan, üretilen SQL/değişiklikleri incele. Design-time factory yalnızca migration üretimi/model kontrolü içindir; gerçek parolaya veya DB'ye bağlanmaz. Gerçek uygulama için Compose migrate hizmetini kullan. Uygulanmış migration silinmez/değiştirilmez. Başlangıç migration'ı kasıtlı olarak boş modelin sürüm kaydıdır.

Mac terminalinde aynı kontroller için SDK konteyneri örneği (önce bağımlılık restore gerekir):

```sh
docker run --rm -v "$PWD:/source" -w /source mcr.microsoft.com/dotnet/sdk:10.0.401 sh -c 'dotnet restore src/GurBoya.Web --locked-mode && dotnet tool restore && dotnet build src/GurBoya.Web -c Release --no-restore && dotnet format src/GurBoya.Web --verify-no-changes --no-restore && dotnet ef migrations has-pending-model-changes --project src/GurBoya.Web'
```

Gerçek PostgreSQL + HTTP kabul testleri (Mac Bash, OpenSSL, curl, Docker):

```sh
bash scripts/test-app.sh
bash scripts/test-postgres.sh
```

F1-B testi rastgele host portu ve benzersiz gurboya-f1b-* projesi kullanır. Web açılışının DDL yapmaması, eksik migration, tekrar migration, Türkçe ekran/404/yerel CSS, root olmayan kullanıcı, loopback portu, DB hesap yetkileri, bağlantı kesintisi/toparlanma ve loglarda parola bulunmaması sınanır. Test DB'si işletme verisi içermez. Çıkışta konteyner/ağ kaldırılır, volume ve .tmp/ altında log/sırlar korunur. Testler Windows doğrulaması yerine geçmez.

## F2 ilk giriş ve kullanım

İlk kurulumda .env içindeki ADMIN_PASSWORD için en az 10 karakterli, harf/rakam içeren benzersiz bir şifre belirle. Ardından DB hesap hazırlığı ve migration adımlarından sonra:

```sh
docker compose -f compose.yaml -f compose.app.yaml run --rm admin
docker compose -f compose.yaml -f compose.app.yaml up -d --force-recreate web
```

Hesap adı yonetici'dir; giriş ekranı yalnızca şifre ister. admin komutu mevcut hesabı/şifreyi değiştirmez. Bu geliştirme tesliminde üretilen ilk giriş bilgileri .local-notes/ilk-giris.txt dosyasındadır (0600, Git dışında); terminal çıktısına veya belgelere şifre yazılmadı. Giriş yaptıktan sonra Şifre menüsünden değiştirebilirsin. Beş hatalı giriş hesabı 5 dakika kilitler. Oturum 8 saatlik kayan süreyle sınırlıdır; Çıkış düğmesi oturumu kapatır. Açık kullanıcı kaydı veya bulut girişi yoktur.

1. **Ürünler ve stok → Yeni ürün:** ad, tür, marka, renk, ambalaj litresi, birim, isteğe bağlı kategori/barkod gir. Boyada marka/renk/pozitif litre/kutu zorunludur. Diğer ürünlerde kutu, adet veya gram seçilebilir; gram hassasiyeti 1 gramdır.
2. **Fiyat:** alış ve satış tutarlarını seçilen birim başına TL gir. KDV seçimi her iki fiyata uygulanır. KDV ekle ile 100 TL + %15 = 115 TL; KDV dahil 115 TL içinden net 100 TL ve 15 TL KDV ayrılır. Fiyat ayrıntısını göster düğmesi kaydetmeden hesaplar. Oran karttan düzeltilebilir; eski fiyat geçmişi değişmez. Birim fiyatında dört ondalığa kadar hassasiyet desteklenir (ör. gram başına 0,025 TL); binlik ayırıcı kullanma.
3. **Stok işlemi:** giriş için pozitif tam miktar; gerekçeli düzeltme için artı/eksi fark; sayım için raftaki toplam miktarı gir. Her işlemde açıklama gerekir. Sayımda sıfır ve değişmeyen miktar da kaydedilir. Negatif stok oluşmaz. Stok bittiğinde “Stokta yok” görünür; giriş sonrası tekrar kullanılabilir. Bu işlemler satış fişi oluşturmaz.
4. **Geçmiş:** stok ve fiyat kayıtları silinmez. Ürünü silmek yerine pasifleştir; geçmiş görünür kalır. Yeniden etkinleştirme mümkündür. Hareketi olan ürünün birimi/türü/litresi değişmez; farklı ambalaj için yeni kart açılır. Form eski kaldıysa yenileyerek tekrar dene.

Sayfalar 50 kayıtlık sayfalama kullanır. Arama ürün adı, marka, renk, kategori ve barkodu kapsar. Mevcut stok hareketlerinin toplamı kart bakiyesine eşit tutulur. Aynı formun tekrar gönderimi aynı işlem UUID'siyle çift stok/fiyat kaydı üretmez.

## F2 yükseltme ve yedek

Mevcut kurulumda önce web'i durdurup benzersiz isimli custom pg_dump yedeği al; ayrı DB'de pg_restore ile doğrula. Sonra güncel imajı derle, migrate, gerekiyorsa admin ve web başlatma komutlarını uygula. F2 migration'ları geçmiş migration dosyalarını değiştirmez. Veritabanı dump'ı kullanıcı parola özetlerini de içerir; yedekler hassastır ve Git dışında tutulur. Ayrı DB'ye prova canlı DB'nin üstüne yazmaz.

Felaket kurtarmada dump şema/verileri geri getirir, sunucu rollerini ve Data Protection volume'unu kapsamaz. --no-owner/--no-privileges ile yüklenen prova DB'si uygulama çalıştırmaya hazır yetki düzeni sayılmaz. Canlı kurtarmada rol sahiplikleri, app/infrastructure yetkileri ve geçmiş tablolarının UPDATE/DELETE yasakları ayrıca doğrulanmalıdır. Anahtar kaybı veritabanını silmez fakat eski oturumları geçersiz kılar.

scripts/test-app.sh artık F1-B altyapı kontrollerine F2 HTTP testlerini de ekler; Python 3 standart kütüphanesi gerekir. Giriş/CSRF/kilitleme, KDV/ondalık, ürün/stock replay, eşzamanlı son stok, sayım, fiyat geçmişi, pasif ürün, rollback, mutabakat, F2 dump/restore ve yeniden oluşturma sonrası oturum sınanır. Synthetic kullanıcı/parola/çerezler yalnızca .tmp/ altında tutulur. Test her seferinde ayrı DB ve anahtar volume'ları bırakır; otomatik volume silmez.


## F3 satış ve iade kullanımı

Satışlar → Yeni satış ekranında ürün/marka/renk/barkod arayıp sepete ekle. Aynı ürün ayrı renk veya indirimle birden fazla satırda bulunabilir; stok kontrolü ürünün tüm satırlarını toplar. Miktar kutu/adet/gram tam sayıdır. Sepet henüz tamamlanmamış işlemdir, DB'ye kaydedilmez; sayfa kapanırsa kaybolur ve stok etkilenmez.

- Satır ve fiş için TL veya yüzde indirim seç. Önce satır indirimi, sonra fiş indirimi uygulanır; indirim bedeli aşamaz. Fiş indirimi satırlara oransal dağıtılır.
- Boya satırında **Renklendirme ücreti ekle** düğmesine bas; satırın tamamı için KDV dahil ek tutar ve renk kodunu gir. Tekrar KDV eklenmez; mevcut boya oranıyla net/KDV ayrılır. Ücret boya fiyatına kalıcı yazılmaz. Renklendirildi işareti, ücret sıfır olsa bile iade ve iptali engeller.
- **Toplamı hesapla** ile net/KDV/toplamı gör. Fiş toplamı müşteri lehine yalnız kuruş altı kadar aşağı yuvarlanır. Satır kuruşları fiş toplamını koruyacak şekilde dağıtılır; bütün satırları tek tek aşağı yuvarlamanın birikmesi önlenir.
- Ödemeyi uygulama dışında aldıktan sonra nakit veya kart seçip **Ödeme alındı — satışı tamamla** düğmesine bas. Uygulama POS işlemi yapmaz; cari/veresiye, parçalı ödeme ve kasa bakiyesi tutmaz. Fiyat/stok değişmişse ilgili ürünü sepetten çıkarıp tekrar ekle.
- Satış detayında iade miktarını ve satılabilir stoğa dönen miktarı ayrı gir. Sağlam iade stoğa döner; hasarlı iade için stoğa dönüş 0'dır. Gerekçe ve nakit/kart para iadesi yöntemi seçilir; dışarıda yapılan para iadesi onaylanır. Pasif ürünün sağlam iadesi stoğa dönebilir ama kartı otomatik etkinleştirmez.
- İade tutarı özgün indirimli satırdan hesaplanır. Örneğin 30,01 TL'lik üç ürün ayrı ayrı iade edilirse 10,01 + 10,00 + 10,00 TL geri ödenir. Toplam iade özgün satış bedelini aşmaz. Ürün kartının sonraki fiyat/ad/renk değişiklikleri geçmiş satışı değiştirmez.
- **Kalan satışın tamamını iptal et ve stoğa döndür**, kalan tüm miktarı sağlam kabul ederek ters belge oluşturur. Renklendirilmiş boya varsa bu iptal kullanılamaz; diğer satırlar ayrı iade edilebilir. İptal ve iade geçmişi silmez.

Yeni migration: `20261010175529_SalesAndReturns`. Önceki üç migration değişmedi. Mevcut kurulumda yeniden ilk kurulum/admin çalıştırma; her şema yükseltmesinden önce web'i durdur, benzersiz güncel yedek al ve ayrı DB'ye dönüşünü doğrula. Ardından incelenmiş migrate ve web başlatma adımlarını uygula. Satışlar oluştuğunda migration geri alma işlemi veri kaybı doğurabilir; olağan düzeltmeler yeni migration ile yapılır.

`scripts/test-app.sh`, F1-B/F2 testlerine `scripts/test-sales.py` senaryolarını ekler. Satış/iade rollback hata enjeksiyonu yalnız izole `gurboya-f1b-*` projesinde yapılır. Görsel tarayıcı ve Windows pilotu ayrı kabul işleridir.

## F4 raporları ve yerel yedek

Giriş sonrası **Raporlar** menüsü: mevcut/biten stok ve tarih aralıklı satış/iade raporları. Satış/iade farkı, nakit/kart ayrımı ve ürün miktarları gösterilir; Excel/yazdırma yoktur. Türkiye saatiyle iki tarih dahildir; iadeler kendi işlem tarihinde sayılır.

Saatlik yerel yedek ve 30 gün saklama için ayrı Compose dosyası hazırlandı. Windows kurulumu kullanıcı isteğiyle ertelendi. Etkinleştirme, kontrol, kurtarma ve bekleyen kabul adımları [F4 işletim rehberindedir](f4_operations.md).
