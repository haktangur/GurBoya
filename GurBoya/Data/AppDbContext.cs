using GurBoya.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Musteri> Musteriler => Set<Musteri>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Siparis> Siparisler => Set<Siparis>();
    public DbSet<SiparisKalemi> SiparisKalemleri => Set<SiparisKalemi>();
    public DbSet<KasaHareketi> KasaHareketleri => Set<KasaHareketi>();
    public DbSet<StokHareketi> StokHareketleri => Set<StokHareketi>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Urun>(e => {
            e.Property(u => u.AlisFiyati).HasPrecision(18, 2);
            e.Property(u => u.SatisFiyati).HasPrecision(18, 2);
            e.Property(u => u.StokMiktari).HasPrecision(18, 3);
            e.Property(u => u.KritikStokSeviyesi).HasPrecision(18, 3);
        });
        builder.Entity<SiparisKalemi>(e => {
            e.Property(k => k.BirimFiyat).HasPrecision(18, 2);
            e.Property(k => k.Miktar).HasPrecision(18, 3);
            e.Property(k => k.IndirimYuzde).HasPrecision(5, 2);
        });
        builder.Entity<KasaHareketi>()
            .Property(k => k.Tutar).HasPrecision(18, 2);
        builder.Entity<Musteri>()
            .Property(m => m.Bakiye).HasPrecision(18, 2);
        builder.Entity<StokHareketi>()
            .Property(s => s.Miktar).HasPrecision(18, 3);
    }
}