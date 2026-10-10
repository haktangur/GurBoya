# F2 kapanışı ve yeni sohbet devri

Devir tarihi: 10 Ekim 2026. Önce [AGENTS.md](AGENTS.md), ardından onun okuma sırası uygulanır. Kullanıcıya verilecek metin [devam promptundadır](codex_start_prompt.md).

## Tamamlanan aşama ve Git

F1-A altyapı, F1-B uygulama iskeleti ve F2 ürün/stok/şifreli giriş Mac ARM64 üzerinde tamamlandı. F2 kod commit'i `edaf19383869094c8d8a27bd8c9db4a967547907`, çalışma dalı `feat/f2-inventory`, origin `https://github.com/haktangur/GurBoya.git`. 10 Ekim'de uzak dal kimliği bu kod commit'iyle eşleşti; bu devir belgeleri onun üzerine ayrı commit olarak eklenir. Son belge commit'ini `git log` ve uzak dal ile kontrol et; sabit kimliğe reset yapma.

F1-B `84692fb`, F1-A `2649482` bu dalın geçmişindedir. Geliştirme dalları main'e birleştirilmedi; main güncel uygulama tabanı değildir. Yeni iş dalı gerekiyorsa uzak güncellemeleri kontrol ederek tamamlanan kod ve devir belgelerinin son commit'inden aç. Normal push yetkisi devam eder; force push yoktur. Kullanıcı değişikliklerini koru.

## Çalışan yerel kurulum

10 Ekim kontrolünde DB healthy, web çalışır ve `GET http://localhost:5080/health/ready` HTTP 200 / ready döndü. Uygulama yalnızca `127.0.0.1:5080` üzerinde; DB hosta yayımlanmaz. Giriş: `http://localhost:5080/Account/Login`.

Tek hesap `yonetici`; ekran yalnızca şifre ister. İlk giriş notu `.local-notes/ilk-giris.txt` içindedir; şifre sonradan değiştirilmiş olabilir. `.env`, bu not, `.tmp/` ve `backups/` Git dışındadır. Sırları okumak/çıktılamak normal durum kontrolü için gerekli değildir. Git klonu yerel veri, sır veya Docker volume'larını taşımaz; başka bilgisayar kurulumu ayrı iştir.

Bu makinede F2 yükseltmesi ve yönetici oluşturma yapıldı. `.env` dosyasını ezme; db-setup/admin/migrate komutlarını gereksiz yere tekrarlama. 9 Ekim teslim kontrolünde ürün listesi boştu; kullanıcı sonradan veri girmiş olabilir. Güncel veriyi boş veya silinebilir kabul etme.

Depo kökünden durum/başlatma:

```sh
docker compose -f compose.yaml -f compose.app.yaml ps
curl --fail --silent http://localhost:5080/health/ready
# Yalnızca servisler kapalıysa, mevcut yerel imajlarla:
docker compose -f compose.yaml -f compose.app.yaml up -d --pull never db web
```

Önceki yükseltme yedeği `backups/before-f2-b2cb25677a80.dump`; ayrı `before_f2_b2cb25677a80` DB'sine dönüşü doğrulandı. Bu F2 öncesi yedektir, güncel verinin yedeği değildir. Sonraki şema değişiminde yeni benzersiz yedek ve ayrı DB'de dönüş provası gerekir. Volume silme ve canlı DB üstüne restore yapma. İşletim ayrıntıları [README](README.md) içinde.

## Korunacak uygulama ve kararlar

Razor Pages + EF Core + Npgsql onaylıdır. SDK 10.0.401, runtime/EF/Identity 10.0.9, Npgsql 10.0.3; Dockerfile ve lock dosyaları sabittir. Host SDK'sını değiştirmeden Docker SDK ile doğrulama yapıldı.

