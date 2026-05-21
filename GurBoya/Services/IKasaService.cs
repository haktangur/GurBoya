using GurBoya.Models;

namespace GurBoya.Services;

public interface IKasaService
{
    Task<List<KasaHareketi>> GetAllAsync(DateTime? baslangic, DateTime? bitis);
    Task<decimal> GunlukCiroAsync(DateTime tarih);
    Task<decimal> ToplamBakiyeAsync();
    Task ManuelEkleAsync(KasaHareketi hareket);
}