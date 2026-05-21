using GurBoya.Data;
using GurBoya.Models;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Services;

public class SiparisService : ISiparisService
{
    private readonly AppDbContext _db;

    public SiparisService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Siparis>> GetAllAsync()
    {
        return await _db.Siparisler
            .Include(s => s.Musteri)
            .Include(s => s.Kalemler).ThenInclude(k => k.Urun)
            .OrderByDescending(s => s.Tarih)
            .ToListAsync();
    }

    public async Task<Siparis?> GetByIdAsync(int id)
    {
        return await _db.Siparisler
            .Include(s => s.Musteri)
            .Include(s => s.Kalemler).ThenInclude(k => k.Urun)
            .Include(s => s.KasaHareketleri)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Siparis> CreateAsync(int? musteriId, List<SiparisKalemi> kalemler, OdemeTuru odemeTuru, string? not)
    {
        var siparis = new Siparis
        {
            MusteriId = musteriId,
            Not = not,
            Durum = SiparisDurumu.Tamamlandi,
            Tarih = DateTime.Now
        };

        foreach (var kalem in kalemler)
        {
            var urun = await _db.Urunler.FindAsync(kalem.UrunId);
            if (urun == null) continue;

            kalem.BirimFiyat = urun.SatisFiyati;
            siparis.Kalemler.Add(kalem);

            urun.StokMiktari -= kalem.Miktar;
            _db.StokHareketleri.Add(new StokHareketi
            {
                UrunId = urun.Id,
                Tip = StokHareketTipi.Cikis,
                Miktar = kalem.Miktar,
                Not = $"Siparis #{siparis.Id}"
            });
        }

        _db.Siparisler.Add(siparis);
        await _db.SaveChangesAsync();

        var toplam = siparis.Kalemler.Sum(k => k.Miktar * k.BirimFiyat * (1 - k.IndirimYuzde / 100));

        _db.KasaHareketleri.Add(new KasaHareketi
        {
            SiparisId = siparis.Id,
            MusteriId = musteriId,
            Tip = HareketTipi.Gelir,
            OdemeTuru = odemeTuru,
            Tutar = toplam,
            Aciklama = $"Siparis #{siparis.Id}"
        });

        if (musteriId.HasValue && odemeTuru == OdemeTuru.Veresiye)
        {
            var musteri = await _db.Musteriler.FindAsync(musteriId.Value);
            if (musteri != null) musteri.Bakiye += toplam;
        }

        await _db.SaveChangesAsync();
        return siparis;
    }

    public async Task TamamlaAsync(int id, OdemeTuru odemeTuru)
    {
        var siparis = await _db.Siparisler
            .Include(s => s.Kalemler)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (siparis == null) return;

        siparis.Durum = SiparisDurumu.Tamamlandi;
        await _db.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var siparis = await _db.Siparisler
            .Include(s => s.Kalemler)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (siparis == null) return;

        siparis.Durum = SiparisDurumu.Iptal;

        foreach (var kalem in siparis.Kalemler)
        {
            var urun = await _db.Urunler.FindAsync(kalem.UrunId);
            if (urun != null) urun.StokMiktari += kalem.Miktar;
        }

        await _db.SaveChangesAsync();
    }
}