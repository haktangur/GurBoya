# GürBoya

Türkçe boya ve hırdavat dükkânı yönetim uygulaması. Tek bilgisayarda tarayıcıdan açılır; uygulama ve PostgreSQL Docker Compose ile yerel çalışır. Bulut gerekmez. Kurulum tamamlandıktan sonra günlük işlemler internet gerektirmez.

## Mevcut durum — 8 Ekim 2026

F1-A için PostgreSQL 18.6 + Docker Compose altyapısı oluşturuldu. Uygulama ve iş tabloları henüz yok. Frontend/backend/ORM seçimi F1-B öncesinde onay bekliyor. Kabul testlerinin gerçek sonuçları [ilerleme belgesinde](progress.md) tutulur.

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

Kökte compose.yaml, .env.example ve .gitignore; scripts/ altında altyapı kabul testi bulunur. Gerçek sırlar, test verileri ve yedekler Git dışında kalır. [GitHub deposu](https://github.com/haktangur/GurBoya) için normal push yetkilidir.

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

config --quiet yapılandırmayı denetler; düz config çıktısı parolayı açığa çıkarabileceği için paylaşılmaz. Host portu yayımlanmaz; yönetim compose exec üzerinden yapılır. Gelecekteki uygulama aynı database ağı üzerinden db:5432 adresine bağlanacak. gurboya_admin başlangıç superuser hesabıdır; uygulama hesabı değildir. Asgari yetkili uygulama/migration hesapları F1-B'de oluşturulacak.

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
