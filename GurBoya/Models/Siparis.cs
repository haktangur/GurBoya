namespace GurBoya.Models;

public enum SiparisDurumu { Bekliyor, Tamamlandi, Iptal }

public class Siparis
{
    public int Id { get; set; }
    public int? MusteriId { get; set; }
    public Musteri? Musteri { get; set; }
    public DateTime Tarih { get; set; } = DateTime.Now;
    public SiparisDurumu Durum { get; set; } = SiparisDurumu.Bekliyor;
    public string? Not { get; set; }
    public ICollection<SiparisKalemi> Kalemler { get; set; } = [];
    public ICollection<KasaHareketi> KasaHareketleri { get; set; } = [];
}

public class SiparisKalemi
{
    public int Id { get; set; }
    public int SiparisId { get; set; }
    public Siparis Siparis { get; set; } = null!;
    public int UrunId { get; set; }
    public Urun Urun { get; set; } = null!;
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal IndirimYuzde { get; set; } = 0;
}