using GurBoya.Models;

namespace GurBoya.Services;

public interface ISiparisService
{
    Task<List<Siparis>> GetAllAsync();
    Task<Siparis?> GetByIdAsync(int id);
    Task<Siparis> CreateAsync(int? musteriId, List<SiparisKalemi> kalemler, OdemeTuru odemeTuru, string? not);
    Task TamamlaAsync(int id, OdemeTuru odemeTuru);
    Task IptalEtAsync(int id);
}