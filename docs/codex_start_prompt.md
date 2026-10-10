# Yeni sohbete devam promptu

```text
GürBoya projesine devam ediyoruz. Önce docs/AGENTS.md, ardından docs/next_session.md, docs/README.md, docs/project_context.md, docs/decisions.md, docs/architecture.md, docs/database.md, docs/project_plan.md, docs/development_rules.md, docs/progress.md, docs/chatgpt.md ve docs/f4_operations.md dosyalarını oku.

Çalışma dalı feat/f2-inventory; main güncel değil. Git durumunu, origin https://github.com/haktangur/GurBoya.git uzak dal kimliğini ve Docker servislerini kontrol et. En güncel dal ucundan devam et; eski commit'e reset yapma. Mevcut veri/.env/hesap/volume korunur; sırları çıktılamazsın, ilk kurulumu tekrarlamazsın, uygulanmış migration'ları değiştirmezsin.

F1-A/F1-B/F2/F3 teknik olarak tamamlandı. F4 raporları ve saatlik yerel yedek altyapısı Mac izole testlerinden geçti; F4 kapanmadı. Kullanıcı F3 ekranlarını henüz kontrol etmedi. Windows bilgisayarına şu an erişim yok, kurulum kullanıcı isteğiyle ertelendi. Erişim sağlanmadan Windows kurulumu/OS açılışı/gerçek internet kesintisi/Windows restore veya görsel kabul tamamlandı sayılmaz. F5 isteğe bağlı, başlatılmadı.

K24–K26: Excel/yazdırma yok; temel stok, tarih aralıklı satış/iade/fark, nakit/kart ve ürün miktarı raporları hazır. Yedek hedef bilgisayarda yerel, saatlik, 30 gün saklanır; yaklaşık bir saatlik kayıp hedefi son yedek başarılı ve servis/DB çalışırken geçerlidir. Aynı disk arızasına karşı harici kopya seçilmedi. compose.backup.yaml servisi Mac normal kurulumunda etkinleştirilmedi; test edildi, hedef Windows için hazırdır. F2/F3 iş kurallarını tekrar sorma ve modülleri baştan yazma.

Varsa kullanıcının F3/F4 ekran kontrolü sonucunu al. Windows ertelemesini kaldırdığını varsayma; hedef erişimi geldiğinde sürüm/donanım/ACL, gerçek açılış/internet kesintisi ve restore pilotunu uygula. Uygun testleri çalıştır, belgeleri docs/ altında güncelle, sır/diff kontrolü yap, mantıksal commit'lerle çalışma dalına normal push et ve uzak kimliği doğrula. Push yetkilidir; force push, volume silme ve canlı DB üstüne restore yoktur.
```
