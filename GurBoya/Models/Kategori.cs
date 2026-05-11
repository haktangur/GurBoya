namespace GurBoya.Models;

public class Kategori
{
    public int Id { get; set; }
    public string Ad { get; set; } = null!;
    public ICollection<Urun> Urunler { get; set; } = [];
}