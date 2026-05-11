using GurBoya.Models;

namespace GurBoya.Services;

public interface IUrunService
{
    Task<List<Urun>> GetAllAsync();
    Task<Urun?> GetByIdAsync(int id);
    Task CreateAsync(Urun urun);
    Task UpdateAsync(Urun urun);
    Task DeleteAsync(int id);
    Task<List<Urun>> GetKritikStokAsync();
    Task StokGuncelleAsync(int urunId, decimal miktar, StokHareketTipi tip, string? not = null);
}