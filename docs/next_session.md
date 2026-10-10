# F4 geliştirme devri — 10 Ekim 2026

Önce [AGENTS.md](AGENTS.md), [devam promptu](codex_start_prompt.md) ve oradaki belgeleri oku. Güncel çalışma dalı `feat/f2-inventory`; `main` güncel geliştirmeleri içermez. Git durumunu ve `git ls-remote` kimliğini kontrol et; eski devir commit'ine reset yapma. Normal push yetkilidir; force push yoktur.

F1-A/F1-B/F2/F3 teknik olarak tamamlandı. F4 raporları ve saatlik yerel/30 günlük yedek altyapısı geliştirildi, Mac izole testleri geçti; **F4 kapanmadı**. Kullanıcı F3 ekranlarını henüz kontrol etmedi. Windows bilgisayarına erişim yok ve kurulumu açıkça erteledi. F5 isteğe bağlıdır, başlatılmadı.

## Güncel kararlar

K24–K26: Excel aktarımı ve yazdırma yok. Temel raporlar mevcut/biten stok, tarih aralıklı satış/iade/fark, nakit/kart ve ürün miktarlarıdır. Yedek hedef bilgisayarda yerel, saatlik, 30 gün saklama; yaklaşık bir saatlik kayıp hedefi servis/DB açık ve yedekler başarılıyken geçerlidir. Harici kopya seçilmedi. Aynı disk arızası yedekleri de kaybettirebilir.

Onaylı F2/F3 kurallarını yeniden sorma. Satır/fiş TL-yüzde indirim, KDV dahil satıra renklendirme ücreti, sıfır ücretli olsa da renklendirilmiş boya iade yasağı, dışarıda ödeme alındığında nakit/kart tamamlama, sağlam/hasarlı iade ve müşteri lehine kuruş kuralları korunur. Cari/POS/pigment ekleme.

## Uygulama ve koruma

Yerel URL http://localhost:5080. `.env`, hesap, işletme verisi ve volume'lar korunur; sırları durum kontrolü için okuma/çıktılama. İlk kurulum/admin/db-setup veya mevcut migration komutlarını tekrar çalıştırma. Uygulanmış dört migration değişmedi. F4 şema değiştirmez.

`Pages/Reports/` raporları, `compose.backup.yaml` ve `scripts/backup/` yedek servisini içerir. [F4 işletim rehberi](f4_operations.md) etkinleştirme/kurtarma ve gerçek kabul listesidir. Saatlik servis Mac normal kurulumunda etkinleştirilmedi; izole testte doğrulandı. `backups/automatic` hedef yerel dizindir. Eski F3 yükseltme yedeğini güncel veri sanma; şema değişikliği öncesi yeni benzersiz yedek ve ayrı DB restore gerekir.

## Doğrulama ve sıradaki iş

`bash scripts/test-app.sh` F1-B/F2/F3 ve F4 rapor senaryolarını, `bash scripts/test-backup.sh` gerçek dump/restore ve yedek yaşam döngüsünü izole DB'lerde sınar. Kanıtlar [progress.md](progress.md). İşletme verisi test fixture'ı değildir. Test volume'ları silinmez.

Sıradaki iş kullanıcı F3/F4 ekran geri bildirimi ve hedef Windows erişimi geldiğinde donanım/OS gereksinimlerini doğrulamaktır. Windows/AMD64, OS açılışı, gerçek internet kesintisi, Windows restore ve görsel kabul yapılmadan tamamlandı sayılmaz. Kullanıcının Windows ertelemesini kaldırdığını varsayma. Normal çalışma dalına commit/push sonrası uzak kimliği doğrula.
