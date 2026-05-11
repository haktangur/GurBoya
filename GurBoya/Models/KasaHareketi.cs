namespace GurBoya.Models;

public enum OdemeTuru { Nakit, KrediKarti, Havale, Veresiye }
public enum HareketTipi { Gelir, Gider }

public class KasaHareketi
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Now;
    public HareketTipi Tip { get; set; }
    public OdemeTuru OdemeTuru { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public int? SiparisId { get; set; }
    public Siparis? Siparis { get; set; }
    public int? MusteriId { get; set; }
    public Musteri? Musteri { get; set; }
}