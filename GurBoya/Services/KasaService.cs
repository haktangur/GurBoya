using GurBoya.Data;
using GurBoya.Models;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Services;

public class KasaService : IKasaService
{
    private readonly AppDbContext _db;

    public KasaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KasaHareketi>> GetAllAsync(DateTime? baslangic, DateTime? bitis)
    {
        var query = _db.KasaHareketleri
            .Include(k => k.Musteri)
            .Include(k => k.Siparis)
            .AsQueryable();

        if (baslangic.HasValue)
            query = query.Where(k => k.Tarih >= baslangic.Value);
        if (bitis.HasValue)
            query = query.Where(k => k.Tarih <= bitis.Value.AddDays(1));

        return await query.OrderByDescending(k => k.Tarih).ToListAsync();
    }

    public async Task<decimal> GunlukCiroAsync(DateTime tarih)
    {
        return await _db.KasaHareketleri
            .Where(k => k.Tarih.Date == tarih.Date && k.Tip == HareketTipi.Gelir)
            .SumAsync(k => k.Tutar);
    }

    public async Task<decimal> ToplamBakiyeAsync()
    {
        var gelir = await _db.KasaHareketleri
            .Where(k => k.Tip == HareketTipi.Gelir)
            .SumAsync(k => k.Tutar);
        var gider = await _db.KasaHareketleri
            .Where(k => k.Tip == HareketTipi.Gider)
            .SumAsync(k => k.Tutar);
        return gelir - gider;
    }

    public async Task ManuelEkleAsync(KasaHareketi hareket)
    {
        _db.KasaHareketleri.Add(hareket);
        await _db.SaveChangesAsync();
    }
}