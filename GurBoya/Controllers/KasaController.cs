using GurBoya.Data;
using GurBoya.Models;
using GurBoya.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Controllers;

public class KasaController : Controller
{
    private readonly IKasaService _kasaService;
    private readonly AppDbContext _db;

    public KasaController(IKasaService kasaService, AppDbContext db)
    {
        _kasaService = kasaService;
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis)
    {
        if (!baslangic.HasValue) baslangic = DateTime.Today;
        if (!bitis.HasValue) bitis = DateTime.Today;

        var hareketler = await _kasaService.GetAllAsync(baslangic, bitis);

        ViewBag.Baslangic = baslangic.Value.ToString("yyyy-MM-dd");
        ViewBag.Bitis = bitis.Value.ToString("yyyy-MM-dd");
        ViewBag.GunlukCiro = await _kasaService.GunlukCiroAsync(DateTime.Today);
        ViewBag.ToplamBakiye = await _kasaService.ToplamBakiyeAsync();
        ViewBag.ToplamGelir = hareketler.Where(h => h.Tip == HareketTipi.Gelir).Sum(h => h.Tutar);
        ViewBag.ToplamGider = hareketler.Where(h => h.Tip == HareketTipi.Gider).Sum(h => h.Tutar);

        return View(hareketler);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Musteriler = new SelectList(await _db.Musteriler.OrderBy(m => m.Ad).ToListAsync(), "Id", "Ad");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KasaHareketi hareket)
    {
        hareket.Tarih = DateTime.Now;
        await _kasaService.ManuelEkleAsync(hareket);
        TempData["Basari"] = "Kasa hareketi eklendi.";
        return RedirectToAction(nameof(Index));
    }
}