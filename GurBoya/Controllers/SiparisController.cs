using GurBoya.Data;
using GurBoya.Models;
using GurBoya.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Controllers;

public class SiparisController : Controller
{
    private readonly ISiparisService _siparisService;
    private readonly AppDbContext _db;

    public SiparisController(ISiparisService siparisService, AppDbContext db)
    {
        _siparisService = siparisService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var siparisler = await _siparisService.GetAllAsync();
        return View(siparisler);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var siparis = await _siparisService.GetByIdAsync(id);
        if (siparis == null) return NotFound();
        return View(siparis);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Musteriler = new SelectList(await _db.Musteriler.OrderBy(m => m.Ad).ToListAsync(), "Id", "Ad");
        ViewBag.Urunler = await _db.Urunler.Where(u => u.AktifMi).OrderBy(u => u.Ad).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int? musteriId, string odemeTuru, string? not, List<int> urunId, List<decimal> miktar, List<decimal> indirim)
    {
        var kalemler = new List<SiparisKalemi>();
        for (int i = 0; i < urunId.Count; i++)
        {
            if (urunId[i] == 0 || miktar[i] <= 0) continue;
            kalemler.Add(new SiparisKalemi
            {
                UrunId = urunId[i],
                Miktar = miktar[i],
                IndirimYuzde = indirim.Count > i ? indirim[i] : 0
            });
        }

        if (!kalemler.Any())
        {
            TempData["Hata"] = "En az bir ürün eklemelisiniz.";
            ViewBag.Musteriler = new SelectList(await _db.Musteriler.OrderBy(m => m.Ad).ToListAsync(), "Id", "Ad");
            ViewBag.Urunler = await _db.Urunler.Where(u => u.AktifMi).OrderBy(u => u.Ad).ToListAsync();
            return View();
        }

        var odeme = Enum.Parse<OdemeTuru>(odemeTuru);
        var siparis = await _siparisService.CreateAsync(musteriId, kalemler, odeme, not);
        TempData["Basari"] = $"Sipariş #{siparis.Id} oluşturuldu.";
        return RedirectToAction(nameof(Detail), new { id = siparis.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id)
    {
        await _siparisService.IptalEtAsync(id);
        TempData["Basari"] = "Sipariş iptal edildi.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}