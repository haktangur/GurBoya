using GurBoya.Models;

namespace GurBoya.Services;

public interface IMusteriService
{
    Task<List<Musteri>> GetAllAsync();
    Task<Musteri?> GetByIdAsync(int id);
    Task CreateAsync(Musteri musteri);
    Task UpdateAsync(Musteri musteri);
    Task DeleteAsync(int id);
}