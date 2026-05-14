using GurBoya.Models;
using GurBoya.Services;
using Microsoft.AspNetCore.Mvc;

namespace GurBoya.Controllers;

public class MusteriController : Controller
{
    private readonly IMusteriService _musteriService;

    public MusteriController(IMusteriService musteriService)
    {
        _musteriService = musteriService;
    }

    public async Task<IActionResult> Index(string? arama)
    {
        var musteriler = await _musteriService.GetAllAsync();
        if (!string.IsNullOrEmpty(arama))
            musteriler = musteriler.Where(m => m.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase)
                || (m.Telefon != null && m.Telefon.Contains(arama))).ToList();
        ViewBag.Arama = arama;
        return View(musteriler);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var musteri = await _musteriService.GetByIdAsync(id);
        if (musteri == null) return NotFound();
        return View(musteri);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Musteri musteri)
    {
        if (ModelState.IsValid)
        {
            await _musteriService.CreateAsync(musteri);
            TempData["Basari"] = $"{musteri.Ad} başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(musteri);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var musteri = await _musteriService.GetByIdAsync(id);
        if (musteri == null) return NotFound();
        return View(musteri);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Musteri musteri)
    {
        if (ModelState.IsValid)
        {
            await _musteriService.UpdateAsync(musteri);
            TempData["Basari"] = $"{musteri.Ad} başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        return View(musteri);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _musteriService.DeleteAsync(id);
        TempData["Basari"] = "Müşteri silindi.";
        return RedirectToAction(nameof(Index));
    }
}