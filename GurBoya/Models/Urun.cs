namespace GurBoya.Models;

public class Urun
{
    public int Id { get; set; }
    public string Kod { get; set; } = null!;
    public string Ad { get; set; } = null!;
    public int KategoriId { get; set; }
    public Kategori Kategori { get; set; } = null!;
    public decimal AlisFiyati { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal StokMiktari { get; set; }
    public decimal KritikStokSeviyesi { get; set; } = 5;
    public string? Birim { get; set; }
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;
    public ICollection<SiparisKalemi> SiparisKalemleri { get; set; } = [];
    public ICollection<StokHareketi> StokHareketleri { get; set; } = [];
}