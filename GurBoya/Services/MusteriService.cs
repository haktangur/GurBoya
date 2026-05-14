using GurBoya.Data;
using GurBoya.Models;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Services;

public class MusteriService : IMusteriService
{
    private readonly AppDbContext _db;

    public MusteriService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Musteri>> GetAllAsync()
    {
        return await _db.Musteriler
            .OrderBy(m => m.Ad)
            .ToListAsync();
    }

    public async Task<Musteri?> GetByIdAsync(int id)
    {
        return await _db.Musteriler
            .Include(m => m.Siparisler)
            .Include(m => m.KasaHareketleri)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateAsync(Musteri musteri)
    {
        _db.Musteriler.Add(musteri);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Musteri musteri)
    {
        _db.Musteriler.Update(musteri);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var musteri = await _db.Musteriler.FindAsync(id);
        if (musteri != null)
        {
            _db.Musteriler.Remove(musteri);
            await _db.SaveChangesAsync();
        }
    }
}