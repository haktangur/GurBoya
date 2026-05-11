namespace GurBoya.Models;

public class Musteri
{
    public int Id { get; set; }
    public string Ad { get; set; } = null!;
    public string? Telefon { get; set; }
    public string? Adres { get; set; }
    public decimal Bakiye { get; set; } = 0;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    public ICollection<Siparis> Siparisler { get; set; } = [];
    public ICollection<KasaHareketi> KasaHareketleri { get; set; } = [];
}