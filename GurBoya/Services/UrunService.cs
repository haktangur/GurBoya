using GurBoya.Data;
using GurBoya.Models;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Services;

public class UrunService : IUrunService
{
    private readonly AppDbContext _db;

    public UrunService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Urun>> GetAllAsync()
    {
        return await _db.Urunler
            .Include(u => u.Kategori)
            .OrderBy(u => u.Ad)
            .ToListAsync();
    }

    public async Task<Urun?> GetByIdAsync(int id)
    {
        return await _db.Urunler
            .Include(u => u.Kategori)
            .Include(u => u.StokHareketleri)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task CreateAsync(Urun urun)
    {
        _db.Urunler.Add(urun);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Urun urun)
    {
        _db.Urunler.Update(urun);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var urun = await _db.Urunler.FindAsync(id);
        if (urun != null)
        {
            urun.AktifMi = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Urun>> GetKritikStokAsync()
    {
        return await _db.Urunler
            .Include(u => u.Kategori)
            .Where(u => u.AktifMi && u.StokMiktari <= u.KritikStokSeviyesi)
            .ToListAsync();
    }

    public async Task StokGuncelleAsync(int urunId, decimal miktar, StokHareketTipi tip, string? not = null)
    {
        var urun = await _db.Urunler.FindAsync(urunId);
        if (urun == null) return;

        urun.StokMiktari += tip == StokHareketTipi.Giris ? miktar : -miktar;

        _db.StokHareketleri.Add(new StokHareketi
        {
            UrunId = urunId,
            Tip = tip,
            Miktar = miktar,
            Not = not
        });

        await _db.SaveChangesAsync();
    }
}