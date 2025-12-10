using MedicalEquipmentManagementSystem.Data;
using MedicalEquipmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalEquipmentManagementSystem.Controllers;

/// <summary>
/// Kontroler obs³uguj¹cy operacje CRUD na sprzêcie medycznym.
/// </summary>
public class MedicalEquipmentController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicjalizuje now¹ instancjê kontrolera.
    /// </summary>
    /// <param name="context">Kontekst bazy danych.</param>
    public MedicalEquipmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Wyœwietla listê ca³ego sprzêtu medycznego.
    /// </summary>
    /// <returns>Widok z list¹ sprzêtu.</returns>
    public async Task<IActionResult> Index()
    {
        var equipment = await _context.MedicalEquipments
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View(equipment);
    }

    /// <summary>
    /// Wyœwietla szczegó³y wybranego sprzêtu.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu.</param>
    /// <returns>Widok ze szczegó³ami sprzêtu lub NotFound.</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Wyœwietla formularz tworzenia nowego sprzêtu.
    /// </summary>
    /// <returns>Widok formularza tworzenia.</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Przetwarza formularz tworzenia nowego sprzêtu.
    /// </summary>
    /// <param name="equipment">Dane nowego sprzêtu.</param>
    /// <returns>Przekierowanie do listy lub widok z b³êdami walidacji.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,SerialNumber,Manufacturer,Model,Category,Location,Status,PurchaseDate,LastMaintenanceDate,NextMaintenanceDate,Notes")] MedicalEquipment equipment)
    {
        if (await _context.MedicalEquipments.AnyAsync(e => e.SerialNumber == equipment.SerialNumber))
        {
            ModelState.AddModelError(nameof(equipment.SerialNumber), "Sprzêt o tym numerze seryjnym ju¿ istnieje.");
        }

        if (ModelState.IsValid)
        {
            equipment.CreatedAt = DateTime.UtcNow;
            _context.Add(equipment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Pomyœlnie dodano sprzêt: {equipment.Name}";

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    /// <summary>
    /// Wyœwietla formularz edycji sprzêtu.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu do edycji.</param>
    /// <returns>Widok formularza edycji lub NotFound.</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments.FindAsync(id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Przetwarza formularz edycji sprzêtu.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu.</param>
    /// <param name="equipment">Zaktualizowane dane sprzêtu.</param>
    /// <returns>Przekierowanie do listy lub widok z b³êdami walidacji.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SerialNumber,Manufacturer,Model,Category,Location,Status,PurchaseDate,LastMaintenanceDate,NextMaintenanceDate,Notes,CreatedAt")] MedicalEquipment equipment)
    {
        if (id != equipment.Id)
        {
            return NotFound();
        }

        if (await _context.MedicalEquipments.AnyAsync(e => e.SerialNumber == equipment.SerialNumber && e.Id != id))
        {
            ModelState.AddModelError(nameof(equipment.SerialNumber), "Sprzêt o tym numerze seryjnym ju¿ istnieje.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                equipment.UpdatedAt = DateTime.UtcNow;
                _context.Update(equipment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Pomyœlnie zaktualizowano sprzêt: {equipment.Name}";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EquipmentExistsAsync(equipment.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    /// <summary>
    /// Wyœwietla potwierdzenie usuniêcia sprzêtu.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu do usuniêcia.</param>
    /// <returns>Widok potwierdzenia usuniêcia lub NotFound.</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Przetwarza usuniêcie sprzêtu.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu do usuniêcia.</param>
    /// <returns>Przekierowanie do listy.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var equipment = await _context.MedicalEquipments.FindAsync(id);

        if (equipment is not null)
        {
            var name = equipment.Name;
            _context.MedicalEquipments.Remove(equipment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Pomyœlnie usuniêto sprzêt: {name}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sprawdza czy sprzêt o podanym Id istnieje.
    /// </summary>
    /// <param name="id">Identyfikator sprzêtu.</param>
    /// <returns>True jeœli sprzêt istnieje, w przeciwnym razie false.</returns>
    private async Task<bool> EquipmentExistsAsync(int id)
    {
        return await _context.MedicalEquipments.AnyAsync(e => e.Id == id);
    }
}
