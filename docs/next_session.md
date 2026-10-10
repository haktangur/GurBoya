# F3 kapanışı ve yeni sohbet devri

Devir tarihi: 10 Ekim 2026. Önce [AGENTS.md](AGENTS.md), ardından [devam promptunu](codex_start_prompt.md) ve onun okuma sırasını uygula.

## Tamamlanan kapsam ve Git

F1-A/F1-B/F2/F3 Mac ARM64 üzerinde tamamlandı. Çalışma dalı `feat/f2-inventory`, origin `https://github.com/haktangur/GurBoya.git`. F3, F2 devir commit'i `d5ad5b57e714f92f8cf9e3b3c974c18273ff58b6` üzerinden geliştirildi. F3 kod/test/tasarım commit'i `7fe048a822de098f0d8703f8f299fb71ee9eb383`; ilk devir commit'i `8d82e20fcfa7fa052ddcc023fae571ab5961f39e` 10 Ekim’de GitHub çalışma dalı ile eşleşerek doğrulandı. Bu son kapanış belgesi onun üzerine eklenir. En son F3 ve belge commit'lerini `git log` ve `git ls-remote` ile kontrol et; sabit eski kimliğe reset yapma. main geliştirmeleri içermez. Çalışma dalına normal push yetkilidir; force push yoktur.

- Ürün/stok: elle boya marka/litre/renk/fiyat, diğerlerinde kutu/adet/gram; tam sayı, eksi stok engeli; fiyat/stok geçmişi ve sayım.
- Tek şifreli yonetici hesabı; şifre değişimi, CSRF ve kalıcı oturum anahtarları.
- KDV ekle/dahil, kullanıcı seçimi başlangıç %15, ürün kartında değişebilir.
- F3: satır ve fişte yüzde/TL indirim; boya satırına KDV dahil elle renklendirme ücreti ve renk; nakit/kart ödeme alındığında tamamlama; sağlam/hasarlı iade ve ters belgeli iptal.
- Renklendirilmiş boya ücret 0 olsa da iade/iptal edilemez. Ücret kartın fiyatını kalıcı değiştirmez, tüm satış satırına aittir.
- Fiş yalnız kuruş altı kadar aşağı, birikimli kısmi iade yukarı yuvarlanır; satır payları toplamı korur, iadeler özgün bedeli aşamaz. Detaylar decisions.md ve database.md'de.
- Tamamlanmamış sepet formdadır; DB taslağı değildir, stok ayırmaz, sayfa kapanınca kaybolur. Cari/veresiye/parçalı tahsilat, POS entegrasyonu ve pigment yoktur.

## Yerel kurulum ve koruma

Uygulama `http://localhost:5080`, yalnız 127.0.0.1'e açık. DB hosta yayımlanmaz. `.env`, `.local-notes/`, `.tmp/` ve `backups/` Git dışındadır. Sırları durum kontrolü için okuma/çıktılama. İlk giriş notu güncel şifre olmayabilir; hesabı yeniden oluşturma veya sıfırlama. İşletme verisini boş/silinebilir varsayma. Volume silme ve canlı DB üstüne restore yoktur.

```sh
docker compose -f compose.yaml -f compose.app.yaml ps
curl --fail --silent http://localhost:5080/health/ready
# Yalnız servisler durmuşsa, mevcut imajlarla:
docker compose -f compose.yaml -f compose.app.yaml up -d --pull never db web
```

Uygulanmış migration'lar:

- `20261007231325_InitialInfrastructure`
- `20261009120632_InventoryAndOwner`
- `20261009121326_StockCount`
- `20261010175529_SalesAndReturns`

Uygulanmış dosyaları değiştirme; yeni şema yeni migration gerektirir. Web başlangıcı migration çalıştırmaz. Son yerel yükseltmenin benzersiz yedek/ayrı restore DB kanıtı progress.md'dedir. Sonraki değişiklik için yeniden güncel yedek gerekir; eski yedeği güncel veri sanma.

## Kod ve test haritası

`Inventory/` ürün/stok, `Sales/` satış/iade/decimal hesap, `Pages/Sales/` satış ekranları; `Data/AppDbContext.cs`, `Sales/SalesModel.cs` ve `Data/Migrations/` model/şema. Razor Pages + EF Core + Npgsql; sabit SDK 10.0.401, runtime/EF 10.0.9 ve Npgsql 10.0.3. Ek bağımlılık eklenmedi.

Satış ve iade belge/satır/stok/UUID kayıtları tek transaction'dır. UUID/hash replay, kaynak satış ve sıralı ürün kilitleri, eski form sürümü kontrolü ve DB deferred bütünlük trigger'ları korunmalı. Renklendirilmiş boya ve iade sınırı DB'de de korunur. Pasif ürüne sağlam iade stoğu dönebilir, kart etkinleştirilmez. Satış toplamını kasa bakiyesi veya maliyet yöntemi olmadan brüt kâr olarak sunma.

`bash scripts/test-app.sh`: F1-B/F2 + `scripts/test-sales.py`; izole gerçek PostgreSQL/HTTP, KDV/indirim/renklendirme/iade, tekrarlar, yarış, rollback hata enjeksiyonu, değişmezlik ve restore. `bash scripts/test-postgres.sh`: F1-A kalıcılık/restore. Son sonuçlar progress.md'de. Testler normal DB'de çalışmaz; test volume'ları otomatik silinmez.

Windows/AMD64 çalıştırma, OS açılışı, tam internet kesintisi ve görsel tarayıcı kontrolü yapılmadı. Bu oturumdaki bilgisayar envanterinde bağlı browser sağlayıcısı yoktu. HTTP testlerini görsel test sayma; Mac kabulünü Windows canlı kabulü sayma.

## Sıradaki F4

İlk sürüm için bir zorunlu faz (F4) kaldı; F5 yalnız ayrıca istenirse yapılacak genişlemelerdir. Kullanıcı giriş bilgisini aldı fakat ekran kontrolü sonucunu henüz bildirmedi. Yeni sohbette varsa bu geri bildirimi al; görsel kabulü yapılmış sayma. Şifreyi ortak belgelere veya başlangıç promptuna ekleme.

Kullanıcıyla temel stok/satış raporlarını, Windows sürümü/donanımı ve yedek saklama/harici ortam/kabul edilen veri kaybını netleştir. Sonra Windows pilotu, çevrimdışı açılış, kullanıcı kabulü ve yedek/kurtarma planını uygula. Onaylanmış F2/F3 iş kurallarını yeniden sorma, tamamlanmış modülleri baştan yazma. Cari/POS/pigment modülü kendiliğinden ekleme.
