namespace GurBoya.Models;

public enum StokHareketTipi { Giris, Cikis, Sayim, Iade }

public class StokHareketi
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public Urun Urun { get; set; } = null!;
    public StokHareketTipi Tip { get; set; }
    public decimal Miktar { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Now;
    public string? Not { get; set; }
}