using GurBoya.Data;
using GurBoya.Models;
using GurBoya.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Controllers;

public class UrunController : Controller
{
    private readonly IUrunService _urunService;
    private readonly AppDbContext _db;

    public UrunController(IUrunService urunService, AppDbContext db)
    {
        _urunService = urunService;
        _db = db;
    }

    public async Task<IActionResult> Index(string? arama, int? kategoriId)
    {
        var urunler = await _urunService.GetAllAsync();

        if (!string.IsNullOrEmpty(arama))
            urunler = urunler.Where(u => u.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase)
                || u.Kod.Contains(arama, StringComparison.OrdinalIgnoreCase)).ToList();

        if (kategoriId.HasValue)
            urunler = urunler.Where(u => u.KategoriId == kategoriId).ToList();

        ViewBag.Kategoriler = new SelectList(await _db.Kategoriler.ToListAsync(), "Id", "Ad", kategoriId);
        ViewBag.Arama = arama;
        ViewBag.KritikStokSayisi = (await _urunService.GetKritikStokAsync()).Count;

        return View(urunler);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var urun = await _urunService.GetByIdAsync(id);
        if (urun == null) return NotFound();
        return View(urun);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Kategoriler = new SelectList(await _db.Kategoriler.ToListAsync(), "Id", "Ad");
        return View();
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Urun urun)
{
    if (ModelState.IsValid)
    {
        await _urunService.CreateAsync(urun);
        TempData["Basari"] = $"{urun.Ad} başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }
    ViewBag.Kategoriler = new SelectList(await _db.Kategoriler.ToListAsync(), "Id", "Ad", urun.KategoriId);
    return View(urun);
}

    public async Task<IActionResult> Edit(int id)
    {
        var urun = await _urunService.GetByIdAsync(id);
        if (urun == null) return NotFound();
        ViewBag.Kategoriler = new SelectList(await _db.Kategoriler.ToListAsync(), "Id", "Ad", urun.KategoriId);
        return View(urun);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Urun urun)
    {
        if (ModelState.IsValid)
        {
            await _urunService.UpdateAsync(urun);
            TempData["Basari"] = $"{urun.Ad} başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Kategoriler = new SelectList(await _db.Kategoriler.ToListAsync(), "Id", "Ad", urun.KategoriId);
        return View(urun);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _urunService.DeleteAsync(id);
        TempData["Basari"] = "Ürün pasife alındı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StokGuncelle(int urunId, decimal miktar, StokHareketTipi tip, string? not)
    {
        await _urunService.StokGuncelleAsync(urunId, miktar, tip, not);
        TempData["Basari"] = "Stok güncellendi.";
        return RedirectToAction(nameof(Detail), new { id = urunId });
    }
}