- Boya marka/litre/renk/fiyat elle; litre ambalaj hacmi, stok/fiyat kutu başına.
- Diğer ürünler kutu/adet/gram; miktar tam sayı, gram hassasiyeti 1. Negatif stok engellenir, stok girişi ürünü tekrar kullanılabilir kılar.
- TL birim fiyatında KDV ekle/dahil; başlangıç %15 kullanıcı tercihi, karttan değişebilir. Net/KDV/toplam ayrı, decimal dört basamak. Fiş yuvarlaması henüz onaylanmadı.
- Tek şifreli hesap; ürün arama/düzenleme/pasifleştirme, stok giriş/düzeltme/sayım, değişmez fiyat/stok geçmişi var. Satış/iade/cari/tahsilat ve pigment modülü yok.

Kod haritası: `src/GurBoya.Web/Inventory/` iş kuralları; `Pages/Products/` ürün/stok ekranları; `Pages/Account/` giriş/şifre; `Data/AppDbContext.cs` model; `Data/Migrations/` şema. [database.md](database.md) içindeki “F2 uygulanan model” ve kod geçerlidir; eski öneri tablolarını topluca oluşturma.

Uygulanmış ve değiştirilmeyecek migration'lar:

- `20261007231325_InitialInfrastructure`
- `20261009120632_InventoryAndOwner`
- `20261009121326_StockCount`

Yeni şema yeni migration gerektirir; web açılışı migration çalıştırmaz. Stok transaction, ürün kilidi, UUID tekrar koruması, bakiye mutabakatı ve DB geçmiş korumalarını koru.

## Test durumu ve sınırlar

F2 kod commit'i için `bash scripts/test-app.sh` ve `bash scripts/test-postgres.sh` çıkış 0; locked restore/Release build, format, EF model uyumu geçti. Gerçek PostgreSQL/HTTP testleri giriş/CSRF, KDV, tekrar gönderim, eşzamanlı son stok, sayım, geçmiş, rollback, mutabakat, ayrı DB restore ve oturum kalıcılığını kapsar. Ayrıntılı sonuç/yerel kanıt yolları [progress.md](progress.md) içinde. Testler izole projelerde çalışır; volume'lar bilinçli korunur, otomatik temizlikle silinmez.

Devir yalnızca belge değişikliğidir; tam kabul paketi tekrar çalıştırılmadı. Yerel hazır olma kontrolü yenilendi; belge bağlantıları, Git diff ve yerel parola dışlama kontrolleri geçti. Windows/AMD64 çalıştırma, OS açılışı, tam internet kesintisi ve görsel tarayıcı kontrolü yapılmadı. Önceki Safari erişimi verilmedi; HTTP testini görsel test sayma. Mac F2 tamamlanması Windows canlı kullanım kabulü değildir.

## Sonraki sohbetin ilk işi

F3 satış/iade koduna geçmeden kullanıcıya anlaşılır şekilde şu iş kurallarını sor; yanıtları [decisions.md](decisions.md) ve kapsam belgelerine işle:

1. İndirim olacak mı; TL/yüzde, ürün satırı/fiş toplamı seçeneklerinden hangileri gerekli?
2. Satışta net/KDV/toplam ve kısmi iade kuruşları nasıl yuvarlanacak? Somut örnekle öneri sun; F2 birim fiyatındaki dört basamağı fiş kuralı sanma.
3. Renklendirilmiş boya iadesi kabul ediliyor mu? Hasarlı ürün iadesinde para iadesi ile satılabilir stoğa dönüş ayrı mı yönetilecek?
4. Renklendirme için ayrı ücret alınacak mı?
5. Nakit/kart/veresiye ve kısmi ödeme gerekiyor mu? Cari/tahsilat MVP için zorunlu mu?

Onaylı marka/birim/şifre/KDV kararlarını tekrar sorma. Belirsiz kurallara bağlı geliştirmeyi yanıt gelmeden başlatma. Satış toplamını kasa bakiyesi veya maliyet yöntemi seçilmeden brüt kâr olarak sunma. Bu devir aşamasında F3 kodu başlatılmadı.
